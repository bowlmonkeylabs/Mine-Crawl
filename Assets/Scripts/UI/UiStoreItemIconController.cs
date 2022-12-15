using System.Collections;
using Sirenix.Utilities;
using BML.Scripts.Store;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class UiStoreItemIconController : MonoBehaviour
    {
        [SerializeField] private StoreItem _storeItem;
        [SerializeField] private Image _iconImage;
        [SerializeField] private UiStoreItemDetailController _uiStoreItemDetailController;

        public void Init(StoreItem storeItem) 
        {
            _storeItem = storeItem;
            _iconImage.sprite = _storeItem._itemIcon;
        }

        public void SetStoreItemToSelected() {
            if(_uiStoreItemDetailController != null && _storeItem != null) {
                _uiStoreItemDetailController.SetSelectedStoreItem(_storeItem);
            }
        }
    }
}
