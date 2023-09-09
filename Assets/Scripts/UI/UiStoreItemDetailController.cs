using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Player;
using BML.Scripts.Player.Items;
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

        void OnEnable() {
            this.SetDetails("", null, "", "");
        }

        public void SetSelectedStoreItem(PlayerItem storeItem) {
            this.SetDetails(storeItem.Name, storeItem.Icon, storeItem.EffectDescription, storeItem.StoreDescription);
        }

        private void SetDetails(string itemLabel, Sprite itemIcon, string itemEffectDescription, string itemStoreDescription) {
            _itemTitleText.text = itemLabel;
            _itemImage.enabled = itemIcon != null;
            _itemImage.sprite = itemIcon;
            _itemEffectText.text = itemEffectDescription;
            _itemBodyText.text = itemStoreDescription;
        }
    }
}
