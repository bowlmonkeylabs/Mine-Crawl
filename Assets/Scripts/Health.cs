using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class Health : MonoBehaviour
    {
        [SerializeField] [ShowIf("_useHealthVariable")] [LabelText("health")] private IntVariable _healthReference;
        [SerializeField] [HideIf("_useHealthVariable")] private int _health;
        [SerializeField] private bool _useHealthVariable = false;
        [SerializeField] private float _invincibilitySeconds = 0;
        [SerializeField] private bool _isInvincible = false;
        [SerializeField] private UnityEvent<int, int> _onHealthChange;
        [SerializeField] private UnityEvent _onTakeDamage;
        [SerializeField] private UnityEvent _onDeath;
        
        private float lastDamageTime = Mathf.NegativeInfinity;

        private int _value{get => Value; set {
            if(_useHealthVariable) _healthReference.Value = value;
            else _health = value;
        }}

        public int Value {get => _useHealthVariable ? _healthReference.Value : _health;}
        public bool IsDead {get => Value <= 0;}

        public void DecrementHealth(int amount) {
            if (Value <= 0) return;
            
            if (_isInvincible || 
                !Mathf.Approximately(0f, _invincibilitySeconds) && lastDamageTime + _invincibilitySeconds > Time.time)
                return;

            lastDamageTime = Time.time;

            _onHealthChange.Invoke(Value - amount, Value);
            if (amount < 0) _onTakeDamage.Invoke();
            
            _value -= amount;
            if (Value <= 0)
            {
                Death();
            }
        }

        public void IncrementHealth(int amount) {
            this.DecrementHealth(-amount);
        }

        private void Death()
        {
            _onDeath.Invoke();
        }
        
        public void SetInvincible(bool isInvincible)
        {
            _isInvincible = isInvincible;
        }
    }
}
