using System;
using System.Linq;
using BML.Scripts.CaveV2;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.Utils;
using CaveV2.Clayxel;
using CaveV2.MudBun;
using Clayxels;
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

        private void OnEnable()
        {
            _caveGenerator.OnAfterGenerate += TryGenerateWithCooldown;
        }

        private void OnDisable()
        {
            _caveGenerator.OnAfterGenerate -= TryGenerateWithCooldown;
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
                // Spawn room
                GameObject newGameObject = GameObjectUtils.SafeInstantiate(instanceAsPrefabs, _caveGraphRenderParams.TunnelPrefab, mudRenderer.transform);
                var edgeDiff = (caveNodeConnectionData.Target.LocalPosition -
                                caveNodeConnectionData.Source.LocalPosition);
                var edgeMidPosition = caveNodeConnectionData.Source.LocalPosition + edgeDiff / 2;
                var edgeRotation = Quaternion.LookRotation(edgeDiff);
                var localScale = new Vector3(1f, 1f, caveNodeConnectionData.Length);
                newGameObject.transform.SetPositionAndRotation(edgeMidPosition, edgeRotation);
                newGameObject.transform.localScale = localScale;
            }

            // TODO find out how to update the Clayxels without the game object selected; none of the below seem to update it
            // clayContainer.needsUpdate = true;
            // clayContainer.scanClayObjectsHierarchy();
            // clayContainer.forceUpdateAllSolids();
            // clayContainer.solidUpdated(0);
            // clayContainer.computeClay();
        }

        #endregion
        
    }
}