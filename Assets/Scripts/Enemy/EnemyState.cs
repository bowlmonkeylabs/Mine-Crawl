using System;
using BML.Scripts.CaveV2.CaveGraph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Enemy
{
    public class EnemyState : MonoBehaviour
    {
        // [SerializeField] private Enemy Type
        [SerializeField] private BehaviorDesigner.Runtime.BehaviorTree _behaviorTree;
        [SerializeField] private Difficulty _difficulty;
        [SerializeField] private CaveNodeDataDebugComponent _currentNode;
        [SerializeField] private AggroState _aggro;
        [SerializeField] private int _nodeDistanceFromPlayer;
        [SerializeField] private float _updateFrequency = .25f;
        
        [ShowInInspector, ReadOnly] private bool isAlerted;
        [ShowInInspector, ReadOnly] private bool isPlayerInLoS;

        public bool IsAlerted { get => isAlerted; set => isAlerted = value; }
        public bool IsPlayerInLoS { get => isPlayerInLoS; set => isPlayerInLoS = value; }
        
        private float lastUpdateTime = Mathf.NegativeInfinity;

        #region Enums

        [Serializable]
        enum Difficulty
        {
            Easy,
            Medium,
            Difficulty
        }

        [Serializable]
        enum AggroState
        {
            Idle,        //Not yet alerted
            Seeking,     //Alerted no LoS
            Engaged      //Alerted and LoS
        }

        #endregion

        #region UnityLifecyle

        private void OnDrawGizmosSelected()
        {
            switch (_aggro)
            {
                case (AggroState.Idle):
                    Gizmos.color = Color.green;
                    break;
                case (AggroState.Seeking):
                    Gizmos.color = Color.yellow;
                    break;
                case (AggroState.Engaged):
                    Gizmos.color = Color.red;
                    break;
                default: 
                    Gizmos.color = Color.magenta;
                    break;
            }

            Gizmos.DrawSphere(transform.position + Vector3.up * 1.5f, .2f);
        }

        private void Update()
        {
            UpdateAggroState();
        }

        #endregion

        private void UpdateAggroState()
        {
            if (!isAlerted)
            {
                _aggro = AggroState.Idle;
                return;
            }


            if (!isPlayerInLoS)
                _aggro = AggroState.Seeking;
            else
                _aggro = AggroState.Engaged;

        }
    }
}