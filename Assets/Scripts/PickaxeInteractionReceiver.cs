using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Player;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class PickaxeInteractionReceiver : InteractionReceiver
    {
        [SerializeField] private UnityEvent<HitInfo> _onPickaxeInteract;
        [SerializeField] private UnityEvent<HitInfo> _onPickaxeSecondaryInteract;

        public void ReceiveInteraction(HitInfo param)
        {
            _onPickaxeInteract.Invoke(param);
            OnInteract.Invoke();
        }
        
        public void ReceiveSecondaryInteraction(HitInfo param)
        {
            _onPickaxeSecondaryInteract.Invoke(param);
            OnInteract.Invoke();
        }
    }
}