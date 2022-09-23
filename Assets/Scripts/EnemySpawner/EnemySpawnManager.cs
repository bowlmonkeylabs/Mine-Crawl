using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.SpawnObjects;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    public class EnemySpawnManager : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField, Required] private CaveGenComponentV2 _caveGenerator;
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
        
        [UnityEngine.Tooltip(
            "Controls range within spawn points will be considered for active spawning. 'Player Distance' is defined by the CaveNodeData, in terms of graph distance from the player's current location.")]
        [SerializeField, MinMaxSlider(0, 10, true)]
        private Vector2Int _minMaxSpawnPlayerDistance = new Vector2Int(1, 3);

        [UnityEngine.Tooltip("While exit challenge is active, the spawner will override the 'Min Max Player Spawn Distance' to 0, meaning enemies will only spawn in the same room as the player. (This works only because the exit challenge is constructed as a single 'room' in the level data.")]
        [SerializeField] private BoolReference _isExitChallengeActive;
        
        [Required, SerializeField] [InlineEditor()] private EnemySpawnerParams _enemySpawnerParams;

        #endregion

        private Dictionary<string, List<SpawnPoint>> _allSpawnPointsByTag;
        private Dictionary<string, List<SpawnPoint>> _activeSpawnPointsByTag;

        private float lastSpawnTime = Mathf.NegativeInfinity;
        private float percentToMaxSpawn;
        
        #region Unity lifecycle

        private void Awake()
        {
            // InitSpawnPoints();
        }

        private void OnEnable()
        {
            _caveGenerator.OnAfterGenerate += InitSpawnPoints;
            _caveGenerator.OnAfterUpdatePlayerDistance += CacheActiveSpawnPoints;
        }

        private void OnDisable()
        {
            _caveGenerator.OnAfterGenerate -= InitSpawnPoints;
            _caveGenerator.OnAfterUpdatePlayerDistance -= CacheActiveSpawnPoints;
        }

        private void Update()
        {
            percentToMaxSpawn = (Time.time - _levelStartTime.Value) / (_minutesToMaxSpawn.Value * 60f);
            _currentSpawnDelay.Value = _spawnDelayCurve.Value.Evaluate(percentToMaxSpawn);

            HandleSpawning();
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_allSpawnPointsByTag != null)
            {
                foreach (var tag in _allSpawnPointsByTag.Keys)
                {
                    foreach (var spawnPoint in _allSpawnPointsByTag[tag])
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawSphere(spawnPoint.transform.position, .5f);
                    }

                    if (_activeSpawnPointsByTag != null)
                    {
                        foreach (var spawnPoint in _activeSpawnPointsByTag[tag])
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawSphere(spawnPoint.transform.position, .5f);
                        }
                    }
                }
            }
        }

        #endregion
        
        #region Spawn points

        private void InitSpawnPoints()
        {
            var tagsToSpawn = _enemySpawnerParams.SpawnAtTags
                .Select(spawnAtTag => spawnAtTag.Tag)
                .Distinct()
                .ToList();

            var tags = String.Join(", ", tagsToSpawn);
            // Debug.Log($"InitSpawnPoints {tags}");

            _allSpawnPointsByTag = _caveGenerator.CaveGraph.Vertices
                .SelectMany(caveNodeData => caveNodeData.SpawnPoints)
                .GroupBy(sp => sp.gameObject.tag)
                .Where(group => tagsToSpawn.Contains(group.Key))
                .ToDictionary(group => group.Key, group => group.ToList());

            // Debug.Log($"{_allSpawnPointsByTag.Keys.Count} tags with spawn points");
            // foreach (var kv     in _allSpawnPointsByTag)
            // {
            //     Debug.Log($"{kv.Key}: {kv.Value.Count}");
            // }
            
            CacheActiveSpawnPoints();
        }
        
        private bool SpawnPointIsActive(SpawnPoint spawnPoint)
        {
            bool inRangeOfPlayer = (spawnPoint.ParentNode.PlayerDistance >= _minMaxSpawnPlayerDistance.x
                && spawnPoint.ParentNode.PlayerDistance <= _minMaxSpawnPlayerDistance.y);
            bool isExitChallengeActive = (_isExitChallengeActive.Value);
            bool isCurrentRoom = (spawnPoint.ParentNode.PlayerDistance == 0);

            return (!isExitChallengeActive && inRangeOfPlayer)
                   || (isExitChallengeActive && isCurrentRoom);
        }

        private void CacheActiveSpawnPoints()
        {
            _activeSpawnPointsByTag = new Dictionary<string, List<SpawnPoint>>();
            foreach (var kv in _allSpawnPointsByTag)
            {
                // Debug.Log($"CacheActiveSpawnPoints {kv.Key}");
                _activeSpawnPointsByTag[kv.Key] = kv.Value.Where(SpawnPointIsActive).ToList();
            }
        }
        
        #endregion
        
        #region Spawning

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
            List<SpawnPoint> potentialSpawnPointsForTag = _activeSpawnPointsByTag[randomEnemy.Tag];

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
        
        #endregion
        
    }
}