using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace BML.Scripts.Cave.MarchingCubesModified
{
    public abstract class Marching
    {
        public Grid Grid { get; set; }
        
        /// <summary>
        /// The surface value in the voxels. Normally set to 0. 
        /// </summary>
        public float Surface { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private float[] Cube { get; set; }

        /// <summary>
        /// Winding order of triangles use 2,1,0 or 0,1,2
        /// </summary>
        protected int[] WindingOrder { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        public Marching(Grid grid, float surface)
        {
            Grid = grid;
            Surface = surface;
            Cube = new float[8];
            WindingOrder = new int[] { 0, 1, 2 };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="voxels"></param>
        /// <param name="verts"></param>
        /// <param name="indices"></param>
        public virtual void Generate(float[,,] voxels, IList<Vector3> verts, IList<int> indices)
        {

            int width = voxels.GetLength(0);
            int height = voxels.GetLength(1);
            int depth = voxels.GetLength(2);

            UpdateWindingOrder();

            int x, y, z, i;
            int ix, iy, iz;
            for (x = 0; x <= width - 1; x++)
            {
                for (y = 0; y <= height - 1; y++)
                {
                    for (z = 0; z <= depth - 1; z++)
                    {
                        //Get the values in the 8 neighbours which make up a cube
                        for (i = 0; i < 8; i++)
                        {
                            ix = x + VertexOffset[i, 0];
                            ix = Math.Min(ix, width - 1);
                            iy = y + VertexOffset[i, 1];
                            iy = Math.Min(iy, height - 1);
                            iz = z + VertexOffset[i, 2];
                            iz = Math.Min(iz, depth - 1);

                            var value = voxels[ix, iy, iz];
                            Cube[i] = value;
                        }

                        //Perform algorithm
                        March(x, y, z, Cube, verts, indices);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="voxels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="verts"></param>
        /// <param name="indices"></param>
        public virtual void Generate(IList<float> voxels, int width, int height, int depth, IList<Vector3> verts, IList<int> indices)
        {
            Debug.Log($"Generate: {width} {height} {depth}");

            UpdateWindingOrder();

            int x, y, z, i;
            int ix, iy, iz;
            for (x = 0; x <= width - 1; x++)
            {
                for (y = 0; y <= height - 1; y++)
                {
                    for (z = 0; z <= depth - 1; z++)
                    {
                        //Get the values in the 8 neighbours which make up a cube
                        for (i = 0; i < 8; i++)
                        {
                            ix = x + VertexOffset[i, 0];
                            iy = y + VertexOffset[i, 1];
                            iz = z + VertexOffset[i, 2];

                            Cube[i] = voxels[ix + iy * width + iz * width * height];
                        }

                        //Perform algorithm
                        March(x, y, z, Cube, verts, indices);
                    }
                }
            }

        }

        /// <summary>
        /// Update the winding order. 
        /// This determines how the triangles in the mesh are orientated.
        /// </summary>
        protected virtual void UpdateWindingOrder()
        {
            if (Surface > 0.0f)
            {
                WindingOrder[0] = 2;
                WindingOrder[1] = 1;
                WindingOrder[2] = 0;
            }
            else
            {
                WindingOrder[0] = 0;
                WindingOrder[1] = 1;
                WindingOrder[2] = 2;
            }
        }

         /// <summary>
        /// MarchCube performs the Marching algorithm on a single cube
        /// </summary>
        protected abstract void March(int x, int y, int z, float[] cube, IList<Vector3> vertList, IList<int> indexList);

        /// <summary>
        /// GetOffset finds the approximate point of intersection of the surface
        /// between two points with the values v1 and v2
        /// </summary>
        protected virtual float GetOffset(float v1, float v2)
        {
            float delta = v2 - v1;
            return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
        }

        /// <summary>
        /// VertexOffset lists the positions, relative to vertex0, 
        /// of each of the 8 vertices of a cube.
        /// vertexOffset[8][3]
        /// </summary>
        protected static readonly int[,] VertexOffset = new int[,]
	    {
	        {0, 0, 0},{1, 0, 0},{1, 1, 0},{0, 1, 0},
	        {0, 0, 1},{1, 0, 1},{1, 1, 1},{0, 1, 1}
	    };

    }

}
