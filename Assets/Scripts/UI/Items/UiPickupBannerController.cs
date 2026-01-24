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

        [NonSerialized, ShowInInspector, ReadOnly]
        private List<string> _recentItems = new List<string>();

        #region Unity lifecycle

        private void OnEnable()
        {
            _playerInventory.OnAnyPlayerItemAdded += DisplayItemOnBanner;
            _onItemDiscovered.Subscribe(OnItemDiscoveredDynamic);
        }

        private void OnDisable()
        {
            _playerInventory.OnAnyPlayerItemAdded -= DisplayItemOnBanner;
            _onItemDiscovered.Unsubscribe(OnItemDiscoveredDynamic);
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

            _recentItems.Add(item.Name);

            // Show newest item first in the list
            // Show up to _maxItemsToShow items
            // Show an ellipsis if there are more items than we can show
            var itemsToShow = _recentItems.Skip(Mathf.Max(0, _recentItems.Count - _maxItemsToShow)).Reverse().ToList();
            if (_recentItems.Count > _maxItemsToShow)
            {
                itemsToShow.Add("...");
            }

            _text.text = string.Join("\n", itemsToShow);

            _showBannerFeedbacks.StopFeedbacks();
            _showBannerFeedbacks.PlayFeedbacks();
        }

        public void ClearBanner()
        {
            _recentItems.Clear();
            _text.text = "";
        }
        
        #endregion

    }
}