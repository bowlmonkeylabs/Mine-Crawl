using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "QuaternionVariable", menuName = "BML/Variables/QuaternionVariable", order = 0)]
    public class QuaternionVariable : Variable<Quaternion> {}
}