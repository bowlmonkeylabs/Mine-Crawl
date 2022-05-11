using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables.VariableWrappers
{
    public class SetIntValue : MonoBehaviour
    {
        [SerializeField] private IntReference sourceValue;
        [SerializeField] private IntReference targetValue;

        public void SetInt()
        {
            targetValue.Value = sourceValue.Value;
        }
    }
}

