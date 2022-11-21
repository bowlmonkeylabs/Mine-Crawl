using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Helper
{
    public class ApplyScaleFromVariable : MonoBehaviour
    {
        [SerializeField] private SafeFloatValueReference _scaleToApply;
        [SerializeField] private float _multiplier = 1f;
        [SerializeField] private bool _applyOnEnable = true;

        private void OnEnable()
        {
            if (!_applyOnEnable)
                return;

            Apply();
        }

        [Button]
        public void Apply()
        {
            transform.localScale = Vector3.one * _scaleToApply.Value * _multiplier;
        }
    }
}