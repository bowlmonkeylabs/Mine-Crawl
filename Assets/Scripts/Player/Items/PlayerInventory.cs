using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using Sirenix.OdinInspector;
using UnityEngine;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.ItemTreeGraph;
using BML.Scripts.Player.Items.ItemEffects;
using Sirenix.Utilities;
using UnityEngine.Serialization;

namespace BML.Scripts.Player.Items
{
    [CreateAssetMenu(fileName = "PlayerInventory", menuName = "BML/Player/PlayerInventory", order = 0)]
    [InlineEditor]
    [ExecuteAlways]
    public class PlayerInventory : ScriptableObject, IResettableScriptableObject
    {
        #region Inspector

        [SerializeField, FoldoutGroup("Player settings")] private BoolVariable _isPlayerGodMode;
        
        [Tooltip("The inventory that the player will start with. This is only used when the player inventory is reset.")]
        [SerializeField, FoldoutGroup("Player settings")] private PlayerInventory _startingInventory;
        
        private const float PROPERTY_SPACING = 20;
        
        #region Resources

        [SerializeField]
        [TabGroup("Resources", order: 4f), InlineProperty, HideLabel]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<PlayerResource, SlotTypeFilter> Resources;
        
        #endregion

        #region Player items

        [Button("Update"), PropertyOrder(-1)]
        internal void OnSlotsChangedInInspector()
        {
            PassiveStackableItemTrees.OnSlotsChangedInInspector();
            PassiveStackableItems.OnSlotsChangedInInspector();
            PassiveItems.OnSlotsChangedInInspector();
            ActiveItems.OnSlotsChangedInInspector();
            ConsumableItems.OnSlotsChangedInInspector();
        }
        
        [SerializeField]
        [TabGroup("Upgrades"), InlineProperty, HideLabel]
        [Title("Passive stackable item trees")]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<ItemTreeGraphStartNode, SlotTypeFilter> PassiveStackableItemTrees;
        
        [SerializeField]
        [TabGroup("Upgrades"), InlineProperty, HideLabel]
        [Title("Passive stackable items")]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<PlayerItem, SlotTypeFilter> PassiveStackableItems;
        
        [SerializeField]
        [TabGroup("Other", order: 3f), InlineProperty, HideLabel]
        [Title("Passive items")]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<PlayerItem, SlotTypeFilter> PassiveItems;

        [SerializeField]
        [TabGroup("Active items", order: 0f), InlineProperty, HideLabel]
        [Title("Active items")]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<PlayerItem, SlotTypeFilter> ActiveItems;

        [SerializeField]
        [TabGroup("Other"), InlineProperty, HideLabel]
        [Title("Consumable items")]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<PlayerItem, SlotTypeFilter> ConsumableItems;
        
        [NonSerialized]
        public Queue<PlayerItem> OnAcquiredConsumableQueue = new Queue<PlayerItem>();

        #endregion
        
        #endregion

        #region Public interface

        public bool CheckIfCanAddItem(PlayerItem item, bool ignoreReplacementCooldown = false)
        {
            // TODO implement pickup timer, for any timer that will replace an item in the inventory slot
            switch (item.Type)
            {
                case ItemType.PassiveStackable:
                    bool hasTree = PassiveStackableItemTrees.Contains(item.PassiveStackableTreeStartNode);
                    if (!hasTree)
                    {
                        (bool canAddTree, _) = PassiveStackableItemTrees.CheckIfCanAddItem(item.PassiveStackableTreeStartNode, ignoreReplacementCooldown);
                        hasTree = canAddTree;
                    }
                    if (hasTree)
                    {
                        (bool canAdd, _) = PassiveStackableItems.CheckIfCanAddItem(item, ignoreReplacementCooldown);
                        return canAdd;
                    }
                    return false;
                case ItemType.Passive:
                    return PassiveItems.CheckIfCanAddItem(item, ignoreReplacementCooldown).canAdd;
                case ItemType.Active:
                    return ActiveItems.CheckIfCanAddItem(item, ignoreReplacementCooldown).canAdd;
                case ItemType.Consumable:
                    // Allow pickup if the item grants ANY resources that the player has room to hold
                    // TODO this CanAddResource check should really be implemented on the inventory
                    bool canGrantAnyResourcesOnAcquired = item.ItemEffects
                        .Where(e => e.Trigger == ItemEffectTrigger.OnAcquired && e is AddResourceItemEffect)
                        .Any(e => (e as AddResourceItemEffect).CanAddResource());
                    // OR if the item has effects with any triggers other than OnAcquired
                    var effectsNotOnAcquired = item.ItemEffects.Where(e => e.Trigger != ItemEffectTrigger.OnAcquired).ToList();
                    if (canGrantAnyResourcesOnAcquired || effectsNotOnAcquired.Any())
                    {
                        if (canGrantAnyResourcesOnAcquired && !effectsNotOnAcquired.Any())
                        {
                            ignoreReplacementCooldown = true;
                        }
                        return ConsumableItems.CheckIfCanAddItem(item, ignoreReplacementCooldown).canAdd;
                    }
                    return false;
            }

            return false;
        }

