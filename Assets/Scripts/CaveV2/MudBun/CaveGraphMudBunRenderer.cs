﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using BML.Utils;
using BML.Utils.Random;
using Mono.CSharp.Linq;
using MudBun;
using Shapes;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.MudBun
{
    [ExecuteAlways]
    public class CaveGraphMudBunRenderer : MudBunGenerator
    {
        #region Inspector

        [Required, SerializeField]
        private CaveGenComponentV2 _caveGenerator;
        private CaveGraphV2 _caveGraph => _caveGenerator.CaveGraph;

        /// <summary>
        /// Renders MudBun as an interior by covering the entire cave area with a base brush for rooms to be "carved" out of.
        /// The outside faces of the area are culled for performance as they will never be seen.
        /// NOTE: when using this option, placed brushes must be set to SUBTRACT instead of union.
        /// </summary>
        [SerializeField] private bool _renderAsInterior = true;
        [SerializeField] private GameObject _interiorAreaFill;
        
        [Required, InlineEditor, SerializeField]
        private CaveGraphMudBunRendererParameters _caveGraphRenderParams;

        #endregion
        
        #region Unity lifecycle

        protected override void OnEnable()
        {
            base.OnEnable();
            _caveGenerator.OnAfterGenerate += GenerateMudBunInternal;
            _caveGraphRenderParams.OnValidateEvent += TryGenerateWithCooldown_OnValidate;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _caveGenerator.OnAfterGenerate -= GenerateMudBunInternal;
            _caveGraphRenderParams.OnValidateEvent -= TryGenerateWithCooldown_OnValidate;
        }

        #endregion

        #region MudBun
        
        class ConnectionPortEdge 
        {
            public CaveNodeConnectionPort ConnectionPort;
            public CaveNodeConnectionData Edge;
        }

        class SelectedRoom 
        {
            public GameObject RoomPrefab = null;
            public Quaternion RoomRotation = Quaternion.identity;
            public Vector3 RoomScale;
            public Dictionary<CaveNodeConnectionData, int> EdgeToConnectionPortIndexMap;
        }

        /// <summary>
        /// The shader we're using on the generated mesh utilizes the vertex colors as masks for applying textures; This function determines the map value for a given node.
        /// </summary>
        /// <returns></returns>
        private Color GetMaskColors(CaveNodeData caveNodeData)
        {
            bool isMainPathOrMerchant = caveNodeData.MainPathDistance == 0 || caveNodeData.NodeType == CaveNodeType.Merchant;
            float isMainPathOrMerchantFactor = (isMainPathOrMerchant ? 1f : 0f);
            float objectiveDistanceFactor = (float)caveNodeData.ObjectiveDistance / _caveGenerator.MaxObjectiveDistance;
            objectiveDistanceFactor = 1f - objectiveDistanceFactor;
            objectiveDistanceFactor = Mathf.Clamp01((objectiveDistanceFactor - 0.5f) * 2);
            return new Color(
                isMainPathOrMerchantFactor,
                objectiveDistanceFactor,
                0f,
                1f
            );
        }
        
        /// <summary>
        /// The shader we're using on the generated mesh utilizes the vertex colors as masks for applying textures; This function determines the map value for a given node.
        /// </summary>
        /// <returns></returns>
        private Color GetMaskColors(CaveNodeConnectionData caveNodeConnectionData)
        {
            bool isMainPathOrMerchant = caveNodeConnectionData.MainPathDistance == 0 ||
                                        caveNodeConnectionData.Source.NodeType == CaveNodeType.Merchant ||
                                        caveNodeConnectionData.Target.NodeType == CaveNodeType.Merchant;
            float isMainPathOrMerchantFactor = (isMainPathOrMerchant ? 1f : 0f);
            float objectiveDistanceFactor = (float)caveNodeConnectionData.ObjectiveDistance / _caveGenerator.MaxObjectiveDistance;
            objectiveDistanceFactor = 1f - objectiveDistanceFactor;
            objectiveDistanceFactor = Mathf.Clamp01((objectiveDistanceFactor - 0.5f) * 2);
            return new Color(
                isMainPathOrMerchantFactor,
                objectiveDistanceFactor,
                0f,
                1f
            );
        }

        protected override IEnumerator GenerateMudBunInternal(
            MudRenderer mudRenderer,
            bool instanceAsPrefabs
        )
        {
            int totalBrushCount = _caveGraph.VertexCount + _caveGraph.EdgeCount; 
            if (totalBrushCount > 200)
            {
                throw new Exception($"MudBun will not be generated with such a large cave graph (Vertices:{_caveGraph.VertexCount}, Edge:{_caveGraph.EdgeCount})");
            }
            
            base.GenerateMudBunInternal(mudRenderer, instanceAsPrefabs);
            
            Random.InitState(SeedManager.Instance.GetSteppedSeed("MudBunRenderer"));

            var localOrigin = mudRenderer.transform.position;

            // Render as interior
            if (_renderAsInterior)
            {
                GameObject newGameObject = GameObjectUtils.SafeInstantiate(instanceAsPrefabs, _interiorAreaFill, mudRenderer.transform);
                newGameObject.transform.position = localOrigin;
                newGameObject.transform.localScale = _caveGenerator.CaveGenBounds.size;
            }
            
            // Spawn "rooms" at each cave node
            foreach (var caveNodeData in _caveGraph.Vertices.OrderBy(vertex => vertex.StartDistance))
            {
                // Select room to spawn
                SelectedRoom selectedRoom;
                List<CaveNodeConnectionData> edges = _caveGraph.AdjacentEdges(caveNodeData).ToList();
                
                List<WeightedValueEntry<SelectedRoom>> validWeightedRoomPairs = _caveGraphRenderParams.GetWeightedRoomOptionsForType(caveNodeData.NodeType).Options
                .Where(roomWeightedPair => roomWeightedPair.Weight > 0)
                .Select(roomWeightedPair => {
                    return new WeightedValueEntry<SelectedRoom>(new SelectedRoom() {
                            RoomPrefab = roomWeightedPair.Value,
                            RoomScale = Vector3.one * caveNodeData.Scale,
                            RoomRotation = Quaternion.identity,
                            EdgeToConnectionPortIndexMap = new Dictionary<CaveNodeConnectionData, int>()
                        }, roomWeightedPair.Weight);
                })
                .Where(roomWeightedPair => {
                    List<CaveNodeConnectionPort> roomConnectionPorts = roomWeightedPair.Value.RoomPrefab.GetComponent<CaveGraphMudBunRoom>()?.ConnectionPorts;

                    if (roomConnectionPorts == null || roomConnectionPorts.Count() != edges.Count())
                    {
                        return false;
                    }

                    var edgeConnectionPortPairingsSets = roomConnectionPorts.Select(
                        (startingConnPort, startingConnPortIndex) => edges.Select(
                            (edge, edgeIndex) => new ConnectionPortEdge
                            {
                                ConnectionPort = roomConnectionPorts[(startingConnPortIndex + edgeIndex) % roomConnectionPorts.Count],
                                Edge = edge,
                            }).ToList()
                        ).ToList();

                    float minimalValidRotation = edgeConnectionPortPairingsSets.Aggregate(float.PositiveInfinity, (minimalValidRotation, edgeConnectionPortPairingsSet) => {
                        (float LowerRotationBound, float UpperRotationBound) = edgeConnectionPortPairingsSet.Aggregate((-1000f, 1000f), (rotationBounds, edgeConnectionPortPairing) => {
                            if(Mathf.Abs(edgeConnectionPortPairing.Edge.SteepnessAngle) > edgeConnectionPortPairing.ConnectionPort.AngleRangeVertical) {
                                return rotationBounds;
                            }
                            
                            //get the connection points location in relation to the center of the room, then relate it to the center of the cave node
                            var connectionPortInCaveGraphSpace = (edgeConnectionPortPairing.ConnectionPort.transform.position - roomWeightedPair.Value.RoomPrefab.transform.position) + caveNodeData.LocalPosition;
                             //get the position of this end of the edge in 2d space
                            var caveNodePosition = caveNodeData.LocalPosition.xz();
                            //get the position of the other end of the edge in 2d space
                            var otherEndPosition = edgeConnectionPortPairing.Edge.OtherEnd(caveNodeData).LocalPosition.xz();
                            //move connection port to cave graph space, and reduce it to two dimensions
                            var initialConnectionPortPositionInCaveGraphSpace = connectionPortInCaveGraphSpace.xz();
                            var initialConnectionPortDirection = (initialConnectionPortPositionInCaveGraphSpace - caveNodePosition).normalized;
                            //could also just do ConnectionPort.position.magnitue but want to keep it in cave graph space
                            var radiusToConnectionPort = (initialConnectionPortPositionInCaveGraphSpace - caveNodePosition).magnitude;
                            //align connection port to other end
                            var initialAlignedConnectionPortDirection = (otherEndPosition - caveNodePosition).normalized;
                            var initialAlignedConnectionPort = (radiusToConnectionPort * initialAlignedConnectionPortDirection) + caveNodePosition;

                            var angleToRotateConnectionPortToAlignWithOtherEnd = -Vector2.SignedAngle(initialConnectionPortDirection, initialAlignedConnectionPortDirection);
                            
                            //get the max number of degrees the room can be rotated before it breaks the connection points angle range
                            
                            float angleTolerance = edgeConnectionPortPairing.ConnectionPort.AngleRangeHorizontal / 2;
                            float angleRange = Enumerable.Range(1, 359).FirstOrDefault(degrees => {
                                var newConnectionPointDirection = initialAlignedConnectionPortDirection.Rotate(degrees);
                                var newConnectionPointPosition = caveNodePosition + (radiusToConnectionPort * newConnectionPointDirection);
                                var directionToOtherEndFromConnectionPoint = (otherEndPosition - newConnectionPointPosition).normalized;
                                var angleToOtherEndFromConnectionPoint = Vector2.Angle(newConnectionPointDirection, directionToOtherEndFromConnectionPoint);
                                
                                return angleToOtherEndFromConnectionPoint > angleTolerance;
                            });
                            //if it somehow gets through the whole range of degrees without finding one that breaks the tolerance (should be impossible?)
                            //it will return default, so check for that and set to final number
                            if(angleRange == default) {
                                angleRange = 360;
                            }
                            //the number returned will be the degree past the point of tolerance, so subtract 1
                            angleRange -= 1;

                            var lowerRotationBound = angleToRotateConnectionPortToAlignWithOtherEnd - angleRange;
                            var upperRotationBound = angleToRotateConnectionPortToAlignWithOtherEnd + angleRange;

                            //go thru and determine what the smallest possible range of rotation angles we can do to rotate the room is
                            if(lowerRotationBound > rotationBounds.Item1) {
                                rotationBounds.Item1 = lowerRotationBound;
                            }

                            if(upperRotationBound < rotationBounds.Item2) {
                                rotationBounds.Item2 = upperRotationBound;
                            }
                            
                            return rotationBounds;
                        });

                        //there is no rotation for the room to meet the the constraints on the current set of pairings, so continue
                        if(LowerRotationBound > UpperRotationBound) {
                            return minimalValidRotation;
                        }

                        //determine if newest valid rotation is less than the current one, if so update it
                        float midPointRotation = (LowerRotationBound + UpperRotationBound) / 2f;
                        if(Mathf.Abs(midPointRotation) < Mathf.Abs(minimalValidRotation)) {
                            edgeConnectionPortPairingsSet.ForEach(edgeConnectionPortPairing => {
                                roomWeightedPair.Value.EdgeToConnectionPortIndexMap[edgeConnectionPortPairing.Edge] = roomConnectionPorts.FindIndex(rcp => rcp == edgeConnectionPortPairing.ConnectionPort);
                            }); 
                            
                            return midPointRotation;
                        }

                        return minimalValidRotation;
                    });
                    
                    //this room has no valid rotation to make the connection ports work
                    if(float.IsPositiveInfinity(minimalValidRotation)) {
                        return false;
                    }

                    //set valid rooms rotation
                    roomWeightedPair.Value.RoomRotation = Quaternion.AngleAxis(minimalValidRotation, Vector3.up);
                    return true;
                }).ToList();

                var validWeightedRoomOptions = new WeightedValueOptions<SelectedRoom>() {
                    Options = validWeightedRoomPairs
                };

                //no valid room was found so set to default room
                if(validWeightedRoomOptions.Options.Count() <= 0) {
                    var roomPrefab = _caveGraphRenderParams.GetRandomDefaultRoomForType(caveNodeData.NodeType);
                    selectedRoom = new SelectedRoom(){
                        RoomPrefab = roomPrefab,
                        RoomScale = Vector3.one * caveNodeData.Scale,
                    };
                } else {
                    validWeightedRoomOptions.Normalize();
                    selectedRoom = validWeightedRoomOptions.RandomWithWeights();
                }

                // Spawn room
                GameObject newGameObject = GameObjectUtils.SafeInstantiate(instanceAsPrefabs, selectedRoom.RoomPrefab, mudRenderer.transform);
                newGameObject.transform.SetPositionAndRotation(localOrigin + caveNodeData.LocalPosition, selectedRoom.RoomRotation);
                newGameObject.transform.localScale = selectedRoom.RoomScale;
                caveNodeData.GameObject = newGameObject;

                var nodeColor = GetMaskColors(caveNodeData);
                if (nodeColor != Color.black)
                {
                    List<MudMaterial> materials = caveNodeData.GameObject.GetComponentsInChildren<MudMaterial>()?.ToList();
                    materials?.ForEach(m =>
                    {
                        m.Color = nodeColor;
                        m.ContributeMaterial = true;
                    });
                }
                
                var instancedRoomConnectionPorts = newGameObject.GetComponent<CaveGraphMudBunRoom>()?.ConnectionPorts;
                if(instancedRoomConnectionPorts != null && instancedRoomConnectionPorts.Count() > 0 && selectedRoom.EdgeToConnectionPortIndexMap != null) {
                    selectedRoom.EdgeToConnectionPortIndexMap.Keys.ToList().ForEach(edge => {
                        var connectionPort = instancedRoomConnectionPorts[selectedRoom.EdgeToConnectionPortIndexMap[edge]];
                        if(caveNodeData == edge.Source) {
                            edge.SourceConnectionPort = connectionPort;
                        } else {
                            edge.TargetConnectionPort = connectionPort;
                        }
                    });
                }

                // Assign reference to cave node data
                var caveNodeDataDebugComponent = newGameObject.GetComponent<CaveNodeDataDebugComponent>();
                if (caveNodeDataDebugComponent != null)
                {
                    caveNodeDataDebugComponent.CaveNodeData = caveNodeData;
                }
            }

            // Doing some of this work across multiple frames actually seems to be pretty important;
            // For one, the colliders which are placed with the rooms seem to need to do some initialization before their bounds correctly reflect their world position.
            yield return null;
            
            // Spawn "tunnel" on each edge to ensure nodes are connected
            bool first = true;
            foreach (var caveNodeConnectionData in _caveGraph.Edges)
            {
                
                var source = caveNodeConnectionData.Source;
                var target = caveNodeConnectionData.Target;
                var sourceConnectionPort = caveNodeConnectionData.SourceConnectionPort;
                var targetConnectionPort = caveNodeConnectionData.TargetConnectionPort;
                
                Vector3 sourceWorldPosition = sourceConnectionPort != null ? sourceConnectionPort.transform.position : _caveGenerator.LocalToWorld(source.LocalPosition);
                Vector3 targetWorldPosition = targetConnectionPort != null ? targetConnectionPort.transform.position : _caveGenerator.LocalToWorld(target.LocalPosition);
                
                // Calculate tunnel position
                var edgeDiff = (targetWorldPosition - sourceWorldPosition);
                var edgeDirFlattened = Vector3.ProjectOnPlane(edgeDiff, Vector3.up).normalized * 1f;
                var edgeMidPosition = sourceWorldPosition + edgeDiff / 2;
                var edgeRotation = Quaternion.LookRotation(edgeDiff);
                var edgeFlattenedRotation = Quaternion.LookRotation(edgeDirFlattened, Vector3.up);

                if (first)
                {
                    // first = false;

                    var sourceTestPosition = sourceWorldPosition + (sourceConnectionPort != null ? Vector3.zero : edgeDiff / 4f);
                    var targetTestPosition = targetWorldPosition - (targetConnectionPort != null ? Vector3.zero : edgeDiff / 4f);

                    // TODO prevent tunnels from cutting into rooms
                    // var sourceEdgePosition = source.LocalPosition;
                    // var sourceEdgePosition = source.BoundsColliders.ClosestPointOnBounds(edgeMidPosition);
                    
                    // NOTE: Using edgeRotation as the collider rotation for bounds check because rooms currently don't have
                    // very accurate room colliders and are just approximated by box. Can cause issues with
                    // non-symetrical colliders
                    var sourceEdgePosition = source.BoundsColliders.ClosestPointOnBounds(sourceTestPosition, edgeRotation);
                    // var sourceEdgePosition = source.BoundsColliders.ClosestPointOnBounds(sourceTestPosition, source.GameObject.transform.rotation);
                    var sourceColliderCenter = source.BoundsColliders.Select(coll => coll.bounds.center).Average();
                    sourceEdgePosition.y = sourceColliderCenter.y;
                    // sourceEdgePosition -= edgeDirFlattened;
                    sourceEdgePosition -= (edgeFlattenedRotation * _caveGraphRenderParams.TunnelConnectionOffset);
                    
                    // var targetEdgePosition = target.LocalPosition;
                    // var targetEdgePosition = target.BoundsColliders.ClosestPointOnBounds(edgeMidPosition);
                    
                    // NOTE: Using edgeRotation as the collider rotation for bounds check because rooms currently don't have
                    // very accurate room colliders and are just approximated by box. Can cause issues with
                    // non-symetrical colliders
                    var targetEdgePosition = target.BoundsColliders.ClosestPointOnBounds(targetTestPosition, edgeRotation);
                    // var targetEdgePosition = target.BoundsColliders.ClosestPointOnBounds(targetTestPosition, target.GameObject.transform.rotation);
                    var targetColliderCenter = target.BoundsColliders.Select(coll => coll.bounds.center).Average();
                    targetEdgePosition.y = targetColliderCenter.y;
                    // targetEdgePosition += edgeDirFlattened;
                    targetEdgePosition += (edgeFlattenedRotation * _caveGraphRenderParams.TunnelConnectionOffset);
                    // var targetColl = target.BoundsColliders.First();
                    // Debug.Log(
                    //     $"Collider Pos Target (Transform {targetColl.transform.position}) (Collider {targetColl.bounds.center}) (Node {target.LocalPosition})");
                    // var targetEdgePosition = target.BoundsColliders.First().transform.position;
                    // Debug.Log($"Local Position (Source {source.LocalPosition}) (Target {target.LocalPosition})");
                    // Debug.Log($"Closest Point (Source {sourceEdgePosition}) (Target {targetEdgePosition}) (Edge Mid {edgeMidPosition})");

                    edgeDiff = targetEdgePosition - sourceEdgePosition;
                    edgeMidPosition = sourceEdgePosition + edgeDiff / 2;
                    edgeRotation = Quaternion.LookRotation(edgeDiff);
                }

                var edgeLength = edgeDiff.magnitude;
                var localScale = new Vector3(caveNodeConnectionData.Radius, caveNodeConnectionData.Radius, edgeLength);
                // Debug.Log($"Edge length: EdgeLengthRaw {caveNodeConnectionData.Length} | Result Edge Length {edgeLength} | Source {caveNodeConnectionData.Source.Size} | Target {caveNodeConnectionData.Target.Size}");

                // Choose tunnel
                GameObject tunnelPrefab;
                
                if (caveNodeConnectionData.IsBlocked)
                {
                    bool sourceOrTargetIsChallengeRoom = source.NodeType == CaveNodeType.Challenge || target.NodeType == CaveNodeType.Challenge;
                    tunnelPrefab = sourceOrTargetIsChallengeRoom 
                        ? _caveGraphRenderParams.TunnelWithSlidingBarrierPrefab 
                        : _caveGraphRenderParams.TunnelWithBarrierPrefab;
#warning TODO find tunnel barrier game object
                    // caveNodeConnectionData.Barrier = ;
                }
                else if (caveNodeConnectionData.Length >= 20f)
                {
                    tunnelPrefab = _caveGraphRenderParams.TunnelLongPrefab;
                }
                else
                {
                    tunnelPrefab = _caveGraphRenderParams.TunnelPrefab;
                }
                
                // Spawn tunnel
                GameObject newGameObject = GameObjectUtils.SafeInstantiate(instanceAsPrefabs, tunnelPrefab, mudRenderer.transform);

                newGameObject.transform.SetPositionAndRotation(edgeMidPosition, edgeRotation);
                newGameObject.transform.localScale = localScale;
                caveNodeConnectionData.GameObject = newGameObject;

                var nodeColor = GetMaskColors(caveNodeConnectionData);
                if (nodeColor != Color.black)
                {
                    List<MudMaterial> materials = caveNodeConnectionData.GameObject.GetComponentsInChildren<MudMaterial>()?.ToList();
                    materials?.ForEach(m =>
                    {
                        m.Color = nodeColor;
                        m.ContributeMaterial = true;
                    });
                }
                
                // Assign reference to cave node connection data
                var caveNodeConnectionDataDebugComponent = newGameObject.GetComponent<CaveNodeConnectionDataDebugComponent>();
                if (caveNodeConnectionDataDebugComponent != null)
                {
                    caveNodeConnectionDataDebugComponent.CaveNodeConnectionData = caveNodeConnectionData;
                }
            }

            yield break;
        }

        protected override void GenerateMudBunInternal()
        {
            if (!_caveGenerator.IsGenerated)
            {
                Debug.LogWarning($"MudBun: Failed, cave graph is not generated");
                return;
            }
            
            base.GenerateMudBunInternal();
        }

        #endregion
    }
}