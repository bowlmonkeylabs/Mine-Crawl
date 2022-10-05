using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using QFSW.QC;
using UnityEngine;

namespace BML.Scripts.QuantumConsoleExtensions
{
    public class QuantumConsoleStateReader : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private BoolVariable _isActive;
        [SerializeField] private QuantumConsole _quantumConsole;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _quantumConsole.OnActivate += UpdateIsActive;
            _quantumConsole.OnDeactivate += UpdateIsActive;
        }

        private void OnDisable()
        {
            _quantumConsole.OnActivate -= UpdateIsActive;
            _quantumConsole.OnDeactivate -= UpdateIsActive;
        }

        #endregion

        private void UpdateIsActive()
        {
            _isActive.Value = _quantumConsole.IsActive;
        }
    }
}