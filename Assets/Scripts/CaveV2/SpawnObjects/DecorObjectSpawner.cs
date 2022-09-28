using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class DecorObjectSpawner : MonoBehaviour
    {
        [SerializeField] private CaveGenComponentV2 _caveGen;
        [SerializeField] private GameObject _mudRendererObj;
        [SerializeField] private LayerMask _roomBoundsLayerMask;
        [SerializeField] [MinMaxSlider(0f, 180f)] private Vector2 _minMaxAngle = new Vector2(0f, 180f);
        [SerializeField] [Range(0, 1)] private float _noiseFilterValueMin = .5f;
        [SerializeField] private int _maxDistanceFromMainPath = 10;
        [SerializeField] private AnimationCurve _mainPathDecayCurve;
        
        [Tooltip("Defaulting to false for now just because tunnels dont store cave graph data at the moment. i.e. distance to main path")]
        [SerializeField] private bool _assignPointsToTunnels = false;
        
        [SerializeField] private float _pointDensity = .25f;
        [SerializeField] private float _pointRadius = .25f;
        [SerializeField] private List<Vector3> debugPoints;

        [ShowInInspector, ReadOnly] private int pointCount;
        
        private Vector3 randomPoint;
        private MeshCollider meshCollider;

        private Dictionary<CaveNodeData, List<Vector3>> caveNodePointDict =
            new Dictionary<CaveNodeData, List<Vector3>>();

        float[] sizes;
        float[] cumulativeSizes;
        float total = 0;

        [SerializeField] private List<NoiseBox> _noiseBoxes = new List<NoiseBox>();
        [SerializeField] private Vector3 _noiseBoxCount = new Vector3(100f, 100f, 100f);
        [SerializeField] [Range(0, 1)] private float _noiseScale = 100f;

        struct NoiseBox
        {
            public Vector3 pos;
            public Vector3 scale;
            public float value;
        }

        #region Unity Lifecycle

        void OnDrawGizmosSelected()
        {
            foreach (Vector3 debugPoint in debugPoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(debugPoint, _pointRadius);
            }

            foreach (var noiseBox in _noiseBoxes)
            {
                //Recalc value to allow for changing dynamically while in editor
                float value = Mathf.Clamp01(Perlin.Noise(noiseBox.pos * _noiseScale));
                if (value >= _noiseFilterValueMin)
                {
                    Gizmos.color = new Color(.5f, .5f, .5f, .5f);
                    Gizmos.DrawCube(noiseBox.pos, noiseBox.scale);
                }
            }
        }

        #endregion

        #region Visualize Noise

        [Button]
        public void VisualizeNoise()
        {
            UnVisualizeNoise();
            Bounds caveBounds = _caveGen.CaveGenBounds;
            int countX = Mathf.FloorToInt(_noiseBoxCount.x);
            int countY = Mathf.FloorToInt(_noiseBoxCount.y);
            int countZ = Mathf.FloorToInt(_noiseBoxCount.z);
            
            Vector3 noiseBoxScale = new Vector3(
                caveBounds.size.x / countX,
                caveBounds.size.y / countY,
                caveBounds.size.z / countZ
            );
            Vector3 caveBoundsWorldCenter = _caveGen.transform.position + _caveGen.CaveGenBounds.center;

            for (int x = 0; x < countX; x++)
            {
                float xOffset = (x - countX / 2) * noiseBoxScale.x;
                for (int y = 0; y < countY; y++)
                {
                    float yOffset = (y - countY / 2) * noiseBoxScale.y;
                    for (int z = 0; z < countX; z++)
                    {
                        float zOffset = (z - countZ / 2) * noiseBoxScale.z;
                        NoiseBox noiseBox = new NoiseBox();
                        noiseBox.scale = noiseBoxScale;
                        noiseBox.pos = caveBoundsWorldCenter + new Vector3(xOffset, yOffset, zOffset);
                        noiseBox.value = Mathf.Clamp01(Perlin.Noise(noiseBox.pos * _noiseScale));
                        _noiseBoxes.Add(noiseBox);
                    }
                }
            }
        }

        [Button]
        public void UnVisualizeNoise()
        {
            _noiseBoxes.Clear();
        }

        #endregion

        #region Show/Remove Points

        [Button]
        public void GenerateDebugPoints()
        {
            RemoveDebugPoints();
            meshCollider = _mudRendererObj.GetComponent<MeshCollider>();
            List<int> filteredTriangles = FilterTrianglesNormal(meshCollider.sharedMesh.triangles.ToList(), meshCollider.sharedMesh.normals.ToList());
            CalcAreas(filteredTriangles, meshCollider.sharedMesh.vertices);
            for (int i = 0; i < pointCount; i++)
                addDebugPoint(filteredTriangles);
            
            FilterDebugPointsNoise();
        }

        [Button]
        public void RemoveDebugPoints()
        {
            debugPoints.Clear();
        }
        
        private void addDebugPoint(List<int> filteredTriangles)
        {
            randomPoint = GetRandomPointOnMesh(filteredTriangles, meshCollider.sharedMesh);
            randomPoint += meshCollider.transform.position;
            debugPoints.Add(randomPoint);
        }

        #endregion

        #region Points to Rooms

        [Button]
        public void AssignPointsToRooms()
        {
            caveNodePointDict.Clear();
            
            foreach (var vert in _caveGen.CaveGraph.Vertices)
            {
                GameObject roomObj = vert.GameObject;
                AssignPoints(roomObj);
            }
            
            if (!_assignPointsToTunnels)
                return;
                
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
            
            CaveNodeDataDebugComponent nodeDataDebug = nodeObj.GetComponent<CaveNodeDataDebugComponent>();
            int distanceFromMainPath = nodeDataDebug.CaveNodeData.MainPathDistance;

            //TODO: Make sure this collider is on the room bounds layer

            // Add point to room if within room collider bounds
            // Remove point from master list if too far from main path
            List<Vector3> pointsToRemove = new List<Vector3>();
            foreach (var point in debugPoints)
            {
                if (roomBounds.bounds.Contains(point))
                {
                    float stayChance =
                        _mainPathDecayCurve.Evaluate((float) distanceFromMainPath / _maxDistanceFromMainPath);
                    
                    //Use curve to remove points
                    if (Random.value < stayChance)
                        roomPoints.Add(point);
                    else
                        pointsToRemove.Add(point);
                }
            }

            debugPoints = debugPoints.Except(pointsToRemove).ToList();
            
            //Set points to room gizmo
            RenderDecorPoints roomDecorRend = nodeObj.GetComponentInChildren<RenderDecorPoints>();
            roomDecorRend.ClearDebugPoints();
            roomDecorRend.SetDebugPoints(roomPoints, _pointRadius);
        }

        [Button]
        public void ClearPointsFromRooms()
        {
            foreach (var decorRend in FindObjectsOfType<RenderDecorPoints>())
            {
                decorRend.ClearDebugPoints();
            }
        }

        #endregion
        
        #region Filtering

        private List<int> FilterTrianglesNormal(List<int> triangles, List<Vector3> normals)
        {
            List<int> filteredTriangles = new List<int>();
            for(int i = 0; i < triangles.Count;)
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
            }

            return filteredTriangles;
        }

        private void FilterDebugPointsNoise()
        {
            List<Vector3> pointsToRemove = new List<Vector3>();
            foreach (var point in debugPoints)
            {
                if (Mathf.Clamp01(Perlin.Noise(point * _noiseScale)) < _noiseFilterValueMin)
                    pointsToRemove.Add(point);
            }

            debugPoints = debugPoints.Except(pointsToRemove).ToList();
        }

        #endregion

        #region Util

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

            pointCount = Mathf.FloorToInt(total / _pointDensity);
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

        #endregion
    }
}