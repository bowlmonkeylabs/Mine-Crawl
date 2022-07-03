using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Player;

namespace BML.Scripts
{
    public class PickaxeInteractionReceiver : InteractionReceiver
    {
        public UnityEvent<PickaxeHitInfo> OnPickaxeInteract;

        public void ReceiveInteraction(PickaxeHitInfo param)
        {
            OnPickaxeInteract.Invoke(param);
            OnInteract.Invoke();
        }
    }
}