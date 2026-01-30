using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.Player
{
    /// <summary>
    /// Sway flame rotation away from the player based on mouse input and current velocity.
    /// </summary>
    public class TorchFlameSway : MonoBehaviour
    {
        [Header("External References")]
        [SerializeField] private Vector2Reference _moveInput;
        [SerializeField] private Vector2Reference _mouseInput;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Vector3Reference _currentVelocity;
        [SerializeField] private BoolReference _isGrounded;

        [Header("Settings")]
        [SerializeField] private Vector2 _maxAngle = new Vector2(30f, 30f);
        [SerializeField, Range(0f, 1f)] private float _swayFromMouse = 1f;
        [SerializeField, Range(0f, 1f)] private float _swayFromVelocity = 1f;
        [SerializeField, Range(0f, 1f)] private float _swayFromCameraPitch = 1f;

        #region Unity lifecycle

        private void Update()
        {
            // Sway the flame rotation based on mouse input and current velocity.
            // Sway opposite to the direction of movement, as if the flame is being pushed by the air resistance.
            // For now, linearly interpolate

            var swayAmount = Vector3.zero;
            if (_swayFromMouse > 0f)
            {
                // Sway on X axis based on mouse X input, inverted
                // Sway on Z axis based on mouse Y input, not inverted
                swayAmount += new Vector3(
                    Mathf.Clamp(-1 * _mouseInput.Value.x * _maxAngle.x * _swayFromMouse, -_maxAngle.x, _maxAngle.x),
                    0f,
                    Mathf.Clamp(_mouseInput.Value.y * _maxAngle.y * _swayFromMouse, -_maxAngle.y, _maxAngle.y)
                );
            }

            if (_swayFromCameraPitch > 0f)
            {
                // As camera approaches straight up or down, lerp the flame rotation towards world up
                var cameraPitch = _mainCamera.transform.eulerAngles.x;
                if (cameraPitch > 180f) cameraPitch -= 360f; // Convert to -180 to 180 range
                var cameraPitchFactor = Mathf.InverseLerp(-90f, 90f, cameraPitch); // Remap camera pitch from -90 to 90 to 0 to 1
                swayAmount.z = Mathf.Lerp(-_maxAngle.y, _maxAngle.y, cameraPitchFactor) * _swayFromCameraPitch;
            }

            // Apply sway based on player velocity
            if (_swayFromVelocity > 0f)
            {
                // Convert player velocity to camera-local space, flattened on the XZ plane.
                var localVelocity = _mainCamera.transform.InverseTransformDirection(_currentVelocity.Value);
                localVelocity = localVelocity.xoz();
                // Sway on X axis based on local velocity X, inverted
                // Sway on Z axis based on local velocity Z, not inverted
                swayAmount += new Vector3(
                    Mathf.Clamp(-1 * localVelocity.x * _maxAngle.x * _swayFromVelocity, -_maxAngle.x, _maxAngle.x),
                    0f,
                    Mathf.Clamp(localVelocity.z * _maxAngle.y * _swayFromVelocity, -_maxAngle.y, _maxAngle.y)
                );

                // Debug.Log($"TorchFlameSway: (LocalVelocity: {localVelocity}) (SwayAmount: {swayAmount})");
            }

            // Clamp to max angle
            swayAmount = new Vector3(
                Mathf.Clamp(swayAmount.x, -_maxAngle.x, _maxAngle.x),
                0f,
                Mathf.Clamp(swayAmount.z, -_maxAngle.y, _maxAngle.y )
            );

            transform.localRotation = Quaternion.Euler(swayAmount);
        }

        #endregion
    }
}