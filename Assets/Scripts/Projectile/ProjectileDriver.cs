using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Compass;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    public class ProjectileDriver : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private Rigidbody rb;
        [SerializeField] private float speed = 10f;
        [SerializeField] private int _limitRange = 0;
        private bool _doLimitRange => _limitRange > 0;
        private bool _rangeLimitExceeded = false;
        [SerializeField, ShowIf("_doLimitRange")] private float _dragWhenRangeLimitExceeded = 3f; 
        
        [SerializeField] private string deflectLayer = "EnemyProjectileDeflected";
        [SerializeField] private TransformSceneReference mainCameraRef;

        [SerializeField] 
        private SafeBoolValueReference _enableHoming;
        private bool _showHomingParameters => (_enableHoming.Value || !_enableHoming.UseConstant || _enableHomingOnDeflect.Value || !_enableHomingOnDeflect.UseConstant);
        [SerializeField]
        private SafeBoolValueReference _enableHomingOnDeflect;
        [HorizontalGroup("EnableHomingAxes"), ShowIf("_showHomingParameters")] [SerializeField] 
        private bool _enableHorizontalHoming = true;
        [HorizontalGroup("EnableHomingAxes"), ShowIf("_showHomingParameters")] [SerializeField] 
        private bool _enableVerticalHoming = true;
        [FoldoutGroup("Homing", true), ShowIf("_showHomingParameters")] [SerializeField, Range(0f, 1f), OnValueChanged("UpdateCurvability"), Tooltip("Leave curvability set to 0 if you want to manually tweak the homing PID parameters.")] 
        private float _curvability;
        private bool _doUseCurvability => _curvability > 0;
        [FoldoutGroup("Homing"), ShowIf("_showHomingParameters")] [SerializeField]
        private CurveVariable _curvabilityToMinMaxOutput;
        [FoldoutGroup("Homing"), ShowIf("_showHomingParameters")] [SerializeField] 
        private LayerMask _homingTargetAcquisitionLayerMask;
        [FoldoutGroup("Homing"), ShowIf("_showHomingParameters")] [SerializeField] 
        private SafeTransformValueReference _homingTarget;
        [FoldoutGroup("Homing"), ShowIf("_showHomingParameters")] [SerializeField] 
        private PIDParameters _horizontalRotationPidParameters;
        [FoldoutGroup("Homing"), ShowIf("@_showHomingParameters && !_doUseCurvability")] [SerializeField] 
        private bool _useSamePidParametersForVerticalRotation = true;
        [FoldoutGroup("Homing"), ShowIf("@_showHomingParameters && !_doUseCurvability && !_useSamePidParametersForVerticalRotation")] [SerializeField] 
        private PIDParameters _verticalRotationPidParameters;
        [FoldoutGroup("Homing")] [ShowInInspector, ShowIf("_showHomingParameters")]
        private PID2 _horizontalRotationPidController;
        [FoldoutGroup("Homing")] [ShowInInspector, ShowIf("_showHomingParameters")] 
        private PID2 _verticalRotationPidController;
        
        #endregion

        private Vector3 moveDirection;

        private Vector3 _positionLastFixedUpdate;
        private float _traveledDistance;
        private void UpdateCurvability()
        {
            if (_enableHoming)
            {
                if (_doUseCurvability)
                {
                    var size = _curvabilityToMinMaxOutput.Value.Evaluate(_curvability);
                    _horizontalRotationPidParameters.OutputMinMax = new Vector2(-size, size);
                }
                else
                {
                    _horizontalRotationPidParameters.OutputMinMax = Vector2.zero;
                }
            }
        }

        #region Unity lifecycle

        private void Start()
        {
            if (_enableHoming)
            {
                InitializePidControllers();
            }

            if (_doLimitRange)
            {
                _traveledDistance = 0;
            }
            _positionLastFixedUpdate = transform.position;
            moveDirection = transform.forward;
            ApplyVelocity();
        }
        
        private void FixedUpdate()
        {
            if (_doLimitRange)
            {
                float distanceThisUpdate = (transform.position - _positionLastFixedUpdate).magnitude;
                _traveledDistance += distanceThisUpdate;
                if (_traveledDistance >= _limitRange && !_rangeLimitExceeded)
                {
                    _rangeLimitExceeded = true;
                    rb.useGravity = true;
                    rb.drag = _dragWhenRangeLimitExceeded;
                }
                _positionLastFixedUpdate = transform.position;
            }

            if (_doLimitRange && _rangeLimitExceeded)
            {
                return;
            }
            
            if (_enableHoming)
            {
                // Update target selection
                // TODO
                float checkDistance = _limitRange > 0 ? _limitRange : speed * 2;
                bool didHit = Physics.SphereCast(transform.position, 1f, transform.forward, out var hitInfo, checkDistance, _homingTargetAcquisitionLayerMask);
                if (didHit)
                {
                    _homingTarget.AssignConstantValue(hitInfo.transform);
                }
                else
                {
                    bool didHit2 = Physics.SphereCast(transform.position, 3f, transform.forward, out var hitInfo2, checkDistance, _homingTargetAcquisitionLayerMask);
                    if (didHit2)
                    {
                        _homingTarget.AssignConstantValue(hitInfo2.transform);
                    }
                    else
                    {
                        
                    }
                }

                if (_homingTarget?.Value != null)
                {
                    // Adjust direction towards target
                    var targetPosition = _homingTarget.Value.position;
                    var rotation = transform.rotation;
                    Vector3 targetDirection = (targetPosition - transform.position);
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

                    var currentOffsetFromTargetEulerAngles = rotation.eulerAngles - targetRotation.eulerAngles;
                    
                    if (currentOffsetFromTargetEulerAngles.x <= -180) currentOffsetFromTargetEulerAngles.x += 360;
                    else if (currentOffsetFromTargetEulerAngles.x >= 180) currentOffsetFromTargetEulerAngles.x -= 360;
                    
                    if (currentOffsetFromTargetEulerAngles.y <= -180) currentOffsetFromTargetEulerAngles.y += 360;
                    else if (currentOffsetFromTargetEulerAngles.y >= 180) currentOffsetFromTargetEulerAngles.y -= 360;
                    
                    float horizontalTorqueCorrection = _horizontalRotationPidController.GetOutput(currentOffsetFromTargetEulerAngles.y, 0, Time.fixedDeltaTime);
                    float verticalTorqueCorrection = _verticalRotationPidController.GetOutput(currentOffsetFromTargetEulerAngles.x, 0, Time.fixedDeltaTime);

                    float distanceToTargetFac = Mathf.Clamp01(targetDirection.magnitude / 10f);
                    
                    var torque = (distanceToTargetFac * (!_enableHorizontalHoming ? 0 : horizontalTorqueCorrection) * Vector3.up)
                                 + (distanceToTargetFac * (!_enableVerticalHoming ? 0 : verticalTorqueCorrection) * Vector3.right);
                    
                    rb.AddRelativeTorque(torque);
                    moveDirection = transform.forward;
                }
            }
            
            ApplyVelocity();
        }

        private void OnDrawGizmos()
        {
            var position = transform.position;
            Shapes.Draw.Line(position, position + transform.forward, Color.blue);
            
            var color1 = new Color(0.0f, 0.15f, 1f, 0.6f);
            var color2 = new Color(0.8f,  0.9f, 0f, 0.3f);
            Shapes.Draw.Line(ShapesBlendMode.Transparent, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Meters, transform.position, transform.position+transform.forward*_limitRange, color1, color1, 1f);
            Shapes.Draw.Line(ShapesBlendMode.Transparent, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Meters, transform.position, transform.position+transform.forward*_limitRange, color2, color2, 3f);

            if (_enableHoming && _homingTarget.Value != null)
            {
                var targetPosition = _homingTarget.Value.transform.position;
                var targetDirection = targetPosition - position;
                Shapes.Draw.Line(position, position + targetDirection.normalized, Color.yellow);
            }
        }

        #endregion

        #region Public interface

        public void Deflect(HitInfo hitInfo)
        {
            gameObject.layer = LayerMask.NameToLayer(deflectLayer);
            if (_enableHomingOnDeflect)
            {
                // TODO add a better interface to take info from the hit to augment projectile parameters (damage, speed, homing, homing targets/layers, curvability, color/visual)
                _enableHoming.ReferenceTypeSelector = SafeBoolValueReference.BoolReferenceTypes.Constant;
                _enableHoming.Value = true;
                InitializePidControllers();
            }
            
            Redirect(hitInfo.HitDirection);
        }

        public void Redirect(Vector3 newDirection)
        {
            moveDirection = newDirection.normalized;
            rb.position = mainCameraRef.Value.position;
            rb.transform.forward = moveDirection;
            ApplyVelocity();
        }

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
            ApplyVelocity();
        }

        #endregion

        #region Rigidbody

        private void ApplyVelocity()
        {
            rb.velocity = moveDirection * speed;
        }

        #endregion

        #region PID

        private void InitializePidControllers()
        {
            if (_horizontalRotationPidController == null) _horizontalRotationPidController = new PID2(_horizontalRotationPidParameters);
            _horizontalRotationPidController.Reset();
            if (_verticalRotationPidController == null) _verticalRotationPidController = new PID2(_useSamePidParametersForVerticalRotation || _doUseCurvability ? _horizontalRotationPidParameters : _verticalRotationPidParameters);
            _verticalRotationPidController.Reset();
        }

        #endregion

        
    }
}