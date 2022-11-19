using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using UnityEngine;

namespace BML.Scripts.Helper
{
    public class SetColliderRadius : MonoBehaviour
    {
        [SerializeField] private SphereCollider _collider;
        [SerializeField] private SafeFloatValueReference _radius;

        private void OnEnable()
        {
            SetRadius();
            _radius.Subscribe(SetRadius);
        }

        private void OnDisable()
        {
            _radius.Unsubscribe(SetRadius);
        }

        private void SetRadius()
        {
            _collider.radius = _radius.Value;
        }
    }
}