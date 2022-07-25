using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

namespace BML.Scripts
{
    [RequireComponent(typeof(BehaviorTree))]
    public class Alertable : MonoBehaviour
    {
        [SerializeField] private BehaviorTree behaviorTree;

        public void SetAlerted() {
            behaviorTree.SendEvent("SetAlerted");
        }

    }
}
