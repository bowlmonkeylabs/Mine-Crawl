using UnityEngine;

namespace BML.Scripts
{
    public class LootRandomizer : MonoBehaviour
    {
        [SerializeField] private LootTable _lootTable;

        public void Drop()
        {
            var rand = Random.value;
            _lootTable.Evaluate(rand);
        }
    }
}