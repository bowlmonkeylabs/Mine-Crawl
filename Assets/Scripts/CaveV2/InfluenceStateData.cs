using System;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using UnityEditor;
using UnityEngine;

namespace BML.Scripts.CaveV2
{
    [CreateAssetMenu(fileName = "InfluenceStateData", menuName = "BML/Cave Gen/InfluenceStateData", order = 0)]
    public class InfluenceStateData : ScriptableVariableBase
    {
        public Dictionary<Collider, CaveNodeData> _currentNodes; // TODO remove?
        public Dictionary<Collider, CaveNodeConnectionData> _currentNodeConnections; // TODO remove?
        public Dictionary<Collider, ICaveNodeData> _current;

        #region Unity Lifecycle

        private void OnEnable()
        {
            Reset();
        }

        #endregion

        public override void Reset()
        {
            _currentNodes = new Dictionary<Collider, CaveNodeData>();
            _currentNodeConnections = new Dictionary<Collider, CaveNodeConnectionData>();
            _current = new Dictionary<Collider, ICaveNodeData>();
#if UNITY_EDITOR
            string fullPath = AssetDatabase.GetAssetPath(this);
            ScriptableObjectResetOnEnterPlaymode.RegisterVariableToReset(fullPath);
            Debug.Log("Reset InfluenceStateData");
#endif
        }
    }
}