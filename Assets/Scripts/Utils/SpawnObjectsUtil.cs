using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class SpawnObjectsUtil
    {
        public static Vector3 GetPointTowards(Vector3 position, Vector3 direction, LayerMask layerMask, float checkDistance)
        {
            var didHit = Physics.Raycast(new Ray(position, direction), out var hitInfo, checkDistance, layerMask);
            if (didHit)
            {
                return hitInfo.point;
            }
            return position;
        }
    }
}