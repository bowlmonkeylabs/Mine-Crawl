using System;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "Vector2Variable", menuName = "BML/Variables/Vector2Variable", order = 0)]
    public class Vector2Variable : Variable<Vector2>, IFloatValue
    {
        public string GetName() => name;
        public string GetDescription() => Description;
        public float GetFloat() => Value.magnitude;
        // TODO custom property drawer to allow choice of component or magnitude
        float IValue<float>.GetValue(Type type) => GetFloat();
    }
}