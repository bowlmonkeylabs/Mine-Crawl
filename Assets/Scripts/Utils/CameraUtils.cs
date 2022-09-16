using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class CameraUtils 
    {
        /// <summary>
        /// Check if given bounds around specified pos are within view of the camera
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="boundSize"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static bool IsVisible(Vector3 pos, Vector3 boundSize, Camera camera) {
            var bounds = new Bounds(pos, boundSize);
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }
        
        public static bool IsSceneViewCameraInRange(Vector3 position, float distance)
        {
            Camera camera = Camera.current;
            Vector3 cameraPos = camera.WorldToScreenPoint(position);
            return ((cameraPos.x >= 0) &&
                    (cameraPos.x <= camera.pixelWidth) &&
                    (cameraPos.y >= 0) &&
                    (cameraPos.y <= camera.pixelHeight) &&
                    (cameraPos.z > 0) &&
                    (cameraPos.z < distance));
        }
    
    }
}

