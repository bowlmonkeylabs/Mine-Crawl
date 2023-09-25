using System;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using BML.ScriptableObjectCore.Scripts.Variables.ValueTypeVariables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "BML/Variables/BoolVariable", order = 0)]
    public class BoolVariable : ValueTypeVariable<bool>, IValue<int>
    {
        public string GetName() => name;
        public string GetDescription() => Description;
        public int GetValue(System.Type type) => Value ? 1 : 0;
    }
    
    [Serializable]
    [InlineProperty]
    public class BoolReference : ValueTypeReference<bool, BoolVariable> { }
}