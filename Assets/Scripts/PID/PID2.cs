﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Compass
{
    public enum DifferenceFunction
    {
        Subtract,
        DeltaAngle,
    }
    
    [Serializable]
    public class PIDParameters
    {
        /// <summary>
        /// Constant proportion
        /// </summary>
        public float Kp;

        /// <summary>
        /// Constant integral
        /// </summary>
        public float Ki;

        /// <summary>
        /// Constant derivative
        /// </summary>
        public float Kd;

        public float IntegralSaturation = 1f;

        public Vector2 OutputMinMax = new Vector2(-1, 1);

        public DerivativeMethod DerivativeMethod = DerivativeMethod.Velocity;

        public DifferenceFunction DifferenceFunction = DifferenceFunction.Subtract;

        public static Func<float, float, float> Subtract = (float amount, float from) => from - amount;

        public PIDParameters(float kp, float ki, float kd, float integralSaturation, Vector2 outputMinMax, DerivativeMethod derivativeMethod = DerivativeMethod.Velocity, DifferenceFunction differenceFunction = DifferenceFunction.Subtract)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;
            IntegralSaturation = integralSaturation;
            OutputMinMax = outputMinMax;
            DerivativeMethod = derivativeMethod;
            DifferenceFunction = differenceFunction;
        }
        
    }
    
    [Serializable]
    public class PID2
    {
        public PIDParameters Parameters
        {
            get => _parameters;
            set
            {
                _parameters = value;

                switch (_parameters.DifferenceFunction)
                {
                    case DifferenceFunction.Subtract:
                        _differenceFunction = PIDParameters.Subtract;
                        break;
                    case DifferenceFunction.DeltaAngle:
                        _differenceFunction = Mathf.DeltaAngle;
                        break;
                }
            }
        }
        [ShowInInspector] private PIDParameters _parameters;

        private Func<float, float, float> _differenceFunction;

        [ShowInInspector] private bool _isDerivativeInitialized;
        [ShowInInspector] private float _p, _i, _d;
        [ShowInInspector] private float _prevError;
        [ShowInInspector] private float _prevValue;

        public float PrevError => _prevError;
        public float PrevValue => _prevValue;

        public PID2(PIDParameters parameters)
        {
            Parameters = parameters;
        }

        /// <summary>
        /// Based on the code from Brian-Stone on the Unity forums
        /// https://forum.unity.com/threads/rigidbody-lookat-torque.146625/#post-1005645
        /// </summary>
        /// <param name="currentError"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public float GetOutput(float currentValue, float targetValue, float deltaTime)
        {
            var currentError = _differenceFunction(currentValue, targetValue);
            currentError = (Double.IsNaN(currentError) ? 0 : currentError);
            _p = currentError;
            _i = Mathf.Clamp(
                _i + (_p * deltaTime),
                -_parameters.IntegralSaturation,
                _parameters.IntegralSaturation
            );
            if (!_isDerivativeInitialized)
            {
                _isDerivativeInitialized = true;
            }
            else
            {
                switch (_parameters.DerivativeMethod)
                {
                    case DerivativeMethod.ErrorRateChange:
                        float errorRateOfChange = (_p - _prevError) / deltaTime;
                        _d = errorRateOfChange;
                        break;
                    default:
                    case DerivativeMethod.Velocity:
                        float valueRateOfChange = (currentValue - _prevValue) / deltaTime;
                        _d = -valueRateOfChange;
                        break;
                
                }
            }
            
            _prevValue = currentValue;
            _prevError = currentError;

            var p = _p * _parameters.Kp;
            var i = _i * _parameters.Ki;
            var d = _d * _parameters.Kd;
        
            return Mathf.Clamp(p + i + d, _parameters.OutputMinMax.x, _parameters.OutputMinMax.y);
        }

        public void Reset()
        {
            _isDerivativeInitialized = false;
        }
    }
}