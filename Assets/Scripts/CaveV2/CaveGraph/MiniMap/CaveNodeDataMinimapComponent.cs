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

        [SerializeField] public ShapeRenderer OuterRenderer;

        #region Unity lifecycle

        private void Update()
        {
            UpdatePlayerOccupied();
        }

        #endregion

        private Color GetNodeColor(CaveNodeData caveNodeData)
        {          
            Color color;

            if (caveNodeData.PlayerVisited)
            {
                color = CaveGenerator.MinimapParameters.VisitedColor;
            }
            else
            {
                color = CaveGenerator.MinimapParameters.DefaultColor;
            }

            return color;
        }

        private void UpdatePlayerOccupied()
        {
            if (OuterRenderer != null && CaveGenerator?.MinimapParameters != null)
            {
                Color outerColor = GetNodeColor(CaveNodeData);
                OuterRenderer.Color = outerColor;
            }
        }
    }
}