using System;
using BML.Scripts.CaveV2.CaveGraph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class SpawnPoint : MonoBehaviour
    {
        #region Inspector
        
        [ShowInInspector, ReadOnly] public CaveNodeData ParentNode { get; set; }
        
        [SerializeField] [Range(0f, 1f)] private float _startingSpawnChance = 1f;
        [ShowInInspector, ReadOnly] public float SpawnChance { get; set; } = 1f;

        // TODO fetch current object tag and display it. Show a warning/error if object is NOT tagged.
        
        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            ResetSpawnProbability();
        }

        #endregion
        
        #region Public interface

        public void ResetSpawnProbability()
        {
            SpawnChance = _startingSpawnChance;

            // TODO calculate base on object parameters
        }
        
        #endregion
    }
}