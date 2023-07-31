using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Utils;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    public class RigidBodyLauncher : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private Transform _firePoint;
        [SerializeField] private TransformSceneReference _container;
        
        [FormerlySerializedAs("_quantity2")] [SerializeField] private SafeIntValueReference _quantity;
        [SerializeField] private ForceMode _forceMode;
        
        [SerializeField] private Vector3Reference _direction;
        [SerializeField] private Vector3Reference _directionVariance;

        [SerializeField] private SafeFloatValueReference _force;
        [SerializeField] private SafeFloatValueReference _forceVariance;

        [SerializeField] private Vector3Reference _torque;
        [SerializeField] private Vector3Reference _torqueVariance;

        [SerializeField] private bool _instantiateAsPrefab = true;
        
        #endregion

        #region Unity lifecycle

        #endregion

        public void Launch(GameObject prefab)
        {
            for (int i = 0; i < _quantity.Value; i++)
            {
                LaunchObject(prefab);
            }
        }

        private void LaunchObject(GameObject prefab)
        {
            var position = (!_firePoint.SafeIsUnityNull() ? _firePoint.position : this.transform.position);
            var parent = (_container.Value != null ? _container.Value : this.transform);
            
            var newGameObject = GameObjectUtils.SafeInstantiate(_instantiateAsPrefab, prefab, parent);
            newGameObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            newGameObject.SetActive(true);
            
            var newRigidBody = newGameObject.GetComponent<Rigidbody>();
            if (newRigidBody != null)
            {
                // Calculate initial force and direction
                var directionVariance = new Vector3(
                    Random.Range(-1, 1) *  _directionVariance.Value.x,
                    Random.Range(-1, 1) *  _directionVariance.Value.y,
                    Random.Range(-1, 1) *  _directionVariance.Value.z
                );
                var forceVariance = Random.Range(-1, 1) *  _forceVariance.Value;
                var force = (_force.Value + forceVariance) * (_direction.Value.normalized + directionVariance);

                // Calculate initial torque
                var torqueVariance = new Vector3(
                    Random.Range(-1, 1) * _torqueVariance.Value.x,
                    Random.Range(-1, 1) * _torqueVariance.Value.y,
                    Random.Range(-1, 1) * _torqueVariance.Value.z
                );
                var torque = _torque.Value + torqueVariance;
                
                // Apply force and torque
                newRigidBody.AddForce(force, _forceMode);
                newRigidBody.AddTorque(torque, _forceMode);
            }
        }
    }
}