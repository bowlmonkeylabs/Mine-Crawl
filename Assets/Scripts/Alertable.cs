using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BML.Scripts
{
    [RequireComponent(typeof(BehaviorDesigner.Runtime.BehaviorTree))]
    public class Alertable : MonoBehaviour
    {
        [SerializeField] private BehaviorDesigner.Runtime.BehaviorTree behaviorTree;

        public void SetAlerted() {
            behaviorTree.SendEvent("SetAlerted");
        }

    }
}
