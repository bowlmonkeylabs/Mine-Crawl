using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.Scripts.ItemTreeGraph;
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

        #endregion

    }
}
