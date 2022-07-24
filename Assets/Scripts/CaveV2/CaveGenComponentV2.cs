using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2.CaveGraph;
using UnityEngine;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.CaveV2.Util;
using BML.Scripts.CaveV2.Clayxel;
using BML.Scripts.CaveV2.SpawnObjects;
using BML.Scripts.Utils;
using GK;
using QuikGraph.Algorithms;
using Sirenix.OdinInspector;
using UnityEditor;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2
{
    [ExecuteInEditMode]
    public class CaveGenComponentV2 : MonoBehaviour
    {

        #region Inspector

        [SerializeField] private bool _generateOnChange;

        [SerializeField] private bool _retryOnFailure = true;
        [SerializeField] private int _maxRetryDepth = 3;

        [SerializeField] private bool _showTraversabilityCheck = false;
        
        [SerializeField, DisableIf("$_notOverrideBounds")] private Bounds _caveGenBounds = new Bounds(Vector3.zero, new Vector3(10,6,10));
        [SerializeField] private bool _overrideBounds = false;
        private bool _notOverrideBounds => !_overrideBounds;
        public Bounds CaveGenBounds => (_overrideBounds ? _caveGenBounds : _caveGenParams.PoissonBounds);

        [SerializeField] private IntReference _seedDebugReference;
        
        [Required] [InlineEditor, Space(10f)]
        [SerializeField] private CaveGenParameters _caveGenParams;
        
        [Required] [InlineEditor, Space(10f)]
        [SerializeField] private CaveGraphClayxelRenderer _caveGraphClayxelRenderer;

        [Required] [InlineEditor, Space(10f)]
        [SerializeField] private CaveGraphMudBunRenderer _caveGraphMudBunRenderer;

        [Required] [InlineEditor, Space(10f)]
        [SerializeField] private LevelObjectSpawner _levelObjectSpawner;
        
        #endregion

        #region Untiy Lifecycle

        private void Awake()
        {
            _seedDebugReference.Value = _caveGenParams.Seed;
        }

        #endregion
        
        #region Events
        
        public delegate void AfterGenerate();

        public AfterGenerate OnAfterGenerate;
        
        #endregion
        
        #region Cave generation

        public CaveGraphV2 CaveGraph => _caveGraph;
        [HideInInspector, SerializeField] private CaveGraphV2 _caveGraph;

        private int _retryDepth = 0;
        
        [PropertyOrder(-1)]
        [Button, LabelText("Generate Cave Graph")]
        private void GenerateCaveGraphButton()
        {
            _retryDepth = 0;
            GenerateCaveGraph();
        }
        
        private void GenerateCaveGraph(bool useRandomSeed = true)
        {
            DestroyCaveGraph();

            if (useRandomSeed)
            {
                _caveGenParams.UpdateRandomSeed();
            }
            _caveGraph = GenerateCaveGraph(_caveGenParams, CaveGenBounds);

            OnAfterGenerate?.Invoke();
        }

        private CaveGraphV2 RetryGenerateCaveGraph(bool checkRetryDepth = true)
        {
            if (checkRetryDepth) {
                _retryDepth++;
                if (_retryDepth >= _maxRetryDepth)
                {
                    throw new Exception($"Exceeded cave generation max retries ({_maxRetryDepth})");
                }
            }
            
            if (_caveGenParams.LockSeed || !_retryOnFailure)
            {
                throw new Exception("Cave generation failed; level is not traversable. To automatically resolve, ensure 'Retry on failure' is checked and 'Lock seed' is unchecked.");
            }
            else
            {
                _caveGenParams.UpdateRandomSeed(false);
                return GenerateCaveGraph(_caveGenParams, CaveGenBounds);
            }
        }

        [PropertyOrder(-1)]
        [Button]
        private void DestroyCaveGraph()
        {
            _caveGraph = null;
        }

        private CaveGraphV2 GenerateCaveGraph(CaveGenParameters caveGenParams, Bounds bounds)
        {
            Random.InitState(caveGenParams.Seed);
            
            var caveGraph = new CaveGraphV2();
            
            var poissonBoundsWithPadding = caveGenParams.GetBoundsWithPadding(bounds, CaveGenParameters.PaddingType.Inner);

            // Generate start node position
            var startBounds = new Bounds(
                poissonBoundsWithPadding.center + (poissonBoundsWithPadding.extents / 2),
                poissonBoundsWithPadding.extents / 2);
            var startPosition = RandomUtils.RandomInBounds(startBounds) + startBounds.center;
            startPosition = poissonBoundsWithPadding.max - poissonBoundsWithPadding.extents / 4;
            var startNode = new CaveNodeData(startPosition, 1f);
            caveGraph.AddVertex(startNode);
            caveGraph.StartNode = startNode;
            
            // Generate end node position
            var endBounds = new Bounds(
                poissonBoundsWithPadding.center - (poissonBoundsWithPadding.extents / 2),
                poissonBoundsWithPadding.extents / 2);
            var endPosition = RandomUtils.RandomInBounds(endBounds) + endBounds.center;
            endPosition = poissonBoundsWithPadding.min + poissonBoundsWithPadding.extents / 4;
            var endNode = new CaveNodeData(endPosition, 1f);
            caveGraph.AddVertex(endNode);
            caveGraph.EndNode = endNode;

            var initialSamples = new List<Vector3> { startPosition, endPosition };

            // Generate initial points for graph nodes with poisson distribution
            var poisson = new PoissonDiscSampler3D(poissonBoundsWithPadding.size, caveGenParams.PoissonSampleRadius);
            var samplePoints = poisson.Samples(initialSamples);
            var vertices = samplePoints.Where(point =>
            {
                return !initialSamples.Contains(point);
            }).Select(point =>
            {
                // var pointRelativeToBoundsCenter = (point - poissonBoundsWithPadding.size / 2);
                var pointRelativeToBoundsCenter = point;
                var size = Random.Range(caveGenParams.RoomScaling.x, caveGenParams.RoomScaling.y);
                var node = new CaveNodeData(pointRelativeToBoundsCenter, size);
                return node;
            });
            caveGraph.AddVertexRange(vertices);
            
            // Add an edge between every possible combination of nodes, and calculate the distance/cost
            var numVertices = caveGraph.Vertices.Count();
            if (numVertices > 300)
            {
                throw new Exception($"Cave graph has too many vertices ({numVertices}) for our inefficient adjacency calculation; consider revising this code in order to continue!");
            }
            var vertexCombinations =
                from v1 in caveGraph.Vertices
                from v2 in caveGraph.Vertices
                where v1 != v2
                select new {v1, v2};
            foreach (var vertexPair in vertexCombinations)
            {
                // Skip nodes which are already connected
                var foundInEdge = caveGraph.TryGetEdge(vertexPair.v1, vertexPair.v2, out var inEdge);
                var foundOutEdge = caveGraph.TryGetEdge(vertexPair.v2, vertexPair.v1, out var outEdge);
                if (foundInEdge || foundOutEdge)
                {
                    continue;
                }
                var edgeRadius = Math.Max(vertexPair.v1.Size, vertexPair.v2.Size);
                var edge = new CaveNodeConnectionData(vertexPair.v1, vertexPair.v2, edgeRadius);
                caveGraph.AddEdge(edge);
            }

            caveGraph.TryGetEdge(startNode, endNode, out var startEndEdge);
            if (startEndEdge != null)
            {
                caveGraph.RemoveEdge(startEndEdge);
            }
            
            // Remove long edges
            var maxLength = caveGenParams.PoissonSampleRadius * caveGenParams.MaxEdgeLengthFactor;
            caveGraph.RemoveEdgeIf(edge => edge.Length >= maxLength);
            
            // Remove steep edges
            var maxAngle = caveGenParams.MaxEdgeSteepnessAngle;
            caveGraph.RemoveEdgeIf(edge => edge.SteepnessAngle >= maxAngle);

            // Remove everything but the shortest path between start and end; the "main" path
            if (caveGenParams.OnlyShortestPathBetweenStartAndEnd)
            {
                var shortestPathFromStartFunc = caveGraph.ShortestPathsDijkstra(edge => edge.Length, startNode);
                shortestPathFromStartFunc(endNode, out var shortestPathFromStartToEnd);
                var shortestPathFromStartToEndList = shortestPathFromStartToEnd?.ToList();
                
                if (shortestPathFromStartToEndList != null)
                {
                    var keepEdges = shortestPathFromStartToEndList;
                    
                    // Offshoots from main path
                    if (caveGenParams.UseOffshootsFromMainPath)
                    {
                        for (int n = 0; n < caveGenParams.NumOffshoots; n++)
                        {
                            int randomOffshootPathIndex = Random.Range(1, shortestPathFromStartToEndList.Count - 1);
                            CaveNodeData offshootStart = shortestPathFromStartToEndList[randomOffshootPathIndex].Source;

                            for (int i = 0; i < caveGenParams.OffshootLength; i++)
                            {
                                var adjacentEdges = caveGraph.AdjacentEdges(offshootStart)
                                    .Where(edge =>
                                    {
                                        var edgeAreadyUsed = keepEdges.Contains(edge);

                                        CaveNodeData otherVertex;
                                        if (offshootStart == edge.Source) otherVertex = edge.Target;
                                        otherVertex = edge.Source;
                                        var vertexAlreadyUsed = keepEdges.Any(e =>
                                            e.Source == otherVertex || e.Target == otherVertex);
                                        return !edgeAreadyUsed && !vertexAlreadyUsed;
                                    })
                                    .ToList();
                                if (!adjacentEdges.Any())
                                    break;
                            
                                int randomEdgeIndex = Random.Range(0, adjacentEdges.Count);
                                var randomEdge = adjacentEdges[randomEdgeIndex];
                                keepEdges.Add(randomEdge);

                                if (offshootStart == randomEdge.Source) offshootStart = randomEdge.Target;
                                else if (offshootStart == randomEdge.Target) offshootStart = randomEdge.Source;
                                else break;
                            }
                        }
                    }
                    
                    // Remove all edges except what we want to keep
                    caveGraph.RemoveEdgeIf(edge => !keepEdges.Contains(edge));
                }
            }
            
            // Minimum spanning tree
            if (caveGenParams.MinimumSpanningTree)
            {
                var pointsOfInterest = caveGraph.GetRandomVertices(caveGenParams.MinimumSpanningNodes, false);
                var minimumGraph = new CaveGraphV2();
                minimumGraph.AddVertex(caveGraph.StartNode);
                minimumGraph.AddVertex(caveGraph.EndNode);
                minimumGraph.AddVertexRange(pointsOfInterest);
                var test = new DelaunayCalculator();
                var delaunayPoints = minimumGraph.Vertices.Select(v => v.LocalPosition.xz()).ToList();
                var triangulation = test.CalculateTriangulation(delaunayPoints);
                var triangulationCaveNodes = triangulation.Vertices
                    .Select(v2 =>
                        minimumGraph.Vertices.FirstOrDefault(caveNode =>
                            Mathf.Approximately(caveNode.LocalPosition.x, v2.x)
                            && Mathf.Approximately(caveNode.LocalPosition.z, v2.y)))
                    .ToList();
                var edges = new List<CaveNodeConnectionData>();
                for (int i = 0; i < triangulation.Triangles.Count; i += 3)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var sourceIndex = triangulation.Triangles[i+j];
                        var targetIndex = triangulation.Triangles[i+((j+1)%3)];
                        var sourceNode = triangulationCaveNodes[sourceIndex];
                        var targetNode = triangulationCaveNodes[targetIndex];
                        if (sourceNode == null || targetNode == null)
                        {
                            continue;
                        }

                        var edgeRadius = Math.Max(sourceNode.Size, targetNode.Size);
                        var edge = new CaveNodeConnectionData(sourceNode, targetNode, edgeRadius);
                        edges.Add(edge);
                    }
                }
                minimumGraph.AddEdgeRange(edges);
                var minimumSpanningTree = 
                    minimumGraph.MinimumSpanningTreeKruskal(
                        edge => edge.Length).ToList();
                minimumGraph.RemoveEdgeIf(edge => !minimumSpanningTree.Contains(edge));
                _minimumSpanningTreeGraphTEMP = minimumGraph;
                // TODO
            }
            
            // Remove orphaned nodes
            if (caveGenParams.RemoveOrphanNodes)
            {
                caveGraph.RemoveVertexIf(vertex =>
                {
                    return caveGraph.IsAdjacentEdgesEmpty(vertex)
                           && vertex != startNode && vertex != endNode;
                });
            }

            // Check traversability
            {
                var shortestPathFromStartFunc = caveGraph.ShortestPathsDijkstra(edge => edge.Length, startNode);
                shortestPathFromStartFunc(endNode, out var shortestPathFromStartToEnd);
                caveGraph.MainPath = shortestPathFromStartToEnd?.ToList();
                if (shortestPathFromStartToEnd == null)
                {
                    // Not traversable
                    if (_showTraversabilityCheck)
                    {
                        Debug.LogWarning($"Seed {caveGenParams.Seed} failed traversability check, retrying cave generation.");
                    }
                    
                    _retryDepth++;
                    if (_retryDepth >= _maxRetryDepth)
                    {
                        Debug.LogError($"Exceeded cave generation max retries ({_maxRetryDepth})");
                        return caveGraph;
                    }
                    return RetryGenerateCaveGraph(false);
                }
            }
            
            return caveGraph;
        }

        private CaveGraphV2 _minimumSpanningTreeGraphTEMP;
        
        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _caveGenParams.OnValidateEvent += OnValidate;
        }
        
        private void OnDisable()
        {
            _caveGenParams.OnValidateEvent -= OnValidate;
        }

        private void OnValidate()
        {
            if (_generateOnChange)
            {
                _retryDepth = 0;
                GenerateCaveGraph(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw generation bounds
            Gizmos.color = Color.white;
            var outerBounds = _caveGenParams.GetBoundsWithPadding(CaveGenBounds, CaveGenParameters.PaddingType.Outer);
            Gizmos.DrawWireCube(outerBounds.center, outerBounds.size);
            Gizmos.color = Color.gray;
            var poissonBoundsWithPadding = _caveGenParams.GetBoundsWithPadding(CaveGenBounds, CaveGenParameters.PaddingType.Inner);
            Gizmos.DrawWireCube(poissonBoundsWithPadding.center, poissonBoundsWithPadding.size);
            
            // Draw cave graph
            if (_caveGraph != null)
            {
                _caveGraph.DrawGizmos(LocalOrigin, _showTraversabilityCheck, Color.white);
            }
            if (_caveGenParams.MinimumSpanningTree && _minimumSpanningTreeGraphTEMP != null)
            {
                _minimumSpanningTreeGraphTEMP.DrawGizmos(
                    LocalOrigin + Vector3.up * _caveGenParams.PoissonBounds.size.y, 
                    _showTraversabilityCheck,
                    Color.yellow);
            }
            
            #if UNITY_EDITOR
            // Draw seed label
            Handles.Label(transform.position, _caveGenParams.Seed.ToString());
            #endif

        }

        #endregion

        #region Utils

        public Vector3 LocalOrigin => this.transform.position + CaveGenBounds.center;
        
        public Vector3 LocalToWorld(Vector3 localPosition)
        {
            return LocalOrigin + localPosition;
        }

        #endregion
    }
}