using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Store
{
    [InlineEditor()]
    [CreateAssetMenu(fileName = "StoreItem", menuName = "BML/Store/StoreItem", order = 0)]
    public class StoreItem : ScriptableObject
    {
        [Tooltip("This name will be used for the UI button object (not the button label)")]
        public string _storeButtonName;
        
        [Tooltip("Use this to generated button from prefab variant instead of generic storeUiButton)")]
        public GameObject _uiReplacePrefab;
        
        [Tooltip("Event the store button raises when this item is selected in UI")]
        public DynamicGameEvent _onPurchaseEvent;
        
        public int _resourceCost;
        public int _rareResourceCost;
        public int _enemyResourceCost;
        
        [Tooltip("List of items to ")]
        public List<PurchaseItem> PurchaseItems;
    }

    [System.Serializable]
    public class PurchaseItem
    {
        public IntVariable _incrementOnPurchase;
        [Tooltip("Leave blank to hide this element in store text")]
        public string _storeText;
        public int _incrementAmount;
    }
}