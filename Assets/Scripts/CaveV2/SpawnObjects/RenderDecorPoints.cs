using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class RenderDecorPoints : MonoBehaviour
    {
        public List<Vector3> _debugPoints;

        private float _pointRadius;

        public void SetDebugPoints(List<Vector3> points, float radius)
        {
            _debugPoints = points;
            _pointRadius = radius;
        }

        public void ClearDebugPoints()
        {
            _debugPoints.Clear();
        }
        
        void OnDrawGizmosSelected()
        {
            foreach (Vector3 debugPoint in _debugPoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(debugPoint, _pointRadius);
            }
        }
    }
}