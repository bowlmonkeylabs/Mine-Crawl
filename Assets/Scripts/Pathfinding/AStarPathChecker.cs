using System;
using Pathfinding;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts.Pathfinding
{
    public class AStarPathChecker : MonoBehaviour
    {
        [SerializeField] private AIPath _ai;
        [SerializeField] private UnityEvent<bool> OnUpdateReachedEndOfPath;
        [SerializeField] private float _updateDelay = .1f;
        
        private float lastUpdateTime = Mathf.NegativeInfinity;
        private bool reachedEndOfPath;

        private void Update()
        {
            if (lastUpdateTime + _updateDelay > Time.time)
                return;

            reachedEndOfPath = !_ai.hasPath || (_ai.hasPath && _ai.reachedEndOfPath);
            OnUpdateReachedEndOfPath.Invoke(reachedEndOfPath);
        }
    }
}