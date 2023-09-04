using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Store;
using UnityEngine;
using BML.Scripts.Utils;
using BML.ScriptableObjectCore.Scripts.Events;
using Sirenix.OdinInspector;
using BML.Scripts.Player;
using BML.Scripts.Player.Items;

namespace BML.Scripts.UI
{
    public class UiPlayerUpgradeInventory : MonoBehaviour
    {
        [SerializeField] StoreInventory _upgradeStoreInventory;
        [SerializeField] private Transform _iconsContainer;

        private void Start()
        {
            UpdateInventory();
            GenerateStoreIcons();
        }

        void OnEnable() {
            UpdateInventory();
            GenerateStoreIcons();
            // _onBuyEvent.Subscribe(OnBuy_Dynamic);
        }

        void OnDisable() {
            // _onBuyEvent.Unsubscribe(OnBuy_Dynamic);
        }

        [Button]
        private void GenerateStoreIcons() {
            DestroyStoreIcons();

            // ResolveInventory();
            
            // if(_itemsPlayerHas.Count > _iconsContainer.childCount) {
            //     Debug.LogError("Upgrade inventory does not have enough slots to display all items");
            //     return;
            // }

            // for(int i = 0; i < _itemsPlayerHas.Count; i++) {
            //     GameObject iconGameObject = _iconsContainer.GetChild(i).gameObject;
            //     iconGameObject.GetComponent<UiStoreItemIconController>().Init(_itemsPlayerHas[i]);
            //     iconGameObject.SetActive(true);
            // }
        }

        [Button]
        private void DestroyStoreIcons()
        {
            foreach(Transform iconTransform in _iconsContainer) {
                iconTransform.gameObject.SetActive(false);
            }
        }

        protected void OnBuy_Dynamic(object prevStoreItem, object playerItem) {
            OnBuy(playerItem as PlayerItem);
        }
        protected void OnBuy(PlayerItem playerItem)
        {
            // if (!_itemsPlayerHas.Contains(storeItem))
            // {
            //     _itemsPlayerHas.Add(storeItem);
            // }
            GenerateStoreIcons();
        }

        private void UpdateInventory()
        {
            // for (int i = 0; i < _itemsPlayerHas.Count; i++)
            // {
            //     var inventoryItem = _itemsPlayerHas[i];
            //     if (inventoryItem._playerInventoryAmount.Value <= 0)
            //     {
            //         _itemsPlayerHas.RemoveAt(i);
            //         i--;
            //     }
            // }
            
            // foreach (var storeItem in _upgradeStoreInventory.StoreItems)
            // {
            //     if (storeItem._playerInventoryAmount.Value > 0 &&
            //         !_itemsPlayerHas.Contains(storeItem))
            //     {
            //         _itemsPlayerHas.Add(storeItem);
            //     }
            // }
        }
        
    }
}
