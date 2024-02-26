using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using UnityEngine;

namespace BML.Scripts.Player.Items.Store
{
    public class UpgradeItemStoreInventory : ItemTreeGraphStoreInventory
    {
        [SerializeField] private PlayerAbilityItems _playerAbilityItems;
        [SerializeField] private string _callToActionAbilityText = "Choose an ability:";
        
        protected override IEnumerable<PlayerItem> GetItemPool()
        {
            if(_playerAbilityItems.AbilityUpgradeAvailable()) {
                var steppedSeed = SeedManager.Instance.GetSteppedSeed("ItemTreeGraph_Ability");
                Random.InitState(steppedSeed);
                
                var unobtainedAbilityType = _playerAbilityItems.UnobtainedAbilityTypes().OrderBy(i => Random.value).ToList()[0];

                return _playerAbilityItems.GetAbilityOptionsForType(unobtainedAbilityType);
            }

            return base.GetItemPool();
        }

        public override string GetCallToActionText()
        {
            if(_playerAbilityItems.AbilityUpgradeAvailable()) {
                return _callToActionAbilityText;
            }

            return base.GetCallToActionText();
        }
        
    }
}