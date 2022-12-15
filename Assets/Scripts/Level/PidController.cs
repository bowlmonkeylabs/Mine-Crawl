﻿using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Compass;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.Level
{
    public class PidController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private TransformSceneReference _target;
        [SerializeField] private bool _doAlignToTargetOnEnable;
        
        [SerializeField] private float _maxAngularVelocity = 20;

        [SerializeField] private PIDParameters _thrustParameters;

        [SerializeField] private bool _useXValuesForAllAxes = false;

        [SerializeField] private PIDParameters _xAxisParameters;
        
        [HideIf("$_useXValuesForAllAxes")]
        [SerializeField] private PIDParameters _yAxisParameters;
        
        [HideIf("$_useXValuesForAllAxes")]
        [SerializeField] private PIDParameters _zAxisParameters;
        
        [ShowInInspector] private PID2 _thrustPIDController;
        [ShowInInspector] private PID2 _xAxisPIDController;
        [ShowInInspector] private PID2 _yAxisPIDController;
        [ShowInInspector] private PID2 _zAxisPIDController;

        private Rigidbody _rb;

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            if (_doAlignToTargetOnEnable)
            {
                var targetPosition = _target.Value.transform.position;
                var targetDirection = targetPosition - transform.position;
                Vector3 rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360, 0.00f);
                Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);

                transform.rotation = targetRotation;
            }
        }

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _thrustPIDController = new PID2(_thrustParameters);
            _xAxisPIDController = new PID2(_xAxisParameters);
            if (_useXValuesForAllAxes)
            {
                _yAxisPIDController = new PID2(_xAxisParameters);
                _zAxisPIDController = new PID2(_xAxisParameters);
            }
            else
            {
                _yAxisPIDController = new PID2(_yAxisParameters);
                _zAxisPIDController = new PID2(_zAxisParameters);
            }
        }
        
        private void Update()
        {
            if (_target.SafeIsUnityNull()) return;

            _thrustPIDController.Parameters = _thrustParameters;
            _xAxisPIDController.Parameters = _xAxisParameters;

            if (_useXValuesForAllAxes)
            {
                _yAxisPIDController.Parameters = _xAxisParameters;
                _zAxisPIDController.Parameters = _xAxisParameters;
            }
            else
            {
                _yAxisPIDController.Parameters = _yAxisParameters;
                _zAxisPIDController.Parameters = _zAxisParameters;
            }
        }
        
        void FixedUpdate()
        {
            _rb.maxAngularVelocity = _maxAngularVelocity;
            
            if (_target.SafeIsUnityNull()) return;
            
            //Get the required rotation based on the target position - we can do this by getting the direction
            //from the current position to the target. Then use rotate towards and look rotation, to get a quaternion thingy.
            var targetPosition = _target.Value.transform.position;
            var rotation = transform.rotation;
            var targetDirection = targetPosition - transform.position;
            Vector3 rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360, 0.00f);
            Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);

            //Figure out the error for each axis
            var thrustAlignment = 
                Vector3.Project(_rb.velocity, targetDirection).magnitude / _rb.velocity.magnitude;
            var velocityZero = Mathf.Clamp01(1 - (_rb.velocity.magnitude / 5f));
            
            float thrustCorrection = _thrustPIDController.GetOutput(
                0, 
                targetDirection.magnitude * (thrustAlignment + velocityZero), 
                Time.fixedDeltaTime
            );
            
            float xTorqueCorrection = _xAxisPIDController.GetOutput(
                rotation.eulerAngles.x, 
                targetRotation.eulerAngles.x, 
                Time.fixedDeltaTime);

            float yTorqueCorrection = _yAxisPIDController.GetOutput(
                rotation.eulerAngles.y, 
                targetRotation.eulerAngles.y, 
                Time.fixedDeltaTime);

            float zTorqueCorrection = _zAxisPIDController.GetOutput(
                rotation.eulerAngles.z, 
                targetRotation.eulerAngles.z, 
                Time.fixedDeltaTime);
            
            var torque = (xTorqueCorrection * Vector3.right) + (yTorqueCorrection * Vector3.up) +
                         (zTorqueCorrection * Vector3.forward);
            _rb.AddRelativeTorque(torque);
            // var sqrDistance = (targetPosition - _rb.position).sqrMagnitude;
            // var sqrDistanceFactor = Mathf.Clamp01(sqrDistance / _maxSqrDistance);
            // var thrust = _thrustBySquareDistance.Evaluate(sqrDistanceFactor) * _maxThrust;
            // _rb.AddRelativeForce((Vector3.forward) * thrust * Time.fixedDeltaTime);
            _rb.AddRelativeForce((Vector3.forward) * thrustCorrection);
        }

        private void OnDrawGizmos()
        {
            var position = transform.position;
            var targetPosition = _target.Value.transform.position;
            var targetDirection = targetPosition - position;

            Shapes.Draw.Line(position, position + transform.forward, Color.blue);
            Shapes.Draw.Line(position, position + targetDirection.normalized, Color.yellow);
        }

        #endregion
    }
}