using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.Store
{
    public class UiStoreButtonController : MonoBehaviour
    {
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField] private Button _button;
        [SerializeField] private StoreItem _itemToPurchase;
        [SerializeField] private IntVariable _resourceCount;
        [SerializeField] private IntVariable _rareResourceCount;
        [SerializeField] private IntVariable _enemyResourceCount;
        
        public void Init(StoreItem itemToPurchase)
        {
            _itemToPurchase = itemToPurchase;
        }

        public void Raise()
        {
            _onPurchaseEvent.Raise(_itemToPurchase);
        }

        void Update() {
            _button.interactable = _itemToPurchase._resourceCost <= _resourceCount.Value &&
                _itemToPurchase._rareResourceCost <= _rareResourceCount.Value &&
                _itemToPurchase._enemyResourceCost <= _enemyResourceCount.Value;
        }
    }
}