using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace BML.Scripts.UI
{
    public class UiPlayerExperienceBarController : MonoBehaviour
    {
        [SerializeField] private Image _barImage;
        [SerializeField] private IntVariable _currentExperience;
        [SerializeField] private IntVariable _currentLevel;
        [SerializeField] private EvaluateCurveVariable _experienceEvalCurve;

        private void Awake()
        {
            UpdateBar();
            _currentExperience.Subscribe(UpdateBar);
            _experienceEvalCurve.Subscribe(UpdateBar);
        }

        private void OnDestroy()
        {
            _currentExperience.Unsubscribe(UpdateBar);
            _experienceEvalCurve.Unsubscribe(UpdateBar);
        }

        protected void UpdateBar()
        {
            float previousExperienceRequirement = _currentLevel.Value > 0 ? _experienceEvalCurve.EvaluateCurve(_experienceEvalCurve.GetTimeValue() - 1) : 0;
            _barImage.fillAmount = (float)(_currentExperience.Value - previousExperienceRequirement)/(float)(_experienceEvalCurve.Value - previousExperienceRequirement);
        }
    }
}