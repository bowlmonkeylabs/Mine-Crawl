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
        [SerializeField] private PlayerItem _activeItem;
        [SerializeField] private PlayerItem _passiveItem;
        [SerializeField] private List<PlayerItem> _passiveStackableItems;

        public PlayerItem ActiveItem {
            get => _activeItem;
            set {
                if(value != null) {
                    OnActiveItemRemoved.Invoke(_passiveItem);
                }
                _activeItem = value;
                OnActiveItemAdded.Invoke(value);
            }
        }
        public PlayerItem PassiveItem {
            get => _passiveItem;
            set {
                if(value != null) {
                    OnPassiveItemRemoved.Invoke(_passiveItem);
                }
                _passiveItem = value;
                OnPassiveItemAdded.Invoke(value);
            }
        }
        public List<PlayerItem> PassiveStackableItems { get => _passiveStackableItems; }
        

        public event OnPlayerItemChanged<PlayerItem> OnPassiveStackableItemAdded;
        public event OnPlayerItemChanged<PlayerItem> OnPassiveStackableItemRemoved;
        public event OnPlayerItemChanged<PlayerItem> OnPassiveItemAdded;
        public event OnPlayerItemChanged<PlayerItem> OnPassiveItemRemoved;
        public event OnPlayerItemChanged<PlayerItem> OnActiveItemAdded;
        public event OnPlayerItemChanged<PlayerItem> OnActiveItemRemoved;

        public void AddPassiveStackableItem(PlayerItem playerItem) {
            _passiveStackableItems.Add(playerItem);
            OnPassiveStackableItemAdded.Invoke(playerItem);
        }

        public void RemovePassiveStackableItem(PlayerItem playerItem) {
            _passiveStackableItems.Remove(playerItem);
            OnPassiveStackableItemRemoved.Invoke(playerItem);
        }

        public void ResetScriptableObject()
        {
            this._activeItem = null;
            this._passiveItem = null;
            this._passiveStackableItems.Clear();
        }
    }
}
