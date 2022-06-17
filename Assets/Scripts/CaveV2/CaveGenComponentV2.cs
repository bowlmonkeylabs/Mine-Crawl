using System;
using System.Linq;
using UnityEngine;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.Util;
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
        
        [SerializeField] private Bounds _caveGenBounds = new Bounds(Vector3.zero, new Vector3(10,6,10));
        
        [Required] [InlineEditor]
        [SerializeField] private CaveGenParameters _caveGenParams;
        
        [Required] [InlineEditor]
        [SerializeField] private CaveGraphClayxelRenderer _caveGraphClayxelRenderer;

        [Required] [InlineEditor]
        [SerializeField] private LevelObjectSpawner _levelObjectSpawner;
        
        #endregion

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
            _caveGraph = GenerateCaveGraph(_caveGenParams, _caveGenBounds);
            
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
                var size = Random.Range(1f, 3f); // TODO parameterize
                var node = new CaveNodeData(pointRelativeToBoundsCenter, size);
                return node;
            });
            caveGraph.AddVertexRange(vertices);
            
            // Assign random start node
            var startNode = caveGraph.GetRandomVertex();
            caveGraph.StartNode = startNode;

            return caveGraph;
        }
        
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
            var outerBounds = _caveGenParams.GetBoundsWithPadding(_caveGenBounds, CaveGenParameters.PaddingType.Outer);
            Gizmos.DrawWireCube(outerBounds.center, outerBounds.size);
            Gizmos.color = Color.gray;
            var poissonBoundsWithPadding = _caveGenParams.GetBoundsWithPadding(_caveGenBounds, CaveGenParameters.PaddingType.Inner);
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

        public Vector3 LocalOrigin => this.transform.position + _caveGenBounds.center;
        
        public Vector3 LocalToWorld(Vector3 localPosition)
        {
            return LocalOrigin + localPosition;
        }

        #endregion
    }
}