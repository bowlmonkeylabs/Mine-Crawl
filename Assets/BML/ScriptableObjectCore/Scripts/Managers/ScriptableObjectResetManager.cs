using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Managers
{
    public class ScriptableObjectResetManager : MonoBehaviour
    {
        [SerializeField] private VariableContainer ResetContainer;
        [SerializeField] private bool _resetOnStart;
        [SerializeField] private GameEvent _onReset;

        #region Unity lifecycle
        
        public void Start()
        {
            if (_resetOnStart)
            {
                ResetValues();
            }
        }

        private void OnEnable()
        {
            if (!_onReset.SafeIsUnityNull())
            {
                _onReset.Subscribe(ResetValues);
            }
        }

        private void OnDisable()
        {
            if (!_onReset.SafeIsUnityNull())
            {
                _onReset.Unsubscribe(ResetValues);
            }
        }

        #endregion

        public void ResetValues()
        {
            foreach (var variable in ResetContainer.GetFloatVariables())
            {
                variable.Reset();
            }
            foreach (var variable in ResetContainer.GetIntVariables())
            {
                variable.Reset();
            }
            foreach (var variable in ResetContainer.GetQuaternionVariables())
            {
                variable.Reset();
            }
            foreach (var variable in ResetContainer.GetVector2Variables())
            {
                variable.Reset();
            }
            foreach (var variable in ResetContainer.GetVector3Variables())
            {
                variable.Reset();
            }
            foreach (var variable in ResetContainer.GetBoolVariables())
            {
                variable.Reset();
            }
            foreach (var variable in ResetContainer.GetTimerVariables())
            {
                variable.ResetTimer();
            }
        }
    }
}


