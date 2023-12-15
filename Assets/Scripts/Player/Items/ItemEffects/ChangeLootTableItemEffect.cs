using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.ScriptableObjectVariables;
using Sirenix.OdinInspector;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class ChangeLootTableItemEffect : ItemEffect
    {
        #region Inspector

        public ItemLootTableVariable LootTableVariable;
        
        private bool _lootTableKeyDoesNotExist => LootTableVariable != null && LootTableVariable.Value != null && !LootTableVariable.Value.HasKey(LootTableKey);
        [InfoBox("Key does not exist in selected loot table.", InfoMessageType.Error, visibleIfMemberName:"_lootTableKeyDoesNotExist")]
        public LootTableKey LootTableKey;
        
        public float LootTableAddAmount;

        #endregion
        protected override bool ApplyEffectInternal()
        {
            LootTableVariable.Value.ModifyProbability(LootTableKey, LootTableAddAmount);

            return true;
        }

        protected override bool UnapplyEffectInternal()
        {
            // Changes to the loot table are not exactly reversible if something else has modified it since. if we need this to be accurate, I think we should store a history of all adjustments to the loot table, rather than destructively modifiying it
            LootTableVariable.Value.ModifyProbability(LootTableKey, -LootTableAddAmount);

            return true;
        }

        public override void Reset()
        {
            
        }
        
    }
}