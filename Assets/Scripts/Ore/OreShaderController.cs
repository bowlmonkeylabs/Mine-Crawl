using System;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    [RequireComponent(typeof(Renderer))]
    public class OreShaderController : MonoBehaviour
    {
        #region Inspector

        [SerializeField, Required] private Health _health;
        [SerializeField, Required] private Transform _critMarkerTransform;
        [FormerlySerializedAs("_renderer")] [SerializeField, Required] private Renderer _oreRenderer;
        [SerializeField, Required] private Renderer _oreCrystalsRenderer;
        
        private static readonly int CritPosition = Shader.PropertyToID("_CritPosition");
        private static readonly int CrackPosition0 = Shader.PropertyToID("_CrackPosition0");
        private static readonly int CrackPosition1 = Shader.PropertyToID("_CrackPosition1");
        private static readonly int CrackPosition2 = Shader.PropertyToID("_CrackPosition2");
        
        private static readonly int DamageFac = Shader.PropertyToID("_DamageFac");

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            
        }

        // private void OnDrawGizmosSelected()
        // {
        //     var crackPosition0 = _oreRenderer.material.GetVector(CrackPosition0);
        //     if (crackPosition0 != Vector4.zero)
        //     {
        //         Gizmos.DrawWireSphere(crackPosition0, 1f);
        //     }
        //     var crackPosition1 = _oreRenderer.material.GetVector(CrackPosition1);
        //     if (crackPosition1 != Vector4.zero)
        //     {
        //         Gizmos.DrawWireSphere(crackPosition1, 1f);
        //     }
        //     var crackPosition2 = _oreRenderer.material.GetVector(CrackPosition2);
        //     if (crackPosition2 != Vector4.zero)
        //     {
        //         Gizmos.DrawWireSphere(crackPosition2, 1f);
        //     }
        // }

        #endregion

        public void UpdateShaderOnPickaxeInteract(HitInfo hitInfo)
        {
            if (_critMarkerTransform.gameObject.activeInHierarchy)
            {
                var localCritPosition = _oreRenderer.transform.InverseTransformPoint(_critMarkerTransform.position);
                _oreRenderer.material.SetVector(CritPosition, localCritPosition);
            }

            float oreDamageFac = 1 - ((float)_health.Value / (float)_health.StartingHealth);
            const int numCracks = 3;
            int oreDamageStep = Mathf.CeilToInt(oreDamageFac * numCracks);
            switch (oreDamageStep)
            {
                default:
                case 0:
                    break;
                case 1:
                    var crackPosition0 = _oreRenderer.material.GetVector(CrackPosition0);
                    if (crackPosition0 == Vector4.zero)
                    {
                        var localHitPosition = GetLocalHitPosition(hitInfo);
                        _oreRenderer.material.SetVector(CrackPosition0, localHitPosition);
                    }
                    break;
                case 2:
                    var crackPosition1 = _oreRenderer.material.GetVector(CrackPosition1);
                    if (crackPosition1 == Vector4.zero)
                    {
                        var localHitPosition = GetLocalHitPosition(hitInfo);
                        _oreRenderer.material.SetVector(CrackPosition1, localHitPosition);
                    }
                    break;
                case 3:
                    var crackPosition2 = _oreRenderer.material.GetVector(CrackPosition2);
                    if (crackPosition2 == Vector4.zero)
                    {
                        var localHitPosition = GetLocalHitPosition(hitInfo);
                        _oreRenderer.material.SetVector(CrackPosition2, localHitPosition);
                    }
                    break;
            }
            
            _oreCrystalsRenderer.material.SetFloat(DamageFac, oreDamageFac);
        }
        
        private Vector3 GetLocalHitPosition(HitInfo hitInfo)
        {
            var hitPosition = hitInfo.HitPositon;
            if (!hitPosition.HasValue)
            {
                var coll = _oreRenderer.GetComponent<Collider>();
                var colliderMaxExtent = coll.bounds.extents.Max();
                
                var hitDirection = hitInfo.HitDirection;
                if (!hitInfo.HitDirection.HasValue)
                {
                    hitDirection = Vector3.up * colliderMaxExtent;
                }
                
                hitPosition = coll.ClosestPointOnBounds(coll.transform.position - hitDirection.Value);
            }
            
            return _oreRenderer.transform.InverseTransformPoint(hitPosition.Value);
        }

    }
}