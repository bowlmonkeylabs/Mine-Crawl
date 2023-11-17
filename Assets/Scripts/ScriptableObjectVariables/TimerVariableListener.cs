using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts.ScriptableObjectVariables
{
    public class TimerVariableListener : MonoBehaviour
    {
        [SerializeField] private TimerVariable _timer;
        [SerializeField] private UnityEvent _onStarted;
        [SerializeField] private UnityEvent _onFinished;

        private void OnEnable()
        {
            _timer.SubscribeStarted(OnStarted);
            _timer.SubscribeFinished(OnFinished);
        }
        
        private void OnDisable()
        {
            _timer.UnsubscribeStarted(OnStarted);
            _timer.UnsubscribeFinished(OnFinished);
        }
        
        private void OnStarted()
        {
            _onStarted.Invoke();
        }

        private void OnFinished()
        {
            _onFinished.Invoke();
        }
    }
}