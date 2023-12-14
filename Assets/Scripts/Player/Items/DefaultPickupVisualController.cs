using UnityEngine;

namespace BML.Scripts.Player.Items
{
    public class DefaultPickupVisualController : MonoBehaviour
    {
        [SerializeField] private PlayerItem _item;
        
        [SerializeField] private SpriteRenderer _spriteRendererFront;
        [SerializeField] private SpriteRenderer _spriteRendererBack;

        public void SetItem(PlayerItem item)
        {
            _item = item;
            UpdateAssignedItem();
        }

        private void UpdateAssignedItem()
        {
            if (_item == null) return;

            // if (_spriteRendererFront.sprite != null)
            // {
            //     var oldWidth = _spriteRendererFront.sprite.texture.width;
            //     var newWidth = _item.Icon.texture.width;
            //     var scalingFactor = oldWidth / newWidth;
            //     _spriteRendererFront.transform.localScale *= scalingFactor;
            //     _spriteRendererBack.transform.localScale *= scalingFactor;
            // }
            
            var iconColor = (_item.UseIconColor ? _item.IconColor : Color.white);
            _spriteRendererFront.sprite = _item.Icon;
            _spriteRendererFront.color = iconColor;
            
            _spriteRendererBack.sprite = _item.Icon;
            _spriteRendererBack.color = iconColor;
        }
    }
}