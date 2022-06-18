using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2
{
    [CreateAssetMenu(fileName = "CaveGenClayxelRenderParams", menuName = "BML/CaveGenClayxelRenderParams", order = 0)]
    public class CaveGraphClayxelRendererParameters : ScriptableObject
    {
        #region Inspector

        [SerializeField] public GameObject StartRoomPrefab;
        [SerializeField] public GameObject EndRoomPrefab;
        [SerializeField] public GameObject TunnelPrefab;
        
        [Serializable]
        private class RoomOption
        {
            // [PreviewField]
            public GameObject roomPrefab;
            // [Tooltip("Set to -1 to disable limit.")]
            // public int countLimit = -1;
        }
        [SerializeField] private List<RandomUtils.WeightPair<RoomOption>> _roomOptions;
        
        #endregion

        #region Buttons

        

        #endregion
        
        #region Unity lifecycle
        
        public delegate void OnValidateFunction();
        public event OnValidateFunction OnValidateEvent;

        private void OnValidate()
        {
            OnValidateEvent?.Invoke();
        }

        #endregion

        #region Utils

        public GameObject GetRandomRoom()
        {
            // var roomOptions = _roomOptions.Where(r => r.value.countLimit)
            return RandomUtils.RandomWithWeights(_roomOptions).roomPrefab;
        }

        #endregion
    }
}