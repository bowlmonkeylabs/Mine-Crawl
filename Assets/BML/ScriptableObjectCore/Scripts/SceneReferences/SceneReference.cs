using BML.ScriptableObjectCore.Scripts.CustomAttributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.SceneReferences
{
    [SynchronizedHeader]
    public class SceneReference<T> : ScriptableObject
    {
        [TextArea (7, 10)] [HideInInlineEditors] public string Description;
        [SerializeField] private T reference;
    
        public T Value
        {
            get => reference;
            set => reference = value;
        }
    }
}

