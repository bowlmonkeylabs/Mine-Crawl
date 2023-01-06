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
        [SerializeField] private IntVariable _enemyResourceCount;
        [SerializeField] private BoolVariable _isGodModeEnabled;
        [SerializeField] private BoolVariable _inCombat;
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField] private GameEvent _onStoreFailOpenEvent;
        [SerializeField] private DynamicGameEvent _onInsufficientResources;
        [SerializeField] private UnityEvent _onPurchaseItem;

        private void Awake()
        {
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
                if (_inCombat.Value)
                {
                    _onStoreFailOpenEvent.Raise();
                    return;
                }

                var canAffordItem = storeItem.CheckIfCanAfford(_resourceCount.Value, _rareResourceCount.Value, _enemyResourceCount.Value);
                if (!canAffordItem.Overall)
                {
                    _onStoreFailOpenEvent.Raise();
                    _onInsufficientResources.Raise(storeItem);
                    return;
                }
            
                _resourceCount.Value -= storeItem._resourceCost;
                _rareResourceCount.Value -= storeItem._rareResourceCost;
                _enemyResourceCount.Value -= storeItem._enemyResourceCost;
            }

            DoPurchase(storeItem);
        }

        private void DoPurchase(StoreItem storeItem)
        {
            storeItem._onPurchased.Invoke();
            _onPurchaseItem.Invoke();
        }
    }
}