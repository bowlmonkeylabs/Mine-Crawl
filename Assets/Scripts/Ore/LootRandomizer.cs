using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    public class LootRandomizer : MonoBehaviour
    {
        [SerializeField] private LootTable _lootTable;

        [ShowInInspector] private float _randomRoll;

        public void SetRandomRoll(float value)
        {
            _randomRoll = value;
        }
        
        public void Drop()
        {
            _lootTable.Evaluate(_randomRoll);
        }
    }
}