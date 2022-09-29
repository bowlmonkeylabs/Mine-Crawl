using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class RenderDecorPoints : MonoBehaviour
    {
        public List<DecorObjectSpawner.Point> _debugPoints = new List<DecorObjectSpawner.Point>();

        private float _pointRadius;

        public void SetDebugPoints(List<DecorObjectSpawner.Point> points, float radius)
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
            if (_debugPoints.IsNullOrEmpty()) return;
            
            foreach (DecorObjectSpawner.Point debugPoint in _debugPoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(debugPoint.pos, _pointRadius);
            }
        }
    }
}