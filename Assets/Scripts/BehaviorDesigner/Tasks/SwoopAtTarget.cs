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
        [SerializeField] private SharedFloat _swoopTime;
        [SerializeField] private AIBase ai;

        private float swoopStartTime;
        private Vector3 startPos;
        private Vector3 destination;
        private CharacterController charController;

        public override void OnStart()
        {
            charController = transform.GetComponentInChildren<CharacterController>();
            swoopStartTime = Time.time;
            ai.enabled = false;
            charController.enabled = true;
            startPos = _swoopTransform.Value.position;
            destination = _targetRef.Value.position; //Store player pos at target
        }
        
        public override void OnEnd()
        {
            ai.enabled = true;
            charController.enabled = false;
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
            
            //TODO: only calc next 2 once
            var horizontalDir = (destination.xoz() - startPos.xoz()).normalized;
            var horizontalDelta = (destination.xoz() - startPos.xoz()).magnitude;
            var horizontalTarget = startPos.xoz() + horizontalDir * horizontalDelta * percentComplete;
            var horizontalMove = (horizontalTarget - _swoopTransform.Value.position.xoz()).magnitude * horizontalDir;

            var veritcalDir = Vector3.down;
            var verticalDelta = Mathf.Abs(destination.y - startPos.y);
            var verticalTarget = startPos.oyo() + veritcalDir * verticalDelta * percentComplete;
            var verticalMove = (verticalTarget - _swoopTransform.Value.position.oyo()).magnitude * veritcalDir;

            charController.Move(horizontalMove + verticalMove);
        }
    }
}