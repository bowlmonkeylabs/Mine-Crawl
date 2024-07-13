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
        
        [FormerlySerializedAs("defaultItemVisualController")] [FormerlySerializedAs("_defaultPickupVisualController")] [SerializeField, Tooltip("This default mesh is only used when the item has no ObjectPrefab assigned.")]
        private DefaultItemVisualController _defaultItemVisualController;

        [NonSerialized] private ItemVisualController _itemVisualController;

        [SerializeField] private Transform _pickupVisualParent;
        
        [SerializeField] private Rigidbody _pickupRigidbody;

        [SerializeField] private Timer _pickupDelayTimer;
        [SerializeField] private MMF_Player _idleFeedbacks;
        [SerializeField] private MMF_Player _activateFeedbacks;
        [SerializeField] private MMF_Player _receiveFeedbacks;
        [SerializeField] private GameObject _activateTrigger;
        
        [SerializeField] private DynamicGameEvent _onReceivePickup;
        [SerializeField] private PlayerInventory _playerInventory;

        #endregion
        
        #region Public interface
        
        public Rigidbody PickupRigidbody => _pickupRigidbody;

        public void SetItem(PlayerItem item)
        {
            if (_item != null)
            {
                _playerInventory.UnsubscribeOnPickupabilityChanged(_item, UpdateActivated);
            }
            _item = item;
            if (_item != null)
            {
                _playerInventory.SubscribeOnPickupabilityChanged(_item, UpdateActivated);
            }
            UpdateAssignedItem();
        }

        public void SetPickupDelay(float timeSeconds)
        {
            _pickupDelayTimer.Duration = timeSeconds;
            _pickupDelayTimer.ResetTimer();
            _pickupDelayTimer.StartTimer();
        }

        public void TryActivatePickup()
        {
            if (_playerInventory.CheckIfCanAddItem(_item))
            {
                _activateFeedbacks.PlayFeedbacks();
            }
        }

        public void TryReceivePickup()
        {
            if (_playerInventory.CheckIfCanAddItem(_item))
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
                _playerInventory.UnsubscribeOnPickupabilityChanged(_item, UpdateActivated);
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
            _defaultItemVisualController.SetItem(_item);
            _defaultItemVisualController.gameObject.SetActive(useDefault3dObject);

            if (!useDefault3dObject)
            {
                var newVisualGameObject = GameObjectUtils.SafeInstantiate(true, _item.ObjectPrefab, _pickupVisualParent);
                _itemVisualController = newVisualGameObject.GetComponent<ItemVisualController>();
                if (_itemVisualController != null)
                {
                    _itemVisualController.SetItem(_item);
                }
            }

            // TODO assign pickup sound overrides?
        }

        private void UpdateActivated()
        {
            if (_playerInventory.CheckIfCanAddItem(_item) && _pickupDelayTimer.IsFinished)
            {
                // _activateTrigger.SetActive(true);
                // actually it seems better if activation is only turned on on trigger enter
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