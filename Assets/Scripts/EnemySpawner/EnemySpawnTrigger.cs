using System;
using BML.ScriptableObjectCore.Scripts.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    public class EnemySpawnTrigger : MonoBehaviour
    {
        [SerializeField] private DynamicGameEvent _onSpawnPointEnterSpawnTrigger;
        [SerializeField] private DynamicGameEvent _onSpawnPointExitSpawnTrigger;
        [Required, SerializeField] [InlineEditor()] private EnemySpawnerParams _enemySpawnerParams;

        private void OnTriggerEnter(Collider other)
        {
            string otherTag = other.gameObject.tag;
            foreach (var spawnAtTag in _enemySpawnerParams.SpawnAtTags)
            {
                if (spawnAtTag.Tag.Equals(otherTag))
                {
                    EnemySpawnInfo enemySpawnInfo = new EnemySpawnInfo()
                    {
                        spawnPoint = other.gameObject.transform,
                        spawnPointTag = other.gameObject.tag
                    };
                    _onSpawnPointEnterSpawnTrigger.Raise(enemySpawnInfo);
                    break;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            string otherTag = other.gameObject.tag;
            foreach (var spawnAtTag in _enemySpawnerParams.SpawnAtTags)
            {
                if (spawnAtTag.Tag.Equals(otherTag))
                {
                    EnemySpawnInfo enemySpawnInfo = new EnemySpawnInfo()
                    {
                        spawnPoint = other.gameObject.transform,
                        spawnPointTag = other.gameObject.tag
                    };
                    _onSpawnPointExitSpawnTrigger.Raise(enemySpawnInfo);
                    break;
                }
            }
        }
    }

    public class EnemySpawnInfo
    {
        public Transform spawnPoint;
        public string spawnPointTag;
    }
}