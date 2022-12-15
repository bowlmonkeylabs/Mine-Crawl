using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player
{
    public class FallDamageController : MonoBehaviour
    {
        [SerializeField] private Damageable _damageable;
        [SerializeField] private BoolVariable _isGrounded;
        [SerializeField] private BoolReference _isRopeMovementEnabled;
        [SerializeField] private AnimationCurve _fallDamageCurve;
        [SerializeField] private MMF_Player _fallDamageFeedback;
        
        private float fallStartHeight;

        #region UnityLifecyle

        private void OnEnable()
        {
            _isGrounded.Subscribe(OnGroundingStatusChanged);
            _isRopeMovementEnabled.Subscribe(ResetFall);
        }

        private void OnDisable()
        {
            _isGrounded.Unsubscribe(OnGroundingStatusChanged);
            _isRopeMovementEnabled.Unsubscribe(ResetFall);
        }

        #endregion
        
        private void OnGroundingStatusChanged(bool prev, bool isGrounded)
        {
            if (isGrounded)
                TryApplyFallDamage();
            else
                ResetFall();
        }

        private void ResetFall()
        {
            fallStartHeight = transform.position.y;
        }

        private void TryApplyFallDamage()
        {
            if (!_isGrounded.Value || _isRopeMovementEnabled.Value)
                return;
            
            float deltaFall = Mathf.Max(0f, fallStartHeight - transform.position.y);
            
            if (deltaFall <= 0f)
                return;

            int damage = Mathf.FloorToInt(_fallDamageCurve.Evaluate(deltaFall));
            Debug.Log($"Applying fall damage {damage} | Height {deltaFall}");

            if (damage <= 0)
                return;

            HitInfo hitInfo = 
                new HitInfo(DamageType.Fall_Damage, damage, Vector3.up, transform.position);
            _damageable.TakeDamage(hitInfo);
            _fallDamageFeedback.PlayFeedbacks();
        }
    }
}