using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.Scripts.ItemTreeGraph;
using Sirenix.Utilities;
using UnityEngine.Serialization;

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
        
        [FormerlySerializedAs("_activeStackableItems")]
        [OnValueChanged("OnActiveItemChanged")]
        [SerializeField] private List<PlayerItem> _activeItems;
        [SerializeField] private int _swappableActiveItemIndex = 3;

        private PlayerItem _swappableActiveItem
        {
            get => _activeItems[_swappableActiveItemIndex];
            set
            {
                _activeItems[_swappableActiveItemIndex] = value;
            }
        }

        [NonSerialized]
        private PlayerItem _prevActiveItem;
        private void OnActiveItemChanged()
        {
            if (_swappableActiveItem != null)
            {
                OnAnyItemAdded?.Invoke(_swappableActiveItem);
                OnActiveItemAdded?.Invoke(_swappableActiveItem);
            }
            else
            {
                OnAnyItemRemoved?.Invoke(null);
                OnActiveItemRemoved?.Invoke(null);
            }
            _prevActiveItem = _swappableActiveItem;
        }

        [OnValueChanged("OnPassiveItemChanged")]
        [SerializeField, InfoBox("Item is not of type 'Passive'", InfoMessageType.Error, "@_passiveItem != null && _passiveItem.Type != ItemType.Passive")]
        private PlayerItem _passiveItem;
        
        [NonSerialized]
        private PlayerItem _prevPassiveItem;
        private void OnPassiveItemChanged()
        {
            if (_passiveItem != null)
            {
                OnAnyItemAdded?.Invoke(_passiveItem);
                OnPassiveItemAdded?.Invoke(_passiveItem);
            }
            else if (_prevPassiveItem != null)
            {
                OnAnyItemRemoved?.Invoke(_prevPassiveItem);
                OnPassiveItemRemoved?.Invoke(_prevPassiveItem);
            }
            _prevPassiveItem = _passiveItem;
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

        #region Unity lifecycle

        private void Awake()
        {
            int numActiveItemSlots = _swappableActiveItemIndex + 1;
            if (_activeItems.Count < numActiveItemSlots)
            {
                _activeItems.SetLength(numActiveItemSlots);
            }
            _prevActiveItem = _swappableActiveItem;
            _prevPassiveItem = _passiveItem;
        }

        #endregion

        #region Public interface

        public PlayerItem SwappableActiveItem 
        {
            get => _swappableActiveItem;
            set {
                if (value == null || value.Type == ItemType.Active)
                {
                    if (_swappableActiveItem != null)
                    {
                        OnAnyItemRemoved?.Invoke(_swappableActiveItem);
                        OnActiveItemRemoved?.Invoke(_swappableActiveItem);
                    }
                    _activeItems[_swappableActiveItemIndex] = value;
                    if (value != null)
                    {
                        OnAnyItemAdded?.Invoke(value);
                        OnActiveItemAdded?.Invoke(value);
                    }
                    _prevActiveItem = _swappableActiveItem;
                }
            }
        }
        
        public List<PlayerItem> ActiveItems => _activeItems;
        
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
                    _prevPassiveItem = _passiveItem;
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
            this._activeItems.ForEach(p => p?.ResetScriptableObject());
            this._passiveItem?.ResetScriptableObject();
            this._passiveStackableItems.ForEach(p => p?.ResetScriptableObject());

            this._swappableActiveItem = null;
            int numActiveItemSlots = _swappableActiveItemIndex + 1;
            if (_activeItems.Count != numActiveItemSlots)
            {
                _activeItems.SetLength(numActiveItemSlots);
            }
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
