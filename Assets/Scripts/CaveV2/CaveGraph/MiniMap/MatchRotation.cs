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

        #endregion

        #region Unity lifecycle

        private void FixedUpdate()
        {
            if (_target.Value != null && _transform.Value != null)
            {
                Quaternion rotation = _readLocalRotation ? _target.Value.localRotation : _target.Value.rotation;
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