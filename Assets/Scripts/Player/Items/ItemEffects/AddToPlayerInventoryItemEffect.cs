using BML.Scripts.CaveV2;
using BML.Scripts.ScriptableObjectVariables;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class AddToPlayerInventoryItemEffect : ItemEffect
    {
        #region Inspector

        [SerializeField] private bool _useLootTable = false;
        [SerializeField, HideIf("_useLootTable")] private PlayerItem _item;
        [SerializeField, ShowIf("_useLootTable"), HideReferenceObjectPicker, HideLabel] private ItemLootTableReference _lootTable;
        
        #endregion

        private PlayerInventory _playerInventory;
        
        public void PrimeEffect(PlayerInventory playerInventory)
        {
            _playerInventory = playerInventory;
        }

        protected override bool ApplyEffectInternal()
        {
            if (_useLootTable)
            {
                SeedManager.Instance.UpdateSteppedSeed(SteppedSeedKey);
                Random.InitState(SeedManager.Instance.GetSteppedSeed(SteppedSeedKey));
                var randomRoll = Random.value;
                var lootTableEntry = _lootTable.Value.Evaluate(randomRoll);

                bool allAdded = true;
                foreach (var playerItem in lootTableEntry.Drops)
                {
                    bool didAdd = _playerInventory.TryAddItem(playerItem, true, true);
                    if (!didAdd) allAdded = false;
                }
                return allAdded;
            }
            else
            {
                bool didAdd = _playerInventory.TryAddItem(_item, true, true);
                return didAdd;
            }
        }

        protected override bool UnapplyEffectInternal()
        {
            bool didRemove = _playerInventory.TryRemoveItem(_item);
            return didRemove;
        }
        
    }
}