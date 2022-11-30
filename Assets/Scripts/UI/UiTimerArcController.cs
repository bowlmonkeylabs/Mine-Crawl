using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using ThisOtherThing.UI.Shapes;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiTimerArcController : MonoBehaviour
    {
        #region Inspector

        [Required, SerializeField] private TimerReference _timer;
        [Required, SerializeField] private Arc _arc;
        [Required, SerializeField] private Color _colorRunning;
        [Required, SerializeField] private Color _colorFinished;
        [SerializeField, Range(0f, 1f)] private float _maxFill = 1f;

        public enum DisplayMode
        {
            TimeRemaining,
            TimeElapsed,
        }

        [SerializeField] private DisplayMode _displayMode;

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
            _arc.ArcProperties.Length = fillPercent * _maxFill;
            bool isFull = (_timer.RemainingTime <= 0 || !_timer.IsStarted);
            _arc.ShapeProperties.FillColor = (isFull ? _colorFinished : _colorRunning);
            _arc.ForceMeshUpdate();
        }
        
    }
}