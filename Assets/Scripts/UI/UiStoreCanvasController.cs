using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Store;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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
        [SerializeField] private string _resourceIconText;
        [SerializeField] private string _rareResourceIconText;
        [SerializeField] private string _enemyResourceIconText;
        [SerializeField] private StoreInventory _storeInventory;

        private List<Button> buttonList = new List<Button>();

        private void Awake()
        {
            GenerateStoreItems();
        }

        [Button("Generate Store Items")]
        public void GenerateStoreItems()
        {
            DestroyShopItems();
            
            foreach (var storeItem in _storeInventory.StoreItems)
            {
                var prefab = storeItem._uiReplacePrefab != null ? storeItem._uiReplacePrefab : _storeUiButtonPrefab;
                var newStoreItem = GameObjectUtils.SafeInstantiate(true, prefab, _listContainerStoreButtons);
                newStoreItem.name = $"Button_{storeItem._storeButtonName}";
                
                var purchaseEvent = newStoreItem.GetComponent<StorePurchaseEvent>();
                purchaseEvent.Init(storeItem._onPurchaseEvent, storeItem);

                var storeItemText = newStoreItem.GetComponentInChildren<TMPro.TMP_Text>();

                storeItemText.text = GenerateStoreText(storeItem);
                
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
        public void DestroyShopItems()
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

        private String GenerateStoreText(StoreItem storeItem)
        {
            string costText = "";
            
            if (storeItem._resourceCost > 0)
                costText += $" + {storeItem._resourceCost}{_resourceIconText}";
                
            if (storeItem._rareResourceCost > 0)
                costText += $" + {storeItem._rareResourceCost}{_rareResourceIconText}";
                
            if (storeItem._enemyResourceCost > 0)
                costText += $" + {storeItem._enemyResourceCost}{_enemyResourceIconText}";

            //Remove leading +
            if (costText != "") costText = costText.Substring(3);

            string resultText = "";
            
            foreach (var purchaseItem in storeItem.PurchaseItems)
            {
                if (!purchaseItem._storeText.IsNullOrWhitespace())  //Dont add if left blank (Ex. for max health also inc health but dont show)
                    resultText += $" + {purchaseItem._incrementAmount} {purchaseItem._storeText}";
            }

            //Remove leading +
            if (resultText != "") resultText = resultText.Substring(3);

            return $"{costText} -> {resultText}";
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