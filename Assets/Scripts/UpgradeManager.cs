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
        [SerializeField] private IntVariable _oreCount;
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
            
            if (_oreCount.Value < storeItem._cost.Value) return;

            storeItem._incrementOnPurchase.Value += storeItem._incrementAmount.Value;
            _oreCount.Value -= storeItem._cost.Value;
        }
    }
}