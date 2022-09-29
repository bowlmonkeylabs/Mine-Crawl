using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.Utils
{
    public static class MeshUtils
    {
        public static void InvertNormals(this Mesh mesh)
        {
            // Reverse the triangles
            mesh.triangles = mesh.triangles.Reverse().ToArray();

            // also invert the normals
            mesh.normals = mesh.normals.Select(n => -n).ToArray();
            return;
            
            // TODO consider doing this in place on the mesh data, with no memory allocations for the temp arrays used below
            // Invert normals
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i];
            }
            // Assign inverted normals back to input mesh
            // mesh.normals = normals;
            Debug.Log($"{mesh.normals[0]} | {normals[0]}");
            mesh.SetNormals(normals);
            Debug.Log($"{mesh.normals[0]} | {normals[0]}");
    
            // int[] triangles = mesh.triangles;
            // for (int i = 0; i < triangles.Length; i++)
            // {
            //     int tri = triangles[i];
            //     triangles[i] = triangles[i + 2];
            //     triangles[i + 2] = tri;
            // }
            // // Assign triangles back to input mesh
            // mesh.triangles = triangles;
        }
        
        public static (float[] sizes, float[] cumulativeSizes, float totalArea) CalcAreas(List<int> filteredTriangles, Vector3[] vertices)
        {
            float[] sizes = GetTriSizes(filteredTriangles, vertices);
            float[] cumulativeSizes = new float[sizes.Length];
            float totalArea = 0;

            for (int i = 0; i < sizes.Length; i++)
            {
                totalArea += sizes[i];
                cumulativeSizes[i] = totalArea;
            }

            return (sizes, cumulativeSizes, totalArea);
        }

        public static (Vector3 point, Vector3 faceNormal) GetRandomPointOnMeshAreaWeighted(List<int> filteredTriangles, 
            Mesh mesh, float[] sizes, float[]cumulativeSizes, float totalArea)
        {
            float randomsample = Random.value * totalArea;
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

        public static float[] GetTriSizes(List<int> tris, Vector3[] verts)
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
        
        public static Vector3 GetTriangleFaceNormal(List<int> triangles, List<Vector3> normals, int triangleStartIndex)
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
        
    }
}