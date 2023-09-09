using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Managers
{
    public class ScriptableObjectResetManager : SerializedMonoBehaviour
    {
        [SerializeField] private VariableContainer ResetContainer;
        [SerializeField] private List<IResettableScriptableObject> ResettableScriptableObjects;
        [SerializeField] private bool _resetOnStart;
        [SerializeField] private GameEvent _resetVariables;
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
            if (!_resetVariables.SafeIsUnityNull())
            {
                _resetVariables.Subscribe(ResetValues);
            }
        }

        private void OnDisable()
        {
            if (!_resetVariables.SafeIsUnityNull())
            {
                _resetVariables.Unsubscribe(ResetValues);
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
            if(ResettableScriptableObjects != null) {
                foreach (var resettable in ResettableScriptableObjects)
                {
                    resettable.ResetScriptableObject();
                }
            }

            if (!_onReset.SafeIsUnityNull())
            {
                _onReset.Raise();
            }
        }
    }
}


