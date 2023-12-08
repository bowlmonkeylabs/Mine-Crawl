using System;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class EnemyBarrierCubeShaderController : MonoBehaviour
    {
        #region Inspector

        [SerializeField, Required] private SafeTransformValueReference _playerTransform;
        [SerializeField, Required] private Renderer _renderer;
        
        private static readonly int PlayerPosition = Shader.PropertyToID("_PlayerPosition");

        #endregion

        #region Unity lifecycle

        private void Update()
        {
            UpdateShaderWithPlayerPosition();
        }

        #endregion

        public void UpdateShaderWithPlayerPosition()
        {
            var localPlayerPosition = _renderer.transform.InverseTransformPoint(_playerTransform.Value.position);
            _renderer.material.SetVector(PlayerPosition, localPlayerPosition);
        }

    }
}