using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts.Tasks
{
    [TaskCategory("Unity/Transform")]
    [TaskDescription("Rotates the transform so the forward vector points at worldPosition. Returns Success.")]
    public class LookAtTransform : Action
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The Transform that the task operates on. If null the task Transform is used.")]
        public SharedTransform _targetTransform;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The Transform to look at. If null, will try to use target")]
        public TransformSceneReference _targetRef;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The Transform to look at.")]
        public SharedTransform _target;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("If true, will only rotate about the world up axis")]
        public bool _horizontalOnly = true;

        private Transform currentTarget;
        
        public override void OnStart()
        {
            if (_targetTransform.Value == null)
                _targetTransform.Value = transform;
                
            currentTarget = (_targetRef != null && _targetRef.Value != null) ? _targetRef.Value : _target.Value;
        }

        public override TaskStatus OnUpdate()
        {
            Vector3 delta = currentTarget.position - _targetTransform.Value.position;
            Vector3 dir;
            if (_horizontalOnly)
                dir = delta.xoz().normalized;
            else
                dir = delta.normalized;

            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            _targetTransform.Value.rotation = rot;

            return TaskStatus.Running;
        }
    }
}