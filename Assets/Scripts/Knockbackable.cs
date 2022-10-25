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

        public void SetKnockback(Vector3 direction) {
            behaviorTree.SendEvent<object, object>("SetKnockback", direction, Time.time);
        }

        public void SetKnockback(HitInfo hitInfo) {
            SetKnockback(hitInfo.HitDirection);
        }

    }
}
