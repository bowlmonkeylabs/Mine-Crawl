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
        [SerializeField, Required] private Transform _critMarkerTransform;
        [SerializeField, Required] private Renderer _renderer;
        
        private static readonly int CritPosition = Shader.PropertyToID("_CritPosition");
        private static readonly int CrackPosition0 = Shader.PropertyToID("_CrackPosition0");
        private static readonly int CrackPosition1 = Shader.PropertyToID("_CrackPosition1");
        private static readonly int CrackPosition2 = Shader.PropertyToID("_CrackPosition2");

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            
        }

        private void OnEnable()
        {
            // UpdateShader();
            // _health.OnHealthChange += UpdateShaderOnHealthChange;
        }
        
        private void OnDisable()
        {
            // _health.OnHealthChange -= UpdateShaderOnHealthChange;
        }

        #endregion

        private void UpdateShaderOnHealthChange(int prevHealth, int currHealth)
        {
            float healthFactor = (float) currHealth / _health.StartingHealth;
            // _renderer.material.SetFloat("_VertexCrackStrengthFactor", 1 - healthFactor);
            // _renderer.material.SetFloat("_CrackStrengthFactor", 1 - healthFactor);
        }

        public void UpdateShaderOnPickaxeInteract(HitInfo hitInfo)
        {
            var localCritPosition = _critMarkerTransform.position - _renderer.transform.position;
            _renderer.material.SetVector(CritPosition, localCritPosition);

            float oreDamageFac = 1 - ((float)_health.Value / (float)_health.StartingHealth);
            const int numCracks = 3;
            int oreDamageStep = Mathf.CeilToInt(oreDamageFac * numCracks);
            switch (oreDamageStep)
            {
                default:
                case 0:
                    break;
                case 1:
                    var crackPosition0 = _renderer.material.GetVector(CrackPosition0);
                    if (crackPosition0 == Vector4.zero)
                    {
                        var localHitPosition = hitInfo.HitPositon - _renderer.transform.position;
                        _renderer.material.SetVector(CrackPosition0, localHitPosition);
                    }
                    break;
                case 2:
                    var crackPosition1 = _renderer.material.GetVector(CrackPosition1);
                    if (crackPosition1 == Vector4.zero)
                    {
                        var localHitPosition = hitInfo.HitPositon - _renderer.transform.position;
                        _renderer.material.SetVector(CrackPosition1, localHitPosition);
                    }
                    break;
                case 3:
                    var crackPosition2 = _renderer.material.GetVector(CrackPosition2);
                    if (crackPosition2 == Vector4.zero)
                    {
                        var localHitPosition = hitInfo.HitPositon - _renderer.transform.position;
                        _renderer.material.SetVector(CrackPosition2, localHitPosition);
                    }
                    break;
            }
        }

        private void UpdateShader()
        { 
            float healthFactor = (float) _health.Value / _health.StartingHealth;
            // _renderer.material.SetFloat("_VertexCrackStrengthFactor", 1 - healthFactor);
            // _renderer.material.SetFloat("_CrackStrengthFactor", 1 - healthFactor);
        }

    }
}