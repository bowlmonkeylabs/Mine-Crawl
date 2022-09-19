using System;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.CaveV2.CaveGraph
{
    [ExecuteAlways]
    public class CaveNodeDataDebugComponent : MonoBehaviour
    {
        [ReadOnly] public CaveNodeData CaveNodeData;
        [ReadOnly] public CaveGenComponentV2 CaveGenerator; 
        [ReadOnly] public ShapeRenderer InnerRenderer;
        [ReadOnly] public ShapeRenderer OuterRenderer;

        #region Unity lifecycle

        private void Awake()
        {
            InnerRenderer = GetComponent<ShapeRenderer>();
        }

        private void Update()
        {
            UpdatePlayerOccupied();
        }

        #endregion

        private Color GetNodeColor(CaveNodeData caveNodeData, CaveGenComponentV2.GizmoColorScheme colorScheme)
        {
            Color color;
            float fac;
            switch (colorScheme)
            {
                case CaveGenComponentV2.GizmoColorScheme.PlayerVisited:
                    if (CaveNodeData.PlayerOccupied) color = CaveGenerator.DebugNodeColor_Occupied;
                    else if (CaveNodeData.PlayerVisited) color = CaveGenerator.DebugNodeColor_Visited;
                    else color = CaveGenerator.DebugNodeColor_Default;
                    break;
                case CaveGenComponentV2.GizmoColorScheme.MainPath:
                    if (CaveNodeData.MainPathDistance == 0) color = CaveGenerator.DebugNodeColor_MainPath;
                    else if (CaveNodeData.ObjectiveDistance == 0) color = CaveGenerator.DebugNodeColor_End;
                    else color = CaveGenerator.DebugNodeColor_Default;
                    break;
                case CaveGenComponentV2.GizmoColorScheme.MainPathDistance:
                    fac = (float) caveNodeData.MainPathDistance / (float) CaveGenerator.MaxMainPathDistance;
                    color = CaveGenerator.DebugNodeColor_Gradient.Evaluate(fac);
                    break;
                case CaveGenComponentV2.GizmoColorScheme.ObjectiveDistance:
                    fac = (float) caveNodeData.ObjectiveDistance / (float) CaveGenerator.MaxObjectiveDistance;
                    color = CaveGenerator.DebugNodeColor_Gradient.Evaluate(fac);
                    break;
                case CaveGenComponentV2.GizmoColorScheme.PlayerDistance:
                    fac = (float) caveNodeData.PlayerDistance / (float) CaveGenerator.MaxPlayerDistance;
                    color = CaveGenerator.DebugNodeColor_Gradient.Evaluate(fac);
                    break;
                default:
                    color = CaveGenerator.DebugNodeColor_Default;
                    break;
            }

            return color;
        }

        private void UpdatePlayerOccupied()
        {
            // Debug.Log($"CaveNodeDataDebugComponent: UpdatePlayerOccupied");

            if (InnerRenderer != null)
            {
                Color innerColor = GetNodeColor(CaveNodeData, CaveGenerator.GizmoColorScheme_Inner);
                InnerRenderer.Color = innerColor;
            }

            if (OuterRenderer != null)
            {
                Color outerColor = GetNodeColor(CaveNodeData, CaveGenerator.GizmoColorScheme_Outer);
                OuterRenderer.Color = outerColor;
            }
        }
    }
}