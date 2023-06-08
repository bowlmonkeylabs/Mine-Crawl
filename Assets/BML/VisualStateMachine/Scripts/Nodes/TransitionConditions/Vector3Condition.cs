using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.ValueReferences;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
namespace BML.VisualStateMachine.Scripts.Nodes.TransitionConditions
{
    [System.Serializable]
[HideReferenceObjectPicker]
public class Vector3Condition : ITransitionCondition
{
    [HideLabel, Required, SerializeField, HideReferenceObjectPicker]
    private Vector3Reference targetParameter;

    [HideLabel] public Comparison xCompare;
    [HideLabel] public Comparison yCompare;
    [HideLabel] public Comparison zCompare;

    private string parentTransitionName = "";

    
    public enum Comparator
    {
        Ignore,
        LessThan,
        GreaterThan,
        AbsGreaterThan,
        AbsLessThan,
    }
    
    private static Dictionary<Comparator, string> ComparatorToString = new Dictionary<Comparator, string>()
    {
        { Comparator.LessThan, " < "},
        { Comparator.GreaterThan, " > "},
        { Comparator.AbsGreaterThan, " (abs)> "},
        { Comparator.AbsLessThan, " (abs)< "}
    };
    
    [System.Serializable]
    public struct Comparison
    {
        public Comparator comparator;
        public FloatReference value;
    }

    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
    }

    public bool Evaluate(List<TriggerVariable> receivedTriggers)
    {
        bool xIs = Compare(xCompare, targetParameter.Value.x);
        bool yIs = Compare(yCompare, targetParameter.Value.y);
        bool zIs = Compare(zCompare, targetParameter.Value.z);
        return xIs && yIs && zIs;
    }

    public override string ToString()
    {
        if (targetParameter == null) return "";
        string compareStringX = xCompare.comparator != Comparator.Ignore ? ComparatorToString[xCompare.comparator] : "";
        string compareStringY = yCompare.comparator != Comparator.Ignore ? ComparatorToString[yCompare.comparator] : "";
        string compareStringZ = zCompare.comparator != Comparator.Ignore ? ComparatorToString[zCompare.comparator] : "";
        return $"{targetParameter.Name}.X {compareStringX} {xCompare.value.Name}.X &&" +
               $" {targetParameter.Name}.Y {compareStringY} {yCompare.value.Name}.Y &&" +
               $" {targetParameter.Name}.Z {compareStringZ} {zCompare.value.Name}.Z";
    }

    private bool Compare(Comparison comparison, float paramValue)
    {
        switch (comparison.comparator)
        {
            case Comparator.GreaterThan:
                return paramValue > comparison.value.Value;
            case Comparator.LessThan:
                return paramValue < comparison.value.Value;
            case Comparator.AbsGreaterThan:
                return Mathf.Abs(paramValue) > comparison.value.Value;
            case Comparator.AbsLessThan:
                return Mathf.Abs(paramValue) < comparison.value.Value;
            case Comparator.Ignore:
                return true;
        }

        return false;
    }
}
}
