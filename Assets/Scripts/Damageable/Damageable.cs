using System;
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
                    var damageRemaining = damageResult.Damage;
                    bool damagedTempHealth = false;
                    // int tempHealthBeforeDamage = 0;
                    //
                    // if (healthTemporary != null)
                    // {
                    //     tempHealthBeforeDamage = healthTemporary.Value;
                    //     damagedTempHealth = healthTemporary.Damage(damageResult);
                    // }
                    // Debug.Log($"damageResult to temp health: {damageResult}");
                    // Debug.Log($"tempHealthBeforeDamage: {tempHealthBeforeDamage}");
                    // var remainingDamage = damageResult.Damage -  Mathf.Min(damageResult.Damage, damageResult.Damage - tempHealthBeforeDamage);
                    // Debug.Log($"damageResult to reg health: {remainingDamage}");
                    if (healthTemporary != null)
                    {
                        var tempHealthDamage = Math.Min(damageRemaining, healthTemporary.Value);
                        //Debug.Log($"{name} taking {tempHealthDamage} to Health Temp");
                        damagedTempHealth = healthTemporary.Damage(tempHealthDamage);
                        if (damagedTempHealth)
                        {
                            damageRemaining -= tempHealthDamage;
                        }
                    }
                    
                    //Debug.Log($"{name} taking {damageRemaining} to Health");
                    if (!health.Damage(damageRemaining) && !damagedTempHealth)
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
