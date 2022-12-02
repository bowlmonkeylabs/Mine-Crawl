using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Enemy
{
    public class EnemyState : MonoBehaviour
    {
        // [SerializeField] private Enemy Type
        [SerializeField] private BehaviorDesigner.Runtime.BehaviorTree _behaviorTree;
        [SerializeField] private LayerMask _nodeBoundsLayerMask;
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
        
        private Dictionary<Collider, CaveNodeData> _currentNodes;
        private Dictionary<Collider, CaveNodeConnectionData> _currentNodeConnections;

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

        private void Awake()
        {
            _currentNodes = new Dictionary<Collider, CaveNodeData>();
            _currentNodeConnections = new Dictionary<Collider, CaveNodeConnectionData>();
        }

        private void Update()
        {
            UpdateAggroState();
        }

        private void OnTriggerEnter(Collider other)
        {
            bool isNodeBoundsLayer = other.gameObject.IsInLayerMask(_nodeBoundsLayerMask);
            if (isNodeBoundsLayer)
            {
                bool isAlreadyEntered =
                    (_currentNodes.ContainsKey(other) || _currentNodeConnections.ContainsKey(other));
                
                if (isAlreadyEntered)
                {
                    return;
                }
                
                var caveNodeDataComponent = other.GetComponentInParent<CaveNodeDataDebugComponent>();
                if (caveNodeDataComponent != null)
                {
                    var caveNodeData = caveNodeDataComponent.CaveNodeData;
                    if (caveNodeData == null)
                    {
                        return;
                    }
                        
                    _currentNodes.Add(other, caveNodeData);
                }
                else
                {
                    var caveNodeConnectionDataComponent =
                        other.GetComponentInParent<CaveNodeConnectionDataDebugComponent>();
                    if (caveNodeConnectionDataComponent == null)
                    {
                        return;
                    }
                        
                    var caveNodeConnectionData = caveNodeConnectionDataComponent.CaveNodeConnectionData;
                    if (caveNodeConnectionData == null)
                    {
                        return;
                    }

                    _currentNodeConnections.Add(other, caveNodeConnectionData);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            bool isNodeBoundsLayer = other.gameObject.IsInLayerMask(_nodeBoundsLayerMask);
            if (isNodeBoundsLayer)
            {
                if (_currentNodes.ContainsKey(other))
                {
                    _currentNodes.Remove(other);
                }
                else if (_currentNodeConnections.ContainsKey(other))
                {
                    _currentNodeConnections.Remove(other);
                }
            }
        }

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

            Vector3 gizmoPivot = transform.position + Vector3.up * 1.5f;
            Gizmos.DrawSphere(gizmoPivot, .2f);
            
            // Draw line from sphere to room/tunnel centers it belongs to
            Gizmos.color = Color.white;

            foreach (var node in _currentNodes)
            {
                Gizmos.DrawLine(gizmoPivot, node.Value.GameObject.transform.position);
            }
            
            foreach (var edge in _currentNodeConnections)
            {
                Gizmos.DrawLine(gizmoPivot, edge.Value.GameObject.transform.position);
            }
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