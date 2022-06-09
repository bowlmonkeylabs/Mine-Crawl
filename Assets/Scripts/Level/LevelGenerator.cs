using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BML.Scripts.Level
{
    [ExecuteInEditMode]
    public class LevelGenerator : MonoBehaviour
    {
        #region Inspector
        
        [FoldoutGroup("Seed")]
        [SerializeField] private int _seed;
        [FoldoutGroup("Seed")]
        [SerializeField] private bool _lockSeed;
        
        [FoldoutGroup("Layout")]
        [PropertySpace(SpaceAfter = 10)]
        [FormerlySerializedAs("_roomLayout")]
        [SerializeField] private List<LevelRoom> _manualRoomLayout;
        [FoldoutGroup("Layout")]
        [FormerlySerializedAs("_roomPrefabs")]
        [SerializeField] private List<LevelRoom> _randomRoomPrefabs;

        #endregion
        
        private List<LevelRoom> _level;

        #region Unity lifecycle

        #if UNITY_EDITOR
        private void Awake()
        {
            var levels = GetComponentsInChildren<LevelRoom>();
            if (levels != null && levels.Length > 0)
            {
                _level = new List<LevelRoom>();
                foreach (var levelRoom in levels)
                {
                    _level.Add(levelRoom);
                }
            }
        }
        #endif

        #endregion

        [Button]
        private void Generate()
        {
            Destroy();

            if (!_lockSeed)
            {
                _seed = Random.Range(Int32.MinValue, Int32.MaxValue);
            }
            Random.InitState(_seed);

            if (_manualRoomLayout?.Count == 0)
            {
                return;
            }
            _level = new List<LevelRoom>();
            for (int i = 0; i < _manualRoomLayout.Count; i++)
            {
                if (_manualRoomLayout[i] == null)
                {
                    var randomIndex = Random.Range(0, _randomRoomPrefabs.Count);
                    var randomRoomPrefab = _randomRoomPrefabs[randomIndex];
                    AppendRoom(randomRoomPrefab);
                }
                else
                {
                    AppendRoom(_manualRoomLayout[i]);
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
            roomInstance.GetComponent<LevelRoom>().Generate();
            
            _level.Add(roomInstance);

            return roomInstance;
        }
    }
}