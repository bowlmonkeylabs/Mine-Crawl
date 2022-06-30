using BML.Scripts.Utils;
using QuikGraph;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph
{
    public class CaveNodeData
    {
        public Vector3 LocalPosition { get; private set; }
        public float Size { get; private set; }

        public CaveNodeData(Vector3 localPosition, float size)
        {
            LocalPosition = localPosition;
            Size = size;
        }
    }
    
    public class CaveNodeConnectionData : IEdge<CaveNodeData>
    {
        public CaveNodeData Source { get; private set; }
        public CaveNodeData Target { get; private set; }
        public float Radius { get; private set; }
        public float Length { get; private set; }
        public float SteepnessAngle { get; private set; }

        public CaveNodeConnectionData(CaveNodeData source, CaveNodeData target, float radius)
        {
            Source = source;
            Target = target;
            Radius = radius;
            Length = Vector3.Distance(source.LocalPosition, target.LocalPosition);
            
            var edgeDir = (target.LocalPosition - source.LocalPosition).normalized;
            if (edgeDir.y < 0)
            {
                edgeDir = -edgeDir;
            }
            var horizontalComponent = edgeDir.xoz().magnitude;
            var verticalComponent = edgeDir.y;
            SteepnessAngle = Mathf.Rad2Deg * Mathf.Atan2(verticalComponent, horizontalComponent);
        }
        
    }
    
}