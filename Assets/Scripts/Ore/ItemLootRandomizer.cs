using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using BML.Scripts.Player.Items;
using BML.Scripts.ScriptableObjectVariables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class ItemLootRandomizer : MonoBehaviour
    {
        [FormerlySerializedAs("_lootTable")] [SerializeField] private LootTableVariable lootTableVariable;
        [SerializeField] private BoolReference _levelObjectsGenerated;
        [SerializeField] private UnityEvent<GameObject> _onDrop;
        [SerializeField] private UnityEvent _onNothingDrop;

        [ShowInInspector] private float _randomRoll;
        [SerializeField] private BoolReference _levelObjectsGenerated;

        private int _seedIncrement = 0;

        private void Start()
        {
            SelfSetRandomRoll();
        }

        public void SelfSetRandomRoll()
        {
            // This is the cover the case where ore is placed at runtime and loot
            // manager has already done init.
            if (_levelObjectsGenerated.Value)
            {
                // If we want ore placed at runtime in game (outside editor), probably
                // want seed based off of something like world pos of ore that is more
                // grounded in the game world
                Random.InitState(SeedManager.Instance.GetSteppedSeed("LootTable" + transform.position + _seedIncrement));
                _seedIncrement++;
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
            if(lootTableEntry.Key == LootTableKey.Nothing) {
                _onNothingDrop.Invoke();
                return;
            }
            
            foreach (var itemDeal in lootTableEntry.Drops)
            {
                _onDrop?.Invoke(itemDeal);
            }
            SelfSetRandomRoll();
        }
    }
}