using System;
using BML.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class EnemyDamageOnTrigger : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private LayerMask _damageMask;
        [SerializeField] private float _damageCooldown = 2;

        private float _lastDamageTime = Mathf.NegativeInfinity;
        
        private void OnTriggerEnter(Collider other)
        {
            AttemptDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            AttemptDamage(other);
        }

        private void AttemptDamage(Collider other)
        {
            if(_lastDamageTime + _damageCooldown > Time.time) {
                return;
            }
            
            GameObject otherObj = other.gameObject;
            if (!otherObj.IsInLayerMask(_damageMask)) return;

            EnemyDamageable damageable = otherObj.GetComponent<EnemyDamageable>();
            if (damageable != null) {
                damageable.TakeDamage(new HitInfo(_damage, (other.transform.position - transform.position).normalized));
                _lastDamageTime = Time.time;
            }
        }
    }
}