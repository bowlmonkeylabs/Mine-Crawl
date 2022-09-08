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
        
        [SerializeField] private UnityEvent<HitInfo> _onDamage;
        [SerializeField] private UnityEvent<HitInfo> _onFailDamage;
        [SerializeField] private UnityEvent<HitInfo> _onCrit;
        [SerializeField] private UnityEvent _onDeath;

        
        public void TakeDamage(HitInfo hitInfo)
        {
            //Don't play feedbacks if didn't actually take damage
            if (!health.DecrementHealth(hitInfo.Damage))
            {
                _onFailDamage.Invoke(hitInfo);
                return;
            }
                
            
            _onDamage.Invoke(hitInfo);

            if(health.IsDead) {
                _onDeath.Invoke();
            }
        }

        public void TakeCritDamage(HitInfo hitInfo) {
            hitInfo.Damage *= this._critMultiplier;
            _onCrit.Invoke(hitInfo);
            this.TakeDamage(hitInfo);
        }
        
    }
}