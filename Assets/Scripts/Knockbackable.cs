using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BML.Scripts.Player;

namespace BML.Scripts
{
    [RequireComponent(typeof(BehaviorTree))]
    public class Knockbackable : MonoBehaviour
    {
        [SerializeField] private BehaviorTree behaviorTree;

        public void SetKnockback(Vector3 direction) {
            behaviorTree.SendEvent("SetKnockback", direction);
            Debug.Log("hellosdpiughf");
        }

        public void SetKnockback(PickaxeHitInfo pickaxeHitInfo) {
            SetKnockback(pickaxeHitInfo.HitDirection);
        }

    }
}
