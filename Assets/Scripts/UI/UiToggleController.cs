using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiToggleController : MonoBehaviour
    {
        [SerializeField] private BoolVariable _variable;
        
        private Toggle toggle;
        
        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            
            UpdateSliderUi();
            _variable.Subscribe(UpdateSliderUi);
            toggle.onValueChanged.AddListener(UpdateStoredValue);
        }

        private void OnDestroy()
        {
            _variable.Unsubscribe(UpdateSliderUi);
            toggle.onValueChanged.RemoveListener(UpdateStoredValue);
        }

        protected void UpdateStoredValue(bool sliderValue)
        {
            _variable.Value = toggle.isOn;
        }

        protected void UpdateSliderUi()
        {
            toggle.isOn = _variable.Value;
        }
    }
}