        public bool CheckIfCanAffordItem(PlayerItem item)
        {
            return _isPlayerGodMode.Value || item.ItemCost.All((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerAmount >= entry.Value);
        }

        public bool CheckIfCanBuy(PlayerItem item, bool ignoreReplacementCooldown = false)
        {
            return CheckIfCanAffordItem(item) && CheckIfCanAddItem(item, ignoreReplacementCooldown);
        }

        public void DeductCosts(PlayerItem item)
        {
            item.ItemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerAmount -= entry.Value);
        }

        public bool TryAddItem(PlayerItem item, bool ignoreReplacementCooldown = false, bool dropOverflow = false)
        {
            bool didAdd = false;
            
            switch (item.Type)
            {
                case ItemType.PassiveStackable:
                    bool hasTree = PassiveStackableItemTrees.Contains(item.PassiveStackableTreeStartNode);
                    if (!hasTree)
                    {
                        bool didAddTree = PassiveStackableItemTrees.TryAddItem(item.PassiveStackableTreeStartNode, ignoreReplacementCooldown, dropOverflow);
                        hasTree = didAddTree;
                    }
                    if (hasTree)
                    {
                        didAdd = PassiveStackableItems.TryAddItem(item, ignoreReplacementCooldown, dropOverflow);
                    }
                    break;
                case ItemType.Passive:
                    didAdd = PassiveItems.TryAddItem(item, ignoreReplacementCooldown, dropOverflow);
                    break;
                case ItemType.Active:
                    didAdd = ActiveItems.TryAddItem(item, ignoreReplacementCooldown, dropOverflow);
                    break;
                case ItemType.Consumable:
                    // TODO what to do if it gets queued, but fails to get added to the inventory?
                    didAdd = true;
                    bool anyEffectsOtherThanOnAcquire = item.ItemEffects.Any(e => e.Trigger != ItemEffectTrigger.OnAcquired);
                    if (anyEffectsOtherThanOnAcquire)
                    {
                        didAdd = ConsumableItems.TryAddItem(item, ignoreReplacementCooldown, dropOverflow);
                    }
                    if (didAdd)
                    {
                        OnAcquiredConsumableQueue.Enqueue(item);
                    }
                    break;
            }

            return didAdd;
        }

        public bool TryRemoveItem(PlayerItem item)
        {
            bool didRemove = false;
            
            switch (item.Type)
            {
                case ItemType.PassiveStackable:
                    didRemove = PassiveStackableItems.TryRemoveItem(item);
                    break;
                case ItemType.Passive:
                    didRemove = PassiveItems.TryRemoveItem(item);
                    break;
                case ItemType.Active:
                    didRemove = ActiveItems.TryRemoveItem(item);
                    break;
                case ItemType.Consumable:
                    didRemove = ConsumableItems.TryRemoveItem(item);
                    break;
            }
            
            return didRemove;
        }

        public bool TryRemoveConsumable(int index)
        {
            bool didRemove = ConsumableItems.TryRemoveItem(index);
            return didRemove;
        }

