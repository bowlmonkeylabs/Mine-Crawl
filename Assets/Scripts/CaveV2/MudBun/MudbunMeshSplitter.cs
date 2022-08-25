using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.MudBun
{
    public class MudbunMeshSplitter : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private float _groundDotProductMin = 0f;
        [SerializeField] private string _groundLayer = "Terrain";
        [SerializeField] private string _wallLayer = "Obstacle";

        [ShowInInspector] [ReadOnly] private List<int> _triangles = new List<int>();
        [ShowInInspector] [ReadOnly] private List<Vector3> _normals = new List<Vector3>();
        [ShowInInspector] [ReadOnly] private List<Vector3>  _vertices = new List<Vector3>();
        
        private List<Vector3> faceNormals = new List<Vector3>();
        private List<int> triangleIndices_g = new List<int>();
        private List<int> triangleIndices_w = new List<int>();
        private List<int> g_trianglesList = new List<int>();
        private List<int> w_trianglesList = new List<int>();
        
        
        [Button]
        public void SliceMesh()
        {
            if (_meshFilter == null)
                _meshFilter = GetComponent<MeshFilter>();
            
            if (_meshRenderer == null)
                _meshRenderer = GetComponent<MeshRenderer>();
            
            //TODO: use mesh instead of sharedMesh for runtime?
            Mesh mesh = _meshFilter.sharedMesh;
            
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;

            _normals = normals.ToList();
            _triangles = triangles.ToList();
            _vertices = vertices.ToList();
            
            Debug.Log($"Normals size: {normals.Length}");
            Debug.Log($"Triangles size: {triangles.Length}");
            Debug.Log($"Vertices size: {vertices.Length}");
            
            //See this post to use normals of triangle face, NOT THE VERTICES
            //https://forum.unity.com/threads/how-do-i-get-the-normal-of-each-triangle-in-mesh.101018/
            
            //Loop through triangle indices and calculate the face normal for each triangle
            //Store this normal in a list whose index is the triangle's index
            for(int i = 0; i < triangles.Length;)
            {
                //Vector3 P1 = vertices[triangles[i]];
                Vector3 N1 = normals[triangles[i]];
                i++;
                
                //Vector3 P2 = vertices[triangles[i]];
                Vector3 N2 = normals[triangles[i]];
                i++;
                
                //Vector3 P3 = vertices[triangles[i]];
                Vector3 N3 = normals[triangles[i]];
                i++;

                Vector3 faceNormal = (N1 + N2 + N3) / 3;
                faceNormals.Add(faceNormal);
            }
            
            for (int i = 0; i < faceNormals.Count; i++)
            {
                if (Vector3.Dot(faceNormals[i], Vector3.up) > _groundDotProductMin)
                    triangleIndices_g.Add(i);
                else
                    triangleIndices_w.Add(i);
            }
            
            Debug.Log($"Detected {triangleIndices_g.Count} / {triangles.Length / 3f} triangles that are ground");
            
            int[] g_triangles = new int[triangleIndices_g.Count * 3];
            int[] w_triangles = new int[triangleIndices_w.Count * 3];

            //Set ground data
            //Store the actual triangle vertex index triplets in array
            for (int i = 0; i < triangleIndices_g.Count; i++)
            {
                g_triangles[i * 3] = triangles[triangleIndices_g[i] * 3];
                g_triangles[i * 3 + 1] = triangles[triangleIndices_g[i] * 3 + 1];
                g_triangles[i * 3 + 2] = triangles[triangleIndices_g[i] * 3 + 2];
            }

            g_trianglesList = g_triangles.ToList();
            
            //Set wall data
            //Store the actual triangle vertex index triplets in array
            for (int i = 0; i < triangleIndices_w.Count; i++)
            {
                w_triangles[i * 3] = triangles[triangleIndices_w[i] * 3];
                w_triangles[i * 3 + 1] = triangles[triangleIndices_w[i] * 3 + 1];
                w_triangles[i * 3 + 2] = triangles[triangleIndices_w[i] * 3 + 2];
            }

            w_trianglesList = w_triangles.ToList();

            mesh.subMeshCount = 2;
            Debug.Log($"Incrementing submesh count: {mesh.subMeshCount}");

            mesh.SetTriangles(g_triangles, 0);
            mesh.SetTriangles(w_triangles, 1);
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            _meshFilter.sharedMesh = mesh;
        }

        [Button]
        public void SeparateMesh()
        {
            var wallsObj =
                GameObjectUtils.SafeInstantiate(false, new GameObject(), transform.parent);
            wallsObj.name = "Cave_Walls";
            wallsObj.layer = LayerMask.NameToLayer(_wallLayer);
            MeshFilter meshfilter_w = wallsObj.AddComponent<MeshFilter>();
            Mesh mesh_w = new Mesh();
            MeshRenderer meshRenderer_w = wallsObj.AddComponent<MeshRenderer>();
            MeshCollider meshCollider_w = wallsObj.AddComponent<MeshCollider>();
            
            mesh_w.Clear();
            mesh_w.SetVertices(_vertices);
            mesh_w.SetTriangles(w_trianglesList, 0);
            mesh_w.SetColors(_meshFilter.sharedMesh.colors);
            mesh_w.RecalculateNormals();
            mesh_w.RecalculateBounds();
            mesh_w.RecalculateTangents();
            meshfilter_w.sharedMesh = mesh_w;
            meshCollider_w.sharedMesh = mesh_w;
            meshRenderer_w.material = _meshRenderer.material;

            var groundObj =
                GameObjectUtils.SafeInstantiate(false, new GameObject(), transform.parent);
            groundObj.name = "Cave_Ground";
            groundObj.layer = LayerMask.NameToLayer(_groundLayer);
            MeshFilter meshfilter_g = groundObj.AddComponent<MeshFilter>();
            Mesh mesh_g = new Mesh();
            MeshRenderer meshRenderer_g = groundObj.AddComponent<MeshRenderer>();
            MeshCollider meshCollider_g = groundObj.AddComponent<MeshCollider>();
            
            mesh_g.Clear();
            mesh_g.SetVertices(_vertices);
            mesh_g.SetTriangles(g_trianglesList, 0);
            mesh_g.SetColors(_meshFilter.sharedMesh.colors);
            mesh_g.RecalculateNormals();
            mesh_g.RecalculateBounds();
            mesh_g.RecalculateTangents();
            meshfilter_g.sharedMesh = mesh_g;
            meshCollider_g.sharedMesh = mesh_g;
            meshRenderer_g.material = _meshRenderer.material;
        }
    }
}