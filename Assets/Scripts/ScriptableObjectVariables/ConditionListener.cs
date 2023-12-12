using System;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts.ScriptableObjectVariables
{
    public class ConditionListener : MonoBehaviour
    {
        [SerializeField] private SafeBoolValueReference _condition;
        [SerializeField] private UnityEvent _onTrue;
        [SerializeField] private UnityEvent _onFalse;
        [SerializeField] private bool _subscribeToUpdates = true;

        private void OnEnable()
        {
            if (_subscribeToUpdates)
            {
                _condition.Subscribe(CheckCondition);
            }
        }

        private void OnDisable()
        {
            _condition.Unsubscribe(CheckCondition);
        }

        private void CheckCondition()
        {
            if (_condition.Value)
            {
                _onTrue?.Invoke();
            }
            else
            {
                _onFalse?.Invoke();
            }
        }
        
    }
}