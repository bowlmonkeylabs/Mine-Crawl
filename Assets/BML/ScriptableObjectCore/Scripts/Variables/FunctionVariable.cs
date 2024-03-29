﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BML.ScriptableObjectCore.Scripts.Utils;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Serializable]
    internal class ValueOperatorPair
    {
        [HideLabel]
        public FunctionVariableOperators op;

        [HideLabel, HideReferenceObjectPicker]
        public SafeFloatValueReference value = new SafeFloatValueReference();
    }

    internal enum FunctionVariableOperators {
        Add,
        Subtract,
        Multiply,
        Divide,
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        Min,
        Max,
    }

    internal static class FunctionVariableOperatorsExtensions
    {
        public delegate float Operator(float value1, float value2);
        
        public static Operator AsFunction(this FunctionVariableOperators op) => OperatorToFunction[op];
        public static string AsString(this FunctionVariableOperators op) => OperatorToString[op];

        private static float Add(float a, float b) => a + b;
        private static float Subtract(float a, float b) => a - b;
        private static float Multiply(float a, float b) => a * b;
        private static float Divide(float a, float b) => a / b;
        private static float Equal(float a, float b) => Mathf.Approximately(a, b) ? 1f : 0f;
        private static float NotEqual(float a, float b) => !Mathf.Approximately(a, b) ? 1f : 0f;
        private static float LessThan(float a, float b) => a < b ? 1f : 0f;
        private static float LessThanOrEqual(float a, float b) => a <= b ? 1f : 0f;
        private static float GreaterThan(float a, float b) => a > b ? 1f : 0f;
        private static float GreaterThanOrEqual(float a, float b) => a >= b ? 1f : 0f;
        private static float Min(float a, float b) => Mathf.Min(a, b);
        private static float Max(float a, float b) => Mathf.Max(a, b);
        
        private static Dictionary<FunctionVariableOperators, Operator> OperatorToFunction = new Dictionary<FunctionVariableOperators,Operator>
        {
            {FunctionVariableOperators.Add, Add},
            {FunctionVariableOperators.Subtract, Subtract},
            {FunctionVariableOperators.Multiply, Multiply},
            {FunctionVariableOperators.Divide, Divide},
            {FunctionVariableOperators.Equal, Equal},
            {FunctionVariableOperators.NotEqual, NotEqual},
            {FunctionVariableOperators.LessThan, LessThan},
            {FunctionVariableOperators.LessThanOrEqual, LessThanOrEqual},
            {FunctionVariableOperators.GreaterThan, GreaterThan},
            {FunctionVariableOperators.GreaterThanOrEqual, GreaterThanOrEqual},
            {FunctionVariableOperators.Min, Min},
            {FunctionVariableOperators.Max, Max},
        };
        
        private static Dictionary<FunctionVariableOperators, string> OperatorToString = new Dictionary<FunctionVariableOperators, string>
        {
            {FunctionVariableOperators.Add, "+"},
            {FunctionVariableOperators.Subtract, "-"},
            {FunctionVariableOperators.Multiply, "*"},
            {FunctionVariableOperators.Divide, "/"},
            {FunctionVariableOperators.Equal, "=="},
            {FunctionVariableOperators.NotEqual, "!="},
            {FunctionVariableOperators.LessThan, "<"},
            {FunctionVariableOperators.LessThanOrEqual, "<="},
            {FunctionVariableOperators.GreaterThan, ">"},
            {FunctionVariableOperators.GreaterThanOrEqual, ">="},
            {FunctionVariableOperators.Min, "MIN"},
            {FunctionVariableOperators.Max, "MAX"},
        };
    }
    
    [Required]
    [CreateAssetMenu(fileName = "FunctionVariable", menuName = "BML/Variables/FunctionVariable", order = 0)]
    public class FunctionVariable : SerializedScriptableObject, IFloatValue
    {
#region Inspector

        [TextArea (7, 10)]
        [HideInInlineEditors]
        public string Description;

        [Title("$Equation")]
        [LabelText("Operations"), LabelWidth(60f)]
        [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "CustomAddOperation")]
        [SerializeField]
        private List<ValueOperatorPair> Operations = new List<ValueOperatorPair>();

        private void CustomAddOperation()
        {
            Operations.Add(new ValueOperatorPair());
        }
        
        [ShowInInspector]
        [LabelWidth(60f)]
        [ReadOnly]
        private float _result;
        
#endregion

#region Interface

        public delegate void OnUpdateDelta(float previousValue, float currentValue);
        protected event OnUpdateDelta _OnUpdateDelta;

        public float Value => _result;

        public string GetName() => name;
        public string GetDescription() => Description;
        public float GetValue(Type type) => Value;
        public float GetFloat() => Value;

        public void Subscribe(OnUpdate callback)
        {
            this.OnUpdate += callback;
        }
        public void Subscribe(OnUpdateDelta callback)
        {
            this._OnUpdateDelta += callback;
        }

        public void Unsubscribe(OnUpdate callback)
        {
            this.OnUpdate -= callback;
        }
        
        public void Unsubscribe(OnUpdateDelta callback)
        {
            this._OnUpdateDelta -= callback;
        }
        
        public bool Save(string folderPath, string name = "")
        { 
            return this.SaveInstance(folderPath, name);
        }

#endregion
    
        protected event OnUpdate OnUpdate;

        private void OnEnable()
        {
            Recalculate();
            Operations.ForEach((o) => o.value.Subscribe(Recalculate));
        }

        private void OnDisable()
        {
            Operations.ForEach((o) => o.value.Unsubscribe(Recalculate));
        }

        [Button]
        private void Recalculate()
        {
            var _prev = _result;
            _result = Operations.Aggregate(0f, (accumulator, op) =>
            {
                var operation = op.op.AsFunction();
                return operation(accumulator, op.value.Value);
            });
            OnUpdate?.Invoke();
            _OnUpdateDelta?.Invoke(_prev, _result);
        }

        private string Equation
        {
            get
            {
                string equationStr = "Equation:";
                if (Operations == null || Operations.Count == 0)
                {
                    return equationStr;
                }
                var equation = new StringBuilder(equationStr);
                equation.Append(" v0");
                for (int i = 1; i < Operations.Count; i++)
                {
                    equation.Append($" {Operations[i].op.AsString()} v{i}");
                }
                return equation.ToString();
            }
        }
    }
}