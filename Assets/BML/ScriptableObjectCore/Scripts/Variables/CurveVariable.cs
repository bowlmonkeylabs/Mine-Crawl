using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "CurveVariable", menuName = "BML/Variables/CurveVariable", order = 0)]
    public class CurveVariable : Variable<AnimationCurve> {}
}