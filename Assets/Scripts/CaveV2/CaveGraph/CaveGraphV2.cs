using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
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
        
        public IEnumerable<CaveNodeData> GetRandomVertices(int numVertices, bool allowRepeats, IEnumerable<CaveNodeData> excludeVertices = null)
        {
            var vertices = (excludeVertices != null)
                ? Vertices.Except(excludeVertices).ToList()
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

        public void FloodFillDistance(IEnumerable<ICaveNodeData> seeds, Action<ICaveNodeData, int> updateStoredDistance)
        {
            int distanceToTarget = 0;
            
            var toCheck = new HashSet<ICaveNodeData>(seeds); // TODO also add any edges with both ends in seeds
            var toCheckNext = new HashSet<ICaveNodeData>();
            var alreadyChecked = new HashSet<ICaveNodeData>(toCheck);

            while (toCheck.Count > 0)
            {
                foreach (var curr in toCheck)
                {
                    updateStoredDistance(curr, distanceToTarget);

                    if (curr is CaveNodeData node)
                    {
                        var adjacentEdges = this.AdjacentEdges(node)
                            .Where(v => !alreadyChecked.Contains(v));
                        toCheckNext.AddRange(adjacentEdges);
                    }
                    else if (curr is CaveNodeConnectionData connection)
                    {
                        if (!alreadyChecked.Contains(connection.Source)) toCheckNext.Add(connection.Source);
                        if (!alreadyChecked.Contains(connection.Target)) toCheckNext.Add(connection.Target);
                    }
                }
                
                toCheck.Clear();
                toCheck.AddRange(toCheckNext);
                alreadyChecked.AddRange(toCheck);
                toCheckNext.Clear();
                distanceToTarget += 1;
            }
        }

        public (bool success, IEnumerable<ICaveNodeData> path) ShortestPathToPlayer(ICaveNodeData caveNode)
        {
            if (caveNode.PlayerDistance == 0)
            {
                return (false, Enumerable.Empty<ICaveNodeData>());
            }
            
            var path = new List<ICaveNodeData>();
            
            ICaveNodeData next = caveNode;
            // List<ICaveNodeData> toCheckNext = new List<ICaveNodeData> { caveNode };
            while (true)
            {
                // var next = toCheckNext.First();
                path.Add(next);
                
                if (next.PlayerDistance == 0)
                {
                    return (true, path);
                    break;
                }

                if (next is CaveNodeData node)
                {
                    var adjacentEdges = this.AdjacentEdges(node)
                        .OrderBy(v => v.PlayerDistance);
                    // toCheckNext = adjacentEdges.ToList<ICaveNodeData>();
                    next = adjacentEdges.FirstOrDefault();
                }
                else if (next is CaveNodeConnectionData connection)
                {
                    bool sourceFirst = connection.Source.PlayerDistance < connection.Target.PlayerDistance;
                    var first = (sourceFirst ? connection.Source : connection.Target);
                    var second = (sourceFirst ? connection.Target : connection.Source);
                    // toCheckNext.Clear();
                    // if (!path.Contains(first) && first.PlayerDistance <= next.PlayerDistance) toCheckNext.Add(first);
                    // if (!path.Contains(second) && second.PlayerDistance <= next.PlayerDistance) toCheckNext.Add(second);
                    next = first;
                }

                if (next == null)
                {
                    Debug.Log("No available next node in path!");
                    return (false, path);
                    break;
                }

                // next = toCheckNext.First();
                if (path.Contains(next))
                {
                    Debug.Log("Path contains a cycle!");
                    return (false, path);
                    break;
                }
            }

            Debug.Log("No available next node in path!");
            return (false, path);
        }

        public (Vector3 localPosition, float volume, AnimationCurve customRolloffCurve, float maxDistance) AugmentAsCaveSound(Vector3 position, float volume, AnimationCurve customRolloffCurve, float maxDistance)
        {
            // find the cave room that the sound is in
            var (room, closestPoint) = this.FindContainingRoom(position); // TODO expensive!
            
            // if the sound is not in a room, do nothing
            // if the player is in the same room as the sound, do nothing
            if (room == null || room.PlayerDistance <= 1)
            {
                return (position, volume, customRolloffCurve, maxDistance);
            }
            
            // if the player is in a different room, adjust the sound location and strength
            // TODO this doesn't account for there being multiple paths to the player. does it matter?
            var (success, path) = this.ShortestPathToPlayer(room); // TODO check success?
            var pathList = path.ToList();
            Debug.Log($"path: {(success ? "" : "FAILED")} {pathList.Count()} {string.Join(",", pathList.Select(n => (n is CaveNodeData ? "Node" : (n is CaveNodeConnectionData ? "Connection" : ""))))}");
            var caveDistance = pathList.Count;
            var newRoomPathList = pathList.TakeWhile(caveNode => caveNode.PlayerDistance >= 1).ToList();
            var newRoom = newRoomPathList.Last();
            var newLocalPosition = newRoom.LocalPosition;

            float GetHalfSize(ICaveNodeData node)
            {
                if (node is CaveNodeData caveNodeData)
                {
                    return 0;
                }
                else if (node is CaveNodeConnectionData caveNodeConnectionData)
                {
                    return caveNodeConnectionData.Length / 2;
                }
                return 0;
            }

            float GetPathLength(List<ICaveNodeData> path)
            {
                // TODO if either end is a connection, only half the length is added
                float connectionsLength = path
                    .Where(node => node.IsConnection)
                    .Sum((conn) => (conn as CaveNodeConnectionData).Length);
                float removeLength = GetHalfSize(path.First()) + GetHalfSize(path.Last());
                return connectionsLength - removeLength;
            }

            AnimationCurve RasterizeAnimationCurve(AnimationCurve curve, int resolution = 30)
            {
                var newKeys = Enumerable.Range(0, resolution)
                    .Select(i =>
                    {
                        var time = (float)i / resolution;
                        var value = curve.Evaluate(time);
                        return new Keyframe(time, value);
                    })
                    .ToArray();
                var rasteredCurve = new AnimationCurve(newKeys);
                return rasteredCurve;
            } 

            var worldSpacePathLength = GetPathLength(pathList);

            var newRoomDistanceFromSource = GetPathLength(newRoomPathList);
            
            var curveTime = newRoomDistanceFromSource / maxDistance;
            var rolloffAtNewRoom = customRolloffCurve.Evaluate(curveTime);
            var slopeAtNewRoom = (customRolloffCurve.Evaluate(curveTime + 0.02f) - rolloffAtNewRoom) / 0.02f;
            var volumeAtNewRoom = rolloffAtNewRoom * volume;

            Debug.Log($"(Cave distance: {caveDistance}) (World distance: {worldSpacePathLength}) (New source distance: {newRoomDistanceFromSource}) (Curve time {curveTime}) (Rolloff: {rolloffAtNewRoom}) (Volume: {volumeAtNewRoom})");

            var newKeys = RasterizeAnimationCurve(customRolloffCurve).keys
                .Where(key => key.time >= curveTime)
                .Select(key => new Keyframe(key.time - curveTime, key.value, key.inTangent, key.outTangent, key.inWeight, key.outWeight))
                .Prepend(new Keyframe(0, rolloffAtNewRoom, slopeAtNewRoom, slopeAtNewRoom))
                .ToArray();
            
            var newCustomRolloffCurve = new AnimationCurve(newKeys);
            var newMaxDistance = maxDistance - newRoomDistanceFromSource;
            // var newMaxDistance = maxDistance;

            Debug.Log($"(New keys {newKeys.Length})");

            return (newLocalPosition, volume, newCustomRolloffCurve, newMaxDistance);
        }
        
        public void DrawGizmos(Vector3 localOrigin, bool showMainPath, Color color)
        {
            Gizmos.color = color;
            foreach (var caveNodeData in Vertices)
            {
                var worldPosition = localOrigin + caveNodeData.LocalPosition;
                var size = caveNodeData.Scale;
                Color gizmoColor = color;
                if (this.IsAdjacentEdgesEmpty(caveNodeData))
                {
                    gizmoColor = Color.gray;
                    gizmoColor.a = 0.4f;
                }
                if (caveNodeData == StartNode) gizmoColor = Color.green;
                if (caveNodeData == EndNode) gizmoColor = Color.red;
                Gizmos.color = gizmoColor;
                Gizmos.DrawSphere(worldPosition, size);
            }
            
            Gizmos.color = color;
            foreach (var caveNodeConnectionData in Edges)
            {
                var sourceWorldPosition = localOrigin + caveNodeConnectionData.Source.LocalPosition;
                var targetWorldPosition = localOrigin + caveNodeConnectionData.Target.LocalPosition;
                Gizmos.DrawLine(sourceWorldPosition, targetWorldPosition);
            }
            
            if (showMainPath && MainPath != null)
            {
                Gizmos.color = Color.green;
                foreach (var caveNodeConnectionData in MainPath)
                {
                    var sourceWorldPosition = localOrigin + caveNodeConnectionData.Source.LocalPosition;
                    var targetWorldPosition = localOrigin + caveNodeConnectionData.Target.LocalPosition;
                    Gizmos.DrawLine(sourceWorldPosition, targetWorldPosition);
                }
            }
        }
        
    }
}