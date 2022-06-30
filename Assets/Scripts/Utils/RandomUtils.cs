using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace BML.Scripts.Utils
{
    public class RandomUtils
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
    }
}