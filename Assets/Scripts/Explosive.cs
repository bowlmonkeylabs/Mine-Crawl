using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Utils;

namespace BML.Scripts
{
    public class Explosive : MonoBehaviour
    {
        
        [SerializeField] private FloatReference _explosionTime;
        [SerializeField] private FloatReference _explosionRadius;
        [SerializeField] private LayerMask _explosionMask;
        [SerializeField] private DamageType _damageType;
        [SerializeField] private IntReference _damage;

        [SerializeField] private UnityEvent _onActivate;
        [SerializeField] private UnityEvent _onExplosion;

        private bool isActive;
        private float activateTime;

        public void Activate()
        {
            if (isActive)
                return;

            activateTime = Time.time;
            isActive = true;
            _onActivate.Invoke();
        }

        public void Deactivate()
        {
            isActive = false;
        }

        private void Update()
        {
            if (!isActive)
                return;

            if (activateTime + _explosionTime.Value < Time.time)
                Explode();
        }

        private void Explode()
        {
            //NOTE: This will damage player and enemies on a per-collider basis.
            //      Does not check if multiple colliders belong to same entity.

            Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius.Value, _explosionMask);

            foreach (var col in colliders)
            {
                if(col.gameObject == gameObject || col.attachedRigidbody?.gameObject == gameObject) {
                    continue;
                }

                Damageable damageable = col.GetComponent<Damageable>();
                if (damageable == null)
                {
                    damageable = col.attachedRigidbody?.GetComponent<Damageable>();
                }

                if(damageable == null) {
                    damageable = col.transform.parent?.GetComponent<Damageable>();
                }

                if(damageable != null) {
                    damageable.TakeDamage(new HitInfo(_damageType, _damage.Value, (col.transform.position - transform.position).normalized));
                }
            }

            isActive = false;
            _onExplosion.Invoke();
        }
    }
}