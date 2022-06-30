using System;
using System.Linq;
using UnityEngine;

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
        
    }
}