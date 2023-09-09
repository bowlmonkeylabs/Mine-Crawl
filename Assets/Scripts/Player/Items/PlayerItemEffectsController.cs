using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BML.Scripts.Player.Items
{
    public class PlayerItemEffectsController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory _playerInventory;

        #region Unity lifecycle

        void OnEnable() {
            _playerInventory.PassiveStackableItems.ForEach(psi => psi.ItemEffects.ForEach(itemEffect => {
                if(itemEffect.Trigger == ItemEffectTrigger.WhenAcquiredOrActivated) {
                    this.ApplyEffect(itemEffect);
                }
            }));
            _playerInventory.PassiveItem?.ItemEffects.ForEach(itemEffect => {
                if(itemEffect.Trigger == ItemEffectTrigger.WhenAcquiredOrActivated) {
                    this.ApplyEffect(itemEffect);
                }
            });
            _playerInventory.OnPassiveStackableItemAdded += ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveStackableItemRemoved += UnApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemAdded += ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemRemoved += UnApplyWhenAcquiredOrActivatedEffects;
        }

        void OnDisable() {
            _playerInventory.PassiveStackableItems.ForEach(psi => psi.ItemEffects.ForEach(itemEffect => {
                if(itemEffect.Trigger == ItemEffectTrigger.WhenAcquiredOrActivated) {
                    this.UnApplyEffect(itemEffect);
                }
            }));
            _playerInventory.PassiveItem?.ItemEffects.ForEach(itemEffect => {
                if(itemEffect.Trigger == ItemEffectTrigger.WhenAcquiredOrActivated) {
                    this.UnApplyEffect(itemEffect);
                }
            });
            _playerInventory.OnPassiveStackableItemAdded -= ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveStackableItemRemoved -= UnApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemAdded -= ApplyWhenAcquiredOrActivatedEffects;
            _playerInventory.OnPassiveItemRemoved -= UnApplyWhenAcquiredOrActivatedEffects;
        }

        #endregion

        #region Input callbacks

        private void OnUseActiveItem(InputValue value)
        {
            if (value.isPressed && _playerInventory.ActiveItem != null)
            {
                this.ApplyWhenAcquiredOrActivatedEffects(_playerInventory.ActiveItem);
            }
        }

        #endregion

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
        }

        private void UnApplyEffect(ItemEffect itemEffect) {
            if(itemEffect.Type == ItemEffectType.StatIncrease) {
                itemEffect.Stat.Value -= itemEffect.StatIncreaseAmount;
            }
        }
    }
}
