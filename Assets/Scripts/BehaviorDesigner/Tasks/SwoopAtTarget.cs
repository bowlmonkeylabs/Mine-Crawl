using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Utils;
using Pathfinding;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace BML.Scripts.Tasks
{
    [TaskCategory("AStarPro")]
    [TaskDescription("Swoop at target transform from above")]
    public class SwoopAtTarget : Action
    {
        [SerializeField] private SharedTransform _swoopTransform;
        [SerializeField] private TransformSceneReference _targetRef;
        [SerializeField] private Transform _rotationPivot;
        [SerializeField] private SharedFloat _swoopTime;
        [SerializeField] private SharedFloat _swoopYOffset;
        [SerializeField] private AnimationCurve _horizontalMoveCurve;
        [SerializeField] private AnimationCurve _verticalMoveCurve;
        [SerializeField] private AIBase ai;

        private float swoopStartTime;
        private Vector3 startPos;
        private Vector3 destination;
        private Vector3 horizontalDir, verticalDir;
        private float horizontalDelta, verticalDelta;
        private CharacterController charController;

        public override void OnStart()
        {
            charController = transform.GetComponentInChildren<CharacterController>();
            swoopStartTime = Time.time;
            ai.enabled = false;
            charController.enabled = true;
            startPos = _swoopTransform.Value.position;
            destination = _targetRef.Value.position + _swoopYOffset.Value * Vector3.up; //Store player pos at target
            horizontalDir = (destination.xoz() - startPos.xoz()).normalized;
            horizontalDelta = (destination.xoz() - startPos.xoz()).magnitude;
            verticalDir = Vector3.down;
            verticalDelta = Mathf.Abs(destination.y - startPos.y);
        }
        
        public override void OnEnd()
        {
            ai.enabled = true;
            charController.enabled = false;
            _rotationPivot.localRotation = Quaternion.identity;
        }
        
        public override TaskStatus OnUpdate()
        {
            if (swoopStartTime + _swoopTime.Value > Time.time)
            {
                HandleSwoop();
                return TaskStatus.Running;
            }

            return TaskStatus.Success;
        }

        public void HandleSwoop()
        {
            var percentComplete = (Time.time - swoopStartTime) / _swoopTime.Value;

            var horizontalTarget = startPos.xoz() + horizontalDir * horizontalDelta * _horizontalMoveCurve.Evaluate(percentComplete);
            var horizontalMove = (horizontalTarget - _swoopTransform.Value.position.xoz()).magnitude * horizontalDir;
            
            var verticalTarget = startPos.oyo() + verticalDir * verticalDelta * _verticalMoveCurve.Evaluate(percentComplete);
            var verticalMove = (verticalTarget - _swoopTransform.Value.position.oyo()).magnitude * verticalDir;

            var velocity = horizontalMove + verticalMove;
            charController.Move(velocity);
            if (velocity.magnitude > 0)
                _rotationPivot.rotation = Quaternion.LookRotation(charController.velocity.normalized, Vector3.up);
        }
    }
}