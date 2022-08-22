using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class Explosive : MonoBehaviour
    {
        [SerializeField] private FloatReference _explosionTime;
        [SerializeField] private FloatReference _explosionRadius;
        [SerializeField] private IntReference _damagePlayer;
        [SerializeField] private IntReference _damageEnemy;
        [SerializeField] private IntReference _damageOre;
        [SerializeField] private IntReference _damageExitBarrier;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private LayerMask _enemyMask;
        [SerializeField] private LayerMask _interactableMask;
        [SerializeField] private String _oreTag;
        [SerializeField] private String _exitBarrierTag;

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
            
            Collider[] playerColliders = Physics.OverlapSphere(transform.position, _explosionRadius.Value, _playerMask);

            foreach (var col in playerColliders)
            {
                ExplosiveDamageable damageable = col.GetComponent<ExplosiveDamageable>();
                damageable.TakeDamage(new ExplosionHitInfo() {
                    Damage = _damagePlayer.Value
                });
            }
            
            Collider[] enemyColliders = Physics.OverlapSphere(transform.position, _explosionRadius.Value, _enemyMask);

            foreach (var col in enemyColliders)
            {
                ExplosiveDamageable damageable = col.GetComponent<ExplosiveDamageable>();
                damageable.TakeDamage(new ExplosionHitInfo() {
                    Damage = _damageEnemy.Value
                });
            }
            
            Collider[] interactableColliders = Physics.OverlapSphere(transform.position, _explosionRadius.Value, _interactableMask);

            foreach (var col in interactableColliders)
            {
                ExplosiveDamageable damageable = col.GetComponentInParent<ExplosiveDamageable>();

                if (damageable == null) continue;

                if (damageable.gameObject.tag.Equals(_oreTag) && damageable.gameObject != gameObject) {
                    damageable.TakeDamage(new ExplosionHitInfo() {
                        Damage = _damageOre.Value
                    });
                    continue;
                }

                if(damageable.gameObject.tag.Equals(_exitBarrierTag)) {
                    damageable.TakeDamage(new ExplosionHitInfo() {
                        Damage = _damageExitBarrier.Value
                    });
                }
            }

            isActive = false;
            _onExplosion.Invoke();
        }
    }
}