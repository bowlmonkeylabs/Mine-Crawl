using System;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Store;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private IntVariable _resourceCount;
        [SerializeField] private IntVariable _rareResourceCount;
        [SerializeField] private IntVariable _enemyResourceCount;
        [SerializeField] private BoolVariable _isGodModeEnabled;
        [SerializeField] private StoreInventory _storeInventory;


        private void Awake()
        {
            foreach (var storeItem in _storeInventory.StoreItems)
            {
                storeItem._onPurchaseEvent.Subscribe(AttemptPurchase);
            }
        }

        private void OnDestroy()
        {
            foreach (var storeItem in _storeInventory.StoreItems)
            {
                storeItem._onPurchaseEvent.Unsubscribe(AttemptPurchase);
            }
        }

        private void AttemptPurchase(object prev, object storeItemObj)
        {
            StoreItem storeItem = (StoreItem) storeItemObj;

            if (_isGodModeEnabled.Value)
            {
                storeItem._incrementOnPurchase.Value += storeItem._incrementAmount.Value;
                return;
            }

            if (_resourceCount.Value < storeItem._resourceCost.Value ||
                _rareResourceCount.Value < storeItem._rareResourceCost.Value ||
                _enemyResourceCount.Value < storeItem._enemyResourceCost.Value)
                return;
            
                
            
            _resourceCount.Value -= storeItem._resourceCost.Value;
            _rareResourceCount.Value -= storeItem._rareResourceCost.Value;
            _enemyResourceCount.Value -= storeItem._enemyResourceCost.Value;

            storeItem._incrementOnPurchase.Value += storeItem._incrementAmount.Value;
        }
    }
}