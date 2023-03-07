using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph.Minimap
{
    public class MinimapController : MonoBehaviour
    {
        #region Inspector


        [SerializeField] private bool _doCenterOnPlayer = true;
        [SerializeField] private SafeTransformValueReference _levelRoot;
        [SerializeField] private SafeTransformValueReference _playerPosition;

        [SerializeField] private SafeFloatValueReference _zoomLevel;
        private float _zoomBaseline = 0.006f;
        
        [SerializeField] private Transform _translationTarget;
        [SerializeField] private Transform _scalingTarget;

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            if (_scalingTarget != null)
            {
                _zoomBaseline = _scalingTarget.localScale.x;
            }
        }

        private void FixedUpdate()
        {
            if (_doCenterOnPlayer && _translationTarget != null && _levelRoot?.Value != null && _playerPosition?.Value != null)
            {
                Vector3 relativePosition = (_playerPosition.Value.position - _levelRoot.Value.position);
                _translationTarget.localPosition = -relativePosition;
            }

            if (_scalingTarget != null)
            {
                float scale = _zoomBaseline * _zoomLevel.Value;
                Vector3 localScale = _scalingTarget.localScale;
                localScale.Set(scale, scale, scale);
                _scalingTarget.localScale = localScale;
            }
        }

        #endregion
    }
}