using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.CaveV2.SpawnObjects;
using BML.Scripts.Enemy;
using BML.Scripts.Utils;
using BML.Utils;
using BML.Utils.Random;
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
        [SerializeField] public bool IsSpawningPaused = false;
        [SerializeField] public bool IsDespawningPaused = false;
        #else
        public bool IsSpawningPaused = false;
        public bool IsDespawningPaused = false;
        #endif
        
        [TitleGroup("Scene References")]
        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;
        [SerializeField] private Transform _enemyContainer;

        [TitleGroup("Spawning Parameters")]
        [SerializeField, Range(1, 10)] private int _despawnNodeDistance = 5;
        [SerializeField] private BoolVariable _isSpawningPaused;
        [SerializeField] private IntVariable _currentEnemyCount;
        [SerializeField] private BoolVariable _hasPlayerExitedStartRoom;
        [SerializeField] private SafeFloatValueReference _intensityScore;
        [SerializeField] private DynamicGameEvent _onEnemyKilled;
        [SerializeField] private GameEvent _onAfterGenerateLevelObjects;
        [Required, SerializeField] [InlineEditor()] private EnemySpawnerParams _enemySpawnerParams;
        [SerializeField] private float _immediateSpawnDelay = 0.5f;
        private float _lastImmediateSpawnTime = 0f;

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

        public EnemySpawnerParams EnemySpawnerParams => _enemySpawnerParams;

        [ReadOnly, ShowInInspector] private Dictionary<string, List<EnemySpawnPoint>> _allSpawnPointsByTag;
        [ReadOnly, ShowInInspector] private Dictionary<string, List<EnemySpawnPoint>> _activeSpawnPointsByTag;

        private float lastSpawnTime = Mathf.NegativeInfinity;
        private float lastDespawnTime = Mathf.NegativeInfinity;
        
        #region Unity lifecycle

        private void Awake()
        {
            InitSpawnCosts();
        }

        private void OnEnable()
        {
            _caveGenerator.OnAfterUpdatePlayerDistance += CacheActiveSpawnPoints;
            _onEnemyKilled.Subscribe(OnEnemyKilled);
            
            _isSpawningPaused.Value = true;
            _hasPlayerExitedStartRoom.Subscribe(EnableSpawning);
            _onAfterGenerateLevelObjects.Subscribe(InitSpawnPoints);
        }

        private void OnDisable()
        {
            _caveGenerator.OnAfterUpdatePlayerDistance -= CacheActiveSpawnPoints;
            _onEnemyKilled.Unsubscribe(OnEnemyKilled);
            _hasPlayerExitedStartRoom.Unsubscribe(EnableSpawning);
            _onAfterGenerateLevelObjects.Unsubscribe(InitSpawnPoints);
        }

        private void Update()
        {
            if (!IsDespawningPaused) HandleDespawning();
            if (!IsSpawningPaused && !_isSpawningPaused.Value) HandleImmediateSpawning();
            if (!IsSpawningPaused && !_isSpawningPaused.Value) HandleSpawning();
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

        [Button("Debug Init Spawn Costs")]
        private void InitSpawnCosts()
        {
            if (_enableLogs) Debug.Log("EnemySpawner InitSpawnCosts");
            

                int minCost = _enemySpawnerParams.SpawnPointParams.Min(e => e.Cost);
                int maxCost = _enemySpawnerParams.SpawnPointParams.Max(e => e.Cost);

                var intermediate = _enemySpawnerParams.SpawnPointParams.Select(e => ((float) maxCost / e.Cost) * minCost).ToList();
                var total = intermediate.Sum();

                for (int i = 0; i < intermediate.Count; i++)
                {
                    _enemySpawnerParams.SpawnPointParams[i].NormalizedSpawnWeight = intermediate[i] / (float) total;
                    if (_enableLogs) Debug.Log(
                        $"EnemySpawner InitSpawnCosts {_enemySpawnerParams.SpawnPointParams[i].Prefab.name} | " +
                              $"(Cost {_enemySpawnerParams.SpawnPointParams[i].Cost})" +
                              $"(Norm {_enemySpawnerParams.SpawnPointParams[i].NormalizedSpawnWeight})");
                }
        }

        private void InitSpawnPoints()
        {
            var tagsToSpawn = _enemySpawnerParams.SpawnPointParams
                .SelectMany(spawnAtTag => spawnAtTag.Tags)
                .Distinct()
                .ToList();

            var tags = String.Join(", ", tagsToSpawn);
            if (_enableLogs) Debug.Log($"EnemySpawner InitSpawnPoints {tags}");

            _allSpawnPointsByTag = _caveGenerator.CaveGraph.AllNodes
                .SelectMany(caveNodeData => caveNodeData.EnemySpawnPoints)
                .GroupBy(sp => sp.gameObject.tag)
                .Where(group => tagsToSpawn.Contains(group.Key))
                .ToDictionary(group => group.Key, group => group.ToList());

            if (_enableLogs)
            {
                Debug.Log($"EnemySpawner InitSpawnPoints {_allSpawnPointsByTag.Keys.Count} tags with spawn points");
                foreach (var kv in _allSpawnPointsByTag)
                {
                    Debug.Log($"EnemySpawner InitSpawnPoints ({kv.Key} {kv.Value.Count})");
                }
            }
            
            CacheActiveSpawnPoints();
        }
        
        private bool SpawnPointIsActive(EnemySpawnPoint spawnPoint)
        {
            if(!spawnPoint.gameObject.activeSelf) {
                return false;
            }
            
            bool inRangeOfPlayer = (spawnPoint.ParentNode.PlayerDistance >= _minMaxSpawnPlayerDistance.x
                && spawnPoint.ParentNode.PlayerDistance <= _minMaxSpawnPlayerDistance.y
                && !spawnPoint.ParentNode.PlayerOccupiedAdjacentNodeOrConnection);
            // bool spawnImmediate = (spawnPoint.SpawnImmediate && spawnPoint.ParentNode.PlayerDistance <= spawnPoint.GuaranteeSpawnRange);
            bool isExitChallengeActive = (_isExitChallengeActive.Value);
            bool isCurrentRoom = (spawnPoint.ParentNode.PlayerDistance == 0);

            return (spawnPoint.SpawnImmediate ? spawnPoint.ParentNode.PlayerDistance <= spawnPoint.GuaranteeSpawnRange : inRangeOfPlayer)
                   || (isExitChallengeActive && isCurrentRoom);
        }

        private void CacheActiveSpawnPoints()
        {
            _activeSpawnPointsByTag = new Dictionary<string, List<EnemySpawnPoint>>();
            foreach (var kv in _allSpawnPointsByTag)
            {
                // Debug.Log($"CacheActiveSpawnPoints {kv.Key}");
                _activeSpawnPointsByTag[kv.Key] = kv.Value.Where(SpawnPointIsActive).ToList();
            }
            
            if (_enableLogs)
            {
                Debug.Log($"EnemySpawner CacheActiveSpawnPoints {_activeSpawnPointsByTag.Keys.Count} tags with spawn points");
                foreach (var kv in _activeSpawnPointsByTag)
                {
                    Debug.Log($"EnemySpawner CacheActiveSpawnPoints ({kv.Key} {kv.Value.Count})");
                }
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

                    if (_enableLogs)
                        Debug.Log($"baseWeight({baseWeight}) + weightModifierUnexplored({weightModifierUnexplored}) + weightModifierObjective({weightModifierObjective}) + weightModifierAhead({weightModifierAhead}) + weightModifierPlayerInfluence({weightModifierPlayerInfluence})");
                }
                    
                float modifiedWeight = Mathf.Clamp01(baseWeight + weightModifierUnexplored + weightModifierObjective + weightModifierAhead + weightModifierPlayerInfluence);

                caveNodeData.EnemySpawnPoints.Where(spawnPoint =>
                    _activeSpawnPointsByTag.ContainsKey(spawnPoint.tag))
                        .ForEach(spawnPoint =>
                        {
                            spawnPoint.EnemySpawnWeight = modifiedWeight;
                        });
            }
        }
        
        #endregion
        
        #region Spawning

        private void EnableSpawning()
        {
            if (_hasPlayerExitedStartRoom.Value)
            {
                _isSpawningPaused.Value = false;
            }
            
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

            int despawnCount = 0;
            for (int i = 0; i < _enemyContainer.childCount; i++)
            {
                var childEnemyState = _enemyContainer.GetChild(i).GetComponent<EnemyState>();
                if (childEnemyState == null) continue;
                
                if (childEnemyState.DistanceToPlayer < _despawnNodeDistance) continue;

                var childEnemySpawnable = _enemyContainer.GetChild(i).GetComponent<EnemySpawnable>();
                if (childEnemySpawnable == null) continue;
                
                if (_enableLogs)
                    Debug.Log(
                        $"HandleDespawning Despawning {childEnemySpawnable.name} {childEnemySpawnable.gameObject.GetInstanceID()}" +
                        $"with node distance from player: {childEnemyState.DistanceToPlayer}.");
                
                childEnemySpawnable.Despawn();
                despawnCount++;
            }

            // Subtracting directly instead of calling UpdateEnemyCount because it looks like
            // child count of enemy container is not update until next tick so that function
            // doesn't properly update enemy count
            _currentEnemyCount.Value -= despawnCount;
        }

        private void HandleSpawning()
        {
            float currentSpawnDelay = (_intensityScore.Value <= _enemySpawnerParams.LowIntensity)
                ? _enemySpawnerParams.SpawnDelayLowIntensity
                : _enemySpawnerParams.SpawnDelay;
            
            if (Time.time < lastSpawnTime + currentSpawnDelay)
            {
                return;
            }

            bool noActiveSpawnPoints = _activeSpawnPointsByTag == null
                                       || _activeSpawnPointsByTag.Count == 0
                                       || _activeSpawnPointsByTag.All(kv => kv.Value.Count == 0);
            if (noActiveSpawnPoints)
            {
                if (_enableLogs) Debug.Log($"HandleSpawning No active spawn points");
                return;
            }

            // Check against current enemy cap
            bool reachedGlobalSpawnCap = _currentEnemyCount.Value >= _enemySpawnerParams.SpawnCap;
            bool anySpawnPointsIgnoreGlobalCap =
                _activeSpawnPointsByTag.Any(kv => kv.Value.Any(spawnPoint => spawnPoint.IgnoreGlobalSpawnCap));
            UpdateEnemyCount();
            if (reachedGlobalSpawnCap && !anySpawnPointsIgnoreGlobalCap)
            {
                if (_enableLogs) Debug.Log($"HandleSpawning Spawn cap full");
                return;
            }
            
            //TODO: Here use the right spawn params based on difficulty of room enemy is being spawned in
            
            var weightPairs =  _enemySpawnerParams.SpawnPointParams.Select(e => 
                new WeightedValueEntry<EnemySpawnParams>(e, e.NormalizedSpawnWeight)).ToList();

            var currentUniqueSeedForContext = SeedManager.Instance.GetSteppedSeed("EnemySpawnerCount") + SeedManager.Instance.GetSteppedSeed("EnemySpawnerRetry");
            Random.InitState(currentUniqueSeedForContext);
            SeedManager.Instance.UpdateSteppedSeed("EnemySpawnerRetry");

            EnemySpawnParams randomEnemyParams = RandomUtils.RandomWithWeights(weightPairs);

            List<EnemySpawnPoint> potentialSpawnPointsForTag = randomEnemyParams.Tags.SelectMany(tag => _activeSpawnPointsByTag.ContainsKey(tag) ? _activeSpawnPointsByTag[tag] : new List<EnemySpawnPoint>())
                .Where(spawnPoint => spawnPoint.EnemySpawnWeight != 0 && !spawnPoint.Occupied
                    && (!reachedGlobalSpawnCap || spawnPoint.IgnoreGlobalSpawnCap)
                    && !spawnPoint.SpawnImmediate
                ).ToList();
            
            // If no spawn points in range for this tag, return
            if (potentialSpawnPointsForTag.Count == 0)
            {
                if (_enableLogs) Debug.LogWarning($"HandleSpawning No valid spawn points available for enemy: {String.Join(", ", randomEnemyParams.Tags)}");
                return;
            }
            
            // Normalize spawn point weights
            var sumSpawnPointWeights = potentialSpawnPointsForTag.Sum(spawnPoint => spawnPoint.EnemySpawnWeight);
            List<WeightedValueEntry<EnemySpawnPoint>> spawnPointWeights = 
                potentialSpawnPointsForTag
                    .Select(spawnPoint =>
                    {
                        float spawnWeightNormalized = spawnPoint.EnemySpawnWeight / sumSpawnPointWeights;
                        spawnPoint.SpawnChance = spawnWeightNormalized;
                        return new WeightedValueEntry<EnemySpawnPoint>(spawnPoint, spawnWeightNormalized);
                    })
                    .ToList();
            
            // Choose weighted random spawn point
            EnemySpawnPoint randomSpawnPoint = RandomUtils.RandomWithWeights(spawnPointWeights);

            // Calculate random spawn position offset
            // TODO refactor to fixed offset?
            var randomOffset = Random.insideUnitCircle;
            var spawnPosition = randomSpawnPoint.transform.position +
                             new Vector3(randomOffset.x, 0f, randomOffset.y) * randomEnemyParams.SpawnRadiusOffset;
            
            // Raycast from spawn position to place new game object along the level surface
            var spawnPos = randomSpawnPoint.Project(randomEnemyParams.SpawnPosOffset, currentUniqueSeedForContext);
            bool hitStableSurface = (spawnPos.position != null);

            // Cancel spawn if did not find surface to spawn
            if (randomEnemyParams.RequireStableSurface && !hitStableSurface)
            {
                if (_enableLogs)
                    Debug.LogWarning($"Failed to spawn {randomEnemyParams.Prefab?.name}. No stable " +
                              $"surface found!");
                return;
            }

            // Spawn chosen enemy at chosen spawn point
            var newEnemy = SpawnEnemy((Vector3)spawnPos.position, (Quaternion)spawnPos.rotation, randomEnemyParams, randomSpawnPoint, randomSpawnPoint.ParentNode,
                !randomSpawnPoint.IgnoreGlobalSpawnCap);

            EnemyState enemyState = newEnemy.GetComponent<EnemyState>();
            if (enemyState != null)
                enemyState.AlertOnStart = randomSpawnPoint.AlertOnStart;
            else
                Debug.LogWarning("Failed to find EnemyState script on spawned enemy!");
            
            SeedManager.Instance.UpdateSteppedSeed("EnemySpawnerCount");
            SeedManager.Instance.UpdateSteppedSeed("EnemySpawnerRetry", SeedManager.Instance.Seed);
            lastSpawnTime = Time.time;
        }

        private void HandleImmediateSpawning()
        {
            if (Time.time < _lastImmediateSpawnTime + _immediateSpawnDelay)
                return;
            
            bool anyToSpawnImmediate = _activeSpawnPointsByTag.Any(kv => kv.Value
                .Any(spawnPoint => spawnPoint.SpawnImmediate && !spawnPoint.Occupied && spawnPoint.SpawnChance > 0));
            if (anyToSpawnImmediate)
            {
                var currentUniqueSeedForContext = SeedManager.Instance.GetSteppedSeed("EnemySpawnerImmediateCount") +
                                                  SeedManager.Instance.GetSteppedSeed("EnemySpawnerImmediateRetry"); 
                Random.InitState(currentUniqueSeedForContext);
                SeedManager.Instance.UpdateSteppedSeed("EnemySpawnerImmediateRetry");
                
                foreach (var kv in _activeSpawnPointsByTag)
                {
                    var enemyOptionsForTag = _enemySpawnerParams.SpawnPointParams
                        .Where(enemySpawnerParams => enemySpawnerParams.Tags.Contains(kv.Key))
                        .ToList();
                    if (!enemyOptionsForTag.Any())
                    {
                        // TODO handle error
                        continue;
                    }
                    
                    var immediateSpawnPoints = _activeSpawnPointsByTag[kv.Key]
                        .Where(spawnPoint => spawnPoint.SpawnImmediate && !spawnPoint.Occupied && spawnPoint.SpawnChance > 0)
                        .OrderBy(spawnPoint => spawnPoint.EnemySpawnWeight);

                    foreach (var spawnPoint in immediateSpawnPoints)
                    {            
                        bool reachedGlobalSpawnCap = _currentEnemyCount.Value >= _enemySpawnerParams.SpawnCap;
                        if (reachedGlobalSpawnCap && !spawnPoint.IgnoreGlobalSpawnCap)
                        {
                            continue;
                        }
                        
                        // TODO weight the choice more logically
                        var randomEnemyIndex = Random.Range(0, enemyOptionsForTag.Count - 1);
                        var enemySpawnParams = enemyOptionsForTag[randomEnemyIndex];
                        
                        // Calculate random spawn position offset
                        var randomOffset = Random.insideUnitCircle;
                        var spawnPosition = spawnPoint.transform.position +
                                            new Vector3(randomOffset.x, 0f, randomOffset.y) * enemySpawnParams.SpawnRadiusOffset;
                
                        // Raycast from spawn position to place new game object along the level surface
                        var spawnPos = spawnPoint.Project(enemySpawnParams.SpawnPosOffset, currentUniqueSeedForContext);
                        bool hitStableSurface = (spawnPos.position != null);
                
                        // Cancel spawn if did not find surface to spawn
                        if (enemySpawnParams.RequireStableSurface && !hitStableSurface)
                        {
                            if (_enableLogs)
                                Debug.LogWarning($"Failed to spawn {enemySpawnParams.Prefab?.name}. No stable " +
                                                 $"surface found!");
                            continue;
                        }

                        // Spawn chosen enemy at chosen spawn point
                        var newEnemy = SpawnEnemy((Vector3)spawnPos.position, (Quaternion)spawnPos.rotation, enemySpawnParams, spawnPoint, spawnPoint.ParentNode,
                            !spawnPoint.IgnoreGlobalSpawnCap);

                        if (!spawnPoint.IgnoreGlobalSpawnCap)
                        {
                            UpdateEnemyCount();
                        }
                    }
                    
                }
                
                SeedManager.Instance.UpdateSteppedSeed("EnemySpawnerImmediateCount");
                SeedManager.Instance.UpdateSteppedSeed("EnemySpawnerImmediateRetry", SeedManager.Instance.Seed);
                _lastImmediateSpawnTime = Time.time;
            }
        }
        
        private GameObject SpawnEnemy(Vector3 position, Quaternion rotation, EnemySpawnParams enemy, EnemySpawnPoint spawnPoint,
            ICaveNodeData caveNodeData, bool doCountForSpawnCap)
        {
            // Instantiate new enemy game object
            var newGameObject = GameObjectUtils.SafeInstantiate(true, enemy.Prefab, _enemyContainer);
            newGameObject.transform.SetPositionAndRotation(position, rotation);

            if (spawnPoint != null)
            {
                spawnPoint.RecordEnemySpawned(enemy.OccupySpawnPoint);
            }
            
            // Set parameters on enemy despawnable instance
            var enemySpawnable = newGameObject.GetComponent<EnemySpawnable>();
            if (enemySpawnable != null)
            {
                enemySpawnable.DoCountForSpawnCap = doCountForSpawnCap;
                enemySpawnable.SpawnPoint = spawnPoint;
            }

            var spawnedObjectCaveNodeData = newGameObject.GetComponent<SpawnedObjectCaveNodeData>();
            if(spawnedObjectCaveNodeData != null) {
                spawnedObjectCaveNodeData.CaveNode = spawnPoint.ParentNode;
            }

            // Log and update enemy count
            if (_enableLogs) Debug.Log($"SpawnEnemy {enemy.Prefab.name} {position}");
            _currentEnemyCount.Value = _enemyContainer.Cast<Transform>().Count(t => t.gameObject.activeSelf);

            return newGameObject;
        }

        public GameObject SpawnEnemyByName(EnemySpawnerParams enemySpawnParams, Vector3 position, string enemyName,
            bool doCountForSpawnCap, float randomOffsetRadius)
        {
            var enemy = enemySpawnParams.SpawnPointParams
                .FirstOrDefault(e => e.Prefab.name.StartsWith(enemyName, StringComparison.OrdinalIgnoreCase));
            if (enemy == null)
            {
                throw new ArgumentException($"EnemySpawnManager: '{enemyName}' Enemy not found in spawner list");
            }

            return SpawnEnemy(position, Quaternion.identity, enemy, null, null, doCountForSpawnCap);
        }

        private void OnEnemyKilled(object prevValue, object currValue)
        {
            UpdateEnemyCount();
        }

        private void UpdateEnemyCount()
        {
            _currentEnemyCount.Value = _enemyContainer.Cast<Transform>()
                .Select(child => child.GetComponent<EnemySpawnable>())
                .Count(d => d != null && d.DoCountForSpawnCap && d.gameObject.activeSelf);
        }

        #endregion

        #region Interface

        public void DespawnEnemies(EnemyState.AggroState aggroState, int nodeDist)
        {
            int despawnCount = 0;
            for (int i = 0; i < _enemyContainer.childCount; i++)
            {
                var childEnemyState = _enemyContainer.GetChild(i).GetComponent<EnemyState>();
                if (childEnemyState == null) continue;
            
                if (childEnemyState.DistanceToPlayer < nodeDist) continue;
                if (childEnemyState.Aggro != aggroState) continue;

                var childEnemySpawnable = _enemyContainer.GetChild(i).GetComponent<EnemySpawnable>();
                if (childEnemySpawnable == null) continue;
            
                if (_enableLogs)
                    Debug.Log(
                        $"DespawnEnemies Despawning {childEnemySpawnable.name} " +
                        $"with node distance from player: {childEnemyState.DistanceToPlayer} " +
                        $"and aggro state {childEnemyState.Aggro}");
            
                childEnemySpawnable.Despawn();
                despawnCount++;
            }

            // Subtracting directly instead of calling UpdateEnemyCount because it looks like
            // child count of enemy container is not update until next tick so that function
            // doesn't properly update enemy count
            _currentEnemyCount.Value -= despawnCount;
        
        }

        #endregion

    }
}