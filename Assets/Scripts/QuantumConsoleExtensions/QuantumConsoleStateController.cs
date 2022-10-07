using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using QFSW.QC;
using UnityEngine;

namespace BML.Scripts.QuantumConsoleExtensions
{
    public class QuantumConsoleStateController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private BoolVariable _isActive;
        [SerializeField] private QuantumConsole _quantumConsole;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _quantumConsole.OnActivate += ReadIsActive;
            _quantumConsole.OnDeactivate += ReadIsActive;
            
            _isActive.Subscribe(SetIsActive);
        }

        private void OnDisable()
        {
            _quantumConsole.OnActivate -= ReadIsActive;
            _quantumConsole.OnDeactivate -= ReadIsActive;
            
            _isActive.Unsubscribe(SetIsActive);
        }

        #endregion

        private void ReadIsActive()
        {
            _isActive.Unsubscribe(SetIsActive);
            _isActive.Value = _quantumConsole.IsActive;
            _isActive.Subscribe(SetIsActive);
        }
        
        private void SetIsActive(bool previousValue, bool currentValue)
        {
            if (currentValue) _quantumConsole.Activate();
            else _quantumConsole.Deactivate();
        }
    }
}