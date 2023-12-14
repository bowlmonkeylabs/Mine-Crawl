using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.ItemTreeGraph;
using BML.Scripts.Player.Items.ItemEffects;
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
        [SerializeField, FoldoutGroup("Descriptors"), ShowIf("_useIconColor")] private Color _iconColor = Color.white;
        [SerializeField, FoldoutGroup("Descriptors"), PreviewField(100, ObjectFieldAlignment.Left)] protected Sprite _icon;
        [SerializeField, FoldoutGroup("Descriptors"), ReadOnly, ShowIf("@_itemType == ItemType.PassiveStackable")]
        public ItemTreeGraphStartNode PassiveStackableTreeStartNode;
        
        [SerializeField, FoldoutGroup("Pickup"), PreviewField(100, ObjectFieldAlignment.Left), AssetsOnly] protected GameObject _objectPrefab;
        [SerializeField, FoldoutGroup("Pickup")] private PlayerInventory _playerInventory;
        
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
        public GameObject ObjectPrefab => _objectPrefab;
        public Dictionary<PlayerResource, int> ItemCost => _itemCost;
        public ItemType Type => _itemType;
        public List<ItemEffect> ItemEffects => _itemEffects;
        public int? RemainingActivations => _itemEffects.FirstOrDefault(e => e.UseActivationLimit)?.RemainingActivations.Value;

        #endregion
        
        #region Unity lifecycle
        
        private void OnEnable()
        {
            switch (_itemType)
            {
                case ItemType.PassiveStackable:
                    _playerInventory.PassiveStackableItems.OnItemAdded += OnInventoryUpdated;
                    _playerInventory.PassiveStackableItems.OnItemRemoved += OnInventoryUpdated;
                    break;
                case ItemType.Passive:
                    _playerInventory.PassiveItems.OnItemAdded += OnInventoryUpdated;
                    _playerInventory.PassiveItems.OnItemRemoved += OnInventoryUpdated;
                    break;
                case ItemType.Active:
                    _playerInventory.ActiveItems.OnItemAdded += OnInventoryUpdated;
                    _playerInventory.ActiveItems.OnItemRemoved += OnInventoryUpdated;
                    break;
                case ItemType.Consumable:
                    _itemEffects.Where(e => e is AddResourceItemEffect)
                        .ForEach(e =>
                            (e as AddResourceItemEffect).Resource.OnAmountChanged += InvokeOnPickupabilityChanged);
                    break;
            }
            
            OnPickupabilityChanged += InvokeOnBuyabilityChanged;
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => {
                entry.Key.OnAmountChanged += InvokeOnBuyabilityChanged;
            });
        }

        private void OnDisable()
        {
            switch (_itemType)
            {
                case ItemType.PassiveStackable:
                    _playerInventory.PassiveStackableItems.OnItemAdded -= OnInventoryUpdated;
                    _playerInventory.PassiveStackableItems.OnItemRemoved -= OnInventoryUpdated;
                    break;
                case ItemType.Passive:
                    _playerInventory.PassiveItems.OnItemAdded -= OnInventoryUpdated;
                    _playerInventory.PassiveItems.OnItemRemoved -= OnInventoryUpdated;
                    break;
                case ItemType.Active:
                    _playerInventory.ActiveItems.OnItemAdded -= OnInventoryUpdated;
                    _playerInventory.ActiveItems.OnItemRemoved -= OnInventoryUpdated;
                    break;
                case ItemType.Consumable:
                    _itemEffects.Where(e => e is AddResourceItemEffect)
                        .ForEach(e =>
                            (e as AddResourceItemEffect).Resource.OnAmountChanged -= InvokeOnPickupabilityChanged);
                    break;
            }
            
            OnPickupabilityChanged -= InvokeOnBuyabilityChanged;
            _itemCost.ForEach((KeyValuePair<PlayerResource, int> entry) => {
                entry.Key.OnAmountChanged -= InvokeOnBuyabilityChanged;
            });
        }
        
        #endregion
        
        #region Affordability/Store interface

        public delegate void _OnPickupStatusChanged();
        public event _OnPickupStatusChanged OnBuyabilityChanged;
        public event _OnPickupStatusChanged OnPickupabilityChanged;
        
        private void OnInventoryUpdated(PlayerItem playerItem)
        {
            InvokeOnBuyabilityChanged();
        }
        
        private void InvokeOnBuyabilityChanged()
        {
            OnBuyabilityChanged?.Invoke();
        }
        
        private void InvokeOnPickupabilityChanged()
        {
            OnPickupabilityChanged?.Invoke();
        }

        public bool CheckIfCanBuy()
        {
            return CheckIfCanAfford() && CheckIfCanPickup();
        }

        public bool CheckIfCanPickup()
        {
            // return AllowPickupCondition.Value;
            
            // TODO use inventory to check if can hold
            bool canPickup = true;
            switch (_itemType)
            {
                case ItemType.PassiveStackable:
                    // prevent if the player already holds this item
                    break;
                case ItemType.Passive:
                    break;
                case ItemType.Active:
                    break;
                case ItemType.Consumable:
                    // if it grants a resource, ensure there is space
                    var addResourceEffects = _itemEffects.Where(e => e is AddResourceItemEffect);
                    if (addResourceEffects.Any())
                    {
                        // Allow pickup if the item grants ANY resources that the player has room to hold
                        canPickup = addResourceEffects.Any(e => (e as AddResourceItemEffect).CanAddResource());
                    }
                    break;
            }

            return canPickup;
        }

        //TODO: return what resources you're short on
        private bool CheckIfCanAfford() 
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
