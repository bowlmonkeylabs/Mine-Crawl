using System.Collections;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BML.Scripts.Player;
using BML.Scripts.Player.Items;

namespace BML.Scripts.UI
{
    public class UiStoreItemIconController : MonoBehaviour
    {
        [SerializeField] private PlayerItem _storeItem;
        [SerializeField] private Image _iconImage;
        [SerializeField] private CountMode _countMode = CountMode.None;
        [SerializeField] private GameObject _countGameObject;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private UiStoreItemDetailController _uiStoreItemDetailController;

        public enum CountMode
        {
            None,
            Inventory,
            BuyAmount,
        }
        
        public void Init(PlayerItem storeItem) 
        {
            _storeItem = storeItem;
            _iconImage.sprite = _storeItem.Icon;
            _iconImage.color = _storeItem.UseIconColor ? _storeItem.IconColor : Color.white;
            
            if (_countMode == CountMode.None)
            {
                _countGameObject.SetActive(false);
            }
            else
            {
                int countValue = 0;
                if (_countMode == CountMode.Inventory)
                {
                    countValue = 1;
                }
                else if (_countMode == CountMode.BuyAmount)
                {
                    countValue = 1;
                }
                
                bool doShowCount = (countValue > 1);
                _countGameObject.SetActive(doShowCount);
                _countText.text = countValue.ToString();
            }
        }

        public void SetStoreItemToSelected() {
            if(_uiStoreItemDetailController != null && _storeItem != null) {
                _uiStoreItemDetailController.SetSelectedStoreItem(_storeItem);
            }
        }
    }
}
