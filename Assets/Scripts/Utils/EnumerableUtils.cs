using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class EnumerableUtils
    {
        public static Vector3 Sum(this IEnumerable<Vector3> vectors)
        {
            Vector3 sum = new Vector3();
            foreach (var vec in vectors)
            {
                sum += vec;
            }
            return sum;
        }
        
        public static Vector3 Average(this IEnumerable<Vector3> vectors)
        {
            Vector3 sum = new Vector3();
            int count = 0;
            foreach (var vec in vectors)
            {
                sum += vec;
                count++;
            }

            Vector3 average = sum / count;
            return average;
        }
    }
}