using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class Damageable : MonoBehaviour
    {
        [SerializeField] [ShowIf("useHealthVariable")] [LabelText("health")] private IntVariable _healthReference;
        [SerializeField] [HideIf("useHealthVariable")] private int _health;
        [SerializeField] private bool useHealthVariable = false;

        [SerializeField] private int critMultiplier = 2;

        [SerializeField] private UnityEvent OnDamage;
        [SerializeField] private UnityEvent OnDeath;

        public bool IsDead => useHealthVariable ? _healthReference.Value <= 0 : _health <= 0;

        public void TakeDamage(int damage)
        {
            if (useHealthVariable)
            {
                if (_healthReference.Value <= 0) return;
                _healthReference.Value -= damage;
                OnDamage.Invoke();
                if (_healthReference.Value <= 0)
                {
                    Death();
                }
            }
            else
            {
                if (_health <= 0) return;
                _health -= damage;
                OnDamage.Invoke();
                if (_health <= 0)
                {
                    Death();
                }
            }
        }

        public void TakeCritDamage(int damage) {
            this.TakeDamage(damage * this.critMultiplier);
        }

        private void Death()
        {
            OnDeath.Invoke();
        }
    }
}