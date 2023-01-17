using System;
using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts.UI.Graph
{
    public class UiGraphBoundsSynchronizer : MonoBehaviour
    {
        [SerializeField] private List<UiGraph> _graphs = new List<UiGraph>();

        private void OnEnable()
        {
            foreach (var graph in _graphs)
            {
                graph.onUpdateGraph += SynchronizerGraphBounds;
            }
        }
        
        private void OnDisable()
        {
            foreach (var graph in _graphs)
            {
                graph.onUpdateGraph -= SynchronizerGraphBounds;
            }
        }

        private void SynchronizerGraphBounds()
        {
            Vector2 newMin = new Vector2(Mathf.Infinity, Mathf.Infinity);
            Vector2 newMax = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);

            foreach (var graph in _graphs)
            {
                if (graph.GraphMin.x < newMin.x) newMin.x = graph.GraphMin.x;
                if (graph.GraphMin.y < newMin.y) newMin.y = graph.GraphMin.y;
                if (graph.GraphMax.x > newMax.x) newMax.x = graph.GraphMax.x;
                if (graph.GraphMax.y > newMax.y) newMax.y = graph.GraphMax.y;
            }

            foreach (var graph in _graphs)
            {
                graph.SetGraphMinMax(newMin, newMax);
            }
        }
    }
}