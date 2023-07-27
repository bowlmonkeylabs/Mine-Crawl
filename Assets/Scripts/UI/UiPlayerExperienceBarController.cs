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
        [SerializeField] private FloatReference _requiredExperience;
        [SerializeField] private FloatReference _previousRequiredExperience;
        [SerializeField] private EvaluateCurveVariable _experienceEvalCurve;


        private void OnEnable()
        {
            UpdateBar();
            _currentExperience.Subscribe(UpdateBar);
            _experienceEvalCurve.Subscribe(UpdateBar);
        }

        private void OnDisable()
        {
            _currentExperience.Unsubscribe(UpdateBar);
            _experienceEvalCurve.Unsubscribe(UpdateBar);
        }

        protected void UpdateBar()
        {
            Debug.Log($"prev: {_previousRequiredExperience.Value} | curr: {_requiredExperience.Value}");
            _barImage.fillAmount = (float)(_currentExperience.Value - _previousRequiredExperience.Value)/(float)(_requiredExperience.Value - _previousRequiredExperience.Value);
        }

    }
}