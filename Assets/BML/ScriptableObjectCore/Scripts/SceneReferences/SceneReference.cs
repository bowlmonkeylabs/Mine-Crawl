using BML.ScriptableObjectCore.Scripts.CustomAttributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.SceneReferences
{
    public delegate void OnUpdate();
    public delegate void OnUpdate<T>(T previousValue, T currentValue);
    
    [SynchronizedHeader]
    public class SceneReference<T> : ScriptableVariableBase
    {
        [HideInInlineEditors] public bool EnableDebugOnUpdate;
        [TextArea (7, 10)] [HideInInlineEditors] public string Description;
        [SerializeField] private T reference;
        
        protected T prevValue;
        protected event OnUpdate OnUpdate;
        protected event OnUpdate<T> OnUpdateDelta;
    
        public virtual T Value
        {
            get => reference;
            set
            { 
                prevValue = reference;
                reference = value;
                this.OnUpdateDelta?.Invoke(prevValue, reference);
                this.OnUpdate?.Invoke();
                if(EnableDebugOnUpdate)
                    Debug.LogError($"name: {name} | prevValue: {prevValue} | currentValue: {reference}");
                
                prevValue = reference;
                // Setting this means that 'prevValue' technically only contains the previous value during the frame an update occurs, otherwise it is in sync with 'runtimeValue'
                // But this allows delta updates to work even when we edit values through the inspector (which circumvents this Value.set() method)
            } 
        }
        
        [Button]
        public void BroadcastUpdate()
        {
            OnUpdate?.Invoke();
            OnUpdateDelta?.Invoke(prevValue, reference);
            
            prevValue = reference;
            // Setting this means that 'prevValue' technically only contains the previous value during the frame an update occurs, otherwise it is in sync with 'runtimeValue'
            // But this allows delta updates to work even when we edit values through the inspector (which circumvents the Value.set() method)
        }
        
        public void Subscribe(OnUpdate callback)
        {
            this.OnUpdate += callback;
        }

        public void Subscribe(OnUpdate<T> callback)
        {
            this.OnUpdateDelta += callback;
        }

        public void Unsubscribe(OnUpdate callback)
        {
            this.OnUpdate -= callback;
        }

        public void Unsubscribe(OnUpdate<T> callback)
        {
            this.OnUpdateDelta -= callback;
        }
        
        private void OnDisable()
        {
            OnUpdate = null;
            OnUpdateDelta = null;
            //Undo.DestroyObjectImmediate(this);
            //AssetDatabase.SaveAssets();
        }

    }
}

