﻿using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using BML.Scripts.Store;

namespace BML.Scripts.UI
{
    public class UiStoreButtonController : MonoBehaviour
    {
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;
        [SerializeField] private Button _button;
        [SerializeField] private TMPro.TMP_Text _costText;
        [SerializeField] private TMPro.TMP_Text _buyAmountText;
        [SerializeField] private UiStoreItemIconController _storeItemIcon;
        [SerializeField] private UiStoreItemDetailController _uiStoreItemDetailController;
        
        [SerializeField] private string _resourceIconText;
        [SerializeField] private string _rareResourceIconText;
        [SerializeField] private string _enemyResourceIconText;
        [SerializeField] private IntVariable _resourceCount;
        [SerializeField] private IntVariable _rareResourceCount;
        [SerializeField] private IntVariable _enemyResourceCount;
        [SerializeField] private BoolVariable _isGodModeEnabled;

        [SerializeField] private StoreItem _itemToPurchase;
        
        public void Init(StoreItem itemToPurchase)
        {
            _itemToPurchase = itemToPurchase;
            _storeItemIcon.Init(_itemToPurchase);
            SetButtonText();
            SetInteractable();
        }

        public void Raise()
        {
            _onPurchaseEvent.Raise(_itemToPurchase);
        }

        void Update() {
            SetInteractable();
        }

        public void SetStoreItemToSelected() {
            if(_uiStoreItemDetailController != null && _itemToPurchase != null) {
                _uiStoreItemDetailController.SetSelectedStoreItem(_itemToPurchase);
            }
        }

        private void SetInteractable() {
            if(_isGodModeEnabled.Value) {
                _button.interactable = true;
                return;
            }

            _button.interactable = _itemToPurchase._resourceCost <= _resourceCount.Value &&
                _itemToPurchase._rareResourceCost <= _rareResourceCount.Value &&
                _itemToPurchase._enemyResourceCost <= _enemyResourceCount.Value &&
                (!_itemToPurchase._hasMaxAmount || (_itemToPurchase._playerInventoryAmount.Value < _itemToPurchase._maxAmount.Value));
        }

        private void SetButtonText()
        {
            string costText = "";
            
            if (_itemToPurchase._resourceCost > 0)
                costText += $" + {_itemToPurchase._resourceCost}{_resourceIconText}";
                
            if (_itemToPurchase._rareResourceCost > 0)
                costText += $" + {_itemToPurchase._rareResourceCost}{_rareResourceIconText}";
                
            if (_itemToPurchase._enemyResourceCost > 0)
                costText += $" + {_itemToPurchase._enemyResourceCost}{_enemyResourceIconText}";

            //Remove leading +
            if (costText != "") costText = costText.Substring(3);

            _costText.text = costText;

            _buyAmountText.text = ""+_itemToPurchase._buyAmount;
        }
    }
}