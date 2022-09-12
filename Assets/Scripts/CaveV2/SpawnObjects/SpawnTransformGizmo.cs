using System;
using UnityEngine;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class SpawnTransformGizmo : MonoBehaviour
    {
        [SerializeField] private float _gizmoRadius = 1f;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(this.transform.position, _gizmoRadius);
        }
    }
}