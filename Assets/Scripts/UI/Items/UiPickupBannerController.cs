using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Player.Items;
using BML.Scripts.Player.Items.Mushroom;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace BML.Scripts.UI.Items
{
    public class UiPickupBannerController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private BoolVariable _isAnyMenuOpen;
        [SerializeField] private DynamicGameEvent _onItemDiscovered;
        
        [SerializeField] private TMP_Text _text;
        [SerializeField] private int _maxItemsToShow = 4;
        [SerializeField] private MMF_Player _showBannerFeedbacks;

        #region Unity lifecycle

        private void OnEnable()
        {
            ClearBanner();
            _playerInventory.OnAnyPlayerItemAdded += DisplayItemOnBanner;
            _onItemDiscovered.Subscribe(OnItemDiscoveredDynamic);
            _isAnyMenuOpen.Subscribe(OnAnyMenuOpenChanged);
        }

        private void OnDisable()
        {
            _playerInventory.OnAnyPlayerItemAdded -= DisplayItemOnBanner;
            _onItemDiscovered.Unsubscribe(OnItemDiscoveredDynamic);
            _isAnyMenuOpen.Unsubscribe(OnAnyMenuOpenChanged);
        }

        #endregion
        
        private void OnItemDiscoveredDynamic(object prev, object curr)
        {
            OnItemDiscovered(curr as PlayerItem);
        }

        private void OnItemDiscovered(PlayerItem item)
        {
            DisplayItemOnBanner(item);
        }

        private void OnAnyMenuOpenChanged(bool prev, bool curr)
        {
            if (curr)
            {
                // When a menu is opened, close the banner.
                _showBannerFeedbacks.StopFeedbacks(); // Stopping the feedbacks will hide the banner.
            }
            else
            {
                // When a menu is closed, do nothing.
            }
        }

        #region UI control
        
        private void DisplayItemOnBanner(PlayerItem item)
        {
            if (_isAnyMenuOpen.Value)
            {
                // Don't add items to banner if a menu is open; the assumption is:
                // 1. The banner is obscured by the menu
                // 2. The item probably came from the menu (e.g., store purchase), so the player should already be aware of it
                // So we just skip showing it
                return;
            }

            _text.text = item.Name;

            _showBannerFeedbacks.StopFeedbacks();
            _showBannerFeedbacks.PlayFeedbacks();
        }

        public void ClearBanner()
        {
            _text.text = "";
        }
        
        #endregion

    }
}