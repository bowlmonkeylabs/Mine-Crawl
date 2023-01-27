using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.Projectile
{
    public class IgnoredCollisionsHandler : MonoBehaviour
    {
        [Tooltip("This object's colliders")]
        [SerializeField] private List<Collider> _colliders;
        
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private List<Collider> ignoredColliders = new List<Collider>();
        
        public void InitIgnoredColliders(List<Collider> collidersToIgnore)
        {
            ignoredColliders = collidersToIgnore;
            ToggleIgnoredCollisions(true);
        }

        public void EnableCollisions()
        {
            ToggleIgnoredCollisions(false);
        }

        private void ToggleIgnoredCollisions(bool ignore)
        {
            if (_colliders.IsNullOrEmpty())
                Debug.LogWarning("No colliders assigned for this object! It will be unable to" +
                                 " ignore collisions with ignoredColliders!");
            
            foreach (var col in _colliders)
            {
                foreach (var ignoredCollider in ignoredColliders)
                {
                    UnityEngine.Physics.IgnoreCollision(col, ignoredCollider, ignore);
                }
            }
        }
    }
}