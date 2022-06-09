using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.UI
{
    public class UiTextTimerFormatter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _formatString = "D2";
        [SerializeField] private TimerReference _variable;
        [FormerlySerializedAs("_timerValue")] [SerializeField] private TimerDisplayMode _timerDisplayMode;

        private enum TimerDisplayMode
        {
            ElapsedTime,
            RemainingTime,
        }

        private void Awake()
        {
            UpdateText();
            _variable.Subscribe(UpdateText);
        }

        private void Start()
        {
            UpdateText();
        }

        private void OnDestroy()
        {
            _variable.Unsubscribe(UpdateText);
        }

        protected string GetFormattedValue()
        {
            float timerValue = 0;
            switch (_timerDisplayMode)
            {
                case TimerDisplayMode.ElapsedTime:
                    timerValue = _variable.ElapsedTime;
                    break;
                case TimerDisplayMode.RemainingTime:
                    timerValue = _variable.RemainingTime ?? 0;
                    break;
            }
            return FormatTime(timerValue);
        }

        private string FormatTime(float seconds)
        {
            int minutes = (int) seconds / 60;
            seconds %= 60;
            return $"{minutes.ToString(_formatString)}:{seconds.ToString(_formatString)}";
        }

        protected void UpdateText()
        {
            _text.text = GetFormattedValue();
        }
    }
}