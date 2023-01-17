using System.Collections;
using System.Collections.Generic;
using Intensity;
using UnityEngine;

namespace BML.Script.Intensity
{
    [CreateAssetMenu(fileName = "IntensityResponseStateData", menuName = "BML/Intensity/IntensityResponseStateData", order = 0)]
    public class IntensityResponseStateData : ScriptableObject
    {
        public IntensityController.IntensityResponse Value;
    }
}
