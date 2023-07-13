using System;
using BML.ScriptableObjectCore.Scripts.Events;
using TMPro;
using UnityEngine;

namespace BML.Scripts.UI
{
    public class UiSetDynamicText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private DynamicGameEvent _onPrintDialogue;

        private void OnEnable()
        {
            _onPrintDialogue.Subscribe(SetText);
        }

        private void OnDisable()
        {
            _onPrintDialogue.Unsubscribe(SetText);
        }

        public void SetText(object prev, object current)
        {
            string currentString = (string) current;
            _text.text = currentString;
        }
    }
}