using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;
using BML.Scripts.Player.Items;
using Sirenix.Utilities;
using UnityEngine.Serialization;

namespace BML.Scripts.UI.Items
{
    public class UiStoreButtonController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] public UiItemStoreController ParentItemStoreController;

        [HideInInspector] public Button Button => _button;
        [SerializeField] private Button _button;
        
        [SerializeField] private TMPro.TMP_Text _costText;
        [SerializeField] private UiPlayerItemCounterController _uiPlayerItemIconController;
        [SerializeField] private UiStoreItemDetailController _uiStoreItemDetailController;

        [SerializeField] private PlayerInventory _playerInventory;

        [SerializeField] private PlayerItem _itemToPurchase;
        public PlayerItem ItemToPurchase => _itemToPurchase;

        private bool _enableLogs => ParentItemStoreController?.EnableLogs ?? false;
        
        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            if (_itemToPurchase != null)
            {
                _playerInventory.UnsubscribeOnBuyabilityChanged(_itemToPurchase, UpdateInteractable);
            }
        }

        #endregion

        #region Public interface
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemToPurchase"></param>
        /// <param name="skipUiUpdate">Use this parameter if you plan to call UpdateButtons() manually, to update all the buttons at once in a batch.</param>
        public void Init(PlayerItem itemToPurchase, bool skipUiUpdate = false)
        {
            if (_itemToPurchase != null)
            {
                _playerInventory.UnsubscribeOnBuyabilityChanged(_itemToPurchase, UpdateInteractable);
            }
            _itemToPurchase = itemToPurchase;
            _uiPlayerItemIconController.Item = itemToPurchase;
            if (!skipUiUpdate)
            {
                SetButtonText();
                UpdateInteractable();
            }

            if (_itemToPurchase != null)
            {
                _playerInventory.SubscribeOnBuyabilityChanged(_itemToPurchase, UpdateInteractable);
            }
        }

        public void TryPurchase()
        {
            ParentItemStoreController.TryPurchase(_itemToPurchase);
        }

        public void SetStoreItemToSelected()
        {
            if (_uiStoreItemDetailController != null && _itemToPurchase != null)
            {
                _uiStoreItemDetailController.SetSelectedStoreItem(_itemToPurchase);
            }
        }
        
        #endregion

        #region UI control
        
        public void UpdateInteractable()
        {
            if (_enableLogs) Debug.Log($"UpdateInteractable ({(_button.SafeIsUnityNull() ? "null" : _button.gameObject.name)})");

            bool canBuyItem = _itemToPurchase != null && _playerInventory.CheckIfCanBuy(_itemToPurchase, true);
            if (!_button.SafeIsUnityNull() && _button.interactable != canBuyItem)
            {
                _button.interactable = canBuyItem;
            }
        }

        public void SetButtonText()
        {
            if (!_costText.SafeIsUnityNull())
            {
                _costText.text = _itemToPurchase?.FormatCostsAsText() ?? "";
            }
        }
        
        #endregion
        
    }
}