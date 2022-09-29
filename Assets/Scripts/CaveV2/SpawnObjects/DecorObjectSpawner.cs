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
        [SerializeField] private GameObject _prefabToSpawn;
        [SerializeField] private bool _spawnAsPrefab = true;
        [SerializeField] private LayerMask _roomBoundsLayerMask;
        [SerializeField] [MinMaxSlider(0f, 180f)] private Vector2 _minMaxAngle = new Vector2(0f, 180f);
        [SerializeField] [Range(0, 1)] private float _noiseFilterValueMin = .5f;
        [SerializeField] private int _maxDistanceFromMainPath = 10;
        [SerializeField] private AnimationCurve _mainPathDecayCurve;

        [SerializeField] private float _pointDensity = .25f;
        [SerializeField] private float _pointRadiusDebug = .25f;
        [SerializeField] private float _pointNormalLengthDebug = 1f;
        [SerializeField, ReadOnly] private List<Point> _points = new List<Point>();

        [ShowInInspector, ReadOnly] private int pointCount;
        
        private MeshCollider meshCollider;

        private Dictionary<CaveNodeData, List<Vector3>> caveNodePointDict =
            new Dictionary<CaveNodeData, List<Vector3>>();

        float[] sizes;
        float[] cumulativeSizes;
        float total = 0;

        [SerializeField] private List<NoiseBox> _noiseBoxes = new List<NoiseBox>();
        [SerializeField] private Vector3 _noiseBoxCount = new Vector3(100f, 100f, 100f);
        [SerializeField] [Range(0, 1)] private float _noiseScale = 100f;

        public struct Point
        {
            public Vector3 pos;
            public Vector3 normal;
        }

        struct NoiseBox
        {
            public Vector3 pos;
            public Vector3 scale;
            public float value;
        }

        #region Unity Lifecycle

        void OnDrawGizmosSelected()
        {
            foreach (Point debugPoint in _points)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(debugPoint.pos, _pointRadiusDebug);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(debugPoint.pos, debugPoint.pos + debugPoint.normal * _pointNormalLengthDebug);
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
            _points.Clear();
        }
        
        private void addDebugPoint(List<int> filteredTriangles)
        {
            Point randomPoint = new Point();
            (randomPoint.pos, randomPoint.normal) = GetRandomPointOnMesh(filteredTriangles, meshCollider.sharedMesh);
            randomPoint.pos += meshCollider.transform.position;
            _points.Add(randomPoint);
        }

        #endregion

        #region Spawn Objects

        [Button]
        public void SpawnObjects()
        {
            RemoveObjects();
            foreach (var point in _points)
            {
                var newGameObj = GameObjectUtils.SafeInstantiate(_spawnAsPrefab, _prefabToSpawn, transform);
                newGameObj.transform.position = point.pos;
                newGameObj.transform.up = point.normal;
            }
        }

        [Button]
        public void RemoveObjects()
        {
            var children = Enumerable.Range(0, this.transform.childCount)
                .Select(i => this.transform.GetChild(i).gameObject)
                .ToList();
            foreach (var childObject in children)
            {
                GameObject.DestroyImmediate(childObject);
            }
        }

        #endregion

        #region Points to Rooms

        [Button]
        public void AssignPointsToRooms()
        {
            caveNodePointDict.Clear();
            
            if (_caveGen.CaveGraph.VertexCount < 1)
                Debug.LogError("Cave graph has no vertices.");
            if (_caveGen.CaveGraph.EdgeCount < 1)
                Debug.LogError("Cave graph has no edges.");
            
            foreach (var vert in _caveGen.CaveGraph.Vertices)
            {
                GameObject roomObj = vert.GameObject;
                CaveNodeDataDebugComponent nodeDataDebug = roomObj.GetComponent<CaveNodeDataDebugComponent>();
                int distanceFromMainPath = nodeDataDebug.CaveNodeData.MainPathDistance;
                AssignPoints(roomObj, distanceFromMainPath);
            }

            foreach (var edge in _caveGen.CaveGraph.Edges)
            {
                GameObject edgeObj = edge.GameObject;
                CaveNodeConnectionDataDebugComponent  edgeDataDebug = edgeObj.GetComponent<CaveNodeConnectionDataDebugComponent>();
                int distanceFromMainPath = Mathf.Max(edgeDataDebug.CaveNodeConnectionData.Source.MainPathDistance,
                                                    edgeDataDebug.CaveNodeConnectionData.Target.MainPathDistance);
                AssignPoints(edgeObj, distanceFromMainPath);
            }
        }
        

        private void AssignPoints(GameObject nodeObj, int distToMainPath)
        {
            List<Point> roomPoints = new List<Point>();
            Collider roomBounds = nodeObj.GetComponentInChildren<Collider>();

            //TODO: Make sure this collider is on the room bounds layer

            // Add point to room if within room collider bounds
            // Remove point from master list if too far from main path
            List<Point> pointsToRemove = new List<Point>();
            foreach (var point in _points)
            {
                if (roomBounds.bounds.Contains(point.pos))
                {
                    float stayChance = distToMainPath > _maxDistanceFromMainPath ?
                                        0f : 
                                        _mainPathDecayCurve.Evaluate((float) distToMainPath / _maxDistanceFromMainPath);

                    //Use curve to remove points
                    if (Random.value < stayChance)
                        roomPoints.Add(point);
                    else
                        pointsToRemove.Add(point);
                }
            }

            _points = _points.Except(pointsToRemove).ToList();
            
            //Set points to room gizmo
            RenderDecorPoints roomDecorRend = nodeObj.GetComponentInChildren<RenderDecorPoints>();
            roomDecorRend.ClearDebugPoints();
            roomDecorRend.SetDebugPoints(roomPoints, _pointRadiusDebug);
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
            for(int i = 0; i < triangles.Count; i+=3)
            {
                Vector3 faceNormal = GetTriangleFaceNormal(triangles, normals, i);

                if (Vector3.Angle(faceNormal, Vector3.up) >= _minMaxAngle.x &&
                    Vector3.Angle(faceNormal, Vector3.up) <= _minMaxAngle.y)
                {
                    filteredTriangles.Add(triangles[i]);
                    filteredTriangles.Add(triangles[i+1]);
                    filteredTriangles.Add(triangles[i+2]);
                }
            }

            return filteredTriangles;
        }

        private void FilterDebugPointsNoise()
        {
            List<Point> pointsToRemove = new List<Point>();
            foreach (var point in _points)
            {
                if (Mathf.Clamp01(Perlin.Noise(point.pos * _noiseScale)) < _noiseFilterValueMin)
                    pointsToRemove.Add(point);
            }

            _points = _points.Except(pointsToRemove).ToList();
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

            pointCount = Mathf.FloorToInt(total * _pointDensity);
        }

        public (Vector3, Vector3) GetRandomPointOnMesh(List<int> filteredTriangles, Mesh mesh)
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
            
            // Get Face Normal at Point
            Vector3 faceNormal = GetTriangleFaceNormal(filteredTriangles, mesh.normals.ToList(), triIndex);
            
            return (pointOnMesh, faceNormal);
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
        
        private Vector3 GetTriangleFaceNormal(List<int> triangles, List<Vector3> normals, int triangleStartIndex)
        {
            int i = triangleStartIndex;
            
            Vector3 N1 = normals[triangles[i]];
            i++;
            Vector3 N2 = normals[triangles[i]];
            i++;
            Vector3 N3 = normals[triangles[i]];
            i++;

            return (N1 + N2 + N3) / 3;
        }

        #endregion
    }
}