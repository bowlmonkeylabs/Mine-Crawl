using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class Timer : MonoBehaviour
    {

        [SerializeField] private bool _activateOnStart;
        [SerializeField] private SafeFloatValueReference _duration;
        [SerializeField] private UnityEvent _onFinished;

        public float Duration
        {
            get => _duration.Value;
            set => _duration.SetConstantValue(value);
        }
        public float RemainingTime => remainingTime;
        public float ElapsedTime => _duration.Value - (remainingTime);
        public bool IsFinished => isFinished;

        public bool IsStopped => isStopped;
        
        private float startTime = Mathf.NegativeInfinity;
        private bool isStopped = true;
        private float remainingTime;
        private bool isFinished = false;

        private void Start()
        {
            if (_activateOnStart) StartTimer();
        }

        private void Update()
        {
            UpdateTime();
        }

        public void StartTimer()
        {
            isStopped = false;
            isFinished = false;
            startTime = Time.time;
        }

        public void ResetTimer()
        {
            isStopped = true;
            isFinished = false;
            startTime = Time.time;
            remainingTime = _duration.Value;
        }

        public void StopTimer()
        {
            isStopped = true;
        }

        public void UpdateTime()
        {
            if (!isStopped && !isFinished)
            {
                remainingTime = Mathf.Max(0f, _duration.Value - (Time.time - startTime));
                if (remainingTime == 0)
                {
                    isFinished = true;
                    _onFinished.Invoke();
                }
            }
            
        }
    }
}