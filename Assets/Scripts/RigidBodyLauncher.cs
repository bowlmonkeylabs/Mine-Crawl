using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Utils;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts
{
    public class RigidBodyLauncher : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private Transform _firePoint;
        [SerializeField] private TransformSceneReference _container;
        
        [SerializeField] private ForceMode _forceMode;
        
        [SerializeField] private Vector3 _direction;
        [SerializeField] private Vector3 _directionVariance;

        [SerializeField] private SafeFloatValueReference _force;
        [SerializeField] private SafeFloatValueReference _forceVariance;

        [SerializeField] private Vector3 _torque;
        [SerializeField] private Vector3 _torqueVariance;
        
        #endregion

        #region Unity lifecycle

        #endregion

        public void Launch(GameObject prefab)
        {
            var position = (!_firePoint.SafeIsUnityNull() ? _firePoint.position : this.transform.position);
            var parent = (_container.Value != null ? _container.Value : this.transform);
            
            var newGameObject = GameObjectUtils.SafeInstantiate(true, prefab, parent);
            newGameObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            
            var newRigidBody = newGameObject.GetComponent<Rigidbody>();
            if (newRigidBody != null)
            {
                // Calculate initial force and direction
                var directionVariance = new Vector3(
                    Random.Range(-_directionVariance.x, _directionVariance.x),
                    Random.Range(-_directionVariance.y, _directionVariance.y),
                    Random.Range(-_directionVariance.z, _directionVariance.z)
                );
                var forceVariance = Random.Range(-_forceVariance.Value, _forceVariance.Value);
                var force = (_force.Value + forceVariance) * (_direction + directionVariance);

                // Calculate initial torque
                var torqueVariance = new Vector3(
                    Random.Range(-_torqueVariance.x, _torqueVariance.x),
                    Random.Range(-_torqueVariance.y, _torqueVariance.y),
                    Random.Range(-_torqueVariance.z, _torqueVariance.z)
                );
                var torque = _torque + torqueVariance;
                
                // Apply force and torque
                newRigidBody.AddForce(force, _forceMode);
                newRigidBody.AddTorque(torque, _forceMode);
            }
        }
    }
}