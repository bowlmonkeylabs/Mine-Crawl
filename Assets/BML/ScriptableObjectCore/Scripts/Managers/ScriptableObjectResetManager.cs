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
        [SerializeField, FoldoutGroup("Reset On Start")] private VariableContainer ResetOnStartVariableContainer;
        [SerializeField, FoldoutGroup("Reset On Start")] private List<IResettableScriptableObject> ResetOnStartResettableScriptableObjects;
        [SerializeField, FoldoutGroup("Reset On Destroy")] private VariableContainer ResetOnDestroyVariableContainer;
        [SerializeField, FoldoutGroup("Reset On Destroy")] private List<IResettableScriptableObject> ResetOnDestroyResettableScriptableObjects;
        [SerializeField] private GameEvent _resetVariables;
        [SerializeField] private GameEvent _onResetOnStart;
        [SerializeField] private GameEvent _onResetOnDestroy;

        #region Unity lifecycle
        
        public void Start()
        {
            ResetValues(ResetOnStartVariableContainer, ResetOnStartResettableScriptableObjects);
            ScriptableObjectResetOnEnterPlaymode.SkipResetOnEnterPlaymode(true);
        }

        private void OnEnable()
        {
            if (!_resetVariables.SafeIsUnityNull())
            {
                _resetVariables.Subscribe(ResetAllValues);
            }
        }

        private void OnDisable()
        {
            if (!_resetVariables.SafeIsUnityNull())
            {
                _resetVariables.Unsubscribe(ResetAllValues);
            }
        }

        public void OnDestroy()
        {
            ResetValues(ResetOnDestroyVariableContainer, ResetOnDestroyResettableScriptableObjects);
        }

        #endregion

        public void ResetAllValues() {
            ResetValues(ResetOnStartVariableContainer, ResetOnStartResettableScriptableObjects);
            ResetValues(ResetOnDestroyVariableContainer, ResetOnDestroyResettableScriptableObjects);
        }

        public void ResetValues(VariableContainer ResetContainer, List<IResettableScriptableObject> ResettableScriptableObjects)
        {
            if(ResetContainer != null) {
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

            if (ResettableScriptableObjects != null)
            {
                foreach (var resettable in ResettableScriptableObjects)
                {
                    resettable.ResetScriptableObject();
                }
            }

            if (!_onResetOnStart.SafeIsUnityNull())
            {
                _onResetOnStart.Raise();
            }
        }
    }
}


