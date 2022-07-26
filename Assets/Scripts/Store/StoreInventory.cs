using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Store
{
    [InlineEditor()]
    [CreateAssetMenu(fileName = "StoreInventory", menuName = "BML/Store/StoreInventory", order = 0)]
    public class StoreInventory : ScriptableObject
    {
        [SerializeField] private List<StoreItem> _storeItems;
        public List<StoreItem> StoreItems => _storeItems;
    }
}