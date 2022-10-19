using BML.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class DamageOnCollision : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private LayerMask _damageMask;
        [SerializeField] private float _damageCooldown = 2;
        [SerializeField] private UnityEvent _onCollisionEnter;

        private float _lastDamageTime = Mathf.NegativeInfinity;
        
        private void OnCollisionEnter(Collision col)
        {
            AttemptDamage(col.collider);
        }

        private void AttemptDamage(Collider other)
        {
            if(_lastDamageTime + _damageCooldown > Time.time) {
                return;
            }
            
            GameObject otherObj = other.gameObject;
            if (!otherObj.IsInLayerMask(_damageMask)) return;

            Damageable damageable = otherObj.GetComponent<Damageable>();
            
            if (damageable == null && other.attachedRigidbody != null)
                damageable = other.attachedRigidbody.GetComponent<Damageable>();
            
            if (damageable != null) {
                damageable.TakeDamage(new HitInfo(_damage, (other.transform.position - transform.position).normalized));
                _lastDamageTime = Time.time;
                _onCollisionEnter.Invoke();
            }
        }
    }
}