using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "BML/Variables/BoolVariable", order = 0)]
    public class BoolVariable : Variable<bool> {}
}