using BML.ScriptableObjectCore.Scripts.Events;
using Mono.CSharp;
using UnityEngine;

namespace BML.Scripts.Store
{
    public class StorePurchaseEvent : MonoBehaviour
    {
        [SerializeField] private DynamicGameEvent _purchaseEvent;
        [SerializeField] private StoreItem _itemToPurchase;

        public void Init(DynamicGameEvent purchaseEvent, StoreItem itemToPurchase)
        {
            _purchaseEvent = purchaseEvent;
            _itemToPurchase = itemToPurchase;
        }

        public void Raise()
        {
            _purchaseEvent.Raise(_itemToPurchase);
        }
    }
}