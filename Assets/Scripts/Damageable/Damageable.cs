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

        public Health Health => health;
        public Health HealthTemporary => healthTemporary;

        // public Dictionary<DamageType, DamageableItem> Value { get {return _value;}}

        public void TakeDamage(HitInfo hitInfo)
        {
            if (this.enabled == false || health.IsDead)
            {
                Debug.Log($"Damageable {gameObject.name} is disabled or dead, skipping damage from {hitInfo}");
                return;
            }

            foreach(DamageableItem di in _damageable){
                if(di.DamageType.HasFlag(hitInfo.DamageType)) {
                    var damageResult = di.ApplyDamage(hitInfo);
                    var damageRemaining = damageResult.Damage;
                    bool damagedTempHealth = false;

                    if (healthTemporary != null)
                    {
                        var tempHealthDamage = Math.Min(damageRemaining, healthTemporary.Value);
                        damagedTempHealth = healthTemporary.Damage(tempHealthDamage);
                        if (damagedTempHealth)
                        {
                            damageRemaining -= tempHealthDamage;
                        }
                    }

                    bool dealtDamageToMainHealth = health.Damage(damageRemaining, damagedTempHealth);
                    if (!dealtDamageToMainHealth && !damagedTempHealth)
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
