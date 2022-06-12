using System.Linq;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts.Cave.DirectedGraph
{
    public class CaveGraph : DirectedGraph<Vector3, CaveNodeData, CaveNodeConnectionData>
    {
        private Bounds _voxelBounds;
        private Transform _voxelBoundsTransform;
        
        public CaveGraph(Bounds voxelBounds, Transform voxelBoundsTransform)
        {
            _voxelBounds = voxelBounds;
            _voxelBoundsTransform = voxelBoundsTransform;
        }
        
        #region Adjacency

        public void ConnectNearestNeighbors(int n)
        {
            var nodeNeighbors = this.GetAllNodesUnordered()
                .Select(node =>
                {
                    var neighbors = this.GetAllNodesUnordered()
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
                    var connectionData = new CaveNodeConnectionData();
                    node.node.AddConnection(neighbor.otherNode, connectionData);
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
                
            }
            
            

            return voxels;
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