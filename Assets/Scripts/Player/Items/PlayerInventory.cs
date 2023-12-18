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

        [SerializeField, FoldoutGroup("Player dependency")] private BoolVariable _isPlayerGodMode;
        
        private const float PROPERTY_SPACING = 20;
        
        #region Resources

        [SerializeField]
        [BoxGroup("Resources"), InlineProperty, HideLabel]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<PlayerResource> Resources;
        
        #endregion

        #region Player items
        
        [SerializeField]
        [BoxGroup("Passive Stackable Trees"), InlineProperty, HideLabel]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<ItemTreeGraphStartNode> PassiveStackableItemTrees;
        
        [SerializeField]
        [BoxGroup("Passive Stackable Items"), InlineProperty, HideLabel]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<PlayerItem> PassiveStackableItems;
        
        [SerializeField]
        [BoxGroup("Passive Items"), InlineProperty, HideLabel]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<PlayerItem> PassiveItems;

        [SerializeField]
        [BoxGroup("Active Items"), InlineProperty, HideLabel]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<PlayerItem> ActiveItems;

        [SerializeField]
        [BoxGroup("Consumable Items"), InlineProperty, HideLabel]
        [PropertySpace(SpaceAfter = PROPERTY_SPACING)]
        public ItemSlotType<PlayerItem> ConsumableItems;
        
        [NonSerialized]
        public Queue<PlayerItem> OnAcquiredConsumableQueue = new Queue<PlayerItem>();

        #endregion
        
        #endregion

        #region Public interface

        public bool CheckIfCanAddItem(PlayerItem item)
        {
            // TODO implement pickup timer, for any timer that will replace an item in the inventory slot
            switch (item.Type)
            {
                case ItemType.PassiveStackable:
                    return true;
                    break;
                case ItemType.Passive:
                    return PassiveItems.CheckIfCanAddItem(item).canAdd;
                    break;
                case ItemType.Active:
                    return ActiveItems.CheckIfCanAddItem(item).canAdd;
                    break;
                case ItemType.Consumable:
                    // Allow pickup if the item grants ANY resources that the player has room to hold
                    // TODO this CanAddResource check should really be implemented on the inventory
                    bool canGrantAnyResourcesOnAcquired = item.ItemEffects
                        .Where(e => e.Trigger == ItemEffectTrigger.OnAcquired && e is AddResourceItemEffect)
                        .Any(e => (e as AddResourceItemEffect).CanAddResource());
                    // OR if the item has effects with any triggers other than OnAcquired
                    var effectsNotOnAcquired = item.ItemEffects.Where(e => e.Trigger != ItemEffectTrigger.OnAcquired);
                    if (canGrantAnyResourcesOnAcquired || effectsNotOnAcquired.Any())
                    {
                        return ConsumableItems.CheckIfCanAddItem(item).canAdd;
                    }
                    return false;
                    break;
            }

            return false;
        }

        public bool CheckIfCanAffordItem(PlayerItem item)
        {
            return _isPlayerGodMode.Value || item.ItemCost.All((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerAmount >= entry.Value);
        }

        public bool CheckIfCanBuy(PlayerItem item)
        {
            return CheckIfCanAffordItem(item) && CheckIfCanAddItem(item);
        }

        public void DeductCosts(PlayerItem item)
        {
            item.ItemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerAmount -= entry.Value);
        }

        public bool TryAddItem(PlayerItem item)
        {
            bool didAdd = false;
            
            switch (item.Type)
            {
                case ItemType.PassiveStackable:
                    bool hasTree = PassiveStackableItemTrees.Contains(item.PassiveStackableTreeStartNode);
                    if (!hasTree)
                    {
                        bool didAddTree = PassiveStackableItemTrees.TryAddItem(item.PassiveStackableTreeStartNode);
                        hasTree = didAddTree;
                    }
                    if (hasTree)
                    {
                        didAdd = PassiveStackableItems.TryAddItem(item);
                    }
                    break;
                case ItemType.Passive:
                    didAdd = PassiveItems.TryAddItem(item);
                    break;
                case ItemType.Active:
                    didAdd = ActiveItems.TryAddItem(item);
                    break;
                case ItemType.Consumable:
                    // TODO what to do if it gets queued, but fails to get added to the inventory?
                    OnAcquiredConsumableQueue.Enqueue(item);
                    didAdd = true;
                    bool anyEffectsOtherThanOnAcquire = item.ItemEffects.Any(e => e.Trigger != ItemEffectTrigger.OnAcquired);
                    if (anyEffectsOtherThanOnAcquire)
                    {
                        // didAdd = ConsumableItems.TryAddItem(item);
                        ConsumableItems.TryAddItem(item);
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
            this.ActiveItems.ForEach(p => p?.ResetScriptableObject());
            this.PassiveItems.ForEach(p => p?.ResetScriptableObject());
            this.PassiveStackableItems.ForEach(p => p?.ResetScriptableObject());

            this.PassiveItems.Clear();
            this.PassiveStackableItems.Clear();
            this.PassiveStackableItemTrees.Clear();

            OnReset?.Invoke();
        }

        #endregion
        
        #region Unity lifecycle

        private void OnEnable()
        {
            PassiveStackableItems.OnItemAdded += InvokeOnAnyPlayerItemAdded;
            PassiveStackableItems.OnItemRemoved += InvokeOnAnyPlayerItemRemoved;
            PassiveStackableItems.OnItemReplaced += InvokeOnAnyPlayerItemReplaced;
            
            ActiveItems.OnItemAdded += InvokeOnAnyPlayerItemAdded;
            ActiveItems.OnItemRemoved += InvokeOnAnyPlayerItemRemoved;
            ActiveItems.OnItemReplaced += InvokeOnAnyPlayerItemReplaced;

            PassiveItems.OnItemAdded += InvokeOnAnyPlayerItemAdded;
            PassiveItems.OnItemRemoved += InvokeOnAnyPlayerItemRemoved;
            PassiveItems.OnItemReplaced += InvokeOnAnyPlayerItemReplaced;

            ConsumableItems.OnItemAdded += InvokeOnAnyPlayerItemAdded;
            ConsumableItems.OnItemRemoved += InvokeOnAnyPlayerItemRemoved;
            ConsumableItems.OnItemReplaced += InvokeOnAnyPlayerItemReplaced;
        }

        private void OnDisable()
        {
            PassiveStackableItems.OnItemAdded -= InvokeOnAnyPlayerItemAdded;
            PassiveStackableItems.OnItemRemoved -= InvokeOnAnyPlayerItemRemoved;
            PassiveStackableItems.OnItemReplaced -= InvokeOnAnyPlayerItemReplaced;
            
            PassiveItems.OnItemAdded -= InvokeOnAnyPlayerItemAdded;
            PassiveItems.OnItemRemoved -= InvokeOnAnyPlayerItemRemoved;
            PassiveItems.OnItemReplaced -= InvokeOnAnyPlayerItemReplaced;
            
            ActiveItems.OnItemAdded -= InvokeOnAnyPlayerItemAdded;
            ActiveItems.OnItemRemoved -= InvokeOnAnyPlayerItemRemoved;
            ActiveItems.OnItemReplaced -= InvokeOnAnyPlayerItemReplaced;
            
            ConsumableItems.OnItemAdded -= InvokeOnAnyPlayerItemAdded;
            ConsumableItems.OnItemRemoved -= InvokeOnAnyPlayerItemRemoved;
            ConsumableItems.OnItemReplaced -= InvokeOnAnyPlayerItemReplaced;
        }

        #endregion

        #region Events
        
        public event IResettableScriptableObject.OnResetScriptableObject OnReset;
        
        public delegate void OnPlayerItemChanged(PlayerItem item);

        public delegate void OnPlayerItemReplaced(PlayerItem item);
        // public delegate void OnPlayerItemChanged();

        public event ItemSlotType<PlayerItem>.OnSlotItemChanged<PlayerItem> OnAnyPlayerItemAdded;
        public event ItemSlotType<PlayerItem>.OnSlotItemChanged<PlayerItem> OnAnyPlayerItemRemoved;
        public event ItemSlotType<PlayerItem>.OnSlotItemReplaced<PlayerItem> OnAnyPlayerItemReplaced;
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

        [NonSerialized] private Dictionary<(PlayerItem, Action), ItemSlotType<PlayerItem>.OnSlotItemChanged<PlayerItem>> _itemOnInventoryItemUpdatedCallbacks 
            = new Dictionary<(PlayerItem, Action), ItemSlotType<PlayerItem>.OnSlotItemChanged<PlayerItem>>();
        [NonSerialized] private Dictionary<(PlayerItem, Action), ItemSlotType<PlayerItem>.OnSlotItemChanged> _itemOnInventoryUpdatedCallbacks 
            = new Dictionary<(PlayerItem, Action), ItemSlotType<PlayerItem>.OnSlotItemChanged>();
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
            ItemSlotType<PlayerItem>.OnSlotItemChanged<PlayerItem> onInventoryItemUpdated = (PlayerItem playerItem) => callback();
            _itemOnInventoryItemUpdatedCallbacks[(item, callback)] = onInventoryItemUpdated;

            ItemSlotType<PlayerItem>.OnSlotItemChanged onInventoryUpdated = () => callback();
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

            ItemSlotType<PlayerItem>.OnSlotItemChanged onInventoryUpdated = _itemOnInventoryUpdatedCallbacks[(item, callback)];
            
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
