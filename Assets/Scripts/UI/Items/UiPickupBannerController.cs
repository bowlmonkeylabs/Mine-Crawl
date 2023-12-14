using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.Player.Items;
using BML.Scripts.Player.Items.Mushroom;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

namespace BML.Scripts.UI.Items
{
    public class UiPickupBannerController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private DynamicGameEvent _onItemDiscovered;
        
        [SerializeField] private TMP_Text _text;
        [SerializeField] private MMF_Player _showBannerFeedbacks;

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
            _text.text = item.Name;
            _showBannerFeedbacks.StopFeedbacks();
            _showBannerFeedbacks.PlayFeedbacks();
        }
        
        #endregion

    }
}