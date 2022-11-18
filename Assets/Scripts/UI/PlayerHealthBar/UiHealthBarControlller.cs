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
        public Health Health => _health;

        [SerializeField] private IntReference _healthMaxValue;

        [SerializeField] private float _initAnimateSeconds;

        [SerializeField] private int _lowHealthWarningThreshold = 2;
        public bool IsLowHealth => _health.Value <= _lowHealthWarningThreshold;

        private List<UiHealthHeartController> _heartChildren;

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            _health = _healthTransformSceneReference.Value.GetComponent<Health>();
            _heartChildren = new List<UiHealthHeartController>();
        }

        private void OnEnable()
        {
            FindChildren();
            StartCoroutine(InitChildren());
            _health.OnHealthChange += UpdateHeartsFill;
            _healthMaxValue.Subscribe(OnMaxHealthChanged);
            _health.OnInvincibilityChange += UpdateInvincibility;
        }

        private void OnDisable()
        {
            _health.OnHealthChange -= UpdateHeartsFill;
            _healthMaxValue.Unsubscribe(OnMaxHealthChanged);
            _health.OnInvincibilityChange -= UpdateInvincibility;
        }

        #endregion
        
        #region UI control

        private void FindChildren()
        {
            _heartChildren.Clear();
            for (int i = 0; i < this.transform.childCount; i++)
            {
                var heartController = this.transform.GetChild(i).GetComponent<UiHealthHeartController>();
                if (!heartController.SafeIsUnityNull())
                {
                    heartController.Initialize(this);
                    heartController.gameObject.SetActive(false);
                    _heartChildren.Add(heartController);
                }
            }
        }

        private IEnumerator InitChildren()
        {
            int delta = _health.Value - 0;
            for (int i = 0; i < _heartChildren.Count; i++)
            {
                var heartController = _heartChildren[i];
                int heartHealth = 2 * i;
                
                heartController.gameObject.SetActive(heartHealth < _healthMaxValue.Value);
                heartController.SetValue(_health.Value - heartHealth, delta);
                
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
        } 

        private void OnMaxHealthChanged(int prevMaxHealth, int currentMaxHealth)
        {
            int delta = currentMaxHealth - prevMaxHealth;
            for (int i = 0; i < _heartChildren.Count; i++)
            {
                var heartController = _heartChildren[i];
                int heartHealth = 2 * i;
                
                heartController.gameObject.SetActive(heartHealth < _healthMaxValue.Value);
                heartController.SetValue(_health.Value - heartHealth, delta);

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
        }

        private void UpdateHeartsFill(int prevHealth, int currentHealth)
        {
            int delta = currentHealth - prevHealth;
            for (int i = 0; i < _heartChildren.Count; i++)
            {
                var heartController = _heartChildren[i];
                int heartHealth = 2 * i;
                
                heartController.SetValue(currentHealth - heartHealth, delta);
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