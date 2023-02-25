using UnityEngine;

namespace BML.Scripts.CaveV2
{
    public interface IPlayerController
    {
        public void SetPosition(Vector3 destination, bool resetFallDamage = true);
    }
}