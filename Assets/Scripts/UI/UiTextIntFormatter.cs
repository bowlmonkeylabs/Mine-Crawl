using System;
using System.Text;
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
            
            
            return _variable.Value.ToString(asciiString);
        }

        protected void UpdateText()
        {
            _text.text = GetFormattedValue();
        }

        public void SetVariable(IntVariable intVariable)
        {
            _variable = intVariable;
        }
        
    }
}