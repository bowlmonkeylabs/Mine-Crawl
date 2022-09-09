using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    [CreateAssetMenu(fileName = "LevelObjectSpawnerParams", menuName = "BML/Cave Gen/LevelObjectSpawnerParameters", order = 0)]
    public class LevelObjectSpawnerParameters : ScriptableObject
    {
        #region Inspector

        [Serializable]
        public class SpawnAtTagParameters
        {
            [Serializable, InlineProperty]
            public class MinMax
            {
                [HorizontalGroup(.1f), HideLabel] public bool EnableMin = false;
                [HorizontalGroup(.3f), HideLabel, EnableIf("$EnableMin")] public int ValueMin = 1;
                [HorizontalGroup(.1f, MarginLeft = .1f), HideLabel] public bool EnableMax = false;
                [HorizontalGroup(.3f), HideLabel, EnableIf("$EnableMax")] public int ValueMax = 1;
            }
            
            [PropertySpace(10,0)]
            public string Tag;
            public GameObject Prefab;
            public bool InstanceAsPrefab;
            public bool ChooseWithoutReplacement;
            [Range(0f, 1f)] public float SpawnProbability;

            public MinMax MinMaxGlobalAmount;
            public MinMax MinMaxClusterSize;
            [PropertySpace(0,10)]
            [Range(0f, 1f)] public float MainPathProbability = 0.5f;
        }
        public List<SpawnAtTagParameters> SpawnAtTags;

        public LayerMask TerrainLayerMask;
        
        [Range(0f,100f)]
        public float MaxRaycastLength = 10f;
        
        #endregion
        
        #region Unity lifecycle
        
        public delegate void OnValidateFunction();
        public event OnValidateFunction OnValidateEvent;

        private void OnValidate()
        {
            OnValidateEvent?.Invoke();
        }

        #endregion
    }
}