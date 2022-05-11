using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    [RequireComponent(typeof(Slider))]
    public class UiSliderController : MonoBehaviour
    {
        [SerializeField] private FloatVariable _variable;
        
        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            
            UpdateSliderUi();
            _variable.Subscribe(UpdateSliderUi);
            _slider.onValueChanged.AddListener(UpdateStoredValue);
        }

        private void OnDestroy()
        {
            _variable.Unsubscribe(UpdateSliderUi);
            _slider.onValueChanged.RemoveListener(UpdateStoredValue);
        }

        protected void UpdateStoredValue(float sliderValue)
        {
            _variable.Value = _slider.value;
        }

        protected void UpdateSliderUi()
        {
            _slider.value = _variable.Value;
        }
    }
}