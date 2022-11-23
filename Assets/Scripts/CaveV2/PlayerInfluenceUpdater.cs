using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts.CaveV2
{
    public class PlayerInfluenceUpdater : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private LayerMask _nodeBoundsLayerMask;
        [SerializeField] private CaveGenComponentV2 _caveGenerator;
        [SerializeField] private BoolReference _isExitChallengeActive;

        #endregion

        private Dictionary<Collider, CaveNodeData> _currentNodes;
        private Dictionary<Collider, CaveNodeConnectionData> _currentNodeConnections;

        #region Unity lifecycle

        private void Awake()
        {
            _currentNodes = new Dictionary<Collider, CaveNodeData>();
            _currentNodeConnections = new Dictionary<Collider, CaveNodeConnectionData>();
        }

        private void OnEnable()
        {
            _isExitChallengeActive.Subscribe(UpdatePlayerDistanceToCurrent);
        }
        
        private void OnDisable()
        {
            _isExitChallengeActive.Unsubscribe(UpdatePlayerDistanceToCurrent);
        }

        private void OnTriggerEnter(Collider other)
        {
            var layerName = LayerMask.LayerToName(other.gameObject.layer);
            if (layerName == "Room Bounds")
            {
                // Debug.Log($"OnTriggerEnter ROOM BOUNDS");
            }
            bool isNodeBoundsLayer = other.gameObject.IsInLayerMask(_nodeBoundsLayerMask);
            if (isNodeBoundsLayer)
            {
                // Debug.Log($"PlayerInfluenceUpdater: OnTriggerEnter");
                bool isAlreadyEntered =
                    (!_currentNodes.ContainsKey(other) && !_currentNodeConnections.ContainsKey(other));
                if (isAlreadyEntered)
                {
                    var caveNodeDataComponent = other.GetComponentInParent<CaveNodeDataDebugComponent>();
                    if (caveNodeDataComponent != null)
                    {
                        var caveNodeData = caveNodeDataComponent.CaveNodeData;
                        if (caveNodeData == null)
                        {
                            // Debug.LogError($"Cave node data NULL for this {other.gameObject.layer} collider");
                            return;
                        }

                        // bool areOtherCurrentNodes = (_currentNodes.Count > 0);
                        // bool newNodeAdjacentToCurrentNodes = _currentNodes.Values
                        //     .Any(currentNode => 
                        //         _caveGenerator.CaveGraph.TryGetEdge(currentNode, caveNodeData, out var edge1)
                        //         || _caveGenerator.CaveGraph.TryGetEdge(caveNodeData, currentNode, out var edge2));
                        // if (areOtherCurrentNodes && !newNodeAdjacentToCurrentNodes)
                        // {
                        //     // Skip node which does not have direct connection to other current nodes
                        //     return;
                        // }
                        
                        caveNodeData.PlayerVisited = true;
                        caveNodeData.PlayerOccupied = true;
                        _currentNodes.Add(other, caveNodeData);
                        _caveGenerator.UpdatePlayerDistance(_currentNodes.Values.AsEnumerable());
                        // Debug.Log($"PLAYER IN {caveNodeData.LocalPosition}");
                    }
                    else
                    {
                        var caveNodeConnectionDataComponent =
                            other.GetComponentInParent<CaveNodeConnectionDataDebugComponent>();
                        if (caveNodeConnectionDataComponent == null)
                        {
                            // Debug.LogError($"Cave node component was not found for this {other.gameObject.layer} collider");
                            return;
                        }
                        
                        var caveNodeConnectionData = caveNodeConnectionDataComponent.CaveNodeConnectionData;
                        if (caveNodeConnectionData == null)
                        {
                            // Debug.LogError($"Cave node connection data NULL for this {other.gameObject.layer} collider");
                            return;
                        }
                        
                        caveNodeConnectionData.PlayerVisited = true;
                        caveNodeConnectionData.PlayerOccupied = true;
                        _currentNodeConnections.Add(other, caveNodeConnectionData);
                        // _caveGenerator.UpdatePlayerDistance(_currentNodes.Values.AsEnumerable());
                        // Debug.Log($"PLAYER IN {caveNodeConnectionData.Source.LocalPosition} <-> {caveNodeConnectionData.Target.LocalPosition}");
                    }
                }
                else
                {
                    // Debug.LogError($"{other.gameObject.layer} trigger entered, but already in bounds");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var layerName = LayerMask.LayerToName(other.gameObject.layer);
            if (layerName == "Room Bounds")
            {
                // Debug.Log($"OnTriggerExit ROOM BOUNDS");
            }
            bool isNodeBoundsLayer = other.gameObject.IsInLayerMask(_nodeBoundsLayerMask);
            if (isNodeBoundsLayer)
            {
                // Debug.Log($"PlayerInfluenceUpdater: OnTriggerExit");
                if (_currentNodes.ContainsKey(other))
                {
                    // Debug.Log($"PLAYER LEFT {_currentNodes[other]}");
                    _currentNodes[other].PlayerOccupied = false;
                    _currentNodes.Remove(other);
                    _caveGenerator.UpdatePlayerDistance(_currentNodes.Values.AsEnumerable());
                }
                else if (_currentNodeConnections.ContainsKey(other))
                {
                    // Debug.Log($"PLAYER LEFT {_currentNodeConnections[other].Source.LocalPosition} <-> {_currentNodeConnections[other].Target.LocalPosition}");
                    _currentNodeConnections[other].PlayerOccupied = false;
                    _currentNodeConnections.Remove(other);
                    _caveGenerator.UpdatePlayerDistance(_currentNodes.Values.AsEnumerable());
                }
                else
                {
                    // Debug.LogError($"{other.gameObject.layer} trigger exited, but already left room");
                }
            }
        }

        #endregion

        private void UpdatePlayerDistanceToCurrent()
        {
            _caveGenerator.UpdatePlayerDistance(_currentNodes.Values.AsEnumerable());
        }
    }
}