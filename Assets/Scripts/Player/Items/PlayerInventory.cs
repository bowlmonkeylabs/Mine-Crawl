using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.Scripts.ItemTreeGraph;

namespace BML.Scripts.Player.Items
{
    public delegate void OnPlayerItemChanged<PlayerItem>(PlayerItem item);
    public delegate void OnInventoryChanged();

    [InlineEditor()]
    [CreateAssetMenu(fileName = "PlayerInventory", menuName = "BML/Player/PlayerInventory", order = 0)]
    [ExecuteAlways]
    public class PlayerInventory : ScriptableObject, IResettableScriptableObject
    {
        #region Inspector

        [OnValueChanged("OnActiveItemChanged")]
        [SerializeField, InfoBox("Item is not of type 'Active'", InfoMessageType.Error, "@_activeItem != null && _activeItem.Type != ItemType.Active")] private PlayerItem _activeItem;
        private void OnActiveItemChanged()
        {
            OnAnyItemAdded?.Invoke(_activeItem);
            OnActiveItemAdded?.Invoke(_activeItem);
        }
        
        [OnValueChanged("OnPassiveItemChanged")]
        [SerializeField, InfoBox("Item is not of type 'Passive'", InfoMessageType.Error, "@_passiveItem != null && _passiveItem.Type != ItemType.Passive")]
        private PlayerItem _passiveItem;
        private void OnPassiveItemChanged()
        {
            OnAnyItemAdded?.Invoke(_passiveItem);
            OnPassiveItemAdded?.Invoke(_passiveItem);
        }
        
        [OnValueChanged("OnPassiveStackableItemsChanged")]
        [SerializeField] private List<PlayerItem> _passiveStackableItems;
        private void OnPassiveStackableItemsChanged()
        {
            OnPassiveStackableItemChanged?.Invoke();
        }

        [OnValueChanged("OnPassiveStackableItemTreesChanged")]
        [SerializeField] private List<ItemTreeGraphStartNode> _passiveStackableItemTrees;
        private void OnPassiveStackableItemTreesChanged()
        {
            OnPassiveStackableItemTreeChanged?.Invoke();
        }

        #endregion

        #region Public interface

        public PlayerItem ActiveItem 
        {
            get => _activeItem;
            set {
                if (value == null || value.Type == ItemType.Active)
                {
                    if (_activeItem != null)
                    {
                        OnAnyItemRemoved?.Invoke(_activeItem);
                        OnActiveItemRemoved?.Invoke(_activeItem);
                    }
                    _activeItem = value;
                    if (value != null)
                    {
                        OnAnyItemAdded?.Invoke(value);
                        OnActiveItemAdded?.Invoke(value);
                    }
                }
            }
        }
        
        public PlayerItem PassiveItem 
        {
            get => _passiveItem;
            set {
                if (value == null || value.Type == ItemType.Passive)
                {
                    if (_passiveItem != null)
                    {
                        OnAnyItemRemoved?.Invoke(_passiveItem);
                        OnPassiveItemRemoved?.Invoke(_passiveItem);
                    }
                    _passiveItem = value;
                    if (value != null)
                    {
                        OnAnyItemAdded?.Invoke(value);
                        OnPassiveItemAdded?.Invoke(value);
                    }
                }
            }
        }
        
        public List<PlayerItem> PassiveStackableItems => _passiveStackableItems;

        public void AddPassiveStackableItem(PlayerItem playerItem) 
        {
            if (playerItem.Type == ItemType.PassiveStackable)
            {
                _passiveStackableItems.Add(playerItem);
                OnAnyItemAdded?.Invoke(playerItem);
                OnPassiveStackableItemAdded?.Invoke(playerItem);
            }
        }

        public void RemovePassiveStackableItem(PlayerItem playerItem) 
        {
            if (playerItem.Type == ItemType.PassiveStackable)
            {
                bool didRemove = _passiveStackableItems.Remove(playerItem);
                if (didRemove)
                {
                    OnAnyItemRemoved?.Invoke(playerItem);
                    OnPassiveStackableItemRemoved?.Invoke(playerItem);
                }
            }
        }

        public List<ItemTreeGraphStartNode> PassiveStackableItemTrees => _passiveStackableItemTrees;

        public void AddPassiveStackableItemTree(ItemTreeGraphStartNode itemTreeStartNode) 
        {
            if (!_passiveStackableItemTrees.Contains(itemTreeStartNode))
            {
                _passiveStackableItemTrees.Add(itemTreeStartNode);
                OnPassiveStackableItemTreeAdded?.Invoke(itemTreeStartNode);
            }
        }

        public void RemovePassiveStackableItemTree(ItemTreeGraphStartNode itemTreeStartNode) 
        {
            bool didRemove = _passiveStackableItemTrees.Remove(itemTreeStartNode);
            if (didRemove)
            {
                OnPassiveStackableItemTreeRemoved?.Invoke(itemTreeStartNode);
            }
        }

        public void ResetScriptableObject()
        {
            this._activeItem?.ResetScriptableObject();
            this._passiveItem?.ResetScriptableObject();
            this._passiveStackableItems.ForEach(p => p.ResetScriptableObject());
            
            this._activeItem = null;
            this._passiveItem = null;
            this._passiveStackableItems.Clear();
            this._passiveStackableItemTrees.Clear();

            OnReset?.Invoke();
        }

        #endregion

        #region Events
        
        public event IResettableScriptableObject.OnResetScriptableObject OnReset;
        //the parameter passed into the remove events is the item that was removed, and the param passed into the added events is the item that was added
        public event OnPlayerItemChanged<PlayerItem> OnAnyItemAdded;
        public event OnPlayerItemChanged<PlayerItem> OnAnyItemRemoved;
        public event OnPlayerItemChanged<ItemTreeGraphStartNode> OnPassiveStackableItemTreeAdded;
        public event OnPlayerItemChanged<ItemTreeGraphStartNode> OnPassiveStackableItemTreeRemoved;
        public event OnInventoryChanged OnPassiveStackableItemTreeChanged;
        public event OnPlayerItemChanged<PlayerItem> OnPassiveStackableItemAdded;
        public event OnPlayerItemChanged<PlayerItem> OnPassiveStackableItemRemoved;
        public event OnInventoryChanged OnPassiveStackableItemChanged;
        public event OnPlayerItemChanged<PlayerItem> OnPassiveItemAdded;
        public event OnPlayerItemChanged<PlayerItem> OnPassiveItemRemoved;
        public event OnPlayerItemChanged<PlayerItem> OnActiveItemAdded;
        public event OnPlayerItemChanged<PlayerItem> OnActiveItemRemoved;

        #endregion

    }
}
