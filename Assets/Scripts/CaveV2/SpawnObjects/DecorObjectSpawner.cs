using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
        [SerializeField] private bool _drawClosestPoints;
        [SerializeField, ReadOnly] private List<Point> _points = new List<Point>();

        [ShowInInspector, ReadOnly] private int pointCount;
        
        private MeshCollider meshCollider;

        // Room obj and dist to main path
        private Dictionary<GameObject, int> roomDataDict =
            new Dictionary<GameObject, int>();

        private Dictionary<Point, Vector3> pointToClosestPoint = new Dictionary<Point, Vector3>();
        
        float[] sizes;
        float[] cumulativeSizes;
        float total = 0;

        [SerializeField] private List<NoiseBox> _noiseBoxes = new List<NoiseBox>();
        [SerializeField] private Vector3 _noiseBoxCount = new Vector3(100f, 100f, 100f);
        [SerializeField] [Range(0, 1)] private float _noiseScale = 100f;

        public class Point
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
            foreach (var point in _points)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(point.pos, _pointRadiusDebug);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(point.pos, point.pos + point.normal * _pointNormalLengthDebug);
            }

            if (_drawClosestPoints)
            {
                foreach (var pointKV in pointToClosestPoint)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(pointKV.Key.pos, pointKV.Value);
                    Gizmos.DrawSphere(pointKV.Value, _pointRadiusDebug/2f);
                }
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
                AddPoint(filteredTriangles);
            
            FilterPointsNoise();
        }

        [Button]
        public void RemoveDebugPoints()
        {
            _points.Clear();
        }
        
        private void AddPoint(List<int> filteredTriangles)
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
            GetRoomData();

            List<Point> pointsToRemove = new List<Point>();
            pointToClosestPoint.Clear();
            foreach (var point in _points)
            {
                bool foundRoom = false;
                (GameObject roomObj, int distFromMainPath) currentRoomInfo = (null, Int32.MaxValue);
                (GameObject roomObj, int distFromMainPath, float distanceFromPoint, Vector3 closestPoint) closestRoomInfo = (null, Int32.MaxValue, Mathf.Infinity, Vector3.zero);
                foreach (var roomData in roomDataDict)
                {
                    //TODO: Move to other method to fetch collider?
                    Collider boundsCollider = roomData.Key.GetComponentInChildren<Collider>();
                    Collider[] allRoomColliders = roomData.Key.GetComponentsInChildren<Collider>();
                    foreach (var col in allRoomColliders)
                    {
                        if (col.gameObject.IsInLayerMask(_roomBoundsLayerMask))
                        {
                            boundsCollider = col;
                            break;
                        }
                    }
                    
                    currentRoomInfo.roomObj = roomData.Key;
                    currentRoomInfo.distFromMainPath = roomData.Value;

                    //Stop if contained by room
                    if (IsPointWithinCollider(boundsCollider, point.pos))
                    {
                        foundRoom = true;
                        closestRoomInfo.closestPoint = point.pos;
                        break;
                    }
                    
                    //Else see how far this room is. Keep track of closest room
                    //TODO: dont calculate ClosestPoint twice (already done in IsPointWithinCollider)
                    Vector3 closestPoint = boundsCollider.ClosestPoint(point.pos);
                    float distToCollider = Vector3.Distance(point.pos , closestPoint);  //TODO: change to sqr dist
                    if (distToCollider < closestRoomInfo.distanceFromPoint)
                    {
                        closestRoomInfo.roomObj = currentRoomInfo.roomObj;
                        closestRoomInfo.distFromMainPath = currentRoomInfo.distFromMainPath;
                        closestRoomInfo.distanceFromPoint = distToCollider;
                        closestRoomInfo.closestPoint = closestPoint;
                    }
                }

                //TODO: Maybe theres better way to decouple this method call from this one
                bool removePoint;
                if (foundRoom)
                    removePoint = FilterPointDistanceToMainPath(point, currentRoomInfo.roomObj, currentRoomInfo.distFromMainPath);
                else
                    removePoint = FilterPointDistanceToMainPath(point, closestRoomInfo.roomObj, closestRoomInfo.distFromMainPath);

                if (removePoint)
                {
                    pointsToRemove.Add(point);
                }
                else
                {
                    pointToClosestPoint[point] = closestRoomInfo.closestPoint;
                }
            }

            _points = _points.Except(pointsToRemove).ToList();
        }

        private void GetRoomData()
        {
            if (_caveGen.CaveGraph.VertexCount < 1)
                Debug.LogError("Cave graph has no vertices.");
            if (_caveGen.CaveGraph.EdgeCount < 1)
                Debug.LogError("Cave graph has no edges.");

            roomDataDict.Clear();

            foreach (var vert in _caveGen.CaveGraph.Vertices)
            {
                GameObject vertObj = vert.GameObject;
                CaveNodeDataDebugComponent nodeDataDebug = vertObj.GetComponent<CaveNodeDataDebugComponent>();

                roomDataDict[vertObj] = nodeDataDebug.CaveNodeData.MainPathDistance;
                
                RenderDecorPoints roomDecorRend = vertObj.GetComponentInChildren<RenderDecorPoints>();
                roomDecorRend.ClearPoints();
            }

            foreach (var edge in _caveGen.CaveGraph.Edges)
            {
                GameObject edgeObj = edge.GameObject;
                CaveNodeConnectionDataDebugComponent  edgeDataDebug = edgeObj.GetComponent<CaveNodeConnectionDataDebugComponent>();
                
                // For edges, the dist to main path is max of connected rooms
                roomDataDict[edgeObj] = Mathf.Max(edgeDataDebug.CaveNodeConnectionData.Source.MainPathDistance,
                    edgeDataDebug.CaveNodeConnectionData.Target.MainPathDistance);
                
                RenderDecorPoints roomDecorRend = edgeObj.GetComponentInChildren<RenderDecorPoints>();
                roomDecorRend.ClearPoints();
            }
        }

        

        //TODO: Put this in some util class?
        private bool IsPointWithinCollider(Collider collider, Vector3 point)
        {
            return collider.ClosestPoint(point) == point;
        }

        [Button]
        public void ClearPointsFromRooms()
        {
            foreach (var decorRend in FindObjectsOfType<RenderDecorPoints>())
            {
                decorRend.ClearPoints();
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

        private void FilterPointsNoise()
        {
            List<Point> pointsToRemove = new List<Point>();
            foreach (var point in _points)
            {
                if (Mathf.Clamp01(Perlin.Noise(point.pos * _noiseScale)) < _noiseFilterValueMin)
                    pointsToRemove.Add(point);
            }

            _points = _points.Except(pointsToRemove).ToList();
        }
        
        private bool FilterPointDistanceToMainPath(Point point, GameObject roomObj, int distToMainPath)
        {
            float stayChance;

            if (distToMainPath > _maxDistanceFromMainPath)
                stayChance = 0f;
            // Avoid divide by 0
            else if (_maxDistanceFromMainPath == 0 &&
                     distToMainPath == _maxDistanceFromMainPath)
                stayChance = _mainPathDecayCurve.Evaluate(0);
            else
                stayChance = _mainPathDecayCurve.Evaluate((float) distToMainPath / _maxDistanceFromMainPath);

            //Use curve to remove points
            if (Random.value < stayChance)
            {
                //Set points to room
                RenderDecorPoints roomDecorRend = roomObj.GetComponentInChildren<RenderDecorPoints>();
                roomDecorRend.AddPoint(point, _pointRadiusDebug);
                return false;
            }
            else
            {
                //Tell caller to remove this point
                return true;
            }
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