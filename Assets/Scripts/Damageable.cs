using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class Damageable : MonoBehaviour
    {
        [SerializeField] [ShowIf("_useHealthVariable")] [LabelText("health")] private IntVariable _healthReference;
        [SerializeField] [HideIf("_useHealthVariable")] private int _health;
        [SerializeField] private bool _useHealthVariable = false;

        [SerializeField] private int _critMultiplier = 2;
        [SerializeField] private int _invincibilitySeconds = 0;

        [SerializeField] private UnityEvent _onDamage;
        [SerializeField] private UnityEvent _onCrit;
        [SerializeField] private UnityEvent _onDeath;

        private float lastDamageTime = Mathf.NegativeInfinity;

        public bool IsDead => _useHealthVariable ? _healthReference.Value <= 0 : _health <= 0;

        public void TakeDamage(int damage)
        {
            if (lastDamageTime + _invincibilitySeconds > Time.time)
                return;

            lastDamageTime = Time.time;
            
            if (_useHealthVariable)
            {
                if (_healthReference.Value <= 0) return;
                _healthReference.Value -= damage;
                _onDamage.Invoke();
                if (_healthReference.Value <= 0)
                {
                    Death();
                }
            }
            else
            {
                if (_health <= 0) return;
                _health -= damage;
                _onDamage.Invoke();
                if (_health <= 0)
                {
                    Death();
                }
            }
        }

        public void TakeCritDamage(int damage) {
            this.TakeDamage(damage * this._critMultiplier);
            _onCrit.Invoke();
        }

        private void Death()
        {
            _onDeath.Invoke();
        }
    }
}