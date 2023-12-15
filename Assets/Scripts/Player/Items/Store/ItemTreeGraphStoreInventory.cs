using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts.Player.Items.Store
{
    public class ItemTreeGraphStoreInventory : AbstractItemStoreInventory
    {
        [SerializeField] private ItemTreeGraph _itemTreeGraph;
        [SerializeField] private List<PlayerItem> _fallbackOptions;
        
        protected override IEnumerable<PlayerItem> GetItemPool()
        { 
            var upgradesFromTree = _itemTreeGraph.GetUnobtainedItemPool();
            if (upgradesFromTree.Count > 0)
            {
                return upgradesFromTree;
            }
            return _fallbackOptions;
        }
        
    }
}