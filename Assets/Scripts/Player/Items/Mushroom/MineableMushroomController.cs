using System.Linq;
using UnityEngine;

namespace BML.Scripts.Player.Items.Mushroom
{
    public class MineableMushroomController : MonoBehaviour
    {
        [SerializeField] private MushroomItem _mushroomItem;
        [SerializeField] private Renderer _mushroomRenderer;
        
        public void SetMushroomItem(PlayerItem mushroomItem)
        {
            _mushroomItem = mushroomItem as MushroomItem;
            UpdateMushroomItem();
        }

        public void SetMushroomItem(MushroomItem mushroomItem)
        {
            _mushroomItem = mushroomItem;
            UpdateMushroomItem();
        }
        
        private void UpdateMushroomItem()
        {
            // assign cap material from item to renderer
            var temp = Instantiate(_mushroomItem.MushroomItemVisual.ObjectPrefab);
            _mushroomRenderer.materials = temp.GetComponent<Renderer>().materials.ToArray();
            DestroyImmediate(temp);
        }
    }
}