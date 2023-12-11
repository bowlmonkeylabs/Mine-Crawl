using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.Scripts.ItemTreeGraph;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.Player.Items
{
    public enum ItemType 
    {
        PassiveStackable = 1,
        Passive = 2,
        Active = 3,
        Consumable = 4,
    }

    [InlineEditor()]
    [CreateAssetMenu(fileName = "PlayerItem", menuName = "BML/Player/PlayerItem", order = 0)]
    public class PlayerItem : SerializedScriptableObject, IResettableScriptableObject
    {
        #region Inspector
        
        [SerializeField, FoldoutGroup("Descriptors")] private string _name;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea] private string _effectDescription;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea] private string _storeDescription;
        [SerializeField, FoldoutGroup("Descriptors")] private bool _useIconColor;
        [SerializeField, FoldoutGroup("Descriptors"), ShowIf("_useIconColor")] private Color _iconColor;
        [SerializeField, FoldoutGroup("Descriptors"), PreviewField(100, ObjectFieldAlignment.Left)] private Sprite _icon;
        [SerializeField, FoldoutGroup("Descriptors"), ReadOnly, ShowIf("@_itemType == ItemType.PassiveStackable")]
        public ItemTreeGraphStartNode PassiveStackableTreeStartNode;
        
        [FormerlySerializedAs("_storeCost")]
        [SerializeField]
        [FoldoutGroup("Store")]
        [DictionaryDrawerSettings(KeyLabel = "Resource", ValueLabel = "Amount", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private Dictionary<PlayerResource, int> _itemCost = new Dictionary<PlayerResource, int>();

        [SerializeField, FoldoutGroup("Effect")] private ItemType _itemType = ItemType.PassiveStackable;
        [SerializeField, FoldoutGroup("Effect")]
        // [HideReferenceObjectPicker]
        private List<ItemEffect> _itemEffects = new List<ItemEffect>();

        #endregion
        
        #region Public interface
        
        public string Name => _name;
        public string EffectDescription => _effectDescription;
        public string StoreDescription => _storeDescription;
        public bool UseIconColor => _useIconColor;
        public Color IconColor => _iconColor;
        public Sprite Icon => _icon;
        public Dictionary<PlayerResource, int> ItemCost => _itemCost;
        public ItemType Type => _itemType;
        public List<ItemEffect> ItemEffects => _itemEffects;

        #endregion
        
        #region Unity lifecycle
        
        private void OnEnable() 
        {
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => {
                entry.Key.OnAmountChanged += InvokeOnAffordabilityChanged;
            });
        }

        private void OnDisable() 
        {
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => {
                entry.Key.OnAmountChanged -= InvokeOnAffordabilityChanged;
            });
        }
        
        #endregion
        
        #region Affordability/Store interface

        public delegate void _OnAffordabilityChanged();
        public event _OnAffordabilityChanged OnAffordabilityChanged;
        
        private void InvokeOnAffordabilityChanged()
        {
            OnAffordabilityChanged?.Invoke();
        }

        //TODO: return what resources you're short on
        public bool CheckIfCanAfford() 
        {
            return _itemCost.All((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerAmount >= entry.Value);
        }

        public void DeductCosts()
        {
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => entry.Key.PlayerAmount -= entry.Value);
        }

        public string FormatCostsAsText() 
        {
            return String.Join(" + ", _itemCost.Select((KeyValuePair<PlayerResource, int> entry) => $"{entry.Value}{entry.Key.IconText}"));
        }
        
        #endregion
        
        #region IResettableScriptableObject
        
        public event IResettableScriptableObject.OnResetScriptableObject OnReset;

        public void ResetScriptableObject()
        {
            _itemEffects.ForEach(e => e.Reset());
            OnReset?.Invoke();
        }
        
        #endregion
        
    }
}
