﻿using System;
using System.Collections.Generic;
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
        [ShowInInspector] public float Size { get; set; }
        
        // Calculated properties
        [ShowInInspector] public int MainPathDistance { get; set; }
        [ShowInInspector] public int ObjectiveDistance { get; set; }
        [ShowInInspector] public int PlayerDistance { get; set; }
        [ShowInInspector] public bool PlayerVisited { get; set; }
        [ShowInInspector] public bool PlayerOccupied { get; set; }
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
        [ShowInInspector] public HashSet<SpawnPoint> SpawnPoints { get; set; }
        [ShowInInspector] public HashSet<Torch> Torches { get; set; }

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
            Torches = new HashSet<Torch>();
        }
    }
}