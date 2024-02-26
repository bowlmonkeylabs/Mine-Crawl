using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using BML.Utils.Random;
using DrawXXL;
using QuikGraph;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.CaveGraph
{
    [Serializable]
    public class CaveGraphV2 : QuikGraph.UndirectedGraph<CaveNodeData, CaveNodeConnectionData>
    {
        public CaveNodeData StartNode { get; set; }
        public CaveNodeData EndNode { get; set; }
        public CaveNodeData MerchantNode { get; set; }
        
        public List<CaveNodeConnectionData> MainPath { get; set; }

        public IEnumerable<ICaveNodeData> AllNodes => this.Vertices.Concat<ICaveNodeData>(this.Edges);

        public CaveGraphV2() : base(true)
        {
            
        }

        private CaveGraphV2(bool allowParallelEdges, EdgeEqualityComparer<CaveNodeData> edgeEqualityComparer) 
            : base(allowParallelEdges, edgeEqualityComparer)
        {
            
        }
        
        public CaveGraphV2(CaveGraphV2 graph) : this(graph.AllowParallelEdges, graph.EdgeEqualityComparer)
        {
            this.EdgeCapacity = graph.EdgeCapacity;
            this.AddVertexRange(graph.Vertices);
            this.AddEdgeRange(graph.Edges);
            this.StartNode = graph.StartNode;
            this.EndNode = graph.EndNode;
            this.MerchantNode = graph.MerchantNode;
        }

        public CaveNodeData GetRandomVertex(IEnumerable<CaveNodeData> excludeVertices = null)
        {
            var vertices = (excludeVertices != null)
                ? Vertices.Except(excludeVertices).ToList()
                : Vertices.ToList();
            if (!vertices.Any())
            {
                return null;
            }
            int randomIndex = Random.Range(0, vertices.Count);
            var randomVertex = vertices[randomIndex];
            return randomVertex;
        }
        
        public List<CaveNodeData> GetRandomVertices(
            int numVertices, 
            bool allowRepeats, 
            Func<CaveNodeData, bool> includeVertices = null
        ) {
            var vertices = (includeVertices != null)
                ? Vertices.Where(includeVertices).ToList()
                : Vertices.ToList();

            if (numVertices >= vertices.Count)
                return vertices;
            
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

        public CaveNodeData GetNormalWeightedRandomVertex(Vector3 nearPoint, float nearThreshold = 10f, IEnumerable<CaveNodeData> excludeVertices = null)
        {
            var candidateVertices = this.Vertices
                .Where(v => excludeVertices == null || !excludeVertices.Contains(v))
                .Select(v => (caveNodeData: v, dist: Vector3.Distance(nearPoint, v.LocalPosition)))
                .Where(v => v.dist <= nearThreshold)
                .OrderBy(v => v.dist)
                .ToList();
            var randomIndex = RandomUtils.RandomGaussian(0, candidateVertices.Count - 1);
            return candidateVertices[randomIndex].caveNodeData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <warning>Potentially expensive method invocation; Need to consider a more efficient implementation.</warning>
        /// <param name="localPosition"></param>
        /// <returns></returns>
        public CaveNodeData GetNearestVertex(Vector3 localPosition)
        {
            if (!Vertices.Any()) return null;

            var distances = Vertices
                .Select(vertex => (
                    vertex: vertex,
                    distance: (vertex.LocalPosition - localPosition).sqrMagnitude
                ))
                .OrderBy(tuple => tuple.distance)
                .AsParallel();
            var nearestVertex = distances.FirstOrDefault().vertex;
            return nearestVertex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <warning>Potentially expensive method invocation; Need to consider a more efficient implementation.</warning>
        /// <param name="position"></param>
        /// <returns></returns>
        public (ICaveNodeData nodeData, Vector3? closestPoint) FindContainingRoom(Vector3 position, float allowableRadius = -1f)
        {
            if (!AllNodes.Any()) return (null, null);
            
            bool foundExactRoom = false;
            (ICaveNodeData nodeData, float sqlDistanceToPoint, Vector3 closestPoint) closestRoomInfo 
                = (null, sqlDistanceToPoint: Mathf.Infinity, closestPoint: Vector3.zero);

            foreach (var nodeData in AllNodes)
            {
                if (nodeData.BoundsColliders.Count <= 0)
                { 
                    Debug.LogError($"Room object {nodeData.GameObject.name} is missing bounds collider!");
                }

                // Stop if contained by room
                Vector3 closestPoint = Vector3.zero;
                foreach (var collider in nodeData.BoundsColliders)
                {
                    bool isInCollider;
                    (isInCollider, closestPoint) = collider.IsPointInsideCollider(position);
                    if (isInCollider)
                    {
                        foundExactRoom = true;
                        closestRoomInfo.nodeData = nodeData;
                        closestRoomInfo.closestPoint = position;
                        closestRoomInfo.sqlDistanceToPoint = 0;
                        goto roomLoopEnd;
                    }
                }

                // Else see how far this room is from the current point. Keep track of closest room.
                float sqrDistanceToCollider = Vector3.SqrMagnitude(position - closestPoint);
                if (sqrDistanceToCollider < closestRoomInfo.sqlDistanceToPoint)
                {
                    closestRoomInfo.nodeData = nodeData;
                    closestRoomInfo.closestPoint = closestPoint;
                    closestRoomInfo.sqlDistanceToPoint = sqrDistanceToCollider;
                }
            }
            roomLoopEnd:

            if (allowableRadius < 0f || closestRoomInfo.sqlDistanceToPoint <= Mathf.Pow(allowableRadius, 2))
            {
                return (closestRoomInfo.nodeData, closestRoomInfo.closestPoint);
            }
            else
            {
                return (null, null);
            }
        }

        public void FloodFillDistance(IEnumerable<CaveNodeData> targetVertices, Action<CaveNodeData, int> updateStoredVertexDistance)
        {
            int distanceToTarget = 0;
            var verticesToCheck = new HashSet<CaveNodeData>(targetVertices);
            var verticesToCheckNext = new HashSet<CaveNodeData>();
            var checkedVertices = new HashSet<CaveNodeData>(verticesToCheck);

            while (verticesToCheck.Count > 0)
            {
                foreach (var caveNodeData in verticesToCheck)
                {
                    updateStoredVertexDistance(caveNodeData, distanceToTarget);

                    if (this.ContainsVertex(caveNodeData))
                    {
                        var adjacentVertices = this.AdjacentVertices(caveNodeData).Where(v => !checkedVertices.Contains(v));
                        verticesToCheckNext.AddRange(adjacentVertices);
                    }
                }
                
                verticesToCheck.Clear();
                verticesToCheck.AddRange(verticesToCheckNext);
                checkedVertices.AddRange(verticesToCheck);
                verticesToCheckNext.Clear();
                distanceToTarget += 1;
            }
        }
        
        public void DrawGizmos(Vector3 localOrigin, bool showMainPath, Color color, bool displayText = true, float lineWeight = 1f)
        {
            Gizmos.color = color;
            foreach (var caveNodeData in Vertices)
            {
                var worldPosition = localOrigin + caveNodeData.LocalPosition;
                var size = caveNodeData.Scale * 2;
                Color gizmoColor = color;
                if (this.IsAdjacentEdgesEmpty(caveNodeData))
                {
                    gizmoColor = Color.gray;
                    gizmoColor.a = 0.4f;
                }
                if (caveNodeData == StartNode) gizmoColor = Color.green;
                if (caveNodeData == EndNode) gizmoColor = Color.red;
                var adjacentVertices = this.AdjacentVertices(caveNodeData).ToList();
                bool isCandidate = adjacentVertices.Count == 1 
                                   && caveNodeData.MainPathDistance > 1
                                   && adjacentVertices.All(v => v.ObjectiveDistance < caveNodeData.ObjectiveDistance);
                // bool isCandidate = adjacentVertices.Count == 1;
                if (isCandidate)
                {
                    gizmoColor = Color.yellow;
                }
                
                Gizmos.color = gizmoColor;
                Gizmos.DrawSphere(worldPosition, size);
                // DrawShapes.Sphere(worldPosition, radius: size, color: gizmoColor);

                if (displayText)
                {
                    string type = "";
                    if (MerchantNode == caveNodeData) type = "Merchant";
                    if (StartNode == caveNodeData) type = "Start";
                    if (EndNode == caveNodeData) type = "End";
                    // DrawText.WriteFramed(caveNodeData.ToString(type), worldPosition - (Vector3.up * size * 2), size:size);
                }
            }
            
            Gizmos.color = color;
            foreach (var caveNodeConnectionData in Edges)
            {
                var sourceWorldPosition = localOrigin + caveNodeConnectionData.Source.LocalPosition;
                var targetWorldPosition = localOrigin + caveNodeConnectionData.Target.LocalPosition;
                // Gizmos.DrawLine(sourceWorldPosition, targetWorldPosition);
                
                DrawBasics.Line(sourceWorldPosition, targetWorldPosition, color: color, width: lineWeight);
            }
            
            if (showMainPath && MainPath != null)
            {
                Gizmos.color = Color.green;
                foreach (var caveNodeConnectionData in MainPath)
                {
                    var sourceWorldPosition = localOrigin + caveNodeConnectionData.Source.LocalPosition;
                    var targetWorldPosition = localOrigin + caveNodeConnectionData.Target.LocalPosition;
                    // Gizmos.DrawLine(sourceWorldPosition, targetWorldPosition);
                    
                    DrawBasics.Line(sourceWorldPosition, targetWorldPosition, color: Color.green, width: lineWeight);
                }
            }
        }
        
    }
}