using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.Utils
{
    public static class RandomUtils
    {
        [Serializable]
        public struct WeightPair<T>
        {
            public T value;
            public float weight;
            public WeightPair(T value, float weight)
            {
                this.value = value;
                this.weight = weight;
            }
        }
        
        public static T RandomWithWeights<T>(List<WeightPair<T>> choices)
        {
            float rand = Random.Range(0f, 1f);
            float acc = 0;
            foreach (var pair in choices)
            {
                acc += pair.weight;
                if (rand <= acc) return pair.value;
            }
            return choices.First().value;
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
                Random.value * size.x,
                Random.value * size.y,
                Random.value * size.z) - size / 2;
            return randomPosition;
        }
        
        public static T GetRandomElement<T>(this List<T> list) where T : class
        {
            int randomIndex = Random.Range(0, list.Count);
            return list[randomIndex];
        }
    }
}