using System;
using BML.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class DamageOnTrigger : MonoBehaviour
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
            
            Debug.Log($"Trying to damage {other.gameObject.name}");

            GameObject otherObj = other.gameObject;
            if (!otherObj.IsInLayerMask(_damageMask)) return;
            
            Debug.Log($"Will damage {other.gameObject.name}");

            Damageable damageable = otherObj.GetComponent<Damageable>();
            if (damageable != null) {
                damageable.TakeDamage(_damage);
                _lastDamageTime = Time.time;
                Debug.Log($"Damaged {other.gameObject.name}");
            }
        }
    }
}