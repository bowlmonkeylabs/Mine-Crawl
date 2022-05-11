using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;
namespace BML.VisualStateMachine.Scripts.Nodes.TransitionConditions
{
    [System.Serializable]
    [HideReferenceObjectPicker]
    public class FloatCondition : ITransitionCondition
    {
    
        [SerializeField] [HideLabel] [HideReferenceObjectPicker] 
        private FloatReference targetParameter;
    
        [HideLabel] public Comparator comparator;
        [HideLabel] public FloatReference value;
    
        private string parentTransitionName = "";

    
        public enum Comparator
        {
            LessThan,
            GreaterThan
        }
        
        private static Dictionary<Comparator, string> ComparatorToString = new Dictionary<Comparator, string>()
        {
            { Comparator.LessThan, " < "},
            { Comparator.GreaterThan, " > "}
        };

        public void Init(string transitionName)
        {
            parentTransitionName = transitionName;
        }

        public bool Evaluate(List<TriggerVariable> receivedTriggers)
        {
            float paramValue = targetParameter.Value;

            if (comparator == Comparator.GreaterThan)
                return paramValue > value.Value;
        
            if (comparator == Comparator.LessThan)
                return paramValue < value.Value;

            return false;
        }
    
        public override string ToString()
        {
            if (targetParameter != null)
                return $"{targetParameter.Name} {ComparatorToString[comparator]} {value.Name}";
            else
                return "<Missing Float>";
        }
    
    }
}
