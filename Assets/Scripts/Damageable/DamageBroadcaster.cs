using System.Collections.Generic;
using BML.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class DamageBroadcaster : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private LayerMask _damageMask;
        [SerializeField] private float _damageCooldown = 2;
        [SerializeField] private bool _dealDamageToTriggers = false;
        [SerializeField] private DamageType _damageType;
        [SerializeField] private UnityEvent _onDealDamage;
        
        private float _lastDamageTime = Mathf.NegativeInfinity;
        
        public void AttemptDamage(List<Collider> colliders)
        {
            colliders.ForEach(AttemptDamage);
        }
        
        public void AttemptDamage(Collider other)
        {
            if(_lastDamageTime + _damageCooldown > Time.time) {
                return;
            }
            
            GameObject otherObj = other.gameObject;
            if (!otherObj.IsInLayerMask(_damageMask)) return;
            
            if (!_dealDamageToTriggers && other.isTrigger) return;

            Damageable damageable = otherObj.GetComponent<Damageable>();

            if (damageable == null && other.attachedRigidbody != null)
                damageable = other.attachedRigidbody.GetComponent<Damageable>();
            
            if (damageable != null) {
                damageable.TakeDamage(new HitInfo(_damageType, _damage, (other.transform.position - transform.position).normalized));
                _lastDamageTime = Time.time;
                _onDealDamage.Invoke();
            }
        }
    }
}