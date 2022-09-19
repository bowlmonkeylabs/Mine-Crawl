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

        private void UpdatePlayerOccupied()
        {
            // Debug.Log($"CaveNodeDataDebugComponent: UpdatePlayerOccupied");

            if (InnerRenderer != null)
            {
                Color innerColor;
                if (CaveNodeData.PlayerOccupied) innerColor = CaveGenerator.DebugNodeColor_Occupied;
                else if (CaveNodeData.PlayerVisited) innerColor = CaveGenerator.DebugNodeColor_Visited;
                else innerColor = CaveGenerator.DebugNodeColor_Default;
                InnerRenderer.Color = innerColor;
            }

            if (OuterRenderer != null)
            {
                Color outerColor;
                if (CaveNodeData.MainPathDistance == 0) outerColor = CaveGenerator.DebugNodeColor_MainPath;
                else if (CaveNodeData.ObjectiveDistance == 0) outerColor = CaveGenerator.DebugNodeColor_End;
                else outerColor = CaveGenerator.DebugNodeColor_Default;
                OuterRenderer.Color = outerColor;
            }
        }
    }
}