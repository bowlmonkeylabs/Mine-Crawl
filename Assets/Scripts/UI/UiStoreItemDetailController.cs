using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Store;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiStoreItemDetailController : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text _itemTitleText;
        [SerializeField] private Image _itemImage;
        [SerializeField] private TMPro.TMP_Text _itemEffectText;
        [SerializeField] private TMPro.TMP_Text _itemBodyText;

        public void SetSelectedStoreItem(StoreItem storeItem) {
            _itemTitleText.text = storeItem._itemLabel;
            _itemImage.sprite = storeItem._itemIcon;
            _itemEffectText.text = String.Join(" + ", storeItem.PurchaseItems.Select(pi => pi._storeText).AsEnumerable());
            _itemBodyText.text = storeItem._itemStoreDescription;
        }
    }
}
