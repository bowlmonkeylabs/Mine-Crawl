﻿using System;
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

        protected override void GenerateMudBunInternal(
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
            
            // Spawn "tunnel" on each edge to ensure nodes are connected
            foreach (var caveNodeConnectionData in _caveGraph.Edges)
            {
                var source = caveNodeConnectionData.Source;
                var target = caveNodeConnectionData.Target;
                
                // Calculate tunnel position
                var sourceTargetDiff = (target.LocalPosition - source.LocalPosition);
                var sourceTargetDiffProjectedToGroundNormalized = Vector3.ProjectOnPlane(sourceTargetDiff, Vector3.up).normalized;
                var sourceEdgePosition = source.LocalPosition +
                                         (source.Scale / 2 * _caveGraphRenderParams.BaseRoomRadius * sourceTargetDiffProjectedToGroundNormalized);
                var targetEdgePosition = target.LocalPosition +
                                         (target.Scale / 2 * _caveGraphRenderParams.BaseRoomRadius * -1 * sourceTargetDiffProjectedToGroundNormalized);
                
                var edgeDiff = (targetEdgePosition - sourceEdgePosition);
                var edgeMidPosition = source.LocalPosition + edgeDiff / 2;
                var edgeRotation = Quaternion.LookRotation(edgeDiff);
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