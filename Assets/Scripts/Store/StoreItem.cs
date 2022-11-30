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
        [Tooltip("Label to show to represent item on store button")]
        public string _itemLabel;

        [HorizontalGroup("MaxAmount", 10f)]
        [Tooltip("Is the player limited to a certain amount of this item?")]
        public bool _hasMaxAmount;
        
        [HorizontalGroup("MaxAmount", 10f)]
        [ShowIf("_hasMaxAmount")]
        public IntReference _maxAmount;

        [Tooltip("The amount of this item the player already has (can be bool)")]
        public SafeIntValueReference _playerInventoryAmount;
        
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
        
        [Tooltip("List of items to Increment on Purchase")]
        [PropertySpace(5f, 0f)]
        public List<PurchaseItem> PurchaseItems;
    }

    [System.Serializable]
    public class PurchaseItem
    {
        [Tooltip("Leave blank to hide this element in store text")]
        public string _storeText;
        public UnityEvent OnPurchase;
    }
}