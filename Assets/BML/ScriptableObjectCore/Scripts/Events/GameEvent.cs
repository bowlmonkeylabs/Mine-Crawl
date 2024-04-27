using System;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.CustomAttributes;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Events
{
    [Required]
    [SynchronizedHeader]
    [CreateAssetMenu(fileName = "GameEvent", menuName = "BML/Events/GameEvent", order = 0)]
    public class GameEvent : ScriptableVariableBase
    {
        [TextArea (7, 10)] [HideInInlineEditors] public string Description;
        
        private List<GameEventListener> listeners = 
            new List<GameEventListener>();
        
        public event OnUpdate OnUpdate;
        
        #region Code-based Registration

        public void Subscribe(OnUpdate callback)
        {
            if (enableLogs) Debug.Log($"Subscribe {this.name} ({callback.Method.Name})");
            this.OnUpdate += callback;
        }

        public void Unsubscribe(OnUpdate callback)
        {
            if (enableLogs) Debug.Log($"Unsubscribe {this.name} ({callback.Method.Name})");
            this.OnUpdate -= callback;
        }

        #endregion
        
        #region GameEventListener Registration

        public void RegisterListener(GameEventListener listener)
        {
            if (enableLogs) Debug.Log($"RegisterListener {this.name} ({listener.name})");
            listeners.Add(listener);
        }

        public void UnregisterListener(GameEventListener listener)
        {
            if (enableLogs) Debug.Log($"UnregisterListener {this.name} ({listener.name})");
            listeners.Remove(listener);
        }

        #endregion

        public void Raise()
        {
            if (enableLogs) Debug.Log($"Raise {this.name}");
            OnUpdate?.Invoke();

            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }
        
        public override void Reset()
        {
            OnUpdate = null;
            listeners.Clear();
        }

        
    }
}