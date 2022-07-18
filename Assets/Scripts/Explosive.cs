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
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private LayerMask _enemyMask;

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
                Damageable damageable = col.GetComponent<Damageable>();
                damageable.TakeDamage(_damagePlayer.Value);
            }
            
            Collider[] enemyColliders = Physics.OverlapSphere(transform.position, _explosionRadius.Value, _enemyMask);

            foreach (var col in enemyColliders)
            {
                Damageable damageable = col.GetComponent<Damageable>();
                damageable.TakeDamage(_damageEnemy.Value);
            }

            isActive = false;
            _onExplosion.Invoke();
        }
    }
}