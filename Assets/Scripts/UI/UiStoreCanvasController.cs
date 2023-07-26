using BML.Scripts.Utils;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Store;
using BML.Scripts.CaveV2;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace BML.Scripts.UI
{
    public class UiStoreCanvasController : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField, FoldoutGroup("CaveGen")] private GameObjectSceneReference _caveGeneratorGameObjectSceneReference;
        private CaveGenComponentV2 _caveGenerator => _caveGeneratorGameObjectSceneReference?.CachedComponent as CaveGenComponentV2;
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField] private Transform _listContainerStoreButtons;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private int _maxItemsShown = 0;
        [SerializeField] private bool _randomizeStoreOnBuy;
        [SerializeField] private bool _filterOutMaxedItems;
        [SerializeField] private Button _buttonNavLeft;
        [SerializeField] private Button _buttonNavRight;
        [SerializeField] private StoreInventory _storeInventory;
        [SerializeField] private IntVariable _resourceCount;
        [SerializeField] private IntVariable _rareResourceCount;
        [SerializeField] private IntVariable _upgradesAvailableCount;
        
        #endregion
        
        private List<UiStoreButtonController> buttonList = new List<UiStoreButtonController>();

        #region Unity lifecycle
        
        private void Awake()
        {
            // Debug.Log("Awake");
            
            #warning Remove this once we're done working on the stores/inventory
            GenerateStoreItems();
            
            // SetNavigationOrder();
        }

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            
            UpdateButtons();
            
            _resourceCount.Subscribe(UpdateButtons);
            _rareResourceCount.Subscribe(UpdateButtons);
            _upgradesAvailableCount.Subscribe(UpdateButtons);
            
            _onPurchaseEvent.Subscribe(OnBuy);
        }

        void OnDisable()
        {
            // Debug.Log("OnDisable");
            
            _resourceCount.Unsubscribe(UpdateButtons);
            _rareResourceCount.Unsubscribe(UpdateButtons);
            _upgradesAvailableCount.Unsubscribe(UpdateButtons);
            
            _onPurchaseEvent.Unsubscribe(OnBuy);
        }
        
        #endregion
        
        #region Public interface

        [Button("Generate Store Items")]
        public void GenerateStoreItems()
        {
            DestroyShopItems();

            List<StoreItem> shownStoreItems = _storeInventory.StoreItems;

            if(_filterOutMaxedItems) {
                shownStoreItems = shownStoreItems.Where(si => !si._hasMaxAmount || (si._playerInventoryAmount.Value < si._maxAmount.Value)).ToList();
            }

            if(_randomizeStoreOnBuy) {
                Random.InitState(SeedManager.Instance.GetSteppedSeed("UpgradeStore"));
                shownStoreItems = shownStoreItems.OrderBy(c => Random.value).ToList();
            }

            if(_maxItemsShown > 0) {
                shownStoreItems = shownStoreItems.Take(_maxItemsShown).ToList();
            }

            if(shownStoreItems.Count > _listContainerStoreButtons.childCount) {
                Debug.LogError("Store does not have enough buttons to display options");
                return;
            }

            for(int i = 0; i < shownStoreItems.Count; i++) {
                GameObject buttonGameObject = _listContainerStoreButtons.GetChild(i).gameObject;
                var uiStoreButtonControllerComponent = buttonGameObject.GetComponent<UiStoreButtonController>();
                uiStoreButtonControllerComponent.Init(shownStoreItems[i]);
                buttonGameObject.SetActive(true);
                buttonList.Add(uiStoreButtonControllerComponent);
            }

            UpdateButtons();
        }

        [Button("Destroy Store Items")]
        public void DestroyShopItems()
        {
            buttonList.Clear();
            
            for(int i = 0; i < _listContainerStoreButtons.childCount - 1; i++) {
                _listContainerStoreButtons.GetChild(i).gameObject.SetActive(false);
            }
        }
        
        public void SelectDefault()
        {
            var firstUsableButton = buttonList.FirstOrDefault(button => button.Button.gameObject.activeSelf && button.Button.IsInteractable())?.Button ?? _cancelButton;
            if (firstUsableButton != null)
            {
                firstUsableButton.Select();
            }
        }
        
        #endregion
        
        #region UI control

        private void UpdateButtons()
        {
            foreach (var button in buttonList)
            {
                button.UpdateInteractable();
            }
            
            SetNavigationOrder();
        }

        private void SetNavigationOrder(bool includeInactive = false, bool includeNonInteractable = false)
        {
            var filteredButtons = buttonList
                .Where(b =>
                    (includeInactive || b.gameObject.activeSelf) 
                    && (includeNonInteractable || b.Button.IsInteractable()))
                .Select(b => b.Button)
                .ToList();
            if (_cancelButton != null)
            {
                filteredButtons.Add(_cancelButton);
            }

            // Debug.Log($"SetNavigationOrder: ({this.transform.parent.name}) ({filteredButtons.Count} active buttons)");
            // Debug.Log(string.Join(", ", buttonList.Select(b => $"({b.gameObject.activeSelf}, {b.Button.IsInteractable()})")));
            
            for(int i = 0; i < filteredButtons.Count; i++)
            {
                int prevIndex = (i == 0 ? filteredButtons.Count - 1 : i - 1);
                int nextIndex = (i >= filteredButtons.Count - 1 ? 0 : i + 1);
                
                Navigation nav = new Navigation();
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = filteredButtons[prevIndex];
                nav.selectOnDown = filteredButtons[nextIndex];
                if(_buttonNavLeft != null) nav.selectOnLeft = _buttonNavLeft;
                if(_buttonNavRight != null) nav.selectOnRight = _buttonNavRight;
                
                filteredButtons[i].navigation = nav;
                // Debug.Log(filteredButtons[i].navigation.selectOnUp.name);
                // Debug.Log(buttonList.FirstOrDefault(b =>
                //     (includeInactive || b.gameObject.activeSelf) 
                //     && (includeNonInteractable || b.Button.IsInteractable()))?.Button.navigation.selectOnUp.name);
            }
        }

        protected void OnBuy(object prevStoreItem, object storeItem)
        {
            SeedManager.Instance.UpdateSteppedSeed("UpgradeStore");
            if(_randomizeStoreOnBuy) {
                GenerateStoreItems();
            }
        }
        
        #endregion

        
    }
}