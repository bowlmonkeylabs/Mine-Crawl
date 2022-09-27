using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class DecorObjectSpawner : MonoBehaviour
    {
        [SerializeField] private CaveGenComponentV2 _caveGen;
        [SerializeField] private MeshCollider _meshCollider;
        [SerializeField] private LayerMask _roomBoundsLayerMask;
        [SerializeField] [MinMaxSlider(0f, 180f)] private Vector2 _minMaxAngle = new Vector2(0f, 180f);
        [SerializeField] private float _dotMin = .5f;
        [SerializeField] private int _pointCount = 5000;
        [SerializeField] private float _pointRadius = .25f;

        private Vector3 randomPoint;
        public List<Vector3> debugPoints;

        float[] sizes;
        float[] cumulativeSizes;
        float total = 0;

        [Button]
        public void AddDebugPoints()
        {
            List<int> filteredTriangles = FilterTrianglesNormal(_meshCollider.sharedMesh);
            CalcAreas(filteredTriangles, _meshCollider.sharedMesh.vertices);
            for (int i = 0; i < _pointCount; i++)
                addDebugPoint(filteredTriangles);
        }

        [Button]
        public void RemoveDebugPoints()
        {
            debugPoints.Clear();
        }

        [Button]
        public void AssignPointsToRooms()
        {
            foreach (var vert in _caveGen.CaveGraph.Vertices)
            {
                GameObject roomObj = vert.GameObject;
                AssignPoints(roomObj);
            }
            
            foreach (var edge in _caveGen.CaveGraph.Edges)
            {
                GameObject edgeObj = edge.GameObject;
                AssignPoints(edgeObj);
            }
        }

        private void AssignPoints(GameObject nodeObj)
        {
            List<Vector3> roomPoints = new List<Vector3>();
            Collider roomBounds = nodeObj.GetComponentInChildren<Collider>();
            
            //TODO: Make sure this collider is on the room bounds layer
                
            foreach (var point in debugPoints)
            {
                if (roomBounds.bounds.Contains(point))
                {
                    roomPoints.Add(point);
                }
            }
                
            RenderDecorPoints roomDecorRend = nodeObj.GetComponentInChildren<RenderDecorPoints>();
            roomDecorRend.ClearDebugPoints();
            roomDecorRend.SetDebugPoints(roomPoints, _pointRadius);
        }

        [Button]
        public void ClearPointsFromRooms()
        {
            foreach (var decorRend in GameObject.FindObjectsOfType<RenderDecorPoints>())
            {
                decorRend.ClearDebugPoints();
            }
        }

        void OnDrawGizmosSelected()
        {
            foreach (Vector3 debugPoint in debugPoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(debugPoint, _pointRadius);
            }
        }

        private void addDebugPoint(List<int> filteredTriangles)
        {
            randomPoint = GetRandomPointOnMesh(filteredTriangles, _meshCollider.sharedMesh);
            randomPoint += _meshCollider.transform.position;
            debugPoints.Add(randomPoint);
        }

        private List<int> FilterTrianglesNormal(Mesh mesh)
        {
            List<int> filteredTriangles = new List<int>();
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;
            for(int i = 0; i < triangles.Length;)
            {
                Vector3 N1 = normals[triangles[i]];
                int T1 = triangles[i];
                i++;
                Vector3 N2 = normals[triangles[i]];
                int T2 = triangles[i];
                i++;
                Vector3 N3 = normals[triangles[i]];
                int T3 = triangles[i];
                i++;

                Vector3 faceNormal = (N1 + N2 + N3) / 3;
                

                if (Vector3.Angle(faceNormal, Vector3.up) >= _minMaxAngle.x &&
                    Vector3.Angle(faceNormal, Vector3.up) <= _minMaxAngle.y)
                {
                    filteredTriangles.Add(T1);
                    filteredTriangles.Add(T2);
                    filteredTriangles.Add(T3);
                }

                // if (Vector3.Dot(faceNormal, Vector3.up) >= _dotMin)
                // {
                //     filteredTriangles.Add(triangles[i-3]);
                //     filteredTriangles.Add(triangles[i-2]);
                //     filteredTriangles.Add(triangles[i-1]);
                // }
            }

            return filteredTriangles;
        }

        public void CalcAreas(List<int> filteredTriangles, Vector3[] vertices)
        {
            sizes = GetTriSizes(filteredTriangles, vertices);
            cumulativeSizes = new float[sizes.Length];
            total = 0;

            for (int i = 0; i < sizes.Length; i++)
            {
                total += sizes[i];
                cumulativeSizes[i] = total;
            }
        }

        public Vector3 GetRandomPointOnMesh(List<int> filteredTriangles, Mesh mesh)
        {
            float randomsample = Random.value * total;
            int triIndex = -1;

            for (int i = 0; i < sizes.Length; i++)
            {
                if (randomsample <= cumulativeSizes[i])
                {
                    triIndex = i;
                    break;
                }
            }

            if (triIndex == -1)
                Debug.LogError("triIndex should never be -1");

            Vector3 a = mesh.vertices[filteredTriangles[triIndex * 3]];
            Vector3 b = mesh.vertices[filteredTriangles[triIndex * 3 + 1]];
            Vector3 c = mesh.vertices[filteredTriangles[triIndex * 3 + 2]];

            // Generate random barycentric coordinates
            float r = Random.value;
            float s = Random.value;

            if (r + s >= 1)
            {
                r = 1 - r;
                s = 1 - s;
            }

            // Turn point back to a Vector3
            Vector3 pointOnMesh = a + r * (b - a) + s * (c - a);
            return pointOnMesh;
        }

        public float[] GetTriSizes(List<int> tris, Vector3[] verts)
        {
            int triCount = tris.Count / 3;
            float[] sizes = new float[triCount];
            for (int i = 0; i < triCount; i++)
            {
                sizes[i] = .5f * Vector3.Cross(
                    verts[tris[i * 3 + 1]] - verts[tris[i * 3]],
                    verts[tris[i * 3 + 2]] - verts[tris[i * 3]]).magnitude;
            }
            return sizes;
        }
    }
}