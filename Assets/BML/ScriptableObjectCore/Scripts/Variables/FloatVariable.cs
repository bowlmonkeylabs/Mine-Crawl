using System;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using BML.ScriptableObjectCore.Scripts.Variables.ValueTypeVariables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "BML/Variables/FloatVariable", order = 0)]
    public class FloatVariable : ValueTypeVariable<float>, IFloatValue
    {
        public string GetName() => name;
        public string GetDescription() => Description;
        public float GetFloat() => Value;
        public float GetValue(System.Type type) => Value;
    }
    
    [Serializable]
    [InlineProperty]
    public class FloatReference : Reference<float, FloatVariable> { }
}