        public void ResetScriptableObject()
        {
            this.PassiveStackableItems.ForEach(p => p?.ResetScriptableObject());
            this.PassiveItems.ForEach(p => p?.ResetScriptableObject());
            this.ActiveItems.ForEach(p => p?.ResetScriptableObject());
            this.ConsumableItems.ForEach(p => p?.ResetScriptableObject());
            
            if (_startingInventory != null)
            {
                this.PassiveStackableItemTrees.ResetToDefault(_startingInventory.PassiveStackableItemTrees);
                this.PassiveStackableItems.ResetToDefault(_startingInventory.PassiveStackableItems);
                this.PassiveItems.ResetToDefault(_startingInventory.PassiveItems);
                this.ActiveItems.ResetToDefault(_startingInventory.ActiveItems);
                this.ConsumableItems.ResetToDefault(_startingInventory.ConsumableItems);
            }

            OnReset?.Invoke();
        }

        public int GetItemCount(PlayerItem item)
        {
            int count = 0;
            switch (item.Type)
            {
                case ItemType.PassiveStackable:
                    count = PassiveStackableItems.Count(i => i == item);
                    break;
                case ItemType.Passive:
                    count = PassiveItems.Count(i => i == item);
                    break;
                case ItemType.Active:
                    count = ActiveItems.Count(i => i == item);
                    break;
                case ItemType.Consumable:
                    count = ConsumableItems.Count(i => i == item);
                    break;
            }

            return count;
        }

        #endregion
        
        #region Unity lifecycle

        private void OnEnable()
        {
            PassiveStackableItems.OnItemAdded += InvokeOnAnyPlayerItemAdded;
            PassiveStackableItems.OnItemRemoved += InvokeOnAnyPlayerItemRemoved;
            PassiveStackableItems.OnItemReplaced += InvokeOnAnyPlayerItemReplaced;
            PassiveStackableItems.OnItemOverflowed += InvokeOnAnyPlayerItemOverflowed;

            PassiveItems.OnItemAdded += InvokeOnAnyPlayerItemAdded;
            PassiveItems.OnItemRemoved += InvokeOnAnyPlayerItemRemoved;
            PassiveItems.OnItemReplaced += InvokeOnAnyPlayerItemReplaced;
            PassiveItems.OnItemOverflowed += InvokeOnAnyPlayerItemOverflowed;
            
            ActiveItems.OnItemAdded += InvokeOnAnyPlayerItemAdded;
            ActiveItems.OnItemRemoved += InvokeOnAnyPlayerItemRemoved;
            ActiveItems.OnItemReplaced += InvokeOnAnyPlayerItemReplaced;
            ActiveItems.OnItemOverflowed += InvokeOnAnyPlayerItemOverflowed;

            ConsumableItems.OnItemAdded += InvokeOnAnyPlayerItemAdded;
            ConsumableItems.OnItemRemoved += InvokeOnAnyPlayerItemRemoved;
            ConsumableItems.OnItemReplaced += InvokeOnAnyPlayerItemReplaced;
            ConsumableItems.OnItemOverflowed += InvokeOnAnyPlayerItemOverflowed;
        }

        private void OnDisable()
        {
            PassiveStackableItems.OnItemAdded -= InvokeOnAnyPlayerItemAdded;
            PassiveStackableItems.OnItemRemoved -= InvokeOnAnyPlayerItemRemoved;
            PassiveStackableItems.OnItemReplaced -= InvokeOnAnyPlayerItemReplaced;
            PassiveStackableItems.OnItemOverflowed -= InvokeOnAnyPlayerItemOverflowed;
            
            PassiveItems.OnItemAdded -= InvokeOnAnyPlayerItemAdded;
            PassiveItems.OnItemRemoved -= InvokeOnAnyPlayerItemRemoved;
            PassiveItems.OnItemReplaced -= InvokeOnAnyPlayerItemReplaced;
            PassiveItems.OnItemOverflowed -= InvokeOnAnyPlayerItemOverflowed;
            
            ActiveItems.OnItemAdded -= InvokeOnAnyPlayerItemAdded;
            ActiveItems.OnItemRemoved -= InvokeOnAnyPlayerItemRemoved;
            ActiveItems.OnItemReplaced -= InvokeOnAnyPlayerItemReplaced;
            ActiveItems.OnItemOverflowed -= InvokeOnAnyPlayerItemOverflowed;
            
            ConsumableItems.OnItemAdded -= InvokeOnAnyPlayerItemAdded;
            ConsumableItems.OnItemRemoved -= InvokeOnAnyPlayerItemRemoved;
            ConsumableItems.OnItemReplaced -= InvokeOnAnyPlayerItemReplaced;
            ConsumableItems.OnItemOverflowed -= InvokeOnAnyPlayerItemOverflowed;
        }

