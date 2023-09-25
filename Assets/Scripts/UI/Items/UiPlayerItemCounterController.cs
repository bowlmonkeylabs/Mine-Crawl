using System;
using System.Linq;
using BML.Scripts.ItemTreeGraph;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BML.Scripts.UI.Items
{
    [ExecuteAlways]
    public class UiPlayerItemCounterController : MonoBehaviour
    {
        #region Inspector

        private enum ItemSource
        {
            PlayerItem,
            PlayerInventory,
        }

        [TitleGroup("Item")]
        [SerializeField, OnValueChanged("UpdateAssignedItem")] private ItemSource _itemSource;
        [SerializeField, OnValueChanged("UpdateAssignedItem"), ShowIf("@_itemSource == ItemSource.PlayerInventory")] private ItemType _inventoryItemType;
        [SerializeField, OnValueChanged("UpdateAssignedItem"), ShowIf("@_itemSource == ItemSource.PlayerInventory")] 
        private PlayerInventory _playerInventory;
        [ShowIf("@_itemSource == ItemSource.PlayerInventory")]
        public PlayerInventory PlayerInventory
        {
            get => _playerInventory;
            set
            {
                _playerInventory = value;
            }
        }
        [SerializeField, ShowIf("@_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable")] private int _inventoryPassiveStackableTreeSlotIndex;
        [SerializeField, OnValueChanged("UpdateAssignedItem"), ShowIf("@_itemSource == ItemSource.PlayerItem")] private PlayerItem _playerItem;

        [ShowInInspector,
         ShowIf("@_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable"),
         ReadOnly]
        public ItemTreeGraphStartNode InventoryPassiveStackableTreeStartNode
        {
            get
            {
                if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
                {
                    if (_playerInventory == null || _inventoryPassiveStackableTreeSlotIndex >=
                        _playerInventory.PassiveStackableItemTrees.Count)
                    {
                        return null;
                    }
                    var passiveStackableTree =  _playerInventory.PassiveStackableItemTrees[_inventoryPassiveStackableTreeSlotIndex];
                    return passiveStackableTree;
                }
                return null;
            }
        }
        
        [ShowInInspector, ShowIf("@_itemSource == ItemSource.PlayerInventory"), ReadOnly] public PlayerItem Item
        {
            get
            {
                switch (_itemSource)
                {
                    case ItemSource.PlayerItem:
                        return _playerItem;
                        break;
                    default:
                    case ItemSource.PlayerInventory:
                        switch (_inventoryItemType)
                        {
                            case ItemType.Active:
                                return _playerInventory?.ActiveItem;
                            case ItemType.Passive:
                                return _playerInventory?.PassiveItem;
                            case ItemType.PassiveStackable:
                                // TODO we probably want to have the representative information defined on the tree start node, but for now it's easier to just display the first item in the tree
                                return InventoryPassiveStackableTreeStartNode?.FirstItemInTree;
                            default:
                                return null;
                                break;
                        }
                        break;
                }
            }
            set
            {
                _playerItem = value;
                _itemSource = ItemSource.PlayerItem;
            }
        }

        public void SetDisplayPassiveStackableTreeSlotFromInventory(int passiveStackableTreeSlotIndex)
        {
            _inventoryItemType = ItemType.PassiveStackable;
            _inventoryPassiveStackableTreeSlotIndex = passiveStackableTreeSlotIndex;
            _itemSource = ItemSource.PlayerInventory;
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
            UpdatePassiveStackableTreeCounts();
            _playerInventory.OnActiveItemAdded += OnActiveItemChanged;
            _playerInventory.OnActiveItemRemoved += OnActiveItemChanged;
            _playerInventory.OnPassiveItemAdded += OnPassiveItemChanged;
            _playerInventory.OnPassiveItemRemoved += OnPassiveItemChanged;
            _playerInventory.OnPassiveStackableItemChanged += OnPassiveStackableItemChanged;
            _playerInventory.OnPassiveStackableItemTreeChanged += OnPassiveStackableItemTreeChanged;
            _timerImageController?.Timer?.Subscribe(OnItemActivationTimerUpdated);
            _timerImageController?.Timer?.SubscribeFinished(OnItemActivationTimerUpdated);
            UpdateAssignedItem();
        }

        private void OnDisable()
        {
            _playerInventory.OnActiveItemAdded -= OnActiveItemChanged;
            _playerInventory.OnActiveItemRemoved -= OnActiveItemChanged;
            _playerInventory.OnPassiveItemAdded -= OnPassiveItemChanged;
            _playerInventory.OnPassiveItemRemoved -= OnPassiveItemChanged;
            _playerInventory.OnPassiveStackableItemChanged -= OnPassiveStackableItemChanged;
            _playerInventory.OnPassiveStackableItemTreeChanged -= OnPassiveStackableItemTreeChanged;
            _timerImageController?.Timer?.Unsubscribe(OnItemActivationTimerUpdated);
            _timerImageController?.Timer?.UnsubscribeFinished(OnItemActivationTimerUpdated);
        }

        #endregion
        
        #region On data updated callbacks

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

        private void OnActiveItemChanged(PlayerItem playerItem)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.Active)
            {
                UpdateAssignedItem();
            }
        }
        
        private void OnPassiveItemChanged(PlayerItem playerItem)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.Passive)
            {
                UpdateAssignedItem();
            }
        }
        
        private void OnPassiveStackableItemChanged()
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdatePassiveStackableTreeCounts();
            }
        }
        
        private void OnPassiveStackableItemTreeChanged()
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdateAssignedItem();
            }
        }
        
        #endregion
        
        private void UpdateAssignedItem()
        {
            if (Item == null)
            {
                _uiRoot.SetActive(false);
                return;
            }
            
            _uiRoot.SetActive(true);
            _imageIcon.sprite = Item.Icon;
            _imageIcon.color = (Item.UseIconColor ? Item.IconColor : Color.white);
            
            var itemActivationTimer = Item.ItemEffects.FirstOrDefault(e => e.UseActivationCooldownTimer)?.ActivationCooldownTimer;
            if (itemActivationTimer == null)
            {
                _timerImageController.gameObject.SetActive(false);
            }
            else
            {
                _timerImageController.gameObject.SetActive(true);
                _timerImageController.SetTimerVariable(itemActivationTimer);
            }

            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                // for passive stackable items, instead of 'remaining activations count' it should display the number of upgrades acquired in the respective tree.
                _remainingCountTextController.SetVariable(null);
            }
            else
            {
                var remainingActivationsVariable = Item.ItemEffects.FirstOrDefault(e => e.UseActivationLimit)?.RemainingActivations;
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

            _bindingHintText.gameObject.SetActive(Item.Type == ItemType.Active);

            _itemTypeText.text = Item.Type.ToString().ToUpper();
            _itemTypeText.gameObject.SetActive(Item.Type == ItemType.Passive);
        }

        private void UpdatePassiveStackableTreeCounts()
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                if (InventoryPassiveStackableTreeStartNode != null)
                {
                    _remainingCountTextController.gameObject.SetActive(true);
                    _remainingCountTextController.SetConstant(InventoryPassiveStackableTreeStartNode.NumberOfObtainedItemsInTree);
                }
                else
                {
                    _remainingCountTextController.gameObject.SetActive(false);
                }
            }
        }
    }
}