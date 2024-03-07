using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.CaveV2;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.Player.Items
{
    public enum ItemEffectTrigger
    {
        OnAcquired = 0,
        OnActivated = 1,
        RecurringTimer = 2,
        OnDash = 3,
        OnPickaxeSwing = 4,
        OnPickaxeSwingHit = 5,
        OnPickaxeSwingCrit = 6,
        OnPickaxeSweep = 7,
        OnPickaxeSweepHit = 8,
        OnPickaxeKillEnemy = 9,
        OnPickaxeMineHit = 10,
        OnPickaxeMineBreak = 11,
        OnMoveTopSpeed = 12,
    }
    
    [Serializable]
    // [HideReferenceObjectPicker]
    // [InlineProperty]
    public abstract class ItemEffect
    {
        #region Inspector

        [InfoBox("The parent item for this effect is not assigned; Something has gone horribly wrong!", InfoMessageType.Error, "_parentItemNotAssigned")]
        [SerializeField] 
        [PropertySpace(SpaceBefore = 20)]
        private ItemEffectTrigger _trigger;

        [SerializeField] private bool _useActivationLimit = false;
        [SerializeField, ShowIf("_useActivationLimit")] private IntVariable _remainingActivations;
        
        [SerializeField] private bool _useActivationCooldownTimer = false;
        [SerializeField, ShowIf("_useActivationCooldownTimer")] private TimerVariable _activationCooldownTimer;

        [SerializeField] private bool _useActivationChance;
        [SerializeField, ShowIf("_useActivationChance"), Range(0, 1)] private float _minActivationChance;
        [SerializeField, ShowIf("_useActivationChance")] private IntVariable _playerLuckStat;
        [SerializeField, ShowIf("_useActivationChance"), Range(0, 20)] private int _luckForMaxActivationChance;

        [SerializeField, ShowIf("@this.Trigger == ItemEffectTrigger.RecurringTimer")] 
        private TimerVariable _recurringTimerForTrigger;
        [SerializeField, ShowIf("@this.Trigger == ItemEffectTrigger.RecurringTimer")] 
        [Tooltip("The recurring timer only runs while this is true.")] 
        private SafeBoolValueReference _recurringTimerForTriggerCondition;

        #endregion

        #region Public interface

        public ItemEffectTrigger Trigger => _trigger;
        public bool UseActivationLimit => _useActivationLimit;
        public IntVariable RemainingActivations => _remainingActivations;
        public bool UseActivationCooldownTimer => _useActivationCooldownTimer;
        public TimerVariable ActivationCooldownTimer => _activationCooldownTimer;
        public float ActivationChance => !_useActivationChance
            ? 1f
            : (_minActivationChance + ((float)_playerLuckStat.Value / _luckForMaxActivationChance) * (1 - _minActivationChance));
        public TimerVariable RecurringTimerForTrigger => _recurringTimerForTrigger;
        public bool RecurringTimerForTriggerCondition => _recurringTimerForTriggerCondition.Value;

        [HideInInspector]
        public PlayerItem ParentItem;
        private bool _parentItemNotAssigned => ParentItem == null;
        private string SteppedSeedKey => $"{ParentItem.Name}_{Trigger}_{ParentItem.ItemEffects.IndexOf(this)}";

        public bool ApplyEffect(bool isGodMode)
        {
            if (_useActivationLimit && _remainingActivations.Value <= 0 && !isGodMode)
            {
                return false;
            }

            if (_useActivationCooldownTimer && !isGodMode)
            {
                if (_activationCooldownTimer.IsStarted && !_activationCooldownTimer.IsFinished)
                {
                    return false;
                }
            }

            if (_useActivationChance && !isGodMode)
            {
                SeedManager.Instance.UpdateSteppedSeed(SteppedSeedKey);
                Random.InitState(SeedManager.Instance.GetSteppedSeed(SteppedSeedKey));
                float randomRoll = Random.value;
                if (randomRoll > ActivationChance)
                {
                    return false;
                }
            }

            if (_useActivationCooldownTimer && !isGodMode)
            {
                _activationCooldownTimer.RestartTimer();
            }

            if (_useActivationLimit && !isGodMode)
            {
                _remainingActivations.Value -= 1;
            }

            return ApplyEffectInternal();
        }
        
        protected abstract bool ApplyEffectInternal();

        public bool UnapplyEffect()
        {
            if (_useActivationCooldownTimer)
            {
                _activationCooldownTimer.ResetTimer();
            }

            if (_trigger == ItemEffectTrigger.RecurringTimer)
            {
                _recurringTimerForTrigger.ResetTimer();
            }

            return UnapplyEffectInternal();
        }
        
        protected abstract bool UnapplyEffectInternal();

        public virtual void Reset()
        {
            if (_trigger == ItemEffectTrigger.RecurringTimer)
            {
                _recurringTimerForTrigger.ResetTimer();
            }
        }

        #endregion
    }
}