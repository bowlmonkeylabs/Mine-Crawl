using System;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "BML/Variables/FloatVariable", order = 0)]
    public class FloatVariable : Variable<float>, IFloatValue
    {
        public string GetName() => name;
        public string GetDescription() => Description;
        public float GetFloat() => Value;
        public float GetValue(System.Type type) => Value;

    }
}