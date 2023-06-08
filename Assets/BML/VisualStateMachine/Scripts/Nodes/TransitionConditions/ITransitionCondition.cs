using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
namespace BML.VisualStateMachine.Scripts.Nodes.TransitionConditions
{
    public interface ITransitionCondition
    {
        void Init(string transitionName);
        bool Evaluate(List<TriggerVariable> receivedTriggers);
    }

}
