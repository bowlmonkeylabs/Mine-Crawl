using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;
using BML.Scripts.Player.Items;

namespace BML.Scripts.UI
{
    public delegate void OnInteractibilityChanged();

    public class UiStoreButtonController : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;

        [HideInInspector] public Button Button => _button;
        [SerializeField] private Button _button;
        
        [SerializeField] private TMPro.TMP_Text _costText;
        [SerializeField] private UiStoreItemIconController _storeItemIcon;
        [SerializeField] private UiStoreItemDetailController _uiStoreItemDetailController;
        
        [SerializeField] private BoolVariable _isGodModeEnabled;

        [SerializeField] private PlayerItem _itemToPurchase;
        public PlayerItem ItemToPurchase => _itemToPurchase;
        
        #endregion

        #region Public interface

        public event OnInteractibilityChanged OnInteractibilityChanged;
        
        public void Init(PlayerItem itemToPurchase)
        {
            if(_itemToPurchase != null) {
                _itemToPurchase.OnAffordabilityChanged -= UpdateInteractable;
            }
            _itemToPurchase = itemToPurchase;
            _storeItemIcon.Init(_itemToPurchase);
            SetButtonText();
            UpdateInteractable();

            _itemToPurchase.OnAffordabilityChanged += UpdateInteractable;
        }

        public void Raise()
        {
            _onPurchaseEvent.Raise(_itemToPurchase);
        }

        public void SetStoreItemToSelected()
        {
            if(_uiStoreItemDetailController != null && _itemToPurchase != null)
            {
                _uiStoreItemDetailController.SetSelectedStoreItem(_itemToPurchase);
            }
        }
        
        #endregion

        #region UI control
        
        public void UpdateInteractable()
        {
            if(_isGodModeEnabled.Value)
            {
                _button.interactable = true;
                return;
            }

            bool canAffordItem = _itemToPurchase.CheckIfCanAfford();
            if (_button.interactable != canAffordItem)
            {
                _button.interactable = canAffordItem;
            }
        }

        private void SetButtonText()
        {
            _costText.text = _itemToPurchase.FormatCostsAsText();
        }
        
        #endregion
        
    }
}