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

        [SerializeField] private Color _heartFill = new Color(242f/255f, 60f/255f, 60f/255f);
        [SerializeField] private Color _heartOutlineFill = new Color(85f/255f, 25f/255f, 25f/255f);
        [SerializeField] private Color _heartTempFill = new Color(109f/255f, 168f/255f, 255f/255f);
        [SerializeField] private Color _heartTempOutlineFill = new Color(25f/255f, 25f/255f, 85f/255f);

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
            
            if (!totalHealthDelta.HasValue)
            {
                // do nothing
            }
            else if (totalHealthDelta < 0)
            {
                _decrementAnyFeedbacks.Initialization();
                _decrementAnyFeedbacks.PlayFeedbacks();
            }
            else if (totalHealthDelta > 0)
            {
                _incrementAnyFeedbacks.Initialization();
                _incrementAnyFeedbacks.PlayFeedbacks();
            }
            
            if (_healthBarController.IsLowHealth)
            {
                _lowHealthFeedbacks.StopFeedbacks();
                _lowHealthFeedbacks.Initialization();
                _lowHealthFeedbacks.PlayFeedbacks();
            }
            else if (totalHealthDelta > 0 && _lowHealthFeedbacks.gameObject.activeInHierarchy)
            {
                _lowHealthFeedbacks.StopFeedbacks();
                _lowHealthFeedbacks.Initialization();
                _lowHealthFeedbacks.ResetFeedbacks();
            }
        }

        public void SetInvincible(bool invincible)
        {
            _invincible = invincible;
            if (_invincible)
            {
                _invincibleFeedbacks.StopFeedbacks();
                _invincibleFeedbacks.Initialization();
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
            _heartHalfLeftSynchronizer.SetEntryId(isTemporaryHeart ? healthTempColor : healthColor);
            _heartHalfRightSynchronizer.SetEntryId(isTemporaryHeart ? healthTempColor : healthColor);
            _heartFullSynchronizer.SetEntryId(isTemporaryHeart ? healthTempColor : healthColor);

            Debug.Log($"Heart: {name} | IsTemp: {isTemporaryHeart}");
        }
     }
}