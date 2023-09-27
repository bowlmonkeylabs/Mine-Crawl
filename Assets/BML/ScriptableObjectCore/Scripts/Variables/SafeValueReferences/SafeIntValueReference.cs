using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.CustomAttributes;
using BML.ScriptableObjectCore.Scripts.Variables;
using Mono.CSharp;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences
{
    [InlineProperty]
    [SynchronizedHeader]
    [HideReferenceObjectPicker]
    [Serializable]
    public class SafeIntValueReference
    {
        public SafeIntValueReference(int constantValue)
        {
            ReferenceTypeSelector = IntReferenceTypes.Constant;
            ConstantValue = constantValue;
        }
        
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
        protected bool UseConstant => ReferenceTypeSelector == IntReferenceTypes.Constant;
        
        public enum IntReferenceTypes {
            Constant = 0,
            IntVariable = 1,
            FloatVariable = 2,
            BoolVariable = 3,
            FunctionVariable = 4,
            EvaluateCurveVariable = 5,
        }

        public enum IntRoundingBehavior
        {
            Truncate = 0,
            Round = 1,
        }
        
        [VerticalGroup("Top")]
        [HorizontalGroup("Top/Split", LabelWidth = 0.001f)]
        [HorizontalGroup("Top/Split/Left", LabelWidth = .01f)]
        [BoxGroup("Top/Split/Left/Left", ShowLabel = false)]
        [PropertyTooltip("$Tooltip")]
        // [LabelText("T")] [LabelWidth(10f)]
        [HideLabel]
        [SerializeField]
        protected IntReferenceTypes ReferenceTypeSelector;
        
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
        protected int ConstantValue;
        
        
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
        
        [FormerlySerializedAs("RoundingBehavior_FloatValue")]
        [FormerlySerializedAs("RoundingBehavior_FloatVariable")]
        [HideLabel]
        [ShowIf("@!UseConstant && (_showFloatVariable || _showFunctionVariable)")]
        [SerializeField]
        protected IntRoundingBehavior RoundingBehavior;

        private bool _showFloatVariable => ReferenceTypeSelector == IntReferenceTypes.FloatVariable;
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("@!UseConstant && _showFloatVariable")]
        [InlineEditor()]
        [SerializeField]
        protected FloatVariable ReferenceValue_FloatVariable;
        
        private bool _showIntVariable => ReferenceTypeSelector == IntReferenceTypes.IntVariable;
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("@!UseConstant && _showIntVariable")]
        [InlineEditor()]
        [SerializeField]
        protected IntVariable ReferenceValue_IntVariable;
        
        
        private bool _showBoolVariable => ReferenceTypeSelector == IntReferenceTypes.BoolVariable;
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("@!UseConstant && _showBoolVariable")]
        [InlineEditor()]
        [SerializeField]
        protected BoolVariable ReferenceValue_BoolVariable;
        
        
        private bool _showFunctionVariable => ReferenceTypeSelector == IntReferenceTypes.FunctionVariable;
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("@!UseConstant && _showFunctionVariable")]
        [InlineEditor()]
        [SerializeField]
        protected FunctionVariable ReferenceValue_FunctionVariable;
        
        
        private bool _showEvaluateCurveVariable => ReferenceTypeSelector == IntReferenceTypes.EvaluateCurveVariable;
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

        public int Value
        {
            get
            {
                if (UseConstant) 
                    return ConstantValue;
                switch (ReferenceTypeSelector)
                {
                    case IntReferenceTypes.FloatVariable:
                        if (ReferenceValue_FloatVariable == null)
                        {
                            return 0;
                        }
                        switch (RoundingBehavior)
                        {
                            default:
                            case IntRoundingBehavior.Truncate:
                                return (int) Math.Truncate(ReferenceValue_FloatVariable.Value);
                            case IntRoundingBehavior.Round:
                                return Mathf.RoundToInt(ReferenceValue_FloatVariable.Value);
                        }
                    case IntReferenceTypes.IntVariable:
                        return ReferenceValue_IntVariable?.Value ?? 0;
                    case IntReferenceTypes.BoolVariable:
                        return ReferenceValue_BoolVariable.Value ? 1 : 0;
                    case IntReferenceTypes.FunctionVariable:
                        if (ReferenceValue_FunctionVariable == null)
                        {
                            return 0;
                        }
                        switch (RoundingBehavior)
                        {
                            default:
                            case IntRoundingBehavior.Truncate:
                                return (int) Math.Truncate(ReferenceValue_FunctionVariable.Value);
                            case IntRoundingBehavior.Round:
                                return Mathf.RoundToInt(ReferenceValue_FunctionVariable.Value);
                        }
                    case IntReferenceTypes.EvaluateCurveVariable:
                        if (ReferenceValue_EvaluateCurveVariable == null)
                        {
                            return 0;
                        }
                        switch (RoundingBehavior)
                        {
                            default:
                            case IntRoundingBehavior.Truncate:
                                return (int) Math.Truncate(ReferenceValue_EvaluateCurveVariable.Value);
                            case IntRoundingBehavior.Round:
                                return Mathf.RoundToInt(ReferenceValue_EvaluateCurveVariable.Value);
                        }
                    default:
                        Debug.LogError($"Trying to access Int variable but none set in inspector!");
                        return default(int);
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
                    case IntReferenceTypes.FloatVariable:
                        if (ReferenceValue_FloatVariable != null)
                            ReferenceValue_FloatVariable.Value = value;
                        break;
                    case IntReferenceTypes.IntVariable:
                        if (ReferenceValue_FloatVariable != null)
                            ReferenceValue_FloatVariable.Value = Mathf.FloorToInt(value);
                        break;
                    case IntReferenceTypes.BoolVariable:
                        if (ReferenceValue_BoolVariable != null)
                            ReferenceValue_BoolVariable.Value = value != 0;
                        break;
                    case IntReferenceTypes.FunctionVariable:
                        break;
                    case IntReferenceTypes.EvaluateCurveVariable:
                        break;
                    default:
                        Debug.LogError($"Trying to access Int variable but none set in inspector!");
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
                    case IntReferenceTypes.FloatVariable:
                        return ReferenceValue_FloatVariable?.GetName();
                    case IntReferenceTypes.IntVariable:
                        return ReferenceValue_IntVariable?.GetName();
                    case IntReferenceTypes.BoolVariable:
                        return ReferenceValue_BoolVariable?.GetName();
                    case IntReferenceTypes.FunctionVariable:
                        return ReferenceValue_FunctionVariable?.GetName();
                    case IntReferenceTypes.EvaluateCurveVariable:
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
                    case IntReferenceTypes.FloatVariable:
                        return ReferenceValue_FloatVariable?.GetDescription();
                    case IntReferenceTypes.IntVariable:
                        return ReferenceValue_IntVariable?.GetDescription();
                    case IntReferenceTypes.BoolVariable:
                        return ReferenceValue_BoolVariable?.GetDescription();
                    case IntReferenceTypes.FunctionVariable:
                        return ReferenceValue_FunctionVariable?.GetDescription();
                    case IntReferenceTypes.EvaluateCurveVariable:
                        return ReferenceValue_EvaluateCurveVariable?.GetDescription();
                }
                return "<Missing Float>";
            }
        }

        public void Subscribe(OnUpdate callback)
        {
            switch (ReferenceTypeSelector)
            {
                case IntReferenceTypes.FloatVariable:
                    ReferenceValue_FloatVariable?.Subscribe(callback);
                    break;
                case IntReferenceTypes.IntVariable:
                    ReferenceValue_IntVariable?.Subscribe(callback);
                    break;
                case IntReferenceTypes.BoolVariable:
                    ReferenceValue_BoolVariable?.Subscribe(callback);
                    break;
                case IntReferenceTypes.FunctionVariable:
                    ReferenceValue_FunctionVariable?.Subscribe(callback);
                    break;
                case IntReferenceTypes.EvaluateCurveVariable:
                    ReferenceValue_EvaluateCurveVariable?.Subscribe(callback);
                    break;
            }
        }

        public void Unsubscribe(OnUpdate callback)
        {
            switch (ReferenceTypeSelector)
            {
                case IntReferenceTypes.FloatVariable:
                    ReferenceValue_FloatVariable?.Unsubscribe(callback);
                    break;
                case IntReferenceTypes.IntVariable:
                    ReferenceValue_IntVariable?.Unsubscribe(callback);
                    break;
                case IntReferenceTypes.BoolVariable:
                    ReferenceValue_BoolVariable?.Unsubscribe(callback);
                    break;
                case IntReferenceTypes.FunctionVariable:
                    ReferenceValue_FunctionVariable?.Unsubscribe(callback);
                    break;
                case IntReferenceTypes.EvaluateCurveVariable:
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