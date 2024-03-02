using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using Micosmo.SensorToolkit;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    [RequireComponent(typeof(RangeSensor))]
    [RequireComponent(typeof(LOSSensor))]
    public class ExplosiveDamager : MonoBehaviour
    {
        #region Inspector
        
        [TitleGroup("Sensors")]
        
        [SerializeField] private RangeSensor _rangeSensor;
        [SerializeField] private LOSSensor _losSensor;
        [SerializeField] private SafeFloatValueReference _additionalRange;

        [TitleGroup("Effects")]

        [SerializeField] private bool _applyDamage = true;
        [SerializeField, ShowIf("_applyDamage"), Required] private DamageType _damageType;
        [SerializeField, ShowIf("_applyDamage"), Required] private SafeIntValueReference _damage;
        [SerializeField, ShowIf("_applyDamage")] private SafeIntValueReference _additionalDamage;

        [SerializeField] private bool _applyKnockback = true;
        [SerializeField, ShowIf("_applyKnockback")] private bool _customizeKnockback = false;
        [SerializeField, ShowIf("_customizeKnockback"), Required] private SafeFloatValueReference _knockbackTime = null;

        [SerializeField] private bool _applyStun = false;

        [TitleGroup("Feedback")]
        
        [SerializeField] private bool _useExplosiveRadiusFeedback = true;
        [SerializeField, ShowIf("_useExplosiveRadiusFeedback"), Required] private MMF_Player _explosiveRadiusFeedback;

        #endregion

        #region Unity lifecycle

        #if UNITY_EDITOR

        private void OnValidate()
        {
            ConfigureSensors();
        }

        private void ConfigureSensors()
        {
            if (_rangeSensor == null) _rangeSensor = GetComponent<RangeSensor>();
            if (_losSensor == null) _losSensor = GetComponent<LOSSensor>();

            _rangeSensor.PulseMode = PulseRoutine.Modes.Manual;
            _rangeSensor.IgnoreTriggerColliders = true;
            _rangeSensor.Shape = RangeSensor.Shapes.Sphere;

            _losSensor.PulseMode = PulseRoutine.Modes.Manual;
            _losSensor.IgnoreTriggerColliders = true;
            _losSensor.InputSensor = _rangeSensor;
            
            // _rangeSensor.hideFlags = HideFlags.HideInInspector;
            // _losSensor.hideFlags = HideFlags.HideInInspector;
            
            _rangeSensor.hideFlags = HideFlags.None;
            _losSensor.hideFlags = HideFlags.None;
        }
        
        #endif

        private void OnEnable()
        {
            UpdateRadius();
            _additionalRange.Subscribe(UpdateRadius);
        }

        private void OnDisable()
        {
            _additionalRange.Unsubscribe(UpdateRadius);
        }

        #endregion
        
        #region Range 
        
        public float Radius => _rangeSensor?.Sphere.Radius ?? 0;
        
        private float? _initialRadius;

        private void UpdateRadius()
        {
            if (_initialRadius == null)
            {
                _initialRadius = _rangeSensor.Sphere.Radius;
            }
            _rangeSensor.Sphere.Radius = _initialRadius.Value + _additionalRange.Value; 
        }
        
        #endregion
        
        #region Apply Damage
        
        public void ApplyDamage()
        {
            //NOTE: This will damage player and enemies on a per-collider basis.
            //      Does not check if multiple colliders belong to same entity.

            _losSensor.PulseAll();
            var collidersInSight = _losSensor.GetDetections()
                .Select(go => go.GetComponent<Collider>())
                .Where(c => c.gameObject != gameObject && c.attachedRigidbody?.gameObject != gameObject) // TODO is this still needed?
                .ToList();
            
            Debug.Log($"BOMB: ({collidersInSight.Count} colliders hit)");

            foreach (var coll in collidersInSight)
            {
                ApplyDamage(coll);
            }

            if (_useExplosiveRadiusFeedback)
            {
                _explosiveRadiusFeedback.transform.localScale = _rangeSensor.Sphere.Radius * 2 * Vector3.one;
                _explosiveRadiusFeedback.PlayFeedbacks(transform.position);
            }
        }

        private void ApplyDamage(Collider coll)
        {
            HitInfo hitInfo = new HitInfo(
                _damageType,
                _damage.Value + (_additionalDamage?.Value ?? 0),
                (coll.transform.position - _losSensor.transform.position).normalized
            );

            // Apply damage
            if (_applyDamage)
            {
                Damageable damageable = coll.GetComponent<Damageable>() ?? 
                                        coll.attachedRigidbody?.GetComponent<Damageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(hitInfo);
                }
            }

            // Apply knockback
            if (_applyKnockback)
            {
                Knockbackable knockbackable = coll.GetComponent<Knockbackable>() ??
                                              coll.attachedRigidbody?.GetComponent<Knockbackable>();

                if (knockbackable != null)
                {
                    if (_customizeKnockback)
                    {
                        knockbackable.SetKnockback(hitInfo, _knockbackTime.Value);
                    }
                    else
                    {
                        knockbackable.SetKnockback(hitInfo);
                    }
                }
            }

            // Apply stun
            if (_applyStun)
            {
                Stunnable stunnable = coll.GetComponent<Stunnable>() ??
                                      coll.attachedRigidbody?.GetComponent<Stunnable>();
                if (stunnable != null)
                {
                    stunnable.SetStun(hitInfo);
                }
            }
        }
        
        #endregion
        
    }
}