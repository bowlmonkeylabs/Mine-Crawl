using System.Collections;
using Sirenix.Utilities;
using BML.Scripts.Store;
using UnityEngine;

namespace BML.Scripts.UI
{
    public class UiStoreItemIconController : MonoBehaviour
    {
        [SerializeField] private StoreItem _storeItem;
        [SerializeField] private TMPro.TMP_Text _iconText;
        [SerializeField] private TMPro.TMP_Text _tooltipText;

        public void Init(StoreItem storeItem) {
            _storeItem = storeItem;
            SetIconText();
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
