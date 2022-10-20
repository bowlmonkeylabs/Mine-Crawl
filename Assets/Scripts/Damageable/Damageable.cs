using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts {
    public class Damageable : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private List<DamageableItem> _damageable;
        [SerializeField] private UnityEvent<HitInfo> _onDamage;

        // public Dictionary<DamageType, DamageableItem> Value { get {return _value;}}

        public void TakeDamage(HitInfo hitInfo)
        {
            DamageableItem damageable = _damageable.FirstOrDefault<DamageableItem>(di => di.DamageType == hitInfo.DamageType);
            if(damageable != null) {
                if(!health.DecrementHealth(damageable.ApplyDamage(hitInfo.Damage))) {
                    damageable.OnFailDamage.Invoke(hitInfo);
                    return;
                }
                
                _onDamage.Invoke(hitInfo);
                damageable.OnDamage.Invoke(hitInfo);
                damageable.OnDamage.Invoke(hitInfo);
            }
        }
    }
}
