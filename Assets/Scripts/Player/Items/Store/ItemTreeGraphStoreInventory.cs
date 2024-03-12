using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.CaveV2;
using UnityEngine;

namespace BML.Scripts.Player.Items.Store
{
    public class ItemTreeGraphStoreInventory : AbstractItemStoreInventory
    {
        [SerializeField] private SafeIntValueReference _playerLevel;
        [SerializeField] private SafeIntValueReference _playerUnspentLevelCount;
        
        [SerializeField] private ItemTreeGraph _itemTreeGraph;
        [SerializeField] private List<PlayerItem> _fallbackOptions;
        
        protected override IEnumerable<PlayerItem> GetItemPool()
        {
            var effectivePlayerLevel = _playerLevel.Value - _playerUnspentLevelCount.Value;
            var upgradesFromTree = _itemTreeGraph.GetUnobtainedItemPool(effectivePlayerLevel);
            if (upgradesFromTree.Count > 0)
            {
                return upgradesFromTree;
            }
            return _fallbackOptions;
        }
    }
}