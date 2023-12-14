using UnityEngine;

namespace BML.Scripts.Player.Items.Mushroom
{
    public class MushroomItemVisualController : ItemVisualController
    {
        // [SerializeField] private Renderer _mushroomRenderer;
        // [SerializeField] private int _mushroomRendererCapMaterialIndex = 1;
        [SerializeField] private Transform _pickupCenter;

        protected override void UpdateAssignedItem()
        {
            // var mushroomItem = _item as MushroomItem;
            // if (mushroomItem == null)
            // {
            //     Debug.LogError("Can't assign non-mushroom item.");
            //     return;
            // }
            // var mushroomCapMaterial = _mushroomRenderer.materials[_mushroomRendererCapMaterialIndex];
            // mushroomCapMaterial.SetColor("_Color1", mushroomItem.MushroomItemVisual.Color1);
            // mushroomCapMaterial.SetColor("_Color2", mushroomItem.MushroomItemVisual.Color2);

            this.transform.localPosition = -_pickupCenter.localPosition;
        }
    }
}