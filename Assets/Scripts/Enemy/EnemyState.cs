using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.Scripts.CaveV2.CaveGraph;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using BML.Scripts.CaveV2.Objects;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Enemy
{
    public class EnemyState : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private AggroState _aggro;
        [SerializeField] private DynamicGameEvent _onEnemyKilled;
        [SerializeField] private DynamicGameEvent _onEnemyAdded;
        [SerializeField] private DynamicGameEvent _onEnemyRemoved;

        [ShowInInspector, ReadOnly] private bool isAlerted;
        [ShowInInspector, ReadOnly] private bool isPlayerInLoS;

        #endregion

        public AggroState Aggro => _aggro;
        public bool IsAlerted
        {
            get => isAlerted; 
            set {
                isAlerted = value;
                OnAggroStateChanged?.Invoke();
            }
        }
        public bool IsPlayerInLoS { get => isPlayerInLoS; set => isPlayerInLoS = value; }
        
        private float lastUpdateTime = Mathf.NegativeInfinity;
        
        private Dictionary<Collider, CaveNodeData> _currentNodes;
        private Dictionary<Collider, CaveNodeConnectionData> _currentNodeConnections;

        #region Enums

        [Serializable]
        public enum AggroState
        {
            Idle,        //Not yet alerted
            Seeking,     //Alerted no LoS
            Engaged      //Alerted and LoS
        }

        #endregion

        #region Unity lifecyle

        private void Awake()
        {
            _currentNodes = new Dictionary<Collider, CaveNodeData>();
            _currentNodeConnections = new Dictionary<Collider, CaveNodeConnectionData>();
        }

        private void OnEnable()
        {
            var payload = new EnemyStateManager.EnemyStatePayload(this);
            _onEnemyAdded.Raise(payload);
        }
        
        private void OnDisable()
        {
            var payload = new EnemyStateManager.EnemyStatePayload(this);
            _onEnemyRemoved.Raise(payload);
        }

        private void Update()
        {
            UpdateAggroState();
        }

        private void OnTriggerEnter(Collider other)
        {
            bool isNodeBoundsLayer = other.gameObject.IsInLayerMask(CaveNodeDataUtils.RoomBoundsLayerMask);
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
            bool isNodeBoundsLayer = other.gameObject.IsInLayerMask(CaveNodeDataUtils.RoomBoundsLayerMask);
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

        #region State

        private void UpdateAggroState()
        {
            if (!isAlerted)
            {
                _aggro = AggroState.Idle;
                OnAggroStateChanged?.Invoke();
                return;
            }

            if (!isPlayerInLoS)
                _aggro = AggroState.Seeking;
            else
                _aggro = AggroState.Engaged;
            
            OnAggroStateChanged?.Invoke();
        }

        #endregion

        #region Events

        public delegate void OnAggroStateChangedCallback();
        public event OnAggroStateChangedCallback OnAggroStateChanged;

        #endregion
        
        #region Public interface

        public void OnDeath()
        {
            var payload = new EnemyKilledPayload
            {
                Position = this.transform.position
            };
            _onEnemyKilled.Raise(payload);
        }

        #endregion
    }
}