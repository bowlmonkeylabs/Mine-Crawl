using System;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2.Objects;
using BML.Scripts.CaveV2.SpawnObjects;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph.NodeData
{
    [Serializable]
    public class CaveNodeData : ICaveNodeData
    {
        // Fundamental properties
        [ShowInInspector] public Vector3 LocalPosition { get; private set; }
        [ShowInInspector] public float Scale { get; set; }
        [ShowInInspector] public CaveNodeType NodeType { get; set; }
        
        // Calculated properties
        [ShowInInspector] public int MainPathDistance { get; set; }
        [ShowInInspector] public int StartDistance { get; set; }
        [ShowInInspector] public int ObjectiveDistance { get; set; }
        [ShowInInspector] public int PlayerDistance { get; set; }
        [ShowInInspector] public int PlayerDistanceDelta { get; set; }
        [ShowInInspector] public float DirectPlayerDistance { get; set; }
        
        [ShowInInspector]
        public bool PlayerVisited
        {
            get => playerVisited;
            set
            {
                if (!playerVisited && value)
                    OnPlayerVisited();
                
                playerVisited = value;
            }
        }
        [ShowInInspector] public bool PlayerVisitedAdjacent { get; set; }
        [ShowInInspector] public bool PlayerOccupied { get; set; }
        [ShowInInspector] public int TorchRequirement { get; set; }
        [ShowInInspector] public float TorchInfluence { get; set; }
        [ShowInInspector] public float PlayerInfluence { get; set; }
        
        // Fields
        private bool playerVisited;
        
        // Events
        public event EventHandler onPlayerVisited;

        // Scene object references
        [ShowInInspector]
        public GameObject GameObject
        {
            get => _gameObject;
            set
            {
                _gameObject = value;
                BoundsColliders.Clear();
                BoundsColliders.AddRange(this.GetRoomBoundsColliders(CaveNodeDataUtils.RoomBoundsLayerMask));
                TorchRequirement = this.CalculateTorchRequirement();
            }
        }
        private GameObject _gameObject;
        [ShowInInspector] public HashSet<Collider> BoundsColliders { get; set; }
        [ShowInInspector] public HashSet<SpawnPoint> SpawnPoints { get; set; }
        [ShowInInspector] public HashSet<Torch> Torches { get; set; }

        [ShowInInspector] public FloatVariable TorchAreaCoverage { get; set; }

        public CaveNodeData(Vector3 localPosition, float scale, FloatVariable torchAreaCoverage)
        {
            LocalPosition = localPosition;
            Scale = scale;

            MainPathDistance = -1;
            ObjectiveDistance = -1;
            PlayerDistance = -1;
            PlayerDistanceDelta = 0;
            DirectPlayerDistance = -1;
            PlayerVisited = false;
            PlayerVisitedAdjacent = false;
            PlayerOccupied = false;
            TorchRequirement = CaveNodeDataUtils.TorchRequirementMinMax.x;
            TorchInfluence = -1f;
            PlayerInfluence = 0f;

            BoundsColliders = new HashSet<Collider>();
            SpawnPoints = new HashSet<SpawnPoint>();
            Torches = new HashSet<Torch>();

            TorchAreaCoverage = torchAreaCoverage;
        }

        private void OnPlayerVisited()
        {
            onPlayerVisited?.Invoke(this, new EventArgs());
        }
    }
}