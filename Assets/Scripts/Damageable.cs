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

        [SerializeField] private UnityEvent<HitInfo> _onDamage;
        [SerializeField] private UnityEvent<HitInfo> _onCrit;
        [SerializeField] private UnityEvent _onDeath;

        private float lastDamageTime = Mathf.NegativeInfinity;

        public void TakeDamage(HitInfo hitInfo)
        {
            if (_isInvincible || 
                !Mathf.Approximately(0f, _invincibilitySeconds) && lastDamageTime + _invincibilitySeconds > Time.time)
                return;

            lastDamageTime = Time.time;

            _onDamage.Invoke(hitInfo);

            health.DecrementHealth(hitInfo.Damage);

            if(health.IsDead) {
                _onDeath.Invoke();
            }
        }

        public void TakeCritDamage(HitInfo hitInfo) {
            hitInfo.Damage *= this._critMultiplier;
            _onCrit.Invoke(hitInfo);
            this.TakeDamage(hitInfo);
        }

        public void SetInvincible(bool isInvincible)
        {
            _isInvincible = isInvincible;
        }
    }
}