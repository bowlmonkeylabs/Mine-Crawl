using System;
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
        
        #endregion 
        
        #region Unity lifecycle

        private void Awake()
        {
           InitLootRandomizers();
        }

        #endregion
        
        [ShowInInspector] private const int _uniqueStepId = 337;

        [Button]
        private void InitLootRandomizers()
        {
            // Init seed for random generator
            var seed = _caveGenerator.CaveGenParams.Seed + _uniqueStepId;
            Random.InitState(seed);
            
            // Roll all loot randomizers
            var lootRandomizers = _caveGenerator.GetComponentsInChildren<LootRandomizer>();
            foreach (var lootRandomizer in lootRandomizers)
            {
                lootRandomizer.SetRandomRoll(Random.value);
            }
        }
    }
}