using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.SpawnObjects;
using BML.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    public class EnemySpawnManager : MonoBehaviour
    {
        #region Inspector
        
        [TitleGroup("Scene References")]
        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;
        [SerializeField] private Transform _enemyContainer;
        [SerializeField] private Transform _player;
        [Required, SerializeField] private SphereCollider _enemyDeleteRadius;

        [TitleGroup("Spawning Parameters")]
        [SerializeField] private float _spawnOffsetRadius = 5f;
        [SerializeField] private FloatVariable _levelStartTime;
        [SerializeField] private FloatVariable _minutesToMaxSpawn;
        [SerializeField] private CurveVariable _spawnDelayCurve;
        [SerializeField] private CurveVariable _spawnCapCurve;
        [SerializeField] private FloatVariable _currentSpawnDelay;
        [SerializeField] private FloatVariable _currentSpawnCap;
        [SerializeField] private IntVariable _currentEnemyCount;
        [SerializeField] private FloatVariable _currentPercentToMaxSpawn;

        [UnityEngine.Tooltip(
            "Controls range within spawn points will be considered for active spawning. 'Player Distance' is defined by the CaveNodeData, in terms of graph distance from the player's current location.")]
        [SerializeField, MinMaxSlider(0, 10, true)]
        private Vector2Int _minMaxSpawnPlayerDistance = new Vector2Int(1, 3);
        
        [SerializeField] [Range(-1f, 1f), LabelText("Spawn in Unexplored")] private float _weightSpawningInUnexplored = 0f;
        [SerializeField] [Range(-1f, 1f), LabelText("Spawn Towards Objective")] private float _weightSpawningTowardsObjective = 0f;
        
        [UnityEngine.Tooltip("While exit challenge is active, the spawner will override the 'Min Max Player Spawn Distance' to 0, meaning enemies will only spawn in the same room as the player. (This works only because the exit challenge is constructed as a single 'room' in the level data.")]
        [SerializeField] private BoolReference _isExitChallengeActive;
        
        [TitleGroup("Despawning Parameters")]
        [Required, SerializeField] private LayerMask _enemyLayerMask;
        [Required, SerializeField] private float _despawnDelay = 1f;

        [TitleGroup("Enemy Types")]
        [Required, SerializeField] [InlineEditor()] private EnemySpawnerParams _enemySpawnerParams;
        
        [TitleGroup("Debug")]
        [SerializeField] private bool _enableLogs = false;

        #endregion

        [ReadOnly, ShowInInspector] private Dictionary<string, List<SpawnPoint>> _allSpawnPointsByTag;
        [ReadOnly, ShowInInspector] private Dictionary<string, List<SpawnPoint>> _activeSpawnPointsByTag;

        private float lastSpawnTime = Mathf.NegativeInfinity;
        private float lastDespawnTime = Mathf.NegativeInfinity;
        
        #region Unity lifecycle

        private void OnEnable()
        {
            _currentPercentToMaxSpawn.Value = 0;
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
            _currentPercentToMaxSpawn.Value += (Time.deltaTime) / (_minutesToMaxSpawn.Value * 60f);
            _currentSpawnDelay.Value = _spawnDelayCurve.Value.Evaluate(_currentPercentToMaxSpawn.Value);

            HandleDespawning();
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

            if (_enemyDeleteRadius != null)
            {
                var center = _enemyDeleteRadius.transform.position + _enemyDeleteRadius.center;
                var radius = _enemyDeleteRadius.radius;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(center, radius);
                Gizmos.DrawRay(center, _enemyDeleteRadius.transform.up * 30f);
                Gizmos.DrawSphere(center, 0.75f);
            }

            if (_enemyContainer != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                for (int i = 0; i < _enemyContainer.childCount; i++)
                {
                    var enemy = _enemyContainer.GetChild(i);
                    Gizmos.DrawSphere(enemy.position, 2f);
                    Gizmos.DrawRay(enemy.position, enemy.up * 30f);
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
            
            CalculateSpawnPointWeight();
        }

        private void CalculateSpawnPointWeight()
        {
            foreach (var caveNodeData in _caveGenerator.CaveGraph.Vertices)
            {
                float baseWeight = 1f;

                float weightModifierUnexplored = 0;
                float weightModifierObjective = 0;
                if (!_isExitChallengeActive.Value)
                {
                    weightModifierUnexplored = (caveNodeData.PlayerVisited ? -1f : 0f) 
                                               * _weightSpawningInUnexplored;
                    
                    int relativeObjectiveDirection = Math.Sign( _caveGenerator.CurrentMaxPlayerObjectiveDistance 
                                                                - caveNodeData.ObjectiveDistance );
                    relativeObjectiveDirection = Math.Max(-1, relativeObjectiveDirection - 1);
                    weightModifierObjective = (relativeObjectiveDirection * _weightSpawningTowardsObjective);
                }
                    
                float modifiedWeight = Mathf.Max(0f, baseWeight + weightModifierUnexplored + weightModifierObjective);

                caveNodeData.SpawnPoints.Where(spawnPoint =>
                    _activeSpawnPointsByTag.ContainsKey(spawnPoint.tag))
                    .ForEach(spawnPoint =>
                    {
                        spawnPoint.EnemySpawnWeight = modifiedWeight;
                    });
            }
        }
        
        #endregion
        
        #region Spawning

        public void DespawnAll()
        {
            for (int i = 0; i < _enemyContainer.childCount; i++)
            {
                var childEnemy = _enemyContainer.GetChild(i).GetComponent<EnemySpawnable>();
                if (childEnemy == null) continue;

                childEnemy.Despawn();
            }
        }
        
        private void HandleDespawning()
        {
            if (Time.time < lastDespawnTime + _despawnDelay)
                return;

            lastDespawnTime = Time.time;

            var center = _enemyDeleteRadius.transform.position + _enemyDeleteRadius.center;
            var radius = _enemyDeleteRadius.radius;
            var despawnablesInRange = Physics.OverlapSphere(center, radius, _enemyLayerMask)
                .Select(coll => coll.GetComponent<EnemySpawnable>())
                .Where(despawnable => despawnable != null)
                .ToList();
            
            if (_enableLogs) Debug.Log($"HandleDespawning Found {despawnablesInRange.Count}/{_enemyContainer.childCount} enemies in active range.");
            
            for (int i = 0; i < _enemyContainer.childCount; i++)
            {
                var childEnemy = _enemyContainer.GetChild(i).GetComponent<EnemySpawnable>();
                if (childEnemy == null) continue;

                bool foundInActive = despawnablesInRange.Contains(childEnemy);
                if (!foundInActive)
                {
                    childEnemy.Despawn();
                }
            }
        }

        private void HandleSpawning()
        {
            if (Time.time < lastSpawnTime + _currentSpawnDelay.Value)
                return;

            bool noActiveSpawnPoints = _activeSpawnPointsByTag == null
                                       || _activeSpawnPointsByTag.Count == 0
                                       || _activeSpawnPointsByTag.All(kv => kv.Value.Count == 0);
            if (noActiveSpawnPoints)
                return;

            // Check against current enemy cap
            _currentEnemyCount.Value = _enemyContainer.Cast<Transform>()
                .Select(child => child.GetComponent<EnemySpawnable>())
                .Count(d => d != null && d.DoCountTowardsSpawnCap && d.gameObject.activeSelf);
            _currentSpawnCap.Value = _spawnCapCurve.Value.Evaluate(_currentPercentToMaxSpawn.Value);
            _currentSpawnCap.Value = _spawnCapCurve.Value.Evaluate(_currentPercentToMaxSpawn.Value);
            if (_currentEnemyCount.Value >= _currentSpawnCap.Value) 
                return;

            EnemySpawnParams randomEnemy = _enemySpawnerParams.SpawnAtTags.GetRandomElement();
            List<SpawnPoint> potentialSpawnPointsForTag = _activeSpawnPointsByTag[randomEnemy.Tag]
                .Where(spawnPoint => spawnPoint.EnemySpawnWeight != 0)
                .ToList();
            
            // If no spawn points in range for this tag, return
            if (potentialSpawnPointsForTag.Count == 0)
            {
                Debug.LogWarning($"HandleSpawning No valid spawn points available.");
                return;
            }
            
            // Normalize spawn point weights
            var sumSpawnPointWeights = potentialSpawnPointsForTag.Sum(spawnPoint => spawnPoint.EnemySpawnWeight);
            List<RandomUtils.WeightPair<SpawnPoint>> spawnPointWeights = 
                potentialSpawnPointsForTag
                    .Select(spawnPoint =>
                    {
                        float spawnWeightNormalized = spawnPoint.EnemySpawnWeight / sumSpawnPointWeights;
                        spawnPoint.SpawnChance = spawnWeightNormalized;
                        return new RandomUtils.WeightPair<SpawnPoint>(spawnPoint, spawnWeightNormalized);
                    })
                    .ToList();
            
            // Choose weighted random spawn point
            SpawnPoint randomSpawnPoint = RandomUtils.RandomWithWeights(spawnPointWeights);
            
            
            var spawnOffset = Random.insideUnitCircle;
            var spawnPoint = randomSpawnPoint.transform.position +
                             new Vector3(spawnOffset.x, 0f, spawnOffset.y) * _spawnOffsetRadius;
            var newGameObject =
                GameObjectUtils.SafeInstantiate(randomEnemy.InstanceAsPrefab, randomEnemy.Prefab, _enemyContainer);
            var spawnPointOffset = newGameObject.transform.position -
                                   randomEnemy.RaycastDirection * randomEnemy.RaycastOffset;
            newGameObject.transform.position = SpawnObjectsUtil.GetPointTowards(spawnPointOffset,
                randomEnemy.RaycastDirection, _enemySpawnerParams.TerrainLayerMask,
                _enemySpawnerParams.MaxRaycastLength);
            
            if (_enableLogs) Debug.Log($"HandleSpawning Spawned {randomEnemy.Prefab.name} {spawnPoint}");
            _currentEnemyCount.Value = _enemyContainer.Cast<Transform>().Count(t => t.gameObject.activeSelf);
            
            lastSpawnTime = Time.time;
        }
        
        #endregion
        
    }
}