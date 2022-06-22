using System;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.Utils;
using Mono.CSharp;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace BML.Scripts.CaveV2
{
    [ExecuteAlways]
    public class LevelObjectSpawner : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private bool _generateOnChange;
        [SerializeField] private int _maxGeneratesPerSecond = 1;
        private float _generateMinCooldownSeconds => 1f / (float) _maxGeneratesPerSecond;
        
        [Required, SerializeField] private GameObject _player;
        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;
        
        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _caveGenerator.OnAfterGenerate += TrySpawnLevelObjects;
        }

        private void OnDisable()
        {
            _caveGenerator.OnAfterGenerate -= TrySpawnLevelObjects;
        }

        #endregion

        private void TrySpawnLevelObjects()
        {
            if (_generateOnChange)
            {
                SpawnLevelObjects();
            }
        }

        [Button]
        public void SpawnLevelObjects()
        {
            SpawnPlayer(this.transform);
            LevelObjectSpawner.SpawnLevelObjects(_caveGenerator.CaveGraph, this.transform, _player);
        }

        private static void SpawnLevelObjects(CaveGraphV2 caveGraph, Transform parent, GameObject player)
        {
            
        }

        private void SpawnPlayer(Transform parent)
        {
            // Place player at the level start
            if (_caveGenerator.CaveGraph.StartNode != null)
            {
                var startWorldPosition = _caveGenerator.LocalToWorld(_caveGenerator.CaveGraph.StartNode.LocalPosition);
                var playerInThisScene = (_player.scene.handle != 0 && _player.scene.handle == parent.gameObject.scene.handle);
                if (playerInThisScene)
                {
                    // Debug.Log($"Moved player to level start...");
                    _player.transform.position = startWorldPosition;
                }
                else
                {
                    var isPrefab = PrefabUtility.IsPartOfPrefabAsset(_player);
                    GameObject newGameObject = GameObjectUtils.SafeInstantiate(isPrefab, _player, parent);
                    newGameObject.transform.position = startWorldPosition;

                    _player = newGameObject;
                }
                
                // Stop velocity
                var rb = _player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                }
            }
        }
        
        
    }
}