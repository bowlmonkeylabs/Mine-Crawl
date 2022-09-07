using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BML.Scripts.Utils;
using Pathfinding;
using UnityEngine;

namespace BML.Scripts.Tasks
{

    [TaskCategory("AStarPro")]
    [TaskDescription("Knocksback a rich ai using rigidbody")]
    public class Knockback : Action
    {
        [SerializeField] private SharedVector3 KnockBackDirection;
        [SerializeField] private SharedFloat KnockbackTime;
        [SerializeField] private SharedFloat KnockbackMaxSpeed;
        [SerializeField] private SharedFloat KnockbackMinSpeed;
        [SerializeField] private SharedFloat KnockbackVerticalSpeed;

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private AIBase ai;

        private float knockBackStartTime;
        private CharacterController charController;

        public override void OnStart()
        {
            charController = transform.GetComponentInChildren<CharacterController>();
            knockBackStartTime = Time.time;
            ai.enabled = false;
            charController.enabled = true;
        }

        public override void OnEnd()
        {
            ai.enabled = true;
            //_rigidbody.isKinematic = true;
            charController.enabled = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (knockBackStartTime + KnockbackTime.Value > Time.time)
            {
                HandleKnockBack();
                return TaskStatus.Running;
            }

            return TaskStatus.Success;
        }

        private void HandleKnockBack()
        {
            var percentComplete = (Time.time - knockBackStartTime) / KnockbackTime.Value;
            var horizontalSpeed = Mathf.SmoothStep(KnockbackMaxSpeed.Value, KnockbackMinSpeed.Value, percentComplete);
            var horizontalVelocity = horizontalSpeed * KnockBackDirection.Value.xoz();

            var verticalSpeed = Mathf.SmoothStep(KnockbackVerticalSpeed.Value, -KnockbackVerticalSpeed.Value,
                percentComplete);
            var verticalVelocity = verticalSpeed * Vector3.up;

            horizontalVelocity += verticalVelocity;

            charController.Move(horizontalVelocity * Time.deltaTime);
        }

    }
}
