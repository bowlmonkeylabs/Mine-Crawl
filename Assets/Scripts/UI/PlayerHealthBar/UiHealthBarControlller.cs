using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.UI.PlayerHealthBar
{
    public class UiHealthBarControlller : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private TransformSceneReference _damageableTransformSceneReference;
        [ShowInInspector, ReadOnly] private Health _health;
        [ShowInInspector, ReadOnly] private Health _healthTemporary;
        public Health Health => _health;
        public Health HealthTemporary => _healthTemporary;

        [Tooltip("Health stat, Number of red hearts (empty or full) player has")]
        [SerializeField] private IntReference _healthMaxValue;
        [Tooltip("Max possible health the player can get")]
        [SerializeField] private IntReference _healthMaxPossibleValue;
        [Tooltip("Player's current total health (filled red hearts + temp hearts")]
        [SerializeField] private FunctionVariable _totalHealth;

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
            Damageable damageable = _damageableTransformSceneReference.Value.GetComponent<Damageable>();
            _health = damageable.Health;
            _healthTemporary = damageable.HealthTemporary;
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
            _totalHealth.Subscribe(OnTotalHealthChanged);
        }

        private void OnDisable()
        {
            _health.OnHealthChange -= UpdateHeartsFill;
            _healthTemporary.OnHealthChange -= UpdateHeartsTemporaryFill;
            _healthMaxValue.Unsubscribe(OnMaxHealthChanged);
            _health.OnInvincibilityChange -= UpdateInvincibility;
            _totalHealth.Unsubscribe(OnTotalHealthChanged);
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
                bool isTemporaryHealth = i >= _lastHeartIndex;
                if (!isTemporaryHealth)
                {
                    heartHealth = 2 * i;
                
                    heartController.gameObject.SetActive(heartHealth < _healthMaxValue.Value);
                    heartController.SetValue(_health.Value - heartHealth, 0,
                        false);
                }
                else
                {
                    heartHealth = 2 * i - _healthMaxValue.Value;
                    heartController.gameObject.SetActive(heartHealth < _healthTemporary.Value);
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

            // Truncate temp hearts past the max possible hearts
            var maxPossibleTempHearts = _healthMaxPossibleValue.Value - _healthMaxValue.Value;
            _healthTemporary.SetHealth(Mathf.Max(0, 
                Mathf.Min(_healthTemporary.Value, maxPossibleTempHearts)));
        }

        private void OnTotalHealthChanged(float prevTotalHealth, float currentTotalHealth)
        {
            foreach (var heartController in _heartChildren)
            {
                heartController.OnTotalHealthChange(prevTotalHealth, currentTotalHealth);
            }
        }

        private void UpdateHeartsFill(int prevHealth, int currentHealth)
        {
            int delta = currentHealth - prevHealth;
            for (int i = 0; i < _lastHeartIndex; i++)
            {
                var heartController = _heartChildren[i];
                int heartHealth = 2 * i;
                var heartValue = currentHealth - heartHealth;
                
                heartController.gameObject.SetActive(heartHealth < _healthMaxValue.Value + _healthTemporary.Value);
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