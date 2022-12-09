using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Enemy;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts.Enemy
{
    public class EnemyStateManager : MonoBehaviour
    {
        #region Inspector
        
        [SerializeField] private DynamicGameEvent _onEnemyAdded;
        [SerializeField] private DynamicGameEvent _onEnemyRemoved;
        
        [FormerlySerializedAs("_anyEnemiesAlerted")]
        [TitleGroup("Alerted")]
        [SerializeField] private BoolVariable _anyEnemiesEngaged;

        [ShowInInspector, ReadOnly, NonSerialized]
        public HashSet<EnemyState> Enemies;
        
        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            Enemies = new HashSet<EnemyState>();
        }

        private void OnEnable()
        {
            _onEnemyAdded.Subscribe(OnEnemyAddedDynamic);
            _onEnemyRemoved.Subscribe(OnEnemyRemovedDynamic);
        }
        
        private void OnDisable()
        {
            _onEnemyAdded.Unsubscribe(OnEnemyAddedDynamic);
            _onEnemyRemoved.Unsubscribe(OnEnemyRemovedDynamic);
        }

        #endregion

        #region Added/Removed

        public class EnemyStatePayload
        {
            public EnemyState EnemyState;

            public EnemyStatePayload(EnemyState enemyState)
            {
                EnemyState = enemyState;
            }
        }

        private void OnEnemyAddedDynamic(object prev, object curr)
        {
            var payload = curr as EnemyStatePayload;
            OnEnemyAdded(payload);
        }
        private void OnEnemyAdded(EnemyStatePayload payload)
        {
            Enemies.Add(payload.EnemyState);
            payload.EnemyState.OnAggroStateChanged += OnEnemyAggroStateChanged;
        }
        
        private void OnEnemyRemovedDynamic(object prev, object curr)
        {
            var payload = curr as EnemyStatePayload;
            OnEnemyRemoved(payload);
        }
        private void OnEnemyRemoved(EnemyStatePayload payload)
        {
            Enemies.Remove(payload.EnemyState);
            payload.EnemyState.OnAggroStateChanged -= OnEnemyAggroStateChanged;
        }

        #endregion

        #region Alerted

        private void OnEnemyAggroStateChanged()
        {
            _anyEnemiesEngaged.Value = Enemies.Any(e => e.Aggro == EnemyState.AggroState.Engaged);
        }

        #endregion

    }
}
