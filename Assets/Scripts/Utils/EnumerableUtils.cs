using System;
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

        public static IEnumerable<IEnumerable<T>> Permutate<T>(this IEnumerable<T> objects) {
            var permutation = objects.ToList();
            var length = objects.Count();
            var ret = new List<List<T>>();
            ret.Add(objects.ToList());
            var c =  Enumerable.Repeat(0, length).ToList();
            var i = 1;
            var k = 0;
            T p = default(T);

            while(i < length) {
                if(c[i] < i) {
                    k = i % 2 == 1 ? c[i] : 0;
                    p = permutation[i];
                    permutation[i] = permutation[k];
                    permutation[k] = p;
                    ++c[i];
                    i = 1;
                    ret.Add(new List<T>(permutation));
                } else {
                    c[i] = 0;
                    ++i;
                }
            }

            return ret;
        }

        public static string ToPrint<T>(this IEnumerable<T> objects, Func<T, string> formatFunc = null) {
            if(formatFunc == null) {
                return string.Join(", ", objects);
            }

            return string.Join(", ", objects.Select(o => formatFunc(o)));
        }
    }
}