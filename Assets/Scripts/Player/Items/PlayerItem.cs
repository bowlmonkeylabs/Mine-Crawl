using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
using System;

namespace BML.Scripts.Player.Items
{
    public enum ItemType {
        PassiveStackable = 1,
        Active = 2,
        Passive = 3
    }

    [InlineEditor()]
    [CreateAssetMenu(fileName = "PlayerItem", menuName = "BML/Player/PlayerItem", order = 0)]
    public class PlayerItem : SerializedScriptableObject
    {
        [SerializeField, FoldoutGroup("Descriptors")] private string _name;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea] private string _effectDescription;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea] private string _storeDescription;
        [SerializeField, FoldoutGroup("Descriptors")] private Sprite _icon;
        [FoldoutGroup("Descriptors")] private bool _useIconColor;
        [FoldoutGroup("Descriptors"), ShowIf("_useItemIconColor")] private Color _iconColor;

        [SerializeField, FoldoutGroup("Store")] private Dictionary<PlayerResource, int> _itemCost;

        [SerializeField, FoldoutGroup("Effect")] private ItemType _itemType = ItemType.PassiveStackable;

        public Sprite Icon { get => _icon; }
        public bool UseIconColor { get => _icon; }
        public Color IconColor { get => _iconColor; }
        public string Name { get => _name; }
        public string EffectDescription { get => _effectDescription; }
        public string StoreDescription { get => _storeDescription; }

        //TODO: return what resources youre short on
        public bool CheckIfCanAfford() {
            return _itemCost.Any((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerCount >= entry.Value);
        }

        public void DeductCosts() {
            _itemCost.AsParallel().ForAll((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerCount -= entry.Value);
        }

        public string FormatCostsAsText() {
            return String.Join(" + ", _itemCost.Select((KeyValuePair<PlayerResource, int> entry) => $" + {entry.Value}{entry.Key.IconText}"));
        }
    }
}
