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
        [SerializeField, PropertySpace(0f, 10f)] private Health healthTemporary;
        [SerializeField] private List<DamageableItem> _damageable;
        [SerializeField, PropertySpace(10f, 0f)] private UnityEvent<HitInfo> _onDamage;

        // public Dictionary<DamageType, DamageableItem> Value { get {return _value;}}

        public void TakeDamage(HitInfo hitInfo)
        {
            foreach(DamageableItem di in _damageable){
                if(di.DamageType.HasFlag(hitInfo.DamageType)) {
                    var damageResult = di.ApplyDamage(hitInfo);

                    int tempHealthBeforeDamage = 0;
                    if (healthTemporary != null)
                    {
                        tempHealthBeforeDamage = healthTemporary.Value;
                        healthTemporary.Damage(damageResult);
                    }
                    damageResult.Damage -= Mathf.Min(damageResult.Damage, tempHealthBeforeDamage);
                    if (!health.Damage(damageResult) && tempHealthBeforeDamage <= 0)
                    {
                        di.OnFailDamage.Invoke(hitInfo);
                        continue;
                    }

                    _onDamage.Invoke(hitInfo);
                    di.OnDamage.Invoke(hitInfo);

                    if(health.IsDead) {
                        di.OnDeath.Invoke(hitInfo);
                        break;
                    }
                }
            }
        }
    }
}
