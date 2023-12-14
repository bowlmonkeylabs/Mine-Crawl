using BML.ScriptableObjectCore.Scripts.Variables;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class RestartTimerVariableItemEffect : ItemEffect
    {
        #region Inspector

        public TimerVariable TimerVariable;

        #endregion
        
        protected override bool ApplyEffectInternal()
        {
            TimerVariable.RestartTimer();

            return true;
        }

        protected override bool UnapplyEffectInternal()
        {
            return true;
        }

        public override void Reset()
        {
            
        }
        
    }
}