using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using BML.ScriptableObjectCore.Scripts.Managers;

namespace BML.Scripts.Player.Items
{
    public delegate void OnPlayerItemChanged<PlayerItem>(PlayerItem item);

    [InlineEditor()]
    [CreateAssetMenu(fileName = "PlayerInventory", menuName = "BML/Player/PlayerInventory", order = 0)]
    public class PlayerInventory : ScriptableObject, IResettableScriptableObject
    {
        #region Inspector

        [SerializeField, OnValueChanged("OnActiveItemChanged"), InfoBox("Item is not of type 'Active'", InfoMessageType.Error, "@_activeItem != null && _activeItem.Type != ItemType.Active")] private PlayerItem _activeItem;
        private void OnActiveItemChanged()
        {
            OnActiveItemAdded?.Invoke(_activeItem);
        }
        
        [SerializeField, InfoBox("Item is not of type 'Passive'", InfoMessageType.Error, "@_passiveItem != null && _passiveItem.Type != ItemType.Passive")] private PlayerItem _passiveItem;
        
        [SerializeField] private List<PlayerItem> _passiveStackableItems;

        #endregion

        #region Public interface

        public PlayerItem ActiveItem 
        {
            get => _activeItem;
            set {
                if (value.Type == ItemType.Active)
                {
                    if (_activeItem != null) OnActiveItemRemoved?.Invoke(_activeItem);
                    _activeItem = value;
                    if (value != null) OnActiveItemAdded?.Invoke(value);
                }
            }
        }
        
        public PlayerItem PassiveItem 
        {
            get => _passiveItem;
            set {
                if (value.Type == ItemType.Passive)
                {
                    if (_passiveItem != null) OnPassiveItemRemoved?.Invoke(_passiveItem);
                    _passiveItem = value;
                    if (value != null) OnPassiveItemAdded?.Invoke(value);
                }
            }
        }
        
        public List<PlayerItem> PassiveStackableItems => _passiveStackableItems;

        public void AddPassiveStackableItem(PlayerItem playerItem) 
        {
            if (playerItem.Type == ItemType.PassiveStackable)
            {
                _passiveStackableItems.Add(playerItem);
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
                    OnPassiveStackableItemAdded?.Invoke(playerItem);
                }
            }
        }

        public void ResetScriptableObject()
        {
            this._activeItem = null;
            this._passiveItem = null;
            this._passiveStackableItems.Clear();

            OnActiveItemAdded?.Invoke(null);
            OnPassiveItemAdded?.Invoke(null);
            OnPassiveStackableItemAdded?.Invoke(null);
        }

        #endregion

        #region Events

        public event OnPlayerItemChanged<PlayerItem> OnPassiveStackableItemAdded;
        public event OnPlayerItemChanged<PlayerItem> OnPassiveStackableItemRemoved;
        public event OnPlayerItemChanged<PlayerItem> OnPassiveItemAdded;
        public event OnPlayerItemChanged<PlayerItem> OnPassiveItemRemoved;
        public event OnPlayerItemChanged<PlayerItem> OnActiveItemAdded;
        public event OnPlayerItemChanged<PlayerItem> OnActiveItemRemoved;

        #endregion

    }
}
