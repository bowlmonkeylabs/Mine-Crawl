using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Store;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using BML.ScriptableObjectCore.Scripts.Events;

namespace BML.Scripts.UI
{
    public class UiStoreCanvasController : MonoBehaviour
    {
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField] private GameObject _storeUiButtonPrefab;
        [SerializeField] private GameObject _storeResumeButtonPrefab;
        [SerializeField] private Transform _listContainerStoreButtons;
        [SerializeField] private UiMenuPageController _storeUiMenuPageController;
        [SerializeField] private string _resourceIconText;
        [SerializeField] private string _rareResourceIconText;
        [SerializeField] private string _enemyResourceIconText;
        [SerializeField] private int _maxItemsShown = 0;
        [SerializeField] private bool _randomizeStoreOnBuy;
        [SerializeField] private bool _closeStoreOnBuy;
        [SerializeField] private StoreInventory _storeInventory;

        private List<Button> buttonList = new List<Button>();
        private List<StoreItem> shownStoreItems = new List<StoreItem>();

        private void Awake()
        {
            GenerateStoreItems();
        }

        [Button("Generate Store Items")]
        public void GenerateStoreItems()
        {
            DestroyShopItems();

            _onPurchaseEvent.Subscribe(OnBuy);

            shownStoreItems = _storeInventory.StoreItems.Where(si => !si._limitedSupply || !si._playerHasItem.Value).ToList();

            if(_randomizeStoreOnBuy) {
                shownStoreItems = shownStoreItems.Shuffle().ToList();
            }

            if(_maxItemsShown > 0) {
                shownStoreItems = shownStoreItems.Take(_maxItemsShown).ToList();
            }

            foreach (var storeItem in shownStoreItems)
            {
                var newStoreItemButton = GameObjectUtils.SafeInstantiate(true, _storeUiButtonPrefab, _listContainerStoreButtons);
                newStoreItemButton.name = $"Button_{storeItem.name}";
                
                var uiStoreButtonController = newStoreItemButton.GetComponent<UiStoreButtonController>();
                uiStoreButtonController.Init(storeItem);

                var storeItemText = newStoreItemButton.GetComponentInChildren<TMPro.TMP_Text>();

                storeItemText.text = GenerateStoreText(storeItem);
                
                var button = newStoreItemButton.GetComponent<Button>();
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
            _onPurchaseEvent.Unsubscribe(OnBuy);

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
                    resultText += $" + {purchaseItem._storeText}";
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

        protected void OnBuy(object prevStoreItem, object storeItem) {
            if(_randomizeStoreOnBuy) {
                GenerateStoreItems();
            }

            if(_closeStoreOnBuy) {
                _storeUiMenuPageController.ClosePage();
            }
        }
    }
}