using BML.Scripts.CaveV2.CaveGraph;
using Mono.CSharp;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace BML.Scripts.CaveV2
{
    public class LevelObjectSpawner : MonoBehaviour
    {
        [Required, SerializeField] private GameObject _player;
        [Required, SerializeField] private CaveGenComponentV2 _caveGenerator;

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
                    GameObject newGameObject;
#if UNITY_EDITOR
                    var isPrefab = PrefabUtility.IsPartOfPrefabAsset(_player);
                    if (isPrefab)
                    {
                        var newObject = PrefabUtility.InstantiatePrefab(_player, parent);
                        newGameObject = newObject as GameObject;
                        newGameObject.transform.position = startWorldPosition;
                    }
                    else
                    {
                        newGameObject = GameObject.Instantiate(_player, startWorldPosition, Quaternion.identity, parent);
                    }
#else
                    newGameObject = GameObject.Instantiate(player, startWorldPosition, Quaternion.identity, parent);
#endif

                    _player = newGameObject;
                }
            }
        }
    }
}