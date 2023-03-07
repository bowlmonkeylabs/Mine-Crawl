using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.MudBun
{
    [CreateAssetMenu(fileName = "CaveGenMudBunRenderParams", menuName = "BML/Cave Gen/CaveGenMudBunRenderParameters", order = 0)]
    public class CaveGraphMudBunRendererParameters : SerializedScriptableObject
    {
        #region Inspector

        [SerializeField] public Color CaveColor;

        [SerializeField] public Vector3 TunnelConnectionOffset = Vector3.zero;
        
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

        [DictionaryDrawerSettings(KeyLabel = "Room Type", ValueLabel = "Options",
            DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        [SerializeField]
        private Dictionary<CaveNodeType, RandomUtils.WeightedOptions<RoomOption>> _roomTypes =
            new Dictionary<CaveNodeType, RandomUtils.WeightedOptions<RoomOption>>();

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
        
        public GameObject GetRandomRoom(CaveNodeType nodeType = CaveNodeType.Unassigned)
        {
            if (!_roomTypes.ContainsKey(nodeType))
            {
                return null;
            }

            var randomRoom = _roomTypes[nodeType].RandomWithWeights();
            return randomRoom.roomPrefab;
        }

        public IEnumerable<GameObject> GetAllRooms()
        {
            var allPrefabs = new List<GameObject>();

            allPrefabs.Add(this.StartRoomPrefab);
            allPrefabs.Add(this.EndRoomPrefab);
            allPrefabs.Add(this.MerchantRoomPrefab);
            allPrefabs.Add(this.TunnelPrefab);
            var roomTypePrefabs = this._roomTypes
                .Values.SelectMany(v => 
                    v.Options.Select(op => op.value.roomPrefab)
                );
            allPrefabs.AddRange(roomTypePrefabs);

            return allPrefabs;
        }

        #endregion
    }
}