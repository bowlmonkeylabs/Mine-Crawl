using System;
using System.Linq;
using BML.Scripts.Utils;
using Clayxels;
using UnityEngine;

namespace BML.Scripts.Cave.DirectedGraph
{
    public class CaveGraph : DirectedGraph<Vector3, CaveNodeData, CaveNodeConnectionData>
    {
        private Bounds _voxelBounds;
        private Transform _voxelBoundsTransform;
        private Collider _traverseCheckCollider;

        private float _traverseMinSize;
        
        public CaveGraph(Bounds voxelBounds, Transform voxelBoundsTransform, Collider traverseCheckCollider)
        {
            _voxelBounds = voxelBounds;
            _voxelBoundsTransform = voxelBoundsTransform;
            _traverseCheckCollider = traverseCheckCollider;
            _traverseMinSize = _traverseCheckCollider.bounds.extents.magnitude * 2;
        }

        public Vector3 NodeLocalToWorld(Vector3 nodeLocal)
        {
            return nodeLocal + _voxelBounds.min + _voxelBoundsTransform.position;
        }
        
        #region Adjacency

        public void ConnectNearestNeighbors(int n)
        {
            var nodeNeighbors = this.GetAllNodesUnordered()
                .Select(node =>
                {
                    var neighbors = this.GetAllNodesUnordered()
                        .Where(otherNode => otherNode.Key != node.Key)
                        .Select(otherNode => (otherNode: otherNode,
                            distance: Vector3.Distance(node.Data.LocalPosition, otherNode.Data.LocalPosition)))
                        .OrderBy(tuple => tuple.distance)
                        .Take(n)
                        .ToList();
                    return (node: node, neighbors: neighbors);
                })
                .AsParallel()
                .ToList();

            foreach (var node in nodeNeighbors)
            {
                foreach (var neighbor in node.neighbors)
                {
                    // var connectionRadius = Math.Max(node.node.Data.Size, neighbor.otherNode.Data.Size);
                    var connectionRadius = Math.Min(node.node.Data.Size, neighbor.otherNode.Data.Size) * 0.5f;
                    connectionRadius = Math.Max(connectionRadius, _traverseMinSize);
                    var connectionData = new CaveNodeConnectionData(connectionRadius);
                    this.AddConnection(node.node, neighbor.otherNode, connectionData);
                }
            }
            
        }

        #endregion
        
        #region Rendering to voxel representation

        public struct VoxelData
        {
            public float Value { get; private set; }

            public VoxelData(float value)
            {
                Value = value;
            }
        }

        public VoxelData[,,] GetVoxels(Grid grid)
        {
            Vector3 size = _voxelBounds.max - _voxelBounds.min;
            // Vector3Int numVoxels = Vector3.Scale(size, grid.cellSize.Inverse()).FloorToInt();
            Vector3Int numVoxels = grid.GetContainedCellsSize(_voxelBounds);
            Debug.Log($"Get Voxels: {size} | {numVoxels}");
            
            var voxels = new VoxelData[numVoxels.x,numVoxels.y,numVoxels.z];

            float x = 0, y = 0, z = 0;
            for (int ix = 0; ix < numVoxels.x; ix++, x += grid.cellSize.x)
            {
                for (int iy = 0; iy < numVoxels.y; iy++, y += grid.cellSize.y)
                {
                    for (int iz = 0; iz < numVoxels.z; iz++, z += grid.cellSize.z)
                    {
                        // var rand = Random.Range(0, 0.15f);
                        // var value = (float) iy / numVoxels.y;
                        // voxels[ix, iy, iz] = new VoxelData(value + rand);
                        voxels[ix, iy, iz] = new VoxelData(0);
                    }
                }   
            }
            
            foreach (var node in GetAllNodesUnordered())
            {
                // var cellPosition = grid.LocalToCell(node.Data.Position);

                var stampSize = Vector3.one * node.Data.Size;
                var stampCenterWorld = _voxelBoundsTransform.position
                                       + _voxelBounds.min + node.Data.LocalPosition;
                // stampCenterWorld = stampCenterWorld + stampSize.oyo() / 2;
                stampCenterWorld = grid.GetCellCenterWorld(grid.WorldToCell(stampCenterWorld));
                var stampBounds = new Bounds(stampCenterWorld, stampSize);
                // Debug.Log($"Stamp voxels: Local: {node.Data.LocalPosition} | World: {stampCenterWorld} | Size: {stampSize}");

                foreach (var cellPosition in grid.GetCellsOverlapping(stampBounds))
                {
                    var cellIndex = grid.CellToBoundsRelativeCell(_voxelBounds, cellPosition);
                    bool inBounds = (cellIndex.x >= 0 && cellIndex.x < numVoxels.x) &&
                                    (cellIndex.y >= 0 && cellIndex.y < numVoxels.y) &&
                                    (cellIndex.z >= 0 && cellIndex.z < numVoxels.z);
                    // Debug.Log($"Cell: {cellPosition} | Index: {cellIndex} | In bounds: {inBounds}");
                    if (inBounds)
                    {
                        var cellCenter = grid.GetCellCenterWorld(cellPosition);
                        var cellDistanceSqr = (cellCenter - stampCenterWorld).sqrMagnitude;
                        var distFactor = Mathf.InverseLerp(0.1f, stampSize.sqrMagnitude, cellDistanceSqr);
                        var value = Mathf.Lerp(1f, 0f, distFactor);

                        voxels[cellIndex.x, cellIndex.y, cellIndex.z] = new VoxelData(value);
                    }
                }

            }

            foreach (var nodeConnection in GetAllNodeConnectionsUnordered())
            {
                var localOffset = _voxelBoundsTransform.position + _voxelBounds.min;
                var fromCell = grid.WorldToCell(localOffset + nodeConnection.FromNode.Data.LocalPosition);
                var toCell = grid.WorldToCell(localOffset + nodeConnection.ToNode.Data.LocalPosition);
                // foreach (var cellPosition in grid.GetRandomPathBetween(fromCell, toCell))
                foreach (var cellPosition in grid.GetLineBetween(fromCell, toCell, nodeConnection.Data.Radius))
                {
                    var cellIndex = grid.CellToBoundsRelativeCell(_voxelBounds, cellPosition);
                    bool inBounds = (cellIndex.x >= 0 && cellIndex.x < numVoxels.x) &&
                                    (cellIndex.y >= 0 && cellIndex.y < numVoxels.y) &&
                                    (cellIndex.z >= 0 && cellIndex.z < numVoxels.z);
                    // Debug.Log($"Cell: {cellPosition} | Index: {cellIndex} | In bounds: {inBounds}");
                    if (inBounds)
                    {
                        voxels[cellIndex.x, cellIndex.y, cellIndex.z] = new VoxelData(1f);
                    }
                }

                break;
            }
            
            

            return voxels;
        }

