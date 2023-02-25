using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.CaveV2;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    [ExecuteAlways]
    public class LootManager : MonoBehaviour
    {
        #region Inspector

        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;
        [Required, SerializeField] private GameEvent _onAfterGenerateLevelObjects;
        
        #endregion 
        
        #region Unity lifecycle

        private void OnEnable()
        {
            _onAfterGenerateLevelObjects.Subscribe(InitLootRandomizers);
        }
        
        private void OnDisable()
        {
            _onAfterGenerateLevelObjects.Unsubscribe(InitLootRandomizers);
        }

        #endregion

        [Button]
        private void InitLootRandomizers()
        {
            // Init seed for random generator
            Random.InitState(SeedManager.Instance.GetSteppedSeed("LootTable"));
            
            // Roll all loot randomizers
            var lootRandomizers = _caveGenerator.GetComponentsInChildren<LootRandomizer>();
            foreach (var lootRandomizer in lootRandomizers)
            {
                lootRandomizer.SetRandomRoll(Random.value);
            }
        }
    }
}