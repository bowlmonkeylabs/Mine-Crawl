using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Player;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class PlayerDeflectable : MonoBehaviour
    {
        [SerializeField] private BoolVariable _upgradeDeflectProjectiles;
        [SerializeField] private List<Collider> _projectileColliders;
        [SerializeField] private UnityEvent<HitInfo> _onDeflectSuccess;
        [SerializeField] private UnityEvent<HitInfo> _onDeflectFailure;

        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private List<Collider> ignoredColliders = new List<Collider>();

        public void TryDeflect(HitInfo hitInfo)
        {
            if (_upgradeDeflectProjectiles.Value)
            {
                ToggleIgnoredCollisions(false);
                _onDeflectSuccess.Invoke(hitInfo);
            }
            else
                _onDeflectFailure.Invoke(hitInfo);
        }
        
        // To be called by what spawns the projectile
        public void InitIgnoredColliders(List<Collider> collidersToIgnore)
        {
            ignoredColliders = collidersToIgnore;
            ToggleIgnoredCollisions(true);
        }

        // Used to re-enable collisions with the initially ignored colliders (The colliders of what fired the projectile)
        private void ToggleIgnoredCollisions(bool ignore)
        {
            if (_projectileColliders.IsNullOrEmpty())
                Debug.LogWarning("No colliders assigned for this projectile! It will be unable to" +
                                 " ignore collisions with the entity that fired it!");
            
            foreach (var projectileCollider in _projectileColliders)
            {
                foreach (var ignoredCollider in ignoredColliders)
                {
                    Physics.IgnoreCollision(projectileCollider, ignoredCollider, ignore);
                }
            }
        }
    }
}