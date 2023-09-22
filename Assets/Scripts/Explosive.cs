using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Utils;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;

namespace BML.Scripts
{
    public class Explosive : MonoBehaviour
    {
        
        [SerializeField] private bool _setOriginTransform;
        [SerializeField, ShowIf("_setOriginTransform")] private Transform _origin;
        [SerializeField] private FloatReference _explosionTime;
        [SerializeField] private FloatReference _explosionShortFuseTime;
        [SerializeField] private FloatReference _explosionRadius;
        [SerializeField] private LayerMask _explosionMask;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private DamageType _damageType;
        [SerializeField] private IntReference _damage;
        [SerializeField] private bool _applyKnockback = true;
        [SerializeField] private bool _useExplosiveRadiusFeedback = true;
        [SerializeField, ShowIf("_useExplosiveRadiusFeedback")] private MMF_Player _explosiveRadiusFeedback;

        [SerializeField] private UnityEvent _onActivate;
        [SerializeField] private UnityEvent _onExplosion;

        private bool isActive;
        private bool isDeActivated;
        private float activateTime;
        private float currentFuseTime;

        public void Activate()
        {
            if (isActive || isDeActivated)
                return;
            
            Activate(false);
        }
        
        public void ActivateShortFuse()
        {
            Activate(true);
        }

        private void Activate(bool isShortFuse)
        {
            if (!isActive)
                _onActivate.Invoke();
            
            activateTime = Time.time;
            isActive = true;
            currentFuseTime = isShortFuse ? _explosionShortFuseTime.Value : _explosionTime.Value;
        }

        public void Deactivate()
        {
            isActive = false;
            isDeActivated = true;
        }

        private void Update()
        {
            if (!isActive)
                return;

            if (activateTime + currentFuseTime < Time.time)
                Explode();
        }

        private void Explode()
        {
            //NOTE: This will damage player and enemies on a per-collider basis.
            //      Does not check if multiple colliders belong to same entity.

            Vector3 origin = _setOriginTransform ? _origin.position : transform.position;
            Collider[] colliders = Physics.OverlapSphere(origin, _explosionRadius.Value, _explosionMask, QueryTriggerInteraction.Ignore);

            foreach (var col in colliders)
            {
                if(col.gameObject == gameObject || col.attachedRigidbody?.gameObject == gameObject) {
                    continue;
                }

                RaycastHit hit;
                Vector3 originToTarget = col.bounds.center - origin;
                if (Physics.Raycast(origin, originToTarget.normalized, out hit, originToTarget.magnitude, _obstacleMask))
                {
                    // Continue if hit obstacle
                    continue;
                }

                Damageable damageable = col.GetComponent<Damageable>();
                if (damageable == null)
                {
                    damageable = col.attachedRigidbody?.GetComponent<Damageable>();
                }

                HitInfo hitInfo = new HitInfo(_damageType, _damage.Value,
                    (col.transform.position - origin).normalized);
                
                if(damageable != null) {
                    damageable.TakeDamage(hitInfo);
                }
                
                if (!_applyKnockback) continue;

                Knockbackable knockbackable = col.GetComponent<Knockbackable>();
                if (knockbackable == null)
                {
                    knockbackable = col.attachedRigidbody?.GetComponent<Knockbackable>();
                }

                if (knockbackable != null)
                {
                    knockbackable.SetKnockback(hitInfo);
                }
            }

            isActive = false;

            if(_useExplosiveRadiusFeedback) {
                _explosiveRadiusFeedback.transform.localScale = _explosionRadius.Value * 2 * Vector3.one;
                _explosiveRadiusFeedback.PlayFeedbacks(transform.position);
            }
            
            _onExplosion.Invoke();
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
            Gizmos.DrawWireSphere (transform.position, _explosionRadius.Value);
        }
    }
}