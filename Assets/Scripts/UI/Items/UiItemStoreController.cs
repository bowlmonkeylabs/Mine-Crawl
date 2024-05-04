using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Player.Items;
using BML.Scripts.Player.Items.Store;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BML.Scripts.UI.Items
{
    public class UiItemStoreController : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private bool _enableLogs = false;
        public bool EnableLogs => _enableLogs;

        [SerializeField, FoldoutGroup("Inventory")] private AbstractItemStoreInventory _storeInventory;
        [SerializeField, FoldoutGroup("Inventory"), Optional, InfoBox("OPTIONAL. See code for details.")] private DynamicGameEvent _onOpenStoreInventory;

        [SerializeField, FoldoutGroup("Store Manager Refs")] private DynamicGameEvent _tryPurchaseEvent;

        [SerializeField, FoldoutGroup("UI Refs")] private BoolVariable _isStoreMenuOpen;
        [SerializeField, FoldoutGroup("UI Refs")] private Transform _storeButtonsListContainer;
        [SerializeField, FoldoutGroup("UI Refs")] private TMP_Text _storeCallToActionText;
        [SerializeField, FoldoutGroup("UI Refs")] private Button _cancelButton;
        [SerializeField, FoldoutGroup("UI Refs")] private bool _navHorizontal = true;
        [SerializeField, FoldoutGroup("UI Refs"), LabelText("@(_navHorizontal ? \"Button Nav Up\" : \"Button Nav Left\")")] private Button _buttonNavLeft;
        [SerializeField, FoldoutGroup("UI Refs"), LabelText("@(_navHorizontal ? \"Button Nav Down\" : \"Button Nav Right\")")] private Button _buttonNavRight;
        
        #endregion

        #region Unity lifecycle

        private void Awake()
        {
#warning Remove this once we're done working on the stores/inventory?
            PopulateButtonList();
            if (_storeInventory != null)
            {
                UpdateStoreFromInventory();
            }
            _onOpenStoreInventory?.Subscribe(OnOpenStoreInventoryDynamic);
        }
        
        private void OnEnable()
        {
            if (_storeInventory != null)
            {
                _storeInventory.OnAvailableItemsChanged += UpdateStoreFromInventory;
            }
        }
        
        private void OnDisable()
        {
            if (_storeInventory != null)
            {
                _storeInventory.OnAvailableItemsChanged -= UpdateStoreFromInventory;
            }
        }

        private void OnDestroy()
        {
            _onOpenStoreInventory?.Unsubscribe(OnOpenStoreInventoryDynamic);
        }

        #endregion

        #region Dynamically open a store inventory
        // This section is only relevant if the DynamicGameEvent _opOpenStoreInventory is set; otherwise this store UI will only use the hard-wired _storeInventory.
        
        private void OnOpenStoreInventoryDynamic(object prev, object curr)
        {
            OnOpenStoreInventory(curr as AbstractItemStoreInventory);
        }
        
        private void OnOpenStoreInventory(AbstractItemStoreInventory storeInventory)
        {
            if (_storeInventory != null)
            {
                _storeInventory.OnAvailableItemsChanged -= UpdateStoreFromInventory;
            }
            _storeInventory = storeInventory;
            if (_storeInventory != null)
            {
                UpdateStoreFromInventory();
                _storeInventory.OnAvailableItemsChanged += UpdateStoreFromInventory;

                if (_isStoreMenuOpen != null)
                {
                    _isStoreMenuOpen.Value = true;
                }
            }
        }
        
        #endregion

        #region Manage UI buttons

        private List<UiStoreButtonController> _storeItemButtons = new List<UiStoreButtonController>();
        private UiStoreButtonController _lastSelected;

        private void PopulateButtonList()
        {
            _storeItemButtons.Clear();
            
            for (int i = 0; i < _storeButtonsListContainer.childCount; i++)
            {
                var buttonTransform = _storeButtonsListContainer.GetChild(i);
                Button button = buttonTransform.GetComponent<Button>();
                if (_cancelButton != null && button == _cancelButton)
                {
                    continue;
                }

                var buttonController = buttonTransform.GetComponent<UiStoreButtonController>();
                buttonController.ParentItemStoreController = this; // Ideally we should remember to assign this in the inspector, so we can remove this when we're done working on it. We only really need this code if we bring back dynamic shop button addition.
                    
                _storeItemButtons.Add(buttonController);
            }
        }

        private void UpdateStoreFromInventory()
        {
            bool isStoreInventoryDefined = (_storeInventory != null && _storeInventory.AvailableItems != null);
            if (isStoreInventoryDefined && _storeInventory.AvailableItems.Count > _storeItemButtons.Count)
            {
                throw new Exception("Not enough buttons to display all the available items.");
            }
            
            // Cache current selected before we change the active state of any buttons
            var lastSelected = EventSystem.current.currentSelectedGameObject;
            var lastSelectedButtonController = (lastSelected != null ? _storeItemButtons.FirstOrDefault(b => b != null && !b.SafeIsUnityNull() && b.gameObject == lastSelected) : null);

            // Assign new items to buttons and update active state
            for (int i = 0; i < _storeItemButtons.Count; i++)
            {
                var buttonController = _storeItemButtons[i];
                if (!buttonController.SafeIsUnityNull())
                {
                    if (isStoreInventoryDefined && i < _storeInventory.AvailableItems.Count)
                    {
                        var item = _storeInventory.AvailableItems[i];
                        buttonController.Init(item);
                        buttonController.gameObject.SetActive(true);
                    }
                    else
                    {
                        buttonController.gameObject.SetActive(false);
                    }
                }
            }
            
            // Attempt to restore selection
            if (lastSelected == null)
            { 
                // If none was selected, select default
                SelectDefault();
            }
            else if (!lastSelected.activeSelf || !(lastSelectedButtonController?.Button.IsInteractable() ?? true))
            { 
                // If last selected is now inactive or not interactable, try to select next best.
                // Favor not choosing the cancel button, if possible.
                Selectable check = null;
                
                if (_navHorizontal)
                {
                    check = lastSelectedButtonController.Button.navigation.selectOnLeft;
                    if (
                        (_cancelButton != null && check == _cancelButton) 
                        || check == null 
                        || !check.gameObject.activeSelf 
                        || !check.IsInteractable()
                    ) {
                        check = lastSelectedButtonController.Button.navigation.selectOnRight;
                    }
                }
                else
                {
                    check = lastSelectedButtonController.Button.navigation.selectOnUp;
                    if (
                        (_cancelButton != null && check == _cancelButton) 
                        || check == null 
                        || !check.gameObject.activeSelf 
                        || !check.IsInteractable()
                    ) {
                        check = lastSelectedButtonController.Button.navigation.selectOnDown;
                    }
                }
                
                if (check != null && check.gameObject.activeSelf && check.IsInteractable())
                {
                    if (_enableLogs) Debug.Log($"Selecting next button: {check.name}");
                    
                    check.Select();
                }
                else
                {
                    SelectDefault();
                }
            }
            
            SetNavigationOrder();
        }
        
        public void SelectDefault()
        {
            if (_enableLogs) Debug.Log($"SelectDefault ({this.gameObject.name})");
            
            var firstUsableButtonController = _storeItemButtons
                ?.FirstOrDefault(button => button.Button.gameObject.activeSelf && button.Button.IsInteractable());
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

        private void SetNavigationOrder(bool includeInactive = false, bool includeNonInteractable = false)
        {
            if (_enableLogs) Debug.Log($"SetNavigationOrder ({this?.gameObject?.name})");
            
            var filteredButtons = _storeItemButtons
                .Where(b =>
                    !b.SafeIsUnityNull()
                    && (includeInactive || b.gameObject.activeSelf) 
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

        #endregion

        #region Store interface

        public void TryPurchase(PlayerItem item)
        {
            _tryPurchaseEvent.Raise(new TryPurchaseEventPayload
            {
                Item = item,
                DidPurchaseCallback = (didPurchase) =>
                {
                    _storeInventory.BuyItem(item);
                },
            });
        }

        #endregion

    }
}