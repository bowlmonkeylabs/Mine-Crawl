using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts
{
    public class DamageCollider : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private DamageType _damageType;
        
        public void AttemptDamage(List<Collider> colliders)
        {
            foreach (var col in colliders)
            {
                AttemptDamage(col);
            }
        }
        
        public void AttemptDamage(Collider other)
        {
            Damageable damageable = other.GetComponent<Damageable>();

            if (damageable == null && other.attachedRigidbody != null)
                damageable = other.attachedRigidbody.GetComponent<Damageable>();
            
            if (damageable != null) {
                damageable.TakeDamage(new HitInfo(_damageType, _damage, (other.transform.position - transform.position).normalized));
            }
        }
    }
}