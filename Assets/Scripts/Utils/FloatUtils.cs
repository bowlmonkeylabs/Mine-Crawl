using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class FloatUtils
    {
        public static float RoundNearZero(this float val)
        {
            return Mathf.Abs(val) < 0.001f ? 0 : val;
        }

        public static float RemapRange(float value, float oldMin, float oldMax, float newMin, float newMax, bool clamp = false)
        {
            float oldRange = (oldMax - oldMin);
            float pct = Mathf.Approximately(oldRange, 0) 
                ? 0 : (value - oldMin) / oldRange;
            float newValue = pct * (newMax - newMin) + newMin;
            newValue = (clamp ? Mathf.Clamp(newValue, newMin, newMax) : newValue);
            return newValue;
        }
    }
}