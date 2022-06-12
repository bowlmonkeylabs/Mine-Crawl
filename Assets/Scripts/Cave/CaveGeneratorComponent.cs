using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Cave.DirectedGraph;
using BML.Scripts.Cave.MarchingCubesModified;
using BML.Scripts.Utils;
using Common.Unity.Drawing;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Node = BML.Scripts.Cave.DirectedGraph.DirectedGraph<UnityEngine.Vector3, BML.Scripts.Cave.DirectedGraph.CaveNodeData, BML.Scripts.Cave.DirectedGraph.CaveNodeConnectionData>.Node;
using NodeConnection = BML.Scripts.Cave.DirectedGraph.DirectedGraph<UnityEngine.Vector3, BML.Scripts.Cave.DirectedGraph.CaveNodeData, BML.Scripts.Cave.DirectedGraph.CaveNodeConnectionData>.NodeConnection;
using Random = UnityEngine.Random;

namespace BML.Scripts.Cave
{
    public class CaveGeneratorComponent : MonoBehaviour
    {
        #region Inspector
        
        [FoldoutGroup("Seed")]
        [SerializeField] private int _seed;
        [FoldoutGroup("Seed")]
        [SerializeField] private bool _lockSeed;
        
        [TitleGroup("Dependencies")]
        [SerializeField] private Grid _grid;
        [TitleGroup("Dependencies")]
        [SerializeField] private BoxCollider _gridBounds;
        
        [TitleGroup("Poisson parameters")]
        [SerializeField] private float _minRadius = 1f;
        [TitleGroup("Poisson parameters")]
        [SerializeField] private float _graphPadding = 1f;
        [TitleGroup("Poisson parameters")]
        [SerializeField] private float _nodeSizeMin = 0.1f;
        [TitleGroup("Poisson parameters")]
        [SerializeField] private float _nodeSizeMax = 0.5f;
        
        [TitleGroup("Marching cubes parameters")]
        [SerializeField] private float _surfaceValue = 0.5f;
        [TitleGroup("Marching cubes parameters")]
        [SerializeField] private bool _drawNormals = true;
        [TitleGroup("Marching cubes parameters")]
        [SerializeField] private Material _material;

        [TitleGroup("Debug")]
        [SerializeField] private bool _drawGridGizmos = false;
        [TitleGroup("Debug")]
        [SerializeField] private Transform _gridProbe;
        
        #endregion

        private PoissonDiscSampler3D _poissonDiscSampler;
        private List<Vector3> _poissonDiskSamples;

        private CaveGraph _caveGraph;

        private CaveGraph.VoxelData[,,] _caveVoxels;
        
        private Marching _marching;
        private NormalRenderer _marchingNormalRenderer;
        private List<GameObject> _meshes = new List<GameObject>();

        [Button]
        private void PrintGridProbePosition()
        {
            var cellPosition = _grid.WorldToCell(_gridProbe.position);
            var cellCenter = _grid.GetCellCenterWorld(cellPosition);
            var cellIndex = _grid.CellToBoundsRelativeCell(_gridBounds.bounds, cellPosition);
            Debug.Log($"World: {_gridProbe.position} | Cell: {cellPosition} | Cell center: {cellCenter} | Cell index: {cellIndex}");
        }
        
        #region Unity lifecycle
        
        private void Start()
        {
            
        }

        private void OnDrawGizmos()
        {
            // var boundsMin = _gridBounds.bounds.min;
            //
            // if (_poissonDiskSamples != null && _poissonDiskSamples.Count > 0)
            // {
            //     foreach (var position in _poissonDiskSamples)
            //     {
            //         Gizmos.DrawSphere(position.xoy() + boundsMin, _gizmoRadius);
            //     }
            // }

            if (_caveGraph != null)
            {
                _caveGraph.OnDrawGizmos(this.gameObject);
            }

            if (_drawGridGizmos && _grid != null && _gridBounds != null)
            {
                _grid.OnDrawGizmos(this.gameObject, _gridBounds.bounds);
            }
        }
        
        private void OnRenderObject()
        {
            if(_marchingNormalRenderer != null && _meshes.Count > 0 && _drawNormals)
            {
                var m = _meshes[0].transform.localToWorldMatrix;

                _marchingNormalRenderer.LocalToWorld = m;
                _marchingNormalRenderer.Draw();
            }
            
        }

        #endregion

