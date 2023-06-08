using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
namespace BML.VisualStateMachine.Scripts.Nodes
{
    public class BreakpointNode : StateNode
    {
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [TextArea] [SerializeField] private string Message = "";
        
        public override void Enter()
        {
            base.Enter();
            Debug.LogError(Message);
        }
    }
}

