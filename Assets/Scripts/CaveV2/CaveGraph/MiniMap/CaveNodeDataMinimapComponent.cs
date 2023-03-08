using System;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.CaveV2.CaveGraph.Minimap
{
    [ExecuteAlways]
    public class CaveNodeDataMinimapComponent : MonoBehaviour
    {
        [ReadOnly] public CaveNodeData CaveNodeData;
        [ReadOnly] public CaveGenComponentV2 CaveGenerator;
        [ReadOnly] public MinimapController MinimapController;

        [SerializeField] public ShapeRenderer OuterRenderer;

        #region Unity lifecycle

        private void Update()
        {
            // UpdatePlayerOccupied();
        }

        #endregion

        private Color GetNodeColor(CaveNodeData caveNodeData)
        {          
            Color color;
            
            bool isInBounds = MinimapController.IsInBounds(caveNodeData.DirectPlayerDistance);
            if (MinimapController.MinimapParameters.RestrictMapRadius && !isInBounds)
            {
                color = MinimapController.MinimapParameters.CulledColor;
            }
            else if (caveNodeData.PlayerOccupied)
            {
                color = MinimapController.MinimapParameters.OccupiedColor;
            }
            else if (caveNodeData.PlayerVisited)
            {
                color = MinimapController.MinimapParameters.VisitedColor;
            }
            else if (caveNodeData.PlayerVisitedAdjacent)
            {
                color = MinimapController.MinimapParameters.VisibleColor;
            }
            else
            {
                color = MinimapController.MinimapParameters.CulledColor;
            }

            return color;
        }

        public void UpdatePlayerOccupied()
        {
            if (OuterRenderer != null && CaveGenerator?.MinimapParameters != null)
            {
                Color outerColor = GetNodeColor(CaveNodeData);
                OuterRenderer.Color = outerColor;
            }
        }
    }
}