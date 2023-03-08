using System;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
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

        [SerializeField] private bool _remapAngles = false;
        [SerializeField] private Vector3 _remapAnglesMin = Vector3.zero;
        [SerializeField] private Vector3 _remapAnglesMax = Vector3.one * 360;

        #endregion

        #region Unity lifecycle

        private void FixedUpdate()
        {
            Debug.Log($"Match rotation ({name})");
            if (_target.Value != null && _transform.Value != null)
            {
                Quaternion rotation = _readLocalRotation ? _target.Value.localRotation : _target.Value.rotation;
                if (_remapAngles)
                {
                    var eulerAngles = rotation.eulerAngles;
                    var prevEulerAngles = eulerAngles;
                    eulerAngles.Set(
                        Mathf.Lerp(_remapAnglesMin.x, _remapAnglesMax.x, eulerAngles.x / 360f),    
                        Mathf.Lerp(_remapAnglesMin.y, _remapAnglesMax.y, eulerAngles.y / 360f),    
                        Mathf.Lerp(_remapAnglesMin.z, _remapAnglesMax.z, eulerAngles.z / 360f)    
                    );
                    rotation.eulerAngles = eulerAngles;
                    Debug.Log($"(Prev angles {prevEulerAngles}) (Remapped angles {eulerAngles})");
                }
                
                if (_setLocalRotation)
                {
                    _transform.Value.localRotation = rotation;
                }
                else
                {
                    _transform.Value.rotation = rotation;
                }
            }
        }

        #endregion
    }
}