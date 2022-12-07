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

            if(_itemsPlayerHas.Count > _iconsContainer.childCount) {
                Debug.LogError("Upgrade inventory does not have enough slots to display all items");
                return;
            }

            for(int i = 0; i < _itemsPlayerHas.Count; i++) {
                GameObject iconGameObject = _iconsContainer.GetChild(i).gameObject;
                iconGameObject.GetComponent<UiStoreItemIconController>().Init(_itemsPlayerHas[i]);
                iconGameObject.SetActive(true);
            }
        }

        [Button]
        private void DestroyStoreIcons()
        {
            foreach(Transform iconTransform in _iconsContainer) {
                iconTransform.gameObject.SetActive(false);
            }
        }

        protected void OnBuy(object prevStoreItem, object storeItem) {
            GenerateStoreIcons();
        }
    }
}
