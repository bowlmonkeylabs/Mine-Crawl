using System;
using BML.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector.Editor.Drawers;
using UnityEngine;

namespace BML.Scripts.CaveV2.MudBun
{
    public class CaveNodeConnectionPort : MonoBehaviour
    {
        #region Inspector

        [SerializeField, Range(0, 360)] private float _angleRangeHorizontal;
        [SerializeField, Range(0, 360)] private float _angleRangeVertical;
        private float _projectionDistance = 2f;

        #endregion

        #region Unity lifecycle

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            var transformCached = this.transform;
            var checkTransforms = new Transform[] { transformCached, transformCached.parent };
            if (SelectionUtils.InSelection(checkTransforms))
            {
                var thickness = 0.08f;
                
                void DrawAngleRange(Vector2 angleRange, float distance, float thickness)
                {
                    var yMin = Quaternion.AngleAxis(-angleRange.y/2, Vector3.right) * Vector3.forward * distance;
                    var yMax = Quaternion.AngleAxis(angleRange.y/2, Vector3.right) * Vector3.forward * distance;
                    Draw.Color = Color.red;
                    Draw.Line(Vector3.zero, yMin, thickness);
                    Draw.Line(Vector3.zero, yMax, thickness);
                    Draw.ArcDashed(
                        Vector3.zero,
                        Quaternion.LookRotation(Vector3.left, Vector3.forward),
                        DashStyle.DefaultDashStyleLine, 
                        distance,
                        0.1f,
                        Mathf.Deg2Rad * (90 - angleRange.y/2), 
                        Mathf.Deg2Rad * (angleRange.y + 90 - angleRange.y/2));
                    
                    var xMin = Quaternion.AngleAxis(-angleRange.x/2, Vector3.up) * Vector3.forward * distance;
                    var xMax = Quaternion.AngleAxis(angleRange.x/2, Vector3.up) * Vector3.forward * distance;
                    Draw.Color = Color.green;
                    Draw.Line(Vector3.zero, xMin, thickness);
                    Draw.Line(Vector3.zero, xMax, thickness);
                    Draw.ArcDashed(
                        Vector3.zero,
                        Quaternion.LookRotation(Vector3.down, Vector3.forward),
                        DashStyle.DefaultDashStyleLine, 
                        distance,
                        0.1f,
                        Mathf.Deg2Rad * (90 - angleRange.x/2), 
                        Mathf.Deg2Rad * (angleRange.x + 90 - angleRange.x/2));
                }
                
                Draw.Matrix = transformCached.localToWorldMatrix;

                var temp = Vector3.forward * _projectionDistance * 1.2f;
                Draw.Color = Color.white;
                Draw.Line(Vector3.zero, temp);
                Draw.Cone(temp, thickness, thickness);
                
                DrawAngleRange(new Vector2(_angleRangeHorizontal, _angleRangeVertical), _projectionDistance, thickness);
                Draw.Matrix = Matrix4x4.identity;
            }
#endif
        }

        #endregion
    }
}