        #endregion
        
        #region Render to Clayxels

        public void DestroyClayxels(ClayContainer clayContainer)
        {
            if (clayContainer.isFrozen())
            {
                clayContainer.defrostToLiveClayxels();
            }
            
            var clayGameObjects = Enumerable.Range(0, clayContainer.getNumClayObjects())
                .Select(index => clayContainer.getClayObject(index).gameObject)
                .ToList();
            foreach (var gameObject in clayGameObjects)
            {
                if (gameObject != null)
                    GameObject.DestroyImmediate(gameObject);
            }
            
            clayContainer.scanClayObjectsHierarchy();
            clayContainer.computeClay();
        }
        
        public void GetClayxels(ClayContainer clayContainer, float nodeBlend, float connectionBlend, bool renderOutside, 
            GameObject clayObjectPrefab, bool generateCollisionMesh, bool renderLiveClayxels)
        {
            DestroyClayxels(clayContainer);
            
            // Render overall object for subtraction
            if (!renderOutside)
            {
                var position = _voxelBounds.center;
                var scale = _voxelBounds.size;
                
                var clayObject = clayContainer.addClayObject();
                clayObject.transform.position = position;
                clayObject.transform.localScale = scale;
                clayObject.setBlend(100f);
                clayContainer.clayObjectUpdated(clayObject);
            }

            int blendSign = (renderOutside ? 1 : -1);
            nodeBlend *= blendSign;
            connectionBlend *= blendSign;

            
            
            // Render nodes
            foreach (var node in GetAllNodesUnordered())
            {
                var nodeWorld = NodeLocalToWorld(node.Data.LocalPosition);
                
                var newGameObject = GameObject.Instantiate(clayObjectPrefab, clayContainer.transform);
                var clayObject = newGameObject.GetComponent<ClayObject>();
                // var clayObject = clayContainer.addClayObject();
                clayObject.transform.position = nodeWorld;
                clayObject.transform.localScale.Scale(Vector3.one * node.Data.Size);
                clayObject.setBlend(nodeBlend);
                clayContainer.clayObjectUpdated(clayObject);
            }
            
            // Render connections
            foreach (var nodeConnection in GetAllNodeConnectionsUnordered())
            {
                var diff = nodeConnection.ToNode.Data.LocalPosition - nodeConnection.FromNode.Data.LocalPosition;
                var center = nodeConnection.FromNode.Data.LocalPosition + diff / 2;
                var connectionCenterWorld = NodeLocalToWorld(center);
                
                var length = diff.magnitude;
                var scale = new Vector3(nodeConnection.Data.Radius, length, nodeConnection.Data.Radius);

                var forward = new Vector3(-diff.y, diff.x, diff.x);
                forward = Vector3.down;
                var rotation = Quaternion.LookRotation(forward, diff);
                
                var clayObject = clayContainer.addClayObject();
                clayObject.transform.position = connectionCenterWorld;
                // clayObject.transform.localScale = Vector3.one * node.Data.Size;
                clayObject.transform.localScale = scale;
                clayObject.transform.rotation = rotation;
                clayObject.setBlend(connectionBlend);
                clayObject.color = Color.red;
                clayContainer.clayObjectUpdated(clayObject);

                // break;
            }

            // Try anything to rerender the Clayxels without having to select the object in the hierarchy; nothing is working so far
            clayContainer.needsUpdate = true;
            clayContainer.scanClayObjectsHierarchy();
            clayContainer.forceUpdateAllSolids();
            clayContainer.solidUpdated(0);
            clayContainer.computeClay();

            if (generateCollisionMesh)
            {
                // Freeze Clayxels and get collision mesh
                clayContainer.freezeToMesh(clayContainer.getClayxelDetail());
                var meshCollider = clayContainer.GetComponent<MeshCollider>();
                var meshFilter = clayContainer.GetComponent<MeshFilter>();
                if (meshCollider != null && meshFilter != null)
                {
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                }

                if (renderLiveClayxels)
                {
                    clayContainer.defrostToLiveClayxels();
                }
            }

        }
        
        #endregion
        
        #region Unity lifecycle
        
        public void OnDrawGizmos(GameObject caller)
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

            foreach (var node in this.GetAllNodesUnordered())
            {
                var parentPosition = _voxelBounds.min + caller.transform.position;
                
                var position = node.Data.LocalPosition + parentPosition;
                var radius = node.Data.Size / 10f;
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(position, radius);

                foreach (var connection in node.Connections)
                {
                    Gizmos.DrawLine(connection.FromNode.Data.LocalPosition + parentPosition, connection.ToNode.Data.LocalPosition + parentPosition);
                }
            }
        }
        
        #endregion
    }
}