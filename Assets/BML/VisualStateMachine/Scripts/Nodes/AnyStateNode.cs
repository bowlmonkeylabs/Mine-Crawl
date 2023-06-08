using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace BML.VisualStateMachine.Scripts.Nodes
{
    public class AnyStateNode : Node
    {
        [Output(typeConstraint = TypeConstraint.Strict)]  [PropertyOrder(-3)]  public StateMachineConnection Transitions;
    }

}
