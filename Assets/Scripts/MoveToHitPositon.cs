using UnityEngine;

namespace BML.Scripts
{
    public class MoveToHitPositon : MonoBehaviour
    {
        public void Move(HitInfo hitInfo)
        {
            transform.position = hitInfo.HitPositon;
        }
    }
}