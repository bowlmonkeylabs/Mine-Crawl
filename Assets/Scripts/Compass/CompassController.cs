using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.Utils;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

namespace BML.Scripts.Compass
{
    public class CompassController : MonoBehaviour
    {
        private enum TargetMode
        {
            Direction,
            MatchRotation,
        }

        [SerializeField] private TargetMode _targetMode = TargetMode.Direction;
        [SerializeField] private SafeTransformValueReference _target = null;
        [SerializeField] private float _maxAngularVelocity = 20;

        [SerializeField] [Range(-100f, 100f)] private float _footstepNoiseInfluence = 1f;
        [SerializeField] private ForceMode _footstepNoiseForceMode = ForceMode.VelocityChange;

        [SerializeField] [Range(-100f, 100f)] private float _rotationNoiseInfluence = 1f;
        [SerializeField] private Vector3 _rotationNoiseScale = Vector3.one;
        [SerializeField] private ForceMode _rotationNoiseForceMode = ForceMode.Acceleration;
        [SerializeField] [Range(-100f, 100f)] private float _rotationNoiseAngularDifferenceThreshold = 0.1f;

        [SerializeField] private bool _useXValuesForAllAxes = false;
            
        [SerializeField] [Range(-10, 10)] private float _xAxisP, _xAxisI, _xAxisD;

        [HideIf("$_useXValuesForAllAxes")]
        [SerializeField] [Range(-10, 10)] private float _yAxisP, _yAxisI, _yAxisD;

        [HideIf("$_useXValuesForAllAxes")]
        [SerializeField] [Range(-10, 10)] private float _zAxisP, _zAxisI, _zAxisD;

        private PID _xAxisPIDController;
        private PID _yAxisPIDController;
        private PID _zAxisPIDController;

        private Rigidbody _rb;

        private Vector3? _footstepNoiseTargetPosition;

        void Start()
        {
            //_pidController = gameObject.GetComponents<PID>()[0];
            _rb = GetComponent<Rigidbody>();
            _rb.maxAngularVelocity = _maxAngularVelocity;
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
            bool addFootstepNoise = (_footstepNoiseTargetPosition != null &&
                                     !Mathf.Approximately(_footstepNoiseInfluence, 0f));
            if (addFootstepNoise)
            {
                var diffToNoiseTarget = _rb.transform.forward - _footstepNoiseTargetPosition.Value;
                _rb.AddTorque(diffToNoiseTarget * _footstepNoiseInfluence, _footstepNoiseForceMode);
                _footstepNoiseTargetPosition = null;
                return;
            }

            //Get the required rotation based on the target position - we can do this by getting the direction
            //from the current position to the target. Then use rotate towards and look rotation, to get a quaternion thingy.
            Vector3 targetDirection;
            Quaternion targetRotation;
            switch (_targetMode)
            {
                default:
                case TargetMode.Direction:
                    targetDirection = transform.position - _target.Value.transform.position;
                    Vector3 rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360, 0.00f);
                    targetRotation = Quaternion.LookRotation(rotationDirection);
                    break;
                case TargetMode.MatchRotation:
                    targetDirection = _target?.Value?.forward ?? transform.forward;
                    targetRotation = _target?.Value?.rotation ?? transform.rotation;
                    break;
            }

            float targetAngularDifference = Vector3.Angle(transform.forward, targetDirection);
            bool addRotationNoise = (!Mathf.Approximately(_rotationNoiseInfluence, 0f) &&
                                     targetAngularDifference >= _rotationNoiseAngularDifferenceThreshold);
            if (addRotationNoise)
            {
                var position = transform.position;
                var noiseTargetPosition = new Vector3(
                    Mathf.PerlinNoise(position.x * _rotationNoiseScale.x, 0f),
                    Mathf.PerlinNoise(position.y * _rotationNoiseScale.y, 0f),
                    Mathf.PerlinNoise(position.z * _rotationNoiseScale.z, 0f)
                );
                var diffToNoiseTarget = _rb.transform.forward - noiseTargetPosition;
                _rb.AddTorque(diffToNoiseTarget * _rotationNoiseInfluence, _rotationNoiseForceMode);
                return;
            }

            //Figure out the error for each axis
            float xAngleError = Mathf.DeltaAngle(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.x);
            float xTorqueCorrection = _xAxisPIDController.GetOutput(xAngleError, Time.fixedDeltaTime);

            float yAngleError = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, targetRotation.eulerAngles.y);
            float yTorqueCorrection = _yAxisPIDController.GetOutput(yAngleError, Time.fixedDeltaTime);

            float zAngleError = Mathf.DeltaAngle(transform.rotation.eulerAngles.z, targetRotation.eulerAngles.z);
            float zTorqueCorrection = _zAxisPIDController.GetOutput(zAngleError, Time.fixedDeltaTime);

            var torque = (xTorqueCorrection * Vector3.right) + (yTorqueCorrection * Vector3.up) +
                         (zTorqueCorrection * Vector3.forward);
            _rb.AddRelativeTorque(torque);
            // _rb.AddRelativeForce((-Vector3.forward) * _thrust * Time.fixedDeltaTime);
        }

        public void Footstep()
        {
            _footstepNoiseTargetPosition = Random.onUnitSphere;
        }
    }
}