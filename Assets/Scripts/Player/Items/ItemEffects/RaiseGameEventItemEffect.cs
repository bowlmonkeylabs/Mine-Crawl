using BML.ScriptableObjectCore.Scripts.Events;
using UnityEngine;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class RaiseGameEventItemEffect : ItemEffect
    {
        #region Inspector

        [SerializeField] private GameEvent _gameEvent;

        #endregion
        
        protected override bool ApplyEffectInternal()
        {
            _gameEvent.Raise();
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