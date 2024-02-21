using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using UnityEngine;

namespace BML.Scripts.Player.Items.Store
{
    public class ItemTreeGraphStoreInventory : AbstractItemStoreInventory
    {
        [SerializeField] private ItemTreeGraph _itemTreeGraph;
        [SerializeField] private List<PlayerItem> _fallbackOptions;

        [SerializeField] private PlayerAbilityItems _playerAbilityItems;
        
        protected override IEnumerable<PlayerItem> GetItemPool()
        {
            if(_playerAbilityItems.AbilityUpgradeAvailable()) {
                var steppedSeed = SeedManager.Instance.GetSteppedSeed("ItemTreeGraph_Ability");
                Random.InitState(steppedSeed);
                
                var unobtainedAbilityType = _playerAbilityItems.UnobtainedAbilityTypes().OrderBy(i => Random.value).ToList()[0];

                return _playerAbilityItems.GetAbilityOptionsForType(unobtainedAbilityType);
            }

            var upgradesFromTree = _itemTreeGraph.GetUnobtainedItemPool();
            if (upgradesFromTree.Count > 0)
            {
                return upgradesFromTree;
            }
            return _fallbackOptions;
        }
        
    }
}