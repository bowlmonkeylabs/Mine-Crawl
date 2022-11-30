using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Store;
using UnityEngine;
using BML.Scripts.Utils;
using BML.ScriptableObjectCore.Scripts.Events;

namespace BML.Scripts.UI
{
    public class UiPlayerUpgradeInventory : MonoBehaviour
    {
        [SerializeField] StoreInventory _upgradeStoreInventory;
        [SerializeField] private GameObject _storeItemIconPrefab;
        [SerializeField] private Transform _iconsContainer;
        [SerializeField] private DynamicGameEvent _onBuyEvent;

        private List<StoreItem> _itemsPlayerHas;

        void OnEnable() {
            _itemsPlayerHas = _upgradeStoreInventory.StoreItems.Where(si => si._playerInventoryAmount.Value > 0).ToList();
            _onBuyEvent.Subscribe(OnBuy);
            GenerateStoreIcons();
        }

        void OnDisable() {
            _onBuyEvent.Unsubscribe(OnBuy);
        }

        private void GenerateStoreIcons() {
            DestroyStoreIcons();

            foreach (var storeItem in _itemsPlayerHas)
            {
                var newStoreItemButton = GameObjectUtils.SafeInstantiate(true, _storeItemIconPrefab, _iconsContainer);
                newStoreItemButton.name = $"Icon_{storeItem.name}";
                
                var uiStoreItemIconController = newStoreItemButton.GetComponent<UiStoreItemIconController>();
                uiStoreItemIconController.Init(storeItem);
            }
        }

        private void DestroyStoreIcons()
        {
            var children = Enumerable.Range(0, _iconsContainer.childCount)
                .Select(i => _iconsContainer.GetChild(i).gameObject)
                .ToList();
            foreach (var childObject in children)
            {
                GameObject.DestroyImmediate(childObject);
            }
        }

        protected void OnBuy(object prevStoreItem, object storeItem) {
            _itemsPlayerHas = _upgradeStoreInventory.StoreItems.Where(si => si._playerInventoryAmount.Value > 0).ToList();
            GenerateStoreIcons();
        }
    }
}
