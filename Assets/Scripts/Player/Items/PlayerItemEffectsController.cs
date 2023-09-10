using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BML.Scripts.Player.Items
{
    public class PlayerItemEffectsController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private GameEvent _onSwingPickaxe;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private GameEvent _onSwingPickaxeHit;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private GameEvent _onSweepPickaxe;
        [SerializeField, FoldoutGroup("Pickaxe Events")] private GameEvent _onSweepPickaxeHit;

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

        #region Unity lifecycle

        void OnEnable() {
            this.ApplyWhenAcquiredOrActivatedEffectsForPassiveItems();

            _playerInventory.OnPassiveStackableItemAdded += ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveStackableItemRemoved += UnApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemAdded += ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemRemoved += UnApplyWhenAcquiredOrActivatedEffects;

            _onSwingPickaxe.Subscribe(ApplyOnPickaxeSwingEffectsForPassiveItems);
            _onSwingPickaxeHit.Subscribe(ApplyOnPickaxeSwingHitEffectsForPassiveItems);
            _onSweepPickaxe.Subscribe(ApplyOnPickaxeSweepEffectsForPassiveItems);
            _onSweepPickaxeHit.Subscribe(ApplyOnPickaxeSweepHitEffectsForPassiveItems);
        }

        void OnDisable() {
           UnApplyWhenAcquiredOrActivatedEffectsForPassiveItems();

            _playerInventory.OnPassiveStackableItemAdded -= ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveStackableItemRemoved -= UnApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemAdded -= ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemRemoved -= UnApplyWhenAcquiredOrActivatedEffects;

            _onSwingPickaxe.Unsubscribe(ApplyOnPickaxeSwingEffectsForPassiveItems);
            _onSwingPickaxeHit.Unsubscribe(ApplyOnPickaxeSwingHitEffectsForPassiveItems);
            _onSweepPickaxe.Unsubscribe(ApplyOnPickaxeSweepEffectsForPassiveItems);
            _onSweepPickaxeHit.Unsubscribe(ApplyOnPickaxeSweepHitEffectsForPassiveItems);
        }

        void Update() {
            PassiveItems.ForEach(psi => psi.ItemEffects.ForEach(itemEffect => {
                if(itemEffect.Trigger == ItemEffectTrigger.RecurringTimer) {
                    if(itemEffect.Time > (Time.time - itemEffect.LastTimeCheck)) {
                        Debug.Log("Hello?");
                        this.ApplyEffect(itemEffect);
                    }
                    itemEffect.LastTimeCheck = Time.time;
                }
            }));
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

        private void ApplyWhenAcquiredOrActivatedEffectsForActiveItem() {
            this.ApplyOrUnApplyEffectsForTrigger(_playerInventory.ActiveItem, ItemEffectTrigger.WhenAcquiredOrActivated, true);
        }

        private void UnApplyWhenAcquiredOrActivatedEffectsActive() {
            this.ApplyOrUnApplyEffectsForTrigger(_playerInventory.ActiveItem, ItemEffectTrigger.WhenAcquiredOrActivated, false);
        }

        private void ApplyOnPickaxeSwingEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnPickaxeSwing, true));
        }

        private void ApplyOnPickaxeSwingHitEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnPickaxeSwingHit, true));
        }

        private void ApplyOnPickaxeSweepEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnPickaxeSweep, true));
        }

        private void ApplyOnPickaxeSweepHitEffectsForPassiveItems() {
            PassiveItems.ForEach(pi => this.ApplyOrUnApplyEffectsForTrigger(pi, ItemEffectTrigger.OnPickaxeSweepHit, true));
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

        private void ApplyEffect(ItemEffect itemEffect) {
            if(itemEffect.Type == ItemEffectType.StatIncrease) {
                itemEffect.Stat.Value += itemEffect.StatIncreaseAmount;
            }

            if(itemEffect.Type == ItemEffectType.FireProjectile) {
                // container
                // var projectile = GameObjectUtils.SafeInstantiate(true, itemEffect.ProjectilePrefab);
                // projectile.transform.SetPositionAndRotation(_mainCamera.transform.position, _mainCamera.transform.rotation);
                // Calculate throw
            // var throwDir = _mainCamera.forward;
            // var throwForce = throwDir * force;
            
            // // Instantiate throwable
            // var newGameObject = 
            // newGameObject.transform.SetPositionAndRotation(_mainCamera.transform.position, _mainCamera.transform.rotation);
            // var throwable = newGameObject.GetComponentInChildren<Throwable>();
            // throwable.DoThrow(throwForce);
            }

            if(itemEffect.Type == ItemEffectType.ChangeLootTable) {
               itemEffect.LootTableToOverride.OverrideLootTable(itemEffect.OveridingLootTable);
            }
        }

        private void UnApplyEffect(ItemEffect itemEffect) {
            if(itemEffect.Type == ItemEffectType.StatIncrease) {
                itemEffect.Stat.Value -= itemEffect.StatIncreaseAmount;
            }
        }
    }
}
