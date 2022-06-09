using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.Level
{
    public class LevelRoom : MonoBehaviour
    {
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _endPoint;
        [SerializeField] private BoxCollider _gridBounds;
        [SerializeField] private List<GameObject> _prefabs;
        [SerializeField] private Transform _objectsParent;
        [SerializeField] private Vector3 _gridOffset = new Vector3(0.5f, 0f, 0.5f);

        public Transform StartPoint => _startPoint;
        public Transform EndPoint => _endPoint;

        private GameObject[,,] _gameObjects;

        public void Generate()
        {
            Destroy();

            if (_gridBounds.SafeIsUnityNull())
            {
                return;
            }

            var min = _gridBounds.bounds.min;
            var max = _gridBounds.bounds.max;
            
            int minX = Mathf.CeilToInt(min.x);
            int maxX = Mathf.FloorToInt(max.x);
            int minY = Mathf.CeilToInt(min.y);
            int maxY = Mathf.FloorToInt(max.y);
            int minZ = Mathf.CeilToInt(min.z);
            int maxZ = Mathf.FloorToInt(max.z);
            
            // Debug.Log($"Generate room: {min} {max} {minX} {maxX} {minY} {maxY} {minZ} {maxZ}");

            _gameObjects = new GameObject[maxX-minX+1, maxY-minY+1, maxZ-minZ+1];
            
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int z = minZ; z < maxZ; z++)
                    {
                        var randomIndex = Random.Range(0, _prefabs.Count);
                        var randomPrefab = _prefabs[randomIndex];

                        GameObject instance = null;
                        if (!randomPrefab.SafeIsUnityNull())
                        {
                            var gridPosition = this.transform.position + new Vector3(x + _gridOffset.x, y + _gridOffset.y, z + _gridOffset.z);
                            var rotationAngle = Random.Range(0, 3) * 90;
                            var rotation = Quaternion.AngleAxis(rotationAngle, Vector3.up);
                            instance = GameObject.Instantiate(randomPrefab, gridPosition, rotation, _objectsParent);
                        }

                        _gameObjects[x - minX, y - minY, z - minZ] = instance;
                    }
                }
            }
                
        }

        public void Destroy()
        {
            if (_gameObjects == null) return;
            
            foreach (var o in _gameObjects)
            {
                GameObject.DestroyImmediate(o);
            }

            _gameObjects = null;
        }
        
        #region Unity lifecycle

        private void Awake()
        {
            var objectInstances = _objectsParent.GetComponentsInChildren<GameObject>();
            if (objectInstances != null)
            {
                foreach (var objectInstance in objectInstances)
                {
                    GameObject.Destroy(objectInstance);
                }
            }

            _gameObjects = null;
        }

        #endregion
    }
}