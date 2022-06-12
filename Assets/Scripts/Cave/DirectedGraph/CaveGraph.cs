using System.Linq;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts.Cave.DirectedGraph
{
    public class CaveGraph : DirectedGraph<Vector3, CaveNodeData, CaveNodeConnectionData>
    {
        private Bounds _voxelBounds;
        
        public CaveGraph(Bounds voxelBounds)
        {
            _voxelBounds = voxelBounds;
        }
        
        #region Adjacency

        public void ConnectNearestNeighbors(int n)
        {
            var nodeNeighbors = this.GetAllNodesUnordered()
                .Select(node =>
                {
                    var neighbors = this.GetAllNodesUnordered()
                        .Select(otherNode => (otherNode: otherNode,
                            distance: Vector3.Distance(node.Data.Position, otherNode.Data.Position)))
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
            Vector3Int numVoxels = Vector3.Scale(size, grid.cellSize.Inverse()).FloorToInt();
            
            var voxels = new VoxelData[numVoxels.x,numVoxels.y,numVoxels.z];

            float x = 0, y = 0, z = 0;
            for (int ix = 0; ix < numVoxels.x; ix++, x += grid.cellSize.x)
            {
                for (int iy = 0; iy < numVoxels.y; iy++, y += grid.cellSize.y)
                {
                    for (int iz = 0; iz < numVoxels.z; iz++, z += grid.cellSize.z)
                    {
                        var value = Random.Range(0f, 1f);
                        voxels[ix, iy, iz] = new VoxelData(value);
                    }
                }   
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
                var parentPosition = _voxelBounds.min;
                
                var position = node.Data.Position + parentPosition;
                var radius = node.Data.Size;
                Gizmos.DrawSphere(position, radius);

                foreach (var connection in node.Connections)
                {
                    Gizmos.DrawLine(connection.FromNode.Data.Position + parentPosition, connection.ToNode.Data.Position + parentPosition);
                }
            }
        }
        
        #endregion
    }
}