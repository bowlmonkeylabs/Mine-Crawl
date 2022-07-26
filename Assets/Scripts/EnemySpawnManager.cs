﻿using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [SerializeField] private Transform _enemyContainer;
        [SerializeField] private Transform _player;
        [SerializeField] private float _spawnDelay = 1f;
        [SerializeField] private DynamicGameEvent _onSpawnPointEnterSpawnTriggerInner;
        [SerializeField] private DynamicGameEvent _onSpawnPointExitSpawnTriggerInner;
        [SerializeField] private DynamicGameEvent _onSpawnPointEnterSpawnTriggerOuter;
        [SerializeField] private DynamicGameEvent _onSpawnPointExitSpawnTriggerOuter;
        [Required, SerializeField] [InlineEditor()] private EnemySpawnerParams _enemySpawnerParams;

        private Dictionary<string, List<Transform>> tagToSpawnPointsDict =
            new Dictionary<string, List<Transform>>();

        private float lastSpawnTime = Mathf.NegativeInfinity;
        
        #region Unity lifecycle

        private void Awake()
        {
            InitSpawnPoints();
        }

        private void OnEnable()
        {
            _onSpawnPointEnterSpawnTriggerOuter.Subscribe(RegisterSpawnPoint);
            _onSpawnPointExitSpawnTriggerOuter.Subscribe(UnregisterSpawnPoint);
            _onSpawnPointEnterSpawnTriggerInner.Subscribe(UnregisterSpawnPoint);
            _onSpawnPointExitSpawnTriggerInner.Subscribe(RegisterSpawnPoint);
        }

        private void OnDisable()
        {
            _onSpawnPointEnterSpawnTriggerOuter.Unsubscribe(RegisterSpawnPoint);
            _onSpawnPointExitSpawnTriggerOuter.Unsubscribe(UnregisterSpawnPoint);
            _onSpawnPointEnterSpawnTriggerInner.Unsubscribe(UnregisterSpawnPoint);
            _onSpawnPointExitSpawnTriggerInner.Unsubscribe(RegisterSpawnPoint);
        }

        private void Update()
        {
            HandleSpawning();
        }

        #endregion

        private void InitSpawnPoints()
        {
            foreach (var spawnAtTag in _enemySpawnerParams.SpawnAtTags)
            {
                tagToSpawnPointsDict.Add(spawnAtTag.Tag, new List<Transform>());
            }
        }

        private void RegisterSpawnPoint(object prev, object enemySpawnInfoObj)
        {
            EnemySpawnInfo enemySpawnInfo = (EnemySpawnInfo) enemySpawnInfoObj;
            tagToSpawnPointsDict[enemySpawnInfo.spawnPointTag].Add(enemySpawnInfo.spawnPoint);
        }
        
        private void UnregisterSpawnPoint(object prev, object enemySpawnInfoObj)
        {
            EnemySpawnInfo enemySpawnInfo = (EnemySpawnInfo) enemySpawnInfoObj;
            tagToSpawnPointsDict[enemySpawnInfo.spawnPointTag].Remove(enemySpawnInfo.spawnPoint);
        }

        private void HandleSpawning()
        {
            if (lastSpawnTime + _spawnDelay > Time.time)
                return;

            EnemySpawnParams randomEnemy = _enemySpawnerParams.SpawnAtTags.GetRandomElement();
            Transform randomSpawnPoint = tagToSpawnPointsDict[randomEnemy.Tag].GetRandomElement().transform;
            
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