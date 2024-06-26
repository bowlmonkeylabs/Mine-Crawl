using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.ScriptableObjectCore.Scripts.Variables;
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

    [Flags]
    public enum SlotTypeFilter
    {
        None = 0,
        
        Torch = 1 << 0,
        Rope = 1 << 1,
        Bomb = 1 << 2,
        
        AbilityMovement = 1 << 5,
        AbilitySecondaryAttack = 1 << 6,
    }

    [InlineEditor()]
    [CreateAssetMenu(fileName = "PlayerItem", menuName = "BML/Player/PlayerItem", order = 0)]
    public class PlayerItem : SerializedScriptableObject, IResettableScriptableObject, IHasSlotType<SlotTypeFilter>
    {
        #region Inspector
        
        [SerializeField, FoldoutGroup("Descriptors")] protected string _name;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea] private string _effectDescription;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea] private string _storeDescription;
        [SerializeField, FoldoutGroup("Descriptors")] private bool _useIconColor;
        [SerializeField, FoldoutGroup("Descriptors"), ShowIf("_useIconColor")] private Color _iconColor = Color.white;
        [SerializeField, FoldoutGroup("Descriptors"), PreviewField(100, ObjectFieldAlignment.Left)] protected Sprite _icon;
        [SerializeField, FoldoutGroup("Descriptors"), ReadOnly, ShowIf("@_itemType == ItemType.PassiveStackable")]
        public ItemTreeGraphStartNode PassiveStackableTreeStartNode;
        
        [SerializeField, FoldoutGroup("Pickup"), PreviewField(100, ObjectFieldAlignment.Left), AssetsOnly] protected GameObject _objectPrefab;
        
        [FormerlySerializedAs("_storeCost")]
        [SerializeField]
        [FoldoutGroup("Store")]
        [DictionaryDrawerSettings(KeyLabel = "Resource", ValueLabel = "Amount", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private Dictionary<PlayerResource, int> _itemCost = new Dictionary<PlayerResource, int>();
        
        [SerializeField, FoldoutGroup("Effect")] private ItemType _itemType = ItemType.PassiveStackable;
        [FormerlySerializedAs("_slotType")] [SerializeField, FoldoutGroup("Effect")] private SlotTypeFilter slotTypeFilter = SlotTypeFilter.None;
        public SlotTypeFilter SlotTypeFilter => slotTypeFilter;
        [SerializeField, FoldoutGroup("Effect")]
        // [HideReferenceObjectPicker]
        [OnValueChanged("OnItemEffectsChangedInInspector")]
        private List<ItemEffect> _itemEffects = new List<ItemEffect>();

        private void OnItemEffectsChangedInInspector()
        {
            _itemEffects.ForEach(e => e.ParentItem = this);
        }
        
        #endregion
        
        #region Public interface
        
        public virtual string Name => _name;
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

        public virtual void OnAfterApplyEffect()
        {
            
        }

        #endregion

        public string FormatCostsAsText() 
        {
            return String.Join(" + ", _itemCost.Select((KeyValuePair<PlayerResource, int> entry) => $"{entry.Value}{entry.Key.IconText}"));
        }

        #region Unity lifecycle

        // private void OnEnable()
        // {
        //     // Normally the parent reference should be assigned automatically on creation, but use this to force the references to be assigned for all existing items.
        //     #if UNITY_EDITOR
        //     _itemEffects.ForEach(e => e.ParentItem = this);
        //     EditorUtility.SetDirty(this);
        //     // Then Ctrl + S to save all dirty objects
        //     #endif
        // }

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
