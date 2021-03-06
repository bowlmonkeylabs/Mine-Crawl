using System;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.Utils;
using MudBun;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

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
            _caveGenerator.OnAfterGenerate += TryGenerateWithCooldown;
            _caveGraphRenderParams.OnValidateEvent += TryGenerateWithCooldown;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _caveGenerator.OnAfterGenerate -= TryGenerateWithCooldown;
            _caveGraphRenderParams.OnValidateEvent -= TryGenerateWithCooldown;
        }

        #endregion

        #region MudBun

        protected override void GenerateMudBun(
            MudRenderer mudRenderer,
            bool instanceAsPrefabs
        ) {
            base.GenerateMudBun(mudRenderer, instanceAsPrefabs);

            var localOrigin = mudRenderer.transform.position;

            // Spawn "rooms" at each cave node
            foreach (var caveNodeData in _caveGraph.Vertices)
            {
                // Select room to spawn
                GameObject roomPrefab;
                Vector3 roomScale;
                if (caveNodeData == _caveGraph.StartNode &&
                    !_caveGraphRenderParams.StartRoomPrefab.SafeIsUnityNull())
                {
                    roomPrefab = _caveGraphRenderParams.StartRoomPrefab;
                    roomScale = Vector3.one;
                }
                else if (caveNodeData == _caveGraph.EndNode &&
                         !_caveGraphRenderParams.EndRoomPrefab.SafeIsUnityNull())
                {
                    roomPrefab = _caveGraphRenderParams.EndRoomPrefab;
                    roomScale = Vector3.one;
                }
                else
                {
                    // TODO systematically choose which rooms to spawn
                    roomPrefab = _caveGraphRenderParams.GetRandomRoom();
                    roomScale = Vector3.one * caveNodeData.Size;
                }

                // Spawn room
                GameObject newGameObject = GameObjectUtils.SafeInstantiate(instanceAsPrefabs, roomPrefab, mudRenderer.transform);
                newGameObject.transform.position = localOrigin + caveNodeData.LocalPosition;
                newGameObject.transform.localScale = roomScale;
            }
            
            // Spawn "tunnel" on each edge to ensure nodes are connected
            foreach (var caveNodeConnectionData in _caveGraph.Edges)
            {
                // Calculate tunnel position
                var sourceTargetDiff = (caveNodeConnectionData.Target.LocalPosition -
                                        caveNodeConnectionData.Source.LocalPosition);
                var sourceTargetDiffProjectedToGroundNormalized = Vector3.ProjectOnPlane(sourceTargetDiff, Vector3.up).normalized;
                var sourceEdgePosition = caveNodeConnectionData.Source.LocalPosition +
                                         (caveNodeConnectionData.Source.Size / 2 * _caveGraphRenderParams.BaseRoomRadius * sourceTargetDiffProjectedToGroundNormalized);
                var targetEdgePosition = caveNodeConnectionData.Target.LocalPosition +
                                         (caveNodeConnectionData.Target.Size / 2 * _caveGraphRenderParams.BaseRoomRadius * -1 * sourceTargetDiffProjectedToGroundNormalized);
                
                var edgeDiff = (targetEdgePosition - sourceEdgePosition);
                var edgeMidPosition = caveNodeConnectionData.Source.LocalPosition + edgeDiff / 2;
                var edgeRotation = Quaternion.LookRotation(edgeDiff);
                var edgeLength = edgeDiff.magnitude;
                var localScale = new Vector3(1f, 1f, edgeLength);
                // Debug.Log($"Edge length: EdgeLengthRaw {caveNodeConnectionData.Length} | Result Edge Length {edgeLength} | Source {caveNodeConnectionData.Source.Size} | Target {caveNodeConnectionData.Target.Size}");

                // Spawn tunnel
                GameObject newGameObject = GameObjectUtils.SafeInstantiate(instanceAsPrefabs, _caveGraphRenderParams.TunnelPrefab, mudRenderer.transform);
                newGameObject.transform.SetPositionAndRotation(edgeMidPosition, edgeRotation);
                newGameObject.transform.localScale = localScale;
            }
        }

        #endregion
        
    }
}