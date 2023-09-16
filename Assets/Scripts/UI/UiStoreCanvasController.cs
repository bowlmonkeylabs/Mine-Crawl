using System;
using BML.Scripts.Utils;
using System.Collections.Generic;
using System.Linq;
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
using BML.Scripts.Player;
using BML.Scripts.Player.Items;
using Object = System.Object;

namespace BML.Scripts.UI
{
    public enum StoreItemPoolType
    {
        Merchant,
        MerchantEnd
    }

    [System.Serializable]
    public class StoreItemPool
    {
        public StoreItemPoolType StoreItemPoolType;
        public List<PlayerItem> ActiveItemPool;
        public List<PlayerItem> PassiveItemPool;
    }
    
    public class UiStoreCanvasController : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private bool _useGraph = true;
        [ShowIf("_useGraph"), SerializeField] private Player.Items.ItemTreeGraph _itemTreeGraph;
        [HideIf("_useGraph"), SerializeField] private DynamicGameEvent _onSetStorePool;
        [HideIf("_useGraph"), SerializeField] private BoolVariable _game_MenuIsOpen_Store;
        [HideIf("_useGraph"), SerializeField] private List<StoreItemPool> _storeItemPools = new List<StoreItemPool>();
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField] private Transform _listContainerStoreButtons;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private GameObject _noItemsAvailableUi;
        [SerializeField] private int _optionsCount = 2;
        [SerializeField] private bool _randomizeStoreOnBuy;
        [SerializeField] private bool _navHorizontal = false;
        [SerializeField, LabelText("@(_navHorizontal ? \"Button Nav Up\" : \"Button Nav Left\")")] private Button _buttonNavLeft;
        [SerializeField, LabelText("@(_navHorizontal ? \"Button Nav Down\" : \"Button Nav Right\")")] private Button _buttonNavRight;
        
        [SerializeField] private bool _enableLogs = false;
        public bool EnableLogs => _enableLogs;
        
        #endregion
        
        private List<UiStoreButtonController> buttonList = new List<UiStoreButtonController>();
        private UiStoreButtonController lastSelected = null;
        
        [ReadOnly, ShowInInspector]
        private StoreItemPoolType currentType = StoreItemPoolType.Merchant;

        #region Unity lifecycle
        
        private void Awake()
        {
            #warning Remove this once we're done working on the stores/inventory
            {
                for(int i = 0; i < _listContainerStoreButtons.childCount; i++)
                {
                    var buttonTransform = _listContainerStoreButtons.GetChild(i);
                    Button button = buttonTransform.GetComponent<Button>();
                    if (_cancelButton != null && button == _cancelButton)
                    {
                        continue;
                    }

                    var buttonController = buttonTransform.GetComponent<UiStoreButtonController>();
                    buttonController.ParentStoreCanvasController = this; // Ideally we should remember to assign this in the inspector, so we can remove this when we're done working on it. We only really need this code if we bring back dynamic shop button addition.
                }
                GenerateStoreItems();
            }
            
            SetNavigationOrder();
        }

        void OnEnable()
        {
            OnItemPoolUpdated();
            
            UpdateButtons();
            
            _onPurchaseEvent.Subscribe(OnBuy);
        }

        void OnDisable()
        {
            _onPurchaseEvent.Unsubscribe(OnBuy);
        }

        private void OnDestroy()
        {
            if (!_useGraph)
                _onSetStorePool.OnUpdate -= OnSetStorePool;
        }

        #endregion
        
        #region Public interface
        
        public void Init()
        {
            if (!_useGraph)
                _onSetStorePool.OnUpdate += OnSetStorePool;
        }

        private List<PlayerItem> GetItemPool()
        {
            List<PlayerItem> itemPool;
            if(_useGraph) 
            {
                itemPool = _itemTreeGraph.GetUnobtainedItemPool();
            } 
            else
            {
                StoreItemPool storeItemPool = _storeItemPools.FirstOrDefault(pool => pool.StoreItemPoolType == currentType);
                if (storeItemPool == null)
                    Debug.LogError($"No StoreItemPool with type {currentType} assigned in {gameObject.name}!");
                
                itemPool = new List<PlayerItem>(storeItemPool.ActiveItemPool);
                itemPool.AddRange(storeItemPool.PassiveItemPool);
            }
            return itemPool;
        }

        [Button("Generate Store Items")]
        public void GenerateStoreItems()
        {
            DestroyStoreItems();
            
            if (_enableLogs) Debug.Log($"GenerateStoreItems ({this.gameObject.name})");

            List<PlayerItem> shownStoreItems = GetItemPool();

            if (shownStoreItems.Count == 0)
            {
                if (_noItemsAvailableUi != null) _noItemsAvailableUi.SetActive(true);
                return;
            }
            else
            {
                if (_noItemsAvailableUi != null) _noItemsAvailableUi.SetActive(false);
            }
            
            if(_randomizeStoreOnBuy)
            {
                Random.InitState(SeedManager.Instance.GetSteppedSeed("UpgradeStore"));
                shownStoreItems = shownStoreItems.OrderBy(c => Random.value).ToList();
            }

            shownStoreItems = shownStoreItems.Take(_optionsCount).ToList();

            if(shownStoreItems.Count > _listContainerStoreButtons.childCount) {
                Debug.LogError("Store does not have enough buttons to display options");
                return;
            }

            for(int i = 0; i < shownStoreItems.Count; i++) {
                GameObject buttonGameObject = _listContainerStoreButtons.GetChild(i).gameObject;
                UiStoreButtonController uiStoreButtonControllerComponent = buttonGameObject.GetComponent<UiStoreButtonController>();
                
                uiStoreButtonControllerComponent.Init(shownStoreItems[i]);
                buttonGameObject.SetActive(true);
                buttonList.Add(uiStoreButtonControllerComponent);
                uiStoreButtonControllerComponent.OnInteractibilityChanged += SetNavigationOrder;
            }

            UpdateButtons();
        }

        [Button("Destroy Store Items")]
        public void DestroyStoreItems()
        {
            if (_enableLogs) Debug.Log($"DestroyStoreItems ({this.gameObject.name})");
            
            buttonList.Clear();
            
            for(int i = 0; i < _listContainerStoreButtons.childCount; i++)
            {
                var buttonTransform = _listContainerStoreButtons.GetChild(i);
                Button button = buttonTransform.GetComponent<Button>();
                if (_cancelButton != null && button == _cancelButton)
                {
                    continue;
                }

                var buttonController = buttonTransform.GetComponent<UiStoreButtonController>();
                buttonController.OnInteractibilityChanged -= SetNavigationOrder;
                buttonTransform.gameObject.SetActive(false);
            }
        }
        
        public void SelectDefault()
        {
            var firstUsableButtonController = buttonList
                .FirstOrDefault(button => button.Button.gameObject.activeSelf && button.Button.IsInteractable());
            var firstUsableButton = firstUsableButtonController?.Button ?? _cancelButton;
            if (firstUsableButton != null)
            {
                firstUsableButton.Select();
                if (firstUsableButtonController != null)
                {
                    firstUsableButtonController.SetStoreItemToSelected();
                }
            }
        }
        
        #endregion
        
        #region UI control

        private void OnItemPoolUpdated()
        {
            if (!_randomizeStoreOnBuy || buttonList.Count < _optionsCount)
            {
                GenerateStoreItems();
                return;
            }
            
            var itemPool = GetItemPool();
            var allShopButtons = Enumerable.Range(0, _listContainerStoreButtons.childCount - 1)
                .Select(i => _listContainerStoreButtons.GetChild(i).GetComponent<UiStoreButtonController>())
                .Where(button => _cancelButton == null || button.Button != _cancelButton);
            var statusOfItemsCurrentlyInShopButtons = buttonList.Select(button => 
                (button: button, stillInItemPool: button.ItemToPurchase != null && itemPool.Contains(button.ItemToPurchase))).ToList();
            
            if (itemPool.Count == 0)
            {
                if (_noItemsAvailableUi != null) _noItemsAvailableUi.SetActive(true);
                return;
            }
            else
            {
                if (_noItemsAvailableUi != null) _noItemsAvailableUi.SetActive(false);
            }
            
            foreach (var button in buttonList)
            {
                if (button.ItemToPurchase != null)
                {
                    itemPool.Remove(button.ItemToPurchase);
                }
            }
            
            Random.InitState(SeedManager.Instance.GetSteppedSeed("UpgradeStore"));
            int countOfButtonsStillInItemPool = statusOfItemsCurrentlyInShopButtons.Count(i => i.stillInItemPool);
            var replacementOptions = itemPool.OrderBy(item => Random.value)
                .Take(_optionsCount - countOfButtonsStillInItemPool).ToList();
            int replacementIndex = 0;
            foreach (var valueTuple in statusOfItemsCurrentlyInShopButtons)
            {
                if (!valueTuple.stillInItemPool)
                {
                    valueTuple.button.Init(replacementOptions[replacementIndex++]);
                }
            }
        }

        private void UpdateButtons()
        {
            if (_enableLogs) Debug.Log($"UpdateButtons ({this.gameObject.name})");
            
            foreach (var button in buttonList)
            {
                button.UpdateInteractable();
            }
            
            SetNavigationOrder();
        }

        private void SetNavigationOrder() {
            this.SetNavigationOrder(false, false);
        }

        private void SetNavigationOrder(bool includeInactive = false, bool includeNonInteractable = false)
        {
            if (_enableLogs) Debug.Log($"SetNavigationOrder ({this.gameObject.name})");
            
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
                if (_navHorizontal) 
                {
                    nav.selectOnLeft = filteredButtons[prevIndex];
                    nav.selectOnRight = filteredButtons[nextIndex];
                    if (_buttonNavLeft != null) nav.selectOnUp = _buttonNavLeft;
                    if (_buttonNavRight != null) nav.selectOnDown = _buttonNavRight;
                }
                else
                {
                    nav.selectOnUp = filteredButtons[prevIndex];
                    nav.selectOnDown = filteredButtons[nextIndex];
                    if (_buttonNavLeft != null) nav.selectOnLeft = _buttonNavLeft;
                    if (_buttonNavRight != null) nav.selectOnRight = _buttonNavRight;
                }
                
                filteredButtons[i].navigation = nav;
                // Debug.Log(filteredButtons[i].navigation.selectOnUp.name);
                // Debug.Log(buttonList.FirstOrDefault(b =>
                //     (includeInactive || b.gameObject.activeSelf) 
                //     && (includeNonInteractable || b.Button.IsInteractable()))?.Button.navigation.selectOnUp.name);
            }
        }

        protected void OnBuy(object prevStoreItem, object playerItem)
        {
            if (_useGraph) _itemTreeGraph.MarkItemAsObtained((PlayerItem)playerItem);
            if (_randomizeStoreOnBuy)
            {
                SeedManager.Instance.UpdateSteppedSeed("UpgradeStore");
                lastSelected =
                    buttonList.FirstOrDefault(buttonController => buttonController.ItemToPurchase == (PlayerItem)playerItem);
                GenerateStoreItems();
                if (lastSelected != null)
                {
                    lastSelected.SetStoreItemToSelected();
                    lastSelected.Button.Select();
                }
            }
        }

        private void OnSetStorePool(Object prevPool, Object newPool)
        {
            currentType = (StoreItemPoolType) newPool;
            _game_MenuIsOpen_Store.Value = true;
        }
        
        #endregion

        
    }
}