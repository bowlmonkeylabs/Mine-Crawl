using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts.Animation
{
    public class AnimEventReceiver : MonoBehaviour
    {
        [SerializeField] private List<AnimEvent> _animEvents = new List<AnimEvent>();

        [System.Serializable]
        public class AnimEvent
        {
            public string EventName;
            public UnityEvent Response;
        }

        public void ReceiveAnimEvent(string name)
        {
            if (_animEvents.IsNullOrEmpty())
            {
                Debug.LogWarning("Tried to receive AnimEvent but Anim Event List is empty or null!");
                return;
            }

            AnimEvent animEvent = _animEvents.First(a => a.EventName.Equals(name));

            if (animEvent == null)
            {
                Debug.LogWarning("No Anim Event exits with the received name!");
                return;
            }

            animEvent.Response.Invoke();
        }
    }
}