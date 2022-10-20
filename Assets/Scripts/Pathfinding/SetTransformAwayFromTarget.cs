using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts.Pathfinding
{
    public class SetTransformAwayFromTarget : MonoBehaviour
    {
        [SerializeField] [Tooltip("Starting position to move from")] private Transform _origin;
        [SerializeField] [Tooltip("Transform to move away from")] private TransformSceneReference _target;
        [SerializeField] [Tooltip("Distance to move away from origin to target")] private float _distanceFromTarget;

        private void Update()
        {
            Vector3 dirToTarget = (_target.Value.position - transform.position).xoz().normalized;
            transform.position = _origin.position - dirToTarget * _distanceFromTarget;
        }
    }
}