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
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BML.Scripts
{
    public class EnemySpawnManager : MonoBehaviour
    {
        #region Inspector

        #if UNITY_EDITOR
        [FormerlySerializedAs("_isSpawningPaused")] [SerializeField] public bool IsSpawningPaused = false;
        [FormerlySerializedAs("_isDespawningPaused")] [SerializeField] public bool IsDespawningPaused = false;
        #else
        public bool IsSpawningPaused = false;
        public bool IsDespawningPaused = false;
        #endif
        
        [TitleGroup("Scene References")]
        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;
        [SerializeField] private Transform _enemyContainer;
        [SerializeField] private Transform _player;
        [Required, SerializeField] private SphereCollider _enemyDeleteRadius;

        [TitleGroup("Spawning Parameters")]
        [SerializeField] private LayerMask _terrainLayerMask;
        [Range(0f,100f)] private float _maxRaycastLength = 10f;
        [SerializeField] private float _spawnOffsetRadius = 5f;
        [SerializeField] private FloatVariable _currentSpawnDelay;
        [SerializeField] private FloatVariable _currentSpawnCap;
        [SerializeField] private IntVariable _currentEnemyCount;
        [SerializeField] private IntVariable _currentDifficulty;
        [SerializeField] private InfluenceStateData _playerInfluenceState;
        [Required, SerializeField] [InlineEditor()] private List<EnemySpawnerParams> _enemySpawnerParamList = new List<EnemySpawnerParams>();

        [UnityEngine.Tooltip(
            "Controls range within spawn points will be considered for active spawning. 'Player Distance' is defined by the CaveNodeData, in terms of graph distance from the player's current location.")]
        [SerializeField, MinMaxSlider(0, 10, true)]
        private Vector2Int _minMaxSpawnPlayerDistance = new Vector2Int(1, 3);
        
        [SerializeField] [Range(-1f, 1f), LabelText("Spawn in Unexplored")] private float _weightSpawningInUnexplored = 0f;
        [SerializeField] [Range(-1f, 1f), LabelText("Spawn Towards Objective")] private float _weightSpawningTowardsObjective = 0f;
        [SerializeField] [Range(-1f, 1f), LabelText("Spawn Ahead of Player")] private float _weightSpawningAheadOfPlayer = 0f;
        [SerializeField] [Range(-1f, 1f), LabelText("Spawn Outside of Player Influence")] private float _weightSpawningByPlayerInfluence = 0f;

        [UnityEngine.Tooltip("While exit challenge is active, the spawner will override the 'Min Max Player Spawn Distance' to 0, meaning enemies will only spawn in the same room as the player. (This works only because the exit challenge is constructed as a single 'room' in the level data.")]
        [SerializeField] private BoolReference _isExitChallengeActive;
        
        [TitleGroup("Despawning Parameters")]
        [Required, SerializeField] private LayerMask _enemyLayerMask;
        [Required, SerializeField] private float _despawnDelay = 1f;

        [TitleGroup("Debug")]
        [SerializeField] private bool _enableLogs = false;
        
        #endregion

        public List<EnemySpawnerParams> EnemySpawnerParamsList => _enemySpawnerParamList;

        [ReadOnly, ShowInInspector] private Dictionary<string, List<SpawnPoint>> _allSpawnPointsByTag;
        [ReadOnly, ShowInInspector] private Dictionary<string, List<SpawnPoint>> _activeSpawnPointsByTag;

        private EnemySpawnerParams currentParams;
        private float lastSpawnTime = Mathf.NegativeInfinity;
        private float lastDespawnTime = Mathf.NegativeInfinity;
        private int totalEnemySpawnCount;
        
        #region Unity lifecycle

        private void Awake()
        {
            InitSpawnCosts();
            totalEnemySpawnCount = 0;
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
            UpdateDifficultyParams();
            if (!IsDespawningPaused) HandleDespawning();
            if (!IsSpawningPaused) HandleSpawning();
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

        private int StepID = 4;

        [Button("Debug Init Spawn Costs")]
        private void InitSpawnCosts()
        {
            foreach (var enemyParams in _enemySpawnerParamList)
            {
                Random.InitState(_caveGenerator.CaveGenParams.Seed + StepID + totalEnemySpawnCount);
                int minCost = enemyParams.SpawnAtTags.Min(e => e.Cost);
                int maxCost = enemyParams.SpawnAtTags.Max(e => e.Cost);

                var intermediate = enemyParams.SpawnAtTags.Select(e => ((float) maxCost / e.Cost) * minCost).ToList();
                var total = intermediate.Sum();

                for (int i = 0; i < intermediate.Count; i++)
                {
                    enemyParams.SpawnAtTags[i].NormalizedSpawnWeight = intermediate[i] / (float) total;
                    Debug.Log($"{enemyParams.SpawnAtTags[i].Prefab.name} | " +
                              $"Cost: {enemyParams.SpawnAtTags[i].Cost}" +
                              $"| Norm: {enemyParams.SpawnAtTags[i].NormalizedSpawnWeight}");
                }
            }
        }

        private void InitSpawnPoints()
        {
            var tagsToSpawn = _enemySpawnerParamList
                .SelectMany(enemySpawnerParams => enemySpawnerParams.SpawnAtTags)
                .Select(spawnAtTag => spawnAtTag.Tag)
                .Distinct()
                .ToList();

            var tags = String.Join(", ", tagsToSpawn);
            // Debug.Log($"InitSpawnPoints {tags}");

            _allSpawnPointsByTag = _caveGenerator.CaveGraph.AllNodes
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

            return (inRangeOfPlayer)
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
            foreach (var caveNodeData in _caveGenerator.CaveGraph.AllNodes)
            {
                float baseWeight = 1f;

                float weightModifierUnexplored = 0;
                float weightModifierObjective = 0;
                float weightModifierAhead = 0;
                float weightModifierPlayerInfluence = 0;
                if (!_isExitChallengeActive.Value)
                {
                    weightModifierUnexplored = (caveNodeData.PlayerVisited ? -1f : 0f) 
                                               * _weightSpawningInUnexplored;                                   // [-1, 0]
                    
                    int relativeObjectiveDirection = Math.Sign( _caveGenerator.CurrentMaxPlayerObjectiveDistance 
                                                                - caveNodeData.ObjectiveDistance );             // [-1, 1]
                    relativeObjectiveDirection = Math.Max(-1, relativeObjectiveDirection - 1);                  // [-1, 0]
                    weightModifierObjective = (relativeObjectiveDirection * _weightSpawningTowardsObjective);   // [-1, 0]

                    float playerDistanceDelta = (Mathf.Sign(caveNodeData.PlayerDistanceDelta) + 1f) / -2f;
                    weightModifierAhead = (playerDistanceDelta * _weightSpawningAheadOfPlayer);                 // [-1, 0]

                    weightModifierPlayerInfluence = (0 - caveNodeData.PlayerInfluence) * _weightSpawningByPlayerInfluence;                         // [-1, 0]
                }
                    
                float modifiedWeight = Mathf.Clamp01(baseWeight + weightModifierUnexplored + weightModifierObjective + weightModifierAhead + weightModifierPlayerInfluence);

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

        private void UpdateDifficultyParams()
        {
            bool isPlayerInNode = _playerInfluenceState._currentNodes.Count > 0;
            bool isPlayerInNodeConnection = _playerInfluenceState._currentNodeConnections.Count > 0;
            
            // Dont update parameters if player is not within node or edge
            if (!isPlayerInNode && !isPlayerInNodeConnection)
                return;
            
            if (_enemySpawnerParamList.Count != _caveGenerator.DifficultySegmentCount)
            {
                Debug.LogError("Enemy spawn param list should have number of elements equal to the number" +
                               " of difficulty segments set on cave generator!");
                return;
            }
            
            // Aggregate the difficulty of nodes and edges player is currently occupying
            List<int> aggregateDifficulties = new List<int>();

            if (isPlayerInNode)
                aggregateDifficulties = aggregateDifficulties
                    .Union(_playerInfluenceState._currentNodes
                        .Select(n => n.Value.Difficulty)
                        ).ToList();
            
            if (isPlayerInNodeConnection)
                aggregateDifficulties = aggregateDifficulties
                    .Union(_playerInfluenceState._currentNodeConnections
                        .Select(n => n.Value.Difficulty)
                    ).ToList();

            float aggregateDifficultyFactor = (float) aggregateDifficulties.Average();

            _currentDifficulty.Value = Mathf.CeilToInt(aggregateDifficultyFactor);

            //Use calculated difficulty to decide which parameters to use
            currentParams = _enemySpawnerParamList[_currentDifficulty.Value];
            _currentSpawnDelay.Value = currentParams.SpawnDelay;
            _currentSpawnCap.Value = currentParams.SpawnCap;
        }

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
                .Count(d => d != null && d.DoCountForSpawnCap && d.gameObject.activeSelf);
            if (_currentEnemyCount.Value >= _currentSpawnCap.Value)
                return;
            
            //TODO: Here use the right spawn params based on difficulty of room enemy is being spawned in
            
            var weightPairs =  currentParams.SpawnAtTags.Select(e => 
                new RandomUtils.WeightPair<EnemySpawnParams>(e, e.NormalizedSpawnWeight)).ToList();

            Random.InitState(_caveGenerator.CaveGenParams.Seed + StepID + totalEnemySpawnCount);
            EnemySpawnParams randomEnemy = RandomUtils.RandomWithWeights(weightPairs);
            
            List<SpawnPoint> potentialSpawnPointsForTag = _activeSpawnPointsByTag[randomEnemy.Tag]
                .Where(spawnPoint => spawnPoint.EnemySpawnWeight != 0)
                .ToList();
            
            // If no spawn points in range for this tag, return
            if (potentialSpawnPointsForTag.Count == 0)
            {
                Debug.LogWarning($"HandleSpawning No valid spawn points available for enemy: {randomEnemy.Tag}");
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
            
            // Spawn chosen enemy at chosen spawn point
            var newEnemy = SpawnEnemy(randomSpawnPoint.transform.position, randomEnemy, true, _spawnOffsetRadius);
            totalEnemySpawnCount++;
            
            // Update last spawn time
            lastSpawnTime = Time.time;
        }
        
        private GameObject SpawnEnemy(Vector3 position, EnemySpawnParams enemy, bool doCountForSpawnCap, float randomOffsetRadius)
        {
            // Calculate random spawn position offset
            var randomOffset = Random.insideUnitCircle;
            var spawnPoint = position +
                             new Vector3(randomOffset.x, 0f, randomOffset.y) * randomOffsetRadius;
            
            // Instantiate new enemy game object
            var newGameObject =
                GameObjectUtils.SafeInstantiate(true, enemy.Prefab, _enemyContainer);

            // Raycast from spawn position to place new game object along the level surface
            Vector3 spawnPos;
            SpawnObjectsUtil.GetPointTowards(
                (spawnPoint - enemy.RaycastDirection * enemy.RaycastOffset),
                enemy.RaycastDirection,
                out spawnPos,
                _terrainLayerMask,
                _maxRaycastLength);
            newGameObject.transform.position = spawnPos;
            
            // Set parameters on enemy despawnable instance
            var enemySpawnable = newGameObject.GetComponent<EnemySpawnable>();
            if (enemySpawnable != null)
            {
                enemySpawnable.DoCountForSpawnCap = doCountForSpawnCap;
            }

            // Log and update enemy count
            if (_enableLogs) Debug.Log($"SpawnEnemy {enemy.Prefab.name} {spawnPoint}");
            _currentEnemyCount.Value = _enemyContainer.Cast<Transform>().Count(t => t.gameObject.activeSelf);

            return newGameObject;
        }

        public GameObject SpawnEnemyByName(EnemySpawnerParams enemySpawnParams, Vector3 position, string enemyName,
            bool doCountForSpawnCap, float randomOffsetRadius)
        {
            var enemy = enemySpawnParams.SpawnAtTags
                .FirstOrDefault(e => e.Prefab.name.StartsWith(enemyName, StringComparison.OrdinalIgnoreCase));
            if (enemy == null)
            {
                throw new ArgumentException($"EnemySpawnManager: '{enemyName}' Enemy not found in spawner list");
            }

            return SpawnEnemy(position, enemy, doCountForSpawnCap, randomOffsetRadius);
        }
        
        #endregion
        
    }
}