using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.CustomAttributes;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using Mono.CSharp;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences
{
    [InlineProperty]
    [SynchronizedHeader]
    [HideReferenceObjectPicker]
    [Serializable]
    public class SafeTransformValueReference
    {
        #region Inspector

        public String Tooltip => this.Description;

        public enum TransformReferenceTypes {
            Transform = 0,
            TransformSceneReference = 1,
            GameObjectSceneReference = 2,
        }
        
        [VerticalGroup("Top")]
        [HorizontalGroup("Top/Split", LabelWidth = 0.001f)]
        [HorizontalGroup("Top/Split/Left", LabelWidth = .01f)]
        [BoxGroup("Top/Split/Left/Left", ShowLabel = false)]
        [PropertyTooltip("$Tooltip")]
        // [LabelText("T")] [LabelWidth(10f)]
        [HideLabel]
        [SerializeField]
        protected TransformReferenceTypes ReferenceTypeSelector;
        
        #region Reference values

        private bool _showTransform => ReferenceTypeSelector == TransformReferenceTypes.Transform;
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("@_showTransform")]
        [InlineEditor()]
        [SerializeField]
        protected Transform ReferenceValue_Transform;
        
        
        private bool _showTransformSceneReference => ReferenceTypeSelector == TransformReferenceTypes.TransformSceneReference;
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("@_showTransformSceneReference")]
        [InlineEditor()]
        [SerializeField]
        protected TransformSceneReference ReferenceValue_TransformSceneReference;

        
        private bool _showGameObjectSceneReference => ReferenceTypeSelector == TransformReferenceTypes.GameObjectSceneReference;
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("@_showGameObjectSceneReference")]
        [InlineEditor()]
        [SerializeField]
        protected GameObjectSceneReference ReferenceValue_GameObjectSceneReference;

        
        
        #endregion

        
        // protected bool InstanceNotConstant => (!UseConstant && EnableInstanceOptions);
        
        #endregion
        
        #region Interface

        public Transform Value
        {
            get
            {
                switch (ReferenceTypeSelector)
                {
                    case TransformReferenceTypes.Transform:
                        return ReferenceValue_Transform;
                    case TransformReferenceTypes.TransformSceneReference:
                        return ReferenceValue_TransformSceneReference?.Value;
                    case TransformReferenceTypes.GameObjectSceneReference:
                        return ReferenceValue_GameObjectSceneReference?.Value?.transform;
                    default:
                        Debug.LogError($"Trying to access Transform but none set in inspector!");
                        return default(Transform);
                }
            }
            set
            {
                switch (ReferenceTypeSelector)
                {
                    case TransformReferenceTypes.Transform:
                        if (ReferenceValue_Transform != null)
                            ReferenceValue_Transform = value;
                        break;
                    case TransformReferenceTypes.TransformSceneReference:
                        if (ReferenceValue_TransformSceneReference != null)
                            ReferenceValue_TransformSceneReference.Value = value;
                        break;
                    case TransformReferenceTypes.GameObjectSceneReference:
                        Debug.LogError($"Cannot set set Transform for type GameObjectSceneReference!");
                        break;
                    default:
                        Debug.LogError($"Trying to access Transform but none set in inspector!");
                        break;
                }
            }
        }

        public String Name
        {
            get
            {
                switch (ReferenceTypeSelector)
                {
                    case TransformReferenceTypes.Transform:
                        return ReferenceValue_Transform?.name;
                    case TransformReferenceTypes.TransformSceneReference:
                        return ReferenceValue_TransformSceneReference?.name;
                    case TransformReferenceTypes.GameObjectSceneReference:
                        return ReferenceValue_GameObjectSceneReference?.name;
                }
                return "<Missing Transform>";
            }
        }
        
        public String Description
        {
            get
            {
                switch (ReferenceTypeSelector)
                {
                    case TransformReferenceTypes.Transform:
                        return ReferenceValue_Transform?.name;
                    case TransformReferenceTypes.TransformSceneReference:
                        return ReferenceValue_TransformSceneReference?.Description;
                    case TransformReferenceTypes.GameObjectSceneReference:
                        return ReferenceValue_GameObjectSceneReference?.Description;
                }
                return "<Missing Transform>";
            }
        }

        // public void Subscribe(OnUpdate callback)
        // {
        //     switch (ReferenceTypeSelector)
        //     {
        //         case TransformReferenceTypes.Transform:
        //             // ReferenceValue_Transform?.Subscribe(callback);
        //             break;
        //         case TransformReferenceTypes.TransformSceneReference:
        //             // ReferenceValue_TransformSceneReference?.Subscribe(callback);
        //             break;
        //     }
        // }
        //
        // public void Unsubscribe(OnUpdate callback)
        // {
        //     switch (ReferenceTypeSelector)
        //     {
        //         case TransformReferenceTypes.Transform:
        //             // ReferenceValue_Transform?.Unsubscribe(callback);
        //             break;
        //         case TransformReferenceTypes.TransformSceneReference:
        //             // ReferenceValue_TransformSceneReference?.Unsubscribe(callback);
        //             break;
        //     }
        // }

        #endregion
    }
}