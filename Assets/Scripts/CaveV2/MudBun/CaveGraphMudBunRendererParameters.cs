using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using BML.Utils;
using BML.Utils.Random;
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

        /// <summary>
        /// Randomizes room rotation around the transform.up axis.
        /// </summary>
        [SerializeField] public bool RandomizeRoomRotation = true;

        [SerializeField] public Vector3 TunnelConnectionOffset = Vector3.zero;
        
        [SerializeField] public GameObject StartRoomPrefab;
        [SerializeField] public GameObject EndRoomPrefab;
        [SerializeField] public GameObject MerchantRoomPrefab;
        [SerializeField] public GameObject TunnelPrefab;
        [SerializeField] public GameObject TunnelWithBarrierPrefab;
        [SerializeField] public GameObject TunnelWithSlidingBarrierPrefab;
        
        [Serializable]
        [HideReferenceObjectPicker]
        private class RoomOptions
        {
            // [PreviewField]
            // [AssetsOnly]
            [HideReferenceObjectPicker]
            public WeightedValueOptions<GameObject> defaultPrefabs = new WeightedValueOptions<GameObject>();
            // [AssetsOnly]
            [HideReferenceObjectPicker]
            public WeightedValueOptions<GameObject> roomPrefabs = new WeightedValueOptions<GameObject>();
            // [Tooltip("Set to -1 to disable limit.")]
            // public int countLimit = -1;
        }
        
        [DictionaryDrawerSettings(KeyLabel = "Room Type", ValueLabel = "Rooms",
            DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        [SerializeField]
        private Dictionary<CaveNodeType, RoomOptions> _roomTypes =
            new Dictionary<CaveNodeType, RoomOptions>();

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

        public GameObject GetRandomDefaultRoomForType(CaveNodeType nodeType)
        {
            if (!_roomTypes.ContainsKey(nodeType))
            {
                return default;
            }

            return _roomTypes[nodeType].defaultPrefabs.RandomWithWeights();
        }

        public WeightedValueOptions<GameObject> GetWeightedRoomOptionsForType(CaveNodeType nodeType)
        {
            if (!_roomTypes.ContainsKey(nodeType))
            {
                return default;
            }

            return _roomTypes[nodeType].roomPrefabs;
        }

        #endregion
    }
}