using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts
{
    [Serializable]
    [InlineProperty]
    public class CloneableAnimationCurve : ICloneable
    {
        [SerializeField, HideLabel] private AnimationCurve _animationCurve;

        public CloneableAnimationCurve(AnimationCurve animationCurve)
        {
            _animationCurve = animationCurve;
        }
        
        public object Clone()
        {
            return new CloneableAnimationCurve(new AnimationCurve(_animationCurve.keys));
        }

        public Keyframe[] keys => _animationCurve.keys;
        public float Evaluate(float time) => _animationCurve.Evaluate(time);
        
        public static implicit operator CloneableAnimationCurve(AnimationCurve c) => new CloneableAnimationCurve(c);
        public static implicit operator AnimationCurve(CloneableAnimationCurve c) => c._animationCurve;
    }
}