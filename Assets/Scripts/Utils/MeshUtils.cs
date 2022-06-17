using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class MeshUtils
    {
        private static void InvertNormals(this Mesh mesh)
        {
            // TODO consider doing this in place on the mesh data, with no memory allocations for the temp arrays used below
            // Invert normals
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i];
            }
            // Assign inverted normals back to input mesh
            mesh.normals = normals;
    
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i++)
            {
                int tri = triangles[i];
                triangles[i] = triangles[i + 2];
                triangles[i + 2] = tri;
            }
            // Assign triangles back to input mesh
            mesh.triangles = triangles;
        }
        
    }
}