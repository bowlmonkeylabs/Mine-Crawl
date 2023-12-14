using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Enemy;
using BML.Scripts.Player.Items.ItemEffects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BML.Scripts.Player.Items
{
    public class PlayerItemEffectsController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory _playerInventory;

        [SerializeField, FoldoutGroup("Player")] private Transform _mainCamera;
        [SerializeField, FoldoutGroup("Player")] private BoolVariable _inGodMode;
        [SerializeField, FoldoutGroup("Player")] private BoolVariable _inDash;
        
        [SerializeField, FoldoutGroup("Pickaxe Events")] private GameEvent _onSwingPickaxe;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private DynamicGameEvent _onSwingPickaxeHit;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private GameEvent _onSweepPickaxe;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private GameEvent _onSweepPickaxeHit;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private DynamicGameEvent _onSwingPickaxeCrit;
        
        [SerializeField, FoldoutGroup("Enemy Events")] private DynamicGameEvent _onEnemyKilled;

        [SerializeField, FoldoutGroup("Projectile")] private TransformSceneReference MainCameraRef;
        [SerializeField, FoldoutGroup("Projectile")] private TransformSceneReference ProjectileContainer;
        
        [SerializeField, FoldoutGroup("Throwable")] private TransformSceneReference ThrowableContainer;

        private List<PlayerItem> PassiveItems 
        {
            get
            {
                var items = new List<PlayerItem>(_playerInventory.PassiveStackableItems.Items);
                items.AddRange(_playerInventory.PassiveItems.Items);
                return items;
            }
        }
        
        private List<PlayerItem> AllItems 
        {
            get 
            {
                var items = new List<PlayerItem>(_playerInventory.PassiveStackableItems.Items);
                items.AddRange(_playerInventory.PassiveItems.Items);
                items.AddRange(_playerInventory.ActiveItems.Items);
                return items;
            }
        }

        private Vector3 _pickaxeHitPosition = Vector3.negativeInfinity;

        #region Unity lifecycle
        
        private void Start()
        {
            // This needs to run AFTER ScriptableObjectResetManager, which runs on Start (early)
            this.Apply_Passives_OnAcquired();
        }

        private void OnEnable()
        {
            RepopulateEffectsTimersList();

            _playerInventory.OnAnyPlayerItemAdded += CheckEffectsTimersListOnPlayerItemAdded;
            _playerInventory.OnAnyPlayerItemRemoved += CheckEffectsTimersListOnPlayerItemRemoved;
            
            _playerInventory.ActiveItems.OnAnyItemChangedInInspector += RepopulateEffectsTimersList;
            _playerInventory.PassiveStackableItems.OnAnyItemChangedInInspector += RepopulateEffectsTimersList;
            _playerInventory.PassiveStackableItemTrees.OnAnyItemChangedInInspector += RepopulateEffectsTimersList;
            
            _playerInventory.PassiveStackableItems.OnItemAdded += Apply_OnAcquired;
            _playerInventory.PassiveStackableItems.OnItemRemoved += Unapply_OnAcquired;
            _playerInventory.PassiveItems.OnItemAdded += Apply_OnAcquired;
            _playerInventory.PassiveItems.OnItemRemoved += Unapply_OnAcquired;
            _playerInventory.ConsumableItems.OnItemAdded += Apply_OnAcquired;
            _playerInventory.ConsumableItems.OnItemRemoved += Unapply_OnAcquired;

            _inDash.Subscribe(OnInDashChange);
            
            _onSwingPickaxe.Subscribe(Apply_Passives_OnPickaxeSwing);
            _onSwingPickaxeHit.Subscribe(Apply_Passives_OnPickaxeSwingHit);
            _onSweepPickaxe.Subscribe(Apply_Passives_OnPickaxeSweep);
            _onSweepPickaxeHit.Subscribe(Apply_Passives_OnPickaxeSweepHit);
            _onSwingPickaxeCrit.Subscribe(Apply_Passives_OnPickaxeSwingCrit);
            
            _onEnemyKilled.Subscribe(OnEnemyKilledDynamic);
        }

        private void OnDisable() 
        {
            this.Unapply_Passives_OnAcquired();
            
            _effectActivationCooldownTimersToUpdate.Clear();

            _playerInventory.OnAnyPlayerItemAdded -= CheckEffectsTimersListOnPlayerItemAdded;
            _playerInventory.OnAnyPlayerItemRemoved -= CheckEffectsTimersListOnPlayerItemRemoved;
            
            _playerInventory.ActiveItems.OnAnyItemChangedInInspector -= RepopulateEffectsTimersList;
            _playerInventory.PassiveStackableItems.OnAnyItemChangedInInspector -= RepopulateEffectsTimersList;
            _playerInventory.PassiveStackableItemTrees.OnAnyItemChangedInInspector -= RepopulateEffectsTimersList;
            
            _playerInventory.PassiveStackableItems.OnItemAdded -= Apply_OnAcquired;
            _playerInventory.PassiveStackableItems.OnItemRemoved -= Unapply_OnAcquired;
            _playerInventory.PassiveItems.OnItemAdded -= Apply_OnAcquired;
            _playerInventory.PassiveItems.OnItemRemoved -= Unapply_OnAcquired;
            _playerInventory.ConsumableItems.OnItemAdded -= Apply_OnAcquired;
            _playerInventory.ConsumableItems.OnItemRemoved -= Unapply_OnAcquired;

            _inDash.Unsubscribe(OnInDashChange);

            _onSwingPickaxe.Unsubscribe(Apply_Passives_OnPickaxeSwing);
            _onSwingPickaxeHit.Unsubscribe(Apply_Passives_OnPickaxeSwingHit);
            _onSweepPickaxe.Unsubscribe(Apply_Passives_OnPickaxeSweep);
            _onSweepPickaxeHit.Unsubscribe(Apply_Passives_OnPickaxeSweepHit);
            _onSwingPickaxeCrit.Unsubscribe(Apply_Passives_OnPickaxeSwingCrit);
            
            _onEnemyKilled.Unsubscribe(OnEnemyKilledDynamic);
        }

        void Update() 
        {
            Apply_Consumable_OnAcquired();
            
            UpdateEffectsTimers();
        }

        #endregion

        #region Input callbacks
        
        private void OnUseActiveItem1(InputValue value)
        {
            if (value.isPressed && _playerInventory.ActiveItems.ItemCount >= 1 && _playerInventory.ActiveItems[0] != null)
            {
                Apply_Active_OnActivated(0);
            }
        }
        
        private void OnUseActiveItem2(InputValue value)
        {
            if (value.isPressed && _playerInventory.ActiveItems.ItemCount >= 2 && _playerInventory.ActiveItems[1] != null)
            {
                Apply_Active_OnActivated(1);
            }
        }

        private void OnUseActiveItem3(InputValue value)
        {
            if (value.isPressed && _playerInventory.ActiveItems.ItemCount >= 3 && _playerInventory.ActiveItems[2] != null)
            {
                Apply_Active_OnActivated(2);
            }
        }

        private void OnUseActiveItem4(InputValue value)
        {
            if (value.isPressed && _playerInventory.ActiveItems.ItemCount >= 4 && _playerInventory.ActiveItems[3] != null)
            {
                Apply_Active_OnActivated(3);
            }
        }

        private void OnUseConsumableItem1(InputValue value)
        {
            if (value.isPressed && _playerInventory.ConsumableItems.ItemCount >= 1 && _playerInventory.ConsumableItems[0] != null)
            {
                Apply_Consumable_OnActivated(0);
            }
        }

        #endregion

        #region Variable update callbacks

        private void OnInDashChange(bool prevInDash, bool currInDash)
        {
            bool enteringDash = (!prevInDash && currInDash);
            if (enteringDash)
            {
                Apply_Passives_OnDash();
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
            if (payload.HitInfo != null && 
                (payload.HitInfo.DamageType & pickaxeDamageTypes) > 0)
            {
                Apply_Passives_OnPickaxeKillEnemy();
            }
        }

        #endregion

        #region Effects timers

        private List<TimerVariable> _effectActivationCooldownTimersToUpdate;
        private Dictionary<TimerVariable, HashSet<ItemEffect>> _effectRecurringTimersForTriggersToUpdate;
        
        private IEnumerable<TimerVariable> GetActivationCooldownTimers(IEnumerable<PlayerItem> playerItems)
        {
            return playerItems.SelectMany(i =>
                i.ItemEffects
                    .Where(e => e.UseActivationCooldownTimer)
                    .Select(e => e.ActivationCooldownTimer));
        }
        
        private Dictionary<TimerVariable, HashSet<ItemEffect>> GetRecurringTimerTriggers(IEnumerable<PlayerItem> playerItems)
        {
            var itemEffectsWithRecurringTimers = playerItems.SelectMany(item => item.ItemEffects
                .Where(e => e.Trigger == ItemEffectTrigger.RecurringTimer));
            return itemEffectsWithRecurringTimers
                .GroupBy(e => e.RecurringTimerForTrigger, e => e)
                .ToDictionary(g => g.Key, g => new HashSet<ItemEffect>(g));
        }

        private void RepopulateEffectsTimersList()
        {
            _effectActivationCooldownTimersToUpdate = new List<TimerVariable>();
            _effectActivationCooldownTimersToUpdate.AddRange(
                GetActivationCooldownTimers(_playerInventory.ActiveItems));
            _effectActivationCooldownTimersToUpdate.AddRange(
                GetActivationCooldownTimers(_playerInventory.PassiveItems));
            _effectActivationCooldownTimersToUpdate.AddRange(
                GetActivationCooldownTimers(_playerInventory.PassiveStackableItems));
            _effectActivationCooldownTimersToUpdate = _effectActivationCooldownTimersToUpdate.Distinct().ToList();

            _effectRecurringTimersForTriggersToUpdate = GetRecurringTimerTriggers(AllItems);
        }

        private void CheckEffectsTimersListOnPlayerItemAdded(PlayerItem playerItem)
        {
            {
                var itemActivationCooldownTimers = playerItem.ItemEffects
                    .Where(e => e.UseActivationCooldownTimer)
                    .Select(e => e.ActivationCooldownTimer)
                    .ToList();
                if (itemActivationCooldownTimers.Any())
                {
                    _effectActivationCooldownTimersToUpdate.AddRange(itemActivationCooldownTimers);
                    _effectActivationCooldownTimersToUpdate =
                        _effectActivationCooldownTimersToUpdate.Distinct().ToList();
                }
            }

            {
                var itemEffectsWithRecurringTimers = playerItem.ItemEffects
                    .Where(e => e.Trigger == ItemEffectTrigger.RecurringTimer)
                    .ToList();
                foreach (var itemEffect in itemEffectsWithRecurringTimers)
                {
                    if (_effectRecurringTimersForTriggersToUpdate.ContainsKey(itemEffect.RecurringTimerForTrigger))
                    {
                        _effectRecurringTimersForTriggersToUpdate[itemEffect.RecurringTimerForTrigger].Add(itemEffect);
                    }
                    else
                    {
                        _effectRecurringTimersForTriggersToUpdate.Add(itemEffect.RecurringTimerForTrigger, new HashSet<ItemEffect>(new List<ItemEffect> { itemEffect }));
                    }
                }
            }
        }
        
        private void CheckEffectsTimersListOnPlayerItemRemoved(PlayerItem playerItem)
        {
            RepopulateEffectsTimersList();
        }

        private void UpdateEffectsTimers()
        {
            foreach (var timer in _effectActivationCooldownTimersToUpdate)
            {
                timer.UpdateTime();
            }
            
            foreach (var kv in _effectRecurringTimersForTriggersToUpdate)
            {
                var timer = kv.Key;

                bool anyConditionFailing = kv.Value.Any(e => !e.RecurringTimerForTriggerCondition);
                
                if (anyConditionFailing) 
                {
                    if (timer.IsStarted && !timer.IsStopped && !timer.IsFinished)
                    {
                        timer.StopTimer();
                    }
                    continue;
                }
                else if (!timer.IsStarted || timer.IsStopped)
                {
                    timer.StartTimer();
                }
                timer.UpdateTime();
                if (timer.IsFinished) 
                {
                    foreach (var itemEffect in kv.Value)
                    {
                        this.ApplyEffect(itemEffect);
                    }
                    anyConditionFailing = kv.Value.Any(e => !e.RecurringTimerForTriggerCondition);
                    if (!anyConditionFailing)
                    {
                        timer.RestartTimer();
                    }
                    else
                    {
                        timer.ResetTimer();
                    }
                }
            }
        }

        #endregion
        
        #region Apply/Unapply effects callbacks
        
        #region Consumable item effects
        
        private void Apply_Consumable_OnActivated(int index)
        {
            var consumableItem = _playerInventory.ConsumableItems[index];
            this.ApplyOrUnApplyEffectsForTrigger(consumableItem, ItemEffectTrigger.OnActivated, true);
            
            // Remove consumable if used-up
            if ((consumableItem.RemainingActivations ?? 0) <= 0)
            {
                _playerInventory.TryRemoveConsumable(index);
            }
        }

        private void Apply_Consumable_OnAcquired() {
            ApplyOrUnApplyEffectsForTrigger(_playerInventory.OnAcquiredConsumableQueue, ItemEffectTrigger.OnAcquired,
                true);
            _playerInventory.OnAcquiredConsumableQueue.Clear();
        }
        
        #endregion
        
        #region Active item effects
        
        private void Apply_Active_OnActivated(int index) {
            this.ApplyOrUnApplyEffectsForTrigger(_playerInventory.ActiveItems[index], ItemEffectTrigger.OnActivated, true);
        }
        
        #endregion

        #region Passive and Passive stackable item effects
        
        private void Apply_Passives_OnAcquired() {
            ApplyOrUnApplyEffectsForTrigger(PassiveItems, ItemEffectTrigger.OnAcquired, true);
        }

        private void Unapply_Passives_OnAcquired() {
            ApplyOrUnApplyEffectsForTrigger(PassiveItems, ItemEffectTrigger.OnAcquired, false);
        }

        private void Apply_Passives_OnDash() {
            ApplyOrUnApplyEffectsForTrigger(PassiveItems, ItemEffectTrigger.OnDash, true);
        }
        
        private void Apply_Passives_OnPickaxeSwing() {
            ApplyOrUnApplyEffectsForTrigger(PassiveItems, ItemEffectTrigger.OnPickaxeSwing, true);
        }
        
        private void Apply_Passives_OnPickaxeSwingHit(object previousValue, object hitPosition) {
            _pickaxeHitPosition = (Vector3) hitPosition;
            ApplyOrUnApplyEffectsForTrigger(PassiveItems, ItemEffectTrigger.OnPickaxeSwingHit, true);
            _pickaxeHitPosition = Vector3.negativeInfinity;
        }
        
        private void Apply_Passives_OnPickaxeSwingCrit(object previousValue, object hitPosition)
        {
            _pickaxeHitPosition = (Vector3) hitPosition;
            ApplyOrUnApplyEffectsForTrigger(PassiveItems, ItemEffectTrigger.OnPickaxeSwingCrit, true);
            _pickaxeHitPosition = Vector3.negativeInfinity;
        }
        
        private void Apply_Passives_OnPickaxeSweep() {
            ApplyOrUnApplyEffectsForTrigger(PassiveItems, ItemEffectTrigger.OnPickaxeSweep, true);
        }
        
        private void Apply_Passives_OnPickaxeSweepHit() {
            ApplyOrUnApplyEffectsForTrigger(PassiveItems, ItemEffectTrigger.OnPickaxeSweepHit, true);
        }
        
        private void Apply_Passives_OnPickaxeKillEnemy() {
            ApplyOrUnApplyEffectsForTrigger(PassiveItems, ItemEffectTrigger.OnPickaxeKillEnemy, true);
        }

        #endregion

        #region Any item effects
        
        private void Apply_OnAcquired(PlayerItem playerItem) {
            this.ApplyOrUnApplyEffectsForTrigger(playerItem, ItemEffectTrigger.OnAcquired, true);
        }

        private void Unapply_OnAcquired(PlayerItem playerItem) {
            this.ApplyOrUnApplyEffectsForTrigger(playerItem, ItemEffectTrigger.OnAcquired, false);
        }
        
        private void ApplyOrUnApplyEffectsForTrigger(PlayerItem playerItem, ItemEffectTrigger itemEffectTrigger, bool apply = true)
        {
            var applyAction = (apply ? (Action<ItemEffect>)this.ApplyEffect : this.UnApplyEffect);
            var itemEffectsForTrigger = playerItem.ItemEffects.Where(e => e.Trigger == itemEffectTrigger);
            foreach (var itemEffect in itemEffectsForTrigger)
            {
                applyAction(itemEffect);
            }
        }
        
        private void ApplyOrUnApplyEffectsForTrigger(IEnumerable<PlayerItem> items, ItemEffectTrigger itemEffectTrigger, bool apply = true)
        {
            var applyAction = (apply ? (Action<ItemEffect>)this.ApplyEffect : this.UnApplyEffect);
            foreach (var item in items)
            {
                var itemEffectsForTrigger = item.ItemEffects.Where(e => e.Trigger == itemEffectTrigger);
                foreach (var itemEffect in itemEffectsForTrigger)
                {
                    applyAction(itemEffect);
                }
            }
        }

        #endregion
        
        #endregion
        
        private void ApplyEffect(ItemEffect itemEffect)
        {
            try 
            {
                // Handle any special cases that need "priming" with additional arguments
                if (itemEffect is InstantiatePrefabItemEffect asInstantiatePrefabItemEffect)
                {
                    asInstantiatePrefabItemEffect.PrimeEffect(_pickaxeHitPosition, this.transform);
                }
                else if (itemEffect is FireProjectileItemEffect asFireProjectileItemEffect)
                {
                    asFireProjectileItemEffect.PrimeEffect(MainCameraRef.Value.transform);
                }
                else if (itemEffect is ThrowItemEffect asThrowItemEffect)
                {
                    asThrowItemEffect.PrimeEffect(MainCameraRef.Value.transform);
                }
                
                itemEffect.ApplyEffect(_inGodMode.Value);

            } 
            catch(Exception exception) 
            {
                Debug.LogError($"Item effect failed to apply ({itemEffect.GetType().Name}, {itemEffect.Trigger}): {exception.Message} | {exception.InnerException}");
            }
        }

        private void UnApplyEffect(ItemEffect itemEffect)
        {
            itemEffect.UnapplyEffect();
        }
        
    }
}
