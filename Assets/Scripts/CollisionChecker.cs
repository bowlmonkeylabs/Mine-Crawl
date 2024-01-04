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
        [SerializeField] private bool UseColliderReference;
        [SerializeField, HideIf("UseColliderReference")] private FloatReference Radius;
        [Tooltip("Move movement in a single tick that will still check for collisions. To prevent collision check" +
                 "when being teleported to far location.")]
        [SerializeField, HideIf("UseColliderReference")] private float MaxSqrDistForCollision = 10f;
        [SerializeField, ShowIf("UseColliderReference")] private SphereCollider ColliderReference;
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
            // Don't check for collision if moved a large distance (Ex. teleported)
            if (Vector3.SqrMagnitude(transform.position - previousPosition) <= MaxSqrDistForCollision)
                CheckCollisions();
            
            previousPosition = transform.position;
        }

        private void CheckCollisions()
        {
            var radius = UseColliderReference ? ColliderReference.radius : Radius.Value;
            var triggerInteraction =
                ignoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.UseGlobal;
            List<Collider> colliderList =
                Physics.OverlapSphere(transform.position, radius, CollisionMask, triggerInteraction).ToList();

            RaycastHit[] hits = Physics.SphereCastAll(previousPosition, radius,
                (transform.position - previousPosition).normalized,
                Vector3.Distance(previousPosition, transform.position), CollisionMask,
                triggerInteraction);

            foreach (var hit in hits)
            {
                colliderList.Add(hit.collider);
                Debug.Log($"Hit {hit.collider.name}");
            }

            HandleCollisions.Invoke(colliderList);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, ColliderReference?.radius ?? Radius.Value);
        }
    }
}