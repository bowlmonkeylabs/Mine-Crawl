﻿using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Store;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiStoreCanvasController : MonoBehaviour
    {
        [SerializeField] private GameObject _storeUiButtonPrefab;
        [SerializeField] private GameObject _storeResumeButtonPrefab;
        [SerializeField] private Transform _listContainerStoreButtons;
        [SerializeField] private UiMenuPageController _storeUiMenuPageController;
        [SerializeField] private StoreInventory _storeInventory;

        private List<Button> buttonList = new List<Button>();

        private void Awake()
        {
            GenerateStoreItems();
        }

        [Button("Generate Store Items")]
        public void GenerateStoreItems()
        {
            DestoryStoreItems();
            
            foreach (var storeItem in _storeInventory.StoreItems)
            {
                var prefab = storeItem._uiReplacePrefab != null ? storeItem._uiReplacePrefab : _storeUiButtonPrefab;
                var newStoreItem = GameObjectUtils.SafeInstantiate(true, prefab, _listContainerStoreButtons);
                newStoreItem.name = $"Button_{storeItem._storeText}";
                
                var purchaseEvent = newStoreItem.GetComponent<StorePurchaseEvent>();
                purchaseEvent.Init(storeItem._onPurchaseEvent, storeItem);

                var storeItemText = newStoreItem.GetComponentInChildren<TMPro.TMP_Text>();

                storeItemText.text = "";
                
                if (storeItem._resourceCost.Value > 0)
                    storeItemText.text += $" + R{storeItem._resourceCost.Value}";
                
                if (storeItem._rareResourceCost.Value > 0)
                    storeItemText.text += $" + RR{storeItem._rareResourceCost.Value}";
                
                if (storeItem._enemyResourceCost.Value > 0)
                    storeItemText.text += $" + ER{storeItem._enemyResourceCost.Value}";

                //Remove leading +
                storeItemText.text = storeItemText.text.Substring(3);
                
                storeItemText.text += $" -> {storeItem._incrementAmount.Value} {storeItem._storeText}";
                
                var button = newStoreItem.GetComponent<Button>();
                buttonList.Add(button);
            }
            
            var newResumeButton  = GameObjectUtils.SafeInstantiate(true, _storeResumeButtonPrefab, _listContainerStoreButtons);
            var buttonResume = newResumeButton.GetComponent<Button>();
            buttonList.Add(buttonResume);
            
            buttonResume.onClick.AddListener(_storeUiMenuPageController.ClosePage);
            buttonResume.GetComponent<UiEventHandler>().OnCancelAddListener(_storeUiMenuPageController.ClosePage);

            _storeUiMenuPageController.DefaultSelected = buttonList[0];
            SetNavigationOrder();
        }

        [Button("Destroy Store Items")]
        public void DestoryStoreItems()
        {
            buttonList.Clear();
            var children = Enumerable.Range(0, _listContainerStoreButtons.childCount)
                .Select(i => _listContainerStoreButtons.GetChild(i).gameObject)
                .ToList();
            foreach (var childObject in children)
            {
                GameObject.DestroyImmediate(childObject);
            }
        }

        private void SetNavigationOrder()
        {
            for (int i = 0; i < buttonList.Count; i++)
            {
                Button prevButton = i > 0 ? buttonList[i - 1] : buttonList[buttonList.Count - 1];
                Button nextButton = i < buttonList.Count - 1 ? buttonList[i + 1] : buttonList[0];
                Navigation nav = new Navigation();
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = prevButton;
                nav.selectOnDown = nextButton;
                buttonList[i].navigation = nav;
            }
        }
    }
}