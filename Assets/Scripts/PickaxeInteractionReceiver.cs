using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Player;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class PickaxeInteractionReceiver : InteractionReceiver
    {
        [SerializeField] private UnityEvent<PickaxeHitInfo> _onPickaxeInteract;

        public void ReceiveInteraction(PickaxeHitInfo param)
        {
            _onPickaxeInteract.Invoke(param);
            OnInteract.Invoke();
        }
    }
}