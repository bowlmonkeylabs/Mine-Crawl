using UnityEngine;

namespace BML.Scripts.CaveV2
{
    public interface IPlayerController
    {
        public void SetPositionAndRotation(Vector3 destination, Quaternion rotation, bool resetFallDamage = true);
    }
}