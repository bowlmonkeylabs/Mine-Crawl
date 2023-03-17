using System;
using BML.ScriptableObjectCore.Scripts.Variables.VariableOperators;
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

        [SerializeField] public Torus UndiscoveredRenderer;
        [FormerlySerializedAs("OuterRenderer")] [SerializeField] public Sphere DiscoveredRenderer;

        #region Unity lifecycle

        private void Update()
        {
            // UpdatePlayerOccupied();
        }

        #endregion

        private float initialRadius;
        public void Initialize(CaveNodeData caveNodeData, CaveGenComponentV2 caveGenerator, MinimapController minimapController)
        {
            CaveNodeData = caveNodeData;
            CaveGenerator = caveGenerator;
            MinimapController = minimapController;

            if (DiscoveredRenderer != null
                && UndiscoveredRenderer != null)
            {
                initialRadius = DiscoveredRenderer.Radius;
                UpdateRendererRadius();
            }
        }

        private Color GetNodeColor(CaveNodeData caveNodeData)
        {          
            Color color;
            float? alphaOverride;
            
            float playerDistanceFac = 1f - Mathf.Clamp01((float)caveNodeData.PlayerDistance / 2f);
            alphaOverride = (playerDistanceFac * 0.9f) + 0.1f;
            
            bool isInBounds = MinimapController.IsInBounds(caveNodeData.DirectPlayerDistance);
            if (MinimapController.MinimapParameters.RestrictMapRadius && !isInBounds)
            {
                color = MinimapController.MinimapParameters.CulledColor;
                alphaOverride = null;
            }
            else if (caveNodeData.PlayerVisited && caveNodeData == CaveGenerator.CaveGraph.StartNode)
            {
                color = MinimapController.MinimapParameters.Color_StartRoom;
            }
            else if (caveNodeData.PlayerVisited && caveNodeData == CaveGenerator.CaveGraph.EndNode)
            {
                color = MinimapController.MinimapParameters.Color_EndRoom;
            }
            else if (caveNodeData.PlayerVisited && caveNodeData == CaveGenerator.CaveGraph.MerchantNode)
            {
                color = MinimapController.MinimapParameters.Color_MerchantRoom;
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
                alphaOverride = null;
            }

            Color newColor = new Color(color.r, color.g, color.b, alphaOverride ?? color.a);
            Debug.Log($"(Player distance fac {playerDistanceFac}) (Alpha {alphaOverride}) (Color {newColor})");
            return newColor;
        }

        private void UpdateRendererRadius()
        {
            if (DiscoveredRenderer != null
                && UndiscoveredRenderer != null)
            {
                float relativeScale = MinimapController.MinimapParameters.GetNodeTypeRelativeScale(CaveNodeData.NodeType);
                float radius = initialRadius * relativeScale;
                DiscoveredRenderer.Radius = radius;
                UndiscoveredRenderer.Radius = radius;
            }
        }

        public void UpdatePlayerOccupied()
        {
            if (DiscoveredRenderer != null
                && UndiscoveredRenderer != null
                && CaveGenerator?.MinimapParameters != null)
            {
                Color outerColor = GetNodeColor(CaveNodeData);
                if (CaveNodeData.PlayerVisited)
                {
                    UndiscoveredRenderer.enabled = false;
                    DiscoveredRenderer.enabled = true;
                    DiscoveredRenderer.Color = outerColor;
                }
                else
                {
                    DiscoveredRenderer.enabled = false;
                    UndiscoveredRenderer.enabled = true;
                    UndiscoveredRenderer.Color = outerColor;
                }
                
                // TODO remove from update
                UpdateRendererRadius();
            }
        }
    }
}