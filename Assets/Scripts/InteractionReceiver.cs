using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class InteractionReceiver : MonoBehaviour
    {
        public UnityEvent OnInteract;
        public UnityEvent<System.Object> OnInteractParam;
        
        public void ReceiveInteraction()
        {
            OnInteract.Invoke();
        }

        public void ReceiveInteraction(System.Object param)
        {
            OnInteractParam.Invoke(param);
        }
    }
}