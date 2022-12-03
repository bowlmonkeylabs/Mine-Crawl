using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class SpawnObjectsUtil
    {
        public static bool GetPointTowards(Vector3 position, Vector3 direction, out Vector3 result, LayerMask layerMask, float checkDistance)
        {
            var didHit = Physics.Raycast(new Ray(position, direction), out var hitInfo, checkDistance, layerMask);
            if (didHit)
            {
                result = hitInfo.point;
                return true;
            }
            result = position;
            return false;
        }
    }
}