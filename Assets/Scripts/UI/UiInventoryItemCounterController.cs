using System;
using System.Linq;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    [ExecuteAlways]
    public class UiInventoryItemCounterController : MonoBehaviour
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
        [SerializeField, OnValueChanged("UpdateAssignedItem")] private ItemType _itemType;
        [TitleGroup("Item"), ShowInInspector, ReadOnly] private PlayerItem _item
        {
            get
            {
                switch (_itemType)
                {
                    case ItemType.Active:
                        return _playerInventory?.ActiveItem;
                    case ItemType.Passive:
                        return _playerInventory?.PassiveItem;
                    case ItemType.PassiveStackable: 
                    default:
                        return null;
                        break;
                }
            }
        }

        [FormerlySerializedAs("_root")] [TitleGroup("UI"), SerializeField] private GameObject _uiRoot;
        [TitleGroup("UI"), SerializeField] private Image _imageIcon;
        [TitleGroup("UI"), SerializeField] private UiTimerImageController _timerImageController;
        [TitleGroup("UI"), SerializeField] private UiTextIntFormatter _remainingCountTextController;
        [TitleGroup("UI"), SerializeField] private TMP_Text _bindingHintText;
        [TitleGroup("UI"), SerializeField] private TMP_Text _itemTypeText;
        private Color _bindingHintOriginalColor;
        private Color _bindingHintInactiveColor;

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            _bindingHintOriginalColor = _bindingHintText.color;
            _bindingHintInactiveColor = new Color(_bindingHintOriginalColor.r, _bindingHintOriginalColor.g, _bindingHintOriginalColor.b, 0.5f);
        }

        private void OnEnable()
        {
            _playerInventory.OnActiveItemAdded += OnInventoryUpdated;
            _playerInventory.OnActiveItemRemoved += OnInventoryUpdated;
            _playerInventory.OnPassiveItemAdded += OnInventoryUpdated;
            _playerInventory.OnPassiveItemRemoved += OnInventoryUpdated;
            _timerImageController?.Timer?.Subscribe(OnItemActivationTimerUpdated);
            _timerImageController?.Timer?.SubscribeFinished(OnItemActivationTimerUpdated);
            UpdateAssignedItem();
        }

        private void OnDisable()
        {
            
            _playerInventory.OnActiveItemAdded -= OnInventoryUpdated;
            _playerInventory.OnActiveItemRemoved -= OnInventoryUpdated;
            _playerInventory.OnPassiveItemAdded -= OnInventoryUpdated;
            _playerInventory.OnPassiveItemRemoved -= OnInventoryUpdated;
            _timerImageController?.Timer?.Unsubscribe(OnItemActivationTimerUpdated);
            _timerImageController?.Timer?.UnsubscribeFinished(OnItemActivationTimerUpdated);
        }

        #endregion

        private void OnItemActivationTimerUpdated()
        {
            if (_timerImageController.Timer.IsStarted && !_timerImageController.Timer.IsFinished)
            {
                _bindingHintText.color = _bindingHintInactiveColor;
            }
            else
            {
                _bindingHintText.color = _bindingHintOriginalColor;
            }
        }

        private void OnInventoryUpdated(PlayerItem playerItem)
        {
            UpdateAssignedItem();
        }
        
        private void UpdateAssignedItem()
        {
            if (_item == null)
            {
                _uiRoot.SetActive(false);
                return;
            }
            
            _uiRoot.SetActive(true);
            _imageIcon.sprite = _item.Icon;
            _imageIcon.color = (_item.UseIconColor ? _item.IconColor : Color.white);
            
            var itemActivationTimer = _item.ItemEffects.FirstOrDefault(e => e.UseActivationCooldownTimer)?.ActivationCooldownTimer;
            if (itemActivationTimer == null)
            {
                _timerImageController.gameObject.SetActive(false);
            }
            else
            {
                _timerImageController.gameObject.SetActive(true);
                _timerImageController.SetTimerVariable(itemActivationTimer);
            }
            
            var remainingActivationsVariable = _item.ItemEffects.FirstOrDefault(e => e.UseActivationLimit)?.RemainingActivations;
            if (remainingActivationsVariable == null)
            {
                _remainingCountTextController.gameObject.SetActive(false);
            }
            else
            {
                _remainingCountTextController.gameObject.SetActive(true);
                _remainingCountTextController.SetVariable(remainingActivationsVariable);
            }

            _bindingHintText.gameObject.SetActive(_item.Type == ItemType.Active);

            _itemTypeText.text = _item.Type.ToString().ToUpper();
            _itemTypeText.gameObject.SetActive(_item.Type == ItemType.Passive);
        }
    }
}