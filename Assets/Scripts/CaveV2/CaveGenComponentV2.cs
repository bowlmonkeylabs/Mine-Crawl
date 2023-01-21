using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using UnityEngine;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.CaveV2.Objects;
using BML.Scripts.CaveV2.Util;
using BML.Scripts.CaveV2.SpawnObjects;
using BML.Scripts.Utils;
using GK;
using QuikGraph.Algorithms;
using Shapes;
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
        
        [SerializeField] private bool _generateRandomOnStart;
        
        [Tooltip("Keep level generated in Edit Mode. If not generated, then generate with current seed. RETRY WILL NOT WORK WITH THIS ENABLED.")]
        #if UNITY_EDITOR
        [SerializeField] private bool _keepEditModeLevel;
        #else
        private bool _keepEditModeLevel = false;
        #endif

        [SerializeField] public bool RetryOnFailure = true;
        [SerializeField] private int _maxRetryDepth = 3;

        [SerializeField, DisableIf("$_notOverrideBounds")] private Bounds _caveGenBounds = new Bounds(Vector3.zero, new Vector3(10,6,10));
        [SerializeField] private bool _overrideBounds = false;
        private bool _notOverrideBounds => !_overrideBounds;
        public Bounds CaveGenBounds => (_overrideBounds ? _caveGenBounds : _caveGenParams.PoissonBounds);

        [SerializeField] private BoolReference _retrySameSeed;

        [SerializeField] private IntReference _seedDebugReference;
        
        [Required] [InlineEditor, Space(10f)]
        [SerializeField] private CaveGenParameters _caveGenParams;

        public CaveGenParameters CaveGenParams => _caveGenParams;

        [Required] [InlineEditor, Space(10f)]
        [SerializeField] private CaveGraphMudBunRenderer _caveGraphMudBunRenderer;

        [Required] [InlineEditor, Space(10f)]
        [SerializeField] private LevelObjectSpawner _levelObjectSpawner;
        
        [Required] [InlineEditor, Space(10f)]
        [SerializeField] private DecorObjectSpawner _decorObjectSpawner;
        
        [TitleGroup("Player influence")]
        [SerializeField] private float _playerInfluenceUpdatePeriod = 1f;

        [SerializeField, SuffixLabel("units/sec")] private float _playerInfluenceAccumulationRate = 1f/30f;
        [SerializeField, SuffixLabel("units/sec")] private float _playerInfluenceDecayRate = 1f/30f;
        [SerializeField, SuffixLabel("units/sec by player distance"), Tooltip("Curve value represents accumulation multiplier as a function of player distance.")] 
        private CurveVariable _playerInfluenceAccumulationFalloff;
        [SerializeField] private GameEvent _onAfterUpdatePlayerDistance;

        [TitleGroup("Torches")] 
        [SerializeField] private DynamicGameEvent _onTorchPlaced;
        [SerializeField] private float _torchInfluenceUpdatePeriod = 1f;
        [Button]
        private void RecalculateTorchRequirementDebug()
        {
            foreach (var caveNodeData in _caveGraph.Vertices)
            {
                caveNodeData.CalculateTorchRequirement();
            }
        }
        
        [TitleGroup("Enemies")] 
        [SerializeField] private DynamicGameEvent _onEnemyKilled;
        [SerializeField, Range(-1f, 1f)] private float _enemyKilledAddInfluence = 0.5f;
        [Tooltip("The number of different difficulty segments the cave will be divided into. Ex. 3 means each 1/3rd changes difficulty.")]
        [SerializeField] private int _difficultySegmentCount = 3;
        
        public int DifficultySegmentCount => _difficultySegmentCount;

        [TitleGroup("Debug")]
        [SerializeField] private bool _enableLogs = false;
        [HideInInspector] public bool EnableLogs => _enableLogs;
        
        [SerializeField] private bool _showTraversabilityCheck = false;

#if UNITY_EDITOR
        [SerializeField] private bool _generateDebugObjects = true;
