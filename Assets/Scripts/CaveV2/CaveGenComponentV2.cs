using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph;
using UnityEngine;
using BML.Scripts.CaveV2.MudBun;
using BML.Scripts.CaveV2.Util;
using BML.Scripts.CaveV2.Clayxel;
using BML.Scripts.CaveV2.SpawnObjects;
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
        
        [SerializeField, DisableIf("$_notOverrideBounds")] private Bounds _caveGenBounds = new Bounds(Vector3.zero, new Vector3(10,6,10));
        [SerializeField] private bool _overrideBounds = false;
        private bool _notOverrideBounds => !_overrideBounds;
        public Bounds CaveGenBounds => (_overrideBounds ? _caveGenBounds : _caveGenParams.PoissonBounds);
        
        [Required] [InlineEditor]
        [SerializeField] private CaveGenParameters _caveGenParams;
        
        [Required] [InlineEditor]
        [SerializeField] private CaveGraphClayxelRenderer _caveGraphClayxelRenderer;

        [Required] [InlineEditor]
        [SerializeField] private CaveGraphMudBunRenderer _caveGraphMudBunRenderer;

        [Required] [InlineEditor]
        [SerializeField] private LevelObjectSpawner _levelObjectSpawner;
        
        #endregion
        
        #region Events
        
        public delegate void AfterGenerate();

        public AfterGenerate OnAfterGenerate;
        
        #endregion
        
        #region Cave generation

        public CaveGraphV2 CaveGraph => _caveGraph;
        [HideInInspector, SerializeField] private CaveGraphV2 _caveGraph;

        [PropertyOrder(-1)]
        [Button, LabelText("Generate Cave Graph")]
        private void GenerateCaveGraphButton()
        {
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

        [PropertyOrder(-1)]
        [Button]
        private void DestroyCaveGraph()
        {
            _caveGraph = null;
        }

        private static CaveGraphV2 GenerateCaveGraph(CaveGenParameters caveGenParams, Bounds bounds)
        {
            Random.InitState(caveGenParams.Seed);
            
            var caveGraph = new CaveGraphV2();

            // Generate initial points for graph nodes with poisson distribution
            var poissonBoundsWithPadding = caveGenParams.GetBoundsWithPadding(bounds, CaveGenParameters.PaddingType.Inner);
            var poisson = new PoissonDiscSampler3D(poissonBoundsWithPadding.size, caveGenParams.PoissonSampleRadius);
            var samplePoints = poisson.Samples();
            var vertices = samplePoints.Select(point =>
            {
                var pointRelativeToBoundsCenter = (point - poissonBoundsWithPadding.size / 2);
                var size = Random.Range(caveGenParams.RoomScaling.x, caveGenParams.RoomScaling.y);
                var node = new CaveNodeData(pointRelativeToBoundsCenter, size);
                return node;
            });
            caveGraph.AddVertexRange(vertices);
            
            // Assign random start node
            var startNode = caveGraph.GetRandomVertex();
            caveGraph.StartNode = startNode;
            
            // Assign random end node
            var endNode = caveGraph.GetRandomVertex(new List<CaveNodeData> { startNode });
            caveGraph.EndNode = endNode;
            
            // Add an edge between every possible combination of nodes, and calculate the distance/cost
            var numVertices = caveGraph.Vertices.Count();
            if (numVertices > 100)
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
            
            // Remove long edges
            var maxLength = bounds.size.magnitude * caveGenParams.MaxEdgeLengthFactor;
            caveGraph.RemoveEdgeIf(edge => edge.Length >= maxLength);
            
            // Remove steep edges
            var maxAngle = caveGenParams.MaxEdgeSteepnessAngle;
            caveGraph.RemoveEdgeIf(edge => edge.SteepnessAngle >= maxAngle);

            return caveGraph;
        }
        
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
                _caveGraph.DrawGizmos(LocalOrigin);
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