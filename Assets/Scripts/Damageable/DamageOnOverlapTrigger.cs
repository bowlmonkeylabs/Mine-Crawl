using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts
{
    public class DamageOnOverlapTrigger : MonoBehaviour
    {
        [SerializeField] private BoxCollider _trigger;
        [SerializeField] private int _damage = 1;
        [SerializeField] private LayerMask _damageMask;
        [SerializeField] private float _damageCooldown = 2;
        [SerializeField] private bool _dealDamageToTriggers = false;
        [SerializeField] private DamageType _damageType;
        
        private float _lastDamageTime = Mathf.NegativeInfinity;
        
        private void AttemptDamage()
        {
            if(_lastDamageTime + _damageCooldown > Time.time) {
                return;
            }
            ;
            var center = _trigger.transform.TransformPoint(_trigger.center);
            var halfExtents = _trigger.size / 2f;
            Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, _trigger.transform.rotation,
                _damageMask, _dealDamageToTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);

            if (hitColliders.Length < 1)
                return;

            foreach (var other in hitColliders)
            {
                GameObject otherObj = other.gameObject;
                if (!otherObj.IsInLayerMask(_damageMask)) return;

                if (!_dealDamageToTriggers && other.isTrigger) return;

                Damageable damageable = otherObj.GetComponent<Damageable>();

                if (damageable == null && other.attachedRigidbody != null)
                    damageable = other.attachedRigidbody.GetComponent<Damageable>();
            
                if (damageable != null) {
                    damageable.TakeDamage(new HitInfo(_damageType, _damage, (other.transform.position - transform.position).normalized));
                    _lastDamageTime = Time.time;
                }
            }
        }

        public void Damage()
        {
            AttemptDamage();
        }
    }
}