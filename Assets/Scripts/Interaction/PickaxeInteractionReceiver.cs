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
        [SerializeField] private UnityEvent _onHoverInteract;
        [SerializeField] private UnityEvent _onUnHoverInteract;

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

        public void ReceiveHoverInteraction()
        {
            _onHoverInteract.Invoke();
        }

        public void ReceiveUnHoverInteraction()
        {
            _onUnHoverInteract.Invoke();
        }
    }
}