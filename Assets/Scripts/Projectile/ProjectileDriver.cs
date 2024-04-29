using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Compass;
using BML.Scripts.PID;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using DrawXXL;

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
        [FoldoutGroup("Homing", true), ShowIf("_showHomingParameters")] [SerializeField, Range(0f, 1f), OnValueChanged("UpdateCurvability"), Tooltip("Leave curvability set to 0 if you want to manually tweak the homing PID parameters.")] 
        private float _curvability;
        private bool _doUseCurvability => _curvability > 0;
        [FoldoutGroup("Homing"), ShowIf("_showHomingParameters")] [SerializeField]
        private CurveVariable _curvabilityToMinMaxOutput;
        [FoldoutGroup("Homing"), ShowIf("_showHomingParameters")] [SerializeField] 
        private LayerMask _homingTargetAcquisitionLayerMask;
        [FoldoutGroup("Homing"), ShowIf("_showHomingParameters")] [SerializeField] 
        private PidRigidbody _homingPid;

        #endregion

        private Vector3 moveDirection;

        private Vector3 _positionLastFixedUpdate;
        private float _traveledDistance;
        private void UpdateCurvability()
        {
            if (_doUseCurvability)
            {
                var size = _curvabilityToMinMaxOutput.Value.Evaluate(_curvability);
                _homingPid.SetRotationOutputMinMax(new Vector2(-size, size));
            }
            else
            {
                _homingPid.SetRotationOutputMinMax(Vector2.zero);
            }
        }

        #region Unity lifecycle

        private void Start()
        {
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
            
            if (_enableHoming.Value)
            {
                // If no target is acquired yet, check for targets
                if (!_homingPid.HasTarget)
                {
                    // Update target selection
                    // TODO
                    float checkDistance = _limitRange > 0 ? _limitRange : speed * 2;
                    bool didHit = Physics.SphereCast(transform.position, 1f, transform.forward, out var hitInfo, checkDistance, _homingTargetAcquisitionLayerMask, QueryTriggerInteraction.Ignore);
                    if (didHit)
                    {
                        // Prioritize homing directly to the transform of the hit collider; Because of the offset baked into the rigidbody of 'flying' enemies (e.g. Bats), homing to hitInfo.transform caused the projectile to target the enemy origin rather than the center of mass.
                        var targetTransform = hitInfo.collider?.transform ?? hitInfo.rigidbody?.transform ?? hitInfo.transform;
                        _homingPid.SetTarget(targetTransform);
                    }
                    else
                    {
                        bool didHit2 = Physics.SphereCast(transform.position, 3f, transform.forward, out var hitInfo2, checkDistance, _homingTargetAcquisitionLayerMask, QueryTriggerInteraction.Ignore);
                        if (didHit2)
                        {
                            var targetTransform = hitInfo.collider?.transform ?? hitInfo.rigidbody?.transform ?? hitInfo.transform;
                            _homingPid.SetTarget(targetTransform);
                        }
                        else
                        {
                        
                        }
                    }
                }
                
                _homingPid.enabled = _homingPid.HasTarget;
                moveDirection = transform.forward;
            }
            
            ApplyVelocity();
        }

        private void OnDrawGizmos()
        {
            var position = transform.position;
            Shapes.Draw.Line(position, position + transform.forward, Color.blue);
            
            var color1 = new Color(0.0f, 0.15f, 1f, 0.6f);
            var color2 = new Color(0.8f,  0.9f, 0f, 0.3f);
            Shapes.Draw.Line(ShapesBlendMode.Transparent, LineGeometry.Volumetric3D, LineEndCap.Round, ThicknessSpace.Meters, transform.position, transform.position+transform.forward*_limitRange, color1, color1, .25f);

            if (_enableHoming.Value && _homingPid.HasTarget)
            {
                var targetPosition = _homingPid.Target.position;
                var targetDirection = targetPosition - position;
                Shapes.Draw.Line(position, position + targetDirection.normalized, Color.yellow);
            }
        }

        #endregion

        #region Public interface

        public void Deflect()
        {
            var fakeHitInfo = new HitInfo(DamageType.None, 0, -rb.transform.forward);
            Deflect(fakeHitInfo);
        }

        public void Deflect(HitInfo hitInfo)
        {
            gameObject.layer = LayerMask.NameToLayer(deflectLayer);
            if (_enableHomingOnDeflect.Value)
            {
                EnableHoming(true);
            }

            var newDirection = hitInfo.HitDirection ?? -rb.transform.forward;
            Redirect(newDirection, mainCameraRef.Value.position);
        }

        public void Redirect(Vector3 newDirection, Vector3? newPosition = null)
        {
            moveDirection = newDirection.normalized;
            rb.transform.rotation = Quaternion.LookRotation(moveDirection);
            if (newPosition != null)
                rb.position = newPosition.Value;
            ApplyVelocity();
        }

        public void EnableHoming(bool enable, Transform target = null, string newLayer = null)
        {
            // TODO add a better interface to take info from the hit to augment projectile parameters (damage, speed, homing, homing targets/layers, curvability, color/visual)
            if (enable)
            {
                _enableHoming.ReferenceTypeSelector = SafeBoolValueReference.BoolReferenceTypes.Constant;
                _enableHoming.Value = true;
            }
            else
            {
                _enableHoming.Value = false;
            }

            if (target != null)
                _homingPid.SetTarget(target);

            if (newLayer != null)
                gameObject.layer = LayerMask.NameToLayer(newLayer);
        }

        public void SetPosition(Vector3 newPosition)
        {
            rb.position = newPosition;
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
            // rb.velocity = Vector3.zero;
        }

        #endregion

        
    }
}