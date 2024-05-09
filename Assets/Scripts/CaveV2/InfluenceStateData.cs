using System;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace BML.Scripts.CaveV2
{
    [CreateAssetMenu(fileName = "InfluenceStateData", menuName = "BML/Cave Gen/InfluenceStateData", order = 0)]
    public class InfluenceStateData : ScriptableObject, IResettableScriptableObject
    {
        public Dictionary<Collider, CaveNodeData> _currentNodes; // TODO remove?
        public Dictionary<Collider, CaveNodeConnectionData> _currentNodeConnections; // TODO remove?
        public Dictionary<Collider, ICaveNodeData> _current;

        #region Unity Lifecycle

        private void OnEnable()
        {
            ResetScriptableObject();
        }

        #endregion

        public event IResettableScriptableObject.OnResetScriptableObject OnReset;
        public void ResetScriptableObject()
        {
            _currentNodes = new Dictionary<Collider, CaveNodeData>();
            _currentNodeConnections = new Dictionary<Collider, CaveNodeConnectionData>();
            _current = new Dictionary<Collider, ICaveNodeData>();
            OnReset?.Invoke();
        }

        
    }
}