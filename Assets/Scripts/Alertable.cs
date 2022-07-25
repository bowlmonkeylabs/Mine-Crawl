using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

namespace BML.Scripts
{
    [RequireComponent(typeof(Behavior))]
    public class Alertable : MonoBehaviour
    {
        [SerializeField] private Behavior behaviorTree;

        public void SetAlerted() {
            behaviorTree.SendEvent("SetAlerted");
        }
    }
}
