using System;
using BML.ScriptableObjectCore.Scripts.Events;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items
{
    public class ItemPickupController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private PlayerItem _item;

        // [SerializeField, Tooltip("This default mesh is only used when the item has no ObjectPrefab assigned.")] 
        // private PickupShaderController _pickupShaderControllerForDefaultMesh;
        
        [SerializeField, Tooltip("This default mesh is only used when the item has no ObjectPrefab assigned.")]
        private DefaultPickupVisualController _defaultPickupVisualController;
        [SerializeField] private MMF_Player _idleFeedbacks;
        [SerializeField] private MMF_Player _activateFeedbacks;
        [SerializeField] private MMF_Player _receiveFeedbacks;
        [SerializeField] private GameObject _activateTrigger;
        
        [SerializeField] private DynamicGameEvent _onReceivePickup;

        #endregion
        
        #region Public interface

        public void SetItem(PlayerItem item)
        {
            if (_item != null)
            {
                _item.AllowPickupCondition.Unsubscribe(UpdateActivated);
            }
            _item = item;
            UpdateAssignedItem();
        }

        public void TryActivatePickup()
        {
            bool allowPickup = _item?.AllowPickupCondition.Value ?? false;
            if (allowPickup)
            {
                _activateFeedbacks.PlayFeedbacks();
            }
        }

        public void TryReceivePickup()
        {
            bool allowPickup = _item?.AllowPickupCondition.Value ?? false;
            if (allowPickup)
            {
                _idleFeedbacks.StopFeedbacks();
                _receiveFeedbacks.PlayFeedbacks();
                _onReceivePickup?.Raise(_item);
            }
        }
        
        #endregion
        
        #region Unity lifecycle

        private void Awake()
        {
            // TODO does this really need to run on awake?
            UpdateAssignedItem();
        }

        #endregion

        #region Display control
        
        private void UpdateAssignedItem()
        {
            if (_item == null) return;
            
            bool useDefault3dObject = (_item.ObjectPrefab == null);
            // _pickupShaderControllerForDefaultMesh.SetItem(_item);
            // _pickupShaderControllerForDefaultMesh.gameObject.SetActive(useDefault3dObject);
            _defaultPickupVisualController.SetItem(_item);
            _defaultPickupVisualController.gameObject.SetActive(useDefault3dObject);

            // TODO assign 3d representation
            // TODO assign pickup sound overrides?
        }

        private void UpdateActivated()
        {
            bool allowPickup = _item?.AllowPickupCondition.Value ?? false;
            if (allowPickup)
            {
                _activateTrigger.SetActive(true);
            }
            else
            {
                _activateFeedbacks.StopFeedbacks();
                _activateTrigger.SetActive(false);
            }
        }
        
        #endregion
    }
}