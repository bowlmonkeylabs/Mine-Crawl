using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables.VariableWrappers
{
    public class SetFloatValue : SerializedMonoBehaviour
    {
        [SerializeField] private FloatValueReference sourceValue;
        [SerializeField] private FloatReference targetValue;

        public void SetFloat()
        {
            targetValue.Value = sourceValue.Value;
        }
    }
}

