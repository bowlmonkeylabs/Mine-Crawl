using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Player;
using BML.Scripts.Player.Items;
using BML.Scripts.Store;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class StoreManager : MonoBehaviour
    {
        [SerializeField] private BoolVariable _isGodModeEnabled;
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
            PlayerItem storeItem = (PlayerItem) storeItemObj;

            if (!_isGodModeEnabled.Value)
            {
                var canAffordItem = storeItem.CheckIfCanAfford();
                if (!canAffordItem)
                {
                    _onStoreFailOpenEvent.Raise();
                    _onInsufficientResources.Raise(storeItem);
                    return;
                }
            
                storeItem.DeductCosts();
            }

            DoPurchase(storeItem);
        }

        private void DoPurchase(PlayerItem playerItem)
        {
            _onPurchaseItem.Invoke();
        }
    }
}