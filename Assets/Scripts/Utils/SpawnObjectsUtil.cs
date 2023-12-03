using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class SpawnObjectsUtil
    {
        public static bool GetPointTowards(Vector3 position, Vector3 direction, out Vector3 hitPoint, out Vector3 hitNormal, LayerMask layerMask, float checkDistance)
        {
            var didHit = Physics.Raycast(new Ray(position, direction), out var hitInfo, checkDistance, layerMask);
            if (didHit)
            {
                hitPoint = hitInfo.point;
                hitNormal = hitInfo.normal;
                return true;
            }
            hitPoint = position;
            hitNormal = hitInfo.normal;
            return false;
        }
    }
}