using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

[TaskCategory("AStarPro")]
[TaskDescription("Knocksback a rich ai using rigidbody")]
public class Knockback : Action
{
    [SerializeField] private SharedVector3 KnockBackDirection;
    [SerializeField] private SharedFloat KnockbackTime;
    [SerializeField] private SharedFloat KnockbackForce;
	
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private RichAI ai;

    private float knockBackStartTime;
    private float currentSpeed;
    private float prevSpeed;
    private CharacterController charController;

    public override void OnStart()
    {
        charController = transform.GetComponentInChildren<CharacterController>();
        knockBackStartTime = Time.time;
        ai.enabled = false;
        charController.enabled = true;
        currentSpeed = KnockbackForce.Value;
        prevSpeed = KnockbackForce.Value;
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
        currentSpeed = Mathf.Lerp(prevSpeed, 0f,  Time.deltaTime/KnockbackTime.Value);
        var velocity = currentSpeed * KnockBackDirection.Value;
		
        charController.Move(velocity * Time.deltaTime);
        prevSpeed = currentSpeed;
    }

}
