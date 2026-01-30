using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.ItemTreeGraph;
using BML.Scripts.Player;
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

        [ShowInInspector, ReadOnly]
        private string _debugIdentifier => $"[UiPlayerItemCounterController: Source={_itemSource}, ItemType={_inventoryItemType}, SlotIndex={_inventoryItemSlotIndex}]";

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
                        _playerInventory.PassiveStackableItemTrees.SlotCount)
                    {
                        return null;
                    }
                    var passiveStackableTree =  _playerInventory.PassiveStackableItemTrees[_inventoryItemSlotIndex];
                    return passiveStackableTree;
                }
                return null;
            }
        }

        private PlayerItem GetSlotHelper(ItemSlotType<PlayerItem, SlotTypeFilter> itemSlotType)
        {
            if (itemSlotType == null || _inventoryItemSlotIndex >= itemSlotType.SlotCount)
            {
                return null;
            }
            return itemSlotType[_inventoryItemSlotIndex];
        }
        
        [ShowInInspector, ShowIf("@_itemSource == ItemSource.PlayerInventory"), ReadOnly] public PlayerItem Item
        {
            get
            {
                switch (_itemSource)
                {
                    case ItemSource.PlayerItem:
                        return _item;
                    default:
                    case ItemSource.PlayerInventory:
                        switch (_inventoryItemType)
                        {
                            case ItemType.PassiveStackable:
                                // TODO we probably want to have the representative information defined on the tree start node, but for now it's easier to just display the first item in the tree
                                return InventoryPassiveStackableTreeStartNode?.FirstItemInTree;
                            case ItemType.Passive:
                                return GetSlotHelper(_playerInventory.PassiveItems);
                            case ItemType.Active:
                                return GetSlotHelper(_playerInventory.ActiveItems);
                            case ItemType.Consumable:
                                return GetSlotHelper(_playerInventory.ConsumableItems);
                            default:
                                return null;
                        }
                }
            }
            set
            {
                _item = value;
                _itemSource = ItemSource.PlayerItem;
                UpdateAssignedItem();
                TryPlayItemChangedFeedbacks(value);
            }
        }

        public void SetDisplayItemFromPlayerInventory(ItemType inventoryItemType, int inventoryItemSlotIndex)
        {
            _inventoryItemType = inventoryItemType;
            _inventoryItemSlotIndex = inventoryItemSlotIndex;
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
        [TitleGroup("UI"), SerializeField] private Shadow _iconShadow;
        
        [TitleGroup("UI"), SerializeField] private MMF_Player _itemChangedFeedbacks;
        // Disable warning for unused field
        #pragma warning disable 414
        [TitleGroup("UI"), ShowInInspector, ReadOnly] private ItemEffectTimerDisplayMode _timerDisplayMode;
        #pragma warning restore 414
        [TitleGroup("UI"), SerializeField] private UiTimerImageController _timerImageController;
        [TitleGroup("UI"), SerializeField] private UiTextIntFormatter _remainingCountTextController;
        [TitleGroup("UI"), SerializeField] private Transform _remainingCountShadowTransform;
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

            if (_itemSource == ItemSource.PlayerInventory)
            {
                switch (_inventoryItemType)
                {
                    case ItemType.PassiveStackable:
                        _playerInventory.PassiveStackableItems.OnItemAdded += OnPassiveStackableItemAdded;
                        _playerInventory.PassiveStackableItems.OnItemRemoved += OnPassiveStackableItemRemoved;
                        _playerInventory.PassiveStackableItems.OnAnyItemChangedInInspector += OnPassiveStackableItemChangedInInspector;

                        _playerInventory.PassiveStackableItemTrees.OnItemAdded += OnPassiveStackableItemTreeAdded;
                        _playerInventory.PassiveStackableItemTrees.OnItemRemoved += OnPassiveStackableItemTreeRemoved;
                        _playerInventory.PassiveStackableItemTrees.OnAnyItemChangedInInspector += OnPassiveStackableItemTreeChangedInInspector;
                        break;
                    case ItemType.Passive:
                        _playerInventory.PassiveItems.OnItemAdded += OnPassiveItemChanged;
                        _playerInventory.PassiveItems.OnItemRemoved += OnPassiveItemChanged;
                        break;
                    case ItemType.Active:
                        _playerInventory.ActiveItems.OnItemAdded += OnActiveItemChanged;
                        _playerInventory.ActiveItems.OnItemRemoved += OnActiveItemChanged;
                        _playerInventory.ActiveItems.OnAnyItemChangedInInspector += OnActiveItemListChangedInInspector;
                        break;
                    case ItemType.Consumable:
                        _playerInventory.ConsumableItems.OnItemAdded += OnConsumableItemChanged;
                        _playerInventory.ConsumableItems.OnItemRemoved += OnConsumableItemChanged;
                        _playerInventory.ConsumableItems.OnAnyItemChangedInInspector += OnConsumableItemListChangedInInspector;
                        break;
                }

                _playerInventory.OnReset += OnPlayerInventoryReset;
                // _playerInventory.OnAnyPlayerItemChangedInInspector += UpdateAssignedItem;
            }
            
            _timerImageController?.Timer?.Subscribe(OnItemActivationTimerUpdated);
            _timerImageController?.Timer?.SubscribeFinished(OnItemActivationTimerUpdated);
            
            UpdateAssignedItem();
        }

        private void OnDisable()
        {
            if (_itemSource == ItemSource.PlayerInventory)
            {
                switch (_inventoryItemType)
                {
                    case ItemType.PassiveStackable:
                        _playerInventory.PassiveStackableItems.OnItemAdded -= OnPassiveStackableItemAdded;
                        _playerInventory.PassiveStackableItems.OnItemRemoved -= OnPassiveStackableItemRemoved;
                        _playerInventory.PassiveStackableItems.OnAnyItemChangedInInspector -= OnPassiveStackableItemChangedInInspector;
            
                        _playerInventory.PassiveStackableItemTrees.OnItemAdded -= OnPassiveStackableItemTreeAdded;
                        _playerInventory.PassiveStackableItemTrees.OnItemRemoved -= OnPassiveStackableItemTreeRemoved;
                        _playerInventory.PassiveStackableItemTrees.OnAnyItemChangedInInspector -= OnPassiveStackableItemTreeChangedInInspector;
                        break;
                    case ItemType.Passive:
                        _playerInventory.PassiveItems.OnItemAdded -= OnPassiveItemChanged;
                        _playerInventory.PassiveItems.OnItemRemoved -= OnPassiveItemChanged;
                        break;
                    case ItemType.Active:
                        _playerInventory.ActiveItems.OnItemAdded -= OnActiveItemChanged;
                        _playerInventory.ActiveItems.OnItemRemoved -= OnActiveItemChanged;
                        _playerInventory.ActiveItems.OnAnyItemChangedInInspector -= OnActiveItemListChangedInInspector;
                        break;
                    case ItemType.Consumable:
                        _playerInventory.ConsumableItems.OnItemAdded -= OnConsumableItemChanged;
                        _playerInventory.ConsumableItems.OnItemRemoved -= OnConsumableItemChanged;
                        _playerInventory.ConsumableItems.OnAnyItemChangedInInspector -= OnConsumableItemListChangedInInspector;
                        break;
                }
            
                _playerInventory.OnReset -= OnPlayerInventoryReset;
                // _playerInventory.OnAnyPlayerItemChangedInInspector -= UpdateAssignedItem;
            }
            
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
                TryPlayItemChangedFeedbacks(item);
            }
        }
        
        private void OnActiveItemListChangedInInspector()
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.Active)
            {
                UpdateAssignedItem();
            }
        }
        
        private void OnConsumableItemChanged(PlayerItem item)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.Consumable)
            {
                UpdateAssignedItem();
                TryPlayItemChangedFeedbacks(item);
            }
        }
        
        private void OnConsumableItemListChangedInInspector()
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.Consumable)
            {
                UpdateAssignedItem();
            }
        }
        
        private void OnPassiveItemChanged(PlayerItem item)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.Passive)
            {
                UpdateAssignedItem();
                TryPlayItemChangedFeedbacks(item);
            }
        }
        
        private void OnPassiveStackableItemAdded(PlayerItem item)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdatePassiveStackableTreeCounts(item.PassiveStackableTreeStartNode, 1);
                TryPlayItemChangedFeedbacks(item);
            }
        }
        
        private void OnPassiveStackableItemRemoved(PlayerItem item)
        {
            if (_itemSource == ItemSource.PlayerInventory && _inventoryItemType == ItemType.PassiveStackable)
            {
                UpdatePassiveStackableTreeCounts(item.PassiveStackableTreeStartNode, -1);
                TryPlayItemChangedFeedbacks(item);
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

        private void OnPlayerInventoryReset()
        {
            // Since the inventory reset may be triggered on exiting playmode in the editor, we need to make sure this gameObject still exists before continuing.
            if (gameObject != null)
            {
                UpdateAssignedItem();
            }
        }
        
        #endregion

        #region UI control
        
        private static string ActiveItemBindingHint(int index) => $"<style=Player/UseActiveItem{index + 1}>";
        private static string ConsumableItemBindingHint(int index) => $"<style=Player/UseConsumableItem{index + 1}>";
        
        private void UpdateAssignedItem()
        {
            var item = Item;
            if (item == null && (
                    _isStoreDisplay || (
                        _itemSource == ItemSource.PlayerInventory && 
                        _inventoryItemSlotIndex >= _playerInventory.GetDisplaySlotCount(_inventoryItemType)
                    )
                )
            ) {
                _uiRoot.SetActive(false);
                return;
            }

            _imageIcon.sprite = item?.Icon ?? null;
            _imageIcon.color = (item?.UseIconColor ?? false ? item.IconColor : Color.white);
            _imageIcon.gameObject.SetActive(item != null);

            // Update shadow color to match icon color, but preserve existing brightness from inspector.
            var targetShadowColor = ((item?.UseIconColor ?? false) ? item.IconColor : Color.black);

            Vector3 existingShadowHsv;
            Color.RGBToHSV(_iconShadow.effectColor, out existingShadowHsv.x, out existingShadowHsv.y, out existingShadowHsv.z);

            Vector3 targetShadowHsv;
            Color.RGBToHSV(targetShadowColor, out targetShadowHsv.x, out targetShadowHsv.y, out targetShadowHsv.z);

            targetShadowHsv.z = existingShadowHsv.z; // Preserve existing value (brightness) from inspector

            targetShadowColor = Color.HSVToRGB(targetShadowHsv.x, targetShadowHsv.y, targetShadowHsv.z); // Reconstruct color with preserved brightness
            _iconShadow.effectColor = targetShadowColor;

            TimerVariable itemTimer = null;
            // 'Recurring Timer' will take priority display; if null, then 'Activation Cooldown Timer' will be shown. This works with our current requirements, but may need to change in the future.
            var recurringTimer = item?.ItemEffects
                .FirstOrDefault(e => e.Trigger == ItemEffectTrigger.RecurringTimer)
                ?.RecurringTimerForTrigger;
            if (recurringTimer != null)
            {
                _timerDisplayMode = ItemEffectTimerDisplayMode.RecurringTimer;
                itemTimer = recurringTimer;
            }
            else
            {
                var activationCooldownTimer = item?.ItemEffects
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

            // Reset remaining count text color to white. We were seeing a bug (probably related to MMF_TMPColor feedbacks interpolating the color) where the count text vertex color was getting set to (0,0,0,0) (transparent, nothing) upon restarting. This is a hack to fix that. :)
            _remainingCountTextController.TmpText.color = Color.white;

            if (_isStoreDisplay)
            {
                _remainingCountTextController.gameObject.SetActive(false);
                _remainingCountShadowTransform.gameObject.SetActive(false);
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
                    var remainingActivationsVariable = item?.ItemEffects.FirstOrDefault(e => e.UseActivationLimit)?.RemainingActivations;
                    if (remainingActivationsVariable == null)
                    {
                        _remainingCountTextController.gameObject.SetActive(false);
                        _remainingCountShadowTransform.gameObject.SetActive(false);
                    }
                    else
                    {
                        remainingActivationsVariable.Unsubscribe(OnCountValueChanged);
                        remainingActivationsVariable.Subscribe(OnCountValueChanged);
                        _remainingCountTextController.SetVariable(remainingActivationsVariable);
                        _remainingCountTextController.gameObject.SetActive(true);
                        _remainingCountShadowTransform.gameObject.SetActive(true);
                    }
                }
            }
            
            _bindingHintText.gameObject.SetActive(item != null && !_isStoreDisplay && (_inventoryItemType == ItemType.Active || _inventoryItemType == ItemType.Consumable));
            if (_inventoryItemType == ItemType.Active)
            {
                _bindingHintText.SetText(ActiveItemBindingHint(_inventoryItemSlotIndex));
            } 
            else if (_inventoryItemType == ItemType.Consumable)
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
                    _remainingCountShadowTransform.gameObject.SetActive(true);
                    
                    // Since we don't track what the previous value was, this is a hack to always play the 'increment' feedbacks; in normal gameplay, the # of passive stackable upgrades should only ever go up, I think...
                    OnCountValueChanged(InventoryPassiveStackableTreeStartNode.NumberOfObtainedItemsInTree, 
                        InventoryPassiveStackableTreeStartNode.NumberOfObtainedItemsInTree + signOfChange);
                }
                else
                {
                    _remainingCountTextController.gameObject.SetActive(false);
                    _remainingCountShadowTransform.gameObject.SetActive(false);
                }
            }
        }

        private void TryPlayItemChangedFeedbacks(PlayerItem item)
        {
            if (item == Item) // this check won't work correctly on item removal, but for now we only care about playing this feedback on additions anyway
            {
                _itemChangedFeedbacks.PlayFeedbacks();
            }
        }

        #endregion
        
    }
}