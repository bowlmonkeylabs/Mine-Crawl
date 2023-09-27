using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Enemy;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BML.Scripts.Player.Items
{
    public class PlayerItemEffectsController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory _playerInventory;

        [SerializeField, FoldoutGroup("Player")] private BoolVariable _inDash;
        
        [SerializeField, FoldoutGroup("Pickaxe Events")] private GameEvent _onSwingPickaxe;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private DynamicGameEvent _onSwingPickaxeHit;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private GameEvent _onSweepPickaxe;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private GameEvent _onSweepPickaxeHit;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private DynamicGameEvent _onSwingPickaxeCrit;
        
        [SerializeField, FoldoutGroup("Enemy Events")] private DynamicGameEvent _onEnemyKilled;

        [SerializeField, FoldoutGroup("Projectile")] private TransformSceneReference MainCameraRef;
        [SerializeField, FoldoutGroup("Projectile")] private TransformSceneReference ProjectileContainer;

        private List<PlayerItem> PassiveItems {
            get {
                var passiveItems = new List<PlayerItem>();
                passiveItems.AddRange(_playerInventory.PassiveStackableItems);
                if(_playerInventory.PassiveItem != null) {
                    passiveItems.Add(_playerInventory.PassiveItem);
                }
                return passiveItems;
            }
        }

        private Vector3 _pickaxeHitPosition = Vector3.negativeInfinity;

        #region Unity lifecycle
        
        private void Start()
        {
            // This needs to run AFTER ScriptableObjectResetManager, which runs on Start (early)
            this.ApplyWhenAcquiredOrActivatedEffectsForPassiveItems();
        }

        void OnEnable()
        {
            RepopulateEffectsTimersList();

            _playerInventory.OnAnyItemAdded += CheckEffectsTimersListOnItemAdded;
            _playerInventory.OnAnyItemRemoved += CheckEffectsTimersListOnItemRemoved;
            _playerInventory.OnPassiveStackableItemAdded += CheckEffectsTimersListOnItemAdded;
            _playerInventory.OnPassiveStackableItemRemoved += CheckEffectsTimersListOnItemRemoved;
            
            _playerInventory.OnPassiveStackableItemChanged += RepopulateEffectsTimersList;
            _playerInventory.OnPassiveStackableItemTreeChanged += RepopulateEffectsTimersList;
            
            _playerInventory.OnPassiveStackableItemAdded += ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveStackableItemRemoved += UnApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemAdded += ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemRemoved += UnApplyWhenAcquiredOrActivatedEffects;

            _inDash.Subscribe(OnInDashChange);
            
            _onSwingPickaxe.Subscribe(ApplyOnPickaxeSwingEffectsForPassiveItems);
            _onSwingPickaxeHit.Subscribe(ApplyOnPickaxeSwingHitEffectsForPassiveItems);
            _onSweepPickaxe.Subscribe(ApplyOnPickaxeSweepEffectsForPassiveItems);
            _onSweepPickaxeHit.Subscribe(ApplyOnPickaxeSweepHitEffectsForPassiveItems);
            _onSwingPickaxeCrit.Subscribe(ApplyOnPickaxeSwingCritEffectsForPassiveItems);
            
            _onEnemyKilled.Subscribe(OnEnemyKilledDynamic);
        }

        void OnDisable() {
            this.UnApplyWhenAcquiredOrActivatedEffectsForPassiveItems();
            
            _effectTimersToUpdate.Clear();

            _playerInventory.OnAnyItemAdded -= CheckEffectsTimersListOnItemAdded;
            _playerInventory.OnAnyItemRemoved -= CheckEffectsTimersListOnItemRemoved;
            _playerInventory.OnPassiveStackableItemAdded -= CheckEffectsTimersListOnItemAdded;
            _playerInventory.OnPassiveStackableItemRemoved -= CheckEffectsTimersListOnItemRemoved;
            
            _playerInventory.OnPassiveStackableItemChanged -= RepopulateEffectsTimersList;
            _playerInventory.OnPassiveStackableItemTreeChanged -= RepopulateEffectsTimersList;
            
            _playerInventory.OnPassiveStackableItemAdded -= ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveStackableItemRemoved -= UnApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemAdded -= ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemRemoved -= UnApplyWhenAcquiredOrActivatedEffects;

            _inDash.Unsubscribe(OnInDashChange);

            _onSwingPickaxe.Unsubscribe(ApplyOnPickaxeSwingEffectsForPassiveItems);
            _onSwingPickaxeHit.Unsubscribe(ApplyOnPickaxeSwingHitEffectsForPassiveItems);
            _onSweepPickaxe.Unsubscribe(ApplyOnPickaxeSweepEffectsForPassiveItems);
            _onSweepPickaxeHit.Unsubscribe(ApplyOnPickaxeSweepHitEffectsForPassiveItems);
            _onSwingPickaxeCrit.Unsubscribe(ApplyOnPickaxeSwingCritEffectsForPassiveItems);
            
            _onEnemyKilled.Unsubscribe(OnEnemyKilledDynamic);
        }

        void Update() {
            PassiveItems.ForEach(psi => psi.ItemEffects.ForEach(itemEffect => {
                if(itemEffect.Trigger == ItemEffectTrigger.RecurringTimer) {
                    if(Time.time - itemEffect.LastTimeCheck > itemEffect.Time) {
                        this.ApplyEffect(itemEffect);
                        itemEffect.LastTimeCheck = Time.time;
                    }
                }
            }));

            UpdateEffectsTimers();
        }

        #endregion

        #region Input callbacks

        private void OnUseActiveItem(InputValue value)
        {
            if (value.isPressed && _playerInventory.ActiveItem != null)
            {
                ApplyWhenAcquiredOrActivatedEffectsForActiveItem();
            }
        }

        #endregion

        #region Variable update callbacks

        private void OnInDashChange(bool prevInDash, bool currInDash)
        {
            bool enteringDash = (!prevInDash && currInDash);
            if (enteringDash)
            {
                ApplyOnDashEffectsForPassiveItems();
            }
        }

        #endregion

        #region Event callbacks

        private void OnEnemyKilledDynamic(object prev, object payload)
        {
            var typedPayload = payload as EnemyKilledPayload;
            if (payload == null) throw new ArgumentException("Invalid payload for OnEnemyKilled");
            
            OnEnemyKilled(typedPayload);
        }
        
        private void OnEnemyKilled(EnemyKilledPayload payload)
        {
            var pickaxeDamageTypes = DamageType.Player_Pickaxe | DamageType.Player_Pickaxe_Secondary;
            if ((payload.HitInfo.DamageType & pickaxeDamageTypes) > 0)
            {
                ApplyOnPickaxeKillEnemyEffectsForPassiveItems();
            }
        }

        #endregion

        #region Effects timers

        private List<TimerVariable> _effectTimersToUpdate;

        private void RepopulateEffectsTimersList()
        {
            _effectTimersToUpdate = new List<TimerVariable>();
            if (_playerInventory.ActiveItem)
            {
                _effectTimersToUpdate.AddRange(_playerInventory.ActiveItem.ItemEffects.Where(e => e.UseActivationCooldownTimer).Select(e => e.ActivationCooldownTimer));
            }
            if (_playerInventory.PassiveItem)
            {
                _effectTimersToUpdate.AddRange(_playerInventory.PassiveItem.ItemEffects.Where(e => e.UseActivationCooldownTimer).Select(e => e.ActivationCooldownTimer));
            }
            _effectTimersToUpdate.AddRange(_playerInventory.PassiveStackableItems.SelectMany(i => i.ItemEffects.Where(e => e.UseActivationCooldownTimer).Select(e => e.ActivationCooldownTimer)));
            _effectTimersToUpdate = _effectTimersToUpdate.Distinct().ToList();
        }

        private void CheckEffectsTimersListOnItemAdded(PlayerItem playerItem)
        {
            var itemTimers = playerItem.ItemEffects
                .Where(e => e.UseActivationCooldownTimer)
                .Select(e => e.ActivationCooldownTimer)
                .ToList();
            if (!itemTimers.Any())
            {
                return;
            }
            
            _effectTimersToUpdate.AddRange(itemTimers);
            _effectTimersToUpdate = _effectTimersToUpdate.Distinct().ToList();
        }
        
        private void CheckEffectsTimersListOnItemRemoved(PlayerItem playerItem)
        {
            RepopulateEffectsTimersList();
        }

        private void UpdateEffectsTimers()
        {
            foreach (var timer in _effectTimersToUpdate)
            {
                timer.UpdateTime();
            }
        }

        #endregion

        private void ApplyWhenAcquiredOrActivatedEffectsForActiveItem() {
            this.ApplyOrUnApplyEffectsForTrigger(_playerInventory.ActiveItem, ItemEffectTrigger.WhenAcquiredOrActivated, true);
        }

        private void UnApplyWhenAcquiredOrActivatedEffectsActive() {
            this.ApplyOrUnApplyEffectsForTrigger(_playerInventory.ActiveItem, ItemEffectTrigger.WhenAcquiredOrActivated, false);
        }
        
        private void ApplyOnDashEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnDash, true));
        }

        private void ApplyOnPickaxeSwingEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnPickaxeSwing, true));
        }

        private void ApplyOnPickaxeSwingHitEffectsForPassiveItems(object previousValue, object hitPosition) {
            _pickaxeHitPosition = (Vector3) hitPosition;
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnPickaxeSwingHit, true));
            _pickaxeHitPosition = Vector3.negativeInfinity;
        }

        private void ApplyOnPickaxeSwingCritEffectsForPassiveItems(object previousValue, object hitPosition)
        {
            _pickaxeHitPosition = (Vector3) hitPosition;
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnPickaxeSwingCrit, true));
            _pickaxeHitPosition = Vector3.negativeInfinity;
        }

        private void ApplyOnPickaxeSweepEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnPickaxeSweep, true));
        }

        private void ApplyOnPickaxeSweepHitEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnPickaxeSweepHit, true));
        }

        private void ApplyOnPickaxeKillEnemyEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnPickaxeKillEnemy, true));
        }

        private void ApplyWhenAcquiredOrActivatedEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.WhenAcquiredOrActivated, true));
        }

        private void UnApplyWhenAcquiredOrActivatedEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.WhenAcquiredOrActivated, false));
        }

        private void ApplyWhenAcquiredOrActivatedEffects(PlayerItem playerItem) {
            this.ApplyOrUnApplyEffectsForTrigger(playerItem, ItemEffectTrigger.WhenAcquiredOrActivated, true);
        }

        private void UnApplyWhenAcquiredOrActivatedEffects(PlayerItem playerItem) {
            this.ApplyOrUnApplyEffectsForTrigger(playerItem, ItemEffectTrigger.WhenAcquiredOrActivated, false);
        }

        private void ApplyOrUnApplyEffectsForTrigger(PlayerItem playerItem, ItemEffectTrigger itemEffectTrigger, bool apply = true) {
            playerItem.ItemEffects.ForEach(itemEffect => {
                if(itemEffect.Trigger == itemEffectTrigger) {
                    if(apply) {
                        this.ApplyEffect(itemEffect);
                    } else {
                        this.UnApplyEffect(itemEffect);
                    }
                }
            });
        }

        private void ApplyEffect(ItemEffect itemEffect)
        {
            try {
                if (itemEffect.UseActivationLimit && itemEffect.RemainingActivations.Value <= 0)
                {
                    return;
                }
                
                if (itemEffect.UseActivationCooldownTimer)
                {
                    if (itemEffect.ActivationCooldownTimer.IsStarted && !itemEffect.ActivationCooldownTimer.IsFinished)
                    {
                        return;
                    }
                    itemEffect.ActivationCooldownTimer.RestartTimer();
                }
                
                if (itemEffect.UseActivationLimit)
                {
                    itemEffect.RemainingActivations.Value -= 1;
                }
                
                if(itemEffect.Type == ItemEffectType.StatIncrease) {
                    if(itemEffect.UsePercentageIncrease) {
                        itemEffect.FloatStat.Value += itemEffect.FloatStat.DefaultValue * (itemEffect.StatIncreasePercent / 100f);
                    } else {
                        itemEffect.IntStat.Value += itemEffect.StatIncreaseAmount;
                    }

                    if(itemEffect.IsTemporaryStatIncrease) {
                        LeanTween.value(0f, 1f, itemEffect.TemporaryStatIncreaseTime)
                            .setOnComplete(_ => this.UnApplyEffect(itemEffect));
                    }   
                }

                if(itemEffect.Type == ItemEffectType.FireProjectile) {
                    var projectile = GameObjectUtils.SafeInstantiate(true, itemEffect.ProjectilePrefab, ProjectileContainer.Value);
                    var mainCamera = MainCameraRef.Value.transform;
                    projectile.transform.SetPositionAndRotation(mainCamera.position, mainCamera.rotation);
                }

                if(itemEffect.Type == ItemEffectType.ChangeLootTable) {
                    itemEffect.LootTableVariable.Value.ModifyProbability(itemEffect.LootTableKey, itemEffect.LootTableAddAmount);
                }

                if(itemEffect.Type == ItemEffectType.ToggleBoolVariable) {
                itemEffect.BoolVariableToToggle.Value = true;
                }

                if(itemEffect.Type == ItemEffectType.InstantiatePrefab) {
                    var gameObject = GameObjectUtils.SafeInstantiate(true, itemEffect.PrefabToInstantiate, itemEffect.InstantiatePrefabContainer?.Value);
                    var position = itemEffect.InstantiatePrefabPositionTransform?.Value.position ?? (_pickaxeHitPosition != Vector3.negativeInfinity ? _pickaxeHitPosition : transform.position);
                    var rotation = itemEffect.InstantiatePrefabPositionTransform?.Value.rotation ?? transform.rotation;
                    gameObject.transform.SetPositionAndRotation(position, rotation);
                }
            } catch(Exception exception) {
                Debug.LogWarning("Item effect failed to apply: " + exception.Message);
            }
        }

        private void UnApplyEffect(ItemEffect itemEffect) {
            if (itemEffect.UseActivationCooldownTimer)
            {
                itemEffect.ActivationCooldownTimer.ResetTimer();
            }
            
            if(itemEffect.Type == ItemEffectType.StatIncrease) {
                if(itemEffect.UsePercentageIncrease) {
                    itemEffect.FloatStat.Value -= itemEffect.FloatStat.DefaultValue * (itemEffect.StatIncreasePercent / 100f);
                } else {
                    itemEffect.IntStat.Value -= itemEffect.StatIncreaseAmount;
                }
            }

            if(itemEffect.Type == ItemEffectType.ToggleBoolVariable) {
               itemEffect.BoolVariableToToggle.Value = false;
            }
        }
    }
}
