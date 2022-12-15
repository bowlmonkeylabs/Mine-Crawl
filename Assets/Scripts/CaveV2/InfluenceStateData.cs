using System;
using System.Collections.Generic;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using UnityEngine;

namespace BML.Scripts.CaveV2
{
    [CreateAssetMenu(fileName = "InfluenceStateData", menuName = "BML/Cave Gen/InfluenceStateData", order = 0)]
    public class InfluenceStateData : ScriptableObject
    {
        public Dictionary<Collider, CaveNodeData> _currentNodes;
        public Dictionary<Collider, CaveNodeConnectionData> _currentNodeConnections;

        #region Unity Lifecycle

        private void OnEnable()
        {
            Reset();
        }

        #endregion

        private void Reset()
        {
            _currentNodes = new Dictionary<Collider, CaveNodeData>();
            _currentNodeConnections = new Dictionary<Collider, CaveNodeConnectionData>();
        }
    }
}