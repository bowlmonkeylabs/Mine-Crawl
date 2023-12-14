using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items
{
    public abstract class ItemVisualController : MonoBehaviour
    {
        [OnValueChanged("OnItemChangedInInspector")]
        [SerializeField] protected PlayerItem _item;

        public void SetItem(PlayerItem item)
        {
            _item = item;
            UpdateAssignedItem();
        }

        protected abstract void UpdateAssignedItem();

        private void OnItemChangedInInspector()
        {
            UpdateAssignedItem();
        }

    }
}