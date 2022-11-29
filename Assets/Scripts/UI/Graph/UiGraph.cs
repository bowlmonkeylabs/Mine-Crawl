using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace BML.Scripts.UI.Graph
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(LineRenderer))]
    public class UiGraph : MonoBehaviour
    {
        #region Inspector

        private static List<Vector2> _PointsDebugValue = new List<Vector2>
        {
            Vector2.zero,
            Vector2.one,
        }; 

        [SerializeField, Required] private RectTransform _transform;
        [SerializeField, Required] private LineRenderer _lineRenderer;
        [ShowInInspector, ReadOnly] private List<Vector2> _points = new List<Vector2>();
        [ShowInInspector, ReadOnly] private Vector2 _graphMin;
        [ShowInInspector, ReadOnly] private Vector2 _graphMax;

        #endregion

        #region Unity lifecycle

        #endregion

        #region Graph

        public void AddPoint(Vector2 point)
        {
            _points.Add(point);
            
            if (point.x < _graphMin.x) _graphMin.x = point.x;
            if (point.y < _graphMin.y) _graphMin.y = point.y;
            if (point.x > _graphMax.x) _graphMax.x = point.x;
            if (point.y > _graphMax.y) _graphMax.y = point.y;
            
            UpdateGraph();
        }
        
        private static Rect RectTransformToScreenSpace(RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            return new Rect((Vector2)transform.position - (size * 0.5f), size);
        }

        private Vector3 PointToGraphPosition(Vector2 point)
        {
            var fourCornersArrayLocal = new Vector3[4];
            _transform.GetLocalCorners(fourCornersArrayLocal);
            var graphMinLocal = fourCornersArrayLocal[0];
            var graphMaxLocal = fourCornersArrayLocal[2];
            
            // var rectScreenSpace = RectTransformToScreenSpace(_transform);

            var pointFactor = (point - _graphMin) / (_graphMax - _graphMin);
            var pointLocal = Vector3.Scale(
                    pointFactor, 
                    graphMaxLocal - graphMinLocal
                ) + graphMinLocal;
            pointLocal.z = 2;

            // var pointWorld = Camera.main.ScreenToWorldPoint(pointLocal);
            
            return pointLocal;
        }

        [Button]
        private void DebugGraph()
        {
            _points = _PointsDebugValue;
            UpdateGraphMinMax();
            UpdateGraph();
        }

        [Button]
        private void UpdateGraph()
        {
            var positions = _points.Select(PointToGraphPosition).ToArray();
            _lineRenderer.positionCount = positions.Length;
            _lineRenderer.SetPositions(positions);
        }

        private void UpdateGraphMinMax()
        {
            Vector2 graphMin = new Vector2(0, 0);
            Vector2 graphMax = new Vector2(1, 1);
            foreach (var p in _points)
            {
                if (p.x < graphMin.x) graphMin.x = p.x;
                if (p.y < graphMin.y) graphMin.y = p.y;
                if (p.x > graphMax.x) graphMax.x = p.x;
                if (p.y > graphMax.y) graphMax.y = p.y;
            }
            _graphMin = graphMin;
            _graphMax = graphMax;
        }

        #endregion
    }
}