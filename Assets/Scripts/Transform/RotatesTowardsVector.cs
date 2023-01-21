using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace BML.Scripts
{
    public class RotatesTowardsVector : MonoBehaviour
    {
        [SerializeField] private Vector3Reference _rotateTowardsVector;
        [SerializeField] private bool _horizontalOnly;

        private void Update()
        {
            transform.rotation = Quaternion.LookRotation(_horizontalOnly ? _rotateTowardsVector.Value.xoz() : _rotateTowardsVector.Value);
        }
    }
}