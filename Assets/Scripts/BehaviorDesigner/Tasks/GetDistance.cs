using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts.Tasks
{
    [TaskCategory("Unity/Vector3")]
    [TaskDescription("Returns the distance from Transform1 to Transform2")]
    public class GetDistance : Action
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The first Transform")]
        public SharedTransform _transform1;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The first Transform (Reference). This will override transform1")]
        public TransformSceneReference _transformReference1;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The second Transform")]
        public SharedTransform _transform2;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The second Transform (Reference). This will override transform2")]
        public TransformSceneReference _transformReference2;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Ignore the Y axis (take just the horizontal of direction)")]
        public bool ignoreUpAxis = false;

        [BehaviorDesigner.Runtime.Tasks.Tooltip("The distance")]
        [RequiredField]
        public SharedFloat storeResult;

        public override TaskStatus OnUpdate()
        {
            Transform transform1 = (_transformReference1 != null && _transformReference1.Value != null)
                ? _transformReference1.Value
                : _transform1.Value;
            Transform transform2 = (_transformReference2 != null && _transformReference2.Value != null)
                ? _transformReference2.Value
                : _transform2.Value;

            Vector3 delta = (transform2.position - transform1.position);
            
            if (ignoreUpAxis)
                delta.xoz();

            storeResult.Value = delta.magnitude;

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            _transform1 = null;
            _transformReference1 = null;
            _transform2 = null;
            _transformReference2 = null;
            ignoreUpAxis = false;
        }
    }
}