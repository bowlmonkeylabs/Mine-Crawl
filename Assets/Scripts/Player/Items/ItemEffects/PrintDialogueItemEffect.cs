using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.Player.Items;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class PrintDialogueItemEffect : ItemEffect
    {
        #region Inspector

        [AssetsOnly]
        [SerializeField] private DynamicGameEvent _printDialogueEvent;
        [SerializeField] private string _printDialogueString;

        #endregion
        
        protected override bool ApplyEffectInternal()
        {
            _printDialogueEvent.Raise(_printDialogueString);
            
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