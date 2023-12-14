using UnityEngine;

namespace BML.Scripts.Player.Items
{
    public class DefaultItemVisualController : ItemVisualController
    {
        [SerializeField] private SpriteRenderer _spriteRendererFront;
        [SerializeField] private SpriteRenderer _spriteRendererBack;

        protected override void UpdateAssignedItem()
        {
            if (_item == null) return;

            var iconColor = (_item.UseIconColor ? _item.IconColor : Color.white);
            _spriteRendererFront.sprite = _item.Icon;
            _spriteRendererFront.color = iconColor;
            
            _spriteRendererBack.sprite = _item.Icon;
            _spriteRendererBack.color = iconColor;
        }
    }
}