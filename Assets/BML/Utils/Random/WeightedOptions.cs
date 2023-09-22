using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BML.Utils.Random
{        
    [Serializable]
    public class WeightedValueEntry<T> : ICloneable
    {
        // [HorizontalGroup("Split", 0.8f, LabelWidth = 1)]
        [FormerlySerializedAs("value")]
        [HideLabel] [HideReferenceObjectPicker]
        [InlineProperty] public T Value;

        // [HorizontalGroup("Split", 0.2f, LabelWidth = 1)] [HideLabel]
        [FormerlySerializedAs("weight")]
        [MinValue(0)]
        [TableColumnWidth(40, false)]
        public float Weight;

        public WeightedValueEntry(T value, float weight)
        {
            this.Value = value;
            this.Weight = weight;
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    [Serializable]
    public class WeightedReferenceEntry<T> : WeightedValueEntry<T>, ICloneable where T : ICloneable
    {
        public WeightedReferenceEntry(T value, float weight) : base(value, weight) { }

        public override object Clone()
        {
            var clone = (WeightedReferenceEntry<T>)this.MemberwiseClone();
            clone.Value = (T)this.Value.Clone();
            return clone;
        }
    }
    
    [Serializable]
    public class WeightedReferenceOptions<T> : WeightedOptions<T,WeightedReferenceEntry<T>>, ICloneable where T : ICloneable
    {
            
    }

    [Serializable]
    public class WeightedValueOptions<T> : WeightedOptions<T,WeightedValueEntry<T>>, ICloneable
    {
            
    }
    
    [Serializable]
    public class WeightedOptions<T, TWP> : ICloneable where TWP : WeightedValueEntry<T>
    {
        [LabelText("Weighted Options"), HideLabel, Indent(0)]
        [HideReferenceObjectPicker]
        [TableList(AlwaysExpanded = true)]
        public List<TWP> Options;
        
        [Button]
        [PropertyOrder(-1)]
        [DisableIf("@Mathf.Approximately(this.SumWeights, 1f)")]
        [HorizontalGroup("Split", 0.5f, LabelWidth = 100)]
        public void Normalize()
        {
            if (Mathf.Approximately(SumWeights, 1f)) return;

            float scalingFactor =  1f / SumWeights;
            for (var i = 0; i < Options.Count; i++)
            {
                var option = Options[i];
                option.Weight *= scalingFactor;
            }
        }

        [InfoBox("Weights must sum to 1.", InfoMessageType.Error, "@this.SumWeights != 1")]
        [HorizontalGroup("Split", 0.5f, LabelWidth = 100)]
        [ShowInInspector] 
        public float SumWeights => Options?.Sum(option => option.Weight) ?? -1;
        
        public T RandomWithWeights()
        {
            float rand = UnityEngine.Random.value;
            return RandomWithWeights(rand);
        }
        
        public T RandomWithWeights(float randomRoll)
        {
            float acc = 0;
            foreach (var pair in this.Options)
            {
                acc += pair.Weight;
                if (randomRoll <= acc)
                {
                    return pair.Value;
                }
            }
            return this.Options.First().Value;
        }

        public object Clone()
        {
            var clone = (WeightedOptions<T,TWP>)this.MemberwiseClone();
            clone.Options = this.Options.Select(pair => (TWP)pair.Clone()).ToList();
            return clone;
        }
    }
    
}