        #endregion

        #region Events
        
        public event IResettableScriptableObject.OnResetScriptableObject OnReset;
        
        public delegate void OnPlayerItemChanged(PlayerItem item);

        public delegate void OnPlayerItemReplaced(PlayerItem item);
        // public delegate void OnPlayerItemChanged();

        public event ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemChanged<PlayerItem> OnAnyPlayerItemAdded;
        public event ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemChanged<PlayerItem> OnAnyPlayerItemRemoved;
        public event ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemReplaced<PlayerItem> OnAnyPlayerItemReplaced;
        public event ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemChanged<PlayerItem> OnAnyPlayerItemOverflowed;
        // public event OnPlayerItemChanged OnAnyPlayerItemChangedInInspector;     // When changes happen through the inspector, we don't know which specific item changed

        private void InvokeOnAnyPlayerItemAdded(PlayerItem item)
        {
            OnAnyPlayerItemAdded?.Invoke(item);
        }
        
        private void InvokeOnAnyPlayerItemRemoved(PlayerItem item)
        {
            OnAnyPlayerItemRemoved?.Invoke(item);
        }
        
        private void InvokeOnAnyPlayerItemReplaced(PlayerItem oldItem, PlayerItem newItem)
        {
            OnAnyPlayerItemReplaced?.Invoke(oldItem, newItem);
        }
        
        private void InvokeOnAnyPlayerItemOverflowed(PlayerItem item)
        {
            OnAnyPlayerItemOverflowed?.Invoke(item);
        }

        [NonSerialized] private Dictionary<(PlayerItem, Action), ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemChanged<PlayerItem>> _itemOnInventoryItemUpdatedCallbacks 
            = new Dictionary<(PlayerItem, Action), ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemChanged<PlayerItem>>();
        [NonSerialized] private Dictionary<(PlayerItem, Action), ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemChanged> _itemOnInventoryUpdatedCallbacks 
            = new Dictionary<(PlayerItem, Action), ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemChanged>();
        [NonSerialized] private Dictionary<(PlayerItem, Action), OnAmountChanged> _itemOnCostAmountChangedCalledbacks 
            = new Dictionary<(PlayerItem, Action), OnAmountChanged>();
        [NonSerialized] private Dictionary<(PlayerItem, Action), OnUpdate> _itemOnGodModeChangedCallbacks 
            = new Dictionary<(PlayerItem, Action), OnUpdate>();
        
        public void SubscribeOnBuyabilityChanged(PlayerItem item, Action callback)
        {
            if (_itemOnCostAmountChangedCalledbacks.ContainsKey((item, callback)))
            {
                UnsubscribeOnBuyabilityChanged(item, callback);
            }
            OnAmountChanged onCostAmountChanged = () => callback();
            _itemOnCostAmountChangedCalledbacks[(item, callback)] = onCostAmountChanged;
            
            switch (item.Type)
            {
                case ItemType.PassiveStackable:
                case ItemType.Passive:
                case ItemType.Active:
                    break;
                case ItemType.Consumable:
                    item.ItemEffects.Where(e => e is AddResourceItemEffect)
                        .ForEach(e =>
                            (e as AddResourceItemEffect).Resource.OnAmountChanged += onCostAmountChanged);
                    break;
            }
            
            SubscribeOnPickupabilityChanged(item, callback);
            
            OnUpdate onGodModeUpdated = () => callback();
            _itemOnGodModeChangedCallbacks[(item, callback)] = onGodModeUpdated;
            _isPlayerGodMode?.Subscribe(onGodModeUpdated);
            
            item.ItemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => {
                entry.Key.OnAmountChanged += onCostAmountChanged;
            });
        }
        
