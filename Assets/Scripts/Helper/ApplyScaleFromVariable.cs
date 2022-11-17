using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Helper
{
    public class ApplyScaleFromVariable : MonoBehaviour
    {
        [SerializeField] private FloatVariable _scaleToApply;
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
            transform.localScale = Vector3.one * _scaleToApply.Value;
        }
    }
}