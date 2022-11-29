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

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _value.Subscribe(OnValueChanged);
        }
        
        private void OnDisable()
        {
            _value.Unsubscribe(OnValueChanged);
        }

        #endregion

        #region Graph

        private void OnValueChanged(float prev, float curr)
        {
            var point = new Vector2(Time.time, curr);
            Debug.Log($"UiGraphValueOverTime OnValueChanged {point}");
            _graph.AddPoint(point);
        }

        #endregion
    }
}