using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [SerializeField] private Transform _enemyContainer;
        [SerializeField] private Transform _player;
        [SerializeField] private float _spawnDelay = 1f;
        [Required, SerializeField] [InlineEditor()] private EnemySpawnerParams _enemySpawnerParams;

        private Dictionary<EnemySpawnParams, List<GameObject>> enemySpawnPointDict =
            new Dictionary<EnemySpawnParams, List<GameObject>>();

        private float lastSpawnTime = Mathf.NegativeInfinity;
        
        #region Unity lifecycle

        private void Awake()
        {
            PopulateSpawnPoints();
        }

        private void Update()
        {
            HandleSpawning();
        }

        #endregion

        private void PopulateSpawnPoints()
        {
            foreach (var spawnAtTag in _enemySpawnerParams.SpawnAtTags)
            {
                var taggedSpawnPoints = GameObject.FindGameObjectsWithTag(spawnAtTag.Tag).ToList();
                enemySpawnPointDict.Add(spawnAtTag, taggedSpawnPoints);
            }
        }

        private void HandleSpawning()
        {
            if (lastSpawnTime + _spawnDelay > Time.time)
                return;

            EnemySpawnParams randomEnemy = _enemySpawnerParams.SpawnAtTags.GetRandomElement();
            Transform randomSpawnPoint = enemySpawnPointDict[randomEnemy].GetRandomElement().transform;
            
            var newGameObject =
                GameObjectUtils.SafeInstantiate(randomEnemy.InstanceAsPrefab, randomEnemy.Prefab, _enemyContainer);
            newGameObject.transform.position = SpawnObjectsUtil.GetPointUnder(randomSpawnPoint.position,
                _enemySpawnerParams.TerrainLayerMask,
                _enemySpawnerParams.MaxRaycastLength);
            
            Debug.Log($"Spawned: {newGameObject.name}");

            lastSpawnTime = Time.time;
        }
    }
}