using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class CollisionChecker : MonoBehaviour
    {
        [SerializeField] private FloatReference CollisionRadius;
        [SerializeField] private LayerMask CollisionMask;

        [SerializeField] private bool ignoreTriggers = true;

        [SerializeField] private UnityEvent<List<Collider>> HandleCollisions;

        private Vector3 previousPosition;

        private void OnEnable()
        {
            previousPosition = transform.position;
        }

        private void Update()
        {
            CheckCollisions();
            previousPosition = transform.position;
        }

        private void CheckCollisions()
        {
            var triggerInteraction =
                ignoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.UseGlobal;
            List<Collider> colliderList =
                Physics.OverlapSphere(transform.position, CollisionRadius.Value, CollisionMask, triggerInteraction).ToList();

            RaycastHit[] hits = Physics.SphereCastAll(previousPosition, CollisionRadius.Value,
                (transform.position - previousPosition).normalized,
                Vector3.Distance(previousPosition, transform.position), CollisionMask,
                triggerInteraction);

            foreach (var hit in hits)
            {
                colliderList.Add(hit.collider);
            }

            HandleCollisions.Invoke(colliderList);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, CollisionRadius.Value);
        }
    }
}