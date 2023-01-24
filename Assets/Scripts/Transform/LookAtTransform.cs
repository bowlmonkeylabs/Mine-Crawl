using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts
{
    public class LookAtTransform : MonoBehaviour
    {
        [SerializeField] [HideIf("_useReference")] private Transform _targetTransform;
        [SerializeField] [ShowIf("_useReference")] private TransformSceneReference _targetTransformReference;
        [SerializeField] private bool _useReference = false;
        [SerializeField] private bool _horizontalOnly = false;

        private void Update()
        {
            Transform target = _useReference ? _targetTransformReference.Value : _targetTransform;
            if (target.SafeIsUnityNull())
            {
                return;
            }
            Vector3 delta = target.position - transform.position;

            Quaternion rot;

            if (_horizontalOnly)
                delta = delta.xoz();
            
            rot = Quaternion.LookRotation(delta.normalized, Vector3.up);
            transform.rotation = rot;
        }
    }
}