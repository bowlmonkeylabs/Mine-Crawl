using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using Mono.CSharp;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.Utils
{
    public static class GridUtil
    {
        #region Bounding box utilities
        
        public static IEnumerable<Vector3Int> GetCellsOverlapping(this Grid grid, Bounds bounds)
        {
            var cellMin = grid.WorldToCell(bounds.min);
            var cellMax = grid.WorldToCell(bounds.max);

            for (int ix = cellMin.x; ix <= cellMax.x; ix++)
            {
                for (int iy = cellMin.y; iy <= cellMax.y; iy++)
                {
                    for (int iz = cellMin.z; iz <= cellMax.z; iz++)
                    {
                        var cellPosition = new Vector3Int(ix, iy, iz);
                        yield return cellPosition;
                    }
                }
            }
        }
        
        public static IEnumerable<Vector3Int> GetCellsContained(this Grid grid, Bounds bounds)
        {
            var boundsMinCell = grid.WorldToCell(bounds.min);
            Vector3 boundsMinCellWorld = grid.CellToWorld(boundsMinCell);
            if (boundsMinCellWorld.x < bounds.min.x)
            {
                boundsMinCell.x += 1;
            }
            if (boundsMinCellWorld.y < bounds.min.y)
            {
                boundsMinCell.y += 1;
            }
            if (boundsMinCellWorld.z < bounds.min.z)
            {
                boundsMinCell.z += 1;
            }
            
            var boundsMaxCell = grid.WorldToCell(bounds.max) + Vector3Int.one;
            Vector3 boundsMaxCellWorld = grid.CellToWorld(boundsMaxCell);
            if (boundsMaxCellWorld.x > bounds.max.x)
            {
                boundsMaxCell.x -= 1;
            }
            if (boundsMaxCellWorld.y > bounds.max.y)
            {
                boundsMaxCell.y -= 1;
            }
            if (boundsMaxCellWorld.z > bounds.max.z)
            {
                boundsMaxCell.z -= 1;
            }
            
            for (int ix = boundsMinCell.x; ix <= boundsMaxCell.x; ix++)
            {
                for (int iy = boundsMinCell.y; iy <= boundsMaxCell.y; iy++)
                {
                    for (int iz = boundsMinCell.z; iz <= boundsMaxCell.z; iz++)
                    {
                        var cellPosition = new Vector3Int(ix, iy, iz);
                        var cellWorldMin = grid.CellToWorld(cellPosition);
                        var cellWorldMax = cellWorldMin + grid.cellSize;
                        var cellContained = bounds.Contains(cellWorldMin) && bounds.Contains(cellWorldMax);
                        if (cellContained)
                        {
                            yield return cellPosition;
                        }
                    }
                }
            }
        }
        
        public static Vector3Int GetContainedCellsSize(this Grid grid, Bounds bounds)
        {
            var boundsMinCell = grid.WorldToCell(bounds.min);
            Vector3 boundsMinCellWorld = grid.CellToWorld(boundsMinCell);
            if (boundsMinCellWorld.x < bounds.min.x)
            {
                boundsMinCell.x += 1;
            }
            if (boundsMinCellWorld.y < bounds.min.y)
            {
                boundsMinCell.y += 1;
            }
            if (boundsMinCellWorld.z < bounds.min.z)
            {
                boundsMinCell.z += 1;
            }
            
            var boundsMaxCell = grid.WorldToCell(bounds.max) + Vector3Int.one;
            Vector3 boundsMaxCellWorld = grid.CellToWorld(boundsMaxCell);
            if (boundsMaxCellWorld.x > bounds.max.x)
            {
                boundsMaxCell.x -= 1;
            }
            if (boundsMaxCellWorld.y > bounds.max.y)
            {
                boundsMaxCell.y -= 1;
            }
            if (boundsMaxCellWorld.z > bounds.max.z)
            {
                boundsMaxCell.z -= 1;
            }

            return boundsMaxCell - boundsMinCell;
        }

        // WIP
        public static IEnumerable<Vector3Int> GetCellsOverlappingLine(this Grid grid, Vector3 to, Vector3 from, float radius)
        {
            return null;
        }

        // WIP
        public static IEnumerable<Vector3Int> GetCellsOverlappingCollider(this Grid grid, Collider collider)
        {
            foreach (var cellPosition in GetCellsOverlapping(grid, collider.bounds))
            {
                var cellCenter = grid.GetCellCenterWorld(cellPosition);
                var cellBounds = new Bounds(cellCenter, grid.cellSize);
                
                // TODO more complex colliider test
                var inBounds = collider.bounds.Intersects(cellBounds);
                if (inBounds)
                {
                    yield return cellPosition;
                }
            }

            // return null;
        }
        
        // WIP
        public static IEnumerable<Vector3Int> GetRandomPathBetween(this Grid grid, Vector3Int from, Vector3Int to)
        {
            Debug.Log($"Path Between: From: {from} | To: {to}");
            Vector3Int curr = from;
            int guard = 30;
            while (curr != to && guard > 0)
            {
                var remainingMoves = to - curr;
                int randomAxisIndex = Random.Range(0, 3);
                curr[randomAxisIndex] += 1 * Math.Sign(remainingMoves[randomAxisIndex]);
                Debug.Log($"Curr: {curr}");
                guard--;
                yield return curr;
            }
        }
        
        // WIP
        public static IEnumerable<Vector3Int> GetLineBetween(this Grid grid, Vector3Int from, Vector3Int to)
        {
            var diff = to - from;
            int dx = Math.Abs(diff.x), sx = Math.Sign(diff.x);
            int dy = Math.Abs(diff.y), sy = Math.Sign(diff.y);
            int dz = Math.Abs(diff.z), sz = Math.Sign(diff.z);
            int dm = Math.Max(dx, Math.Max(dy, dz)), i = dm; /* maximum difference */
            // to.x = to.y = to.z = dm/2; /* error offset */

            Debug.Log($"Line between: From: {from} | To: {to} | DM: {dm}");
            // yield return from;

            Vector3Int curr = from;
            Vector3Int dest = to;
            for(;;) {  /* loop */
                yield return curr;
                if (i-- == 0) break;
                dest.x -= dx; if (dest.x < 0) { dest.x += dm; curr.x += sx; } 
                dest.y -= dy; if (dest.y < 0) { dest.y += dm; curr.y += sy; } 
                dest.z -= dz; if (dest.x < 0) { dest.z += dm; curr.z += sz; } 
            }
        }
        
        // WIP
        public static IEnumerable<Vector3Int> GetLineBetween(this Grid grid, Vector3Int from, Vector3Int to, float width)
        {
            Debug.Log($"Line between: From: {from} | To: {to} | DM: {width}");
            var sqrWidth = width * width;
            var diff = to - from;
            var bounds = new Bounds(to + diff / 2, diff);
            foreach (var cellPosition in grid.GetCellsOverlapping(bounds))
            {
                var cellCenter = grid.GetCellCenterWorld(cellPosition);
                var cellDiff = cellCenter - from;
                var sqrDistFromLine = (cellDiff - Vector3.Project(cellDiff, diff)).sqrMagnitude;
                if (sqrDistFromLine <= sqrWidth)
                {
                    yield return cellPosition;
                }
            }
            
        }

        #endregion
        
        public static void OnDrawGizmos(this Grid grid, GameObject caller, Bounds bounds)
        {
            #if UNITY_EDITOR
            Camera camera;
            SceneView sceneView = UnityEditor.SceneView.currentDrawingSceneView;
            if (sceneView != null && sceneView.camera != null)
            {
                camera = sceneView.camera;
            }
            else
            {
                camera = Camera.main;
            }
            #else
            Camera camera = Camera.main;
            #endif
            var cameraPosition = camera.transform.position;

            var sqrDistToCenter = (bounds.center - cameraPosition).sqrMagnitude;
            var approxClosest = sqrDistToCenter - bounds.extents.sqrMagnitude;
            var approxFurthest = sqrDistToCenter + bounds.extents.sqrMagnitude;

            Vector3Int? boundsOrigin = null;
            foreach (var cellPosition in grid.GetCellsContained(bounds))
            {
                if (boundsOrigin == null)
                {
                    boundsOrigin = cellPosition;
                    var boundsOriginWorld = grid.CellToWorld(boundsOrigin.Value);
                    Gizmos.DrawCube(boundsOriginWorld, Vector3.one * 0.1f);
                }
                
                var worldPosition = grid.GetCellCenterWorld(cellPosition);

                var sqrDistToCamera = (worldPosition - cameraPosition).sqrMagnitude;
                var cameraDistanceFactor = Mathf.InverseLerp(approxClosest, approxFurthest, sqrDistToCamera);
                var color = Color.Lerp(Color.white, Color.gray, cameraDistanceFactor);

                Gizmos.color = color;
                Gizmos.DrawWireCube(worldPosition, grid.cellSize);
            }
        }

        #region Convert coordinates
        
        public static Vector3Int BoundsLocalToCell(this Grid grid, Bounds bounds, Vector3 boundsLocal)
        {
            var world = bounds.min + boundsLocal;
            var cell = grid.WorldToCell(world);
            return cell;
        }
        
        public static Vector3Int CellToBoundsRelativeCell(this Grid grid, Bounds bounds, Vector3Int cellPosition)
        {
            // Find the first cell is completely within the bounds
            Vector3Int boundsMinCell = grid.WorldToCell(bounds.min);
            Vector3 boundsMinCellWorld = grid.CellToWorld(boundsMinCell);
            if (boundsMinCellWorld.x < bounds.min.x)
            {
                boundsMinCell.x += 1;
            }
            if (boundsMinCellWorld.y < bounds.min.y)
            {
                boundsMinCell.y += 1;
            }
            if (boundsMinCellWorld.z < bounds.min.z)
            {
                boundsMinCell.z += 1;
            }
            
            var boundsRelativeCellPosition = cellPosition - boundsMinCell;
            return boundsRelativeCellPosition;
        }
        
        #endregion
        
        
        
    }
}