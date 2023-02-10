using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BML.Scripts.Utils
{
    public static class RandomUtils
    {
        [Serializable]
        public struct WeightPair<T>
        {
            [HorizontalGroup("Split", 0.8f, LabelWidth = 1)]
            [HideLabel] [HideReferenceObjectPicker]
            [InlineProperty] public T value;
            
            [HorizontalGroup("Split", 0.2f, LabelWidth = 1)]
            [HideLabel]
            public float weight;
            
            public WeightPair(T value, float weight)
            {
                this.value = value;
                this.weight = weight;
            }
        }
        
        [Serializable]
        public struct WeightedOptions<T>
        {
            [HideLabel] [LabelText("Weighted Options")] [Indent(0)]
            [HideReferenceObjectPicker]
            public List<WeightPair<T>> Options;
            
            [Button]
            [PropertyOrder(-1)]
            [DisableIf("@Mathf.Apprxoximately(this.SumWeights, 1f)")]
            [HorizontalGroup("Split", 0.5f, LabelWidth = 100)]
            public void Normalize()
            {
                if (Mathf.Approximately(SumWeights, 1f)) return;

                float scalingFactor =  1f / SumWeights;
                for (var i = 0; i < Options.Count; i++)
                {
                    var option = Options[i];
                    Options[i] = new WeightPair<T>(option.value, option.weight * scalingFactor);
                }
            }

            [InfoBox("Weights must sum to 1.", InfoMessageType.Error, "@this.SumWeights != 1")]
            [HorizontalGroup("Split", 0.5f, LabelWidth = 100)]
            [ShowInInspector] 
            public float SumWeights => Options?.Sum(option => option.weight) ?? -1;
            
            public T RandomWithWeights()
            {
                float rand = Random.Range(0f, 1f);
                float acc = 0;
                foreach (var pair in this.Options)
                {
                    acc += pair.weight;
                    if (rand <= acc) return pair.value;
                }
                return this.Options.First().value;
            }
        }
        
        public static T RandomWithWeights<T>(List<RandomUtils.WeightPair<T>> choices)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min">Inclusive min.</param>
        /// <param name="max">Inclusive max.</param>
        /// <returns></returns>
        public static int Range(int min, int max)
        {
            int length = max - min + 1;
            float stepSize = 1f / (float)length;
            float rand = Random.value;
            float acc = 0;
            for (int i = 0; i < length; i++)
            {
                acc += stepSize;
                if (rand <= acc) return i + min;
            }
            return -1;
        }
        
    }
}