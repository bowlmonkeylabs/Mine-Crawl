using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class TimerVariableItemEffect : ItemEffect
    {
        private enum TimerFunction
        {
            Reset = 0,
            Restart = 1,
            Start = 2,
            Stop = 3,
        }
        
        #region Inspector

        public TimerVariable TimerVariable;
        [SerializeField] private TimerFunction _timerFunctionToCall;

        #endregion
        
        protected override bool ApplyEffectInternal()
        {
            switch (_timerFunctionToCall)
            {
                case TimerFunction.Reset:
                    TimerVariable.ResetTimer();
                    break;
                case TimerFunction.Restart:
                    TimerVariable.RestartTimer();
                    break;
                case TimerFunction.Start:
                    TimerVariable.StartTimer();
                    break;
                case TimerFunction.Stop:
                    TimerVariable.StopTimer();
                    break;
            }
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