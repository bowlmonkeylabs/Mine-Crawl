using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiTimerImageController : MonoBehaviour
    {
        #region Inspector

        [Required, SerializeField] private TimerReference _timer;
        [Required, SerializeField] private Image _image;
        [SerializeField, Range(0f, 1f)] private float _maxFill = 1f;

        public enum DisplayMode
        {
            TimeRemaining,
            TimeElapsed,
        }

        [SerializeField] private DisplayMode _displayMode;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _timer.Subscribe(UpdateImage);
        }

        private void OnDisable()
        {
            _timer.Unsubscribe(UpdateImage);
        }

        #endregion

        private void UpdateImage()
        {
            float fillPercent = 0;
            switch (_displayMode)
            {
                case DisplayMode.TimeRemaining:
                    fillPercent = (_timer.RemainingTime ?? _timer.Duration) / _timer.Duration;
                    break;
                case DisplayMode.TimeElapsed:
                    var timerInactive = (!_timer.IsStarted || _timer.IsFinished);
                    fillPercent = (timerInactive ? 1 : _timer.ElapsedTime / _timer.Duration);
                    break;
            }
            _image.fillAmount = fillPercent * _maxFill;
        }
        
    }
}