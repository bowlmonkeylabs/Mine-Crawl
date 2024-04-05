using Sirenix.OdinInspector;
using UnityEngine;

namespace Projectile
{
    public class ProjectileHomingTarget : MonoBehaviour
    {
        [SerializeField, Required] private Transform _homingTarget;

        public Transform HomingTarget => _homingTarget;
    }
}