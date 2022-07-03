using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class InteractionReceiver : MonoBehaviour
    {
        public UnityEvent OnInteract;
        
        public void ReceiveInteraction()
        {
            OnInteract.Invoke();
        }
    }
}