using System;
using System.Collections.Generic;
using BML.Scripts.CaveV2.SpawnObjects;
using BML.Scripts.Utils;
using QuikGraph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph
{
    [Serializable]
    public class CaveNodeData
    {
        // Fundamental properties
        [ShowInInspector] public Vector3 LocalPosition { get; private set; }
        [ShowInInspector] public float Size { get; set; }
        
        // Calculated properties
        [ShowInInspector] public int MainPathDistance { get; set; }
        [ShowInInspector] public int ObjectiveDistance { get; set; }
        [ShowInInspector] public int PlayerDistance { get; set; }
        [ShowInInspector] public bool PlayerVisited { get; set; }
        [ShowInInspector] public bool PlayerOccupied { get; set; }
        [ShowInInspector] public float PlayerInfluence { get; set; }
        
        // Scene object references
        [ShowInInspector] public GameObject GameObject { get; set; }
        [ShowInInspector] public HashSet<SpawnPoint> SpawnPoints { get; set; }

        public CaveNodeData(Vector3 localPosition, float size)
        {
            LocalPosition = localPosition;
            Size = size;

            MainPathDistance = -1;
            ObjectiveDistance = -1;
            PlayerDistance = -1;
            PlayerVisited = false;
            PlayerOccupied = false;
            PlayerInfluence = -1f;
            
            SpawnPoints = new HashSet<SpawnPoint>();
        }
    }
    
    [Serializable]
    public class CaveNodeConnectionData : IEdge<CaveNodeData>
    {
        // Fundamental properties
        [ShowInInspector] public CaveNodeData Source { get; private set; }
        [ShowInInspector] public CaveNodeData Target { get; private set; }
        [ShowInInspector] public float Radius { get; private set; }
        [ShowInInspector] public float Length { get; private set; }
        [ShowInInspector] public float SteepnessAngle { get; private set; }
        
        // Calculated properties
        [ShowInInspector] public bool PlayerVisited { get; set; }
        [ShowInInspector] public bool PlayerOccupied { get; set; }
        [ShowInInspector] public float PlayerInfluence { get; set; }
        
        // Scene object references
        [ShowInInspector] public GameObject GameObject { get; set; }

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
            PlayerInfluence = -1f;
        }
        
    }
    
}