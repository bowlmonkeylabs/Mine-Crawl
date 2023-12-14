using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.Utils;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.Player.Items
{
    public class ItemPickupController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private PlayerItem _item;

        // [SerializeField, Tooltip("This default mesh is only used when the item has no ObjectPrefab assigned.")] 
        // private PickupShaderController _pickupShaderControllerForDefaultMesh;
        
        [FormerlySerializedAs("_defaultPickupVisualController")] [SerializeField, Tooltip("This default mesh is only used when the item has no ObjectPrefab assigned.")]
        private DefaultItemVisualController defaultItemVisualController;

        [SerializeField] private Transform _pickupVisualParent;
        
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
                _item.OnPickupabilityChanged -= UpdateActivated;
            }
            _item = item;
            if (_item != null)
            {
                _item.OnPickupabilityChanged += UpdateActivated;
            }
            UpdateAssignedItem();
        }

        public void TryActivatePickup()
        {
            if (_item.CheckIfCanPickup())
            {
                _activateFeedbacks.PlayFeedbacks();
            }
        }

        public void TryReceivePickup()
        {
            if (_item.CheckIfCanPickup())
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

        private void OnDestroy()
        {
            if (_item != null)
            {
                _item.OnPickupabilityChanged -= UpdateActivated;
            }
        }

        #endregion

        #region Display control
        
        private void UpdateAssignedItem()
        {
            if (_item == null) return;
            
            bool useDefault3dObject = (_item.ObjectPrefab == null);
            // _pickupShaderControllerForDefaultMesh.SetItem(_item);
            // _pickupShaderControllerForDefaultMesh.gameObject.SetActive(useDefault3dObject);
            defaultItemVisualController.SetItem(_item);
            defaultItemVisualController.gameObject.SetActive(useDefault3dObject);

            if (!useDefault3dObject)
            {
                var newVisualGameObject = GameObjectUtils.SafeInstantiate(true, _item.ObjectPrefab, _pickupVisualParent);
                var newVisualController = newVisualGameObject.GetComponent<ItemVisualController>();
                newVisualController.SetItem(_item);
            }

            // TODO assign pickup sound overrides?
        }

        private void UpdateActivated()
        {
            if (_item.CheckIfCanPickup())
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