using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class LootRandomizer : MonoBehaviour
    {
        [FormerlySerializedAs("_lootTable")] [SerializeField] private LootTableVariable lootTableVariable;

        [SerializeField] private UnityEvent<GameObject> _onDrop;

        [ShowInInspector] private float _randomRoll;

        public void SetRandomRoll(float value)
        {
            _randomRoll = value;
        }
        
        public void Drop()
        {
            var lootTableEntry = lootTableVariable.Value.Evaluate(_randomRoll);
            foreach (var prefab in lootTableEntry.DropPrefabs)
            {
                _onDrop?.Invoke(prefab);
            }
        }
    }
}