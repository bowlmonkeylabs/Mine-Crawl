using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
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
        
        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;
        
        [Required, SerializeField] private GameObject _player;

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
        
        private const int STEP_ID = 2;

        [Button, PropertyOrder(-1), LabelText("Spawn Level Objects")]
        //[EnableIf("$_isGenerationEnabled")]
        public void SpawnLevelObjectsButton()
        {
            SpawnLevelObjects();
        }
        
        public void SpawnLevelObjects(bool retryOnFailure = false)
        {
            // if (!_caveGenerator.IsGenerationEnabled) return;
            
            Random.InitState(_caveGenerator.CaveGenParams.Seed + STEP_ID);
            
            DestroyLevelObjects();
            
            if (_caveGenerator.EnableLogs) Debug.Log($"Level Object Spawner: Generate");

            SpawnPlayer(_caveGenerator, this.transform, _player);

            ResetSpawnPoints();
            CatalogSpawnPoints(_caveGenerator);
            SpawnObjectsAtTags(this.transform, retryOnFailure);
            
            _onAfterGenerateLevelObjects.Raise();
        }

        [Button, PropertyOrder(-1)]
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

        private void SpawnObjectsAtTags(Transform parent, bool retryOnFailure)
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
                    float mainPathDistanceFactor = (float) spawnPoint.ParentNode.MainPathDistance /
                                                   (float)_caveGenerator.MaxMainPathDistance;
                    float mainPathProbability = spawnAtTagParameters.MainPathProbabilityFalloff.Evaluate(mainPathDistanceFactor);

                    float spawnChance = (spawnPoint.SpawnChance * spawnAtTagParameters.SpawnProbability *
                                         mainPathProbability);
                    var rand = Random.value;
                    bool doSpawn = (rand < spawnChance);
                    if (_caveGenerator.EnableLogs) Debug.Log($"Try spawn {spawnAtTagParameters.Prefab?.name}: (Spawn point {spawnPoint.SpawnChance}) (Main path {mainPathProbability}) (Spawn chance {spawnChance}) (Random {rand}) (Do Spawn {doSpawn})");
                    if (doSpawn)
                    {
                        Vector3 spawnPos;

                        var hitStableSurface = SpawnObjectsUtil.GetPointTowards(
                            spawnPoint.transform.position, 
                            spawnAtTagParameters.RaycastDirection, 
                            out spawnPos,
                            _levelObjectSpawnerParams.TerrainLayerMask,
                            _levelObjectSpawnerParams.MaxRaycastLength);
                        
                        var spawnOffset = -spawnAtTagParameters.RaycastDirection * 
                                          spawnAtTagParameters.SpawnPosOffset;
                        
                        // Cancel spawn if did not find surface to spawn
                        if (spawnAtTagParameters.RequireStableSurface && !hitStableSurface)
                        {
                            if (_caveGenerator.EnableLogs)
                                Debug.Log($"Failed to spawn {spawnAtTagParameters.Prefab?.name}. No stable " +
                                          $"surface found!");
                            continue;
                        }

                        var newGameObject =
                            GameObjectUtils.SafeInstantiate(spawnAtTagParameters.InstanceAsPrefab, spawnAtTagParameters.Prefab, parent);
                        newGameObject.transform.SetPositionAndRotation(spawnPos + spawnOffset, spawnPoint.transform.rotation);
                            
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
                    _caveGenerator.RetryGenerateCaveGraph();
                    return;
                }
                
                //Debug.Log($"Remaining Spawn Points After: {tagged.Count - pointsToRemove.Count}");

            }
        }

        private static void SpawnPlayer(CaveGenComponentV2 caveGenerator, Transform parent, GameObject player)
        {
            if (caveGenerator.EnableLogs) Debug.Log($"Spawn player");
            
            // Place player at the level start
            if (caveGenerator.CaveGraph.StartNode != null)
            {
                var startWorldPosition = caveGenerator.LocalToWorld(caveGenerator.CaveGraph.StartNode.LocalPosition);
                var playerInThisScene = (player.scene.handle != 0 && player.scene.handle == parent.gameObject.scene.handle);
                // Debug.Log($"Call SpawnPlayer: Position {startWorldPosition} | Player in scene {playerInThisScene}");
                if (playerInThisScene)
                {
                    MovePlayer(player, startWorldPosition);
                }
                else
                {
#if UNITY_EDITOR
                    var isPrefab = PrefabUtility.IsPartOfPrefabAsset(player);
#else
                    var isPrefab = false;
#endif
                    player = GameObjectUtils.SafeInstantiate(isPrefab, player, parent);
                    MovePlayer(player, startWorldPosition);
                }
                
                // Stop velocity
                var rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                }
            }
        }

        private static void MovePlayer(GameObject player, Vector3 destination)
        {
            // If in play mode, move player using kinematicCharController motor to avoid race condition
            if (ApplicationUtils.IsPlaying_EditorSafe)
            {
                KinematicCharacterMotor motor = player.GetComponent<KinematicCharacterMotor>();
                if (motor != null)
                {
                    motor.SetPosition(destination);
                }
                else
                {
                    player.transform.position = destination;
                    Debug.LogWarning("Could not find KinematicCharacterMotor on player! " +
                                     "Moving player position directly via Transform.");
                }
            }
            else
            {
                player.transform.position = destination;
            }
        }
        
        #endregion

        #region Spawn objects utility

        

        #endregion
        
    }
}