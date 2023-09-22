using System;
using BML.ScriptableObjectCore.Scripts.Variables.ValueTypeVariables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "QuaternionVariable", menuName = "BML/Variables/QuaternionVariable", order = 0)]
    public class QuaternionVariable : ValueTypeVariable<Quaternion> {}
    
    [Serializable]
    [InlineProperty]
    public class QuaternionReference : Reference<Quaternion, QuaternionVariable> { }
}