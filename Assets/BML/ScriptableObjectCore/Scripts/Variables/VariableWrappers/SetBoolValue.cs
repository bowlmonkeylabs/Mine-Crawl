using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables.VariableWrappers
{
    public class SetBoolValue : MonoBehaviour
    {
        [SerializeField] private BoolReference sourceValue;
        [SerializeField] private BoolReference targetValue;

        public void SetTargetValue()
        {
            targetValue.Value = sourceValue.Value;
        }
    }
}

