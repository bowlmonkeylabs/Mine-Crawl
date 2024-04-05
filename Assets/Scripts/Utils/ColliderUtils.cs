using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class ColliderUtils
    {
        public static (bool isInCollider, Vector3 closestPoint) IsPointInsideCollider(this Collider col, Vector3 position)
        {
            Vector3 closestPoint = col.ClosestPoint(position);
            return (closestPoint == position, closestPoint);
        }

        public static Vector3 ClosestPointOnBounds(this IEnumerable<Collider> colliders, Vector3 point)
        {
            float closestSqrDistance = float.PositiveInfinity;
            Vector3 closestPoint = Vector3.positiveInfinity;
            foreach (var collider in colliders)
            {
                if (!collider.enabled) continue;
                
                Vector3 closestPointOnBounds = collider.ClosestPointOnBounds(point);
                float sqrDist = (closestPointOnBounds - point).sqrMagnitude;
                if (sqrDist < closestSqrDistance)
                {
                    closestSqrDistance = sqrDist;
                    closestPoint = closestPointOnBounds;
                }
            }

            return closestPoint;
        }
        
        public static Vector3 ClosestPointOnBounds(this IEnumerable<Collider> colliders, Vector3 point, Quaternion rotation)
        {
            float closestSqrDistance = float.PositiveInfinity;
            Vector3 closestPoint = Vector3.positiveInfinity;
            foreach (var collider in colliders)
            {
                if (!collider.enabled) continue;
                
                var colliderRotation = rotation * collider.transform.localRotation;
                var colliderForward = colliderRotation * Vector3.forward;
                var colliderForwardFlattened = Vector3.ProjectOnPlane(colliderForward, Vector3.up);
                colliderRotation = Quaternion.LookRotation(colliderForwardFlattened, Vector3.up);
                
                Vector3 closestPointOnBounds = Physics.ClosestPoint(point, collider, collider.bounds.center, colliderRotation);
                // Vector3 closestPointOnBounds = collider.ClosestPointOnBounds(point);
                float sqrDist = (closestPointOnBounds - point).sqrMagnitude;
                if (sqrDist < closestSqrDistance)
                {
                    closestSqrDistance = sqrDist;
                    closestPoint = closestPointOnBounds;
                }
            }

            return closestPoint;
        }

        private static RaycastHit[] _raycastHits = new RaycastHit[30];
        public static RaycastHit? Raycast(this HashSet<Collider> colliders, Vector3 origin, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            int numHits = Physics.RaycastNonAlloc(origin, direction, _raycastHits, maxDistance, layerMask, queryTriggerInteraction);
            (RaycastHit? hitInfo, float sqrDistance) min = (null, float.PositiveInfinity);
            for (int i = 0; i < numHits; i++)
            {
                var hit = _raycastHits[i];
                if (colliders.Contains(hit.collider))
                {
                    float sqrDist = origin.SqrDistance(hit.point);
                    if (sqrDist < min.sqrDistance)
                    {
                        min.sqrDistance = sqrDist;
                        min.hitInfo = hit;
                    }
                }
            }

            return min.hitInfo;
        }

        public static Vector3 GetRealWorldCenter(this Collider collider)
        {
            Vector3 targetColliderWorldCenter;
            
            if (collider is BoxCollider)
            {
                targetColliderWorldCenter = collider.transform.TransformPoint((collider as BoxCollider).center);
            }
            else if (collider is SphereCollider)
            {
                targetColliderWorldCenter = collider.transform.TransformPoint((collider as SphereCollider).center);
            }
            else if (collider is CapsuleCollider)
            {
                targetColliderWorldCenter = collider.transform.TransformPoint((collider as CapsuleCollider).center);
            }
            else
            {
                // For other types of colliders, use the bounds
                targetColliderWorldCenter = collider.bounds.center;
            }
            
            return targetColliderWorldCenter;
        }
    }
}