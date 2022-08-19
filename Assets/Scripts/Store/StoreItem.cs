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
        public string _storeText;
        public GameObject _uiReplacePrefab;
        public DynamicGameEvent _onPurchaseEvent;
        public IntReference _incrementOnPurchase;
        public IntReference _incrementAmount;
        public IntReference _resourceCost;
        public IntReference _rareResourceCost;
        public IntReference _enemyResourceCost;
    }
}