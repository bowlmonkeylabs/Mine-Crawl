﻿using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.CustomAttributes;
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
    public class SafeFloatValueReference
    {
        #region Inspector
        
        public String Tooltip => !UseConstant ? this.Description : "";
        
        // [VerticalGroup("Top")]
        // [HorizontalGroup("Top/Split", LabelWidth = 0.001f)]
        // [HorizontalGroup("Top/Split/Left", LabelWidth = .01f)]
        // [BoxGroup("Top/Split/Left/Left", ShowLabel = false)]
        // [PropertyTooltip("$Tooltip")]
        // [LabelText("C")]
        // [LabelWidth(10f)]
        // [SerializeField]
        // protected bool UseConstant = false;
        protected bool UseConstant => ReferenceTypeSelector == FloatReferenceTypes.Constant;
        
        public enum FloatReferenceTypes {
            Constant = 0,
            FloatVariable = 1,
            IntVariable = 2,
            EvaluateCurveVariable = 3,
        }
        
        [VerticalGroup("Top")]
        [HorizontalGroup("Top/Split", LabelWidth = 0.001f)]
        [HorizontalGroup("Top/Split/Left", LabelWidth = .01f)]
        [BoxGroup("Top/Split/Left/Left", ShowLabel = false)]
        [PropertyTooltip("$Tooltip")]
        // [LabelText("T")] [LabelWidth(10f)]
        [HideLabel]
        [SerializeField]
        protected FloatReferenceTypes ReferenceTypeSelector;
        
        // [HorizontalGroup("Top/Split/Left/Right", LabelWidth = .01f)]
        // [BoxGroup("Top/Split/Left/Right/Right", ShowLabel = false)]
        // [PropertyTooltip("$Tooltip")]
        // [LabelText("I")]
        // [LabelWidth(10f)]
        // [OnValueChanged("ResetInstanceOptions")]
        // [ShowIf("@!UseConstant")]
        // [SerializeField]
        // protected bool EnableInstanceOptions = false;
        //
        // [VerticalGroup("Middle")]
        // [VerticalGroup("Middle/Box/Top")]
        // [LabelText("Name")]
        // [LabelWidth(40f)]
        // [ShowIf("InstanceNotConstant")]
        // [SerializeField] 
        // protected String InstanceName = "{I} ";
        //
        // [VerticalGroup("Middle/Box/Middle")]
        // [LabelText("Folder")]
        // [LabelWidth(40f)]
        // [FolderPath(ParentFolder = "Assets/MyAssets/ScriptableObjects/", RequireExistingPath = true)]
        // [ShowIf("InstanceNotConstant")]
        // [SerializeField] 
        // protected String InstancePath = "IntermediateProperties";

        // [ValueDropdown("GetTypes")]
        // [BoxGroup("Middle/Box", ShowLabel = false)]
        // [VerticalGroup("Middle/Box/Middle2")]
        // [LabelText("Type")]
        // [LabelWidth(40f)]
        // [ShowIf("InstanceNotConstant")]
        // [SerializeField] 
        // protected Type InstanceType
        // {
        //     get
        //     {
        //         switch (ReferenceTypeSelector)
        //         {
        //             case FloatReferenceTypes.FloatVariable:
        //                 return typeof(FloatVariable);
        //             case FloatReferenceTypes.IntVariable:
        //                 return typeof(IntVariable);
        //             case FloatReferenceTypes.EvaluateCurveVariable:
        //                 return typeof(EvaluateCurveVariable);
        //             default:
        //                 return null;
        //         }
        //     }
        // }

        [HorizontalGroup("Top/Split", LabelWidth = 0.001f, Width = .7f)]
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("UseConstant")]
        [SerializeField]
        protected float ConstantValue;
        
        
        #region Reference values
        
        // public enum FloatReferenceTypes {
        //     None,
        //     FloatVariable,
        //     IntVariable,
        //     EvaluateCurveVariable,
        // }
        //
        // [BoxGroup("Top/Split/Right", ShowLabel = false)]
        // [HideLabel]
        // [HideIf("UseConstant")]
        // [SerializeField]
        // protected FloatReferenceTypes ReferenceTypeSelector;

        private bool _showFloatVariable => ReferenceTypeSelector == FloatReferenceTypes.FloatVariable;
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("@!UseConstant && _showFloatVariable")]
        [InlineEditor()]
        [SerializeField]
        protected FloatVariable ReferenceValue_FloatVariable;
        
        
        private bool _showIntVariable => ReferenceTypeSelector == FloatReferenceTypes.IntVariable;
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("@!UseConstant && _showIntVariable")]
        [InlineEditor()]
        [SerializeField]
        protected IntVariable ReferenceValue_IntVariable;
        
        
        private bool _showEvaluateCurveVariable => ReferenceTypeSelector == FloatReferenceTypes.EvaluateCurveVariable;
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("@!UseConstant && _showEvaluateCurveVariable")]
        [InlineEditor()]
        [SerializeField]
        protected EvaluateCurveVariable ReferenceValue_EvaluateCurveVariable;
        
        
        
        #endregion

        
        // protected bool InstanceNotConstant => (!UseConstant && EnableInstanceOptions);
        
        #endregion
        
        #region Interface

        public float Value
        {
            get
            {
                if (UseConstant) 
                    return ConstantValue;
                switch (ReferenceTypeSelector)
                {
                    case FloatReferenceTypes.FloatVariable:
                        return ReferenceValue_FloatVariable?.Value ?? 0;
                    case FloatReferenceTypes.IntVariable:
                        return ReferenceValue_IntVariable?.Value ?? 0;
                    case FloatReferenceTypes.EvaluateCurveVariable:
                        return ReferenceValue_EvaluateCurveVariable?.Value ?? 0;
                    default:
                        Debug.LogError($"Trying to access Float variable but none set in inspector!");
                        return default(float);
                }
            }
            set
            {
                if (UseConstant)
                {
                    ConstantValue = value;
                    return;
                }
                switch (ReferenceTypeSelector)
                {
                    case FloatReferenceTypes.FloatVariable:
                        if (ReferenceValue_FloatVariable != null)
                            ReferenceValue_FloatVariable.Value = value;
                        break;
                    case FloatReferenceTypes.IntVariable:
                        if (ReferenceValue_FloatVariable != null)
                            ReferenceValue_FloatVariable.Value = Mathf.FloorToInt(value);
                        break;
                    case FloatReferenceTypes.EvaluateCurveVariable:
                        // if (ReferenceValue_EvaluateCurveVariable != null)
                        //     ReferenceValue_EvaluateCurveVariable.Value = value;
                        break;
                    default:
                        Debug.LogError($"Trying to access Float variable but none set in inspector!");
                        break;
                }
            }
        }

        public String Name
        {
            get
            {
                if (UseConstant)
                    return $"{ConstantValue}";
                switch (ReferenceTypeSelector)
                {
                    case FloatReferenceTypes.FloatVariable:
                        return ReferenceValue_FloatVariable?.GetName();
                    case FloatReferenceTypes.IntVariable:
                        return ReferenceValue_IntVariable?.GetName();
                    case FloatReferenceTypes.EvaluateCurveVariable:
                        return ReferenceValue_EvaluateCurveVariable?.GetName();
                }
                return "<Missing Float>";
            }
        }
        
        public String Description
        {
            get
            {
                if (UseConstant)
                    return $"{ConstantValue}";
                switch (ReferenceTypeSelector)
                {
                    case FloatReferenceTypes.FloatVariable:
                        return ReferenceValue_FloatVariable?.GetDescription();
                    case FloatReferenceTypes.IntVariable:
                        return ReferenceValue_IntVariable?.GetDescription();
                    case FloatReferenceTypes.EvaluateCurveVariable:
                        return ReferenceValue_EvaluateCurveVariable?.GetDescription();
                }
                return "<Missing Float>";
            }
        }

        public void Subscribe(OnUpdate callback)
        {
            switch (ReferenceTypeSelector)
            {
                case FloatReferenceTypes.FloatVariable:
                    ReferenceValue_FloatVariable?.Subscribe(callback);
                    break;
                case FloatReferenceTypes.IntVariable:
                    ReferenceValue_IntVariable?.Subscribe(callback);
                    break;
                case FloatReferenceTypes.EvaluateCurveVariable:
                    ReferenceValue_EvaluateCurveVariable?.Subscribe(callback);
                    break;
            }
        }

        public void Unsubscribe(OnUpdate callback)
        {
            switch (ReferenceTypeSelector)
            {
                case FloatReferenceTypes.FloatVariable:
                    ReferenceValue_FloatVariable?.Unsubscribe(callback);
                    break;
                case FloatReferenceTypes.IntVariable:
                    ReferenceValue_IntVariable?.Unsubscribe(callback);
                    break;
                case FloatReferenceTypes.EvaluateCurveVariable:
                    ReferenceValue_EvaluateCurveVariable?.Unsubscribe(callback);
                    break;
            }
        }

        // [PropertyTooltip("Create an Instance SO")]
        // [VerticalGroup("Middle/Box/Bottom")]
        // [LabelWidth(.01f)]
        // [GUIColor(.85f, 1f, .9f)]
        // [ShowIf("InstanceNotConstant")]
        // [Button("Create Instance", ButtonSizes.Small)]
        // public void CreateInstance()
        // {
        //     switch ()
        //     {}
        //     ScriptableObject tempScriptableObj = Convert.ChangeType(ScriptableObject.CreateInstance(InstanceType), InstanceType) as ScriptableObject;
        //     String fullPath = $"Assets/MyAssets/ScriptableObjects/{InstancePath}/";
        //
        //     //Try saving scriptable object to desired path
        //     if (tempScriptableObj.Save(fullPath, InstanceName))
        //     {
        //         ReferenceValue = tempScriptableObj;
        //         EnableInstanceOptions = false;
        //     }
        // }
        //
        // private void ResetInstanceOptions()
        // {
        //     InstanceName = "{I} ";
        //     InstancePath = "IntermediateProperties";
        // }

        #endregion
    }
}