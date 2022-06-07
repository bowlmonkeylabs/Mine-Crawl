using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.Level
{
    [ExecuteInEditMode]
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private List<LevelRoom> _roomPrefabs;
        [SerializeField] private List<LevelRoom> _roomLayout;
        [SerializeField] private int _seed;
        [SerializeField] private bool _lockSeed;

        private List<LevelRoom> _level;
        
        [Button]
        private void Generate()
        {
            Destroy();

            if (!_lockSeed)
            {
                _seed = Random.Range(Int32.MinValue, Int32.MaxValue);
            }
            Random.InitState(_seed);

            if (_roomLayout?.Count == 0)
            {
                return;
            }
            _level = new List<LevelRoom>();
            for (int i = 0; i < _roomLayout.Count; i++)
            {
                if (_roomLayout[i] == null)
                {
                    var randomIndex = Random.Range(0, _roomPrefabs.Count);
                    var randomRoomPrefab = _roomPrefabs[randomIndex];
                    AppendRoom(randomRoomPrefab);
                }
                else
                {
                    AppendRoom(_roomLayout[i]);
                }
            }
        }

        [Button]
        private void Destroy()
        {
            if (_level != null)
            {
                foreach (var _room in _level)
                {
                    if (_room.SafeIsUnityNull())
                    {
                        continue;
                    }
                    GameObject.DestroyImmediate(_room.gameObject);
                }

                _level.Clear();
            }
        }

        private LevelRoom AppendRoom(LevelRoom roomPrefab)
        {
            var appendPosition = _level.Count > 0
                ? _level.Last().EndPoint.position
                : this.transform.position;
            
            var roomInstance = GameObject.Instantiate(roomPrefab, this.transform);
            roomInstance.transform.position = appendPosition;
            
            _level.Add(roomInstance);

            return roomInstance;
        }
    }
}