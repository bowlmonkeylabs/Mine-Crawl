using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    [RequireComponent(typeof(BehaviorDesigner.Runtime.BehaviorTree))]
    public class Alertable : MonoBehaviour
    {
        [FormerlySerializedAs("behaviorTree")] [SerializeField] private BehaviorDesigner.Runtime.BehaviorTree _behaviorTree;
        [SerializeField] private UnityEvent _onAlerted;

        public void SetAlerted() {
            _behaviorTree.SendEvent("SetAlerted");
            _onAlerted.Invoke();
        }

    }
}
