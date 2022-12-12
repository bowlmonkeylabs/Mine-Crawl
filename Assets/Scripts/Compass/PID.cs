using System;

namespace BML.Scripts.Compass
{
    [Serializable]
    public class PID
    {
        private float _p, _i, _d;
        private float _prevError;

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

        public PID(float p, float i, float d)
        {
            Kp = p;
            Ki = i;
            Kd = d;
        }

        /// <summary>
        /// Based on the code from Brian-Stone on the Unity forums
        /// https://forum.unity.com/threads/rigidbody-lookat-torque.146625/#post-1005645
        /// </summary>
        /// <param name="currentError"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public float GetOutput(float currentError, float deltaTime)
        {
            _p = currentError;
            _i += _p * deltaTime;
            _d = (_p - _prevError) / deltaTime;
            _prevError = currentError;
        
            return _p * Kp + _i * Ki + _d * Kd;
        }
    }
}