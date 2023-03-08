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

        private Color GetNodeColor(CaveNodeConnectionData caveNodeConnectionData)
        {          
            Color color;
            
            bool isInBounds = MinimapController.IsInBounds(caveNodeConnectionData.Source.DirectPlayerDistance)
                              || MinimapController.IsInBounds(caveNodeConnectionData.Target.DirectPlayerDistance);
            if (MinimapController.MinimapParameters.RestrictMapRadius && !isInBounds)
            {
                color = CaveGenerator.MinimapParameters.CulledColor;
            }
            // else if (caveNodeConnectionData.PlayerOccupied)
            // {
            //     color = MinimapController.MinimapParameters.OccupiedColor;
            // }
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
            }

            // int maxViewDistance = 3;
            // float playerDistanceFac = Mathf.Clamp01((float) caveNodeData.PlayerDistance / (float)maxViewDistance);
            //
            // var isInBounds = MinimapController.IsInBounds(this.transform.position);
            // playerDistanceFac = (isInBounds.inBounds ? 1 : 0);
            //
            // float alpha = Mathf.Lerp(0.7f, 1f, 1f - playerDistanceFac);
            // color.a = alpha;

            return color;
        }

        public void UpdatePlayerOccupied()
        {
            if (Line != null && CaveGenerator?.MinimapParameters != null)
            {
                Color color = GetNodeColor(CaveNodeConnectionData);
                Line.Color = color;
            }
        }
    }
}