using UnityEngine;

namespace BML.Scripts.BehaviorTree
{
    public class SetSharedBool : MonoBehaviour
    {
        [SerializeField] private BehaviorDesigner.Runtime.BehaviorTree _behaviorTree;
        [SerializeField] private string _sharedBoolName;

        public void SetValue(bool value)
        {
            _behaviorTree.SetVariableValue(_sharedBoolName, value);
        }
    }
}