using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BML.Scripts.Utils;
using MudBun;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace BML.Scripts.CaveV2.MudBun
{
    [ExecuteAlways]
    public class MudbunMeshSplitter : MonoBehaviour
    {
        [Required, SerializeField] private MudRenderer _mudRenderer;
        [Tooltip("Separate the mudbun mesh into wall and ground after mudbun adds collider. Should be TRUE for play/build.")]
        [Required, SerializeField] private bool _separateAfterAddMudbunCollider = true;
        [Required, SerializeField] private bool _enableLogs;
        [SerializeField] private float _groundDotProductMin = 0f;
        [SerializeField] private string _groundLayer = "Terrain";
        [SerializeField] private string _wallLayer = "Obstacle";
        [SerializeField] private string _groundObjName = "Cave_Ground";
        [SerializeField] private string _wallObjName = "Cave_Walls";
        
        [FoldoutGroup("Debug"), SerializeField, ReadOnly] private MeshFilter _meshFilter;
        [FoldoutGroup("Debug"), SerializeField, ReadOnly] private MeshRenderer _meshRenderer;
        [FoldoutGroup("Debug"), SerializeField, ReadOnly] private MeshCollider _meshCollider;
        [FoldoutGroup("Debug"), SerializeField, ReadOnly] private GameObject _groundObj;
        [FoldoutGroup("Debug"), SerializeField, ReadOnly] private GameObject _wallObj;

        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private int[] _triangles;
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private Vector3[] _normals;
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private Vector3[] _vertices;
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private List<Vector3> faceNormals = new List<Vector3>();
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private List<int> triangleIndices_g = new List<int>();
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private List<int> triangleIndices_w = new List<int>();
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private List<int> g_trianglesList = new List<int>();
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private List<int> w_trianglesList = new List<int>();

        public bool IsMeshSeparated => (_groundObj != null || _wallObj != null);

        private Mesh mesh_original;

        #region Unity Lifecycle

        private void OnEnable()
        {
            _mudRenderer.OnAfterAddCollider += SeparateMeshOnLock;
        }

        private void OnDisable()
        {
            _mudRenderer.OnAfterAddCollider -= SeparateMeshOnLock;
        }

        #endregion


        #region Slice/Separate Mesh
        
        private void GatherMeshData()
        {
            if (_meshFilter == null)
                _meshFilter = GetComponent<MeshFilter>();
            
            if (_meshRenderer == null)
                _meshRenderer = GetComponent<MeshRenderer>();
            
            if (_meshCollider == null)
                _meshCollider = GetComponent<MeshCollider>();
            
            // Use mesh instead of sharedMesh for runtime?
            // Seems to work fine for this procedural mesh...
            mesh_original = _meshFilter.sharedMesh;
            
            _normals = mesh_original.normals;
            _triangles = mesh_original.triangles;
            _vertices = mesh_original.vertices;

            
            if (_enableLogs) Debug.Log($"Normals size: {_normals.Length}");
            if (_enableLogs) Debug.Log($"Triangles size: {_triangles.Length}");
            if (_enableLogs) Debug.Log($"Vertices size: {_vertices.Length}");
            
            // See this post to use normals of triangle face, NOT THE VERTICES' NORMALS
            // https://forum.unity.com/threads/how-do-i-get-the-normal-of-each-triangle-in-mesh.101018/
            
            // Loop through triangle indices and calculate the face normal for each triangle face
            // Store this normal in a list whose index is the triangle's index
            faceNormals.Clear();
            for(int i = 0; i < _triangles.Length;)
            {
                Vector3 N1 = _normals[_triangles[i]];
                i++;
                Vector3 N2 = _normals[_triangles[i]];
                i++;
                Vector3 N3 = _normals[_triangles[i]];
                i++;

                Vector3 faceNormal = (N1 + N2 + N3) / 3;
                faceNormals.Add(faceNormal);
            }

            triangleIndices_g.Clear();
            triangleIndices_w.Clear();
            
            // Determine if triangles are ground or wall
            int j = 0;
            for (int i = 0; i < faceNormals.Count; i++)
            {
                if (Vector3.Dot(faceNormals[i], Vector3.up) > _groundDotProductMin)
                    triangleIndices_g.Add(i);
                else
                    triangleIndices_w.Add(i);

                j++;
            }
            if (_enableLogs) Debug.Log($"Iterations {j} | Count: {triangleIndices_g.Count}");
            
            if (_enableLogs) Debug.Log($"Detected {triangleIndices_g.Count} / {_triangles.Length / 3f} triangles that are ground");
        }
        
        [Button, PropertyOrder(-1), DisableIf("IsMeshSeparated")]
        [InfoBox("Slicing is disabled while separated mesh exists.", InfoMessageType.Warning, "IsMeshSeparated")]
        public void SliceMesh()
        {
            GatherMeshData();
            
            // For each triangle, there are 3 vertex indexes
            int[] g_triangles = new int[triangleIndices_g.Count * 3];
            int[] w_triangles = new int[triangleIndices_w.Count * 3];

            // Set ground data
            // Store the actual triangle vertex index triplets in array
            for (int i = 0; i < triangleIndices_g.Count; i++)
            {
                g_triangles[i * 3] = _triangles[triangleIndices_g[i] * 3];
                g_triangles[i * 3 + 1] = _triangles[triangleIndices_g[i] * 3 + 1];
                g_triangles[i * 3 + 2] = _triangles[triangleIndices_g[i] * 3 + 2];
            }
            g_trianglesList = g_triangles.ToList();
            
            //Set wall data
            //Store the actual triangle vertex index triplets in array
            for (int i = 0; i < triangleIndices_w.Count; i++)
            {
                w_triangles[i * 3] = _triangles[triangleIndices_w[i] * 3];
                w_triangles[i * 3 + 1] = _triangles[triangleIndices_w[i] * 3 + 1];
                w_triangles[i * 3 + 2] = _triangles[triangleIndices_w[i] * 3 + 2];
            }
            w_trianglesList = w_triangles.ToList();

            // Split the original mesh into 2 submeshes and assign same material
            mesh_original.subMeshCount = 2;
            if (_enableLogs) Debug.Log($"Incrementing submesh count: {mesh_original.subMeshCount}");

            Material[] materials = new Material[2];
            materials[0] = _meshRenderer.sharedMaterials[0];
            materials[1] = materials[0];
            _meshRenderer.sharedMaterials = materials;

            mesh_original.SetTriangles(g_triangles, 1);
            mesh_original.SetTriangles(w_triangles, 0);
            
            mesh_original.RecalculateNormals();
            mesh_original.RecalculateBounds();
            mesh_original.RecalculateTangents();

            _meshFilter.sharedMesh = mesh_original;
        }

        private void SeparateMeshOnLock()
        {
            if (!_separateAfterAddMudbunCollider) return;
            SeparateMesh();
        }

        [Button, PropertyOrder(-1), DisableIf("IsMeshSeparated")]
        [InfoBox("Separating is disabled while separated mesh exists.", InfoMessageType.Warning, "IsMeshSeparated")]
        public void SeparateMesh()
        {
            SliceMesh();
            _groundObj = SeparateMeshPart(_groundObjName, _groundLayer, g_trianglesList);
            _wallObj = SeparateMeshPart(_wallObjName, _wallLayer, w_trianglesList);
            if (_meshRenderer != null) _meshRenderer.enabled = false;
            if (_meshCollider != null) _meshCollider.enabled = false;
        }

        private GameObject SeparateMeshPart(string objName, string layerName, List<int> triangleList)
        {
            var separateMeshObj = new GameObject(objName);
            separateMeshObj.transform.parent = transform.parent;
            separateMeshObj.name = objName;
            separateMeshObj.layer = LayerMask.NameToLayer(layerName);
            MeshFilter meshFilter = separateMeshObj.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            MeshRenderer meshRenderer = separateMeshObj.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = separateMeshObj.AddComponent<MeshCollider>();
            
            mesh.Clear();
            
            // Right now copying all vertices from original mesh just to preserve their index
            // Not sure if this could lead any issues or pref impacts later down the line
            // https://answers.unity.com/questions/947930/create-a-mesh-from-a-sub-mesh.html
            mesh.SetVertices(_vertices);
            mesh.SetTriangles(triangleList, 0);
            mesh.SetColors(_meshFilter.sharedMesh.colors);
            mesh.SetUVs(0, _meshFilter.sharedMesh.uv);
            mesh.SetNormals(_normals);
            mesh.SetTangents(_meshFilter.sharedMesh.tangents);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.Optimize();
            // mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;
            meshRenderer.sharedMaterial = _meshRenderer.sharedMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            return separateMeshObj;
        }
        
        #endregion
    }
}