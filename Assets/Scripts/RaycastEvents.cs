using System;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class RaycastEvents : MonoBehaviour
    {
        #region Inspector

        [Tooltip("Defines origin and direction of raycast (if empty will use this transform)")]
        [SerializeField] private Transform _origin;
        [Tooltip("Mask of layers considered a hit")]
        [SerializeField] private LayerMask _hitMask;
        [Tooltip("Mask of layers considered an obstacle (doesn't trigger hit but consumes raycast)")]
        [SerializeField] private LayerMask _obstacleMask;
        [Tooltip("Max distance raycast can travel (Use 0 for infinity)")]
        [SerializeField] private float _raycastMaxDistance = 0f;
        [Tooltip("Radius of cast (Use 0 for raycast instead of spherecast)")]
        [SerializeField] private float _thicknessRadius = 0f;
        [FormerlySerializedAs("_thicknessRadius")]
        [Tooltip("Optionally, use a different radius for the obstacle check. Radius of cast (Use 0 for raycast instead of spherecast)")]
        [SerializeField] private float _obstacleThicknessRadius = 0f;
        [Tooltip("How long to wait between raycast checks")]
        [SerializeField, SuffixLabel("s")] private float _raycastDelay = .05f;

        [SerializeField] private UnityEvent _onHit;
        [SerializeField] private UnityEvent _onHitObstacle;

        #endregion
        
        private float lastRaycastTime = Mathf.NegativeInfinity;

        #region Unity lifecycle

        private void Update()
        {
            if (lastRaycastTime + _raycastDelay > Time.time)
            {
                return;
            }
            lastRaycastTime = Time.time;
            
            bool didHitTarget, didHitObstacle;

            Transform originTransform = _origin != null ? _origin : transform;
            Vector3 origin = originTransform.position;
            Vector3 dir = originTransform.forward;
            float raycastDist = Mathf.Approximately(0f, _raycastMaxDistance) ? Mathf.Infinity : _raycastMaxDistance;
            
            bool separateObstacleCheck = !Mathf.Approximately(_thicknessRadius, _obstacleThicknessRadius);
            if (separateObstacleCheck)
            {
                RaycastHit targetHit, obstacleHit;
                
                didHitTarget = Mathf.Approximately(0f, _thicknessRadius)
                    ? Physics.Raycast(origin, dir, out targetHit, raycastDist, _hitMask,
                        QueryTriggerInteraction.Ignore)
                    : Physics.SphereCast(origin, _thicknessRadius, dir, out targetHit, raycastDist, _hitMask,
                        QueryTriggerInteraction.Ignore);

                didHitObstacle = Mathf.Approximately(0f, _obstacleThicknessRadius)
                    ? Physics.Raycast(origin, dir, out obstacleHit, raycastDist, _obstacleMask,
                        QueryTriggerInteraction.Ignore)
                    : Physics.SphereCast(origin, _obstacleThicknessRadius, dir, out obstacleHit, raycastDist, _obstacleMask,
                        QueryTriggerInteraction.Ignore);

                if (didHitObstacle && didHitTarget)
                {
                    bool hitObstacleFirst = (obstacleHit.distance < targetHit.distance);
                    didHitTarget = !hitObstacleFirst;
                    didHitObstacle = hitObstacleFirst;
                }
            }
            else
            {
                RaycastHit hit;
                
                // Use raycast or spherecast based on whether radius is provided
                LayerMask combinedMask = _hitMask | _obstacleMask;
                bool didHit = Mathf.Approximately(0f, _thicknessRadius)
                    ? Physics.Raycast(origin, dir, out hit, raycastDist, combinedMask,
                        QueryTriggerInteraction.Ignore)
                    : Physics.SphereCast(origin, _thicknessRadius, dir, out hit, raycastDist, combinedMask,
                        QueryTriggerInteraction.Ignore);
                didHitTarget = hit.collider.gameObject.IsInLayerMask(_hitMask);
                didHitObstacle = hit.collider.gameObject.IsInLayerMask(_obstacleMask);
            }

            if (didHitObstacle)
            {
                _onHitObstacle.Invoke();
            }
            if (didHitTarget)
            {
                _onHit.Invoke();
            }
        }

        private void OnDrawGizmosSelected()
        {
            const int N = 10;
            
            bool separateObstacleCheck = !Mathf.Approximately(_thicknessRadius, _obstacleThicknessRadius);
            Transform originTransform = _origin ?? transform;
            var lineLength = originTransform.forward * (_raycastMaxDistance != 0 ? _raycastMaxDistance : 100f);
            var dir = originTransform.right;
            float angleIncrement = 360f / N;
            for (int i = 0; i < N; i++)
            {
                var rotation = Quaternion.Euler(0, 0, i * angleIncrement);
                dir = originTransform.rotation * Vector3.right.RotatePointAroundPivot(Vector3.zero, rotation);
                
                var start = originTransform.position + dir * _thicknessRadius;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(start, start + lineLength);

                if (separateObstacleCheck)
                {
                    var startObstacle = originTransform.position + dir * _obstacleThicknessRadius;
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(startObstacle, startObstacle + lineLength);
                }
            }

        }

        #endregion
        
    }
}