using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.MudBun
{
    [CreateAssetMenu(fileName = "CaveGenMudBunRenderParams", menuName = "BML/Cave Gen/CaveGenMudBunRenderParameters", order = 0)]
    public class CaveGraphMudBunRendererParameters : ScriptableObject
    {
        #region Inspector

        /// <summary>
        /// This property is used to control where tunnels attempt to connect between rooms. It should roughly match the scale of the MudBun room prefab base scale.
        /// </summary>
        [SerializeField] public float BaseRoomRadius;

        [SerializeField] public Color CaveColor;
        
        [SerializeField] public GameObject StartRoomPrefab;
        [SerializeField] public GameObject EndRoomPrefab;
        [SerializeField] public GameObject MerchantRoomPrefab;
        [SerializeField] public GameObject TunnelPrefab;
        [SerializeField] public GameObject TunnelWithBarrierPrefab;

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
        
        public IEnumerable<GameObject> GetAllRooms()
        {
            var allPrefabs = new List<GameObject>();

            allPrefabs.Add(this.StartRoomPrefab);
            allPrefabs.Add(this.EndRoomPrefab);
            allPrefabs.Add(this.MerchantRoomPrefab);
            allPrefabs.Add(this.TunnelPrefab);
            allPrefabs.AddRange(this._roomOptions.Select(r => r.value.roomPrefab));

            return allPrefabs;
        }

        #endregion
    }
}