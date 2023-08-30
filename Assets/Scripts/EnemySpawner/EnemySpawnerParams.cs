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
        public float MaxIntensity = 5;
        public float MinIntesity = 1;
        public List<EnemySpawnParams> SpawnAtTags;
    }
    
    [Serializable]
    public class EnemySpawnParams
    {
        public string Tag;
        public GameObject Prefab;
        public int Cost = 1;
        [FoldoutGroup("Parameters")] public Vector3 RaycastDirection = Vector3.down; // TODO remove deprecated
        [FoldoutGroup("Parameters")] public float RaycastOffset = 0f; // TODO remove deprecated
        [FoldoutGroup("Parameters")] public float SpawnPosOffset = 0f; // TODO remove deprecated
        [FoldoutGroup("Parameters")] public float SpawnRadiusOffset = .5f;
        [FoldoutGroup("Parameters")] public bool RequireStableSurface;
        [FoldoutGroup("Parameters")] public bool InstanceAsPrefab;
        [FoldoutGroup("Parameters")] public bool OccupySpawnPoint;
        [FoldoutGroup("Parameters")] public bool SpawnInPlayerVistedRooms = true; // TODO remove deprecated
        [ReadOnly] public float NormalizedSpawnWeight;
    }
}