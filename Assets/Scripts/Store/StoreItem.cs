using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;

namespace BML.Scripts.Store
{
    [InlineEditor()]
    [CreateAssetMenu(fileName = "StoreItem", menuName = "BML/Store/StoreItem", order = 0)]
    public class StoreItem : ScriptableObject
    {
        [PropertySpace(5f, 0f)]
        [BoxGroup("Cost")] [HorizontalGroup("Cost/H", 10f)]
        [LabelText("R")] [LabelWidth(25f)]
        public int _resourceCost;
        
        [BoxGroup("Cost")] [HorizontalGroup("Cost/H", 10f)]
        [LabelText("RR")] [LabelWidth(25f)]
        public int _rareResourceCost;
        
        [BoxGroup("Cost")] [HorizontalGroup("Cost/H", 10f)]
        [LabelText("ER")] [LabelWidth(25f)]
        public int _enemyResourceCost;

        [FoldoutGroup("Store Options"), Tooltip("How much of this item is the player buying?")]
        public int _buyAmount = 1;

        [FoldoutGroup("Store Options")]
        [Tooltip("Is the player limited to a certain amount of this item?")]
        public bool _hasMaxAmount;
        
        [FoldoutGroup("Store Options")]
        [ShowIf("_hasMaxAmount")]
        public IntReference _maxAmount;

        [FoldoutGroup("Store Options")]
        [Tooltip("The amount of this item the player already has (can be bool)")]
        public SafeIntValueReference _playerInventoryAmount;

        [FoldoutGroup("Descriptors"), Tooltip("Text representation of item")]
        public string _itemLabel;

        [FoldoutGroup("Descriptors"), Tooltip("Icon representation of item")]
        public Sprite _itemIcon;

        [FoldoutGroup("Descriptors"), Tooltip("Description of item's effects"), TextArea]
        public string _itemEffectDescription;

        [FoldoutGroup("Descriptors"), Tooltip("Lore description of item"), TextArea]
        public string _itemStoreDescription;

        [FoldoutGroup("Effects")]
        [Tooltip("Effects to induce when item is bought")]
        public UnityEvent _onPurchased;

        [FoldoutGroup("Effects")]
        [Tooltip("Effects to induce when item is sold")]
        public UnityEvent _onSold;
    }
}