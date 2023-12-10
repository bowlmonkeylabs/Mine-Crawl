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
        [SerializeField, HideIf("_useHealthVariable")] private int _health;
        
        [SerializeField] private bool _hasMaxHealth = false;
        [SerializeField, ShowIf("_hasMaxHealth")] private SafeIntValueReference _maxHealthReference;

        [SerializeField] private bool _isInvincible = false;
        [SerializeField] private bool _useInvincibilityVariable;
        [SerializeField, ShowIf("_useInvincibilityVariable")] private TimerReference _invincibilityTimer;
        [SerializeField, HideIf("_useInvincibilityVariable")] private float _invincibilitySeconds;
        [ShowInInspector, ReadOnly] private bool _isInvincibleFrames = false;
        
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
                    value = Mathf.Min(MaxHealth, value);
                    _healthReference.Value = Mathf.Min(value, MaxHealth);
                }
                else
                {
                    value = Mathf.Min(MaxHealth, value);
                    _health = Mathf.Min(value, MaxHealth);
                }
            }}
        
        public int Value {get => _useHealthVariable ? _healthReference.Value: _health;}
        public bool IsDead {get => Value <= 0;}
        public bool IsInvincible => _isInvincible || _isInvincibleFrames;

        public int StartingHealth => startingHealth;

        public int MaxHealth => _hasMaxHealth ? _maxHealthReference.Value : 999;

        #endregion
        
        #region Unity Lifecycle

        private void Start()
        {
            startingHealth = _value;
        }

        private void Update()
        {
            if (_useInvincibilityVariable)
            {
                _invincibilityTimer.UpdateTime();
                //SetInvincibleFrames(_invincibilityTimer.IsStarted && !_invincibilityTimer.IsFinished);
            }
        }

        private void OnEnable()
        {
            
            {
                if (_useHealthVariable) _healthReference.Subscribe(OnHealthReferenceUpdated);
                if (_useInvincibilityVariable)
                {
                    _invincibilityTimer.SubscribeStarted(EnableInvincibleFrames);
                    _invincibilityTimer.SubscribeFinished(DisableInvincibleFrames);
                }
            }
        }

        private void OnDisable()
        {
            if (_useHealthVariable) _healthReference.Unsubscribe(OnHealthReferenceUpdated);
            if (_useInvincibilityVariable)
            {
                _invincibilityTimer.UnsubscribeStarted(EnableInvincibleFrames);
                _invincibilityTimer.UnsubscribeFinished(DisableInvincibleFrames);
            }
        }

        #endregion

        #region Public interface

        public bool Heal(int amount) 
        {
            return this.SetHealth(Value + amount) > 0;
        }

        private IEnumerator InvincibilityTimer()
        {
            SetInvincibleFrames(true);

            yield return new WaitForSeconds(_invincibilitySeconds);
            // if (
            //     !Mathf.Approximately(0f, _invincibilitySeconds.Value) 
            //     && lastDamageTime + _invincibilitySeconds.Value > Time.time
            // ) {
            //     yield return null;
            // }
            
            SetInvincibleFrames(false);
        }

        public bool Damage(int amount, bool ignoreInvincibility)
        {
            if (IsDead || (IsInvincible && !ignoreInvincibility) || amount == 0) return false;
            
            lastDamageTime = Time.time;

            if (_useInvincibilityVariable)
            {
                // Starting this timer triggers OnStarted event which enabled invincibility
                _invincibilityTimer.RestartTimer();
            }
            else
                StartCoroutine(InvincibilityTimer());
            
            return this.SetHealth(Value - amount, null) < 0;
        }
        
        public bool Damage(int amount)
        {
            return Damage(amount, false);
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
            newValue = Mathf.Clamp(newValue, 0, MaxHealth);

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
            var reviveHealth = (_hasMaxHealth ? MaxHealth : startingHealth);
            _onHealthChange?.Invoke(Value, reviveHealth);
            OnHealthChange?.Invoke(Value, reviveHealth);
            _value = reviveHealth;
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
        private void EnableInvincibleFrames()
        {
            SetInvincibleFrames(true);
        }
        
        private void DisableInvincibleFrames()
        {
            SetInvincibleFrames(false);
        }

        private void SetInvincibleFrames(bool isInvincible)
        {
            _isInvincibleFrames = isInvincible;
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
