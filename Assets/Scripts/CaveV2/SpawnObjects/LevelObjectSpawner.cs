using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.Utils;
using KinematicCharacterController;
using Mono.CSharp;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    [ExecuteAlways]
    public class LevelObjectSpawner : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private bool _generateOnSeparateMudbunMesh;
        [SerializeField] private MudBunGenerator _mudBunGenerator;
        
        [InfoBox("DO NOT LEAVE ON IN PLAY MODE!", InfoMessageType.Error, "_generateOnChange")]
        [SerializeField] private bool _generateOnChange;
        [SerializeField] private int _maxGeneratesPerSecond = 1;
        private float _generateMinCooldownSeconds => 1f / (float) _maxGeneratesPerSecond;

        [SerializeField] private GameEvent _onAfterSeparateMudbunMesh;
        [SerializeField] private GameEvent _onAfterGenerateLevelObjects;
        [SerializeField] private BoolReference _levelObjectsGenerated;
        
        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;
        
        [Required, SerializeField] private TransformSceneReference _player;

        [InlineEditor]
        [Required, SerializeField] private LevelObjectSpawnerParameters _levelObjectSpawnerParams;
        
        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _onAfterSeparateMudbunMesh.Subscribe(TrySpawnLevelObjects);
            _levelObjectSpawnerParams.OnValidateEvent += TrySpawnLevelObjectsWithCooldown;
        }

        private void OnDisable()
        {
            _onAfterSeparateMudbunMesh.Unsubscribe(TrySpawnLevelObjects);
            _levelObjectSpawnerParams.OnValidateEvent -= TrySpawnLevelObjectsWithCooldown;
        }

        #endregion
        
        #region Generate level objects
        
        private bool _isGenerationEnabled => _caveGenerator.IsGenerationEnabled;
        
        private void TrySpawnLevelObjects()
        {
            if (_generateOnSeparateMudbunMesh)
            {
                this.SpawnLevelObjects(_caveGenerator.RetryOnFailure);
            }
        }

        private float lastGenerateTime;
        private void TrySpawnLevelObjectsWithCooldown()
        {
            if (_generateOnChange && !Application.isPlaying)
            {
                var elapsedTime = (Time.time - lastGenerateTime);
                if (elapsedTime >= _generateMinCooldownSeconds)
                {
                    lastGenerateTime = Time.time;
                    this.SpawnLevelObjects();
                }
                else
                {
                    DestroyLevelObjects();
                }
            }
        }

        [PropertyOrder(-1)]
        [ButtonGroup("Generation")]
        [Button("Generate")]
        //[EnableIf("$_isGenerationEnabled")]
        public void SpawnLevelObjectsButton()
        {
            SpawnLevelObjects();
        }
        
        public void SpawnLevelObjects(bool retryOnFailure = false)
        {
            // if (!_caveGenerator.IsGenerationEnabled) return;
            
            Random.InitState(SeedManager.Instance.GetSteppedSeed("LevelObjectSpawer"));
            
            DestroyLevelObjects();
            
            if (_caveGenerator.EnableLogs) Debug.Log($"Level Object Spawner: Generate");

            if (!_player.SafeIsUnityNull() && !_player.Value.SafeIsUnityNull())
            {
                SpawnPlayer(_caveGenerator, this.transform, _player.Value.gameObject);
            }
            else
            {
                if (_caveGenerator.EnableLogs) Debug.Log($"Level Object Spawner: No player assigned");
            }

            ResetSpawnPoints();
            CatalogSpawnPoints(_caveGenerator);
            bool success = SpawnObjectsAtTags(this.transform, retryOnFailure);
            if (!success)
            {
                if (_caveGenerator.EnableLogs)
                {
                    Debug.LogError("LevelObjectSpawner failed.");
                }
                _caveGenerator.RetryGenerateCaveGraph();
                return;
            }
            
            _onAfterGenerateLevelObjects.Raise();
            _levelObjectsGenerated.Value = true;
        }

        [PropertyOrder(-1)]
        [ButtonGroup("Generation")]
        [Button("Destroy")]
        //[EnableIf("$_isGenerationEnabled")]
        public void DestroyLevelObjects()
        {
            // if (!_caveGenerator.IsGenerationEnabled) return;
            if (_caveGenerator.EnableLogs) Debug.Log($"Level Object Spawner: Destroy");

            var children = Enumerable.Range(0, this.transform.childCount)
                .Select(i => this.transform.GetChild(i).gameObject)
                .ToList();
            foreach (var childObject in children)
            {
                GameObject.DestroyImmediate(childObject);
            }
        }
        
        private void ResetSpawnPoints()
        {
            var spawnPoints = FindObjectsOfType<SpawnPoint>();
            foreach (var spawnPoint in spawnPoints)
            {
                spawnPoint.ResetSpawnProbability();
            }
        }

        /// <summary>
        /// Catalog the owned spawn points for every cave node and connection in the graph.
        /// </summary>
        /// <param name="caveGenerator"></param>
        private void CatalogSpawnPoints(CaveGenComponentV2 caveGenerator)
        {
            foreach (var caveNodeData in caveGenerator.CaveGraph.Vertices)
            {
                if (caveNodeData.GameObject.SafeIsUnityNull()) continue;
                caveNodeData.SpawnPoints.Clear();

                var childSpawnPoints = caveNodeData.GameObject
                    .GetComponentsInChildren<SpawnPoint>()
                    .ToList();
                
                caveNodeData.SpawnPoints.AddRange(childSpawnPoints);
                foreach (var childSpawnPoint in childSpawnPoints)
                {
                    childSpawnPoint.ParentNode = caveNodeData;
                }
            }

            foreach (var caveNodeConnectionData in caveGenerator.CaveGraph.Edges)
            {
                if (caveNodeConnectionData.GameObject.SafeIsUnityNull()) continue;
                caveNodeConnectionData.SpawnPoints.Clear();

                var childSpawnPoints = caveNodeConnectionData.GameObject
                    .GetComponentsInChildren<SpawnPoint>()
                    .ToList();
                
                caveNodeConnectionData.SpawnPoints.AddRange(childSpawnPoints);
                foreach (var childSpawnPoint in childSpawnPoints)
                {
                    childSpawnPoint.ParentNode = caveNodeConnectionData;
                }
            }
        }

        /// <summary>
        /// Catalog the spawn points under a give transform and add them to the given cave node. Useful when checking for spawn points under newly added objects, for example.
        /// </summary>
        /// <param name="parent">The transform to search under. (Also includes the transform level in the search)</param>
        /// <param name="iCaveNodeData">The cave node the spawn points should belong to.</param>
        private void CatalogSpawnPoints(Transform parent, ICaveNodeData iCaveNodeData)
        {
            if (parent.SafeIsUnityNull()) return;

            var childSpawnPoints = parent
                .GetComponentsInChildren<SpawnPoint>()
                .ToList();
                
            iCaveNodeData.SpawnPoints.AddRange(childSpawnPoints);
            foreach (var childSpawnPoint in childSpawnPoints)
            {
                childSpawnPoint.ParentNode = iCaveNodeData;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="retryOnFailure"></param>
        /// <returns>False if spawning failed or the spawning parameters are not satisfied.</returns>
        private bool SpawnObjectsAtTags(Transform parent, bool retryOnFailure)
        {
            foreach (var spawnAtTagParameters in _levelObjectSpawnerParams.SpawnAtTags)
            {
                if (_caveGenerator.EnableLogs) Debug.Log($"Spawning {spawnAtTagParameters.Tag} {spawnAtTagParameters.Prefab.name}");

                int spawnCount = 0;

                var taggedSpawnPoints = GameObject.FindGameObjectsWithTag(spawnAtTagParameters.Tag)
                    .Select(go => go.GetComponent<SpawnPoint>())
                    .Where(go => go != null)
                    .OrderBy(s => Random.value)
                    .ToList();

                foreach (var spawnPoint in taggedSpawnPoints)
                {
                    float mainPathDistanceFactor = _caveGenerator.MaxMainPathDistance == 0 ? 0 
                        : (float) spawnPoint.ParentNode.MainPathDistance / (float)_caveGenerator.MaxMainPathDistance;
                    float mainPathProbability = spawnAtTagParameters.MainPathProbabilityFalloff.Evaluate(mainPathDistanceFactor);

                    float spawnChance = (spawnPoint.SpawnChance * spawnAtTagParameters.SpawnProbability *
                                         mainPathProbability);
                    var rand = Random.value;
                    bool doSpawn = (rand < spawnChance);
                    if (_caveGenerator.EnableLogs) Debug.Log($"Try spawn {spawnAtTagParameters.Prefab?.name}: (Spawn point {spawnPoint.SpawnChance}) (Main path {mainPathProbability}) (Spawn chance {spawnChance}) (Random {rand}) (Do Spawn {doSpawn})");
                    if (doSpawn)
                    {
                        var spawnAt = spawnPoint.Project(spawnAtTagParameters.SpawnPosOffset, SeedManager.Instance.Seed);
                        bool hitStableSurface = spawnAt.position.HasValue;
                        var cachedTransform = spawnPoint.transform;
                        spawnAt.position = spawnAt.position ?? cachedTransform.position;
                        spawnAt.rotation = spawnAt.rotation ?? cachedTransform.rotation;
                        
                        // Cancel spawn if did not find surface to spawn
                        if (spawnPoint.RequireStableSurface && !hitStableSurface)
                        {
                            if (_caveGenerator.EnableLogs)
                                Debug.LogWarning($"Failed to spawn {spawnAtTagParameters.Prefab?.name}. No stable " +
                                          $"surface found!");
                            continue;
                        }

                        var newGameObject =
                            GameObjectUtils.SafeInstantiate(spawnAtTagParameters.InstanceAsPrefab, spawnAtTagParameters.Prefab, parent);
                        newGameObject.transform.SetPositionAndRotation(spawnAt.position.Value, spawnAt.rotation.Value);
                        
                        var spawnedObjectCaveNodeData = newGameObject.GetComponent<SpawnedObjectCaveNodeData>();
                        if(spawnedObjectCaveNodeData != null) {
                            spawnedObjectCaveNodeData.CaveNode = spawnPoint.ParentNode;
                        }
                            
                        // Catalog spawn points again in case this newGameObject added more spawn points
                        CatalogSpawnPoints(newGameObject.transform, spawnPoint.ParentNode);

                        if (spawnAtTagParameters.ChooseWithoutReplacement)
                        {
                            spawnPoint.SpawnChance = 0f;
                        }
                        
                        spawnCount++;
                        if (spawnAtTagParameters.MinMaxGlobalAmount.EnableMax
                            && spawnCount >= spawnAtTagParameters.MinMaxGlobalAmount.ValueMax)
                        {
                            break;
                        }
                    }
                    
                }
                
                if (spawnAtTagParameters.MinMaxGlobalAmount.EnableMin
                    && spawnCount < spawnAtTagParameters.MinMaxGlobalAmount.ValueMin)
                {
                    if (_caveGenerator.EnableLogs) Debug.LogWarning($"Level Object Spawner: Minimum not met for object {spawnAtTagParameters.Prefab?.name} on tag {spawnAtTagParameters.Tag} ({spawnCount}/{spawnAtTagParameters.MinMaxGlobalAmount.ValueMin})");
                    return false;
                }
                
                //Debug.Log($"Remaining Spawn Points After: {tagged.Count - pointsToRemove.Count}");

            }

            return true;
        }

        private static void SpawnPlayer(CaveGenComponentV2 caveGenerator, Transform parent, GameObject player)
        {
            if (caveGenerator.EnableLogs) Debug.Log($"Spawn player");
            
            // Place player at the level start
            if (caveGenerator.CaveGraph.StartNode != null)
            {
                var playerController = player.GetComponent<IPlayerController>();
                if (playerController == null)
                {
                    if (caveGenerator.EnableLogs) Debug.LogError("Player controller not found.");
                    return;
                }
                
                var startWorldPosition = caveGenerator.LocalToWorld(caveGenerator.CaveGraph.StartNode.LocalPosition);
                // Debug.Log($"Call SpawnPlayer: Position {startWorldPosition} | Player in scene {playerInThisScene}");
                if (player.scene.isLoaded)
                {
                    playerController.SetPosition(startWorldPosition, true);
                }
                else
                {
#if UNITY_EDITOR
                    var isPrefab = PrefabUtility.IsPartOfPrefabAsset(player);
#else
                    var isPrefab = false;
#endif
                    player = GameObjectUtils.SafeInstantiate(isPrefab, player, parent);
                    playerController.SetPosition(startWorldPosition, true);
                }
                
                // Stop velocity
                var rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                }
            }
        }

        #endregion

        #region Spawn objects utility

        

        #endregion
        
    }
}