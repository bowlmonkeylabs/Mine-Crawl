using System;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph
{
    public class CaveNodeConnectionDataDebugComponent : MonoBehaviour
    {
        [ReadOnly] public CaveNodeConnectionData CaveNodeConnectionData;
        [ReadOnly] public CaveGenComponentV2 CaveGenerator;
        [ReadOnly] public ShapeRenderer InnerRenderer;

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

        private Color GetNodeColor(CaveNodeConnectionData caveNodeConnectionData, CaveGenComponentV2.GizmoColorScheme colorScheme)
        {
            Color color;
            float fac;
            switch (colorScheme)
            {
                case CaveGenComponentV2.GizmoColorScheme.PlayerVisited:
                    if (caveNodeConnectionData.PlayerOccupied) color = CaveGenerator.DebugNodeColor_Occupied;
                    else if (caveNodeConnectionData.PlayerVisited) color = CaveGenerator.DebugNodeColor_Visited;
                    else color = CaveGenerator.DebugNodeColor_Default;
                    break;
                case CaveGenComponentV2.GizmoColorScheme.MainPath:
                    if (caveNodeConnectionData.MainPathDistance == 0) color = CaveGenerator.DebugNodeColor_MainPath;
                    else if (caveNodeConnectionData.ObjectiveDistance == 0) color = CaveGenerator.DebugNodeColor_End;
                    else color = CaveGenerator.DebugNodeColor_Default;
                    break;
                case CaveGenComponentV2.GizmoColorScheme.MainPathDistance:
                    fac = (float) caveNodeConnectionData.MainPathDistance / (float) CaveGenerator.MaxMainPathDistance;
                    color = CaveGenerator.DebugNodeColor_Gradient.Evaluate(fac);
                    break;
                case CaveGenComponentV2.GizmoColorScheme.ObjectiveDistance:
                    fac = (float) caveNodeConnectionData.ObjectiveDistance / (float) CaveGenerator.MaxObjectiveDistance;
                    color = CaveGenerator.DebugNodeColor_Gradient.Evaluate(fac);
                    break;
                case CaveGenComponentV2.GizmoColorScheme.PlayerDistance:
                    fac = (float) caveNodeConnectionData.PlayerDistance / (float) CaveGenerator.MaxPlayerDistance;
                    color = CaveGenerator.DebugNodeColor_Gradient.Evaluate(fac);
                    break;
                case CaveGenComponentV2.GizmoColorScheme.TorchInfluence:
                    fac = caveNodeConnectionData.TorchInfluence;
                    color = CaveGenerator.DebugNodeColor_Gradient.Evaluate(fac);
                    break;
                case CaveGenComponentV2.GizmoColorScheme.PlayerInfluence:
                    fac = caveNodeConnectionData.PlayerInfluence;
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
                Color innerColor = GetNodeColor(CaveNodeConnectionData, CaveGenerator.GizmoColorScheme_Inner);
                InnerRenderer.Color = innerColor;
            }

            // if (OuterRenderer != null)
            // {
            //     Color outerColor = GetNodeColor(CaveNodeConnectionData, CaveGenerator.GizmoColorScheme_Outer);
            //     OuterRenderer.Color = outerColor;
            // }
        }

        private void OnDrawGizmos()
        {
            if (!SelectionUtils.InSelection(this.transform)) return;
            
            Gizmos.color = Color.magenta;
            var rotationFlattened = Vector3.ProjectOnPlane(this.transform.forward, Vector3.up);
            var rotation = Quaternion.LookRotation(rotationFlattened, Vector3.up);
                            
            if (CaveNodeConnectionData.Source?.BoundsColliders != null)
            {
                foreach (var sourceBoundsCollider in CaveNodeConnectionData.Source.BoundsColliders)
                {
                    Gizmos.matrix = Matrix4x4.Translate(sourceBoundsCollider.bounds.center) * Matrix4x4.Rotate(rotation);
                    Gizmos.DrawWireCube(Vector3.zero, sourceBoundsCollider.bounds.size);
                }
            }
            
            if (CaveNodeConnectionData.Target?.BoundsColliders != null)
            {
                foreach (var targetBoundsCollider in CaveNodeConnectionData.Target.BoundsColliders)
                {
                    Gizmos.matrix = Matrix4x4.Translate(targetBoundsCollider.bounds.center) * Matrix4x4.Rotate(rotation);
                    Gizmos.DrawWireCube(Vector3.zero, targetBoundsCollider.bounds.size);
                }
            }

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}