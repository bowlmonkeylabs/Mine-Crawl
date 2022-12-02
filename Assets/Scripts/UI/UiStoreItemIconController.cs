using System.Collections;
using Sirenix.Utilities;
using BML.Scripts.Store;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.UI
{
    public class UiStoreItemIconController : MonoBehaviour
    {
        [SerializeField] private StoreItem _storeItem;
        [SerializeField] private TMPro.TMP_Text _iconText;
        [SerializeField] private TMPro.TMP_Text _tooltipText;
        [SerializeField, Required] private UiEventHandler _uiEventHandler;

        public void Init(StoreItem storeItem, GameObject propagateSubmit = null, GameObject propagateCancel = null) 
        {
            _storeItem = storeItem;
            SetIconText();
            _uiEventHandler.PropagateSubmit = propagateSubmit;
            _uiEventHandler.PropagateCancel = propagateCancel;
        }

        private void SetIconText()
        {
            string resultText = "";
            
            foreach (var purchaseItem in _storeItem.PurchaseItems)
            {
                if (!purchaseItem._storeText.IsNullOrWhitespace())  //Dont add if left blank (Ex. for max health also inc health but dont show)
                    resultText += $" + {purchaseItem._storeText}";
            }

            //Remove leading +
            if (resultText != "") resultText = resultText.Substring(3);

            _tooltipText.text = resultText;

            _iconText.text = _storeItem._itemLabel;
        }
    }
}
