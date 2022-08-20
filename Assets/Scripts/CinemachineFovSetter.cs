using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Cinemachine;
using UnityEngine;

namespace BML.Scripts
{
    public class CinemachineFovSetter : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private CinemachineVirtualCamera _camera;
        [SerializeField] private FloatReference _fovValue;
        [SerializeField] private bool _useHorizontalFov = true;

        #endregion
        
        #region Unity lifecycle

        private void OnEnable()
        {
            SetCinemachineCameraFov();
            _fovValue.Subscribe(SetCinemachineCameraFov);
        }
        
        private void OnDisable()
        {
            _fovValue.Unsubscribe(SetCinemachineCameraFov);
        }

        #endregion

        private void SetCinemachineCameraFov()
        {
            if (_useHorizontalFov)
            {
                var verticalFov = Camera.HorizontalToVerticalFieldOfView(
                    _fovValue.Value,
                    _camera.m_Lens.Aspect
                );
                _camera.m_Lens.FieldOfView = verticalFov;
            }
            else
            {
                _camera.m_Lens.FieldOfView = _fovValue.Value;
            }
        }
    }
}