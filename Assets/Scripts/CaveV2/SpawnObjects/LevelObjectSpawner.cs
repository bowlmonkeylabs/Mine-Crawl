using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.Utils;
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
            
            _caveGenerator.OnAfterGenerate += TrySpawnLevelObjectsWithCooldown;
            _levelObjectSpawnerParams.OnValidateEvent += TrySpawnLevelObjectsWithCooldown;
        }

        private void OnDisable()
        {
            _mudBunGenerator.OnAfterFinished -= TrySpawnLevelObjects;
            
            _caveGenerator.OnAfterGenerate -= TrySpawnLevelObjectsWithCooldown;
            _levelObjectSpawnerParams.OnValidateEvent -= TrySpawnLevelObjectsWithCooldown;
        }

        #endregion
        
        #region Generate level objects
        
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
            if (_generateOnChange)
            {
                var elapsedTime = (Time.time - lastGenerateTime);
                if (elapsedTime >= _generateMinCooldownSeconds)
                {
                    lastGenerateTime = Time.time;
                    this.SpawnLevelObjects();
                }
            }
        }

        [Button, PropertyOrder(-1)]
        public void SpawnLevelObjects()
        {
            DestroyLevelObjects();

            SpawnPlayer(_caveGenerator, this.transform, _player);
            SpawnObjectsAtTags(_levelObjectSpawnerParams, this.transform);
            
            _onAfterGenerateEvent.Raise();
        }

        [Button, PropertyOrder(-1)]
        public void DestroyLevelObjects()
        {
            var children = Enumerable.Range(0, this.transform.childCount)
                .Select(i => this.transform.GetChild(i).gameObject)
                .ToList();
            foreach (var childObject in children)
            {
                GameObject.DestroyImmediate(childObject);
            }
        }

        private static void SpawnObjectsAtTags(LevelObjectSpawnerParameters levelObjectSpawnerParameters, Transform parent)
        {
            foreach (var spawnAtTagParameters in levelObjectSpawnerParameters.SpawnAtTags)
            {
                // Debug.Log($"Spawning {spawnAtTagParameters.Tag} {spawnAtTagParameters.Prefab.name}");
                
                var tagged = GameObject.FindGameObjectsWithTag(spawnAtTagParameters.Tag).ToList();
                foreach (var go in tagged)
                {
                    if (!go.SafeIsUnityNull())
                    {
                        bool doSpawn = (Random.value <= spawnAtTagParameters.SpawnProbability);
                        if (doSpawn)
                        {
                            var newGameObject =
                                GameObjectUtils.SafeInstantiate(spawnAtTagParameters.InstanceAsPrefab, spawnAtTagParameters.Prefab, parent);
                            newGameObject.transform.position = GetPointUnder(go.transform.position,
                                levelObjectSpawnerParameters.TerrainLayerMask,
                                levelObjectSpawnerParameters.MaxRaycastLength);

                            if (spawnAtTagParameters.DeleteTagAfterSpawn)
                            {
                                GameObject.DestroyImmediate(go, false);
                            }
                        }
                    }
                }
                
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
                    player.transform.position = startWorldPosition;
                    // player.transform.SetPositionAndRotation(startWorldPosition, Quaternion.identity);
                    // Debug.Log($"Moved player to level start... (prev: {prevPosition}, curr: {player.transform.position})");
                }
                else
                {
                    var isPrefab = PrefabUtility.IsPartOfPrefabAsset(player);
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

        private static Vector3 GetPointUnder(Vector3 position, LayerMask layerMask, float checkDistance)
        {
            var didHit = Physics.Raycast(new Ray(position, Vector3.down), out var hitInfo, checkDistance, layerMask);
            if (didHit)
            {
                return hitInfo.point;
            }
            return position;
        }

        #endregion
        
    }
}