#else
        private bool _generateDebugObjects = false;
#endif
        [SerializeField, ShowIf("$_generateDebugObjects")] private Transform _debugObjectsContainer;

        public enum GizmoColorScheme
        {
            PlayerVisited,
            MainPath,
            MainPathDistance,
            ObjectiveDistance,
            PlayerDistance,
            TorchInfluence,
            PlayerInfluence,
        }
        
        [FoldoutGroup("Gizmo colors")] [SerializeField] public GizmoColorScheme GizmoColorScheme_Inner = GizmoColorScheme.PlayerVisited;
        [FoldoutGroup("Gizmo colors")] [SerializeField] public GizmoColorScheme GizmoColorScheme_Outer = GizmoColorScheme.MainPath;
        [FoldoutGroup("Gizmo colors")] [SerializeField] public Color DebugNodeColor_Default;
        [FoldoutGroup("Gizmo colors")] [SerializeField] public Color DebugNodeColor_Occupied;
        [FoldoutGroup("Gizmo colors")] [SerializeField] public Color DebugNodeColor_Visited;
        [FoldoutGroup("Gizmo colors")] [SerializeField] public Color DebugNodeColor_Start;
        [FoldoutGroup("Gizmo colors")] [SerializeField] public Color DebugNodeColor_End;
        [FoldoutGroup("Gizmo colors")] [SerializeField] public Color DebugNodeColor_MainPath;
        [FoldoutGroup("Gizmo colors")] [SerializeField] public Gradient DebugNodeColor_Gradient;

        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public int MaxMainPathDistance { get; private set; }
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public int MaxObjectiveDistance { get; private set; }
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public int MaxPlayerDistance { get; private set; }
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public int CurrentMaxPlayerObjectiveDistance { get; private set; }
        
        #endregion

        #region Events
        
        public delegate void AfterGenerate();
        public AfterGenerate OnAfterGenerate;
        
        public delegate void AfterUpdatePlayerDistance();
        public AfterUpdatePlayerDistance OnAfterUpdatePlayerDistance;
        
        #endregion
        
        #region Cave generation

        public CaveGraphV2 CaveGraph => _caveGraph;
        [HideInInspector, SerializeField] private CaveGraphV2 _caveGraph;

        [ShowInInspector]
        [InfoBox("Generation is disabled while MudBun mesh is locked.", InfoMessageType.Error, "_isGenerationDisabled")]
        public bool IsGenerationEnabled => !_caveGraphMudBunRenderer.IsMeshLocked;
        private bool _isGenerationDisabled => !IsGenerationEnabled;

        [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        public bool IsGenerated { get; private set; } = false;

        private int _retryDepth = 0;
        
        [PropertyOrder(-1)]
        [Button, LabelText("Generate Cave Graph")]
        //[EnableIf("$IsGenerationEnabled")]
        private void GenerateCaveGraphButton()
        {
            _retryDepth = 0;
            GenerateCaveGraph();
        }
        
        private void GenerateCaveGraph(bool useRandomSeed = true)
        {
            // if (!IsGenerationEnabled)
            //     return;
            
            DestroyCaveGraph();

            if (useRandomSeed)
            {
                _caveGenParams.UpdateRandomSeed();
            }
            
            _caveGraph = GenerateCaveGraph(_caveGenParams, CaveGenBounds);
            IsGenerated = true;
            
            GenerateCaveGraphDebugObjects();
            
            OnAfterGenerate?.Invoke();
        }

        public void RetryGenerateCaveGraph()
        {
            _retryDepth++;
            if (_retryDepth >= _maxRetryDepth)
            {
                throw new Exception($"Exceeded cave generation max retries ({_maxRetryDepth})");
            }
            
            if (_caveGenParams.LockSeed || !RetryOnFailure)
            {
                throw new Exception("Cave generation failed. To automatically resolve, ensure 'Retry on failure' is checked and 'Lock seed' is unchecked.");
            }
            else
            {
                _caveGenParams.UpdateRandomSeed(false);
                GenerateCaveGraph(false);
            }
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
            
            if (_caveGenParams.LockSeed || !RetryOnFailure)
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
        //[EnableIf("$IsGenerationEnabled")]
        private void DestroyCaveGraph()
        {
            // if (!IsGenerationEnabled) 
            //     return;
            
            _caveGraph = null;
            _minimumSpanningTreeGraphTEMP = null;
            IsGenerated = false;
            DestroyDebugObjects();
            
            _caveGraphMudBunRenderer.DestroyMudBun();
            _levelObjectSpawner.DestroyLevelObjects();
            _decorObjectSpawner.DestroyDecor();
            
        }
        
        private CaveGraphV2 GenerateCaveGraph(CaveGenParameters caveGenParams, Bounds bounds)
        {
            Random.InitState(caveGenParams.Seed);
            
            if (_enableLogs) Debug.Log($"Generating level ({_caveGenParams.Seed})");
            
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
                // var size = Random.Range(caveGenParams.RoomScaling.x, caveGenParams.RoomScaling.y);
                var size = caveGenParams.RoomScaling.x;
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

            // Remove direct connection between start and end
            caveGraph.TryGetEdge(startNode, endNode, out var startEndEdge);
            if (startEndEdge != null)
            {
                caveGraph.RemoveEdge(startEndEdge);
            }
            
            // Remove long edges
            var maxLength = caveGenParams.PoissonSampleRadius * caveGenParams.MaxEdgeLengthFactor;
            caveGraph.RemoveEdgeIf(edge => edge.Length >= maxLength);
            
            // Remove edges outside allowed steepness angles
            // Set radius based on steepness
            caveGraph.RemoveEdgeIf(edge =>
            {
                foreach (var range in caveGenParams.SteepnessRanges)
                {
                    if (edge.SteepnessAngle >= range.Angle.x && edge.SteepnessAngle <= range.Angle.y)
                    {
                        edge.Radius = Random.Range(range.EdgeRadius.x, range.EdgeRadius.y);
                        return false;
                    }
                        
                }
                return true;
            });
            
            // Calculate based on adjacency size
            if (caveGenParams.CalculateRoomSizeBasedOnRawAdjacency)
            {
                foreach (var caveNodeData in caveGraph.Vertices)
                {
                    var adjacentEdges = caveGraph
                        .AdjacentEdges(caveNodeData)
                        .ToList();
                    if (!adjacentEdges.Any())
                        continue;
                    
                    var averageEdgeLength =
                        adjacentEdges.Average(e => e.Length);
                    var edgeLengthFactor =
                        averageEdgeLength / caveGenParams.PoissonSampleRadius;
                    var fac = Mathf.InverseLerp(1f, caveGenParams.MaxEdgeLengthFactor, edgeLengthFactor);
                    var size = Mathf.Lerp(caveGenParams.RoomScaling.x, caveGenParams.RoomScaling.y, fac);
                    caveNodeData.Size = size;
                }
            }

            // Remove everything but the shortest path between start and end; the "main" path
            if (caveGenParams.OnlyShortestPathBetweenStartAndEnd)
            {
                var shortestPathFromStartFunc = caveGraph.ShortestPathsDijkstra(edge => edge.Length, startNode);
                shortestPathFromStartFunc(endNode, out var shortestPathFromStartToEnd);
                var shortestPathFromStartToEndList = shortestPathFromStartToEnd?.ToList();
                
                if (shortestPathFromStartToEndList != null)
                {
                    var keepEdges = new List<CaveNodeConnectionData>();
                    var checkedVertices = new Dictionary<CaveNodeData, int>();

                    int maxRecheckCount = 3;

                    // Offshoots from main path
                    if (caveGenParams.UseOffshootsFromMainPath)
                    {
                        for (int n = 0; n < caveGenParams.NumOffshoots; n++)
                        {
                            var remainingVertices = shortestPathFromStartToEndList
                                .Select(e => e.Source)
                                .Where(e => e != caveGraph.StartNode && e != caveGraph.EndNode)
                                .Except(
                                    checkedVertices
                                        .Where(kv => kv.Value >= maxRecheckCount)
                                        .Select(kv => kv.Key)
                                )
                                .Except(keepEdges.Select(e => e.Source))
                                .ToList();
                            // Debug.Log(n);
                            if (remainingVertices.Count == 0)
                            {
                                break;
                            }
                            int randomOffshootPathIndex = Random.Range(0, remainingVertices.Count);
                            CaveNodeData offshootStart = remainingVertices[randomOffshootPathIndex];

                            var offshootPath = new List<CaveNodeConnectionData>();
                            bool offshootSuccess = true;

                            for (int i = 0; i < caveGenParams.MinMaxOffshootLength.y; i++)
                            {
                                if (checkedVertices.ContainsKey(offshootStart)) checkedVertices[offshootStart] = checkedVertices[offshootStart] + 1;
                                else checkedVertices.Add(offshootStart, 1);
                                
                                var adjacentEdges = caveGraph.AdjacentEdges(offshootStart)
                                    .Where(edge =>
                                    {
                                        // var edgeAlreadyUsed = keepEdges.Contains(edge) || offshootPath.Contains(edge);
                                        var edgeAlreadyUsed = offshootPath.Contains(edge) || shortestPathFromStartToEndList.Contains(edge) || keepEdges.Contains(edge);

                                        CaveNodeData otherVertex;
                                        if (offshootStart == edge.Source) otherVertex = edge.Target;
                                        otherVertex = edge.Source;
                                        var vertexAlreadyUsed = offshootPath.Any(e =>
                                                                    e.Source == otherVertex || e.Target == otherVertex)
                                                                    || keepEdges.Any(e =>
                                                                        e.Source == otherVertex || e.Target == otherVertex);
                                        return !edgeAlreadyUsed && !vertexAlreadyUsed;
                                    })
                                    .ToList();
                                if (!adjacentEdges.Any())
                                {
                                    if (offshootPath.Count < caveGenParams.MinMaxOffshootLength.x)
                                    {
                                        n--;
                                        offshootSuccess = false;
                                    }
                                    break;
                                }
                            
                                int randomEdgeIndex = Random.Range(0, adjacentEdges.Count);
                                var randomEdge = adjacentEdges[randomEdgeIndex];
                                offshootPath.Add(randomEdge);

                                if (offshootStart == randomEdge.Source) offshootStart = randomEdge.Target;
                                else if (offshootStart == randomEdge.Target) offshootStart = randomEdge.Source;
                                else
                                {
                                    if (offshootPath.Count < caveGenParams.MinMaxOffshootLength.x)
                                    {
                                        n--;
                                        offshootSuccess = false;
                                    }
                                    break;
                                }
                            }
                            
                            if (offshootSuccess)
                            {
                                keepEdges.AddRange(offshootPath);
                            }
                        }
                    }
                    
                    // Keep all main path edges
                    keepEdges.AddRange(shortestPathFromStartToEndList);
                    
                    // Remove all edges except what we want to keep
                    caveGraph.RemoveEdgeIf(edge => !keepEdges.Contains(edge));

                    // var recheckSummary = checkedVertices.GroupBy(kv => kv.Value)
                    //     .Select(group => (group.Key, group.Sum(kv => 1)))
                    //     .ToList();
                    // var recheckString = String.Join(" | ", recheckSummary.Select(kv => $"{kv.Item2} main path vertices checked {kv.Key} times"));
                    // Debug.Log(recheckString);
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
            
            // Calculate graph properties for use in later generation steps (e.g. object spawning, enemy spawning)
            {
                // Calculate size
                if (!caveGenParams.CalculateRoomSizeBasedOnRawAdjacency)
                {
                    foreach (var caveNodeData in caveGraph.Vertices)
                    {
                        var averageEdgeLength = 
                            caveGraph.AdjacentEdges(caveNodeData)
                                .Average(e => e.Length);
                        var edgeLengthFactor =
                            averageEdgeLength / caveGenParams.PoissonSampleRadius;
                        var fac = Mathf.InverseLerp(1f, caveGenParams.MaxEdgeLengthFactor, edgeLengthFactor);
                        var size = Mathf.Lerp(caveGenParams.RoomScaling.x, caveGenParams.RoomScaling.y, fac);
                        caveNodeData.Size = size;
                    }
                }
                
                // Calculate distance from objective
                {
                    var objectiveVertices = new List<CaveNodeData> { caveGraph.EndNode };
                    caveGraph.FloodFillDistance(objectiveVertices, (node, dist) => node.ObjectiveDistance = dist);

                    this.MaxObjectiveDistance = caveGraph.Vertices.Max(e => e.ObjectiveDistance);
                    
                    // Assign each node difficulty from [0, _difficultySegmentCount -1]
                    caveGraph.FloodFillDistance(objectiveVertices, (node, dist) =>
                    {
                        int maxDifficulty = _difficultySegmentCount - 1;
                        float objectiveClosenessFactor = 1 - (float) dist / startNode.ObjectiveDistance;
                        var intermediate = Mathf.Min(maxDifficulty,
                            Mathf.FloorToInt(objectiveClosenessFactor * _difficultySegmentCount));
                        node.Difficulty = Mathf.Max(0, intermediate);
                    });
                }

                // Tunnel blockages
                {
                    // Block tunnels between difficulty transitions
                    var maxBlockedRoomSize = caveGenParams.PoissonSampleRadius / 6f; // 6 is the magic number, I think...
                    foreach (var caveNodeConnectionData in caveGraph.Edges)
                    {
                        if (caveNodeConnectionData.Source.Difficulty != caveNodeConnectionData.Target.Difficulty)
                        {
                            caveNodeConnectionData.IsBlocked = true;
                            
                            // Force rooms connected to blockages to be small
                            // (to avoid player walking around the blockage
                            caveNodeConnectionData.Source.Size = maxBlockedRoomSize;
                            caveNodeConnectionData.Target.Size = maxBlockedRoomSize;
                        }
                    }
                }

                // Calculate distance from main path
                {
                    var mainPathVertices = caveGraph.MainPath
                        .SelectMany(e => new List<CaveNodeData> { e.Source, e.Target})
                        .Distinct();
                    caveGraph.FloodFillDistance(mainPathVertices, (node, dist) => node.MainPathDistance = dist);

                    this.MaxMainPathDistance = caveGraph.Vertices.Max(e => e.MainPathDistance);
                }
            }

            // Choose where the merchant is placed
            var merchantCandidateVertices = caveGraph.Vertices
                .AsEnumerable()
                .OrderBy(v => {
                    float mainPathDistanceFactor = (float) v.MainPathDistance / MaxMainPathDistance;
                    float objectiveDistanceFactor = Mathf.Abs(((float) v.ObjectiveDistance / MaxObjectiveDistance) - 0.5f) * -3 + 1;
                    float sortFactor = (mainPathDistanceFactor * 0.5f) + (objectiveDistanceFactor * 0.5f);
                    return -sortFactor;
                });
            caveGraph.MerchantNode = merchantCandidateVertices.First();

            if (_enableLogs) Debug.Log($"Cave graph generated");
            
            return caveGraph;
        }

        private CaveGraphV2 _minimumSpanningTreeGraphTEMP;
        
        #endregion
        
        #region Player influence

        private Coroutine _coroutinePlayerInfluence;
        
        public void UpdatePlayerDistance(IEnumerable<CaveNodeData> playerOccupiedNodes)
        {
            if (!IsGenerated) return;
            
            _caveGraph.FloodFillDistance(
                playerOccupiedNodes,
                (node, dist) =>
                {
                    node.PlayerDistanceDelta = (dist - node.PlayerDistance);
                    node.PlayerDistance = dist;
                });

            if (_caveGraph.Vertices.Any())
            {
                this.MaxPlayerDistance = _caveGraph.Vertices.Max(node => node.PlayerDistance);
                this.CurrentMaxPlayerObjectiveDistance = _caveGraph.Vertices
                    .Where(node => node.PlayerDistance == 0)
                    .Max(node => node.ObjectiveDistance);
            }
            else
            {
                this.MaxPlayerDistance = -1;
                this.CurrentMaxPlayerObjectiveDistance = -1;
            }

            this.OnAfterUpdatePlayerDistance?.Invoke();
            _onAfterUpdatePlayerDistance.Raise();
        }

        public IEnumerator UpdatePlayerInfluenceCoroutine()
        {
            do
            {
                yield return new WaitForSeconds(_playerInfluenceUpdatePeriod);
                UpdatePlayerInfluence();
            } while (useGUILayout);
        }
        
        public void UpdatePlayerInfluence()
        {
            if (!IsGenerated) return;
            
            foreach (var nodeData in _caveGraph.AllNodes)
            {
                float falloffMultiplier = _playerInfluenceAccumulationFalloff.Value.Evaluate(nodeData.PlayerDistance);
                if (falloffMultiplier >= 0)
                {
                    var accumulation = (_playerInfluenceAccumulationRate * _playerInfluenceUpdatePeriod) 
                                       * falloffMultiplier;
                    nodeData.PlayerInfluence = Mathf.Min(1f, nodeData.PlayerInfluence + accumulation);
                }
                else
                {
                    var decay = (_playerInfluenceDecayRate * _playerInfluenceUpdatePeriod) 
                                * (1 - nodeData.TorchInfluence)
                                * falloffMultiplier;
                    nodeData.PlayerInfluence = Mathf.Max(0f, nodeData.PlayerInfluence + decay);
                }

                // For now it is convenient to tack the UpdateTorchInfluence onto this as well, rather than having a separate coroutine.
                UpdateTorchInfluence(nodeData);
            }
        }
        
        #endregion
        
        #region Torch

        public void OnTorchPlaced(object prevValue, object currValue)
        {
            var payload = currValue as TorchPlacedPayload;
            if (payload == null) throw new ArgumentException("Invalid payload for OnTorchPlaced");
            
            OnTorchPlaced(payload);
        }

        public void OnTorchPlaced(TorchPlacedPayload payload)
        {
            var containingRoom = _caveGraph.FindContainingRoom(payload.Position);
            if (containingRoom.nodeData == null)
            {
                Debug.LogError($"Torch placed outside any room in the cave graph. {payload.Position}");
                return;
            }

            containingRoom.nodeData.Torches.Add(payload.Torch);
            
            if (_enableLogs)
                Debug.Log($"CaveGen OnTorchPlaced: (Position {payload.Position}), (Room {containingRoom.nodeData.GameObject.GetInstanceID()})");
        }

        public void UpdateTorchInfluence(ICaveNodeData nodeData)
        {
            var activeTorches = nodeData.Torches
                .Where(t => !t.IsBurntOut)
                .ToList();
            if (activeTorches.Count == 0)
            {
                nodeData.TorchInfluence = 0f;
                return;
            }
            nodeData.TorchInfluence = 
                activeTorches.Average(t => t.PercentRemaining)
                * activeTorches.Count
                / nodeData.TorchRequirement;
        }
        
        #endregion
        
        #region Enemy
        
        public void OnEnemyKilled(object prevValue, object currValue)
        {
            var payload = currValue as EnemyKilledPayload;
            if (payload == null) throw new ArgumentException("Invalid payload for OnEnemyKilled");
            
            OnEnemyKilled(payload);
        }

        public void OnEnemyKilled(EnemyKilledPayload payload)
        {
            var containingRoom = _caveGraph.FindContainingRoom(payload.Position);
            if (containingRoom.nodeData == null)
            {
                Debug.LogError($"Enemy killed outside any room in the cave graph. {payload.Position}");
                return;
            }
            
            containingRoom.nodeData.PlayerInfluence = Mathf.Min(1f, containingRoom.nodeData.PlayerInfluence + _enemyKilledAddInfluence);
            
            if (_enableLogs)
                Debug.Log($"CaveGen OnEnemyKilled: (Position {payload.Position}), (Room {containingRoom.nodeData.GameObject.GetInstanceID()})");
        }
        
        #endregion
        
        #region Cave generation debug

        private void DestroyDebugObjects()
        {
            if (_debugObjectsContainer == null) return;
            
            var debugObjects = Enumerable.Range(0, _debugObjectsContainer.childCount)
                .Select(i => _debugObjectsContainer.GetChild(i).gameObject)
                .ToList();
            foreach (var childObject in debugObjects)
            {
                GameObject.DestroyImmediate(childObject);
            }
        }

        private void GenerateCaveGraphDebugObjects()
        {
#if !UNITY_EDITOR
            return;
#else
            
            if (EnableLogs) Debug.Log("Cave Graph: Generating debug objects");
            
            if (!_generateDebugObjects || _debugObjectsContainer == null || !IsGenerated || _caveGraph == null) return;
            
            // Spawn debug object at each cave node
            foreach (var caveNodeData in _caveGraph.Vertices)
            {
                // Spawn empty object
                GameObject newGameObject = new GameObject("Cave Node Gizmo");
                newGameObject.transform.SetParent(_debugObjectsContainer);
                newGameObject.transform.position = LocalToWorld(caveNodeData.LocalPosition);
                
                // Add shape renderer component for node
                var sphereOuter = newGameObject.AddComponent<Shapes.Sphere>();
                sphereOuter.Color = DebugNodeColor_Default;
                sphereOuter.BlendMode = ShapesBlendMode.Additive;
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(sphereOuter, false);
                
                // Add shape renderer component for secondary node indicator
                GameObject newGameObjectInner = new GameObject("Indicator");
                newGameObjectInner.transform.SetParent(newGameObject.transform);
                newGameObjectInner.transform.position = LocalToWorld(caveNodeData.LocalPosition);
                var sphereInner = newGameObjectInner.AddComponent<Shapes.Sphere>();
                sphereInner.Color = DebugNodeColor_Default;
                sphereInner.Radius = sphereOuter.Radius * 2 / 3;
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(sphereInner, false);
                
                // Add debug component
                var debugComponent = newGameObject.AddComponent<CaveNodeDataDebugComponent>();
                debugComponent.CaveNodeData = caveNodeData;
                debugComponent.CaveGenerator = this;
                debugComponent.InnerRenderer = sphereInner;
                debugComponent.OuterRenderer = sphereOuter;

                UnityEditorInternal.ComponentUtility.MoveComponentUp(debugComponent);
            }
            
            // Spawn debug object on each edge to ensure nodes are connected
            foreach (var caveNodeConnectionData in _caveGraph.Edges)
            {
                // Calculate tunnel position
                var sourceTargetDiff = (caveNodeConnectionData.Target.LocalPosition -
                                        caveNodeConnectionData.Source.LocalPosition);
                var sourceTargetDiffProjectedToGroundNormalized = Vector3.ProjectOnPlane(sourceTargetDiff, Vector3.up).normalized;
                
                var edgeDiff = (caveNodeConnectionData.Target.LocalPosition - caveNodeConnectionData.Source.LocalPosition);
                var edgeMidPosition = caveNodeConnectionData.Source.LocalPosition + edgeDiff / 2;
                var edgeRotation = Quaternion.LookRotation(edgeDiff);
                var edgeLength = edgeDiff.magnitude;
                var localScale = new Vector3(0.1f, 0.1f, edgeLength);
                // Debug.Log($"Edge length: EdgeLengthRaw {caveNodeConnectionData.Length} | Result Edge Length {edgeLength} | Source {caveNodeConnectionData.Source.Size} | Target {caveNodeConnectionData.Target.Size}");

                // Spawn empty object
                GameObject newGameObject = new GameObject("Cave Node Gizmo");
                newGameObject.transform.SetParent(_debugObjectsContainer);
                // newGameObject.transform.SetPositionAndRotation(edgeMidPosition, edgeRotation);
                // newGameObject.transform.position = edgeMidPosition;
                // newGameObject.transform.localScale = localScale;

                // Add shape renderer component
                var shapeLineComponent = newGameObject.AddComponent<Shapes.Line>();
                shapeLineComponent.Start = LocalToWorld(caveNodeConnectionData.Source.LocalPosition);
                shapeLineComponent.End = LocalToWorld(caveNodeConnectionData.Target.LocalPosition);
                shapeLineComponent.Color = DebugNodeColor_Default;
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(shapeLineComponent, false);
                
                // Add debug component
                var debugComponent = newGameObject.AddComponent<CaveNodeConnectionDataDebugComponent>();
                debugComponent.CaveNodeConnectionData = caveNodeConnectionData;
                debugComponent.CaveGenerator = this;
                
                UnityEditorInternal.ComponentUtility.MoveComponentUp(debugComponent);
            }
            
#endif
        }
        
        #endregion

        #region Unity lifecycle
        
        private void Awake()
        {
            _seedDebugReference.Value = _caveGenParams.Seed;
        }

        private void Start()
        {
            if (_keepEditModeLevel && IsGenerated)
                return;
            
            if (_generateRandomOnStart && ApplicationUtils.IsPlaying_EditorSafe)
            {
                if (_enableLogs) Debug.Log($"CaveGraph: Generate random on start");
                _retryDepth = 0;
                bool randomSeed = !_keepEditModeLevel && !_retrySameSeed.Value;
                GenerateCaveGraph(randomSeed);
            }
        }

        private void OnEnable()
        {
            _caveGenParams.OnValidateEvent += OnValidate;
            _onTorchPlaced.Subscribe(OnTorchPlaced);
            _onEnemyKilled.Subscribe(OnEnemyKilled);
            _coroutinePlayerInfluence = StartCoroutine(UpdatePlayerInfluenceCoroutine());
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
#endif
        }
        
        private void OnDisable()
        {
            _caveGenParams.OnValidateEvent -= OnValidate;
            _onTorchPlaced.Unsubscribe(OnTorchPlaced);
            _onEnemyKilled.Unsubscribe(OnEnemyKilled);
            if (_coroutinePlayerInfluence != null) StopCoroutine(_coroutinePlayerInfluence);
            _coroutinePlayerInfluence = null;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
#endif
        }

        private void OnValidate()
        {
            if (_generateOnChange 
                && _isGenerateOnChangeEnabled 
                && !ApplicationUtils.IsPlaying_EditorSafe)
            {
                if (_enableLogs) Debug.Log($"CaveGraph: Generate on change {_isGenerateOnChangeEnabled}");
                _retryDepth = 0;
                GenerateCaveGraph(false);
            } 
        }

        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private bool _isGenerateOnChangeEnabled = false;
#if UNITY_EDITOR
        private void PlayModeStateChanged(PlayModeStateChange stateChange)
        {
            switch (stateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    _isGenerateOnChangeEnabled = true;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    _isGenerateOnChangeEnabled = false;
                    break;
            }
            if (_enableLogs) Debug.Log($"CaveGraph: Play Mode State Changed: {this.name} {stateChange} {_isGenerateOnChangeEnabled}");
        }
#endif

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
            // if (_caveGraph != null)
            // {
            //     _caveGraph.DrawGizmos(LocalOrigin, _showTraversabilityCheck, Color.white);
            // }
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