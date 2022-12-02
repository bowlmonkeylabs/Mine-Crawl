using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Store;
using UnityEngine;
using BML.Scripts.Utils;
using BML.ScriptableObjectCore.Scripts.Events;
using Sirenix.OdinInspector;

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
            _onBuyEvent.Subscribe(OnBuy);
            
#warning Remove this once we're done working on the stores/inventory
            GenerateStoreIcons();
        }

        void OnDisable() {
            _onBuyEvent.Unsubscribe(OnBuy);
        }

        [Button]
        private void GenerateStoreIcons() {
            DestroyStoreIcons();
            
            _itemsPlayerHas = _upgradeStoreInventory.StoreItems.Where(si => si._playerInventoryAmount.Value > 0).ToList();

            foreach (var storeItem in _itemsPlayerHas)
            {
                var newStoreItemButton = GameObjectUtils.SafeInstantiate(true, _storeItemIconPrefab, _iconsContainer);
                newStoreItemButton.name = $"Icon_{storeItem.name}";
                
                var uiStoreItemIconController = newStoreItemButton.GetComponent<UiStoreItemIconController>();
                var storeUiMenuPageControllerGameObject = uiStoreItemIconController.gameObject;
                uiStoreItemIconController.Init(storeItem, storeUiMenuPageControllerGameObject, storeUiMenuPageControllerGameObject);
            }
        }

        [Button]
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
            GenerateStoreIcons();
        }
    }
}
