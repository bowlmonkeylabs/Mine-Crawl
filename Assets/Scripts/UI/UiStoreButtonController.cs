using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;
using BML.Scripts.Store;

namespace BML.Scripts.UI
{
    public class UiStoreButtonController : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;

        [HideInInspector] public Button Button => _button;
        [SerializeField] private Button _button;
        
        [SerializeField] private TMPro.TMP_Text _costText;
        [SerializeField] private TMPro.TMP_Text _buyAmountText;
        [SerializeField] private UiStoreItemIconController _storeItemIcon;
        [SerializeField] private UiStoreItemDetailController _uiStoreItemDetailController;
        
        [SerializeField] private string _resourceIconText;
        [SerializeField] private string _rareResourceIconText;
        [SerializeField] private IntVariable _resourceCount;
        [SerializeField] private IntVariable _rareResourceCount;
        [SerializeField] private IntVariable _upgradesAvailableCount;
        [SerializeField] private BoolVariable _isGodModeEnabled;

        [SerializeField] private StoreItem _itemToPurchase;
        public StoreItem ItemToPurchase => _itemToPurchase;
        
        #endregion

        #region Public interface
        
        public void Init(StoreItem itemToPurchase)
        {
            _itemToPurchase = itemToPurchase;
            _storeItemIcon.Init(_itemToPurchase);
            SetButtonText();
            UpdateInteractable();
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

            bool canAffordItem = _itemToPurchase
                .CheckIfCanAfford(_resourceCount.Value, _rareResourceCount.Value, _upgradesAvailableCount.Value)
                .Overall;
            bool interactable = canAffordItem && (!_itemToPurchase._hasMaxAmount || (_itemToPurchase._playerInventoryAmount.Value < _itemToPurchase._maxAmount.Value));
            if (_button.interactable != interactable)
            {
                _button.interactable = interactable;
            }
        }

        private void SetButtonText()
        {
            string costText = "";
            
            if (_itemToPurchase._resourceCost > 0)
                costText += $" + {_itemToPurchase._resourceCost}{_resourceIconText}";
                
            if (_itemToPurchase._rareResourceCost > 0)
                costText += $" + {_itemToPurchase._rareResourceCost}{_rareResourceIconText}";
                
            if (_itemToPurchase._upgradeCost > 0)
                costText += $" + {_itemToPurchase._upgradeCost}U";

            //Remove leading +
            if (costText != "") costText = costText.Substring(3);

            _costText.text = costText;

            _buyAmountText.text = ""+_itemToPurchase._buyAmount;
        }
        
        #endregion
        
    }
}