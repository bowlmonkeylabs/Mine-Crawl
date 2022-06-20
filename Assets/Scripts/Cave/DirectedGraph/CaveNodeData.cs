using UnityEngine;

namespace BML.Scripts.Cave.DirectedGraph
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
    
    public class CaveNodeConnectionData
    {
        public float Radius { get; private set; }

        public CaveNodeConnectionData(float radius)
        {
            Radius = radius;
        }
    }
}