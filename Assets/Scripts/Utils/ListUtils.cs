using System;
using System.Collections.Generic;

namespace BML.Scripts.Utils
{
    public static class ListUtils
    {
        private static Random rng = new Random();  
        
        public static IList<T> Shuffle<T>(this IList<T> list, bool inPlace = false)  
        {  
            if(!inPlace) {
                list = new List<T>(list);
            }   

            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }

            return list;
        }
    }
}