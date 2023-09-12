using System;
using System.Linq;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    [ExecuteAlways]
    public class UiActiveItemCounterController : MonoBehaviour
    {
        #region Inspector
        
        [TitleGroup("Item")]
        [SerializeField, OnValueChanged("UpdateAssignedItem")] private PlayerInventory _playerInventory;
        public PlayerInventory PlayerInventory
        {
            get => _playerInventory;
            set
            {
                _playerInventory = value;
            }
        }
        [TitleGroup("Item"), ShowInInspector, ReadOnly] private PlayerItem _activeItem => _playerInventory?.ActiveItem;

        [FormerlySerializedAs("_root")] [TitleGroup("UI"), SerializeField] private GameObject _uiRoot;
        [TitleGroup("UI"), SerializeField] private Image _imageIcon;
        [TitleGroup("UI"), SerializeField] private UiTimerImageController _timerImageController;
        [TitleGroup("UI"), SerializeField] private UiTextIntFormatter _remainingCountTextController;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _playerInventory.OnActiveItemAdded += OnActiveItemChanged;
            _playerInventory.OnActiveItemRemoved += OnActiveItemChanged;
            UpdateAssignedItem();
        }

        private void OnDisable()
        {
            
            _playerInventory.OnActiveItemAdded -= OnActiveItemChanged;
            _playerInventory.OnActiveItemRemoved -= OnActiveItemChanged;
        }

        #endregion

        private void OnActiveItemChanged(PlayerItem playerItem)
        {
            UpdateAssignedItem();
        }
        
        private void UpdateAssignedItem()
        {
            if (_activeItem == null)
            {
                _uiRoot.SetActive(false);
                return;
            }
            
            _uiRoot.SetActive(true);
            _imageIcon.sprite = _activeItem.Icon;
            _imageIcon.color = (_activeItem.UseIconColor ? _activeItem.IconColor : Color.white);
            
            var itemActivationTimer = _activeItem.ItemEffects.First(e => e.UseActivationCooldownTimer).ActivationCooldownTimer;
            if (_timerImageController == null)
            {
                _timerImageController.gameObject.SetActive(false);
            }
            else
            {
                _timerImageController.gameObject.SetActive(true);
                _timerImageController.SetTimerVariable(itemActivationTimer);
            }
            
            var remainingActivationsVariable = _activeItem.ItemEffects.FirstOrDefault(e => e.UseActivationLimit)?.RemainingActivations;
            if (remainingActivationsVariable == null)
            {
                _remainingCountTextController.gameObject.SetActive(false);
            }
            else
            {
                _remainingCountTextController.gameObject.SetActive(true);
                _remainingCountTextController.SetVariable(remainingActivationsVariable);
            }
        }
    }
}