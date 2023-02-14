using System;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph
{
    public class CaveNodeConnectionDataDebugComponent : MonoBehaviour
    {
        [ReadOnly] public CaveNodeConnectionData CaveNodeConnectionData;
        [ReadOnly] public CaveGenComponentV2 CaveGenerator;

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