using System;
using BML.ScriptableObjectCore.Scripts.Variables.ReferenceTypeVariables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "CurveVariable", menuName = "BML/Variables/CurveVariable", order = 0)]
    public class CurveVariable : ReferenceTypeVariable<CloneableAnimationCurve> { }
    
    [Serializable]
    [InlineProperty]
    public class CurveReference : Reference<CloneableAnimationCurve, CurveVariable>, ISerializationCallbackReceiver
    {

        private float minValue = float.MaxValue;
        private float minTime = float.MaxValue;
        private float maxValue = float.MinValue;
        private float maxTime = float.MinValue;

        public float MaxValue => maxValue;
        public float MaxTime => maxTime;
        public float MinValue => minValue;
        public float MinTime => minTime;

        private void RefreshMinMax()
        {
            if((Variable != null && Variable.Value != null) || (UseConstant && ConstantValue != null))
            {
                var curve = UseConstant ? ConstantValue : Variable.Value;
                foreach(var key in curve.keys)
                {
                    if(key.value < minValue)
                    {
                        minValue = key.value;
                        minTime = key.time;
                    }
                    if(key.value > maxValue)
                    {
                        maxValue = key.value;
                        maxTime = key.time;
                    }
                }
            }
        }

        public float EvaluateFactor(float time, bool zeroBaseline = false)
        {
            var value = Value.Evaluate(time);
            var min = zeroBaseline ? 0 : minValue;
            var factor = (value - min) / (maxValue - min);
            return factor;
        }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            RefreshMinMax();
        }
    }
}