using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items
{
    public class PickupShaderController : MonoBehaviour
    {
        [SerializeField, Required] private Renderer _renderer;
        [SerializeField] private PlayerItem _item;

        public void SetItem(PlayerItem item)
        {
            _item = item;
            if (item.ObjectPrefab == null)
            {
                UpdateAssignedItem();
            }
        }

        private void UpdateAssignedItem()
        {
            var material = _renderer.material;
            material.SetColor("_MainTexTint", _item.IconColor);
            material.SetTexture("_MainTex", _item.Icon.texture);
        }
    }
}