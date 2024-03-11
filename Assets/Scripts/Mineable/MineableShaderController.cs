using System;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.Mineable
{
    [RequireComponent(typeof(Renderer))]
    public class MineableShaderController : MonoBehaviour
    {
        #region Inspector

        [SerializeField, Required] private Health _health;
        [SerializeField] private Transform _critMarkerTransform;
        [FormerlySerializedAs("_mineableRenderer")] [SerializeField, Required, RequiredListLength(1, 0)] private List<Renderer> _mineableRenderers;
        [FormerlySerializedAs("_mineableAdditionalRenderer")] [SerializeField] private List<Renderer> _mineableAdditionalRenderers;
        
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

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || EditorSceneManager.IsPreviewScene(this.gameObject.scene))
            {
                // Debug.Log("Not in play mode or in prefab mode. Gizmos will not be drawn.");
                return;
            }

            // var radius = 0.1f;
            // var crackPosition0 = _mineableRenderers.First().material.GetVector(CrackPosition0);
            // if (crackPosition0 != Vector4.zero)
            // {
            //     Gizmos.DrawWireSphere((Vector3)crackPosition0 + transform.position, radius);
            // }
            // var crackPosition1 = _mineableRenderers.First().material.GetVector(CrackPosition1);
            // if (crackPosition1 != Vector4.zero)
            // {
            //     Gizmos.DrawWireSphere((Vector3)crackPosition1 + transform.position, radius);
            // }
            // var crackPosition2 = _mineableRenderers.First().material.GetVector(CrackPosition2);
            // if (crackPosition2 != Vector4.zero)
            // {
            //     Gizmos.DrawWireSphere((Vector3)crackPosition2 + transform.position, radius);
            // }
        }

        #endregion

        public void UpdateShaderOnPickaxeInteract(HitInfo hitInfo)
        {
            if (_critMarkerTransform != null && _critMarkerTransform.gameObject.activeInHierarchy)
            {
                foreach (var r in _mineableRenderers)
                {
                    var localCritPosition = r.transform.InverseTransformPoint(_critMarkerTransform.position);
                    r.material.SetVector(CritPosition, localCritPosition);
                }
            }
            
            var materials = _mineableRenderers.SelectMany(r => r.materials);

            float oreDamageFac = 1 - ((float)_health.Value / (float)_health.StartingHealth);
            const int numCracks = 3;
            int oreDamageStep = Mathf.CeilToInt(oreDamageFac * numCracks);
            Debug.Log("Update shader on pickaxe interact. Ore damage step: " + oreDamageStep + " Ore damage fac: " + oreDamageFac + " Health: " + _health.Value + " Starting health: " + _health.StartingHealth + " Ore damage step: " + oreDamageStep + " Num cracks: " + numCracks + " Ore damage fac: " + oreDamageFac);
            switch (oreDamageStep)
            {
                default:
                case 0:
                    break;
                case 1:
                    var crackPosition0 = materials.First().GetVector(CrackPosition0);
                    if (crackPosition0 == Vector4.zero)
                    {
                        var localHitPosition = GetLocalHitPosition(hitInfo);
                        foreach (var material in materials)
                        {
                            material.SetVector(CrackPosition0, localHitPosition);
                        }
                    }
                    break;
                case 2:
                    var crackPosition1 = materials.First().GetVector(CrackPosition1);
                    if (crackPosition1 == Vector4.zero)
                    {
                        var localHitPosition = GetLocalHitPosition(hitInfo);
                        foreach (var material in materials)
                        {
                            material.SetVector(CrackPosition1, localHitPosition);
                        }
                    }
                    break;
                case 3:
                    var crackPosition2 = materials.First().GetVector(CrackPosition2);
                    if (crackPosition2 == Vector4.zero)
                    {
                        var localHitPosition = GetLocalHitPosition(hitInfo);
                        foreach (var material in materials)
                        {
                            material.SetVector(CrackPosition2, localHitPosition);
                        }
                    }
                    break;
            }

            if (_mineableAdditionalRenderers != null)
            {
                var additionalMaterials = _mineableAdditionalRenderers.SelectMany(r => r.materials);
                foreach (var material in additionalMaterials)
                {
                    material.SetFloat(DamageFac, oreDamageFac);
                }
            }
        }
        
        private Vector3 GetLocalHitPosition(HitInfo hitInfo)
        {
            var hitPosition = hitInfo.HitPositon;
            if (!hitPosition.HasValue)
            {
                var colliders = _mineableRenderers
                    .Select(r => r.GetComponent<Collider>());

                hitPosition = colliders.Select(coll =>
                {
                    var hitDir = hitInfo.HitDirection;
                    if (!hitInfo.HitDirection.HasValue)
                    {
                        var colliderMaxExtent = coll.bounds.extents.Max();
                        hitDir = Vector3.up * colliderMaxExtent;
                    }
                    
                    var hitPos = coll.ClosestPointOnBounds(coll.transform.position - hitDir.Value);
                    return hitPos;
                })
                .Average();
            }
            
            return _mineableRenderers.First().transform.InverseTransformPoint(hitPosition.Value);
        }

    }
}