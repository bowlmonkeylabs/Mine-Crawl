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
    public delegate void OnInteractibilityChanged();

    public class UiStoreButtonController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] public UiStoreCanvasController ParentStoreCanvasController;
        
        [SerializeField] private DynamicGameEvent _onPurchaseEvent;

        [HideInInspector] public Button Button => _button;
        [SerializeField] private Button _button;
        
        [SerializeField] private TMPro.TMP_Text _costText;
        [SerializeField] private UiPlayerItemCounterController _uiPlayerItemIconController;
        [SerializeField] private UiStoreItemDetailController _uiStoreItemDetailController;
        
        [SerializeField] private BoolVariable _isGodModeEnabled;

        [SerializeField] private PlayerItem _itemToPurchase;
        public PlayerItem ItemToPurchase => _itemToPurchase;

        private bool _enableLogs => ParentStoreCanvasController?.EnableLogs ?? false;
        
        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            if (_itemToPurchase != null)
            {
                _itemToPurchase.OnAffordabilityChanged -= UpdateInteractable;
            }
        }

        #endregion

        #region Public interface

        public event OnInteractibilityChanged OnInteractibilityChanged;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemToPurchase"></param>
        /// <param name="skipUiUpdate">Use this parameter if you plan to call UpdateButtons() manually, to update all the buttons at once in a batch.</param>
        public void Init(PlayerItem itemToPurchase, bool skipUiUpdate = false)
        {
            if (_itemToPurchase != null)
            {
                _itemToPurchase.OnAffordabilityChanged -= UpdateInteractable;
            }
            _itemToPurchase = itemToPurchase;
            _uiPlayerItemIconController.Item = itemToPurchase;
            if (!skipUiUpdate)
            {
                SetButtonText();
                UpdateInteractable();
            }

            _itemToPurchase.OnAffordabilityChanged += UpdateInteractable;
        }

        public void Raise()
        {
            _onPurchaseEvent.Raise(_itemToPurchase);
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
            if (_enableLogs) Debug.Log($"UpdateInteractable ({_button.gameObject.name})");
            
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

        public void SetButtonText()
        {
            if (!_costText.SafeIsUnityNull())
            {
                _costText.text = _itemToPurchase.FormatCostsAsText();
            }
        }
        
        #endregion
        
    }
}