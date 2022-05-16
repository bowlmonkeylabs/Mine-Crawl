using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using TMPro;
using UnityEngine;

namespace BML.Scripts.UI
{
    public class UiTextIntFormatter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _formatString = "P0";
        [SerializeField] private IntVariable _variable;

        private void Awake()
        {
            UpdateText();
            _variable.Subscribe(UpdateText);
        }

        private void OnDestroy()
        {
            _variable.Unsubscribe(UpdateText);
        }

        protected string GetFormattedValue()
        {
            return _variable.Value.ToString(_formatString);
        }

        protected void UpdateText()
        {
            _text.text = GetFormattedValue();
        }
    }
}