using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Player;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class PickaxeInteractionReceiver : InteractionReceiver
    {
        [SerializeField] private UnityEvent<HitInfo> _onPickaxeInteract;

        public void ReceiveInteraction(HitInfo param)
        {
            _onPickaxeInteract.Invoke(param);
            OnInteract.Invoke();
        }
    }
}