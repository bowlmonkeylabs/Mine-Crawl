using System;
using System.Collections.Generic;
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

        [SerializeField] private bool _enableLogs = false;

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

            if (_enableHoming.Value)
            {
                if (_homingPid.HasTarget)
                {
                    // Draw line to homing target
                    var targetPosition = _homingPid.Target.position;
                    var targetDirection = targetPosition - position;
                    Shapes.Draw.Line(position, position + targetDirection.normalized, Color.yellow);
                }
                else
                {
                    // Draw homing target acquisition spherecasts
                    void DrawSphereCast(Vector3 origin, Vector3 direction, float radius, float distance)
                    {
                        // Draw spheres 2*radius distance apart
                        // Always draw 1 at the start and 1 at the end
                        Gizmos.color = Color.white;
                        for (float i = 0; i < distance; i += 2 * radius)
                        {
                            Gizmos.DrawWireSphere(origin + direction * i, radius);
                        }
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(origin + direction * distance, radius);
                    }

                    for (int i = 0; i < HOMING_TARGET_CHECK_PARAMS.Length; i++)
                    {
                        var radius = HOMING_TARGET_CHECK_PARAMS[i].radius;
                        var drawStep = Mathf.Min(1f, radius);
                        DrawSphereCast(transform.position, transform.forward, radius, HomingTargetCheckDistance);
                    }
                }
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

        /// <summary>
        /// Resolve a homing target Transform from available hit parts. Prefers an existing
        /// ProjectileHomingTarget component; otherwise creates/returns a temp target at a
        /// sensible center (collider bounds or rigidbody center of mass). Intended as the
        /// single entry for target resolution to avoid redundancy.
        /// </summary>
        private Transform HomingHelper_GetHomingTarget(Transform rootTransform, Collider collider, Rigidbody? rigidbody, bool createTempGameObject)
        {
            var hitTransform = rigidbody?.transform ?? collider?.transform ?? rootTransform;
            var homingTargetComponent = hitTransform != null ? hitTransform.GetComponent<ProjectileHomingTarget>() : null;
            if (homingTargetComponent != null)
            {
                if (_enableLogs) Debug.Log($"{_debugIdAndStatus} Found existing ProjectileHomingTarget component on {hitTransform.name}, using its homing target: {homingTargetComponent.HomingTarget.name}");
                return homingTargetComponent.HomingTarget;
            }

            var targetPosition = (
                collider != null ? (Vector3?)collider.bounds.center :
                rigidbody != null ? (Vector3?)rigidbody.worldCenterOfMass :
                (Vector3?)null
            );

            Transform targetTransform;
            if (targetPosition != null && createTempGameObject && hitTransform != null)
            {
                string newTargetName = "Homing Target Temp";
                var existingTempTarget = hitTransform.Find(newTargetName);
                if (existingTempTarget != null)
                {
                    targetTransform = existingTempTarget;
                }
                else
                {
                    var newTargetGameObject = new GameObject(newTargetName).transform;
                    newTargetGameObject.parent = hitTransform;
                    newTargetGameObject.position = targetPosition.Value;
                    targetTransform = newTargetGameObject;
                }
            }
            else
            {
                targetTransform = hitTransform;
            }

            return targetTransform;
        }

        private bool HomingHelper_CheckLineOfSight(Collider collider, Vector3? checkPoint = null)
        {
            bool hasLineOfSight = false;

            // If line of sight is clear, expect to hit the same collider.
            // If line of sight is blocked, expect to hit a different collider.
            bool didHit = Physics.Linecast(
                transform.position,
                checkPoint ?? collider.bounds.center,
                out var hitInfo,
                _homingTargetBlockingLayerMask | _homingTargetAcquisitionLayerMask,
                QueryTriggerInteraction.Ignore
            );

            if (didHit)
            {
                if (hitInfo.collider == collider)
                {
                    // Line of sight is clear.
                    hasLineOfSight = true;
                }
                else
                {
                    // Line of sight is blocked.
                    hasLineOfSight = false;
                    if (_enableLogs) Debug.Log($"{_debugIdAndStatus} Line of sight to {(hitInfo.collider != null ? hitInfo.collider.name : (hitInfo.rigidbody != null ? hitInfo.rigidbody.name : hitInfo.transform.name))} is blocked by {hitInfo.collider.name}");
                }
            }
            else
            {
                // No hit. I would expect to hit the same collider, so not sure why we got no hit.
                hasLineOfSight = false;
                if (_enableLogs) Debug.LogWarning($"{_debugIdAndStatus} Line of sight check did not hit any collider when checking against {collider.name}");
            }

            return hasLineOfSight;
        }

        private float HomingTargetCheckDistance => 
            _limitRange > 0 
            ? (_limitRange - _traveledDistance) * 1.5f // Extend check distance slightly beyond remaining range
            : speed * 2; // Default check distance based on speed

        private readonly (float radius, bool doCheckOverlapSphere)[] HOMING_TARGET_CHECK_PARAMS = new (float, bool)[] 
        {
            (1f, false),
            (3f, true),
            (9f, true),
        };

        /// <summary>
        /// Internal helper used by TryAcquireHomingTarget. Not intended to be called directly as part of the public interface.
        /// Performs OverlapSphere scoring and LOS verification to catch very close targets that SphereCast won't detect.
        /// </summary>
        private Transform? HomingHelper_TryAcquireHomingTargetFromOverlap(Vector3 origin, float radius)
        {
            Transform? targetTransform = null;

            var colliders = Physics.OverlapSphere(
                origin,
                radius,
                _homingTargetAcquisitionLayerMask,
                QueryTriggerInteraction.Ignore
            );
            if (colliders.Length == 0)
            {
                return null;
            }

            // Score nearby colliders by forward alignment and proximity
            var candidates = new List<KeyValuePair<Collider, float>>();
            foreach (var col in colliders)
            {
                // Ignore our own rigidbody/colliders
                if (col.attachedRigidbody == rb ||
                    col.transform.IsChildOf(transform))
                {
                    continue;
                }

                var center = col.bounds.center;
                var toCenter = center - origin;
                var distance = toCenter.magnitude;
                if (Mathf.Approximately(distance, 0f))
                {
                    // Perfect overlap, can't score directionality. Ignore this candidate.
                    continue;
                }
                var forwardDot = Vector3.Dot(transform.forward, toCenter.normalized);
                // Emphasize forward alignment, de-emphasize distance (within radius)
                var score = forwardDot * 2f - (distance / Mathf.Max(radius, 0.0001f));
                candidates.Add(new KeyValuePair<Collider, float>(col, score));
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            candidates.Sort((a, b) => b.Value.CompareTo(a.Value));

            foreach (var candidate in candidates)
            {
                var col = candidate.Key;
                var center = col.bounds.center;
                // Verify line-of-sight
                bool hasLineOfSight = HomingHelper_CheckLineOfSight(col, center);
                if (!hasLineOfSight)
                {
                    continue;
                }

                targetTransform = HomingHelper_GetHomingTarget(col.transform, col, col.attachedRigidbody, true);
                break;
            }

            return targetTransform;
        }

        /// <summary>
        /// Entry point: attempts to acquire a homing target.
        /// Runs a near-field OverlapSphere pre-check, then progressively larger SphereCasts with line-of-sight verification.
        /// </summary>
        /// <param name="replaceCurrentTarget">Whether to replace the current homing target if one exists.</param>
        /// <returns>True if a homing target was acquired, false otherwise.</returns>
        private bool TryAcquireHomingTarget(bool replaceCurrentTarget)
        {
            bool acquiredTarget = false;

            if (!_homingPid.HasTarget || replaceCurrentTarget)
            {
                float checkDistance = HomingTargetCheckDistance;

                for (int i = 0; i < HOMING_TARGET_CHECK_PARAMS.Length; i++)
                {
                    float radius = HOMING_TARGET_CHECK_PARAMS[i].radius;
                    bool doCheckOverlapSphere = HOMING_TARGET_CHECK_PARAMS[i].doCheckOverlapSphere;

                    Transform targetTransform;

                    // Pre-check: OverlapSphere to handle initial overlaps (very close targets)
                    if (doCheckOverlapSphere)
                    {
                        targetTransform = HomingHelper_TryAcquireHomingTargetFromOverlap(transform.position, radius);
                        if (targetTransform != null)
                        {
                            _homingPid.SetTarget(targetTransform);
                            acquiredTarget = true;
                            if (_enableLogs) Debug.Log($"{_debugIdAndStatus} Acquired homing target via OverlapSphere: {(targetTransform != null ? targetTransform.name : "null")}");
                            break;
                        }
                    }

                    // SphereCast to find targets at distance
                    bool didHit = Physics.SphereCast(
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
                        if (_enableLogs) Debug.Log($"{_debugIdAndStatus} SphereCast hit: {(hitInfo.collider != null ? hitInfo.collider.name : (hitInfo.rigidbody != null ? hitInfo.rigidbody.name : hitInfo.transform.name))} at position {hitInfo.point}");

                        // Verify line-of-sight
                        bool hasLineOfSight = HomingHelper_CheckLineOfSight(hitInfo.collider, hitInfo.point);
                        if (!hasLineOfSight)
                        {
                            continue;
                        }

                        // Resolve homing target from hit object
                        targetTransform = HomingHelper_GetHomingTarget(hitInfo.transform, hitInfo.collider, hitInfo.rigidbody, true);
                        if (_enableLogs) Debug.Log($"{_debugIdAndStatus} Acquired homing target: {(targetTransform != null ? targetTransform.name : "null")}");

                        // Set as homing target
                        _homingPid.SetTarget(targetTransform);
                        acquiredTarget = true;

                        // Exit spherecast loop                            
                        break;
                    }
                }
            }

            return acquiredTarget;
        }

        #endregion
        
    }
}