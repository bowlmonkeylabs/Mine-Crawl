using Mono.CSharp;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts {
    public class Damageable : MonoBehaviour
    {
        [SerializeField, PropertySpace(0f, 10f)] private Health health;
        [SerializeField] private List<DamageableItem> _damageable;
        [SerializeField, PropertySpace(10f, 0f)] private UnityEvent<HitInfo> _onDamage;

        // public Dictionary<DamageType, DamageableItem> Value { get {return _value;}}

        public void TakeDamage(HitInfo hitInfo)
        {
            DamageableItem damageable = _damageable.FirstOrDefault<DamageableItem>(di => di.DamageType.HasFlag(hitInfo.DamageType));
            if(damageable != null) {
                if(!health.DecrementHealth(damageable.ApplyDamage(hitInfo.Damage))) {
                    damageable.OnFailDamage.Invoke(hitInfo);
                    return;
                }
                
                _onDamage.Invoke(hitInfo);
                damageable.OnDamage.Invoke(hitInfo);

                if(health.IsDead) {
                    damageable.OnDeath.Invoke(hitInfo);
                }
            }
        }
    }
}
