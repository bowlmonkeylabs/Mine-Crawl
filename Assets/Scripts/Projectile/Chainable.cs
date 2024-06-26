using System;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts;
using BML.Scripts.Utils;
using Micosmo.SensorToolkit;
using UnityEngine;
using UnityEngine.Events;
using DrawXXL;

namespace Projectile
{
    [RequireComponent(typeof(RangeSensor), typeof(LOSSensor))]
    public class Chainable : MonoBehaviour
    {
        [SerializeField] private IntReference _upgradeChainProjectilesCount;
        [SerializeField] private SafeBoolValueReference _upgradeChainProjectilesHoming;
        [SerializeField] private ProjectileDriver _projectileDriver;
        [SerializeField] private RangeSensor _rangeSensor;
        [SerializeField] private LOSSensor _losSensor;
        [SerializeField] private Collider _projectileCollider;
        [SerializeField] private string _redirectLayer;
        [SerializeField] private UnityEvent _onChainFailure;

        private int remainingChains;

        private void Start()
        {
            remainingChains = _upgradeChainProjectilesCount.Value;
        }

        public void TryChain(Collider hitCollider)
        {
            bool didChain = TryChainInternal(hitCollider);
            if (!didChain)
                _onChainFailure.Invoke();
        }

        private bool TryChainInternal(Collider hitCollider)
        {
            if (!_upgradeChainProjectilesHoming.Value || remainingChains < 1)
                return false;
            
            //Ignore collisions and range detection with enemy already hit
            var ignoreRootObj = hitCollider.attachedRigidbody?.gameObject ?? hitCollider.gameObject;
            var ignoreRootObjCollider = ignoreRootObj.GetComponent<Collider>();
            var ignoreRootObjCenter = ignoreRootObjCollider.GetRealWorldCenter();
            Collider[] ignoreColliders = ignoreRootObj.GetComponentsInChildren<Collider>();
            foreach (var ignoreCollider in ignoreColliders)
            {
                Physics.IgnoreCollision(_projectileCollider, ignoreCollider);
                Debug.Log($"Ignoring {_projectileCollider.gameObject.name} and {ignoreCollider.gameObject.name}");
                if (!_rangeSensor.IgnoreList.Contains(ignoreCollider.gameObject))
                {
                    _rangeSensor.IgnoreList.Add(ignoreCollider.gameObject);
                }
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            Debug.Log($"Before Set Pos: {rb.position}");
            // Set position before checking for targets in order to have less obstructed LOS
            _projectileDriver.SetPosition(ignoreRootObjCenter);
            Debug.Log($"After Set Pos: {rb.position}");

            //Find next Target
            _losSensor.PulseAll();
            var detections = _losSensor.GetDetectionsBySignalStrength();

            if (detections.Count == 0)
                return false;

            var target = detections[0];
            var targetCollider = target.GetComponent<Collider>();
            var homingTarget = target.GetComponent<ProjectileHomingTarget>()?.HomingTarget;

            if (homingTarget == null)
            {
                homingTarget = target.transform;
                Debug.LogWarning($"No ProjectileHomingTarget target found, defaulting to: {homingTarget.transform.name}");
            }
            
            // var targetPosition = targetCollider.GetRealWorldCenter();

            Debug.Log($"{targetCollider.transform.position} " +
                      $"| {targetCollider.GetRealWorldCenter()}");

            //Redirect Projectile at target
            var dirToTarget = homingTarget.position - transform.position;
            _projectileDriver.Redirect(dirToTarget);
            _projectileDriver.EnableHoming(true, homingTarget, _redirectLayer);

            Debug.Log($"Done: {rb.position}");
            remainingChains--;
            return true;
        }
    }
}