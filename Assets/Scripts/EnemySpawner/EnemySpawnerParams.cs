using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    [CreateAssetMenu(fileName = "EnemySpawnerParams", menuName = "BML/EnemySpawnerParams", order = 0)]
    public class EnemySpawnerParams : ScriptableObject
    {
        public List<EnemySpawnParams> SpawnAtTags;
        public LayerMask TerrainLayerMask;
        
        [Range(0f,100f)]
        public float MaxRaycastLength = 10f;
    }
    
    [Serializable]
    public class EnemySpawnParams
    {
        public string Tag;
        public GameObject Prefab;
        public bool InstanceAsPrefab;
        public Vector3 RaycastDirection = Vector3.down;
        public float RaycastOffset = 0f;
    }
}