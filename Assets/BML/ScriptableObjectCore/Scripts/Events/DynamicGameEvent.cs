using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.CustomAttributes;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Events
{
    [Required]
    [SynchronizedHeader]
    [CreateAssetMenu(fileName = "DynamicGameEvent", menuName = "BML/Events/DynamicGameEvent", order = 0)]
    public class DynamicGameEvent : ScriptableVariableBase
    {
        [TextArea (7, 10)] [HideInInlineEditors] public String Description;
        public String Name => name;

        private List<DynamicGameEventListener> listeners = 
            new List<DynamicGameEventListener>();
    
        public event OnUpdate<System.Object> OnUpdate;

        private System.Object previous;

        #region Code-based Registration

        public void Subscribe(OnUpdate<System.Object> callback)
        {
            this.OnUpdate += callback;
        }

        public void Unsubscribe(OnUpdate<System.Object> callback)
        {
            this.OnUpdate -= callback;
        }

        #endregion


        #region DynamicGameEventListener Registration

        public void RegisterListener(DynamicGameEventListener listener)
        { listeners.Add(listener); }

        public void UnregisterListener(DynamicGameEventListener listener)
        { listeners.Remove(listener); }

        #endregion

        public void Raise(float obj) {
            this.Raise((object) obj);
        }

        public void Raise(string obj) {
            this.Raise((object) obj);
        }

        public void Raise(int obj) {
            this.Raise((object) obj);
        }

        public void Raise(System.Object obj)
        {
            OnUpdate?.Invoke(previous, obj);
        
            //Broadcast to monobehavior listeners
            for(int i = listeners.Count -1; i >= 0; i--)
                listeners[i].OnEventRaised(obj);
        
            previous = obj;
        }
        
        public override void Reset()
        {
            OnUpdate = null;
            listeners.Clear();
            previous = null;
        }
    }
}

