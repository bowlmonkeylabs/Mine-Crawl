using System;
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

        [SerializeField] private List<StoreItem> _itemsPlayerHas;

        private void Start()
        {
            ResolveInventory();
            #warning Remove this once we're done working on the stores/inventory
            GenerateStoreIcons();
        }

        void OnEnable() {
            #warning Remove this once we're done working on the stores/inventory
            GenerateStoreIcons();
            _onBuyEvent.Subscribe(OnBuy_Dynamic);
        }

        void OnDisable() {
            _onBuyEvent.Unsubscribe(OnBuy_Dynamic);
        }

        [Button]
        private void GenerateStoreIcons() {
            DestroyStoreIcons();

            // ResolveInventory();
            
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

        protected void OnBuy_Dynamic(object prevStoreItem, object storeItem) {
            OnBuy(storeItem as StoreItem);
        }
        protected void OnBuy(StoreItem storeItem)
        {
            if (!_itemsPlayerHas.Contains(storeItem))
            {
                _itemsPlayerHas.Add(storeItem);
            }
            GenerateStoreIcons();
        }

        private void ResolveInventory()
        {
            _itemsPlayerHas = _upgradeStoreInventory.StoreItems.Where(si => si._playerInventoryAmount.Value > 0).ToList();
        }
        
    }
}
