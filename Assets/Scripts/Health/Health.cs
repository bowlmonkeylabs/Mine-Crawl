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
        [SerializeField, ShowIf("_useHealthVariable")] [LabelText("Health")] private IntVariable _healthReference;
        [SerializeField, ShowIf("_useHealthVariable")] [LabelText("Health Temporary")] private IntVariable _tempHealthReference;
        [SerializeField, HideIf("_useHealthVariable")] private int _health;
        [SerializeField, HideIf("_useHealthVariable")] private int _tempHealth;

        [SerializeField] private bool _hasMaxHealth = false;
        [SerializeField, ShowIf("_hasMaxHealth")] private IntReference _maxHealthReference;
        [SerializeField, ShowIf("_hasMaxHealth")] private IntReference _maxHealthPossibleReference;

        [SerializeField] private bool _isInvincible = false;
        [ShowInInspector, ReadOnly] private bool _isInvincibleFrames = false;
        [SerializeField, ShowIf("_isInvincible")] private SafeFloatValueReference _invincibilitySeconds;
        
        [SerializeField] private UnityEvent<int, int> _onHealthChange;
        [SerializeField] private UnityEvent<HitInfo> _onTakeDamageHitInfo;
        [SerializeField] private UnityEvent _onTakeDamage;
        [SerializeField] private UnityEvent<HitInfo> _onDeathHitInfo;
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
                if (_useHealthVariable)
                {
                    value = Mathf.Min(_maxHealthPossible, value);
                    var aboveMaxHealth = Mathf.Max(0, value - _maxHealth);
                    _tempHealthReference.Value = aboveMaxHealth;
                    _healthReference.Value = Mathf.Min(value, _maxHealth);
                }
                else
                {
                    value = Mathf.Min(_maxHealthPossible, value);
                    var aboveMaxHealth = Mathf.Max(0, value - _maxHealth);
                    _tempHealth = aboveMaxHealth;
                    _health = Mathf.Min(value, _maxHealth);
                }
            }}
        
        private int _maxHealth => _hasMaxHealth ? _maxHealthReference.Value : 999;
        private int _maxHealthPossible => _hasMaxHealth ? _maxHealthPossibleReference.Value : 999;
        
        public int Value {get => _useHealthVariable ? _healthReference.Value + _tempHealthReference.Value : _health + _tempHealth;}
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
            if (IsDead || IsInvincible || amount == 0) return false;
            
            lastDamageTime = Time.time;
            StartCoroutine(InvincibilityTimer());

            return this.SetHealth(Value - amount, null) < 0;
        }
        
        public bool Damage(HitInfo hitInfo)
        {
            if (IsDead || IsInvincible || hitInfo.Damage == 0) return false;
            
            lastDamageTime = Time.time;
            StartCoroutine(InvincibilityTimer());

            return this.SetHealth(Value - hitInfo.Damage, hitInfo) < 0;
        }
        
        public int SetHealth(int newValue, HitInfo hitInfo = null)
        {
            // Comment out to allow health to go above max in form of temp hearts
            //newValue = Mathf.Clamp(newValue, 0, _maxHealth);

            var oldValue = Value;
            _value = newValue;
            var delta = Value - oldValue;
            
            _onHealthChange?.Invoke(oldValue, Value);
            OnHealthChange?.Invoke(oldValue, Value);
            if (delta < 0)
            {
                _onTakeDamageHitInfo?.Invoke(hitInfo);
                _onTakeDamage?.Invoke();
            }
            
            if (Value <= 0)
            {
                Death(hitInfo);
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

        public void Kill()
        {
            SetHealth(0);
        }
        
        public void SetInvincible(bool isInvincible)
        {
            _isInvincible = isInvincible;
            OnInvincibilityChange?.Invoke(IsInvincible);
        }
        
        #endregion
        
        private void Death(HitInfo hitInfo)
        {
            _onDeathHitInfo.Invoke(hitInfo);
            _onDeath.Invoke();
            OnDeath?.Invoke();
        }

        private void OnHealthReferenceUpdated(int previousValue, int currentValue)
        {
            int delta = currentValue - previousValue;
            if (delta < 0)
            {
                _onTakeDamageHitInfo?.Invoke(null);
                _onTakeDamage?.Invoke();
            }
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
