using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.Utilities;

namespace BML.Scripts.Player.Items
{
    public enum ItemType {
        PassiveStackable,
        Active,
        Passive
    }

    public enum ItemEffectTrigger {
        WhenAcquiredOrActivated,
        OnPickaxeSwing,
        OnPickaxeSweep
    }

    public enum ItemEffectType {
        StatIncrease,
        FireProjectile
    }

    [Serializable, InlineEditor()]
    public class ItemEffect {
        public ItemEffectTrigger Trigger = ItemEffectTrigger.WhenAcquiredOrActivated;
        public ItemEffectType Type = ItemEffectType.StatIncrease;

        [ShowIfGroup("StatIncrease", Condition = "Type", Value = ItemEffectType.StatIncrease)]
        [ShowIfGroup("StatIncrease")] public IntVariable Stat;
        [ShowIfGroup("StatIncrease")] public int StatIncreaseAmount;

        [ShowIfGroup("FireProjectile", Condition = "Type", Value = ItemEffectType.FireProjectile)]
        [ShowIfGroup("FireProjectile")] public GameObject _projectilePrefab;
    }

    public delegate void OnAffordabilityChanged();

    [InlineEditor()]
    [CreateAssetMenu(fileName = "PlayerItem", menuName = "BML/Player/PlayerItem", order = 0)]
    public class PlayerItem : SerializedScriptableObject
    {
        [SerializeField, FoldoutGroup("Descriptors")] private string _name;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea] private string _effectDescription;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea] private string _storeDescription;
        [SerializeField, FoldoutGroup("Descriptors")] private Sprite _icon;
        [SerializeField, FoldoutGroup("Descriptors")] private bool _useIconColor;
        [SerializeField, FoldoutGroup("Descriptors"), ShowIf("_useIconColor")] private Color _iconColor;

        [
            DictionaryDrawerSettings(KeyLabel = "Resource", ValueLabel = "Amount",
            DisplayMode = DictionaryDisplayOptions.ExpandedFoldout),
            SerializeField
        ] private Dictionary<PlayerResource, int> _itemCost = new Dictionary<PlayerResource, int>();

        [SerializeField, FoldoutGroup("Effect")] private ItemType _itemType = ItemType.PassiveStackable;
        [SerializeField, FoldoutGroup("Effect")] private List<ItemEffect> _itemEffects;

        private void OnEnable() {
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => {
                entry.Key.OnAmountChanged += InvokeOnAffordabilityChanged;
            });
        }

        private void OnDisble() {
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => {
                entry.Key.OnAmountChanged -= InvokeOnAffordabilityChanged;
            });
        }

        private void InvokeOnAffordabilityChanged() {
            OnAffordabilityChanged?.Invoke();
        }

        public Sprite Icon { get => _icon; }
        public bool UseIconColor { get => _icon; }
        public Color IconColor { get => _iconColor; }
        public string Name { get => _name; }
        public string EffectDescription { get => _effectDescription; }
        public string StoreDescription { get => _storeDescription; }
        public List<ItemEffect> ItemEffects { get => _itemEffects; }

        public event OnAffordabilityChanged OnAffordabilityChanged;

        //TODO: return what resources youre short on
        public bool CheckIfCanAfford() {
            return _itemCost.Any((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerCount >= entry.Value);
        }

        public void DeductCosts() {
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerCount -= entry.Value);
        }

        public string FormatCostsAsText() {
            return String.Join(" + ", _itemCost.Select((KeyValuePair<PlayerResource, int> entry) => $"{entry.Value}{entry.Key.IconText}"));
        }
    }
}
