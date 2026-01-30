using System;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Compass;
using BML.Scripts.ItemTreeGraph;
using BML.Scripts.Utils;
using Cinemachine.Utility;
using DrawXXL;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

namespace BML.Scripts.PID
{
    public class PidFakeRigidbody : MonoBehaviour
    {
        #region Inspector

        private enum TargetMode
        {
            Direction,
            MatchRotation,
            MatchPosition,
        }

        [SerializeField] private TargetMode _targetMode = TargetMode.Direction;
        private bool _isMatchPositionMode => _targetMode == TargetMode.MatchPosition;
        [SerializeField] private SafeTransformValueReference _target;
        [SerializeField, HideIf("_isMatchPositionMode")] private bool _doAlignToTargetOnEnable;
        
        private enum MovementMode
        {
            Rigidbody,
            Transform,
        }
        [SerializeField] private MovementMode _movementMode = MovementMode.Rigidbody;

        [SerializeField] private bool _separateRotationAxes = false;
        
        private enum UpdateMethod
        {
            FixedUpdate,
            Update,
        }
        [SerializeField] private UpdateMethod _updateMethod = UpdateMethod.FixedUpdate;
        
        [SerializeField, HideIf("@_updateMethod == UpdateMethod.FixedUpdate")] 
        private bool _useUnscaledTime;
        
        [SerializeField, ShowIf("@_movementMode == MovementMode.Rigidbody")] private float _maxAngularVelocity = 20;

        [SerializeField] private PIDParameters _thrustParameters;
        [SerializeField, HideIf("_isMatchPositionMode")] private PIDParameters _rotationParameters;
        [SerializeField, ShowIf("@!_isMatchPositionMode && _separateRotationAxes")] private PIDParameters _rotationYParameters;
        [SerializeField, ShowIf("@!_isMatchPositionMode && _separateRotationAxes")] private PIDParameters _rotationZParameters;

        [ShowInInspector] private PID2 _thrustPIDController;
        [ShowInInspector, HideIf("_isMatchPositionMode")] private PID2 _rotationPIDController;
        [ShowInInspector, ShowIf("@!_isMatchPositionMode && _separateRotationAxes")] private PID2 _rotationYPIDController;
        [ShowInInspector, ShowIf("@!_isMatchPositionMode && _separateRotationAxes")] private PID2 _rotationZPIDController;
        
        private FakeRigidbody _rb;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            if (_doAlignToTargetOnEnable && _target.Value != null)
            {
                Quaternion targetRotation;
                switch (_targetMode)
                {
                    default:
                    case TargetMode.Direction:
                        var targetPosition = _target.Value.transform.position;
                        var targetDirection = targetPosition - transform.position;
                        targetRotation = Quaternion.LookRotation(targetDirection);
                        break;
                    case TargetMode.MatchRotation:
                        targetRotation = _target.Value.rotation;
                        break;
                    case TargetMode.MatchPosition:
                        targetRotation = transform.rotation;
                        transform.position = _target.Value.position;
                        break;
                }
                
                transform.rotation = targetRotation;
            }
        }

        void Start()
        {
            _rb = GetComponent<FakeRigidbody>();
            _thrustPIDController = new PID2(_thrustParameters);
            _rotationPIDController = new PID2(_rotationParameters);
            if (_separateRotationAxes)
            {
                _rotationYPIDController = new PID2(_rotationYParameters);
                _rotationZPIDController = new PID2(_rotationZParameters);
            }
        }
        
