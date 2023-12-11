using BML.ScriptableObjectCore.Scripts.Variables;

namespace BML.Scripts.Player.Items.ItemEffects
{
    // [Serializable]
    public class SetBoolVariableItemEffect : ItemEffect
    {
        #region Inspector

        public BoolVariable BoolVariable;

        #endregion
        protected override bool ApplyEffectInternal()
        {
            BoolVariable.Value = true;

            return true;
        }

        protected override bool UnapplyEffectInternal()
        {
            BoolVariable.Value = false;

            return true;
        }

        public override void Reset()
        {
            
        }
        
    }
}