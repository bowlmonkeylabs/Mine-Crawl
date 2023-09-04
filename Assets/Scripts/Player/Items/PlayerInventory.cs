using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items
{
    public delegate void OnPlayerItemChanged<PlayerItem>(PlayerItem item);

    [InlineEditor()]
    [CreateAssetMenu(fileName = "PlayerInventory", menuName = "BML/Player/PlayerInventory", order = 0)]
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private PlayerItem _activeItem;
        [SerializeField] private PlayerItem _passiveItem;
        [SerializeField] private List<PlayerItem> _passiveStackableItems;

        public PlayerItem ActiveItem { get => _activeItem; }
        public PlayerItem PassiveItem { get => _passiveItem; }
        public List<PlayerItem> PassiveStackableItems { get => _passiveStackableItems; }

        private event OnPlayerItemChanged<PlayerItem> OnPassiveStackableItemAdded;
    }
}
