using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class SpawnObjectsUtil
    {
        public static Vector3 GetPointUnder(Vector3 position, LayerMask layerMask, float checkDistance)
        {
            var didHit = Physics.Raycast(new Ray(position, Vector3.down), out var hitInfo, checkDistance, layerMask);
            if (didHit)
            {
                return hitInfo.point;
            }
            return position;
        }
    }
}