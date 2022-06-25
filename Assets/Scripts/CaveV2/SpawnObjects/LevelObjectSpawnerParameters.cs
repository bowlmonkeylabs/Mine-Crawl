using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Utils;
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
            public string Tag;
            public GameObject Prefab;
        }
        public List<SpawnAtTagParameters> SpawnAtTags;
        
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