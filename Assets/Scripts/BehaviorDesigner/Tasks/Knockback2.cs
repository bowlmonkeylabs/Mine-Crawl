using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BML.Scripts.Utils;
using Pathfinding;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace BML.Scripts.Tasks
{
    [TaskCategory("AStarPro")]
    [TaskDescription("Knockback a rich ai using character controller")]
    public class Knockback2 : Action
    {
        [SerializeField] private SharedVector3 KnockBackDirection;
        [SerializeField] private SharedFloat KnockbackTime;
        
        [SerializeField] private SharedFloat KnockbackStartSpeed;
        [SerializeField] private SharedFloat KnockbackEndSpeed;
        [SerializeField] private bool UseCurveForKnockbackSpeedLerp;
        [SerializeField] private AnimationCurve KnockbackSpeedCurve;
        
        [SerializeField] private SharedFloat KnockbackStartVerticalSpeed;
        [SerializeField] private SharedFloat KnockbackEndVerticalSpeed;
        [SerializeField] private bool UseCurveForKnockbackVerticalSpeedLerp;
        [SerializeField] private AnimationCurve KnockbackVerticalSpeedCurve;

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

        private float Remap(float a, float b, float x)
        {
            float min = Mathf.Min(a, b);
            float range = Mathf.Abs(b - a);
            return (x * range) + min;
        }

        private void HandleKnockBack()
        {
            var percentComplete = (Time.time - knockBackStartTime) / KnockbackTime.Value;
            
            float horizontalSpeed;
            if (UseCurveForKnockbackSpeedLerp)
            {
                var eval = KnockbackSpeedCurve.Evaluate(percentComplete);
                horizontalSpeed = Remap(KnockbackStartSpeed.Value, KnockbackEndSpeed.Value, eval);
            }
            else
            {
                horizontalSpeed = Mathf.SmoothStep(KnockbackStartSpeed.Value, KnockbackEndSpeed.Value, percentComplete);
            }
            var horizontalVelocity = horizontalSpeed * KnockBackDirection.Value.xoz();

            float verticalSpeed;
            if (UseCurveForKnockbackVerticalSpeedLerp)
            {
                var eval = KnockbackVerticalSpeedCurve.Evaluate(percentComplete);
                verticalSpeed = Remap(KnockbackStartVerticalSpeed.Value, KnockbackEndVerticalSpeed.Value, eval);
            }
            else
            {
                verticalSpeed = Mathf.SmoothStep(KnockbackStartVerticalSpeed.Value, KnockbackEndVerticalSpeed.Value, percentComplete);
            }
            var verticalVelocity = verticalSpeed * Vector3.up;

            horizontalVelocity += verticalVelocity;

            charController.Move(horizontalVelocity * Time.deltaTime);
        }

    }
}