        [Button]
        private void Destroy()
        {
            if (_poissonDiskSamples != null && _poissonDiskSamples.Count > 0)
            {
                _poissonDiskSamples.Clear();
            }

            _caveGraph = null;

            _caveVoxels = null;

            _marching = null;
            _marchingNormalRenderer = null;
            
            foreach (var mesh in _meshes)
            {
                GameObject.DestroyImmediate(mesh);
            }
            _meshes.Clear();
        }

        [Button]
        private void Generate()
        {
            Debug.ClearDeveloperConsole();
            
            Destroy();
            
            if (!_lockSeed)
            {
                _seed = Random.Range(Int32.MinValue, Int32.MaxValue);
            }
            Random.InitState(_seed);
            
            // Compute generation bounds
            var boundsMin = _gridBounds.bounds.min;
            var boundsMax = _gridBounds.bounds.max;
            
            // Generate starting sample points
            var poissonSize = (boundsMax - boundsMin) - (Vector3.one * _graphPadding);
            var poissonMinOffset = (_gridBounds.center - (poissonSize / 2)) - boundsMin;
            _poissonDiscSampler = new PoissonDiscSampler3D(poissonSize, _minRadius);
            _poissonDiskSamples = _poissonDiscSampler.Samples().ToList();

            // Initialize cave graph from sample points
            _caveGraph = new CaveGraph(_gridBounds.bounds, _gridBounds.transform);
            foreach (var samplePosition in _poissonDiskSamples)
            {
                var localPosition = samplePosition + poissonMinOffset;
                var nodeSize = Random.Range(_nodeSizeMin, _nodeSizeMax);
                var nodeData = new CaveNodeData(localPosition, nodeSize);
                // Debug.Log($"Create node: Local: {nodeData.LocalPosition} | Size: {nodeData.Size}");
                var node = new Node(nodeData.LocalPosition, nodeData);
                _caveGraph.AddNode(node);
            }
            
            // Connect some cave nodes to each other
            _caveGraph.ConnectNearestNeighbors(3);
            
            // Render cave graph to voxels
            _caveVoxels = _caveGraph.GetVoxels(_grid);
            
            // Convert our voxel representation to valid input for Marching cubes
            // TODO make marching cubes take in more data per voxel
            float[,,] voxelValues = new float[_caveVoxels.GetLength(0),_caveVoxels.GetLength(1),_caveVoxels.GetLength(2)];
            for (int x = 0; x < _caveVoxels.GetLength(0); x++)
            {
                for (int y = 0; y < _caveVoxels.GetLength(1); y++)
                {
                    for (int z = 0; z < _caveVoxels.GetLength(2); z++)
                    {
                        voxelValues[x, y, z] = _caveVoxels[x, y, z].Value;
                    }
                }
            }
            var voxelArray = new VoxelArray(voxelValues);
            
            // Create mesh from voxels (Marching cubes)
            _marching = new MarchingCubes(_grid, _surfaceValue);
            _marching.Surface = _surfaceValue;
            
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();
            _marching.Generate(voxelArray.Voxels, verts, indices);

            bool smoothNormals = true;
            if (smoothNormals)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    //Presumes the vertex is in local space where
                    //the min value is 0 and max is width/height/depth.
                    Vector3 p = verts[i];

                    float u = p.x / (_caveVoxels.GetLength(0) - 1.0f);
                    float v = p.y / (_caveVoxels.GetLength(1) - 1.0f);
                    float w = p.z / (_caveVoxels.GetLength(2) - 1.0f);

                    Vector3 n = voxelArray.GetNormal(u, v, w);

                    normals.Add(n);
                }

                _marchingNormalRenderer = new NormalRenderer();
                _marchingNormalRenderer.DefaultColor = Color.red;
                _marchingNormalRenderer.Length = 0.25f;
                _marchingNormalRenderer.Load(verts, normals);
            }
            
            var offset = new Vector3(
                -1 * (float)_caveVoxels.GetLength(0) / 2, 
                -1 * (float)_caveVoxels.GetLength(1) / 2, 
                -1 * (float)_caveVoxels.GetLength(2) / 2
            );
            offset = _grid.CellToLocalInterpolated(offset);
            var position = _gridBounds.center + offset;
            CreateMesh32(verts, normals, indices, position);
        }
        
        private void CreateMesh32(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = _material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = position;

            _meshes.Add(go);
        }
    }
}