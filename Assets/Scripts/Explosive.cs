using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Utils;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using System.Linq;

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
        [ShowIf("_applyKnockback"), SerializeField] private bool _customizeKnockback = false;
        [ShowIf("_customizeKnockback"), SerializeField] private FloatReference _knockbackTime = null;
        [SerializeField] private bool _applyStun = false;
        [SerializeField] private bool _useExplosiveRadiusFeedback = true;
        [SerializeField, ShowIf("_useExplosiveRadiusFeedback")] private MMF_Player _explosiveRadiusFeedback;
        [SerializeField] private float _explosionCueOffsetTime = .25f;
        [SerializeField] private MMF_Player _explosionCueFeedbacks;

        [SerializeField] private UnityEvent _onActivate;
        [SerializeField] private UnityEvent _onBeforeExplosion;
        [SerializeField] private UnityEvent _onExplosion;

        private bool isActive;
        private bool isDeActivated;
        private float activateTime;
        private float currentFuseTime;

        private const float _ExplosionRaycastDistanceThreshold = 0.9f;
        private const float _ExplosionRaycastDistanceThresholdSquared = _ExplosionRaycastDistanceThreshold * _ExplosionRaycastDistanceThreshold;

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

        public void UpdateDamage(IntVariable newDamage) {
            _damage.SetVariable(newDamage);
        }

        public void UpdateRadius(FloatVariable newRadius) {
            _explosionRadius.SetVariable(newRadius);
        }

        private void Update()
        {
            if (!isActive)
                return;

            // Feedbacks to cue imminent explosion
            if (activateTime + currentFuseTime - _explosionCueOffsetTime < Time.time)
                _explosionCueFeedbacks?.PlayFeedbacks();

            if (activateTime + currentFuseTime < Time.time)
                Explode();
        }

        private void Explode()
        {
            _onBeforeExplosion.Invoke();

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
                    bool withinThreshold = (hit.point - col.bounds.center).sqrMagnitude <= _ExplosionRaycastDistanceThresholdSquared;
                    if (!withinThreshold)
                    {
                        // Continue if hit obstacle
                        continue;
                    }
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

                if(_applyKnockback) {
                    Knockbackable knockbackable = col.GetComponent<Knockbackable>() ?? col.attachedRigidbody?.GetComponent<Knockbackable>();

                    if(knockbackable != null) {
                        if (_customizeKnockback)
                        {
                            hitInfo.KnockbackTime = _knockbackTime.Value;
                        }
                        
                        knockbackable.SetKnockback(hitInfo);
                    }
                }
                
               if(_applyStun) {
                    Stunnable stunnable = col.GetComponent<Stunnable>() ?? col.attachedRigidbody?.GetComponent<Stunnable>();
                    if(stunnable != null) {
                        stunnable.SetStun(hitInfo);
                    }
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