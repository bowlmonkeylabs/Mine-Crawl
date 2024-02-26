using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using UnityEngine;

namespace BML.Scripts.Player.Items.Store
{
    public class ItemTreeGraphStoreInventory : AbstractItemStoreInventory
    {
        [SerializeField] protected ItemTreeGraph _itemTreeGraph;
        [SerializeField] protected List<PlayerItem> _fallbackOptions;
        [SerializeField] private string _callToActionUpgradeText = "Choose an upgrade:";
        
        protected override IEnumerable<PlayerItem> GetItemPool()
        {
            var upgradesFromTree = _itemTreeGraph.GetUnobtainedItemPool();
            if (upgradesFromTree.Count > 0)
            {
                return upgradesFromTree;
            }
            return _fallbackOptions;
        }

        public override string GetCallToActionText()
        {
            return _callToActionUpgradeText;
        }
        
    }
}