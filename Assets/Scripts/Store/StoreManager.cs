using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class StoreManager : MonoBehaviour
    {
        [SerializeField] private BoolVariable _isGodModeEnabled;
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField] private GameEvent _onStoreFailOpenEvent;
        [SerializeField] private UnityEvent _onPurchaseItem;

        private void Awake()
        {
            _onPurchaseEvent.Subscribe(AttemptPurchaseDynamic);
        }

        private void OnDestroy()
        {
            _onPurchaseEvent.Unsubscribe(AttemptPurchaseDynamic);
        }

        private void AttemptPurchaseDynamic(object prev, object item)
        {
            AttemptPurchase(item as PlayerItem);
        }

        private void AttemptPurchase(PlayerItem item)
        {
            if (!_isGodModeEnabled.Value)
            {
                var canBuyItem = item.CheckIfCanBuy();
                if (!canBuyItem)
                {
                    _onStoreFailOpenEvent.Raise();
                    return;
                }
            
                item.DeductCosts();
            }

            DoPurchase(item);
        }

        private void DoPurchase(PlayerItem item)
        {
            var didAddItem = _playerInventory.TryAddItem(item);
            if (!didAddItem)
            {
                throw new Exception("Purchase failed.");
            }
            _onPurchaseItem.Invoke();
        }
    }
}