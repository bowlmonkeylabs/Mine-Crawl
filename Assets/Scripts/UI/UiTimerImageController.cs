using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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
        [SerializeField] private bool _showWhenTimerIsInactive = true;

        public enum DisplayMode
        {
            TimeRemaining,
            TimeElapsed,
        }

        [SerializeField] private DisplayMode _displayMode;

        #endregion

        #region Public interface

        public TimerReference Timer => _timer;

        public void SetTimerVariable(TimerVariable timerVariable)
        {
            _timer.Unsubscribe(UpdateImage);
            _timer.UnsubscribeFinished(UpdateImage);

            _timer.SetVariable(timerVariable);

            _timer.Subscribe(UpdateImage);
            _timer.SubscribeFinished(UpdateImage);
            UpdateImage();
        }

        #endregion

        #region Unity lifecycle

        private void Start()
        {
            UpdateImage();
        }

        private void OnEnable()
        {
            UpdateImage();
            _timer.Subscribe(UpdateImage);
            _timer.SubscribeFinished(UpdateImage);
        }

        private void OnDisable()
        {
            _timer.Unsubscribe(UpdateImage);
            _timer.UnsubscribeFinished(UpdateImage);
        }

        #endregion

        private void UpdateImage()
        {
            if (this.SafeIsUnityNull() || !this.gameObject.activeInHierarchy)
            {
                return;
            }
            
            float fillPercent = 0;
            var timerInactive = (!_timer.IsStarted || _timer.IsFinished);
            switch (_displayMode)
            {
                case DisplayMode.TimeRemaining:
                    fillPercent = (_timer.RemainingTime ?? _timer.Duration) / _timer.Duration;
                    break;
                case DisplayMode.TimeElapsed:
                    
                    fillPercent = (timerInactive ? 1 : _timer.ElapsedTime / _timer.Duration);
                    break;
            }

            _image.fillAmount = fillPercent * _maxFill;
            _image.enabled = true;
            // _image.enabled = !timerInactive || _showWhenTimerIsInactive;
        }
        
    }
}