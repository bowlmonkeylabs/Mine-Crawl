using System;
using UnityEngine;

namespace BML.Scripts.Compass
{
    [Serializable]
    public class PID
    {
        public DerivativeMethod DerivativeMethod;
        public float IntegralSaturation;

        public static Func<float, float, float> Subtract = (float a, float b) => b - a;
        private Func<float, float, float> _differenceFunction;

        private bool _isDerivativeInitialized;
        private float _p, _i, _d;
        private float _prevError;
        private float _prevValue;

        /// <summary>
        /// Constant proportion
        /// </summary>
        public float Kp { get; set; }

        /// <summary>
        /// Constant integral
        /// </summary>
        public float Ki { get; set; }

        /// <summary>
        /// Constant derivative
        /// </summary>
        public float Kd { get; set; }

        public PID(float p, float i, float d, float integralSaturation, DerivativeMethod derivativeMethod = DerivativeMethod.Velocity, Func<float, float, float> differenceFunction = null)
        {
            Kp = p;
            Ki = i;
            Kd = d;
            IntegralSaturation = integralSaturation;
            DerivativeMethod = derivativeMethod;
            _differenceFunction = (differenceFunction ?? Subtract);
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
            _p = currentError;
            _i += _p * deltaTime;
            if (!_isDerivativeInitialized)
            {
                _isDerivativeInitialized = true;
            }
            else
            {
                switch (DerivativeMethod)
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
        
            return _p * Kp + _i * Ki + _d * Kd;
        }

        public void Reset()
        {
            _isDerivativeInitialized = false;
        }
    }
}