using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.Minimap;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using UnityEngine;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.CaveV2.Objects;
using BML.Scripts.CaveV2.Util;
using BML.Scripts.CaveV2.SpawnObjects;
using BML.Scripts.Utils;
using BML.Utils;
using BML.Utils.Random;
using GK;
using QuikGraph.Algorithms;
using Shapes;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine.Serialization;
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

        [SerializeField] private TransformSceneReference _playerSceneReference;
        [SerializeField] private TransformSceneReference _greenRoomSceneReference;

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
        [SerializeField] private FloatVariable _torchAreaCoverage;
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

        [TitleGroup("Debug")]
        [SerializeField] private bool _enableLogs = false;
        [HideInInspector] public bool EnableLogs => _enableLogs;
        
        [SerializeField] private bool _showTraversabilityCheck = false;
        
// TODO we probably DON'T need to include these in release builds, but I allowed them for now because they're useful for debugging.
// #if UNITY_EDITOR
        [SerializeField] private bool _generateDebugObjects = true;
// #else
//         private bool _generateDebugObjects = false;
// #endif
        [SerializeField, ShowIf("$_generateDebugObjects")] private Transform _debugObjectsContainer;
        
        [TitleGroup("Minimap")]
        [SerializeField] private bool _generateMinimap = true;
        [SerializeField, ShowIf("$_generateMinimap")] private SafeTransformValueReference _minimapObjectsContainer;
        [FormerlySerializedAs("_minimapParameters")] [SerializeField, ShowIf("$_generateMinimap")] public CaveMinimapParameters MinimapParameters;
        [SerializeField] private GameEvent _onRequestRevealWholeMap;

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
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public int MaxStartDistance { get; private set; }
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
        [ButtonGroup("Generation")]
        [Button("Generate")]
        //[EnableIf("$IsGenerationEnabled")]
        public void GenerateCaveGraphButton()
        {
            _retryDepth = 0;
            SeedManager.Instance.UpdateRandomSeed();
            GenerateCaveGraph();
        }
        
        private void GenerateCaveGraph()
        {
            // if (!IsGenerationEnabled)
            //     return;
            
            DestroyCaveGraph();
            
            if (!_playerSceneReference.SafeIsUnityNull() && !_playerSceneReference.Value.SafeIsUnityNull())
            {
                // Move player to a holding area outside the level generation bounds. (we didn't want them interacting with level objects during the generation process)
                var playerController = _playerSceneReference.Value.gameObject.GetComponent<IPlayerController>();
                if (playerController != null)
                {
                    var transform1 = _greenRoomSceneReference.Value.transform;
                    playerController.SetPositionAndRotation(
                        transform1.position, 
                        transform1.rotation, 
                        true
                    );
                }
                else
                {
                    if (EnableLogs) Debug.Log($"Level Object Spawner: Player controller not found.");
                }
            }
            else
            {
                if (EnableLogs) Debug.Log($"Level Object Spawner: No player assigned");
            }
            
            _caveGraph = GenerateCaveGraph(_caveGenParams, CaveGenBounds);
            IsGenerated = true;
            
            GenerateCaveGraphDebugObjects();
            GenerateCaveGraphMinimapObjects();
            
            OnAfterGenerate?.Invoke();
        }

        public void RetryGenerateCaveGraph()
        {
            _retryDepth++;
            if (_retryDepth >= _maxRetryDepth)
            {
                throw new Exception($"Exceeded cave generation max retries ({_maxRetryDepth})");
            }
            
            if (SeedManager.Instance.LockSeed || !RetryOnFailure)
            {
                throw new Exception("Cave generation failed. To automatically resolve, ensure 'Retry on failure' is checked and 'Lock seed' is unchecked.");
            }
            else
            {
                SeedManager.Instance.UpdateRandomSeed(false);
                GenerateCaveGraph();
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
            
            if (SeedManager.Instance.LockSeed || !RetryOnFailure)
            {
                throw new Exception("Cave generation failed; level is not traversable. To automatically resolve, ensure 'Retry on failure' is checked and 'Lock seed' is unchecked.");
            }
            else
            {
                SeedManager.Instance.UpdateRandomSeed(false);
                return GenerateCaveGraph(_caveGenParams, CaveGenBounds);
            }
        }

        [PropertyOrder(-1)]
        [ButtonGroup("Generation")]
        [Button("Destroy")] 
        //[EnableIf("$IsGenerationEnabled")]
        public void DestroyCaveGraph()
        {
            // if (!IsGenerationEnabled) 
            //     return;
            
            _caveGraph = null;
            _minimumSpanningTreeGraphTEMP = null;
            IsGenerated = false;
            DestroyDebugObjects();
            DestroyMinimapObjects();
            
            _caveGraphMudBunRenderer.DestroyMudBun();
            _levelObjectSpawner.DestroyLevelObjects();
            _decorObjectSpawner.DestroyDecor();
            
        }
        
        private CaveGraphV2 GenerateCaveGraph(CaveGenParameters caveGenParams, Bounds bounds)
        {
            Random.InitState(SeedManager.Instance.GetSteppedSeed("CaveGraph"));
            
            if (_enableLogs) Debug.Log($"Generating level ({SeedManager.Instance.GetSteppedSeed("CaveGraph")})");
            
            var caveGraph = new CaveGraphV2();
            
            var poissonBoundsWithPadding = caveGenParams.GetBoundsWithPadding(bounds, CaveGenParameters.PaddingType.Inner);

            // Generate start node position
            var startBounds = new Bounds(
                poissonBoundsWithPadding.center + (poissonBoundsWithPadding.extents / 2),
                poissonBoundsWithPadding.extents / 2);
            var startPosition = RandomUtils.RandomInBounds(startBounds) + startBounds.center;
            startPosition = poissonBoundsWithPadding.max - poissonBoundsWithPadding.extents / 4;
            var startNode = new CaveNodeData(startPosition, 1f, _torchAreaCoverage);
            caveGraph.AddVertex(startNode);
            caveGraph.StartNode = startNode;
            
            // Generate end node position
            var endBounds = new Bounds(
                poissonBoundsWithPadding.center - (poissonBoundsWithPadding.extents / 2),
                poissonBoundsWithPadding.extents / 2);
            var endPosition = RandomUtils.RandomInBounds(endBounds) + endBounds.center;
            endPosition = poissonBoundsWithPadding.min + poissonBoundsWithPadding.extents / 4;
            var endNode = new CaveNodeData(endPosition, 1f, _torchAreaCoverage);
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
                var node = new CaveNodeData(pointRelativeToBoundsCenter, size, _torchAreaCoverage);
                return node;
            });
            caveGraph.AddVertexRange(vertices);
            
            // Add an edge between every possible combination of nodes, and calculate the distance/cost
            var numVertices = caveGraph.Vertices.Count();
            if (numVertices > 10000)
            {
                throw new Exception($"Cave graph has too many vertices ({numVertices}) for our inefficient adjacency calculation; consider revising this code in order to continue!");
            }
            var vertexCombinations =
                from v1 in caveGraph.Vertices
                from v2 in caveGraph.Vertices
                where v1 != v2 &&
                      v1.LocalPosition.RoughDistanceThresholdCheck(
                          v2.LocalPosition, 
                          caveGenParams.PoissonSampleRadius * caveGenParams.MaxEdgeLengthFactor)
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

                var edgeRadius = Math.Max(vertexPair.v1.Scale, vertexPair.v2.Scale);
                
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
            if (caveGenParams.CalculateRoomSize && caveGenParams.CalculateRoomSizeBasedOnRawAdjacency)
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
                    var scale = Mathf.Lerp(caveGenParams.RoomScaling.x, caveGenParams.RoomScaling.y, fac);
                    caveNodeData.Scale = scale;
                }
            }

            // Remove everything but the shortest path between start and end; the "main" path
            if (caveGenParams.OnlyShortestPathBetweenStartAndEnd)
            {
                var shortestPathFromStartFunc = caveGraph.ShortestPathsDijkstra(edge => edge.Length, startNode);
                shortestPathFromStartFunc(endNode, out var shortestPathFromStartToEnd);
                var shortestPathFromStartToEndList = shortestPathFromStartToEnd?.ToList();
                
                if (shortestPathFromStartToEnd == null)
                {
                    // Not traversable
                    if (_showTraversabilityCheck)
                    {
                        Debug.LogWarning($"Seed {SeedManager.Instance.GetSteppedSeed("CaveGraph")} failed traversability check, retrying cave generation.");
                    }
                    
                    _retryDepth++;
                    if (_retryDepth >= _maxRetryDepth)
                    {
                        Debug.LogError($"Exceeded cave generation max retries ({_maxRetryDepth})");
                        return caveGraph;
                    }
                    return RetryGenerateCaveGraph(false);
                }
                
                // Calculate distance from main path
                // This is intentionally done twice; the second run is the "real" result, this first run is just for the generation algorithm to reference. 
                {
                    var mainPathNodes = shortestPathFromStartToEndList
                        .SelectMany(e => new List<CaveNodeData> { e.Source, e.Target })
                        .Distinct()
                        .Concat<ICaveNodeData>(shortestPathFromStartToEndList);
                    caveGraph.FloodFillDistance(mainPathNodes, (node, dist) => node.MainPathDistance = dist);

                    this.MaxMainPathDistance = caveGraph.AllNodes.Max(e => e.MainPathDistance);
                }
                
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
                                        var edgeConnectsToStartOrEnd =
                                            edge.Source == startNode || edge.Source == endNode ||
                                            edge.Target == startNode || edge.Target == endNode;

                                        CaveNodeData otherVertex;
                                        if (offshootStart == edge.Source) otherVertex = edge.Target;
                                        otherVertex = edge.Source;
                                        var vertexAlreadyUsed = offshootPath.Any(e =>
                                                                    e.Source == otherVertex || e.Target == otherVertex)
                                                                    || keepEdges.Any(e =>
                                                                        e.Source == otherVertex || e.Target == otherVertex);
                                        return !edgeAlreadyUsed && !vertexAlreadyUsed && !edgeConnectsToStartOrEnd;
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

                                var sortedAdjacentEdges = adjacentEdges
                                    .OrderByDescending(e =>
                                    {
                                        CaveNodeData other = e.OtherEnd(offshootStart);
                                        if (other == null) return -1;
                                        return other.MainPathDistance;
                                    })
                                    .ToList();
                                int randomEdgeIndex = Mathf.RoundToInt(RandomUtils.RandomGaussian(0, adjacentEdges.Count - 1));
                                var randomEdge = sortedAdjacentEdges[randomEdgeIndex];
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

                        var edgeRadius = Math.Max(sourceNode.Scale, targetNode.Scale);
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
                // TODO use or remove minimum spanning tree
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
                        Debug.LogWarning($"Seed {SeedManager.Instance.GetSteppedSeed("CaveGraph")} failed traversability check, retrying cave generation.");
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
                if (caveGenParams.CalculateRoomSize && !caveGenParams.CalculateRoomSizeBasedOnRawAdjacency)
                {
                    foreach (var caveNodeData in caveGraph.Vertices)
                    {
                        var averageEdgeLength = 
                            caveGraph.AdjacentEdges(caveNodeData)
                                .Average(e => e.Length);
                        var edgeLengthFactor =
                            averageEdgeLength / caveGenParams.PoissonSampleRadius;
                        var fac = Mathf.InverseLerp(1f, caveGenParams.MaxEdgeLengthFactor, edgeLengthFactor);
                        var scale = Mathf.Lerp(caveGenParams.RoomScaling.x, caveGenParams.RoomScaling.y, fac);
                        caveNodeData.Scale = scale;
                    }
                }
                
                // Calculate distance from level start
                {
                    var objectiveVertices = new List<CaveNodeData> { caveGraph.StartNode };
                    caveGraph.FloodFillDistance(objectiveVertices, (node, dist) => node.StartDistance = dist);

                    this.MaxStartDistance = caveGraph.AllNodes.Max(e => e.StartDistance);
                }
                
                // Calculate distance from objective
                {
                    var objectiveVertices = new List<CaveNodeData> { caveGraph.EndNode };
                    caveGraph.FloodFillDistance(objectiveVertices, (node, dist) => node.ObjectiveDistance = dist);

                    this.MaxObjectiveDistance = caveGraph.AllNodes.Max(e => e.ObjectiveDistance);
                }

                // Calculate distance from main path
                // This is intentionally done twice; the second run is the "real" result, the first run is just for the generation algorithm to reference. 
                {
                    var mainPathNodes = caveGraph.MainPath
                        .SelectMany(e => new List<CaveNodeData> { e.Source, e.Target })
                        .Distinct()
                        .Concat<ICaveNodeData>(caveGraph.MainPath);
                    caveGraph.FloodFillDistance(mainPathNodes, (node, dist) => node.MainPathDistance = dist);

                    this.MaxMainPathDistance = caveGraph.AllNodes.Max(e => e.MainPathDistance);
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
            
            // Assign node types
            // TODO more intelligent logic
            foreach (var caveGraphVertex in caveGraph.Vertices)
            {
                if (caveGraphVertex == caveGraph.StartNode) {
                    caveGraphVertex.NodeType = CaveNodeType.Start;
                    continue;
                }
                if (caveGraphVertex == caveGraph.EndNode) {
                    caveGraphVertex.NodeType = CaveNodeType.End;
                    continue;
                }
                if (caveGraphVertex == caveGraph.MerchantNode) {
                    caveGraphVertex.NodeType = CaveNodeType.Merchant;
                    continue;
                }
                
                // Rooms with only one connection (dead ends) are large rooms
                int numEdges = caveGraph.AdjacentEdges(caveGraphVertex).Count();
                if (numEdges == 1)
                {
                    caveGraphVertex.NodeType = CaveNodeType.Large;
                    continue;
                } 
                else if (numEdges >= 4)
                {
                    caveGraphVertex.NodeType = CaveNodeType.Small;
                    continue;
                }
                else
                {
                    caveGraphVertex.NodeType = (CaveNodeType)Random.Range((int)CaveNodeType.Small, (int)CaveNodeType.Medium + 1);
                }
            }
                
            // Decide which tunnels should be blocked
            foreach (var nodeA in caveGraph.Vertices)
            {
                // Block any tunnels connected to exit to make it slightly more difficult to find
                bool isObjective = nodeA.ObjectiveDistance == 0;
                if (isObjective)
                {
                    foreach (var caveNodeConnectionData in caveGraph.AdjacentEdges(nodeA))
                    {
                        caveNodeConnectionData.IsBlocked = true;
                    }
                    continue;
                }

                bool isStart = nodeA.StartDistance == 0;
                if (isStart)
                {
                    continue;
                }
                
                bool isMainPath = nodeA.MainPathDistance == 0;
                if (isMainPath)
                {
                    bool hasBranch = caveGraph.AdjacentVertices(nodeA)
                        .Any(nodeB =>
                            nodeB.MainPathDistance != 0 &&
                            caveGraph.AdjacentVertices(nodeB)
                                .Any(v => v != nodeA && v.MainPathDistance != 0)
                        );
                    if (!hasBranch) continue;
                    
                    var allEdgesNotTowardStartBlocked = true;
                    foreach (var nodeB in caveGraph.AdjacentVertices(nodeA))
                    {
                        bool nodeBIsTowardsStart = (nodeB.StartDistance <= nodeA.StartDistance);
                        bool nodeBIsMainPath = (nodeB.MainPathDistance == 0);
                        if (nodeBIsMainPath && !nodeBIsTowardsStart)
                        {
                            bool nodeBHasBranch = caveGraph.AdjacentVertices(nodeB)
                                .Any(v => v != nodeA && v.MainPathDistance != 0);

                            if (nodeBHasBranch) {
                                allEdgesNotTowardStartBlocked = false;
                                continue;
                            }
                        }

                        caveGraph.TryGetEdge(nodeA, nodeB, out var edge);
                        if (edge != null)
                        {
                            edge.IsBlocked = !nodeBIsTowardsStart;
                        }
                    }
                    
                    if(allEdgesNotTowardStartBlocked) {
                        nodeA.NodeType = CaveNodeType.Challenge;
                    }
                }
            }
            
            if (_enableLogs) Debug.Log($"Cave graph generated");
            
            return caveGraph;
        }

        private CaveGraphV2 _minimumSpanningTreeGraphTEMP;
        
        #endregion
        
        #region Player influence

        private Coroutine _coroutinePlayerInfluence;
        
        public void UpdatePlayerDistance(IEnumerable<ICaveNodeData> playerOccupiedNodes)
        {
            if (!IsGenerated || !playerOccupiedNodes.Any()) return;
            
            // Debug.Log(playerOccupiedNodes.Count());
            _caveGraph.FloodFillDistance(
                playerOccupiedNodes,
                (node, dist) =>
                {
                    node.PlayerDistanceDelta = (dist - node.PlayerDistance);
                    node.PlayerDistance = dist;
                });

            if (_caveGraph.AllNodes.Any())
            {
                this.MaxPlayerDistance = _caveGraph.AllNodes.Max(node => node.PlayerDistance);
                this.CurrentMaxPlayerObjectiveDistance = playerOccupiedNodes.Max(node => node.ObjectiveDistance);

                var playerLocalPosition = WorldToLocal(_playerSceneReference.Value.position);
                foreach (var caveNodeData in _caveGraph.AllNodes)
                {
                    caveNodeData.DirectPlayerDistance = Vector3.Distance(playerLocalPosition, caveNodeData.LocalPosition);
                }
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
#if UNITY_EDITOR
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(sphereOuter, false);
#endif
                
                // Add shape renderer component for secondary node indicator
                GameObject newGameObjectInner = new GameObject("Indicator");
                newGameObjectInner.transform.SetParent(newGameObject.transform);
                newGameObjectInner.transform.position = LocalToWorld(caveNodeData.LocalPosition);
                var sphereInner = newGameObjectInner.AddComponent<Shapes.Sphere>();
                sphereInner.Color = DebugNodeColor_Default;
                sphereInner.Radius = sphereOuter.Radius * 2 / 3;
#if UNITY_EDITOR
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(sphereInner, false);
#endif
                
                // Add debug component
                var debugComponent = newGameObject.AddComponent<CaveNodeDataDebugComponent>();
                debugComponent.CaveNodeData = caveNodeData;
                debugComponent.CaveGenerator = this;
                debugComponent.InnerRenderer = sphereInner;
                debugComponent.OuterRenderer = sphereOuter;
#if UNITY_EDITOR
                UnityEditorInternal.ComponentUtility.MoveComponentUp(debugComponent);
#endif
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
#if UNITY_EDITOR
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(shapeLineComponent, false);
#endif
                
                // Add debug component
                var debugComponent = newGameObject.AddComponent<CaveNodeConnectionDataDebugComponent>();
                debugComponent.CaveNodeConnectionData = caveNodeConnectionData;
                debugComponent.CaveGenerator = this;
                debugComponent.InnerRenderer = shapeLineComponent;
#if UNITY_EDITOR
                UnityEditorInternal.ComponentUtility.MoveComponentUp(debugComponent);
#endif
            }
            
        }
        
        #endregion

        #region Mini Map

        private void DestroyMinimapObjects()
        {
            if (EnableLogs) Debug.Log("Cave Graph: Destroying minimap objects");
            
            if (_minimapObjectsContainer == null || _minimapObjectsContainer.Value == null) return;
            
            var minimapObjects = Enumerable.Range(0, _minimapObjectsContainer.Value.childCount)
                .Select(i => _minimapObjectsContainer.Value.GetChild(i).gameObject)
                .ToList();
            foreach (var childObject in minimapObjects)
            {
                var nodeComponent = _minimapObjectsContainer.Value.GetComponent<CaveNodeDataMinimapComponent>();
                if (nodeComponent != null)
                {
                    OnAfterUpdatePlayerDistance -= nodeComponent.UpdatePlayerOccupied;
                }
                else
                {
                    var nodeConnectionComponent = _minimapObjectsContainer.Value.GetComponent<CaveNodeConnectionDataMinimapComponent>();
                    if (nodeConnectionComponent != null)
                    {
                        OnAfterUpdatePlayerDistance -= nodeComponent.UpdatePlayerOccupied;
                    }
                }
                
                GameObject.DestroyImmediate(childObject);
            }
        }

        private void GenerateCaveGraphMinimapObjects()
        {
            if (EnableLogs) Debug.Log("Cave Graph: Generating minimap objects");

            if (!_generateMinimap || _minimapObjectsContainer?.Value == null || !IsGenerated || _caveGraph == null)
            {
                if (EnableLogs) Debug.LogError("Minimap failed to generate!");
                return;
            }

            var minimapController = _minimapObjectsContainer.Value.GetComponent<MinimapController>();
            if (minimapController == null)
            {
                if (EnableLogs) Debug.LogError("Minimap failed to generate! Minimap controller not found.");
                return;
            }
            
            // Spawn at each cave node
            foreach (var caveNodeData in _caveGraph.Vertices)
            {
                // Spawn new game object
                GameObject newGameObject = GameObjectUtils.SafeInstantiate(
                    true, 
                    MinimapParameters.PrefabCaveNode, 
                    _minimapObjectsContainer.Value);
                // newGameObject.transform.position = LocalToWorld(caveNodeData.LocalPosition);
                newGameObject.transform.localPosition = caveNodeData.LocalPosition;
                
                // Get debug component
                var debugComponent = newGameObject.GetComponent<CaveNodeDataMinimapComponent>();
                debugComponent.Initialize(caveNodeData, this, minimapController);
                
                OnAfterUpdatePlayerDistance += debugComponent.UpdatePlayerOccupied;

#if UNITY_EDITOR
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(debugComponent.DiscoveredRenderer, false);
#endif
            }
            
            // Spawn at each edge
            foreach (var caveNodeConnectionData in _caveGraph.Edges)
            {
                // Spawn new game object
                GameObject newGameObject = GameObjectUtils.SafeInstantiate(
                    true,
                    MinimapParameters.PrefabCaveNodeConnection,
                    _minimapObjectsContainer.Value);
                
                // Get debug component
                var debugComponent = newGameObject.GetComponent<CaveNodeConnectionDataMinimapComponent>();
                debugComponent.Initialize(caveNodeConnectionData, this, minimapController);
                
                OnAfterUpdatePlayerDistance += debugComponent.UpdatePlayerOccupied;

#if UNITY_EDITOR
                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(debugComponent.Line, false);
#endif
            }
        }

        public void SetAllMapped(bool alsoSetVisited)
        {
            foreach (var node in _caveGraph.AllNodes)
            {
                node.PlayerMapped = true;
                if (alsoSetVisited)
                {
                    node.PlayerVisited = true;
                }
            }
        }
        
        public void SetAllMapped()
        {
            SetAllMapped(false);
        }
        
        #endregion

        #region Unity lifecycle

        private void Start()
        {
            if (_keepEditModeLevel && IsGenerated)
                return;
            
            if (_generateRandomOnStart && ApplicationUtils.IsPlaying_EditorSafe)
            {
                if (_enableLogs) Debug.Log($"CaveGraph: Generate random on start");
                _retryDepth = 0;
                GenerateCaveGraph();
            }
        }

        private void OnEnable()
        {
            _caveGenParams.OnValidateEvent += OnValidate;
            _onTorchPlaced.Subscribe(OnTorchPlaced);
            _onEnemyKilled.Subscribe(OnEnemyKilled);
            _onRequestRevealWholeMap.Subscribe(SetAllMapped);
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
            _onRequestRevealWholeMap.Unsubscribe(SetAllMapped);
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
                GenerateCaveGraph();
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
            Gizmos.DrawWireCube(LocalToWorld(outerBounds.center), outerBounds.size);
            Gizmos.color = Color.gray;
            var poissonBoundsWithPadding = _caveGenParams.GetBoundsWithPadding(CaveGenBounds, CaveGenParameters.PaddingType.Inner);
            Gizmos.DrawWireCube(LocalToWorld(poissonBoundsWithPadding.center), poissonBoundsWithPadding.size);
            
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
            Handles.Label(transform.position, SeedManager.Instance.GetSteppedSeed("CaveGraph").ToString());
            #endif

        }

        #endregion

        #region Utils

        public Vector3 LocalOrigin => this.transform.position + CaveGenBounds.center;
        
        public Vector3 LocalToWorld(Vector3 localPosition)
        {
            return LocalOrigin + localPosition;
        }
        
        public Vector3 WorldToLocal(Vector3 worldPosition)
        {
            return worldPosition - LocalOrigin;
        }

        #endregion
    }
}