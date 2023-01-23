using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.Player
{
    public class SwayAndBob : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField]private Vector2Reference _moveInput;
        [SerializeField]private Vector2Reference _mouseInput;
        [SerializeField]private Vector3Reference _currentVelocity;
        [SerializeField]private BoolReference _isGrounded;

        [Header("Settings")]
        [SerializeField] private bool _sway = true;
        [SerializeField] private bool _swayRotation = true;
        [SerializeField] private bool _bobOffset = true;
        [SerializeField] private bool _bobSway = true;
        [SerializeField] private bool _fallOffset = true;
        [SerializeField] private bool _fallRotation = true;
        
        [Header("Sway")]
        [SerializeField] private float _step = 0.01f;
        [SerializeField] private float _maxStepDistance = 0.06f;
        private Vector3 swayPos;
        
        [Header("SwayRotation")]
        [SerializeField] private float _rotationStep = 4f;
        [SerializeField] private float _maxRotationStep = 5f;
        private Vector3 swayEulerRot;

        [Header("Bobbing")] 
        [SerializeField] private float _speedCurve;
        [SerializeField] private float _speedMultiplier = 1f;
        [SerializeField] private float _idleMultiplier;
        [FormerlySerializedAs("_bobPhaseOffsetX")]
        [Tooltip("Probably this set to some value to offset left-hand items from right hand")]
        [SerializeField] [Range(0f, 360f)] private float _bobPhaseOffset;
        
        [Tooltip("Maximum limits of travel from move input")]
        [SerializeField] private Vector3 _travelLimit = Vector3.one * 0.025f;
        
        [Tooltip("Maximum limits of travel from bobbing over time")]
        [SerializeField] private Vector3 _bobLimit = Vector3.one * 0.01f;
        
        private Vector3 bobPosition;
        private float curveSin { get => Mathf.Sin((_speedCurve)); }
        private float curveCos { get => Mathf.Cos((_speedCurve)); }

        private Vector3 originalPosition;

        [Header("BobRotation")]
        [SerializeField] private Vector3 _bobMultiplier;

        private Vector3 bobEulerRot;
        
        [Header("FallOffset")]
        [SerializeField] private Vector3 _travelLimitFall = Vector3.one * 0.1f;
        private Vector3 fallPosition;

        
        [Header("FallRotation")]
        [SerializeField] private float _fallMultiplier;
        private Vector3 fallEulerRot;

        private void OnEnable()
        {
            originalPosition = transform.localPosition;
        }

        private void Update()
        {
            Sway();
            SwayRotation();

            _speedCurve += Time.deltaTime * _speedMultiplier * (_isGrounded.Value ? _currentVelocity.Value.magnitude : 1f) + 1f * Time.deltaTime;
            
            BobOffset();
            BobRotation();
            FallOffset();
            FallRotation();

            CompositePositionRotation();
        }

        private void Sway()
        {
            if (!_sway) { swayPos = Vector3.zero; return; }

            Vector3 invertLook = _mouseInput.Value * -_step;
            invertLook.x = Mathf.Clamp(invertLook.x, -_maxStepDistance, _maxStepDistance);
            invertLook.y = Mathf.Clamp(invertLook.y, -_maxStepDistance, _maxStepDistance);
            invertLook.y = -invertLook.y;
            
            swayPos = invertLook;
        }
        
        private void SwayRotation()
        {
            if (!_swayRotation) { swayEulerRot = Vector3.zero; return; }

            Vector3 invertLook = _mouseInput.Value * -_rotationStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -_maxRotationStep, _maxRotationStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -_maxRotationStep, _maxRotationStep);
            invertLook.y = -invertLook.y;
            
            //TODO: Try making the Z (roll axis) negative so weapon top lags behind
            swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
        }

        private void BobOffset()
        {
            if (!_bobOffset) { bobPosition = Vector3.zero; return; }

            bobPosition.x = 
                (curveCos * _bobLimit.x * (_isGrounded.Value ? 1 : 0)) * _idleMultiplier
                - (_moveInput.Value.x * _travelLimit.x);
            
            bobPosition.y = 
                (curveSin * _bobLimit.y * (_isGrounded.Value ? 1 : 0)) * _idleMultiplier
                - (_moveInput.Value.y * _travelLimit.y);

            bobPosition.z = 
                -(_moveInput.Value.y * _travelLimit.z * _idleMultiplier);
        }

        [Header("Debug")]
        public bool rotX;
        public bool rotY;
        public bool rotZ;
        
        private void BobRotation()
        {
            if (!_bobSway) { bobEulerRot = Vector3.zero; return; }
            
            bobEulerRot.x = (_moveInput.Value != Vector2.zero ? _bobMultiplier.x * (Mathf.Sin(2 * _speedCurve + _bobPhaseOffset)) :
                                                            _idleMultiplier * (Mathf.Sin(2 * _speedCurve + _bobPhaseOffset) / 2f));
            bobEulerRot.y = (_moveInput.Value != Vector2.zero ? _bobMultiplier.y * Mathf.Cos(_speedCurve + _bobPhaseOffset) : 0);
            bobEulerRot.z = (_moveInput.Value != Vector2.zero ? _bobMultiplier.z * curveCos * _moveInput.Value.x : 0);

            if (!rotX) bobEulerRot.x = 0f;
            if (!rotY) bobEulerRot.y = 0f;
            if (!rotZ) bobEulerRot.z = 0f;
        }
        
        private void FallOffset()
        {
            if (!_fallOffset) { fallPosition = Vector3.zero; return; }
            
            fallPosition.y = - (_currentVelocity.Value.y * _travelLimitFall.y);
        }

        private void FallRotation()
        {
            if (!_fallRotation) { fallEulerRot = Vector3.zero; return; }

            fallEulerRot.x = (_currentVelocity.Value.y * _fallMultiplier);
        }

        private float smooth = 10f;
        private float smoothRot = 12;
        private void CompositePositionRotation()
        {
            // Position
            transform.localPosition =
                Vector3.Lerp(transform.localPosition,
                    originalPosition + swayPos + bobPosition + fallPosition,
                    Time.deltaTime * smooth);
            
            // Rotation
            transform.localRotation =
                Quaternion.Slerp(transform.localRotation,
                    Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRot) * Quaternion.Euler(fallEulerRot),
                    Time.deltaTime * smoothRot);
        }
    }
}