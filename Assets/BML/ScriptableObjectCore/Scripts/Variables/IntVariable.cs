using System;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using BML.ScriptableObjectCore.Scripts.Variables.ValueTypeVariables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "IntVariable", menuName = "BML/Variables/IntVariable", order = 0)]
    public class IntVariable : ValueTypeVariable<int>, IFloatValue
    {
        public string GetName() => name;
        public string GetDescription() => Description;
        public float GetFloat() => Value;
        public float GetValue(System.Type type) => Value;

        public void Increment(int increment)
        {
            Value += increment;
        }
    }
    
    [Serializable]
    [InlineProperty]
    public class IntReference : Reference<int, IntVariable> { }
}