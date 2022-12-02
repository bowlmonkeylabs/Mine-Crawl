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

        [Serializable]
        enum Difficulty
        {
            Easy,
            Medium,
            Difficult
        }

        [Serializable]
        enum AggroState
        {
            Idle,        //Not yet alerted
            Seeking,     //Alerted no LoS
            Engaged      //Alerted and LoS
        }
    }
}