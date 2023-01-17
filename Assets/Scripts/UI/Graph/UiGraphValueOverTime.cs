using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.UI.Graph
{
    public class UiGraphValueOverTime : MonoBehaviour
    {
        #region Inspector

        [SerializeField, Required] private UiGraph _graph;
        [SerializeField, Required] private FloatReference _value;
        [SerializeField, Required] private bool _updateOverTime;
        [SerializeField, Required, ShowIf("_updateOverTime")] private float _updateInterval = 1f;
        [SerializeField, Required] private bool _enableLogs;

        #endregion

        private float lastUpdateTime = Mathf.NegativeInfinity;

        #region Unity lifecycle

        private void OnEnable()
        {
            _graph.AddPoint(new Vector2(0f, 0f));
            _graph.AddPoint(new Vector2(0.1f, 0.01f));
            _value.Subscribe(OnValueChanged);
        }
        
        private void OnDisable()
        {
            _value.Unsubscribe(OnValueChanged);
        }

        private void Update()
        {
            if (!_updateOverTime) return;
            
            if (lastUpdateTime + _updateInterval < Time.time)
            {
                var point = new Vector2(Time.time, _value.Value);
                _graph.AddPoint(point);
                if (_enableLogs) Debug.Log($"UiGraphValueOverTime Update Interval {point} | {gameObject.name}");
                lastUpdateTime = Time.time;
            }
        }

        #endregion

        #region Graph

        private void OnValueChanged(float prev, float curr)
        {
            var point = new Vector2(Time.time, curr);
            if (_enableLogs) Debug.Log($"UiGraphValueOverTime OnValueChanged {point} | {gameObject.name}");
            _graph.AddPoint(point);
        }

        #endregion
    }
}