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
        [FormerlySerializedAs("_inventoryActiveSlotIndex")] [SerializeField, OnValueChanged("UpdateAssignedItem"), ShowIf("@_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.Active")] private int _inventoryActiveItemIndex;
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
                                if (_playerInventory == null || _inventoryActiveItemIndex >=
                                    _playerInventory.ActiveItems.Count)
                                {
                                    return null;
                                }
                                return _playerInventory.ActiveItems[_inventoryActiveItemIndex];
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
            
            _playerInventory.OnActiveItemAdded += OnActiveItemChanged;
            _playerInventory.OnActiveItemRemoved += OnActiveItemChanged;
            _playerInventory.OnActiveItemChanged += OnActiveItemListChanged;
            
            _playerInventory.OnPassiveItemAdded += OnPassiveItemChanged;
            _playerInventory.OnPassiveItemRemoved += OnPassiveItemChanged;
            
            _playerInventory.OnPassiveStackableItemAdded += OnPassiveStackableItemAdded;
            _playerInventory.OnPassiveStackableItemRemoved += OnPassiveStackableItemRemoved;
            _playerInventory.OnPassiveStackableItemChanged += OnPassiveStackableItemChanged;

            _playerInventory.OnPassiveStackableItemTreeAdded += OnPassiveStackableItemTreeAdded;
            _playerInventory.OnPassiveStackableItemTreeRemoved += OnPassiveStackableItemTreeRemoved;
            _playerInventory.OnPassiveStackableItemTreeChanged += OnPassiveStackableItemTreeChanged;
            
            _timerImageController?.Timer?.Subscribe(OnItemActivationTimerUpdated);
            _timerImageController?.Timer?.SubscribeFinished(OnItemActivationTimerUpdated);
            
            UpdateAssignedItem();
        }

        private void OnDisable()
        {
            _playerInventory.OnActiveItemAdded -= OnActiveItemChanged;
            _playerInventory.OnActiveItemRemoved -= OnActiveItemChanged;
            _playerInventory.OnActiveItemChanged -= OnActiveItemListChanged;
            
            _playerInventory.OnPassiveItemAdded -= OnPassiveItemChanged;
            _playerInventory.OnPassiveItemRemoved -= OnPassiveItemChanged;
            
            _playerInventory.OnPassiveStackableItemAdded -= OnPassiveStackableItemAdded;
            _playerInventory.OnPassiveStackableItemRemoved -= OnPassiveStackableItemRemoved;
            _playerInventory.OnPassiveStackableItemChanged -= OnPassiveStackableItemChanged;
            
            _playerInventory.OnPassiveStackableItemTreeAdded -= OnPassiveStackableItemTreeAdded;
            _playerInventory.OnPassiveStackableItemTreeRemoved -= OnPassiveStackableItemTreeRemoved;
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
        
        private void OnActiveItemListChanged()
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
        
        private void OnPassiveStackableItemAdded(PlayerItem playerItem)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdatePassiveStackableTreeCounts(playerItem.PassiveStackableTreeStartNode, 1);
            }
        }
        
        private void OnPassiveStackableItemRemoved(PlayerItem playerItem)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdatePassiveStackableTreeCounts(playerItem.PassiveStackableTreeStartNode, -1);
            }
        }
        
        private void OnPassiveStackableItemChanged()
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
        
        private void OnPassiveStackableItemTreeChanged()
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdateAssignedItem();
            }
        }
        
        #endregion
        
        private static string ActiveItemBindingHint(int index) => $"<style=Player/UseActiveItem{index + 1}>";
        
        private void UpdateAssignedItem()
        {
            if (Item == null)
            {
                _uiRoot.SetActive(false);
                return;
            }
            
            _imageIcon.sprite = Item.Icon;
            _imageIcon.color = (Item.UseIconColor ? Item.IconColor : Color.white);

            // 'Recurring Timer' will take priority display; if null, then 'Activation Cooldown Timer' will be shown. This works with our current requirements, but may need to change in the future.
            var recurringTimer = Item.ItemEffects
                .FirstOrDefault(e => e.Trigger == ItemEffectTrigger.RecurringTimer)
                ?.RecurringTimerForTrigger;
            TimerVariable itemTimer = null;
            if (recurringTimer != null)
            {
                _timerDisplayMode = ItemEffectTimerDisplayMode.RecurringTimer;
                itemTimer = recurringTimer;
            }
            else
            {
                var activationCooldownTimer = Item.ItemEffects
                    .FirstOrDefault(e => e.UseActivationCooldownTimer)
                    ?.ActivationCooldownTimer;
                if (activationCooldownTimer != null)
                {
                    _timerDisplayMode = ItemEffectTimerDisplayMode.ActivationCooldownTimer;
                    itemTimer = activationCooldownTimer;
                }
            }
            
            if (itemTimer == null)
            {
                _timerImageController.gameObject.SetActive(false);
            }
            else
            {
                _timerImageController.SetTimerVariable(itemTimer);
                _timerImageController.gameObject.SetActive(true);
            }

            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                // for passive stackable items, instead of 'remaining activations count' it should display the number of upgrades acquired in the respective tree.
                // _remainingCountTextController.SetVariable(null); // clear prev variable to unsubscribe from its events
                UpdatePassiveStackableTreeCounts();
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
                    remainingActivationsVariable.Unsubscribe(OnCountValueChanged);
                    remainingActivationsVariable.Subscribe(OnCountValueChanged);
                    _remainingCountTextController.SetVariable(remainingActivationsVariable);
                    _remainingCountTextController.gameObject.SetActive(true);
                }
            }
            
            _bindingHintText.gameObject.SetActive(Item.Type == ItemType.Active);
            if (Item.Type == ItemType.Active)
            {
                _bindingHintText.SetText(ActiveItemBindingHint(_inventoryActiveItemIndex));
            }

            // _itemTypeText.text = Item.Type.ToString().ToUpper();
            // _itemTypeText.gameObject.SetActive(Item.Type == ItemType.Passive);
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