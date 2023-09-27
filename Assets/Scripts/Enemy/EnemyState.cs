using System;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
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

        [SerializeField] private BehaviorDesigner.Runtime.BehaviorTree _behaviorTree;
        [SerializeField] private AggroState _aggro;
        [SerializeField] private DynamicGameEvent _onEnemyKilled;
        [SerializeField] private DynamicGameEvent _onEnemyAdded;
        [SerializeField] private DynamicGameEvent _onEnemyRemoved;
        [SerializeField] private GameEvent _onAfterUpdatePlayerDistance;

        [ShowInInspector, ReadOnly] private bool isAlerted;
        [ShowInInspector, ReadOnly] private bool isPlayerInLoS;
        [ShowInInspector, ReadOnly] private int distanceToPlayer;
        [ShowInInspector, ReadOnly] private bool alertOnStart;

        public bool AlertOnStart
        {
            get => alertOnStart;
            set => alertOnStart = value;
        }

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

        public int DistanceToPlayer => distanceToPlayer;

        private float lastUpdateTime = Mathf.NegativeInfinity;
        
        private Dictionary<Collider, ICaveNodeData> _currentNodes;

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
            _currentNodes = new Dictionary<Collider, ICaveNodeData>();
        }
        
        private void OnEnable()
        {
            var payload = new EnemyStateManager.EnemyStatePayload(this);
            _onEnemyAdded.Raise(payload);
            _onAfterUpdatePlayerDistance.Subscribe(UpdateDistanceToPlayer);
        }
        
        private void OnDisable()
        {
            var payload = new EnemyStateManager.EnemyStatePayload(this);
            _onEnemyRemoved.Raise(payload);
            _onAfterUpdatePlayerDistance.Unsubscribe(UpdateDistanceToPlayer);
        }

        private void Update()
        {
            if (AlertOnStart && !isAlerted && _behaviorTree.ExecutionStatus == TaskStatus.Running)
                SetAlerted();
            
            UpdateAggroState();
        }

        private void OnTriggerEnter(Collider other)
        {
            TryRegisterCaveNode(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryRegisterCaveNode(other);
        }

        private void OnTriggerExit(Collider other)
        {
            bool isNodeBoundsLayer = other.gameObject.IsInLayerMask(CaveNodeDataUtils.RoomBoundsLayerMask);

            if (!isNodeBoundsLayer)
                return;
            
            if (_currentNodes.ContainsKey(other))
            {
                _currentNodes.Remove(other);
                UpdateDistanceToPlayer();
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

            if (_currentNodes != null)
            {
                foreach (var node in _currentNodes)
                {
                    Gizmos.DrawLine(gizmoPivot, node.Value.GameObject.transform.position);
                }
            }
        }

        #endregion

        #region State

        private void TryRegisterCaveNode(Collider other)
        {
            bool isNodeBoundsLayer = other.gameObject.IsInLayerMask(CaveNodeDataUtils.RoomBoundsLayerMask);

            if (!isNodeBoundsLayer)
                return;
            
            if (_currentNodes.ContainsKey(other))
                return;

            var caveNodeDataComponent = other.GetComponentInParent<CaveNodeDataDebugComponent>();
            if (caveNodeDataComponent != null)
            {
                var caveNodeData = caveNodeDataComponent.CaveNodeData;
                if (caveNodeData == null)
                {
                    return;
                }
                    
                _currentNodes.Add(other, caveNodeData);
                UpdateDistanceToPlayer();
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

                _currentNodes.Add(other, caveNodeConnectionData);
                UpdateDistanceToPlayer();
            }
        }

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

        private void UpdateDistanceToPlayer()
        {
            int dist = Int32.MaxValue;

            _currentNodes.Values
                .ToList()
                .ForEach(n => dist = Mathf.Min(dist, n.PlayerDistance));

            distanceToPlayer = dist;
        }

        [Button]
        private void SetAlerted()
        {
            _behaviorTree.SendEvent("SetAlerted");
        }

        #endregion

        #region Events

        public delegate void OnAggroStateChangedCallback();
        public event OnAggroStateChangedCallback OnAggroStateChanged;

        #endregion
        
        #region Public interface

        public void OnDeath(HitInfo hitInfo)
        {
            var payload = new EnemyKilledPayload
            {
                Position = this.transform.position,
                HitInfo = hitInfo,
            };
            _onEnemyKilled.Raise(payload);
        }
        
        public void OnDeath()
        {
            var payload = new EnemyKilledPayload
            {
                Position = this.transform.position,
                HitInfo = null,
            };
            _onEnemyKilled.Raise(payload);
        }

        #endregion
    }
}