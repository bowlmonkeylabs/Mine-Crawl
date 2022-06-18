using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.CaveGraph
{
    [Serializable]
    public class CaveGraphV2 : QuikGraph.BidirectionalGraph<CaveNodeData, CaveNodeConnectionData>
    {
        public CaveNodeData StartNode { get; set; }
        public CaveNodeData EndNode { get; set; }

        public CaveGraphV2() : base(true)
        {
            
        }

        public CaveNodeData GetRandomVertex(IEnumerable<CaveNodeData> excludeVertices = null)
        {
            var vertices = (excludeVertices != null)
                ? Vertices.Except(excludeVertices).ToList()
                : Vertices.ToList();
            int randomIndex = Random.Range(0, vertices.Count);
            var randomVertex = vertices[randomIndex];
            return randomVertex;
        }
        
        public IEnumerable<CaveNodeData> GetRandomVertices(int numVertices, bool allowRepeats, IEnumerable<CaveNodeData> excludeVertices = null)
        {
            var vertices = (excludeVertices != null)
                ? Vertices.Except(excludeVertices).ToList()
                : Vertices.ToList();
            
            List<CaveNodeData> randomVertices = new List<CaveNodeData>();
            for (int i = 0; i < numVertices; i++)
            {
                int randomIndex = Random.Range(0, vertices.Count);
                var randomVertex = vertices[randomIndex];
                randomVertices.Add(randomVertex);
                
                if (!allowRepeats)
                {
                    vertices.RemoveAt(randomIndex);
                }
            }
            return randomVertices;
        }

        public void DrawGizmos(Vector3 localOrigin)
        {
            Gizmos.color = Color.white;
            foreach (var caveNodeData in Vertices)
            {
                var worldPosition = localOrigin + caveNodeData.LocalPosition;
                var size = caveNodeData.Size;
                var isStartNode = (caveNodeData == StartNode);
                Color gizmoColor = Color.white;
                if (caveNodeData == StartNode) gizmoColor = Color.green;
                if (caveNodeData == EndNode) gizmoColor = Color.red;
                Gizmos.color = gizmoColor;
                Gizmos.DrawSphere(worldPosition, size);
            }
            
            Gizmos.color = Color.white;
            foreach (var caveNodeConnectionData in Edges)
            {
                var sourceWorldPosition = localOrigin + caveNodeConnectionData.Source.LocalPosition;
                var targetWorldPosition = localOrigin + caveNodeConnectionData.Target.LocalPosition;
                Gizmos.DrawLine(sourceWorldPosition, targetWorldPosition);
            }
        }
    }
}