using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class RenderDecorPoints : MonoBehaviour
    {
        public List<DecorObjectSpawner.Point> _debugPoints = new List<DecorObjectSpawner.Point>();

        private float _pointRadius;

        public void AddPoint(DecorObjectSpawner.Point point, float radius)
        {
            _debugPoints.Add(point);
            _pointRadius = radius;
        }

        public void ClearPoints()
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