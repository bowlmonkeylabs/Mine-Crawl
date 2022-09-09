using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
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

        [SerializeField] private bool _generateOnLockMudBunMesh;
        [SerializeField] private MudBunGenerator _mudBunGenerator;
        
        [InfoBox("DO NOT LEAVE ON IN PLAY MODE!", InfoMessageType.Error, "_generateOnChange")]
        [SerializeField] private bool _generateOnChange;
        [SerializeField] private int _maxGeneratesPerSecond = 1;
        private float _generateMinCooldownSeconds => 1f / (float) _maxGeneratesPerSecond;

        [SerializeField] private GameEvent _onAfterGenerateEvent;
        
        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;
        
        [Required, SerializeField] private GameObject _player;

        [InlineEditor]
        [Required, SerializeField] private LevelObjectSpawnerParameters _levelObjectSpawnerParams;
        
        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _mudBunGenerator.OnAfterFinished += TrySpawnLevelObjects;
            _levelObjectSpawnerParams.OnValidateEvent += TrySpawnLevelObjectsWithCooldown;
        }

        private void OnDisable()
        {
            _mudBunGenerator.OnAfterFinished -= TrySpawnLevelObjects;
            _levelObjectSpawnerParams.OnValidateEvent -= TrySpawnLevelObjectsWithCooldown;
        }

        #endregion
        
        #region Generate level objects
        
        private bool _isGenerationEnabled => _caveGenerator.IsGenerationEnabled;
        
        private void TrySpawnLevelObjects()
        {
            if (_generateOnLockMudBunMesh)
            {
                this.SpawnLevelObjects();
            }
        }

        private float lastGenerateTime;
        private void TrySpawnLevelObjectsWithCooldown()
        {
            if (_generateOnChange && _caveGenerator.IsGenerationEnabled && !Application.isPlaying)
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

        [Button, PropertyOrder(-1), EnableIf("$_isGenerationEnabled")]
        public void SpawnLevelObjects()
        {
            // if (!_caveGenerator.IsGenerationEnabled) return;
            if (_caveGenerator.EnableLogs) Debug.Log($"Level Object Spawner: Generate");
            
            Random.InitState(_caveGenerator.CaveGenParams.Seed + STEP_ID);
            
            DestroyLevelObjects();
            
            if (_caveGenerator.EnableLogs) Debug.Log($"Level Object Spawner: Generate");

            SpawnPlayer(_caveGenerator, this.transform, _player);

            ResetSpawnPoints();
            CatalogSpawnPoints(_caveGenerator);
            SpawnObjectsAtTags(this.transform, retryOnFailure);
            
            _onAfterGenerateEvent.Raise();
        }

        [Button, PropertyOrder(-1), EnableIf("$_isGenerationEnabled")]
        public void DestroyLevelObjects()
        {
            if (!_caveGenerator.IsGenerationEnabled) return;
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
        }

        private void CatalogSpawnPoints(Transform parent, CaveNodeData caveNodeData)
        {
            if (parent.SafeIsUnityNull()) return;

            var childSpawnPoints = parent
                .GetComponentsInChildren<SpawnPoint>()
                .ToList();
                
            caveNodeData.SpawnPoints.AddRange(childSpawnPoints);
            foreach (var childSpawnPoint in childSpawnPoints)
            {
                childSpawnPoint.ParentNode = caveNodeData;
            }
        }

        private void SpawnObjectsAtTags(Transform parent, bool retryOnFailure)
        {
            foreach (var spawnAtTagParameters in _levelObjectSpawnerParams.SpawnAtTags)
            {
                //Debug.Log($"Spawning {spawnAtTagParameters.Tag} {spawnAtTagParameters.Prefab.name}");

                int spawnCount = 0;

                var taggedSpawnPoints = GameObject.FindGameObjectsWithTag(spawnAtTagParameters.Tag)
                    .Select(go => go.GetComponent<SpawnPoint>())
                    .Where(go => go != null)
                    .ToList();

                foreach (var spawnPoint in taggedSpawnPoints)
                {
                    bool doSpawn = (Random.value <= spawnPoint.SpawnChance * spawnAtTagParameters.SpawnProbability);
                    if (doSpawn)
                    {
                        var newGameObject =
                            GameObjectUtils.SafeInstantiate(spawnAtTagParameters.InstanceAsPrefab, spawnAtTagParameters.Prefab, parent);
                        newGameObject.transform.position = SpawnObjectsUtil.GetPointUnder(spawnPoint.transform.position,
                            _levelObjectSpawnerParams.TerrainLayerMask,
                            _levelObjectSpawnerParams.MaxRaycastLength);
                            
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
                    if (_caveGenerator.EnableLogs) Debug.LogWarning($"Level Object Spawner: Minimum ({spawnAtTagParameters.MinMaxGlobalAmount.ValueMin}) not met for {spawnAtTagParameters.Tag} > {spawnAtTagParameters.Prefab?.name}");
                    _caveGenerator.RetryGenerateCaveGraph();
                    return;
                }
                
                //Debug.Log($"Remaining Spawn Points After: {tagged.Count - pointsToRemove.Count}");

            }
        }

        private static void SpawnPlayer(CaveGenComponentV2 caveGenerator, Transform parent, GameObject player)
        {
            // Debug.Log($"Call SpawnPlayer");
            // Place player at the level start
            if (caveGenerator.CaveGraph.StartNode != null)
            {
                var startWorldPosition = caveGenerator.LocalToWorld(caveGenerator.CaveGraph.StartNode.LocalPosition);
                var playerInThisScene = (player.scene.handle != 0 && player.scene.handle == parent.gameObject.scene.handle);
                // Debug.Log($"Call SpawnPlayer: Position {startWorldPosition} | Player in scene {playerInThisScene}");
                if (playerInThisScene)
                {
                    // var prevPosition = player.transform.position;
                    KinematicCharacterMotor motor = player.GetComponent<KinematicCharacterMotor>();
                    if (motor != null)
                    {
                        motor.SetPosition(startWorldPosition);
                    }
                    else
                    {
                        player.transform.position = startWorldPosition;
                        Debug.LogWarning("Could not find KinematicCharacterMotor on player! " +
                                         "Moving player position directly via Transform.");
                    }
                    
                    // player.transform.SetPositionAndRotation(startWorldPosition, Quaternion.identity);
                    // Debug.Log($"Moved player to level start... (prev: {prevPosition}, curr: {player.transform.position})");
                }
                else
                {
#if UNITY_EDITOR
                    var isPrefab = PrefabUtility.IsPartOfPrefabAsset(player);
#else
                    var isPrefab = false;
#endif
                    GameObject newGameObject = GameObjectUtils.SafeInstantiate(isPrefab, player, parent);
                    newGameObject.transform.position = startWorldPosition;

                    player = newGameObject;
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