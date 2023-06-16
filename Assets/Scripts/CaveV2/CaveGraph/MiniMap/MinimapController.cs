using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BML.Scripts.CaveV2.CaveGraph.Minimap
{
    public class MinimapController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] public CaveMinimapParameters MinimapParameters;

        [SerializeField] private bool _doCenterOnPlayer = true;
        [SerializeField] private SafeTransformValueReference _levelRoot;
        [SerializeField] private SafeTransformValueReference _playerPosition;

        // [SerializeField] private Transform _translationTarget;
        [SerializeField] private Transform _pivotTranslationTarget;
        [SerializeField] private Transform _scalingTarget;

        [SerializeField] private MatchRotation _cameraRotationCompensation;
        [SerializeField] private MatchRotation _playerMarkerRotationCompensation;

        [SerializeField] private UnityEvent _onOverlayOpen;
        [SerializeField] private UnityEvent _onOverlayClose;

        #endregion

        public float _zoomBaseline = 0.006f;

        #region Unity lifecycle

        private void Awake()
        {
            if (_scalingTarget != null)
            {
                _zoomBaseline = _scalingTarget.localScale.x;
            }
        }

        private void OnEnable()
        {
            MinimapParameters.OpenMapOverlay.Subscribe(UpdateMinimapOverlay);
        }

        private void OnDisable()
        {
            MinimapParameters.OpenMapOverlay.Unsubscribe(UpdateMinimapOverlay);
        }

        private void FixedUpdate()
        {
            if (_doCenterOnPlayer && _pivotTranslationTarget != null && _levelRoot?.Value != null && _playerPosition?.Value != null)
            {
                Vector3 relativePosition = (_playerPosition.Value.position - _levelRoot.Value.position);
                _pivotTranslationTarget.localPosition = -relativePosition;
            }

            if (_scalingTarget != null)
            {
                float scale = _zoomBaseline * MinimapParameters.ZoomLevel.Value;
                Vector3 localScale = _scalingTarget.localScale;
                localScale.Set(scale, scale, scale);
                _scalingTarget.localScale = localScale;
            }
        }

        #endregion

        #region Public interface

        public bool IsInBounds(float directPlayerDistance)
        {
            bool inBounds = (directPlayerDistance <= MinimapParameters.MapPlayerRadius);
            return inBounds;
        }

        #endregion

        private void UpdateMinimapOverlay()
        {
            if (MinimapParameters.OpenMapOverlay != null)
            {
                _cameraRotationCompensation.enabled = !MinimapParameters.OpenMapOverlay.Value;
                _playerMarkerRotationCompensation.enabled = MinimapParameters.OpenMapOverlay.Value;

                if (MinimapParameters.OpenMapOverlay.Value)
                {
                    _onOverlayOpen.Invoke();
                }
                else
                {
                    _onOverlayClose.Invoke();
                }
            }
            else
            {
                _cameraRotationCompensation.enabled = true;
                _playerMarkerRotationCompensation.enabled = false;
            }
        }
    }
}