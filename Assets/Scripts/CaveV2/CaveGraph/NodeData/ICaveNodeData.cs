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
        [ShowInInspector] public bool IsNode { get; }
        [ShowInInspector] public bool IsConnection { get; }
        // Fundamental properties
        [ShowInInspector] public Vector3 LocalPosition { get; }
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
        [ShowInInspector] public bool PlayerVisited { get; set; }
        [ShowInInspector] public bool PlayerVisitedAdjacent { get; set; }
        [ShowInInspector] public bool PlayerVisitedAllAdjacent { get; set; }
        [ShowInInspector] public bool PlayerOccupied { get; set; }
        [ShowInInspector] public bool PlayerOccupiedAdjacentNodeOrConnection { get; set; }
        [ShowInInspector] public int TorchRequirement { get; set; }
        [ShowInInspector] public float TorchInfluence { get; set; }
        [ShowInInspector] public float PlayerInfluence { get; set; }

        // Scene object references
        [ShowInInspector] public GameObject GameObject { get; set; }
        [ShowInInspector] public HashSet<Collider> BoundsColliders { get; set; }
        [ShowInInspector] public HashSet<EnemySpawnPoint> EnemySpawnPoints { get; set; }
        [ShowInInspector] public HashSet<LevelObjectSpawnPoint> LevelObjectSpawnPoints { get; set; }
        [ShowInInspector] public HashSet<Torch> Torches { get; set; }
        
        // Events
        public event EventHandler onPlayerVisited;
    }
}