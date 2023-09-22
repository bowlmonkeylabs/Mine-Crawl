using System.Collections.Generic;
using System.Linq;
using BML.Utils.Random;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Utils.Random
{
    public static class RandomUtils
    {
        public static T RandomWithWeights<T>(List<WeightedValueEntry<T>> choices)
        {
            float rand = UnityEngine.Random.Range(0f, 1f);
            float acc = 0;
            foreach (var pair in choices)
            {
                acc += pair.Weight;
                if (rand <= acc) return pair.Value;
            }
            return choices.First().Value;
        }

        /// <summary>
        /// Returns a random position inside the bounds. 
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns>Position relative to the bounds center.</returns>
        public static Vector3 RandomInBounds(Bounds bounds)
        {
            var size = bounds.max - bounds.min;
            var randomPosition = new Vector3(
                UnityEngine.Random.value * size.x,
                UnityEngine.Random.value * size.y,
                UnityEngine.Random.value * size.z) - size / 2;
            return randomPosition;
        }
        
        public static T GetRandomElement<T>(this List<T> list) where T : class
        {
            int randomIndex = UnityEngine.Random.Range(0, list.Count);
            return list[randomIndex];
        }
        
        /// <summary>
        /// Credit:  http://answers.unity.com/answers/1734538/view.html
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
        {
            float u, v, S;
 
            do
            {
                u = 2.0f * UnityEngine.Random.value - 1.0f;
                v = 2.0f * UnityEngine.Random.value - 1.0f;
                S = u * u + v * v;
            }
            while (S >= 1.0f);
 
            // Standard Normal Distribution
            float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
 
            // Normal Distribution centered between the min and max value
            // and clamped following the "three-sigma rule"
            float mean = (minValue + maxValue) / 2.0f;
            float sigma = (maxValue - mean) / 3.0f;
            return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
        }
        
        public static int RandomGaussian(int minValue = 0, int maxValue = 1)
        {
            float random = RandomGaussian((float)minValue, (float)maxValue);
            int rounded = Mathf.Clamp(Mathf.RoundToInt(random), minValue, maxValue);
            return rounded;
        }
        
    }
}