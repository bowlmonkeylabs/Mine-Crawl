using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Compass;
using BML.Scripts.Player;
using Codice.CM.Common.Tree.Partial;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

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

        [SerializeField] private bool _enableHoming = false;
        [SerializeField, ShowIf("_enableHoming")] private LayerMask _homingTargetAcquisitionLayerMask;
        [SerializeField, ShowIf("_enableHoming")] private SafeTransformValueReference _homingTarget;
        [SerializeField, ShowIf("_enableHoming")] private PIDParameters _horizontalRotationPidParameters;
        [SerializeField, ShowIf("_enableHoming")] private bool _useSamePidParametersForVerticalRotation = true;
        [SerializeField, ShowIf("_enableHoming")] private bool _disableHorizontal = false;
        [SerializeField, ShowIf("_enableHoming")] private bool _disableVertical = false;
        [SerializeField, ShowIf("@_enableHoming && !_useSamePidParametersForVerticalRotation")] private PIDParameters _verticalRotationPidParameters;
        [ShowInInspector, ShowIf("_enableHoming")] private PID2 _horizontalRotationPidController;
        [ShowInInspector, ShowIf("_enableHoming")] private PID2 _verticalRotationPidController;
        
        #endregion

        private Vector3 moveDirection;

        private Vector3 _positionLastFixedUpdate;
        private float _traveledDistance;

        #region Unity lifecycle

        private void Start()
        {
            if (_enableHoming)
            {
                _horizontalRotationPidController = new PID2(_horizontalRotationPidParameters);
                _verticalRotationPidController = new PID2(_useSamePidParametersForVerticalRotation ? _horizontalRotationPidParameters : _verticalRotationPidParameters);
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
                bool didHit = Physics.SphereCast(transform.position, 1f, transform.forward, out var hitInfo, _limitRange, _homingTargetAcquisitionLayerMask);
                if (didHit)
                {
                    _homingTarget.AssignConstantValue(hitInfo.transform);
                }
                else
                {
                    bool didHit2 = Physics.SphereCast(transform.position, 3f, transform.forward, out var hitInfo2, _limitRange, _homingTargetAcquisitionLayerMask);
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
                    
                    var torque = (distanceToTargetFac * (_disableHorizontal ? 0 : horizontalTorqueCorrection) * Vector3.up) + (distanceToTargetFac * (_disableVertical ? 0 : verticalTorqueCorrection) * Vector3.right);
                    
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

            if (_enableHoming && _homingTarget.Value != null)
            {
                var targetPosition = _homingTarget.Value.transform.position;
                var targetDirection = targetPosition - position;
                Shapes.Draw.Line(position, position + targetDirection.normalized, Color.yellow);
            }
        }

        #endregion

        #region Public interface

        public void ChangeDirection(HitInfo hitInfo)
        {
            Deflect(hitInfo.HitDirection);
        }

        public void Deflect(Vector3 deflectDirection)
        {
            gameObject.layer = LayerMask.NameToLayer(deflectLayer);
            
            moveDirection = deflectDirection.normalized;
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

        
    }
}