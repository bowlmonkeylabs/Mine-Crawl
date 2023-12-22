using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class EnemySpawnPoint : SpawnPoint
    {
        #region Inspector

        [TitleGroup("Spawn behavior")]
        [SerializeField] [Range(0f, 1f)] private float _startingSpawnChance = 1f;
        [SerializeField] private bool _disableIfPlayerVisited = false;
        [SerializeField, Tooltip("If the last enemy to occupy this spawn point was despawned, this will ensure they get respawned immediately when the spawn point is active again.")] 
        private bool _persistSpawns = false;
        [ShowInInspector, ReadOnly] private bool _previousSpawnWasDespawned = false;
        [SerializeField, Tooltip("Guarantees this spawn point will be populated as soon as it becomes active.")] 
        private bool _guaranteeSpawnIfInRange = false;
        [ShowIf("_guaranteeSpawnIfInRange"), SerializeField, Tooltip("The range at which a guaranteed spawn point will become active.")] 
        private int _guaranteeSpawnRange = 1;
        [SerializeField, Tooltip("Prevents more than one enemy from spawning here at a time. No other enemy can spawn until the occupier either dies or despawns.")] 
        private bool _limitToOneSpawnAtATime = false;
        [SerializeField, Tooltip("Limit the number of enemies this spawner can produce for it's lifetime.")] 
        private int _spawnLimit = -1;
        [ShowInInspector, ReadOnly] public bool ReachedSpawnLimit => (_spawnLimit > -1 && _spawnCount >= _spawnLimit);
        [ShowInInspector, ReadOnly] private int _spawnCount = 0;
        [SerializeField, Tooltip("If enabled, the above 'spawn limit' will only be applied to spawns from 'guarantee spawn if in range'; after the limit is reached guaranteed spawns will cease, but the spawn point will still be available for the EnemySpawnManager to repopulate through it's normal spawning routine.")] 
        private bool _applySpawnLimitOnlyToGuaranteedSpawns = false;
        [FormerlySerializedAs("IgnoreGlobalSpawnCap")] [SerializeField, Tooltip("Ignore the EnemySpawnManager's global concurrent spawn limit.")] 
        private bool _ignoreGlobalSpawnCap = false;
        [ShowInInspector, ReadOnly] public bool IgnoreGlobalSpawnCap => _ignoreGlobalSpawnCap && (!OnlyIgnoreGlobalSpawnCapForGuaranteedSpawns || (_guaranteeSpawnIfInRange && (_spawnLimit <= -1 || _spawnLimit >= _spawnCount))); 
        [SerializeField] 
        public bool OnlyIgnoreGlobalSpawnCapForGuaranteedSpawns = true; 
        
        [TitleGroup("Enemy behavior")]
        [SerializeField] private bool _alertOnStart = true;
        
        [FoldoutGroup("Current state"), ShowInInspector, ReadOnly] public bool Occupied;
        [FoldoutGroup("Current state"), ShowInInspector, ReadOnly]
        public float SpawnChance
        {
            get => _spawnChance * (!_applySpawnLimitOnlyToGuaranteedSpawns && _spawnLimit > -1 && _spawnCount >= _spawnLimit ? 0 : 1);
            set => _spawnChance = value;
        }
        private float _spawnChance = 1f;
        public bool AlertOnStart => _alertOnStart;
        public int GuaranteeSpawnRange => _guaranteeSpawnRange;
        
        [FoldoutGroup("Current state"), ShowInInspector, ReadOnly] public bool SpawnImmediate => ((_guaranteeSpawnIfInRange && (_spawnLimit <= -1 || _spawnLimit > _spawnCount)) 
                                                                   || (_persistSpawns && _previousSpawnWasDespawned));
        [FoldoutGroup("Current state"), ShowInInspector, ReadOnly] public float EnemySpawnWeight { get; set; } = 1f;

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            ResetSpawnProbability();
        }

        private void OnDisable()
        {
            if (parentNode != null)
            {
                parentNode.onPlayerVisited -= OnPlayerVisited;
            }
        }

        #endregion
        
        #region Public interface

        public void ResetSpawnProbability()
        {
            SpawnChance = _startingSpawnChance;

            // TODO calculate base on object parameters
        }

        public void RecordEnemySpawned(bool occupySpawnPoint)
        {
            _spawnCount++;
            Occupied = (occupySpawnPoint || _limitToOneSpawnAtATime);
        }

        public void RecordEnemyDespawned()
        {
            if (_persistSpawns)
            {
                _previousSpawnWasDespawned = true;
            }
            if (_persistSpawns || _spawnLimit > -1)
            {
                _spawnCount--;
            }
            Occupied = false;
        }

        public void RecordEnemyDied()
        {
            _previousSpawnWasDespawned = false;
            Occupied = false;
        }
        
        #endregion

        protected override void OnPlayerVisited(object o, EventArgs e)
        {
            if (_disableIfPlayerVisited)
            {
                SpawnChance = 0f;
            }
        }
    }
}