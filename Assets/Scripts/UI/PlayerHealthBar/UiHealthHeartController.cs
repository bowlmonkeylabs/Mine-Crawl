using System;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using uPalette.Generated;
using uPalette.Runtime.Core.Synchronizer.Color;

namespace BML.Scripts.UI.PlayerHealthBar
{
    public class UiHealthHeartController : MonoBehaviour
    {
        #region Inspector

        [SerializeField, Range(0, 2)] [OnValueChanged("SetValue_Inspector")] private int _value;
        [SerializeField] private bool _invincible;
        
        [SerializeField] private GameObject _heartOutline;
        [SerializeField] private GameObject _heartHalfLeft;
        [SerializeField] private GameObject _heartHalfRight;
        [SerializeField] private GameObject _heartFull;
        
        [SerializeField] private MMF_Player _invincibleFeedbacks;
        [SerializeField] private MMF_Player _incrementAnyFeedbacks;
        [SerializeField] private MMF_Player _incrementFeedbacks;
        [SerializeField] private MMF_Player _decrementAnyFeedbacks;
        [SerializeField] private MMF_Player _decrementFeedbacks;
        [SerializeField] private MMF_Player _lowHealthFeedbacks;
        
        [ShowInInspector, ReadOnly] private UiHealthBarControlller _healthBarController;

        private ColorSynchronizer _heartHalfLeftSynchronizer, _heartHalfRightSynchronizer, _heartFullSynchronizer, _heartOutlineSynchronizer;
        
        #endregion

        #region Unity lifecycle

        #endregion

        #region Public interface

        public void Initialize(UiHealthBarControlller healthBarController)
        {
            _healthBarController = healthBarController;
            _heartHalfLeftSynchronizer = _heartHalfLeft.GetComponent<ColorSynchronizer>();
            _heartHalfRightSynchronizer = _heartHalfRight.GetComponent<ColorSynchronizer>();
            _heartFullSynchronizer = _heartFull.GetComponent<ColorSynchronizer>();
            _heartOutlineSynchronizer = _heartOutline.GetComponent<ColorSynchronizer>();
        }

        private void SetValue_Inspector(int newValue)
        {
            SetValue(newValue, null, false);
        }
        public void SetValue(int newValue, int? totalHealthDelta, bool isTemporaryHeart)
        {
            int clampedNewValue = Mathf.Clamp(newValue, 0, 2);
            int delta = clampedNewValue - _value;
            _value = clampedNewValue;
            
            UpdateUi(isTemporaryHeart);

            if ((totalHealthDelta ?? 0) != 0 && _value > 0) // only if this section of health was affected by the increase
            {
                if (delta > 0)
                {
                    _incrementFeedbacks.Initialization();
                    _incrementFeedbacks.PlayFeedbacks();
                }
                else if (delta < 0)
                {
                    _decrementFeedbacks.Initialization();
                    _decrementFeedbacks.PlayFeedbacks();
                }
            }
        }

        private LTDescr _lowHealthFeedbacksStartTween;
        private void StartLowHealthFeedbacksTween(float duration)
        {
            if (_healthBarController.IsLowHealth)
            {
                if (_lowHealthFeedbacksStartTween != null)
                {
                    LeanTween.cancel(this.gameObject, _lowHealthFeedbacksStartTween.uniqueId, false);
                }
                _lowHealthFeedbacksStartTween =
                    LeanTween.value(this.gameObject, 0, 1, duration)
                        .setOnComplete(() =>
                        {
                            // this flickers the same image as 'increment/decrement any', so it needs to wait until that is done to play without interfering.
                            // if (_healthBarController.IsLowHealth && !_lowHealthFeedbacks.IsPlaying && !_incrementAnyFeedbacks.IsPlaying && !_decrementAnyFeedbacks.IsPlaying)
                            if (_healthBarController.IsLowHealth && !_lowHealthFeedbacks.IsPlaying)
                            {
                                if (_decrementAnyFeedbacks.IsPlaying)
                                {
                                    _decrementAnyFeedbacks.StopFeedbacks();
                                }
                                if (_incrementAnyFeedbacks.IsPlaying)
                                {
                                    _incrementAnyFeedbacks.StopFeedbacks();
                                }
                                _decrementAnyFeedbacks.ResetFeedbacks();
                                
                                _lowHealthFeedbacks.PlayFeedbacks();
                            }
                        });
            }
        } 
            
