using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.Player
{
    public class SwayAndBob : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField]private Vector2Variable _moveInput;
        [SerializeField]private Vector2Variable _mouseInput;
        [SerializeField]private Vector3Variable _currentVelocity;
        [SerializeField]private BoolVariable _isGrounded;

        [Header("Settings")]
        [SerializeField] private bool _sway = true;
        [SerializeField] private bool _swayRotation = true;
        [SerializeField] private bool _bobOffset = true;
        [SerializeField] private bool _bobSway = true;
        
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
        [SerializeField] private float _idleMultiplier;
        [Tooltip("Want this set to 180 for left-hand items")]
        [SerializeField] [Range(0f, 360f)] private float _bobPhaseOffset;
        
        [Tooltip("Maximum limits of travel from move input")]
        [SerializeField] private Vector3 _travelLimit = Vector3.one * 0.025f;
        
        [Tooltip("Maximum limits of travel from bobbing over time")]
        [SerializeField] private Vector3 _bobLimit = Vector3.one * 0.01f;
        
        private Vector3 bobPosition;
        private float curveSin { get => Mathf.Sin((_speedCurve + _bobPhaseOffset)); }
        private float curveCos { get => Mathf.Cos((_speedCurve + _bobPhaseOffset)); }


        [Header("BobRotation")]
        [SerializeField] [FormerlySerializedAs("multiplier")] private Vector3 _multiplier;

        private Vector3 bobEulerRot;

        private void Update()
        {
            GetInput();

            Sway();
            SwayRotation();

            _speedCurve += Time.deltaTime * (_isGrounded.Value ? _currentVelocity.Value.magnitude : 1f) + 0.01f;
            
            BobOffset();
            BobRotation();

            CompositePositionRotation();
        }

        private void GetInput()
        {
            
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
            invertLook.x = Mathf.Clamp(invertLook.x, -_maxStepDistance, _maxStepDistance);
            invertLook.y = Mathf.Clamp(invertLook.y, -_maxStepDistance, _maxStepDistance);
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
                (curveCos * _bobLimit.y * (_isGrounded.Value ? 1 : 0)) * _idleMultiplier
                - (_moveInput.Value.y * _travelLimit.y);

            bobPosition.z = 
                -(_moveInput.Value.y * _travelLimit.z * _idleMultiplier);
        }

        public bool rotX;
        public bool rotY;
        public bool rotZ;
        
        private void BobRotation()
        {
            if (!_bobSway) { bobEulerRot = Vector3.zero; return; }
            
            bobEulerRot.x = (_moveInput.Value != Vector2.zero ? _multiplier.x * (Mathf.Sin(2 * _speedCurve + _bobPhaseOffset)) :
                                                            _idleMultiplier * (Mathf.Sin(2 * _speedCurve + _bobPhaseOffset) / 2f));
            bobEulerRot.y = (_moveInput.Value != Vector2.zero ? _multiplier.y * curveCos : 0);
            bobEulerRot.z = (_moveInput.Value != Vector2.zero ? _multiplier.z * curveCos * _moveInput.Value.x : 0);

            if (rotX) bobEulerRot.x = 0f;
            if (rotY) bobEulerRot.y = 0f;
            if (rotZ) bobEulerRot.z = 0f;

        }

        private float smooth = 10f;
        private float smoothRot = 12;
        private void CompositePositionRotation()
        {
            // Position
            transform.localPosition =
                Vector3.Lerp(transform.localPosition,
                    swayPos + bobPosition,
                    Time.deltaTime * smooth);
            
            // Rotation
            transform.localRotation =
                Quaternion.Slerp(transform.localRotation,
                    Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRot),
                    Time.deltaTime * smoothRot);
        }
    }
}