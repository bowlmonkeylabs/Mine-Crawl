using System;
using System.Linq;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.Utils;
using Mono.CSharp;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    [ExecuteAlways]
    public class LevelObjectSpawner : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private bool _generateOnLockMudBunMesh;
        [SerializeField] private MudBunGenerator _mudBunGenerator;
        
        [SerializeField, ReadOnly] private bool _generateOnChange;
        [SerializeField] private int _maxGeneratesPerSecond = 1;
        private float _generateMinCooldownSeconds => 1f / (float) _maxGeneratesPerSecond;
        
        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;
        
        [Required, SerializeField] private GameObject _player;

        [InlineEditor]
        [Required, SerializeField] private LevelObjectSpawnerParameters _levelObjectSpawnerParams;
        
        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _mudBunGenerator.OnAfterAddCollider += TrySpawnLevelObjects;
            
            _caveGenerator.OnAfterGenerate += TrySpawnLevelObjectsWithCooldown;
            _levelObjectSpawnerParams.OnValidateEvent += TrySpawnLevelObjectsWithCooldown;
        }

        private void OnDisable()
        {
            _mudBunGenerator.OnAfterAddCollider -= TrySpawnLevelObjects;
            
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

        [Button]
        public void SpawnLevelObjects()
        {
            DestroyLevelObjects();
            
            SpawnPlayer(_caveGenerator, this.transform, _player);
            SpawnObjectsAtTags(_levelObjectSpawnerParams, this.transform);
        }

        [Button]
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
                var tagged = GameObject.FindGameObjectsWithTag(spawnAtTagParameters.Tag);
                foreach (var go in tagged)
                {
                    var newGameObject = GameObjectUtils.SafeInstantiate(true, spawnAtTagParameters.Prefab, parent);
                    newGameObject.transform.position = GetPointUnder(go.transform.position, levelObjectSpawnerParameters.TerrainLayerMask, levelObjectSpawnerParameters.MaxRaycastLength);
                }
            }
        }

        private static void SpawnPlayer(CaveGenComponentV2 caveGenerator, Transform parent, GameObject player)
        {
            // Place player at the level start
            if (caveGenerator.CaveGraph.StartNode != null)
            {
                var startWorldPosition = caveGenerator.LocalToWorld(caveGenerator.CaveGraph.StartNode.LocalPosition);
                var playerInThisScene = (player.scene.handle != 0 && player.scene.handle == parent.gameObject.scene.handle);
                if (playerInThisScene)
                {
                    // Debug.Log($"Moved player to level start...");
                    player.transform.position = startWorldPosition;
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