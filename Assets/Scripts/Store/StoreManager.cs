using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Store;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class StoreManager : MonoBehaviour
    {
        [SerializeField] private IntVariable _resourceCount;
        [SerializeField] private IntVariable _rareResourceCount;
        [SerializeField] private IntVariable _upgradesAvailable;
        [SerializeField] private BoolVariable _isGodModeEnabled;
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField] private GameEvent _onStoreFailOpenEvent;
        [SerializeField] private DynamicGameEvent _onInsufficientResources;
        [SerializeField] private StoreInventory _upgradeStoreInventory;
        [SerializeField] private GameEvent _onVariablesReset;
        [SerializeField] private UnityEvent _onPurchaseItem;

        private void Awake()
        {
            _onVariablesReset.Subscribe(ApplyPlayerInventoryEffects);
            _onPurchaseEvent.Subscribe(AttemptPurchase);
        }

        private void OnDestroy()
        {
            _onPurchaseEvent.Unsubscribe(AttemptPurchase);
        }

        private void AttemptPurchase(object prev, object storeItemObj)
        {
            StoreItem storeItem = (StoreItem) storeItemObj;

            if (!_isGodModeEnabled.Value)
            {
                var canAffordItem = storeItem.CheckIfCanAfford(_resourceCount.Value, _rareResourceCount.Value, _upgradesAvailable.Value);
                if (!canAffordItem.Overall)
                {
                    _onStoreFailOpenEvent.Raise();
                    _onInsufficientResources.Raise(storeItem);
                    return;
                }
            
                _resourceCount.Value -= storeItem._resourceCost;
                _rareResourceCount.Value -= storeItem._rareResourceCost;
                _upgradesAvailable.Value -= storeItem._upgradeCost;
            }

            DoPurchase(storeItem);
        }

        private void DoPurchase(StoreItem storeItem)
        {
            storeItem._onPurchased.Invoke();
            _onPurchaseItem.Invoke();
        }

        private void ApplyPlayerInventoryEffects() {
            _upgradeStoreInventory.StoreItems.ForEach((item) => {
                if(item._playerInventoryAmount.Value > 0) {
                    var itemCount = item._playerInventoryAmount.Value;
                    item._playerInventoryAmount.Value = 0;
                    for(var i = 0; i < itemCount; i++) {
                        item._onPurchased.Invoke();
                    }
                }
            });

            _onVariablesReset.Unsubscribe(ApplyPlayerInventoryEffects);
        }
    }
}