using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Utils;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.Tasks
{
    [TaskCategory("Unity/Transform")]
    [TaskDescription("Rotates the transform so the forward vector points at worldPosition. Returns Success.")]
    public class LookAtTransform : Action
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The Transform that the task operates on. If null the task Transform is used.")] [FormerlySerializedAs("_targetTransform")]
        public SharedTransform _targetRotateTransform;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The Transform to look at. If null, will try to use target")] [FormerlySerializedAs("_targetRef")]
        public TransformSceneReference _targetLookRef;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The Transform to look at.")]
        public SharedTransform _targetLook;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("If true, will only rotate about the local up axis")]
        public bool _horizontalOnly = true;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("If true, will keep running task each frame")]
        public bool _keepRunning = true;

        private Transform currentRotateTarget;
        private Transform currentLookAtTarget;
        private Vector3 originalUpDirection;
        private Vector3 dir;
        
        public override void OnStart()
        {
            currentRotateTarget = (_targetRotateTransform != null && _targetRotateTransform.Value != null) ?
                _targetRotateTransform.Value : transform;
                
            currentLookAtTarget = (_targetLookRef != null && _targetLookRef.Value != null) ?
                _targetLookRef.Value : _targetLook.Value;
            
            originalUpDirection = currentRotateTarget.up;
        }

        public override void OnEnd()
        {
            base.OnEnd();
        }

        public override TaskStatus OnUpdate()
        {
            if (currentLookAtTarget.SafeIsUnityNull() || currentRotateTarget.SafeIsUnityNull())
            {
                return TaskStatus.Failure;
            }
            
            Vector3 delta = currentLookAtTarget.position - currentRotateTarget.position;
            if (_horizontalOnly)
            {
                dir = delta.normalized;
                dir = Vector3.ProjectOnPlane(dir, originalUpDirection).normalized;
            }
            else
            {
                dir = delta.normalized;
            }

            Quaternion rot = Quaternion.LookRotation(dir, originalUpDirection);
            currentRotateTarget.rotation = rot;

            if (_keepRunning)
            {
                return TaskStatus.Running;
            }
            
            return TaskStatus.Success;
        }
    }
}