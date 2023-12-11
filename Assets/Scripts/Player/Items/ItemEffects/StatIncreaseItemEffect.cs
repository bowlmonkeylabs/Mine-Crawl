using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;

namespace BML.Scripts.Player.Items.ItemEffects
{
    [Serializable]
    public class StatIncreaseItemEffect : ItemEffect
    {
        #region Inspector
        
        public bool UsePercentageIncrease = false;
        public bool IsTemporaryStatIncrease = false;
        [ShowIf("IsTemporaryStatIncrease")] public float TemporaryStatIncreaseTime;
        [ShowIf("UsePercentageIncrease")] public FloatVariable FloatStat;
        [HideIf("UsePercentageIncrease")] public IntVariable IntStat;
        [HideIf("UsePercentageIncrease")] public int StatIncreaseAmount;
        [ShowIf("UsePercentageIncrease")] public float StatIncreasePercent;

        #endregion
        
        protected override bool ApplyEffectInternal()
        {
            if (UsePercentageIncrease)
            {
                FloatStat.Value += FloatStat.DefaultValue * (StatIncreasePercent / 100f);
            }
            else
            {
                IntStat.Value += StatIncreaseAmount;
            }

            if (IsTemporaryStatIncrease)
            {
                // TODO does this LT to be attached to GameObject to work properly?
                LeanTween.value(0f, 1f, TemporaryStatIncreaseTime)
                    .setOnComplete(_ => this.UnapplyEffectInternal());
            }

            return true;
        }

        protected override bool UnapplyEffectInternal()
        {
            if (UsePercentageIncrease) 
            {
                FloatStat.Value -= FloatStat.DefaultValue * (StatIncreasePercent / 100f);
            } 
            else 
            {
                IntStat.Value -= StatIncreaseAmount;
            }

            return true;
        }

        public override void Reset()
        {
            
        }
        
    }
}