using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Player;

namespace BML.Scripts
{
    public class PickaxeInteractionReceiver : MonoBehaviour
    {
        public UnityEvent<int> OnInteractDamage;
        public UnityEvent<Vector3> OnInteractHitPoint;
        public UnityEvent<PickaxeHitInfo> OnInteract;

        public void ReceiveInteraction(PickaxeHitInfo param)
        {
            OnInteractDamage.Invoke(param.Damage);
            OnInteractHitPoint.Invoke(param.HitPositon);
            OnInteract.Invoke(param);
        }
    }
}