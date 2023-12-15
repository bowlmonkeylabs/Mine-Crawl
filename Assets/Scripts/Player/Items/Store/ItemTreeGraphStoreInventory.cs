using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts.Player.Items.Store
{
    public class ItemTreeGraphStoreInventory : AbstractItemStoreInventory
    {
        [SerializeField] private ItemTreeGraph _itemTreeGraph;
        
        protected override IEnumerable<PlayerItem> GetItemPool()
        {
            return _itemTreeGraph.GetUnobtainedItemPool();
        }
        
    }
}