using System;
using System.Diagnostics;
using BML.ScriptableObjectCore.Scripts.Events;
using MoreMountains.Feedbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BML.Scripts.Enemy
{
    public class BossController : MonoBehaviour
    {
        [SerializeField] private BehaviorDesigner.Runtime.BehaviorTree _behaviorTree;
        [SerializeField] private Health _health;
        [SerializeField] private int _phaseCount = 3;
        [SerializeField] private int _activateSentinelsPhase = 2;
        [SerializeField] private MMF_Player _phaseChangeFeedback;
        [SerializeField] private GameEvent _activateSentinelsEvent;

        private int currentPhase = 1;
        private bool sentinelsActivated;

        private void Start()
        {
            SetPhase();
        }

        public void OnHealthChanged(int prev, int current)
        {
            SetPhase();
        }

        private void SetPhase()
        {
            int phase = Mathf.Max(1, Mathf.CeilToInt(_health.HealthFactor * _phaseCount));
            _behaviorTree.SetVariableValue("Phase", phase);
            Debug.Log(_health.HealthFactor);
            Debug.Log(phase);
            
            if (currentPhase != phase)
                _phaseChangeFeedback.PlayFeedbacks();

            if (!sentinelsActivated && phase > _activateSentinelsPhase)
            {
                _activateSentinelsEvent.Raise();
                sentinelsActivated = true;
            }
            
            currentPhase = phase;
        }
    }
}