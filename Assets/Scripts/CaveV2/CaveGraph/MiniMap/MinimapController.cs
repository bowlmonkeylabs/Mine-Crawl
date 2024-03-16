using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Utils;
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
        [SerializeField] private Transform _playerCursor;
        
        [SerializeField] private Vector2Variable _moveInput;
        
        [SerializeField] private GameObjectSceneReference _caveGenerator;

        // [SerializeField] private Transform _translationTarget;
        [SerializeField] private Transform _pivotTranslationTarget;
        [SerializeField] private Transform _scalingTarget;

        [SerializeField] private MatchRotation _cameraRotationCompensation;
        [SerializeField] private MatchRotation _playerMarkerRotationCompensation;

        [SerializeField] private UnityEvent _onOverlayOpen;
        [SerializeField] private UnityEvent _onOverlayClose;
        
        private enum UpdateMethod
        {
            Update,
            FixedUpdate,
        }
        [SerializeField] private UpdateMethod _updateMethod = UpdateMethod.FixedUpdate;

        #endregion

        public float _zoomBaseline = 0.006f;

        #region Unity lifecycle

        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
            
            if (_scalingTarget != null)
            {
                _zoomBaseline = _scalingTarget.localScale.x;
            }
        }

        private void OnEnable()
        {
            MinimapParameters.OpenMapOverlay.Subscribe(UpdateMinimapOverlay);
            _moveInput.Subscribe(OnMove);
        }

        private void OnDisable()
        {
            MinimapParameters.OpenMapOverlay.Unsubscribe(UpdateMinimapOverlay);
            _moveInput.Unsubscribe(OnMove);
        }

        private void FixedUpdate()
        {
            if (_updateMethod == UpdateMethod.FixedUpdate)
            {
                UpdateMinimapTransform();
            }
        }

        private void Update()
        {
            if (_updateMethod == UpdateMethod.Update)
            {
                UpdateMinimapTransform();
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

        private Vector3? _centerOnPoint;
        private Vector3? _moveOffset;

        private void OnMove()
        {
            if (MinimapParameters.OpenMapOverlay.Value && _moveInput.Value != Vector2.zero)
            {
                if (_moveOffset == null)
                {
                    _moveOffset = Vector3.zero;
                }
                
                var caveGenComponent = _caveGenerator.CachedComponent as CaveGenComponentV2;
                var bounds = caveGenComponent?.CaveGenBounds;

                float offsetFactor = 0f;
                if (bounds != null)
                {
                    offsetFactor = Mathf.Clamp01(_moveOffset.Value.magnitude / bounds.Value.extents.Max() * 2f);
                }

                var m = 100f;
                var multiplier = FloatUtils.RemapRange(offsetFactor, 0, 1, 0f, m);
                _moveOffset += (_camera.transform.rotation * _moveInput.Value.xoy()).xoz().normalized 
                               * (Time.unscaledDeltaTime * m);
                var centeringForce = (-_moveOffset.Value).normalized * (Time.unscaledDeltaTime * multiplier);
                _moveOffset += centeringForce;
                if (bounds != null)
                {
                    _moveOffset = Vector3.ClampMagnitude(_moveOffset.Value, bounds.Value.extents.magnitude * 2 / 5);
                }
            }
            else if (_moveOffset != Vector3.zero && _moveOffset != null)
            {
                _moveOffset = Vector3.Lerp(_moveOffset.Value, Vector3.zero, 0.1f);
            }
            else
            {
                _moveOffset = null;
            }
        }

        private void UpdateMinimapTransform()
        {
            if (_pivotTranslationTarget != null && _levelRoot?.Value != null && _playerPosition?.Value != null)
            {
                Vector3? pos = null;
                if (_doCenterOnPlayer && _playerPosition?.Value != null && !_centerOnPoint.HasValue)
                {
                    pos = _playerPosition.Value.position - _levelRoot.Value.position;
                }
                else if (_centerOnPoint.HasValue)
                {
                    if (_moveInput.Value != Vector2.zero || _moveOffset != null)
                    {
                        OnMove();
                    }
                    pos = _centerOnPoint.Value + (_moveOffset ?? Vector3.zero);
                }

                if (pos.HasValue)
                {
                    _pivotTranslationTarget.localPosition = -pos.Value;
                    
                    if (_playerCursor != null)
                    {
                        var playerRelativePosition = (_playerPosition.Value.position - pos.Value);
                        _playerCursor.localPosition = _scalingTarget.localRotation * playerRelativePosition;
                    }
                }
            }

            if (_scalingTarget != null)
            {
                float scale = _zoomBaseline * MinimapParameters.ZoomLevel.Value;
                Vector3 localScale = _scalingTarget.localScale;
                localScale.Set(scale, scale, scale);
                _scalingTarget.localScale = localScale;
            }
        }

        private void UpdateMinimapOverlay()
        {
            if (MinimapParameters.OpenMapOverlay != null)
            {
                _cameraRotationCompensation.enabled = !MinimapParameters.OpenMapOverlay.Value;
                _playerMarkerRotationCompensation.enabled = MinimapParameters.OpenMapOverlay.Value;

                if (MinimapParameters.OpenMapOverlay.Value)
                {
                    var caveGenComponent = _caveGenerator.CachedComponent as CaveGenComponentV2;
                    var discoveredNodes = caveGenComponent.CaveGraph.AllNodes
                        .Where(caveNode => caveNode.PlayerMapped)
                        .ToList();
                    var averagePosition = discoveredNodes
                        .Select(caveNode => caveNode.GameObject.transform.position)
                        .Aggregate((current, next) => current + next) / discoveredNodes.Count;
                    
                    _centerOnPoint = averagePosition;
                    _onOverlayOpen.Invoke();
                }
                else
                {
                    _centerOnPoint = null;
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