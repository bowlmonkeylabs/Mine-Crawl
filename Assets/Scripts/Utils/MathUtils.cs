using UnityEngine;
using System.Linq;

namespace BML.Scripts.Utils
{
    public static class MathUtils
    {
        /// <summary>
        /// Get the distance from the center of a rectangle to a point on its edge in direction of dir.
        /// Can multiply this result by dir to get the vector to that point on edge from the center
        /// </summary>
        /// <param name="rectWidth"></param>
        /// <param name="rectHeight"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static float GetDistanceToRectangleEdgeFromCenter(float rectWidth, float rectHeight, Vector3 dir)
        {
            float dirAngle = Vector2.Angle(Vector2.up, dir) * Mathf.Deg2Rad;
            float d1 = Mathf.Abs((rectHeight / 2f) / Mathf.Cos(dirAngle));
            float d2 = Mathf.Abs((rectWidth / 2f) / Mathf.Sin(dirAngle));
            float distanceToEdge = Mathf.Min(d1, d2);

            return distanceToEdge;
        }

        /// <summary>
        /// Get a random number in a range that is reflected about 0
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float GetRandomInRangeReflected(float min, float max)
        {
            float randInRange = Random.Range(min, max);
            float randInReflectedRange = Random.Range(-max, -min);
            return Random.Range(0, 2) == 0 ? randInRange : randInReflectedRange;
        }

        public static int Factorial(int num) {
            return Enumerable.Range(1, num).ToList().Aggregate(1, (factorial, x) => factorial * x);
        }
    }
}
