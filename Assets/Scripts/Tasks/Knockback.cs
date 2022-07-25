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

    public override void OnStart()
    {
        ai.enabled = false;
        knockBackStartTime = Time.time;
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(KnockBackDirection.Value.normalized * KnockbackForce.Value, ForceMode.Impulse);
    }
	
    public override void OnEnd()
    {
        ai.enabled = true;
        _rigidbody.isKinematic = true;
    }

    public override TaskStatus OnUpdate()
    {
        if (knockBackStartTime + KnockbackTime.Value > Time.time)
        {
            return TaskStatus.Running;
        }

        return TaskStatus.Success;
    }

}
