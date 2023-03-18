using System;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph.Minimap
{
    public class CaveNodeConnectionDataMinimapComponent : MonoBehaviour
    {
        [ReadOnly] public CaveNodeConnectionData CaveNodeConnectionData;
        [ReadOnly] public CaveGenComponentV2 CaveGenerator;
        [ReadOnly] public MinimapController MinimapController;

        [SerializeField] public Line Line;

        #region Unity lifecycle

        private void Update()
        {
            // UpdatePlayerOccupied();
        }

        private void OnDestroy()
        {
            MinimapController.MinimapParameters.OpenMapOverlay.Unsubscribe(UpdatePlayerOccupied);
        }

        #endregion

        public void Initialize(CaveNodeConnectionData caveNodeConnectionData, CaveGenComponentV2 caveGenerator, MinimapController minimapController)
        {
            CaveNodeConnectionData = caveNodeConnectionData;
            CaveGenerator = caveGenerator;
            MinimapController = minimapController;
            
            UpdateLinePosition();
            
            MinimapController.MinimapParameters.OpenMapOverlay.Unsubscribe(UpdatePlayerOccupied);
            MinimapController.MinimapParameters.OpenMapOverlay.Subscribe(UpdatePlayerOccupied);
        }

        private Color GetNodeColor(CaveNodeConnectionData caveNodeConnectionData)
        {          
            Color color;
            float? alphaOverride;
            
            float playerDistanceFac = 1f - Mathf.Clamp01((float)caveNodeConnectionData.PlayerDistance / 2f);
            alphaOverride = (playerDistanceFac * 0.9f) + 0.1f;
            
            if (!MinimapController.MinimapParameters.OpenMapOverlay.Value 
                && !(
                    MinimapController.IsInBounds(caveNodeConnectionData.Source.DirectPlayerDistance)
                    || MinimapController.IsInBounds(caveNodeConnectionData.Target.DirectPlayerDistance)
                )
            ) {
                color = CaveGenerator.MinimapParameters.CulledColor;
                alphaOverride = null;
            }
            else if (caveNodeConnectionData.PlayerOccupied)
            {
                color = MinimapController.MinimapParameters.OccupiedColor;
            }
            else if (caveNodeConnectionData.PlayerVisited)
            {
                color = MinimapController.MinimapParameters.VisitedColor;
            }
            else if (caveNodeConnectionData.PlayerVisitedAdjacent)
            {
                color = MinimapController.MinimapParameters.VisibleColor;
            }
            else
            {
                color = MinimapController.MinimapParameters.CulledColor;
                alphaOverride = null;
            }
            
            Color newColor = new Color(color.r, color.g, color.b, alphaOverride ?? color.a);
            return newColor;
            color.a = (alphaOverride ?? color.a);
            return color;
        }

        /// <summary>
        /// Returns 0 if node not visited, so that the tunnel is longer and more obvious.
        /// </summary>
        /// <param name="caveNodeData"></param>
        private float GetNodeDisplayScale(CaveNodeData caveNodeData)
        {
            return (!caveNodeData.PlayerVisitedAdjacent 
                ? 0 
                : MinimapController.MinimapParameters.GetNodeTypeRelativeScale(caveNodeData.NodeType));
        }
        
        public void UpdateLinePosition()
        {
            Vector3 sourceToTargetNormalized = (CaveNodeConnectionData.Target.LocalPosition - CaveNodeConnectionData.Source.LocalPosition).normalized;
            float sourceRelativeScale = GetNodeDisplayScale(CaveNodeConnectionData.Source);
            float targetRelativeScale = GetNodeDisplayScale(CaveNodeConnectionData.Target);
            Vector3 baseOffset = sourceToTargetNormalized * MinimapController.MinimapParameters.LineConnectionEndOffset; 
            Line.Start = CaveNodeConnectionData.Source.LocalPosition + (sourceRelativeScale * baseOffset);
            Line.End = CaveNodeConnectionData.Target.LocalPosition - (targetRelativeScale * baseOffset);
        }

        public void UpdatePlayerOccupied()
        {
            if (Line != null && CaveGenerator?.MinimapParameters != null)
            {
                Color color = GetNodeColor(CaveNodeConnectionData);
                Line.Color = color;

                // bool visited = CaveNodeConnectionData.PlayerVisited;
                bool visited = CaveNodeConnectionData.PlayerVisitedAllAdjacent;
                if (visited)
                {
                    Line.Dashed = false;
                }
                else
                {
                    Line.Dashed = true;
                }
                
                // TODO remove from update
                UpdateLinePosition();
            }
        }
    }
}