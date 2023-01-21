using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.Scripts.Player
{
    public class SetAnimationSpeed : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _parameterName;
        [SerializeField] private EvaluateCurveVariable _scale;

        private void LateUpdate()
        {
            _animator.SetFloat(_parameterName, _scale.Value);
        }
    }
}