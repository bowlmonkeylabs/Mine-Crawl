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
        [SerializeField] private int _maxItemsShown = 0;
        [SerializeField] private bool _randomizeStoreOnBuy;
        [SerializeField] private bool _filterOutMaxedItems;
        [SerializeField] private StoreInventory _storeInventory;

        private List<Button> buttonList = new List<Button>();
        private List<StoreItem> shownStoreItems = new List<StoreItem>();

        private void Awake()
        {
            GenerateStoreItems();
        }

        void OnEnable() {
            _onPurchaseEvent.Subscribe(OnBuy);
        }

        void OnDisable() {
            _onPurchaseEvent.Unsubscribe(OnBuy);
        }

        [Button("Generate Store Items")]
        public void GenerateStoreItems()
        {
            DestroyShopItems();

            shownStoreItems = _storeInventory.StoreItems;

            if(_filterOutMaxedItems) {
                shownStoreItems = shownStoreItems.Where(si => !si._hasMaxAmount || (si._playerInventoryAmount.Value < si._maxAmount.Value)).ToList();
            }

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

        protected void OnBuy(object prevStoreItem, object storeItem) {
            if(_randomizeStoreOnBuy) {
                GenerateStoreItems();
            }
        }
    }
}