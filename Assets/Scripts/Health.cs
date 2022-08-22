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
        [SerializeField] private UnityEvent<int, int> _onHealthChange;
        [SerializeField] private UnityEvent _onDeath;

        private int _value{get => Value; set {
            if(_useHealthVariable) _healthReference.Value = value;
            else _health = value;
        }}

        public int Value {get => _useHealthVariable ? _healthReference.Value : _health;}
        public bool IsDead {get => Value <= 0;}

        public void DecrementHealth(int amount) {
            if (Value <= 0) return;

            _onHealthChange.Invoke(Value - amount, Value);
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
    }
}
