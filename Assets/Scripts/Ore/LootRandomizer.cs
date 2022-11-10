using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class LootRandomizer : MonoBehaviour
    {
        [SerializeField] private LootTable _lootTable;

        [SerializeField] private UnityEvent<GameObject> _onDrop;

        [ShowInInspector] private float _randomRoll;

        public void SetRandomRoll(float value)
        {
            _randomRoll = value;
        }
        
        public void Drop()
        {
            var dropPrefabs = _lootTable.Evaluate(_randomRoll);
            dropPrefabs.ForEach(prefab => _onDrop?.Invoke(prefab));
        }
    }
}