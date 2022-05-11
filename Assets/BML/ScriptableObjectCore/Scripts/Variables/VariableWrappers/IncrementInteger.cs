using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables.VariableWrappers
{
    public class IncrementInteger : MonoBehaviour
    {
        [SerializeField] [Required] private IntVariable intVariable;

        public void Increment(int inc)
        {
            intVariable.Value += inc;
        }
    }
}

