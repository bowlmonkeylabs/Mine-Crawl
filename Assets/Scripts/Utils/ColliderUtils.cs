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
                
                Physics.ClosestPoint(point, collider, collider.bounds.center, colliderRotation);
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
    }
}