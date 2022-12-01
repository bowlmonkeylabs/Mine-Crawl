using System;
using BML.Scripts.CaveV2.CaveGraph;
using UnityEngine;

namespace BML.Scripts.Enemy
{
    public class EnemyState : MonoBehaviour
    {
        // [SerializeField] private Enemy Type
        [SerializeField] private Difficulty _difficulty;
        [SerializeField] private CaveNodeDataDebugComponent _currentNode;
        [SerializeField] private AggroState _aggro;
        [SerializeField] private int _nodeDistanceFromPlayer;

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