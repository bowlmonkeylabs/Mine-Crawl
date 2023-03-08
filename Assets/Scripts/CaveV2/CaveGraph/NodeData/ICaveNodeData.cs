using System;
using System.Collections.Generic;
using BML.Scripts.CaveV2.Objects;
using BML.Scripts.CaveV2.SpawnObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph.NodeData
{
    public interface ICaveNodeData
    {
        // Calculated properties
        [ShowInInspector] public int MainPathDistance { get; }
        [ShowInInspector] public int StartDistance { get; }
        [ShowInInspector] public int ObjectiveDistance { get; }
        [ShowInInspector] public int PlayerDistance { get; }
        [ShowInInspector] public int PlayerDistanceDelta { get; }
        [ShowInInspector] public float DirectPlayerDistance { get; }
        [ShowInInspector] public bool PlayerVisited { get; set; }
        [ShowInInspector] public bool PlayerVisitedAdjacent { get; set; }
        [ShowInInspector] public bool PlayerOccupied { get; set; }
        [ShowInInspector] public int TorchRequirement { get; set; }
        [ShowInInspector] public float TorchInfluence { get; set; }
        [ShowInInspector] public float PlayerInfluence { get; set; }

        // Scene object references
        [ShowInInspector] public GameObject GameObject { get; set; }
        [ShowInInspector] public HashSet<Collider> BoundsColliders { get; set; }
        [ShowInInspector] public HashSet<SpawnPoint> SpawnPoints { get; set; }
        [ShowInInspector] public HashSet<Torch> Torches { get; set; }
        
        // Events
        public event EventHandler onPlayerVisited;
    }
}