using System.Runtime.CompilerServices;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts.Tasks
{
    [TaskCategory("Physics")]
    [TaskDescription("Check if object is within line of sight")]
    public class LineOfSight : Conditional
    {
        [SerializeField] private float _checkDelay = .05f;
        
        [SerializeField] [BehaviorDesigner.Runtime.Tasks.Tooltip("Mask of everything raycast can interact with (including obstacles and target)")] 
        private LayerMask _hitLayerMask;
        
        [SerializeField] [BehaviorDesigner.Runtime.Tasks.Tooltip("Mask of layers that are considered obstacles")] 
        private LayerMask _obstacleLayerMask;
        
        [SerializeField] private Transform _origin;
        [SerializeField] private TransformSceneReference _target;

        private float lastCheckTime = Mathf.NegativeInfinity;
        
        public override TaskStatus OnUpdate()
        {
            if (lastCheckTime + _checkDelay > Time.time)
                return TaskStatus.Failure;

            RaycastHit hit;
            Vector3 dir = (_target.Value.position - _origin.position).normalized;
            if (Physics.Raycast(_origin.position, dir, out hit, Mathf.Infinity, _hitLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject.IsInLayerMask(_obstacleLayerMask))
                    return TaskStatus.Failure;

                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}