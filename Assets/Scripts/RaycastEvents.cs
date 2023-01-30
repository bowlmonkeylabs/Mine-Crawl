using System;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class RaycastEvents : MonoBehaviour
    {
        [Tooltip("Mask of layers considered a hit")]
        [SerializeField] private LayerMask _hitMask;
        [Tooltip("Mask of layers considered an obstacle (doesn't trigger hit but consumes raycast)")]
        [SerializeField] private LayerMask _obstacleMask;
        [Tooltip("Max distance raycast can travel (Use 0 for infinity)")]
        [SerializeField] private float _raycastMaxDistance = 0f;
        [Tooltip("Radius of cast (Use 0 for raycast instead of spherecast)")]
        [SerializeField] private float _thicknessRadius = 0f;
        [Tooltip("How long to wait between raycast checks")]
        [SerializeField, SuffixLabel("s")] private float _raycastDelay = .05f;

        [SerializeField] private UnityEvent _onHit;
        [SerializeField] private UnityEvent _onHitObstacle;

        private float lastRaycastTime = Mathf.NegativeInfinity;

        private void Update()
        {
            if (lastRaycastTime + _raycastDelay > Time.time)
                return;

            lastRaycastTime = Time.time;
            
            RaycastHit hit;
            Vector3 dir = transform.forward.normalized;
            LayerMask hitMask = _hitMask | _obstacleMask;
            float raycastDist = Mathf.Approximately(0f, _raycastMaxDistance) ? Mathf.Infinity : _raycastMaxDistance;

            // Use raycast or spherecast based on whether radius is provided
            bool hitSomething = Mathf.Approximately(0f, _thicknessRadius)
                ? Physics.Raycast(transform.position, dir, out hit, raycastDist, hitMask,
                    QueryTriggerInteraction.Ignore)
                : Physics.SphereCast(transform.position, _thicknessRadius, dir, out hit, raycastDist, hitMask,
                    QueryTriggerInteraction.Ignore);

            if (!hitSomething)
                return;
            
            if (hit.collider.gameObject.IsInLayerMask(_obstacleMask))
            {
                _onHitObstacle.Invoke();
            }
            else if (hit.collider.gameObject.IsInLayerMask(_hitMask))
            {
                _onHit.Invoke();
            }
        }
    }
}