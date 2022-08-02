using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
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
        [SerializeField] private float _spawnOffsetRadius = 5f;
        [SerializeField] private FloatVariable _levelStartTime;
        [SerializeField] private FloatVariable _minutesToMaxSpawn;
        [SerializeField] private CurveVariable _spawnDelayCurve;
        [SerializeField] private CurveVariable _spawnCapCurve;
        [SerializeField] private FloatVariable _currentSpawnDelay;
        [SerializeField] private FloatVariable _currentSpawnCap;
        [SerializeField] private IntVariable _currentEnemyCount;
        [SerializeField] private DynamicGameEvent _onSpawnPointEnterSpawnTriggerInner;
        [SerializeField] private DynamicGameEvent _onSpawnPointExitSpawnTriggerInner;
        [SerializeField] private DynamicGameEvent _onSpawnPointEnterSpawnTriggerOuter;
        [SerializeField] private DynamicGameEvent _onSpawnPointExitSpawnTriggerOuter;
        [Required, SerializeField] [InlineEditor()] private EnemySpawnerParams _enemySpawnerParams;

        private Dictionary<string, List<Transform>> tagToSpawnPointsDict =
            new Dictionary<string, List<Transform>>();

        private float lastSpawnTime = Mathf.NegativeInfinity;
        private float percentToMaxSpawn;
        
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
            percentToMaxSpawn = (Time.time - _levelStartTime.Value) / (_minutesToMaxSpawn.Value * 60f);
            _currentSpawnDelay.Value = _spawnDelayCurve.Value.Evaluate(percentToMaxSpawn);

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
            if (lastSpawnTime + _currentSpawnDelay.Value > Time.time)
                return;

            //Check against current enemy cap
            _currentEnemyCount.Value = _enemyContainer.Cast<Transform>().Count(t => t.gameObject.activeSelf);

            _currentSpawnCap.Value = _spawnCapCurve.Value.Evaluate(percentToMaxSpawn);

            if (_currentEnemyCount.Value >= _currentSpawnCap.Value)
                return;
            

            EnemySpawnParams randomEnemy = _enemySpawnerParams.SpawnAtTags.GetRandomElement();
            List<Transform> potentialSpawnPointsForTag = tagToSpawnPointsDict[randomEnemy.Tag];

            //If no spawn points in range for this tag, return
            if (potentialSpawnPointsForTag.Count == 0)
                return;

            Transform randomSpawnPoint = potentialSpawnPointsForTag.GetRandomElement().transform;


            var spawnOffset = Random.insideUnitCircle;
            var spawnPoint = randomSpawnPoint.position +
                             new Vector3(spawnOffset.x, 0f, spawnOffset.y) * _spawnOffsetRadius;
            var newGameObject =
                GameObjectUtils.SafeInstantiate(randomEnemy.InstanceAsPrefab, randomEnemy.Prefab, _enemyContainer);
            newGameObject.transform.position = SpawnObjectsUtil.GetPointUnder(spawnPoint,
                _enemySpawnerParams.TerrainLayerMask,
                _enemySpawnerParams.MaxRaycastLength);
            
            lastSpawnTime = Time.time;
        }
    }
}