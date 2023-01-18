using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private IntVariable _health;
        
        private Slider _slider;

        #region Unity Lifecycle

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }
        
        private void Start()
        {
            InitSlider();
            UpdateSlider(_health.Value, _health.Value);
            _health.Subscribe(UpdateSlider);
        }

        private void OnDestroy()
        {
            _health.Unsubscribe(UpdateSlider);
        }

        #endregion

        private void InitSlider()
        {
            _slider.minValue = 0;
            _slider.maxValue = _health.Value;
        }

        private void UpdateSlider(int prev, int current)
        {
            _slider.value = current;
        }
    }
}