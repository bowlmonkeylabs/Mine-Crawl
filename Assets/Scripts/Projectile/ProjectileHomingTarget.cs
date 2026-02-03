using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    public class ProjectileHomingTarget : MonoBehaviour
    {
        [SerializeField, Required] private Transform _homingTarget;

        public Transform HomingTarget => _homingTarget;
    }
}