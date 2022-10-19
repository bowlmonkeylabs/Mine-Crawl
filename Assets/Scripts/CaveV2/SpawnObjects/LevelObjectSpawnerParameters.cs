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
            [FoldoutGroup("Parameters")] public Vector3 RaycastDirection = Vector3.down;
            [FoldoutGroup("Parameters")] public float RaycastOffset = 0f;
            [FoldoutGroup("Parameters")] public bool InstanceAsPrefab;
            [FoldoutGroup("Parameters")] public bool ChooseWithoutReplacement;
            [FoldoutGroup("Parameters")] [Range(0f, 1f)] public float SpawnProbability;

            [FoldoutGroup("Parameters")] public MinMax MinMaxGlobalAmount;
            [FoldoutGroup("Parameters")] public MinMax MinMaxClusterSize;
            [PropertySpace(0,10)] [Tooltip("Curve sample space is 0 to 1; this will be 'current room's distance from the main path', divided by 'maximum main path distance present in the level' (this /should/ be close to MaxOffshootLength of the level gen params)")]
            [FoldoutGroup("Parameters")] public AnimationCurve MainPathProbabilityFalloff = AnimationCurve.Constant(0f, 1f, 1f);
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