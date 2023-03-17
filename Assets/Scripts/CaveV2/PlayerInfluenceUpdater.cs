using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.CaveV2
{
    public class PlayerInfluenceUpdater : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private LayerMask _nodeBoundsLayerMask;
        [SerializeField] private GameObjectSceneReference _caveGenComponentGameObjectSceneReference;
        private CaveGenComponentV2 _caveGenerator => _caveGenComponentGameObjectSceneReference.CachedComponent as CaveGenComponentV2;
        [SerializeField] private BoolReference _isExitChallengeActive;
        [SerializeField] private InfluenceStateData _influenceState;

        [SerializeField] private bool _enableLogs = true;

        #endregion
        
        #region Unity lifecycle

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
            bool isNodeBoundsLayer = other.gameObject.IsInLayerMask(_nodeBoundsLayerMask);
            if (isNodeBoundsLayer)
            {
                if (_enableLogs) Debug.Log($"PlayerInfluenceUpdater OnTriggerEnter");
                    
                bool isNotAlreadyEntered =
                    (!_influenceState._currentNodes.ContainsKey(other) &&
                     !_influenceState._currentNodeConnections.ContainsKey(other));
                if (isNotAlreadyEntered)
                {
                    var caveNodeDataComponent = other.GetComponentInParent<CaveNodeDataDebugComponent>();
                    if (caveNodeDataComponent != null)
                    {
                        var caveNodeData = caveNodeDataComponent.CaveNodeData;
                        if (caveNodeData == null)
                        {
                            if (_enableLogs) Debug.Log($"PlayerInfluenceUpdater OnTriggerEnter: Cave node data NULL for this {other.gameObject.layer} collider");
                            return;
                        }

                        // Below is an experiment to only add new nodes which are directly connected to the already occupied nodes. The code achieves this, but it caused as many issues as it solved.
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
                        foreach (var caveNodeConnectionData in _caveGenerator.CaveGraph.AdjacentEdges(caveNodeData))
                        {
                            caveNodeConnectionData.PlayerVisitedAdjacent = true;
                            UpdatePlayerVisitedAllAdjacent(caveNodeConnectionData);
                        }
                        UpdatePlayerVisitedAllAdjacent(caveNodeData);
                        caveNodeData.PlayerOccupied = true;
                        _influenceState._currentNodes.Add(other, caveNodeData);
                        _caveGenerator?.UpdatePlayerDistance(_influenceState._currentNodes.Values.AsEnumerable());
                        
                        if (_enableLogs) Debug.Log($"PlayerInfluenceUpdater OnTriggerEnter: Player entered {caveNodeData.LocalPosition}");
                    }
                    else
                    {
                        var caveNodeConnectionDataComponent =
                            other.GetComponentInParent<CaveNodeConnectionDataDebugComponent>();
                        if (caveNodeConnectionDataComponent == null)
                        {
                            if (_enableLogs) Debug.Log($"PlayerInfluenceUpdater OnTriggerEnter: Cave node component was not found for this {other.gameObject.layer} collider");
                            return;
                        }
                        
                        var caveNodeConnectionData = caveNodeConnectionDataComponent.CaveNodeConnectionData;
                        if (caveNodeConnectionData == null)
                        {
                            if (_enableLogs) Debug.LogError($"PlayerInfluenceUpdater OnTriggerEnter: Cave node connection data NULL for this {other.gameObject.layer} collider");
                            return;
                        }
                        
                        caveNodeConnectionData.PlayerVisited = true;
                        caveNodeConnectionData.Source.PlayerVisitedAdjacent = true;
                        caveNodeConnectionData.Target.PlayerVisitedAdjacent = true;
                        caveNodeConnectionData.PlayerOccupied = true;
                        UpdatePlayerVisitedAllAdjacent(caveNodeConnectionData);
                        UpdatePlayerVisitedAllAdjacent(caveNodeConnectionData.Source);
                        UpdatePlayerVisitedAllAdjacent(caveNodeConnectionData.Target);
                        _influenceState._currentNodeConnections.Add(other, caveNodeConnectionData);
                        // _caveGenerator?.UpdatePlayerDistance(_currentNodes.Values.AsEnumerable());
                        
                        if (_enableLogs) Debug.Log($"PlayerInfluenceUpdater OnTriggerEnter: Player entered {caveNodeConnectionData.Source.LocalPosition} <-> {caveNodeConnectionData.Target.LocalPosition}");
                    }
                }
                else
                {
                    if (_enableLogs) Debug.LogError($"PlayerInfluenceUpdater OnTriggerEnter: {other.gameObject.layer} trigger entered, but already in bounds");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            bool isNodeBoundsLayer = other.gameObject.IsInLayerMask(_nodeBoundsLayerMask);
            if (isNodeBoundsLayer)
            {
                if (_enableLogs) Debug.Log($"PlayerInfluenceUpdater OnTriggerExit");
                
                if (_influenceState._currentNodes.ContainsKey(other))
                {
                    if (_enableLogs) Debug.Log($"PlayerInfluenceUpdater OnTriggerExit: Player left {_influenceState._currentNodes[other].LocalPosition}");
                    
                    _influenceState._currentNodes[other].PlayerOccupied = false;
                    _influenceState._currentNodes.Remove(other);
                    _caveGenerator?.UpdatePlayerDistance(_influenceState._currentNodes.Values.AsEnumerable());
                }
                else if (_influenceState._currentNodeConnections.ContainsKey(other))
                {
                    if (_enableLogs) Debug.Log($"PlayerInfluenceUpdater OnTriggerExit: Player left {_influenceState._currentNodeConnections[other].Source.LocalPosition} <-> {_influenceState._currentNodeConnections[other].Target.LocalPosition}");
                    
                    _influenceState._currentNodeConnections[other].PlayerOccupied = false;
                    _influenceState._currentNodeConnections.Remove(other);
                    _caveGenerator?.UpdatePlayerDistance(_influenceState._currentNodes.Values.AsEnumerable());
                }
                else
                {
                    if (_enableLogs) Debug.LogError($"{other.gameObject.layer} trigger exited, but already left room");
                }
            }
        }

        #endregion

        private void UpdatePlayerDistanceToCurrent()
        {
            _caveGenerator?.UpdatePlayerDistance(_influenceState._currentNodes.Values.AsEnumerable());
        }

        private bool UpdatePlayerVisitedAllAdjacent(CaveNodeData caveNodeData)
        {
            var adjacentEdges = _caveGenerator.CaveGraph.AdjacentEdges(caveNodeData);
            bool playerVisitedAllAdjacent = adjacentEdges.All(e => e.PlayerVisited);
            caveNodeData.PlayerVisitedAllAdjacent = playerVisitedAllAdjacent;
            return playerVisitedAllAdjacent;
        }
        
        private bool UpdatePlayerVisitedAllAdjacent(CaveNodeConnectionData caveNodeConnectionData)
        {
            bool playerVisitedAllAdjacent = caveNodeConnectionData.Source.PlayerVisited && caveNodeConnectionData.Target.PlayerVisited;
            caveNodeConnectionData.PlayerVisitedAllAdjacent = playerVisitedAllAdjacent;
            return playerVisitedAllAdjacent;
        }
    }
}