using System;
using System.Collections;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    [RequireComponent(typeof(BehaviorDesigner.Runtime.BehaviorTree))]
    public class AlertableManager : MonoBehaviour
    {
        [SerializeField] private DynamicGameEvent _onAlertedEvent;
        [SerializeField] private BoolVariable _anyEnemiesAlerted;

        [ShowInInspector, ReadOnly, NonSerialized]
        public HashSet<GameObject> Alerted;

        #region Unity lifecycle

        private void Awake()
        {
            Alerted = new HashSet<GameObject>();
        }

        private void OnEnable()
        {
            _onAlertedEvent.Subscribe(OnAlertedDynamic);
        }
        
        private void OnDisable()
        {
            _onAlertedEvent.Unsubscribe(OnAlertedDynamic);
        }

        #endregion

        public void OnAlertedDynamic(object prev, object curr)
        {
            var payload = curr as Alertable.OnAlertPayload;
            OnAlerted(payload);
        }

        public void OnAlerted(Alertable.OnAlertPayload payload)
        {
            if (payload.Alerted)
            {
                Alerted.Add(payload.GameObject);
            }
            else
            {
                Alerted.Remove(payload.GameObject);
            }

            _anyEnemiesAlerted.Value = Alerted.Count > 0;
        }

    }
}
