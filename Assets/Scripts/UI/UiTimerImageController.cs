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
            var fillPercent = (_timer.RemainingTime ?? _timer.Duration) / _timer.Duration;
            _image.fillAmount = fillPercent;
        }
        
    }
}