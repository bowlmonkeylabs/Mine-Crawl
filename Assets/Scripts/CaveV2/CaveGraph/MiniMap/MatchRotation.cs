using System;
using System.Collections;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph.Minimap
{
    public class MatchRotation : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private SafeTransformValueReference _target;
        [SerializeField] private SafeTransformValueReference _transform;
        [SerializeField] private bool _readLocalRotation = false;
        [SerializeField] private bool _setLocalRotation = false;
        [SerializeField] private bool _clearLocalRotationOnDisable = false;

        [SerializeField] private float _maxDegreesDelta = 360f;
        [SerializeField] private bool _remapAngles = false;
        [SerializeField] private Vector3 _remapAnglesOldMin = Vector3.zero;
        [SerializeField] private Vector3 _remapAnglesOldMax = Vector3.one * 360;
        [SerializeField] private Vector3 _remapAnglesNewMin = Vector3.zero;
        [SerializeField] private Vector3 _remapAnglesNewMax = Vector3.one * 360;

        #endregion

        #region Unity lifecycle

        private void OnDisable()
        {
            if (_clearLocalRotationOnDisable)
            {
                if (_coroutineResetRotation != null)
                {
                    StopCoroutine(_coroutineResetRotation);
                }

                if (ApplicationUtils.IsPlaying_EditorSafe)
                {
                    _coroutineResetRotation = StartCoroutine(CoroutineResetRotation());
                }
                
            }
        }

        private void FixedUpdate()
        {
            if (_target.Value != null && _transform.Value != null)
            {
                Quaternion targetRotation = _readLocalRotation ? _target.Value.localRotation : _target.Value.rotation;
                if (_remapAngles)
                {
                    var eulerAngles = targetRotation.eulerAngles;
                    eulerAngles.Set(
                        FloatUtils.RemapRange(eulerAngles.x, _remapAnglesOldMin.x, _remapAnglesOldMax.x, _remapAnglesNewMin.x, _remapAnglesNewMax.x),    
                        FloatUtils.RemapRange(eulerAngles.y, _remapAnglesOldMin.y, _remapAnglesOldMax.y, _remapAnglesNewMin.y, _remapAnglesNewMax.y),    
                        FloatUtils.RemapRange(eulerAngles.z, _remapAnglesOldMin.z, _remapAnglesOldMax.z, _remapAnglesNewMin.z, _remapAnglesNewMax.z)    
                    );
                    targetRotation.eulerAngles = eulerAngles;
                }

                if (_setLocalRotation)
                {
                    _transform.Value.localRotation = Quaternion.RotateTowards(_transform.Value.localRotation, targetRotation, _maxDegreesDelta);
                }
                else
                {
                    _transform.Value.rotation = Quaternion.RotateTowards(_transform.Value.rotation, targetRotation, _maxDegreesDelta);
                }
            }
        }

        #endregion

        private Coroutine _coroutineResetRotation;
        private IEnumerator CoroutineResetRotation()
        {
            if (_target.Value != null && _transform.Value != null)
            {
                Quaternion targetRotation = Quaternion.identity;
                Quaternion newRotation = _transform.Value.localRotation;
                while (newRotation != targetRotation)
                {
                    newRotation = Quaternion.RotateTowards(
                        _transform.Value.localRotation, 
                        targetRotation, _maxDegreesDelta
                    );
                    _transform.Value.localRotation = newRotation;
                    yield return null;
                }
            }

            _coroutineResetRotation = null;

        }
        
    }
}