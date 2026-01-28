using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts.Player.Items.Store
{
    public class TryPurchaseEventPayload
    {
        public PlayerItem Item;
        public Action<bool> DidPurchaseCallback;
    }
    
    public class StoreManager : MonoBehaviour
    {
        [SerializeField] private BoolVariable _isGodModeEnabled;
        [SerializeField] private PlayerInventory _playerInventory;
        [FormerlySerializedAs("_onPurchaseEvent")] [SerializeField] private DynamicGameEvent _onTryPurchaseEvent;
        [SerializeField] private GameEvent _onStoreFailOpenEvent;
        [SerializeField] private UnityEvent _onPurchaseItem;

        private void Awake()
        {
            _onTryPurchaseEvent.Subscribe(AttemptPurchaseDynamic);
        }

        private void OnDestroy()
        {
            _onTryPurchaseEvent.Unsubscribe(AttemptPurchaseDynamic);
        }

        private void AttemptPurchaseDynamic(object prev, object payload)
        {
            TryPurchase(payload as TryPurchaseEventPayload);
        }

        private void TryPurchase(TryPurchaseEventPayload payload)
        {
            bool canBuyItem = _playerInventory.CheckIfCanBuy(payload.Item, true);
            if (!canBuyItem)
            {
                _onStoreFailOpenEvent.Raise();
                return;
            }
        
            _playerInventory.DeductCosts(payload.Item);

            DoPurchase(payload.Item);

            payload.DidPurchaseCallback(true);
        }

        private void DoPurchase(PlayerItem item)
        {
            var didAddItem = _playerInventory.TryAddItem(item, true);
            if (!didAddItem)
            {
                throw new Exception("Purchase failed.");
            }
            _onPurchaseItem.Invoke();
        }
    }
}