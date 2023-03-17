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

        #endregion

        public void Initialize(CaveNodeConnectionData caveNodeConnectionData, CaveGenComponentV2 caveGenerator, MinimapController minimapController)
        {
            CaveNodeConnectionData = caveNodeConnectionData;
            CaveGenerator = caveGenerator;
            MinimapController = minimapController;
            
            UpdateLinePosition();
        }

        private Color GetNodeColor(CaveNodeConnectionData caveNodeConnectionData)
        {          
            Color color;
            float? alphaOverride;
            
            float playerDistanceFac = 1f - Mathf.Clamp01((float)caveNodeConnectionData.PlayerDistance / 2f);
            alphaOverride = (playerDistanceFac * 0.9f) + 0.1f;
            
            bool isInBounds = MinimapController.IsInBounds(caveNodeConnectionData.Source.DirectPlayerDistance)
                              || MinimapController.IsInBounds(caveNodeConnectionData.Target.DirectPlayerDistance);
            if (MinimapController.MinimapParameters.RestrictMapRadius && !isInBounds)
            {
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
            
            color.a = (alphaOverride ?? color.a);

            return color;
        }

        public void UpdateLinePosition()
        {
            Vector3 sourceToTargetNormalized = (CaveNodeConnectionData.Target.LocalPosition - CaveNodeConnectionData.Source.LocalPosition).normalized;
            float sourceRelativeScale = MinimapController.MinimapParameters.GetNodeTypeRelativeScale(CaveNodeConnectionData.Source.NodeType);
            float targetRelativeScale = MinimapController.MinimapParameters.GetNodeTypeRelativeScale(CaveNodeConnectionData.Target.NodeType);
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