        private void Update()
        {
            if (_target.Value.SafeIsUnityNull()) return;

            _thrustPIDController.Parameters = _thrustParameters;
            _rotationPIDController.Parameters = _rotationParameters;
            if (_separateRotationAxes)
            {
                _rotationYPIDController.Parameters = _rotationYParameters;
                _rotationZPIDController.Parameters = _rotationZParameters;
            }
            
            if (_updateMethod == UpdateMethod.Update)
            {
                UpdatePid(_useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            }
        }
        
        void FixedUpdate()
        {
            if (_updateMethod == UpdateMethod.FixedUpdate)
            {
                UpdatePid(Time.fixedDeltaTime);
            }
        }

        private void OnDrawGizmos()
        {
            var position = transform.position;

            Vector3? targetPosition, targetDirection;
            if (_target.Value != null)
            {
                switch (_targetMode)
                {
                    case TargetMode.MatchPosition:
                        targetPosition = _target.Value?.transform.position ?? (position + transform.forward);
                        DrawBasics.Line(position, targetPosition.Value, Color.blue, 0f, "Match position", DrawBasics.LineStyle.dashed);
                        break;
                    case TargetMode.MatchRotation:
                        targetDirection = _target.Value.forward;
                        DrawBasics.Vector(position, position + transform.forward, Color.blue, 0f);
                        DrawBasics.Line(position, position + targetDirection.Value, Color.blue, 0f, "Match rotation", DrawBasics.LineStyle.dashed);
                        DrawBasics.Line(position, position + _target.Value.right, Color.red, 0f, null, DrawBasics.LineStyle.dashed);
                        DrawBasics.Line(position, position + _target.Value.up, Color.green, 0f, null, DrawBasics.LineStyle.dashed);
                        break;
                    case TargetMode.Direction:
                        targetPosition = _target.Value.transform.position;
                        targetDirection = (targetPosition - position).Value.normalized;
                        DrawBasics.Vector(position, position + transform.forward, Color.yellow, 0f);
                        DrawBasics.Line(position, targetPosition.Value, Color.yellow, 0f, "Direction", DrawBasics.LineStyle.dashed);
                        break;
                }
            }
            
            if (!_isMatchPositionMode && _rotationPIDController != null && _target.Value != null)
            {
                // Shapes.Draw.Line(position, position + transform.forward, Color.blue);
                
                targetDirection = _target.Value.forward;
                var localTargetDirection = transform.InverseTransformDirection(targetDirection.Value);
                // DrawBasics.Line(transform.position, transform.position + (Quaternion.Euler(-90,0,0) * targetDirection.Value).xyo(), Color.magenta);
                // DrawBasics.Line(transform.position, transform.position + transform.up, Color.magenta);

                var radius = 0.3f;
                var lineWidth = 0.02f;
                var projectionLineStyle = DrawBasics.LineStyle.dotDashLong;
                if (!_separateRotationAxes)
                {
                    Func<float, float> remap = (x) => (Mathf.Approximately(x, 0f) ? 0.001f : x);
                    DrawBasics.VectorCircled(
                        position,
                        transform.forward,
                        targetDirection.Value,
                        Color.blue,
                        radius,
                        lineWidth,
                        $"{_rotationPIDController.PrevError.ToString("F1")}"
                    );
                }
                else
                {
                    var targetXProj = transform.TransformDirection(localTargetDirection.oyz());
                    DrawBasics.Line(
                        position,
                        position + targetXProj,
                        Color.red,
                        0f,
                        null,
                        projectionLineStyle
                    );
                    DrawBasics.VectorCircled(
                        position,
                        transform.forward,
                        targetXProj,
                        Color.red,
                        radius,
                        lineWidth,
                        $"{_rotationPIDController.PrevError.ToString("F1")}"
                    );
                    
                    var targetYProj = transform.TransformDirection(localTargetDirection.xoz());
                    DrawBasics.Line(
                        position,
                        position + targetYProj,
                        Color.green,
                        0f,
                        null,
                        projectionLineStyle
                    );
                    DrawBasics.VectorCircled(
                        position,
                        transform.forward,
                        targetYProj,
                        Color.green,
                        radius,
                        lineWidth,
                        $"{_rotationYPIDController.PrevError.ToString("F1")}"
                    );
                    
                    var perpendicular = Quaternion.Euler(-90, 0, 0) * localTargetDirection;
                    var targetZProj = transform.TransformDirection(perpendicular.xyo());
                    DrawBasics.Line(
                        position,
                        position + targetZProj,
                        Color.blue,
                        0f,
                        null,
                        projectionLineStyle
                    );
                    DrawBasics.VectorCircled(
                        position,
                        transform.up,
                        targetZProj,
                        Color.blue,
                        radius,
                        lineWidth,
                        $"{_rotationZPIDController.PrevError.ToString("F1")}"
                    );
                }
                
            }
        }

        #endregion

        private void UpdatePid(float deltaTime)
        {
            _rb.maxAngularVelocity = _maxAngularVelocity;
            
            if (_target.Value.SafeIsUnityNull()) return;
            
            Transform cachedTransform = transform;

            if (_targetMode == TargetMode.MatchPosition)
            {
                var position = cachedTransform.position;
                var rotation = cachedTransform.rotation;
                var targetPosition = _target.Value.position;
                
                var positionDiff = (position - targetPosition);
                
                float thrustCorrection = _thrustPIDController.GetOutput(
                    positionDiff.magnitude, 
                    0, 
                    deltaTime
                );
                Vector3 forceCorrection = positionDiff.normalized * thrustCorrection;

                switch (_movementMode)
                {
                    case MovementMode.Rigidbody:
                        _rb.AddForce(forceCorrection);
                        break;
                    case MovementMode.Transform:
                        transform.position = Vector3.MoveTowards(
                            transform.position, 
                            targetPosition, 
                            thrustCorrection * deltaTime
                        );
                        break;
                }
                // TODO add rotation for some more interesting movement?
            }
            else
            {
                Vector3 cachedTransformForward = cachedTransform.forward;
                
                //Get the required rotation based on the target position - we can do this by getting the direction
                //from the current position to the target. Then use rotate towards and look rotation, to get a quaternion thingy.
                var targetPosition = _target.Value.position;
                var rotation = transform.rotation;
                Vector3 targetDirection;
                Quaternion targetRotation;
                switch (_targetMode)
                {
                    default:
                    case TargetMode.Direction:
                        targetDirection = targetPosition - transform.position;
                        Vector3 rotationDirection = Vector3.RotateTowards(cachedTransformForward, targetDirection,
                            360 * Mathf.Deg2Rad, 0.00f);
                        targetRotation = Quaternion.LookRotation(rotationDirection);
                        break;
                    case TargetMode.MatchRotation:
                        targetDirection = _target.Value.forward;
                        targetRotation = _target.Value.rotation;
                        break;
                }
                
                // Calculate thrust
                var thrustAlignment = 
                    Vector3.Project(_rb.velocity, targetDirection).magnitude / _rb.velocity.magnitude;
                var velocityZero = Mathf.Clamp01(1 - (_rb.velocity.magnitude / 5f));
                float thrustCorrection = _thrustPIDController.GetOutput(
                    0, // TODO fix usage of error
                    targetDirection.magnitude * (thrustAlignment + velocityZero), 
                    deltaTime
                );

                // Vars for _separateRotationAxes = false:
                Vector3 axis = Vector3.zero;
                float rotationCorrection = 0;
                // Vars for _separateRotationAxes = true:
                float xRotationCorrection = 0, yRotationCorrection = 0, zRotationCorrection = 0; // If _separateRotationAxes is true, use separate PID controllers for each axis.
                // Calculate rotation
                var localTargetDirection = cachedTransform.InverseTransformDirection(targetDirection);
                if (_separateRotationAxes)
                {
                    var xError = Vector3.SignedAngle(localTargetDirection.oyz(), Vector3.forward, Vector3.right);
                    xRotationCorrection = _rotationPIDController.GetOutput(
                        xError,
                        0,
                        deltaTime
                    );
                    var yError = Vector3.SignedAngle(localTargetDirection.xoz(), Vector3.forward, Vector3.up);
                    yRotationCorrection = _rotationYPIDController.GetOutput(
                        yError,
                        0,
                        deltaTime
                    );
                    var perpendicular = Quaternion.Euler(-90, 0, 0) * localTargetDirection;
                    var zError = Vector3.SignedAngle(perpendicular.xyo(), Vector3.up, Vector3.forward);
                    zRotationCorrection = _rotationZPIDController.GetOutput(
                        zError,
                        0,
                        deltaTime
                    );
                }
                else
                {
                    axis = Vector3.Cross(Vector3.forward, localTargetDirection);
                    var angleError = Vector3.SignedAngle(localTargetDirection, Vector3.forward, axis);
                    
                    rotationCorrection = _rotationPIDController.GetOutput(
                        angleError, 
                        0, 
                        deltaTime
                    );
                }

                // Apply thrust and rotation forces
                if (_movementMode == MovementMode.Rigidbody)
                {
                    if (_separateRotationAxes)
                    {
                        _rb.AddRelativeTorque(new Vector3(xRotationCorrection, yRotationCorrection, zRotationCorrection));
                    }
                    else
                    {
                        _rb.AddRelativeTorque(axis * rotationCorrection);
                    }

                    _rb.AddRelativeForce(Vector3.forward * thrustCorrection);
                }
                else if (_movementMode == MovementMode.Transform)
                {
                    Quaternion newRotation;
                    if (_separateRotationAxes)
                    {
                        // TODO this isn't right. but isn't needed at the moment.
                        newRotation = Quaternion.Euler(
                            cachedTransform.eulerAngles.x + xRotationCorrection * deltaTime,
                            cachedTransform.eulerAngles.y + yRotationCorrection * deltaTime,
                            cachedTransform.eulerAngles.z + zRotationCorrection * deltaTime
                        );
                    }
                    else
                    {
                        newRotation = Quaternion.RotateTowards(
                            cachedTransform.rotation,
                            targetRotation,
                            rotationCorrection * deltaTime
                        );
                    }
                
                    var newPosition = Vector3.MoveTowards(
                        cachedTransform.position, 
                        targetPosition, 
                        thrustCorrection * deltaTime
                    );

                    transform.SetPositionAndRotation(newPosition, newRotation);
                }
            }
        }
        
        #region Public interface

        public Transform Target => _target.Value;

        public bool HasTarget => _target != null && !_target.Value.SafeIsUnityNull();

        public void SetRotationOutputMinMax(Vector2 minMax)
        {
            _rotationParameters.OutputMinMax = minMax;
            _rotationYParameters.OutputMinMax = minMax;
            _rotationZParameters.OutputMinMax = minMax;
        }

        public void SetTarget(Transform target)
        {
            _target.AssignConstantValue(target);
        }

        #endregion
    }
}