using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BML.Scripts.Player;

namespace BML.Scripts
{
    [RequireComponent(typeof(BehaviorDesigner.Runtime.BehaviorTree))]
    public class Knockbackable : MonoBehaviour
    {
        [SerializeField] private BehaviorDesigner.Runtime.BehaviorTree behaviorTree;

        public void SetKnockback(Vector3 direction, float knockbackTime = default) {
            if(knockbackTime != default) {
                behaviorTree.SetVariableValue("KnockbackTime", knockbackTime);
            }
            behaviorTree.SendEvent<object, object>("SetKnockback", direction, Time.time);
        }

        public void SetKnockback(HitInfo hitInfo) {
            if (hitInfo.HitDirection.HasValue) {
                SetKnockback(hitInfo.HitDirection.Value);
            }
        }

        public void SetKnockback(HitInfo hitInfo, float knockbackTime = default) {
            if (hitInfo.HitDirection.HasValue) {
                SetKnockback(hitInfo.HitDirection.Value, knockbackTime);
            }
        }

    }
}
