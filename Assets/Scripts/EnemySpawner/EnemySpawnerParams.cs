using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    [CreateAssetMenu(fileName = "EnemySpawnerParams", menuName = "BML/EnemySpawnerParams", order = 0)]
    public class EnemySpawnerParams : ScriptableObject
    {
        public float SpawnDelay = 5f;
        public float SpawnCap = 5f;
        public List<EnemySpawnParams> SpawnAtTags;
    }
    
    [Serializable]
    public class EnemySpawnParams
    {
        public string Tag;
        public GameObject Prefab;
        public bool InstanceAsPrefab;
        public int Cost = 1;
        public Vector3 RaycastDirection = Vector3.down;
        public float RaycastOffset = 0f;
        [ReadOnly] public float NormalizedSpawnWeight;
    }
}