using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items
{
    public abstract class ItemVisualController : MonoBehaviour
    {
        [OnValueChanged("OnItemChangedInInspector")]
        [SerializeField] protected PlayerItem _item;

        [SerializeField] private MMF_Player _pickupIdleFeedbacks;

        public void SetItem(PlayerItem item)
        {
            _item = item;
            UpdateAssignedItem();
            _pickupIdleFeedbacks.PlayFeedbacks();
        }

        protected abstract void UpdateAssignedItem();

        private void OnItemChangedInInspector()
        {
            UpdateAssignedItem();
        }

    }
}