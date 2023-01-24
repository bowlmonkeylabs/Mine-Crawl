using System.Runtime.CompilerServices;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Utils;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.Tasks
{
    [TaskCategory("Physics")]
    [TaskDescription("Check if object is within line of sight")]
    public class LineOfSight : Conditional
    {
        [SerializeField] private float _checkDelay = .05f;
        
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Mask of layers considering a hit (probably just player layer)")] 
        [SerializeField]private LayerMask _playerLayerMask;
        
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Mask of layers that are considered obstacles")] 
        [SerializeField]  private LayerMask _obstacleLayerMask;
        
        [SerializeField] private SharedTransform _origin;
        [SerializeField] private TransformSceneReference _target;
        
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Optionally store result. True if hit player, false if hit obstacle")] 
        [SerializeField] private SharedBool _storeValue;

        private float lastCheckTime = Mathf.NegativeInfinity;
        
        public override TaskStatus OnUpdate()
        {
            if (lastCheckTime + _checkDelay > Time.time) return TaskStatus.Failure;
            
            if (_target.Value.SafeIsUnityNull()) return TaskStatus.Failure;
            
            RaycastHit hit;
            Vector3 dir = (_target.Value.position - _origin.Value.position).normalized;
            LayerMask hitMask = _playerLayerMask | _obstacleLayerMask;
            if (Physics.Raycast(_origin.Value.position, dir, out hit, Mathf.Infinity, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject.IsInLayerMask(_obstacleLayerMask))
                {
                    if (_storeValue != null) _storeValue.Value = false;
                    return TaskStatus.Failure;
                }
                    
                if (_storeValue != null) _storeValue.Value = true;
                return TaskStatus.Success;
            }

            if (_storeValue != null) _storeValue.Value = false;
            return TaskStatus.Failure;
        }
    }
}