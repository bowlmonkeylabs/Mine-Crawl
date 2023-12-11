using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.ItemTreeGraph;
using BML.Scripts.Player.Items;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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
        [SerializeField, OnValueChanged("UpdateAssignedItem"), ShowIf("@_itemSource == ItemSource.PlayerInventory")] private int _inventoryItemSlotIndex;
        [SerializeField, OnValueChanged("UpdateAssignedItem"), ShowIf("@_itemSource == ItemSource.PlayerInventory")] private PlayerInventory _playerInventory;
        [ShowIf("@_itemSource == ItemSource.PlayerInventory")]
        public PlayerInventory PlayerInventory
        {
            get => _playerInventory;
            set
            {
                _playerInventory = value;
            }
        }
        [SerializeField, OnValueChanged("UpdateAssignedItem"), ShowIf("@_itemSource == ItemSource.PlayerItem")] private PlayerItem _item;

        [SerializeField] private bool _isStoreDisplay = false;

        [ShowInInspector,
         ShowIf("@_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable"),
         ReadOnly]
        public ItemTreeGraphStartNode InventoryPassiveStackableTreeStartNode
        {
            get
            {
                if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
                {
                    if (_playerInventory == null || _inventoryItemSlotIndex >=
                        _playerInventory.PassiveStackableItemTrees.ItemCount)
                    {
                        return null;
                    }
                    var passiveStackableTree =  _playerInventory.PassiveStackableItemTrees[_inventoryItemSlotIndex];
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
                        return _item;
                        break;
                    default:
                    case ItemSource.PlayerInventory:
                        switch (_inventoryItemType)
                        {
                            case ItemType.Active:
                                if (_playerInventory == null || _inventoryItemSlotIndex >=
                                    _playerInventory.ActiveItems.ItemCount)
                                {
                                    return null;
                                }
                                return _playerInventory.ActiveItems[_inventoryItemSlotIndex];
                            case ItemType.Consumable:
                                if (_playerInventory == null || _inventoryItemSlotIndex >=
                                    _playerInventory.ConsumableItems.ItemCount)
                                {
                                    return null;
                                }
                                return _playerInventory.ConsumableItems[_inventoryItemSlotIndex];
                            case ItemType.Passive:
                                if (_playerInventory == null || _inventoryItemSlotIndex >=
                                    _playerInventory.PassiveItems.ItemCount)
                                {
                                    return null;
                                }
                                return _playerInventory.PassiveItems[_inventoryItemSlotIndex];
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
                _item = value;
                _itemSource = ItemSource.PlayerItem;
            }
        }

        public void SetDisplayPassiveStackableTreeSlotFromInventory(int passiveStackableTreeSlotIndex)
        {
            _inventoryItemType = ItemType.PassiveStackable;
            _inventoryItemSlotIndex = passiveStackableTreeSlotIndex;
            _itemSource = ItemSource.PlayerInventory;
        }

        private enum ItemEffectTimerDisplayMode
        {
            None,
            ActivationCooldownTimer,
            RecurringTimer,
        }

        [FormerlySerializedAs("_root")] [TitleGroup("UI"), SerializeField] private GameObject _uiRoot;
        [TitleGroup("UI"), SerializeField] private Image _imageIcon;
        
        [TitleGroup("UI"), ShowInInspector, ReadOnly] private ItemEffectTimerDisplayMode _timerDisplayMode;
        [TitleGroup("UI"), SerializeField] private UiTimerImageController _timerImageController;
        [TitleGroup("UI"), SerializeField] private UiTextIntFormatter _remainingCountTextController;
        [TitleGroup("UI"), SerializeField] private MMF_Player _remainingCountIncrementFeedbacks;
        [TitleGroup("UI"), SerializeField] private MMF_Player _remainingCountDecrementFeedbacks;
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
            
            _playerInventory.ActiveItems.OnItemAdded += OnActiveItemChanged;
            _playerInventory.ActiveItems.OnItemRemoved += OnActiveItemChanged;
            _playerInventory.ActiveItems.OnAnyItemChangedInInspector += OnActiveItemListChangedInInspector;
            
            _playerInventory.PassiveItems.OnItemAdded += OnPassiveItemChanged;
            _playerInventory.PassiveItems.OnItemRemoved += OnPassiveItemChanged;
            
            _playerInventory.PassiveStackableItems.OnItemAdded += OnPassiveStackableItemAdded;
            _playerInventory.PassiveStackableItems.OnItemRemoved += OnPassiveStackableItemRemoved;
            _playerInventory.PassiveStackableItems.OnAnyItemChangedInInspector += OnPassiveStackableItemChangedInInspector;

            _playerInventory.PassiveStackableItemTrees.OnItemAdded += OnPassiveStackableItemTreeAdded;
            _playerInventory.PassiveStackableItemTrees.OnItemRemoved += OnPassiveStackableItemTreeRemoved;
            _playerInventory.PassiveStackableItemTrees.OnAnyItemChangedInInspector += OnPassiveStackableItemTreeChangedInInspector;
            
            _timerImageController?.Timer?.Subscribe(OnItemActivationTimerUpdated);
            _timerImageController?.Timer?.SubscribeFinished(OnItemActivationTimerUpdated);
            
            UpdateAssignedItem();
        }

        private void OnDisable()
        {
            _playerInventory.ActiveItems.OnItemAdded -= OnActiveItemChanged;
            _playerInventory.ActiveItems.OnItemRemoved -= OnActiveItemChanged;
            _playerInventory.ActiveItems.OnAnyItemChangedInInspector -= OnActiveItemListChangedInInspector;
            
            _playerInventory.PassiveItems.OnItemAdded -= OnPassiveItemChanged;
            _playerInventory.PassiveItems.OnItemRemoved -= OnPassiveItemChanged;
            
            _playerInventory.PassiveStackableItems.OnItemAdded -= OnPassiveStackableItemAdded;
            _playerInventory.PassiveStackableItems.OnItemRemoved -= OnPassiveStackableItemRemoved;
            _playerInventory.PassiveStackableItems.OnAnyItemChangedInInspector -= OnPassiveStackableItemChangedInInspector;
            
            _playerInventory.PassiveStackableItemTrees.OnItemAdded -= OnPassiveStackableItemTreeAdded;
            _playerInventory.PassiveStackableItemTrees.OnItemRemoved -= OnPassiveStackableItemTreeRemoved;
            _playerInventory.PassiveStackableItemTrees.OnAnyItemChangedInInspector -= OnPassiveStackableItemTreeChangedInInspector;
            
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

        private void OnActiveItemChanged(PlayerItem item)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.Active)
            {
                UpdateAssignedItem();
            }
        }
        
        private void OnActiveItemListChangedInInspector()
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.Active)
            {
                UpdateAssignedItem();
            }
        }
        
        private void OnPassiveItemChanged(PlayerItem item)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.Passive)
            {
                UpdateAssignedItem();
            }
        }
        
        private void OnPassiveStackableItemAdded(PlayerItem item)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdatePassiveStackableTreeCounts(item.PassiveStackableTreeStartNode, 1);
            }
        }
        
        private void OnPassiveStackableItemRemoved(PlayerItem item)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdatePassiveStackableTreeCounts(item.PassiveStackableTreeStartNode, -1);
            }
        }
        
        private void OnPassiveStackableItemChangedInInspector()
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdatePassiveStackableTreeCounts();
            }
        }
        
        private void OnPassiveStackableItemTreeAdded(ItemTreeGraphStartNode treeStartNode)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdateAssignedItem();
            }
        }
        
        private void OnPassiveStackableItemTreeRemoved(ItemTreeGraphStartNode treeStartNode)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdateAssignedItem();
            }
        }
        
        private void OnPassiveStackableItemTreeChangedInInspector()
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdateAssignedItem();
            }
        }
        
        #endregion
        
        private static string ActiveItemBindingHint(int index) => $"<style=Player/UseActiveItem{index + 1}>";
        private static string ConsumableItemBindingHint(int index) => $"<style=Player/UseConsumableItem{index + 1}>";
        
        private void UpdateAssignedItem()
        {
            var item = Item;
            if (item == null)
            {
                _uiRoot.SetActive(false);
                return;
            }
            
            _imageIcon.sprite = item.Icon;
            _imageIcon.color = (item.UseIconColor ? item.IconColor : Color.white);

            TimerVariable itemTimer = null;
            // 'Recurring Timer' will take priority display; if null, then 'Activation Cooldown Timer' will be shown. This works with our current requirements, but may need to change in the future.
            var recurringTimer = item.ItemEffects
                .FirstOrDefault(e => e.Trigger == ItemEffectTrigger.RecurringTimer)
                ?.RecurringTimerForTrigger;
            if (recurringTimer != null)
            {
                _timerDisplayMode = ItemEffectTimerDisplayMode.RecurringTimer;
                itemTimer = recurringTimer;
            }
            else
            {
                var activationCooldownTimer = item.ItemEffects
                    .FirstOrDefault(e => e.UseActivationCooldownTimer)
                    ?.ActivationCooldownTimer;
                if (activationCooldownTimer != null)
                {
                    _timerDisplayMode = ItemEffectTimerDisplayMode.ActivationCooldownTimer;
                    itemTimer = activationCooldownTimer;
                }
            }
            if (itemTimer == null || _isStoreDisplay)
            {
                _timerImageController.gameObject.SetActive(false);
            }
            else
            {
                _timerImageController.SetTimerVariable(itemTimer);
                _timerImageController.gameObject.SetActive(true);
            }

            if (_isStoreDisplay)
            {
                _remainingCountTextController.gameObject.SetActive(false);
            }
            else
            {
                if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
                {
                    // for passive stackable items, instead of 'remaining activations count' it should display the number of upgrades acquired in the respective tree.
                    // _remainingCountTextController.SetVariable(null); // clear prev variable to unsubscribe from its events
                    UpdatePassiveStackableTreeCounts();
                }
                else
                {
                    var remainingActivationsVariable = item.ItemEffects.FirstOrDefault(e => e.UseActivationLimit)?.RemainingActivations;
                    if (remainingActivationsVariable == null)
                    {
                        _remainingCountTextController.gameObject.SetActive(false);
                    }
                    else
                    {
                        remainingActivationsVariable.Unsubscribe(OnCountValueChanged);
                        remainingActivationsVariable.Subscribe(OnCountValueChanged);
                        _remainingCountTextController.SetVariable(remainingActivationsVariable);
                        _remainingCountTextController.gameObject.SetActive(true);
                    }
                }
            }
            
            _bindingHintText.gameObject.SetActive(!_isStoreDisplay && (_inventoryItemType == ItemType.Active || _inventoryItemType == ItemType.Consumable));
            if (_inventoryItemType == ItemType.Active)
            {
                _bindingHintText.SetText(ActiveItemBindingHint(_inventoryItemSlotIndex));
            } else if (_inventoryItemType == ItemType.Consumable)
            {
                _bindingHintText.SetText(ConsumableItemBindingHint(_inventoryItemSlotIndex));
            }

            // _itemTypeText.text = item.Type.ToString().ToUpper();
            // _itemTypeText.gameObject.SetActive(item.Type == ItemType.Passive);
            _itemTypeText.gameObject.SetActive(false);
            
            _uiRoot.SetActive(true);
        }

        private void OnCountValueChanged(int prev, int curr)
        {
            if (curr < prev)
            {
                if (!_remainingCountDecrementFeedbacks.SafeIsUnityNull())
                {
                    _remainingCountDecrementFeedbacks.PlayFeedbacks();
                }
            }
            else if (curr > prev)
            {
                if (!_remainingCountIncrementFeedbacks.SafeIsUnityNull())
                {
                    _remainingCountIncrementFeedbacks.PlayFeedbacks();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeStartNode"></param>
        /// <param name="signOfChange">Used to determine whether to play feedbacks for value incrementing or decrementing.</param>
        private void UpdatePassiveStackableTreeCounts(ItemTreeGraphStartNode treeStartNode = null, int signOfChange = 0)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                if (InventoryPassiveStackableTreeStartNode != null)
                {
                    if (treeStartNode != null && treeStartNode != InventoryPassiveStackableTreeStartNode)
                    {
                        return;
                    }
                    
                    _remainingCountTextController.gameObject.SetActive(true);
                    _remainingCountTextController.SetConstant(InventoryPassiveStackableTreeStartNode.NumberOfObtainedItemsInTree);
                    
                    // Since we don't track what the previous value was, this is a hack to always play the 'increment' feedbacks; in normal gameplay, the # of passive stackable upgrades should only ever go up, I think...
                    OnCountValueChanged(InventoryPassiveStackableTreeStartNode.NumberOfObtainedItemsInTree, 
                        InventoryPassiveStackableTreeStartNode.NumberOfObtainedItemsInTree + signOfChange);
                }
                else
                {
                    _remainingCountTextController.gameObject.SetActive(false);
                }
            }
        }
    }
}