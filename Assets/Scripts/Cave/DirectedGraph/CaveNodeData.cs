using UnityEngine;

namespace BML.Scripts.Cave.DirectedGraph
{
    public class CaveNodeData
    {
        public Vector3 Position { get; private set; }
        public float Size { get; private set; }

        public CaveNodeData(Vector3 position, float size)
        {
            Position = position;
            Size = size;
        }
    }
    
    public class CaveNodeConnectionData
    {
        
    }
}