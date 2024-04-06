using System;
using System.Collections.Generic;
using BML.Scripts.CaveV2.Objects;
using BML.Scripts.CaveV2.SpawnObjects;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.Utils;
using QuikGraph;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph.NodeData
{
    [Serializable]
    public class CaveNodeConnectionData : IEdge<CaveNodeData>, ICaveNodeData
    {
        // Fundamental properties
        [ShowInInspector] public bool IsNode => false;
        [ShowInInspector] public bool IsConnection => true;
        [ShowInInspector] public Vector3 LocalPosition => (Source.LocalPosition + Target.LocalPosition) / 2f;
        [ShowInInspector] public CaveNodeData Source { get; private set; }
        [ShowInInspector] public CaveNodeData Target { get; private set; }
        [ShowInInspector] public float Radius { get; set; }
        [ShowInInspector] public float Length { get; private set; }
        [ShowInInspector] public float SteepnessAngle { get; private set; }
        [ShowInInspector] public bool IsBlocked { get; set; }
        [ShowInInspector] public CaveNodeConnectionPort SourceConnectionPort { get; set; }
        [ShowInInspector] public CaveNodeConnectionPort TargetConnectionPort { get; set; }
        
        // Calculated properties
        [ShowInInspector] public int MainPathDistance { get; set; }
        [ShowInInspector] public int StartDistance { get; set; }
        [ShowInInspector] public int ObjectiveDistance { get; set; }
        [ShowInInspector] public int PlayerDistance { get; set; }
        [ShowInInspector] public int PlayerDistanceDelta { get; set; }
        [ShowInInspector] public float DirectPlayerDistance { get; set; }
        [ShowInInspector] public bool PlayerMapped { get; set; }
        [ShowInInspector] public bool PlayerMappedAdjacent { get; set; }
        [ShowInInspector] public bool PlayerMappedAllAdjacent { get; set; }
        [ShowInInspector] 
        public bool PlayerVisited
        {
            get => playerVisited;
            set
            {
                if (!playerVisited && value)
                {
                    OnPlayerVisited();
                }
                playerVisited = value;
            }
        }
        [ShowInInspector] public bool PlayerVisitedAdjacent { get; set; }
        [ShowInInspector] public bool PlayerVisitedAllAdjacent { get; set; }
        [ShowInInspector] public bool PlayerOccupied { get; set; }
        [ShowInInspector] public bool PlayerOccupiedAdjacentNodeOrConnection { get; set; }
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
                BoundsColliders.AddRange(CaveNodeDataUtils.GetRoomBoundsColliders(this, CaveNodeDataUtils.RoomBoundsLayerMask));
            }
        }
        private GameObject _gameObject;
        [ShowInInspector] public HashSet<Collider> BoundsColliders { get; set; }
        [ShowInInspector] public HashSet<EnemySpawnPoint> EnemySpawnPoints { get; set; }
        [ShowInInspector] public HashSet<LevelObjectSpawnPoint> LevelObjectSpawnPoints { get; set; }
        [ShowInInspector] public HashSet<Torch> Torches { get; set; }
        [ShowInInspector] public GameObject Barrier { get; set; }

        public CaveNodeConnectionData(CaveNodeData source, CaveNodeData target, float radius)
        {
            Source = source;
            Target = target;
            Radius = radius;
            Length = Vector3.Distance(source.LocalPosition, target.LocalPosition);
            
            var edgeDir = (target.LocalPosition - source.LocalPosition).normalized;
            if (edgeDir.y < 0)
            {
                edgeDir = -edgeDir;
            }
            var horizontalComponent = edgeDir.xoz().magnitude;
            var verticalComponent = edgeDir.y;
            SteepnessAngle = Mathf.Rad2Deg * Mathf.Atan2(verticalComponent, horizontalComponent);

            MainPathDistance = -1;
            StartDistance = -1;
            ObjectiveDistance = -1;
            PlayerDistance = -1;
            PlayerDistanceDelta = 0;
            DirectPlayerDistance = -1;
            
            PlayerMapped = false;
            PlayerMappedAdjacent = false;
            PlayerMappedAllAdjacent = false;
            PlayerVisited = false;
            PlayerVisitedAdjacent = false;
            PlayerVisitedAllAdjacent = false;
            PlayerOccupied = false;
            TorchRequirement = CaveNodeDataUtils.TorchRequirementMinMax.x;
            TorchInfluence = -1f;
            PlayerInfluence = 0f;

            BoundsColliders = new HashSet<Collider>();
            EnemySpawnPoints = new HashSet<EnemySpawnPoint>();
            LevelObjectSpawnPoints = new HashSet<LevelObjectSpawnPoint>();
            Torches = new HashSet<Torch>();
        }
        
        private void OnPlayerVisited()
        {
            onPlayerVisited?.Invoke(this, new EventArgs());
        }
        
        public CaveNodeData OtherEnd(CaveNodeData known)
        {
            if (known == Source) return Target;
            if (known == Target) return Source;
            return null;
        }

        public bool HasNode(CaveNodeData node)
        {
            return node == Source || node == Target;
        }

        public bool BidirectionalCheck(Func<CaveNodeData, CaveNodeData, bool> check)
        {
            var resultA = check(this.Source, this.Target);
            var resultB = check(this.Target, this.Source);
            return resultA || resultB;
        }
    }
}