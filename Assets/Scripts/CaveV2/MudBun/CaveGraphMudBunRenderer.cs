using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.Utils;
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

        private const int STEP_ID = 2;

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
            
            Random.InitState(_caveGenerator.CaveGenParams.Seed + STEP_ID);

            var localOrigin = mudRenderer.transform.position;
            
            // Spawn "rooms" at each cave node
            foreach (var caveNodeData in _caveGraph.Vertices)
            {
                bool changeNodeColor = true;
                
                // Select room to spawn
                GameObject roomPrefab;
                Vector3 roomScale;
                if (caveNodeData == _caveGraph.StartNode &&
                    !_caveGraphRenderParams.StartRoomPrefab.SafeIsUnityNull())
                {
                    roomPrefab = _caveGraphRenderParams.StartRoomPrefab;
                    roomScale = Vector3.one;
                    changeNodeColor = false;
                }
                else if (caveNodeData == _caveGraph.EndNode &&
                         !_caveGraphRenderParams.EndRoomPrefab.SafeIsUnityNull())
                {
                    roomPrefab = _caveGraphRenderParams.EndRoomPrefab;
                    roomScale = Vector3.one;
                }
                else if(caveNodeData == _caveGraph.MerchantNode &&
                        !_caveGraphRenderParams.MerchantRoomPrefab.SafeIsUnityNull())
                {
                    roomPrefab = _caveGraphRenderParams.MerchantRoomPrefab;
                    roomScale = Vector3.one;
                    changeNodeColor = false;
                }
                else
                {
                    // TODO systematically choose which rooms to spawn
                    roomPrefab = _caveGraphRenderParams.GetRandomRoom(caveNodeData.NodeType);
                    roomScale = Vector3.one * caveNodeData.Scale;
                }

                // Spawn room
                GameObject newGameObject = GameObjectUtils.SafeInstantiate(instanceAsPrefabs, roomPrefab, mudRenderer.transform);
                newGameObject.transform.position = localOrigin + caveNodeData.LocalPosition;
                newGameObject.transform.localScale = roomScale;
                caveNodeData.GameObject = newGameObject;

                // Assign reference to cave node data
                var caveNodeDataDebugComponent = newGameObject.GetComponent<CaveNodeDataDebugComponent>();
                if (caveNodeDataDebugComponent != null)
                {
                    caveNodeDataDebugComponent.CaveNodeData = caveNodeData;
                    if (changeNodeColor)
                    {
                        List<MudMaterial> materials = caveNodeData.GameObject.GetComponentsInChildren<MudMaterial>()?.ToList();
                        materials?.ForEach(m =>
                        {
                            m.Color *= _caveGraphRenderParams.CaveColor;
                        });
                    }
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
                var sourceWorldPosition = _caveGenerator.LocalToWorld(source.LocalPosition);
                var targetWorldPosition = _caveGenerator.LocalToWorld(target.LocalPosition);
                
                // Calculate tunnel position
                var edgeDiff = (targetWorldPosition - sourceWorldPosition);
                var edgeDirFlattened = Vector3.ProjectOnPlane(edgeDiff, Vector3.up).normalized * 1f;
                var edgeMidPosition = sourceWorldPosition + edgeDiff / 2;
                var edgeRotation = Quaternion.LookRotation(edgeDiff);
                var edgeFlattenedRotation = Quaternion.LookRotation(edgeDirFlattened, Vector3.up);

                if (first)
                {
                    // first = false;

                    var sourceTestPosition = sourceWorldPosition + edgeDiff / 4;
                    var targetTestPosition = targetWorldPosition - edgeDiff / 4;
                    // TODO prevent tunnels from cutting into rooms
                    // var sourceEdgePosition = source.LocalPosition;
                    // var sourceEdgePosition = source.BoundsColliders.ClosestPointOnBounds(edgeMidPosition);
                    var sourceEdgePosition = source.BoundsColliders.ClosestPointOnBounds(sourceTestPosition, edgeRotation);
                    var sourceColliderCenter = source.BoundsColliders.Select(coll => coll.bounds.center).Average();
                    sourceEdgePosition.y = sourceColliderCenter.y;
                    // sourceEdgePosition -= edgeDirFlattened;
                    sourceEdgePosition -= (edgeFlattenedRotation * _caveGraphRenderParams.TunnelConnectionOffset);
                    
                    // var targetEdgePosition = target.LocalPosition;
                    // var targetEdgePosition = target.BoundsColliders.ClosestPointOnBounds(edgeMidPosition);
                    var targetEdgePosition = target.BoundsColliders.ClosestPointOnBounds(targetTestPosition, edgeRotation);
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

                    edgeDiff = (targetEdgePosition - sourceEdgePosition);
                    edgeMidPosition = sourceEdgePosition + edgeDiff / 2;
                    edgeRotation = Quaternion.LookRotation(edgeDiff);
                }

                var edgeLength = edgeDiff.magnitude;
                var localScale = new Vector3(caveNodeConnectionData.Radius, caveNodeConnectionData.Radius, edgeLength);
                // Debug.Log($"Edge length: EdgeLengthRaw {caveNodeConnectionData.Length} | Result Edge Length {edgeLength} | Source {caveNodeConnectionData.Source.Size} | Target {caveNodeConnectionData.Target.Size}");

                // Spawn tunnel
                GameObject newGameObject;

                if (caveNodeConnectionData.IsBlocked)
                {
                    newGameObject = GameObjectUtils.SafeInstantiate(instanceAsPrefabs, _caveGraphRenderParams.TunnelWithBarrierPrefab, mudRenderer.transform);
                    #warning TODO find tunnel barrier game object
                    // caveNodeConnectionData.Barrier = ;
                }
                else
                    newGameObject = GameObjectUtils.SafeInstantiate(instanceAsPrefabs, _caveGraphRenderParams.TunnelPrefab, mudRenderer.transform);
                
                newGameObject.transform.SetPositionAndRotation(edgeMidPosition, edgeRotation);
                newGameObject.transform.localScale = localScale;
                caveNodeConnectionData.GameObject = newGameObject;
                
                // Assign reference to cave node connection data
                var caveNodeConnectionDataDebugComponent = newGameObject.GetComponent<CaveNodeConnectionDataDebugComponent>();
                if (caveNodeConnectionDataDebugComponent != null)
                {
                    caveNodeConnectionDataDebugComponent.CaveNodeConnectionData = caveNodeConnectionData;

                    List<MudMaterial> materials = caveNodeConnectionData.GameObject.GetComponentsInChildren<MudMaterial>()?.ToList();
                    materials?.ForEach(m =>
                    {
                        m.Color *= _caveGraphRenderParams.CaveColor;
                    });
                    
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