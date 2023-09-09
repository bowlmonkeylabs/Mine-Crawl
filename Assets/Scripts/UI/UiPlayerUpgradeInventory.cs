using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private Transform _iconsContainer;

        private void Start()
        {
            GenerateStoreIcons();
        }

        void OnEnable() {
            GenerateStoreIcons();
            _playerInventory.OnPassiveStackableItemAdded += OnItemsChanged;
        }

        void OnDisable() {
            _playerInventory.OnPassiveStackableItemAdded -= OnItemsChanged;
        }

        [Button]
        private void GenerateStoreIcons() {
            DestroyStoreIcons();
            
            if(_playerInventory.PassiveStackableItems.Count > _iconsContainer.childCount) {
                Debug.LogError("Upgrade inventory does not have enough slots to display all items");
                return;
            }

            for(int i = 0; i < _playerInventory.PassiveStackableItems.Count; i++) {
                GameObject iconGameObject = _iconsContainer.GetChild(i).gameObject;
                iconGameObject.GetComponent<UiStoreItemIconController>().Init(_playerInventory.PassiveStackableItems[i]);
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

        private void OnItemsChanged(PlayerItem playerItem)
        {
            GenerateStoreIcons();
        }
    }
}