        public void OnTotalHealthChange(float prevTotalHealth, float currentTotalHealth)
        {
            if (_value <= 0)
            {
                return;
            }
            
            float delta = currentTotalHealth - prevTotalHealth;

            if (!_healthBarController.IsLowHealth && delta > 0 && _lowHealthFeedbacks.IsPlaying)
            {
                _lowHealthFeedbacks.StopFeedbacks();
                _lowHealthFeedbacks.ResetFeedbacks();
            }
            
            if (delta < 0)
            {
                if (_lowHealthFeedbacks.IsPlaying)
                {
                    _lowHealthFeedbacks.StopFeedbacks();
                    _lowHealthFeedbacks.ResetFeedbacks();
                }
                
                _decrementAnyFeedbacks.Initialization();
                _decrementAnyFeedbacks.PlayFeedbacks();
                var duration = _decrementAnyFeedbacks.TotalDuration;
                StartLowHealthFeedbacksTween(duration);
            }
            else if (delta > 0)
            {
                if (_lowHealthFeedbacks.IsPlaying)
                {
                    _lowHealthFeedbacks.StopFeedbacks();
                    _lowHealthFeedbacks.ResetFeedbacks();
                }
                
                _incrementAnyFeedbacks.Initialization();
                _incrementAnyFeedbacks.PlayFeedbacks();
                var duration = _incrementAnyFeedbacks.TotalDuration;
                StartLowHealthFeedbacksTween(duration);
            }
            else
            {
                StartLowHealthFeedbacksTween(0);
            }
            
        }

        public void SetInvincible(bool invincible)
        {
            _invincible = invincible;
            if (_invincible)
            {
                _invincibleFeedbacks.StopFeedbacks();
                _invincibleFeedbacks.ResetFeedbacks();
                _invincibleFeedbacks.PlayFeedbacks();
            }
            else if (_incrementFeedbacks.gameObject.activeInHierarchy)
            {
                _invincibleFeedbacks.StopFeedbacks();
                _invincibleFeedbacks.ResetFeedbacks();
            }
        }

        public void TryRestartInvincibleFeedbacks()
        {
            if (_invincible)
            {
                _invincibleFeedbacks.StopFeedbacks();
                _invincibleFeedbacks.ResetFeedbacks();
                _invincibleFeedbacks.PlayFeedbacks();
            }
            else if (_incrementFeedbacks.gameObject.activeInHierarchy)
            {
                _invincibleFeedbacks.StopFeedbacks();
                _invincibleFeedbacks.ResetFeedbacks();
            }
        }

        #endregion

        private void UpdateUi(bool isTemporaryHeart)
        {
            _lowHealthFeedbacks.StopFeedbacks();
            _lowHealthFeedbacks.ResetFeedbacks();
            _invincibleFeedbacks.StopFeedbacks();
            _invincibleFeedbacks.ResetFeedbacks();
            
            switch (_value)
            {
                case 0:
                    _heartHalfLeft.SetActive(false);
                    _heartHalfRight.SetActive(false);
                    _heartFull.SetActive(false);
                    break;
                case 1:
                    _heartHalfLeft.SetActive(true);
                    _heartHalfRight.SetActive(false);
                    _heartFull.SetActive(false);
                    break;
                case 2:
                    _heartFull.SetActive(true);
                    // _halfHeart.SetActive(false);
                    break;
            }

            var healthTempOutlineColor = ColorEntry.UI_Health_Temp_Outline.ToEntryId();
            var healthOutlineColor = ColorEntry.UI_Heart_Outline.ToEntryId();
            var healthColor = ColorEntry.UI_Heart_Fill.ToEntryId();
            var healthTempColor = ColorEntry.UI_Health_Temp_Fill.ToEntryId();
            
            _heartOutlineSynchronizer.SetEntryId(isTemporaryHeart ? healthTempOutlineColor : healthOutlineColor);
            _heartOutlineSynchronizer.enabled = false;
            _heartOutlineSynchronizer.enabled = true;
            _heartHalfLeftSynchronizer.SetEntryId(isTemporaryHeart ? healthTempColor : healthColor);
            _heartHalfRightSynchronizer.SetEntryId(isTemporaryHeart ? healthTempColor : healthColor);
            _heartFullSynchronizer.SetEntryId(isTemporaryHeart ? healthTempColor : healthColor);

            // Init feedbacks to store the new colors
            _invincibleFeedbacks.Initialization();
            _lowHealthFeedbacks.Initialization();
        }
     }
}