using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables.VariableWrappers
{
    public class SetActive : MonoBehaviour
    {
        [SerializeField] private BoolReference Enabled;
        [SerializeField] private GameObject[] Targets;

        private void Start()
        {
            Targets.ForEach(t => t.SetActive(Enabled.Value));
            Enabled.Subscribe(() =>
            {
                Targets.ForEach(t => t.SetActive(Enabled.Value));
            });
        }
    }
}