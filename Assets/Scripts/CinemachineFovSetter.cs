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
            _camera.m_Lens.FieldOfView = _fovValue.Value;
        }
    }
}