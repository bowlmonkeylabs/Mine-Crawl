using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class Health : MonoBehaviour
    {

        #region Inspector
        [SerializeField] private bool _useHealthVariable = false;
        [SerializeField, ShowIf("_useHealthVariable")] [LabelText("health")] private IntVariable _healthReference;
        [SerializeField, HideIf("_useHealthVariable")] private int _health;

        [SerializeField] private bool _hasMaxHealth = false;
        [SerializeField, ShowIf("_hasMaxHealth")] private IntReference _maxHealthReference;

        [SerializeField] private bool _isInvincible = false;
        [ShowInInspector, ReadOnly] private bool _isInvincibleFrames = false;
        [SerializeField, ShowIf("_isInvincible")] private SafeFloatValueReference _invincibilitySeconds;
        
        [SerializeField] private UnityEvent<int, int> _onHealthChange;
        [SerializeField] private UnityEvent _onTakeDamage;
        [SerializeField] private UnityEvent _onDeath;
        [SerializeField] private UnityEvent _onRevive;
        #endregion
        
        #region Events

        public delegate void _HealthChange(int prev, int current);
        public delegate void _Death();
        public delegate void _Revive();
        public delegate void _InvincibilityChange(bool isInvincible);

        public _HealthChange OnHealthChange;
        public _Death OnDeath;
        public _Revive OnRevive;
        public _InvincibilityChange OnInvincibilityChange;

        #endregion
        
        private float lastDamageTime = Mathf.NegativeInfinity;
        private int startingHealth;

        #region Properties

        private int _value{
            get => Value;
            set {
                if(_useHealthVariable) _healthReference.Value = value;
                else _health = value;
            }}

        public int Value {get => _useHealthVariable ? _healthReference.Value : _health;}
        public bool IsDead {get => Value <= 0;}
        public bool IsInvincible => _isInvincible || _isInvincibleFrames;

        public int StartingHealth => startingHealth;

        #endregion
        
        #region Unity Lifecycle

        private void Start()
        {
            startingHealth = _value;
        }

        private void OnEnable()
        {
            if (_useHealthVariable) _healthReference.Subscribe(OnHealthReferenceUpdated);
        }

        private void OnDisable()
        {
            if (_useHealthVariable) _healthReference.Unsubscribe(OnHealthReferenceUpdated);
        }

        #endregion

        #region Public interface

        public bool Heal(int amount) 
        {
            return this.SetHealth(Value + amount) > 0;
        }

        private IEnumerator InvincibilityTimer()
        {
            _isInvincibleFrames = true;
            OnInvincibilityChange?.Invoke(IsInvincible);

            yield return new WaitForSeconds(_invincibilitySeconds.Value);
            // if (
            //     !Mathf.Approximately(0f, _invincibilitySeconds.Value) 
            //     && lastDamageTime + _invincibilitySeconds.Value > Time.time
            // ) {
            //     yield return null;
            // }
            
            _isInvincibleFrames = false;
            OnInvincibilityChange?.Invoke(IsInvincible);
        }
        
        public bool Damage(int amount) 
        {
            if (IsDead || IsInvincible) return false;
            
            lastDamageTime = Time.time;
            StartCoroutine(InvincibilityTimer());

            return this.SetHealth(Value - amount) < 0;
        }
        
        public int SetHealth(int newValue)
        {
            newValue = Mathf.Clamp(newValue, 0, _hasMaxHealth ? _maxHealthReference.Value : 999);

            var oldValue = Value;
            _value = newValue;
            var delta = Value - oldValue;
            
            _onHealthChange?.Invoke(oldValue, Value);
            OnHealthChange?.Invoke(oldValue, Value);
            if (delta < 0) _onTakeDamage?.Invoke();
            
            if (Value <= 0)
            {
                Death();
            }

            return delta;
        }

        public void Revive()
        {
            _onHealthChange?.Invoke(Value, startingHealth);
            OnHealthChange?.Invoke(Value, startingHealth);
            _value = startingHealth;
            _onRevive.Invoke();
            OnRevive?.Invoke();
        }
        
        public void SetInvincible(bool isInvincible)
        {
            _isInvincible = isInvincible;
            OnInvincibilityChange?.Invoke(IsInvincible);
        }
        
        #endregion
        
        private void Death()
        {
            _onDeath.Invoke();
            OnDeath?.Invoke();
        }

        private void OnHealthReferenceUpdated(int previousValue, int currentValue)
        {
            int delta = currentValue - previousValue;
            if (delta < 0) _onTakeDamage?.Invoke();
            if (previousValue <= 0 && currentValue > 0)
            {
                _onRevive?.Invoke();
                OnRevive?.Invoke();
            }
            _onHealthChange.Invoke(previousValue, currentValue);
            OnHealthChange?.Invoke(previousValue, currentValue);
        }
    }
}
