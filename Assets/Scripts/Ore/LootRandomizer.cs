using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class LootRandomizer : MonoBehaviour
    {
        [FormerlySerializedAs("_lootTable")] [SerializeField] private LootTableVariable lootTableVariable;
        [SerializeField] private BoolReference _levelObjectsGenerated;
        [SerializeField] private UnityEvent<GameObject> _onDrop;

        [ShowInInspector] private float _randomRoll;

        private void Start()
        {
            // This is the cover the case where ore is placed at runtime and loot
            // manager has already done init.
            if (_levelObjectsGenerated.Value)
            {
                // If we want ore placed at runtime in game (outside editor), probably
                // want seed based off of something like world pos of ore that is more
                // grounded in the game world
                Random.InitState(SeedManager.Instance.GetSteppedSeed("LootTable" + transform.position));
                SetRandomRoll(Random.value);
            }
            
        }

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