using System;
using System.Text;
using BML.ScriptableObjectCore.Scripts.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.UI
{
    public class UiTextIntFormatter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _formatString = "P0";
        [FormerlySerializedAs("_variable")] [SerializeField] private IntReference _value;

        private void Awake()
        {
            UpdateText();
            _value.Subscribe(UpdateText);
        }

        private void OnDestroy()
        {
            _value.Unsubscribe(UpdateText);
        }

        protected string GetFormattedValue()
        {
            // Create two different encodings.
            Encoding ascii = Encoding.UTF8;
            Encoding unicode = Encoding.Unicode;

            // Convert the string into a byte[].
            byte[] unicodeBytes = unicode.GetBytes(_formatString);

            // Perform the conversion from one encoding to the other.
            byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);

            // Convert the new byte[] into a char[] and then into a string.
            // This is a slightly different approach to converting to illustrate
            // the use of GetCharCount/GetChars.
            char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            string asciiString = new string(asciiChars);
            
            
            return _value.Value.ToString(asciiString);
        }

        protected void UpdateText()
        {
            _text.text = GetFormattedValue();
        }

        public void SetVariable(IntVariable intVariable)
        {
            if (_value != null)
            {
                _value.Unsubscribe(UpdateText);
            }
            _value.SetVariable(intVariable);
            if (_value != null)
            {
                _value.Subscribe(UpdateText);
            }
        }

        /// <summary>
        /// Set a constant value to override the mapped variable.
        /// </summary>
        public void SetConstant(int value)
        {
            _value.SetConstant(value);
            UpdateText();
        }
        
    }
}