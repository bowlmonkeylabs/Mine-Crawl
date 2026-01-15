using BML.Scripts.Player;
using BML.Scripts.Player.Items;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using QFSW.QC;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts.QuantumConsoleExtensions
{
    [CommandPrefix("object.")]
    public class ObjectCommands : SerializedMonoBehaviour
    {
        [TitleGroup("Spawn object settings")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private LayerMask _raycastLayerMask;
        [SerializeField] private TransformSceneReference _itemContainer;
        [SerializeField] private Dictionary<string, GameObject> _spawnableObjects;

        [Command("list", "Lists all spawnable objects.")]
        private void ListSpawnableObjects()
        {
            var str = String.Join(", ", _spawnableObjects.Keys.ToArray().OrderBy(key => key));
            Debug.Log(str);
        }

        [Command("spawn", "Spawns an object where the player is looking.")]
        private void SpawnObject(string objectName)
        {
            if (!_spawnableObjects.TryGetValue(objectName, out GameObject prefab))
            {
                Debug.LogError($"Object with name '{objectName}' not found.");
                return;
            }

            _playerController.SpawnObjectInView(_raycastLayerMask, false, prefab, _itemContainer.Value);
        }
    }
}