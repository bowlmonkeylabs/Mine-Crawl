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
            this.OnUpdate += callback;
        }

        public void Unsubscribe(OnUpdate callback)
        {
            this.OnUpdate -= callback;
        }

        #endregion
        
        #region GameEventListener Registration

        public void RegisterListener(GameEventListener listener)
        { listeners.Add(listener); }

        public void UnregisterListener(GameEventListener listener)
        { listeners.Remove(listener); }

        #endregion

        public void Raise()
        {
            OnUpdate?.Invoke();
            
            for(int i = listeners.Count -1; i >= 0; i--)
                listeners[i].OnEventRaised();
        }

        
    }
}