        public void UnsubscribeOnBuyabilityChanged(PlayerItem item, Action callback)
        {
            bool keyExists =
                _itemOnCostAmountChangedCalledbacks.TryGetValue((item, callback), out var onCostAmountChanged);
            if (!keyExists)
            {
                return;
            }
            
            switch (item.Type)
            {
                case ItemType.PassiveStackable:
                case ItemType.Passive:
                case ItemType.Active:
                    break;
                case ItemType.Consumable:
                    item.ItemEffects.Where(e => e is AddResourceItemEffect)
                        .ForEach(e =>
                            (e as AddResourceItemEffect).Resource.OnAmountChanged -= onCostAmountChanged);
                    break;
            }
            
            UnsubscribeOnPickupabilityChanged(item, callback);
            
            OnUpdate onGodModeUpdated = _itemOnGodModeChangedCallbacks[(item, callback)];
            _isPlayerGodMode?.Unsubscribe(onGodModeUpdated);
            
            item.ItemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => {
                entry.Key.OnAmountChanged -= onCostAmountChanged;
            });
        }

        public void SubscribeOnPickupabilityChanged(PlayerItem item, Action callback)
        {
            if (_itemOnInventoryItemUpdatedCallbacks.ContainsKey((item, callback)))
            {
                UnsubscribeOnPickupabilityChanged(item, callback);
            }
            ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemChanged<PlayerItem> onInventoryItemUpdated = (PlayerItem playerItem) => callback();
            _itemOnInventoryItemUpdatedCallbacks[(item, callback)] = onInventoryItemUpdated;

            ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemChanged onInventoryUpdated = () => callback();
            _itemOnInventoryUpdatedCallbacks[(item, callback)] = onInventoryUpdated;

            switch (item.Type)
            {
                case ItemType.PassiveStackable:
                    PassiveStackableItems.OnItemAdded += onInventoryItemUpdated;
                    PassiveStackableItems.OnItemRemoved += onInventoryItemUpdated;
                    PassiveStackableItems.OnReplacementCooldownTimerStartedOrFinished += onInventoryUpdated;
                    break;
                case ItemType.Passive:
                    PassiveItems.OnItemAdded += onInventoryItemUpdated;
                    PassiveItems.OnItemRemoved += onInventoryItemUpdated;
                    PassiveItems.OnReplacementCooldownTimerStartedOrFinished += onInventoryUpdated;
                    break;
                case ItemType.Active:
                    ActiveItems.OnItemAdded += onInventoryItemUpdated;
                    ActiveItems.OnItemRemoved += onInventoryItemUpdated;
                    ActiveItems.OnReplacementCooldownTimerStartedOrFinished += onInventoryUpdated;
                    break;
                case ItemType.Consumable:
                    ConsumableItems.OnItemAdded += onInventoryItemUpdated;
                    ConsumableItems.OnItemRemoved += onInventoryItemUpdated;
                    ConsumableItems.OnReplacementCooldownTimerStartedOrFinished += onInventoryUpdated;
                    break;
            }
        }

        public void UnsubscribeOnPickupabilityChanged(PlayerItem item, Action callback)
        {
            bool keyExists = _itemOnInventoryItemUpdatedCallbacks.TryGetValue((item, callback), out var onInventoryItemUpdated);
            if (!keyExists)
            {
                return;
            }

            ItemSlotType<PlayerItem, SlotTypeFilter>.OnSlotItemChanged onInventoryUpdated = _itemOnInventoryUpdatedCallbacks[(item, callback)];
            
            switch (item.Type)
            {
                case ItemType.PassiveStackable:
                    PassiveStackableItems.OnItemAdded -= onInventoryItemUpdated;
                    PassiveStackableItems.OnItemRemoved -= onInventoryItemUpdated;
                    PassiveStackableItems.OnReplacementCooldownTimerStartedOrFinished -= onInventoryUpdated;
                    break;
                case ItemType.Passive:
                    PassiveItems.OnItemAdded -= onInventoryItemUpdated;
                    PassiveItems.OnItemRemoved -= onInventoryItemUpdated;
                    PassiveItems.OnReplacementCooldownTimerStartedOrFinished -= onInventoryUpdated;
                    break;
                case ItemType.Active:
                    ActiveItems.OnItemAdded -= onInventoryItemUpdated;
                    ActiveItems.OnItemRemoved -= onInventoryItemUpdated;
                    ActiveItems.OnReplacementCooldownTimerStartedOrFinished -= onInventoryUpdated;
                    break;
                case ItemType.Consumable:
                    ConsumableItems.OnItemAdded -= onInventoryItemUpdated;
                    ConsumableItems.OnItemRemoved -= onInventoryItemUpdated;
                    ConsumableItems.OnReplacementCooldownTimerStartedOrFinished -= onInventoryUpdated;
                    break;
            }
        }

        #endregion

    }
}
