using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.UI.PlayerHealthBar
{
    public class UiHealthBarControlller : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private TransformSceneReference _healthTransformSceneReference;
        [ShowInInspector, ReadOnly] private Health _health;
        [ShowInInspector, ReadOnly] private HealthTemporary _healthTemporary;
        public Health Health => _health;
        public HealthTemporary HealthTemporary => _healthTemporary;

        [SerializeField] private IntReference _healthMaxValue;
        [SerializeField] private IntReference _healthMaxPossibleValue;
        [SerializeField] private IntReference _healthTempValue;

        [SerializeField] private float _initAnimateSeconds;

        [SerializeField] private int _lowHealthWarningThreshold = 2;
        public bool IsLowHealth => _health.Value + _healthTemporary.Value <= _lowHealthWarningThreshold;
        
        // Index of the last permanent heart as child of healthbar
        private int _lastHeartIndex => Mathf.Min(_heartChildren.Count, Mathf.CeilToInt(_healthMaxValue.Value/2f));

        private List<UiHealthHeartController> _heartChildren;
        
        private Coroutine _coroutineInitChildren;
        private bool _finishedInitChildren = false;

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            _finishedInitChildren = false;
            _health = _healthTransformSceneReference.Value.GetComponent<Health>();
            _healthTemporary = _healthTransformSceneReference.Value.GetComponent<HealthTemporary>();
            _heartChildren = new List<UiHealthHeartController>();
        }

        private void OnEnable()
        {
            if (_coroutineInitChildren != null)
            {
                StopCoroutine(_coroutineInitChildren);
                _coroutineInitChildren = null;
            }
            FindChildren();
            _coroutineInitChildren = StartCoroutine(InitChildren());
            
            _health.OnHealthChange += UpdateHeartsFill;
            _healthTemporary.OnHealthChange += UpdateHeartsTemporaryFill;
            _healthMaxValue.Subscribe(OnMaxHealthChanged);
            _health.OnInvincibilityChange += UpdateInvincibility;
        }

        private void OnDisable()
        {
            _health.OnHealthChange -= UpdateHeartsFill;
            _healthTemporary.OnHealthChange -= UpdateHeartsTemporaryFill;
            _healthMaxValue.Unsubscribe(OnMaxHealthChanged);
            _health.OnInvincibilityChange -= UpdateInvincibility;
        }

        #endregion
        
        #region UI control

        private void FindChildren()
        {
            _heartChildren.Clear();
            _heartChildren.AddRange(
                this.GetComponentsInChildren<UiHealthHeartController>(true)
            );
            foreach (var heartController in _heartChildren)
            {
                heartController.Initialize(this);
                heartController.gameObject.SetActive(false);
            }
        }
        
        private IEnumerator InitChildren()
        {
            for (int i = 0; i < _heartChildren.Count; i++)
            {
                var heartController = _heartChildren[i];
                int heartHealth;
                if (i < _lastHeartIndex)
                {
                    heartHealth = 2 * i;
                
                    heartController.gameObject.SetActive(heartHealth < _healthMaxValue.Value);
                    heartController.SetValue(_health.Value - heartHealth, 0,
                        false);
                }
                else
                {
                    heartHealth = 2 * i - _healthMaxValue.Value;
                    heartController.gameObject.SetActive(heartHealth < _healthTempValue.Value);
                    heartController.SetValue(_healthTemporary.Value - heartHealth, 0,
                        true);
                }
                
                if (_health.IsInvincible)
                {
                    heartController.SetInvincible(false);
                    heartController.SetInvincible(true);
                }
                else
                {
                    heartController.SetInvincible(false);
                }
            
                yield return new WaitForSeconds(_initAnimateSeconds);
            }
            
            _finishedInitChildren = true;
            _coroutineInitChildren = null;
            yield return null;
        } 

        private void OnMaxHealthChanged(int prevMaxHealth, int currentMaxHealth)
        {
            if (!_finishedInitChildren) return;
            
            int delta = currentMaxHealth - prevMaxHealth;
            for (int i = 0; i < _heartChildren.Count; i++)
            {
                var heartController = _heartChildren[i];
                int heartHealth = 2 * i;
                
                heartController.gameObject.SetActive(heartHealth < _healthMaxValue.Value);
                heartController.SetValue(_health.Value - heartHealth, delta, false);

                if (_health.IsInvincible)
                {
                    heartController.SetInvincible(false);
                    heartController.SetInvincible(true);
                }
                else
                {
                    heartController.SetInvincible(false);
                }
            }

            // Reduce temp hearts if truncated
            var maxPossibleTempHearts = _healthMaxPossibleValue.Value - _healthMaxValue.Value;
            _healthTempValue.Value = Mathf.Max(0, 
                Mathf.Min(_healthTempValue.Value, maxPossibleTempHearts));
        }

        private void UpdateHeartsFill(int prevHealth, int currentHealth)
        {
            int delta = currentHealth - prevHealth;
            for (int i = 0; i < _lastHeartIndex; i++)
            {
                var heartController = _heartChildren[i];
                int heartHealth = 2 * i;
                var heartValue = currentHealth - heartHealth;
                
                heartController.gameObject.SetActive(heartHealth < _healthMaxValue.Value + _healthTempValue.Value);
                heartController.SetValue(heartValue, delta,
                    false);
            }
        }
        
        private void UpdateHeartsTemporaryFill(int prevHealth, int currentHealth)
        {
            int delta = currentHealth - prevHealth;
            for (int i = _lastHeartIndex; i < _heartChildren.Count; i++)
            {
                var heartController = _heartChildren[i];
                int heartHealth = 2 * i - _healthMaxValue.Value;
                var heartValue = Mathf.Max(0, currentHealth - heartHealth);
                
                heartController.gameObject.SetActive(heartValue > 0);
                heartController.SetValue(heartValue, delta,
                    true);
                
            }
        }

        private void UpdateInvincibility(bool isInvincible)
        {
            for (int i = 0; i < _heartChildren.Count; i++)
            {
                var heartController = _heartChildren[i];
                
                heartController.SetInvincible(isInvincible);
            }
        }

        #endregion
    }
}