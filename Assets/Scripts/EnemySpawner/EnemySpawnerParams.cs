﻿using System;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Attributes;
using BML.Scripts.CaveV2.SpawnObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    [CreateAssetMenu(fileName = "EnemySpawnerParams", menuName = "BML/EnemySpawnerParams", order = 0)]
    public class EnemySpawnerParams : ScriptableObject
    {
        public float SpawnDelay = 5f;
        public float SpawnDelayLowIntensity = 3f;
        public SafeFloatValueReference SpawnCap;
        public float MaxIntensity = 5;
        public float LowIntensity = 2f;
        public float MinIntesity = 1;
        public List<EnemySpawnParams> SpawnPointParams;
    }
    
    [Serializable]
    public class EnemySpawnParams
    {
        [Tag("Spawn_Enemy")] public List<string> Tags;
        public GameObject Prefab;
        public int Cost = 1;
        [FoldoutGroup("Parameters")] public float SpawnPosOffset = 0f;
        [FoldoutGroup("Parameters")] public float SpawnRadiusOffset = .5f;
        [FoldoutGroup("Parameters")] public bool RequireStableSurface;
        [FoldoutGroup("Parameters")] public bool InstanceAsPrefab;
        [FoldoutGroup("Parameters")] public bool OccupySpawnPoint;
        [ReadOnly] public float NormalizedSpawnWeight;
    }
}