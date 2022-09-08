using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class Health : MonoBehaviour
    {

        #region Inspector
        [SerializeField] [ShowIf("_useHealthVariable")] [LabelText("health")] private IntVariable _healthReference;
        [SerializeField] [HideIf("_useHealthVariable")] private int _health;
        [SerializeField] private bool _useHealthVariable = false;
        [SerializeField] private float _invincibilitySeconds = 0;
        [SerializeField] private bool _isInvincible = false;
        [SerializeField] private UnityEvent<int, int> _onHealthChange;
        [SerializeField] private UnityEvent _onTakeDamage;
        [SerializeField] private UnityEvent _onDeath;
        [SerializeField] private UnityEvent _onRevive;
        #endregion
        
        #region Events

        public delegate void _Death();
        public delegate void _Revive();

        public _Death OnDeath;
        public _Revive OnRevive;

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

        #endregion
        
        #region Unity Lifecycle

        private void Start()
        {
            startingHealth = _value;
        }

        #endregion

        public bool DecrementHealth(int amount) {
            if (IsDead) return false;
            
            if (_isInvincible || 
                !Mathf.Approximately(0f, _invincibilitySeconds) && lastDamageTime + _invincibilitySeconds > Time.time)
                return false;

            lastDamageTime = Time.time;

            _onHealthChange.Invoke(Value - amount, Value);
            if (amount < 0) _onTakeDamage.Invoke();
            
            _value -= amount;
            if (Value <= 0)
            {
                Death();
            }

            return true;
        }

        public void IncrementHealth(int amount) {
            this.DecrementHealth(-amount);
        }

        private void Death()
        {
            _onDeath.Invoke();
            OnDeath?.Invoke();
        }

        public void Revive()
        {
            _onHealthChange.Invoke(Value, startingHealth);
            _value = startingHealth;
            _onRevive.Invoke();
            OnRevive?.Invoke();
        }
        
        public void SetInvincible(bool isInvincible)
        {
            _isInvincible = isInvincible;
        }
    }
}
