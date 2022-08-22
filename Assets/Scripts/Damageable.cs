using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    [RequireComponent(typeof(Health))]
    public class Damageable : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private int _critMultiplier = 2;
        [SerializeField] private float _invincibilitySeconds = 0;
        [SerializeField] private bool _isInvincible = false;

        [SerializeField] private UnityEvent _onDamage;
        [SerializeField] private UnityEvent _onCrit;
        [SerializeField] private UnityEvent _onDeath;

        private float lastDamageTime = Mathf.NegativeInfinity;

        protected void TakeDamage(int damage)
        {
            if (_isInvincible || 
                !Mathf.Approximately(0f, _invincibilitySeconds) && lastDamageTime + _invincibilitySeconds > Time.time)
                return;

            lastDamageTime = Time.time;

            _onDamage.Invoke();

            health.DecrementHealth(damage);

            if(health.IsDead) {
                _onDeath.Invoke();
            }
        }

        protected void TakeCritDamage(int damage) {
            _onCrit.Invoke();
            this.TakeDamage(damage * this._critMultiplier);
        }

        public void SetInvincible(bool isInvincible)
        {
            _isInvincible = isInvincible;
        }
    }
}