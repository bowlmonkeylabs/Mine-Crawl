using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.CaveGraph
{
    [Serializable]
    public class CaveGraphV2 : QuikGraph.BidirectionalGraph<CaveNodeData, CaveNodeConnectionData>
    {
        public CaveNodeData StartNode { get; set; }

        public CaveGraphV2() : base(true)
        {
            
        }

        public CaveNodeData GetRandomVertex()
        {
            var vertices = Vertices.ToList();
            var randomIndex = Random.Range(0, vertices.Count);
            var randomVertex = vertices[randomIndex];
            return randomVertex;
        }

        public void DrawGizmos(Vector3 localOrigin)
        {
            Gizmos.color = Color.white;
            foreach (var caveNodeData in Vertices)
            {
                var worldPosition = localOrigin + caveNodeData.LocalPosition;
                var size = caveNodeData.Size / 10f;
                var isStartNode = (caveNodeData == StartNode);
                Gizmos.color = (isStartNode ? Color.green : Color.white);
                Gizmos.DrawSphere(worldPosition, size);
            }
            
            foreach (var caveNodeConnectionData in Edges)
            {
                var sourceWorldPosition = localOrigin + caveNodeConnectionData.Source.LocalPosition;
                var targetWorldPosition = localOrigin + caveNodeConnectionData.Target.LocalPosition;
                Gizmos.DrawLine(sourceWorldPosition, targetWorldPosition);
            }
        }
    }
}