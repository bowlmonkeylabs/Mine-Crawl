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
    }
}