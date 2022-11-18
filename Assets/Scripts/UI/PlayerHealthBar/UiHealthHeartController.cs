using System;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.UI.PlayerHealthBar
{
    public class UiHealthHeartController : MonoBehaviour
    {
        #region Inspector

        [SerializeField, Range(0, 2)] [OnValueChanged("SetValue_Inspector")] private int _value;
        [SerializeField] private bool _invincible;
        
        [SerializeField] private GameObject _heartOutline;
        [SerializeField] private GameObject _heartHalf;
        [SerializeField] private GameObject _heartFull;
        
        [SerializeField] private MMF_Player _invincibleFeedbacks;
        [SerializeField] private MMF_Player _incrementAnyFeedbacks;
        [SerializeField] private MMF_Player _incrementFeedbacks;
        [SerializeField] private MMF_Player _decrementAnyFeedbacks;
        [SerializeField] private MMF_Player _decrementFeedbacks;
        [SerializeField] private MMF_Player _lowHealthFeedbacks;

        [ShowInInspector, ReadOnly] private UiHealthBarControlller _healthBarController;
        
        #endregion

        #region Unity lifecycle

        #endregion

        #region Public interface

        public void Initialize(UiHealthBarControlller healthBarController)
        {
            _healthBarController = healthBarController;
        }

        private void SetValue_Inspector(int newValue)
        {
            SetValue(newValue, null);
        }
        public void SetValue(int newValue, int? totalHealthDelta)
        {
            int clampedNewValue = Mathf.Clamp(newValue, 0, 2);

            int delta = clampedNewValue - _value;
            if (delta > 0)
            {
                _incrementFeedbacks.PlayFeedbacks();
            }
            else if (delta < 0)
            {
                _decrementFeedbacks.PlayFeedbacks();
            }
            
            if (!totalHealthDelta.HasValue)
            {
                // do nothing
            }
            else if (totalHealthDelta < 0)
            {
                _decrementAnyFeedbacks.PlayFeedbacks();
            }
            else if (totalHealthDelta > 0)
            {
                _incrementAnyFeedbacks.PlayFeedbacks();
            }
            
            if (_healthBarController.IsLowHealth)
            {
                _lowHealthFeedbacks.PlayFeedbacks();
            }
            else if (totalHealthDelta > 0)
            {
                _lowHealthFeedbacks.StopFeedbacks();
                _lowHealthFeedbacks.ResetFeedbacks();
            }
            
            _value = clampedNewValue;
            UpdateUi();
        }

        public void SetInvincible(bool invincible)
        {
            _invincible = invincible;
            if (_invincible)
            {
                _invincibleFeedbacks.PlayFeedbacks();
            }
            else
            {
                _invincibleFeedbacks.StopFeedbacks();
                _invincibleFeedbacks.ResetFeedbacks();
            }
        }

        #endregion

        private void UpdateUi()
        {
            switch (_value)
            {
                case 0:
                    _heartHalf.SetActive(false);
                    _heartFull.SetActive(false);
                    break;
                case 1:
                    _heartHalf.SetActive(true);
                    _heartFull.SetActive(false);
                    break;
                case 2:
                    _heartFull.SetActive(true);
                    // _halfHeart.SetActive(false);
                    break;
            }
            
        }
     }
}