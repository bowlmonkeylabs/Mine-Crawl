using System;
using BML.ScriptableObjectCore.Scripts.Events;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace BML.Scripts.Store
{
    public class UiStoreEventInsufficientResourcesController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private DynamicGameEvent _onInsufficientResources;
        [SerializeField] private MMF_Player _insufficientResourcesFeedbacks;
        [SerializeField] private ResourceType _resourceType; 

        public enum ResourceType
        {
            Resource = 1,
            RareResource = 2,
            UpgradeResource = 3,
        } 

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _onInsufficientResources.Subscribe(OnInsufficientResources_Dynamic);
        }

        private void OnDisable()
        {
            _onInsufficientResources.Unsubscribe(OnInsufficientResources_Dynamic);
        }

        #endregion

        private void OnInsufficientResources_Dynamic(object prev, object curr)
        {
            OnInsufficientResources(curr as StoreItem);
        }
        
        private void OnInsufficientResources(StoreItem storeItem)
        {
            bool canAfford = true;
            switch (_resourceType)
            {
                case ResourceType.Resource:
                    canAfford = storeItem.CanAfford.Resource;
                    break;
                case ResourceType.RareResource:
                    canAfford = storeItem.CanAfford.RareResource;
                    break;
                case ResourceType.UpgradeResource:
                    canAfford = storeItem.CanAfford.UpgradeResource;
                    break;
            }

            if (!canAfford)
            {
                _insufficientResourcesFeedbacks.StopFeedbacks();
                _insufficientResourcesFeedbacks.PlayFeedbacks();
            }
        }
    }
}