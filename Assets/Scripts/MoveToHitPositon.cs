using UnityEngine;

namespace BML.Scripts
{
    public class MoveToHitPositon : MonoBehaviour
    {
        public void Move(HitInfo hitInfo)
        {
            if (hitInfo.HitPositon.HasValue)
            {
                transform.position = hitInfo.HitPositon.Value;
            }
        }
    }
}