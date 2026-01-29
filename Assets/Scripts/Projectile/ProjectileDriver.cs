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
        private LayerMask _homingTargetBlockingLayerMask;
        [FoldoutGroup("Homing"), ShowIf("_showHomingParameters")] [SerializeField] 
        private PidRigidbody _homingPid;

        #endregion

        private Vector3 moveDirection;

        private Vector3 _positionLastFixedUpdate;
        private float _traveledDistance;
        
        private bool _isDeflected = false;

        private string _debugIdAndStatus => $"ProjectileDriver ({gameObject.name}) [Deflected: {_isDeflected}] [Homing: {_enableHoming.Value}, HasTarget: {_homingPid.HasTarget}]";

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
                TryAcquireHomingTarget(false);
                
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
            _isDeflected = true;
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

        #region Homing utilities

        private Transform GetHomingTargetFromHitInfo(RaycastHit hitInfo, bool createTempGameObject)
        {
            var hitTransform = hitInfo.collider?.transform ?? hitInfo.rigidbody?.transform ?? hitInfo.transform;

            // Home to the center of mass or bounds center if possible
            var targetPosition = (
                hitInfo.collider != null ? hitInfo.collider.bounds.center :
                hitInfo.rigidbody != null ? hitInfo.rigidbody.worldCenterOfMass :
                (Vector3?) null
            );

            string hitPart = (hitInfo.rigidbody != null ? "Rigidbody" : (hitInfo.collider != null ? "Collider" : "Transform"));
            string hitPartName = (hitInfo.rigidbody != null ? hitInfo.rigidbody.name : (hitInfo.collider != null ? hitInfo.collider.name : hitTransform.name));
            // Debug.Log($"{_debugIdAndStatus} Acquired homing target: {hitTransform.name} at position {(targetPosition != null ? targetPosition.ToString() : "null")}, Hit part: ({hitPart}: {hitPartName})");

            Transform targetTransform;
            if (targetPosition != null && createTempGameObject)
            {
                var newTargetGameObject = new GameObject("Homing Target Temp").transform;
                newTargetGameObject.parent = hitTransform;
                newTargetGameObject.position = targetPosition.Value;

                targetTransform = newTargetGameObject;
            }
            else
            {
                targetTransform = hitTransform;
            }

            return targetTransform;
        }

        private readonly float[] HOMING_TARGET_SPHERECAST_RADII = new float[] {1f, 3f, 7f};

        /// <summary>
        /// Look ahead of the projectile, in a progressively larger spherecast, to find a suitable homing target.
        /// </summary>
        /// <param name="replaceCurrentTarget">Whether to replace the current homing target if one exists.</param>
        /// <returns>True if a homing target was acquired, false otherwise.</returns>
        private bool TryAcquireHomingTarget(bool replaceCurrentTarget)
        {
            bool acquiredTarget = false;

            if (!_homingPid.HasTarget || replaceCurrentTarget)
            {
                Debug.Log($"{_debugIdAndStatus} Trying to acquire homing target...");

                float checkDistance = _limitRange > 0 ? _limitRange - _traveledDistance : speed * 2;
                bool didHit = false;
                for (int i = 0; i < HOMING_TARGET_SPHERECAST_RADII.Length; i++)
                {
                    float radius = HOMING_TARGET_SPHERECAST_RADII[i];
                    didHit = Physics.SphereCast(
                        transform.position, 
                        radius, 
                        transform.forward, 
                        out var hitInfo, 
                        checkDistance, 
                        _homingTargetAcquisitionLayerMask, 
                        QueryTriggerInteraction.Ignore
                    );

                    if (didHit)
                    {
                        Debug.Log($"{_debugIdAndStatus} SphereCast hit: {(hitInfo.collider != null ? hitInfo.collider.name : (hitInfo.rigidbody != null ? hitInfo.rigidbody.name : hitInfo.transform.name))} at position {hitInfo.point}");

                        // Check for line of sight against blocking layers
                        bool didHit2 = Physics.Linecast(
                            transform.position, 
                            hitInfo.point, 
                            out var linecastHit, 
                            _homingTargetBlockingLayerMask | _homingTargetAcquisitionLayerMask, 
                            QueryTriggerInteraction.Ignore
                        );
                        if (didHit2)
                        {
                            // If not the same collider, then the line of sight is blocked
                            if (linecastHit.collider != hitInfo.collider)
                            {
                                Debug.Log($"{_debugIdAndStatus} Line of sight to {(hitInfo.collider != null ? hitInfo.collider.name : (hitInfo.rigidbody != null ? hitInfo.rigidbody.name : hitInfo.transform.name))} is blocked by {linecastHit.collider.name}");
                                continue;
                            }
                        }

                        var targetTransform = GetHomingTargetFromHitInfo(hitInfo, true);
                        _homingPid.SetTarget(targetTransform);
                        acquiredTarget = true;
                        Debug.Log($"{_debugIdAndStatus} Acquired homing target: {(targetTransform != null ? targetTransform.name : "null")}");
                        break;
                    }
                }
            }

            return acquiredTarget;
        }

        #endregion
        
    }
}