using System;
using System.Collections.Generic;
using BML.Scripts.CaveV2.Objects;
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
        [ShowInInspector] public CaveNodeData Source { get; private set; }
        [ShowInInspector] public CaveNodeData Target { get; private set; }
        [ShowInInspector] public float Radius { get; private set; }
        [ShowInInspector] public float Length { get; private set; }
        [ShowInInspector] public float SteepnessAngle { get; private set; }
        
        // Calculated properties
        [ShowInInspector] public int MainPathDistance 
            => Mathf.Min(Source.MainPathDistance, Target.MainPathDistance);
        [ShowInInspector] public int ObjectiveDistance 
            => Mathf.Min(Source.ObjectiveDistance, Target.ObjectiveDistance);
        [ShowInInspector] public int PlayerDistance 
            => Mathf.Min(Source.PlayerDistance, Target.PlayerDistance);
        [ShowInInspector] public int PlayerDistanceDelta
            => Mathf.RoundToInt((Source.PlayerDistanceDelta + Target.PlayerDistanceDelta) / 2f);
        [ShowInInspector] public bool PlayerVisited { get; set; }
        [ShowInInspector] public bool PlayerOccupied { get; set; }
        [ShowInInspector] public int TorchRequirement { get; set; }
        [ShowInInspector] public float TorchInfluence { get; set; }
        [ShowInInspector] public float PlayerInfluence { get; set; }
        
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
        [ShowInInspector] public HashSet<Torch> Torches { get; set; }

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

            PlayerVisited = false;
            PlayerOccupied = false;
            TorchRequirement = CaveNodeDataUtils.TorchRequirementMinMax.x;
            TorchInfluence = -1f;
            PlayerInfluence = 0f;

            BoundsColliders = new HashSet<Collider>();
            Torches = new HashSet<Torch>();
        }
        
    }
}