using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class OreShaderController : MonoBehaviour
    {
        #region Inspector

        [SerializeField, Required] private Health _health;
        [SerializeField, Required] private Renderer _renderer;

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            
        }

        private void OnEnable()
        {
            // UpdateShader();
            _health.OnHealthChange += UpdateShaderOnHealthChange;
        }
        
        private void OnDisable()
        {
            _health.OnHealthChange -= UpdateShaderOnHealthChange;
        }

        #endregion

        private void UpdateShaderOnHealthChange(int prevHealth, int currHealth)
        {
            float healthFactor = (float) currHealth / _health.StartingHealth;
            _renderer.material.SetFloat("_VertexCrackStrengthFactor", 1 - healthFactor);
            _renderer.material.SetFloat("_CrackStrengthFactor", 1 - healthFactor);
        }

        private void UpdateShader()
        { 
            float healthFactor = (float) _health.Value / _health.StartingHealth;
            _renderer.material.SetFloat("_VertexCrackStrengthFactor", 1 - healthFactor);
            _renderer.material.SetFloat("_CrackStrengthFactor", 1 - healthFactor);
        }

    }
}