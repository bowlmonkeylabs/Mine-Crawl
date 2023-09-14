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

namespace BML.Scripts.UI
{
    public class UiStoreCanvasController : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private bool _useGraph = true;
        [ShowIf("_useGraph"), SerializeField] private Player.Items.ItemTreeGraph _itemTreeGraph;
        [HideIf("_useGraph"), SerializeField] private List<PlayerItem> _activeItemPool;
        [HideIf("_useGraph"), SerializeField] private List<PlayerItem> _passiveItemPool;
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField] private Transform _listContainerStoreButtons;
        [SerializeField] private Button _cancelButton;
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

        #region Unity lifecycle
        
        private void Awake()
        {
            #warning Remove this once we're done working on the stores/inventory
            {
                for(int i = 0; i < _listContainerStoreButtons.childCount; i++)
                {
                    var buttonTransform = _listContainerStoreButtons.GetChild(i);
                    var buttonController = buttonTransform.GetComponent<UiStoreButtonController>();
                    if (_cancelButton != null && buttonController.Button == _cancelButton)
                    {
                        continue;
                    }
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
        
        #endregion
        
        #region Public interface

        private List<PlayerItem> GetItemPool()
        {
            List<PlayerItem> itemPool;
            if(_useGraph) 
            {
                itemPool = _itemTreeGraph.GetUnobtainedItemPool();
            } 
            else 
            {
                itemPool = new List<PlayerItem>(_activeItemPool);
                itemPool.AddRange(_passiveItemPool);
            }
            return itemPool;
        }

        [Button("Generate Store Items")]
        public void GenerateStoreItems()
        {
            DestroyStoreItems();
            
            if (_enableLogs) Debug.Log($"GenerateStoreItems ({this.gameObject.name})");

            List<PlayerItem> shownStoreItems = GetItemPool();
            
            if(_randomizeStoreOnBuy) {
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
                var buttonController = buttonTransform.GetComponent<UiStoreButtonController>();
                if (_cancelButton != null && buttonController.Button == _cancelButton)
                {
                    continue;
                }
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
            if (!_randomizeStoreOnBuy)
            {
                GenerateStoreItems();
                return;
            }
            
            var itemPool = GetItemPool();
            var statusOfItemsCurrentlyInShopButtons = buttonList.Select(button => 
                (button: button, stillInItemPool: itemPool.Contains(button.ItemToPurchase))).ToList();
            
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
            _itemTreeGraph.MarkItemAsObtained((PlayerItem)playerItem);
            // SeedManager.Instance.UpdateSteppedSeed("UpgradeStore");
            if (_randomizeStoreOnBuy)
            {
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
        
        #endregion

        
    }
}