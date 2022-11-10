using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Compass;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor.Graphs;
using UnityEngine;

namespace BML.Scripts.Level
{
    public class PidController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private TransformSceneReference _target;
        [SerializeField] private float _maxAngularVelocity = 20;

        [SerializeField] private float _maxThrust = 500f;
        [SerializeField] private float _maxDistance = 5f;
        private float _maxSqrDistance;
        [SerializeField] private AnimationCurve _thrustBySquareDistance = AnimationCurve.Constant(0, 1, 1f);
        
        [SerializeField] private bool _useXValuesForAllAxes = false;

        [SerializeField] [Range(-10, 10)] private float _xAxisP = 1f, _xAxisI = 0.05f, _xAxisD = 0.2f;

        [HideIf("$_useXValuesForAllAxes")]
        [SerializeField] [Range(-10, 10)] private float _yAxisP, _yAxisI, _yAxisD;

        [HideIf("$_useXValuesForAllAxes")]
        [SerializeField] [Range(-10, 10)] private float _zAxisP, _zAxisI, _zAxisD;
        
        private PID _xAxisPIDController;
        private PID _yAxisPIDController;
        private PID _zAxisPIDController;

        private Rigidbody _rb;

        #endregion

        #region Unity lifecycle
        
        void Start()
        {
            _maxSqrDistance = _maxDistance * _maxDistance;
            
            //_pidController = gameObject.GetComponents<PID>()[0];
            _rb = GetComponent<Rigidbody>();
            _xAxisPIDController = new PID(_xAxisP, _xAxisI, _xAxisD);
            if (_useXValuesForAllAxes)
            {
                _yAxisPIDController = new PID(_xAxisP, _xAxisI, _xAxisD);
                _zAxisPIDController = new PID(_xAxisP, _xAxisI, _xAxisD);
            }
            else
            {
                _yAxisPIDController = new PID(_yAxisP, _yAxisI, _yAxisD);
                _zAxisPIDController = new PID(_zAxisP, _zAxisI, _zAxisD);
            }
        }
        
        private void Update()
        {
            if (_target.SafeIsUnityNull()) return;
            
            _xAxisPIDController.Kp = _xAxisP;
            _xAxisPIDController.Ki = _xAxisI;
            _xAxisPIDController.Kd = _xAxisD;

            if (_useXValuesForAllAxes)
            {
                _yAxisPIDController.Kp = _xAxisP;
                _yAxisPIDController.Ki = _xAxisI;
                _yAxisPIDController.Kd = _xAxisD;

                _zAxisPIDController.Kp = _xAxisP;
                _zAxisPIDController.Ki = _xAxisI;
                _zAxisPIDController.Kd = _xAxisD;
            }
            else
            {
                _yAxisPIDController.Kp = _yAxisP;
                _yAxisPIDController.Ki = _yAxisI;
                _yAxisPIDController.Kd = _yAxisD;

                _zAxisPIDController.Kp = _zAxisP;
                _zAxisPIDController.Ki = _zAxisI;
                _zAxisPIDController.Kd = _zAxisD;
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
            float xAngleError = Mathf.DeltaAngle(rotation.eulerAngles.x, targetRotation.eulerAngles.x);
            float xTorqueCorrection = _xAxisPIDController.GetOutput(xAngleError, Time.fixedDeltaTime);

            float yAngleError = Mathf.DeltaAngle(rotation.eulerAngles.y, targetRotation.eulerAngles.y);
            float yTorqueCorrection = _yAxisPIDController.GetOutput(yAngleError, Time.fixedDeltaTime);

            float zAngleError = Mathf.DeltaAngle(rotation.eulerAngles.z, targetRotation.eulerAngles.z);
            float zTorqueCorrection = _zAxisPIDController.GetOutput(zAngleError, Time.fixedDeltaTime);

            var torque = (xTorqueCorrection * Vector3.right) + (yTorqueCorrection * Vector3.up) +
                         (zTorqueCorrection * Vector3.forward);
            var sqrDistance = (targetPosition - _rb.position).sqrMagnitude;
            var sqrDistanceFactor = Mathf.Clamp01(sqrDistance / _maxSqrDistance);
            var thrust = _thrustBySquareDistance.Evaluate(sqrDistanceFactor) * _maxThrust;
            _rb.AddRelativeTorque(torque);
            _rb.AddRelativeForce((Vector3.forward) * thrust * Time.fixedDeltaTime);
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