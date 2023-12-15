using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.Player.Items.Store
{
    public class PlayerItemStoreInventory : AbstractItemStoreInventory
    {
        [SerializeField] private List<PlayerItem> _itemPool;
        [SerializeField] private DynamicGameEvent _tryOpenMerchantWithInventory;
        
        protected override IEnumerable<PlayerItem> GetItemPool()
        {
            return _itemPool;
        }

        public void TryOpenStore()
        {
            _tryOpenMerchantWithInventory.Raise(this);
        }
        
    }
}