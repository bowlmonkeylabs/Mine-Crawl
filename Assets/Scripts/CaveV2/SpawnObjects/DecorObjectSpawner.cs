using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    [ExecuteAlways]
    public class DecorObjectSpawner : MonoBehaviour
    {
        [TitleGroup("Execution"), SerializeField] private bool _generateOnSpawnLevelObjects = true;
        [TitleGroup("Execution"),  SerializeField] private GameEvent _onAfterGenerateLevelObjects;
        [TitleGroup("References"),  SerializeField] private CaveGenComponentV2 _caveGen;
        [TitleGroup("References"),  SerializeField] private GameObject _mudRendererObj;
        [TitleGroup("Object Settings"), SerializeField] private LayerMask _roomBoundsLayerMask;
        [TitleGroup("Object Settings"),  SerializeField] private GameObject _prefabToSpawn;
        [TitleGroup("Object Settings"), SerializeField] private bool _spawnAsPrefab = true;

        [TitleGroup("Filtering"), SerializeField] private float _pointDensity = .25f;
        [TitleGroup("Filtering"), SerializeField, MinMaxSlider(0f, 180f)] private Vector2 _minMaxAngle = new Vector2(0f, 180f);
        [TitleGroup("Filtering"), SerializeField, Range(0, 1)] private float _noiseScale = 100f;
        [TitleGroup("Filtering"), SerializeField, Range(0, 1)] private float _noiseFilterValueMin = .5f;
        [TitleGroup("Filtering"), SerializeField] private bool _spawnInStartRoom;
        [TitleGroup("Filtering"), SerializeField] private bool _spawnInEndRoom;
        [TitleGroup("Filtering"), SerializeField] private int _maxDistanceFromMainPath = 10;
        [TitleGroup("Filtering"), SerializeField] private AnimationCurve _mainPathDecayCurve;
        
        [FoldoutGroup("Debug"), SerializeField] private float _pointRadiusDebug = .25f;
        [FoldoutGroup("Debug"), SerializeField] private float _pointNormalLengthDebug = 1f;
        [FoldoutGroup("Debug"), SerializeField] private bool _drawClosestPoints;
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private int pointCount;
        [FoldoutGroup("Debug"), ReadOnly] private List<Point> _points = new List<Point>();
        [FoldoutGroup("Debug"), SerializeField] private Vector3 _noiseBoxCount = new Vector3(100f, 100f, 100f);
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly] private List<NoiseBox> _noiseBoxes = new List<NoiseBox>();

        private MeshCollider meshCollider;

        private Dictionary<Point, Vector3> pointToClosestPointDebug = new Dictionary<Point, Vector3>();
        
        private float[] sizes;
        private float[] cumulativeSizes;
        private float totalArea = 0;

        public class Point
        {
            public Vector3 pos;
            public Vector3 normal;
        }

        struct NoiseBox
        {
            public Vector3 pos;
            public Vector3 scale;
        }

        #region Unity Lifecycle
        
        private void OnEnable()
        {
            _onAfterGenerateLevelObjects.Subscribe(GenerateDecorOnSpawnLevelObjects);
        }

        private void OnDisable()
        {
            _onAfterGenerateLevelObjects.Unsubscribe(GenerateDecorOnSpawnLevelObjects);
        }

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
                foreach (var pointKV in pointToClosestPointDebug)
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

        #region Generate/Remove Decor

        [Button, PropertyOrder(-1)]
        public void GenerateDecor()
        {
            GeneratePoints();
            AssignPointsToRooms();
            SpawnObjects();
        }

        [Button, PropertyOrder(-1)]
        public void DestroyDecor()
        {
            RemovePoints();
            ClearPointsFromRoomsDebug();
            RemoveObjects();
        }
        
        private void GenerateDecorOnSpawnLevelObjects()
        {
            if (_generateOnSpawnLevelObjects)
                GenerateDecor();
        }

        #endregion

        #region Show/Remove Points
        
        private const int STEP_ID = 3;

        [Button, FoldoutGroup("Debug")]
        public void GeneratePoints()
        {
            Random.InitState(_caveGen.CaveGenParams.Seed + STEP_ID);
            RemovePoints();
            InitMeshData(out var triangles, out var vertices, out var normals);
            triangles = FilterTrianglesNormal(triangles, normals);
            (sizes, cumulativeSizes, totalArea) = MeshUtils.CalcAreas(triangles, vertices);
            pointCount = Mathf.FloorToInt(totalArea * _pointDensity);
            for (int i = 0; i < pointCount; i++)
                AddPoint(triangles, vertices, normals);
            
            FilterPointsNoise();
            FilterPointsRadius();
        }

        public void FilterPointsRadius()
        {
            for (int i = 0; i < param._points.Count; i++)
            {
                List<Point> pointsToRemove = new List<Point>();
                for (int j = i + 1; j < param._points.Count; j++)
                {
                    if (Vector3.SqrMagnitude(param._points[i].pos - param._points[j].pos) <= Mathf.Pow(param._minRadius, 2f))
                        pointsToRemove.Add(param._points[j]);
                }

                param._points = param._points.Except(pointsToRemove).ToList();
            }
        }

        [Button, FoldoutGroup("Debug")]
        public void RemovePoints()
        {
            _points.Clear();
        }

        private void InitMeshData(out List<int> triangles, out List<Vector3> vertices, out List<Vector3> normals)
        {
            meshCollider = _mudRendererObj.GetComponent<MeshCollider>();
            triangles = meshCollider.sharedMesh.triangles.ToList();
            vertices = meshCollider.sharedMesh.vertices.ToList();
            normals = meshCollider.sharedMesh.normals.ToList();
        }
        
        private void AddPoint(List<int> triangles, List<Vector3> vertices, List<Vector3> normals)
        {
            Point randomPoint = new Point();
            (randomPoint.pos, randomPoint.normal) = 
                MeshUtils.GetRandomPointOnMeshAreaWeighted(triangles, vertices, normals, sizes, cumulativeSizes, totalArea);
            randomPoint.pos += meshCollider.transform.position;
            _points.Add(randomPoint);
        }

        #endregion

        #region Spawn Objects

        [Button, FoldoutGroup("Debug")]
        public void SpawnObjects()
        {
            RemoveObjects();
            int count = 0;
            foreach (var point in _points)
            {
                var newGameObj = GameObjectUtils.SafeInstantiate(_spawnAsPrefab, _prefabToSpawn, transform);
                newGameObj.transform.position = point.pos;
                newGameObj.transform.up = point.normal;
                count++;
            }
            //Debug.Log($"Spawned {count} Decor Objs on {pointCount} points");
        }

        [Button, FoldoutGroup("Debug")]
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

        [Button, FoldoutGroup("Debug")]
        public void AssignPointsToRooms()
        {
#if UNITY_EDITOR
            // Only needed to clear debug point renderers
            ClearPointsFromRooms();
#endif

            List<Point> pointsToRemove = new List<Point>();
            pointToClosestPointDebug.Clear();
            foreach (var point in _points)
            {
                bool removePoint = false;
                
                var containingRoom = _caveGen.CaveGraph.FindContainingRoom(point.pos);
                if (containingRoom.nodeData == null)
                {
                    removePoint = true;
                }
                
                Random.InitState(STEP_ID);
                removePoint |= FilterPointDistanceToMainPath(containingRoom.nodeData.MainPathDistance);
                removePoint |= FilterPointStartEndRoom(containingRoom.nodeData);

                if (removePoint)
                {
                    pointsToRemove.Add(point);
                }
                else
                {
                    //Set points to room
                    RenderDecorPoints roomDecorRend = roomObj.GetComponentInChildren<RenderDecorPoints>();
                    roomDecorRend.AddPoint(point, _pointRadiusDebug);
                    pointToClosestPointDebug[point] = closestRoomInfo.closestPoint;
                }
            }

            _points = _points.Except(pointsToRemove).ToList();
            pointCount = _points.Count;
        }

        private void ClearPointsFromRooms()
        {
            if (_caveGen.CaveGraph.VertexCount < 1)
                Debug.LogError("Cave graph has no vertices.");
            if (_caveGen.CaveGraph.EdgeCount < 1)
                Debug.LogError("Cave graph has no edges.");

            foreach (var vert in _caveGen.CaveGraph.Vertices)
            {
                RenderDecorPoints roomDecorRend = vert.GameObject.GetComponentInChildren<RenderDecorPoints>();
                roomDecorRend.ClearPoints();
            }

            foreach (var edge in _caveGen.CaveGraph.Edges)
            {
                RenderDecorPoints roomDecorRend = edge.GameObject.GetComponentInChildren<RenderDecorPoints>();
                roomDecorRend.ClearPoints();
            }
        }

        [Button, FoldoutGroup("Debug"), LabelText("Clear points from all rooms")]
        public void ClearPointsFromRoomsDebug()
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
                Vector3 faceNormal = MeshUtils.GetTriangleFaceNormal(triangles, normals, i);

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
        
        
        private bool FilterPointDistanceToMainPath(int distToMainPath)
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
                return false;
            else
                return true;
            
        }

        private bool FilterPointStartEndRoom(ICaveNodeData caveNodeData)
        {
            if (!_spawnInStartRoom && roomObj == _caveGen.CaveGraph.StartNode.GameObject)
                return true;
            if (!_spawnInEndRoom && roomObj == _caveGen.CaveGraph.EndNode.GameObject)
                return true;

            return false;   
        }

        #endregion
        
        #region Visualize Noise

        [Button, FoldoutGroup("Debug")]
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
                        _noiseBoxes.Add(noiseBox);
                    }
                }
            }
        }

        [Button, FoldoutGroup("Debug")]
        public void UnVisualizeNoise()
        {
            _noiseBoxes.Clear();
        }

        #endregion

        #region Util

        private List<Collider> GetRoomBoundsColliders(GameObject roomObj)
        {
            List<Collider> boundsColliders = new List<Collider>();
            Collider[] allRoomColliders = roomObj.GetComponentsInChildren<Collider>();
            foreach (var col in allRoomColliders)
            {
                if (col.gameObject.IsInLayerMask(_roomBoundsLayerMask))
                {
                    boundsColliders.Add(col);
                }
            }

            return boundsColliders;
        }
        
        
        private (bool isInCollider, Vector3 closestPoint) IsPointWithinCollider(Collider col, Vector3 point)
        {
            Vector3 closestPoint = col.ClosestPoint(point);
            return (closestPoint == point, closestPoint);
        }

        #endregion
        
    }
}