using UnityEngine;
using System.Collections.Generic;
using System;

namespace BML.Scripts.Attributes {
    public class EnumFlagsAttribute : PropertyAttribute
    {
        public EnumFlagsAttribute() { }

        public static List<T> GetSelectedElements<T>(T enumValue) where T : System.Enum
        {
            List<T> selectedElements = new List<T>();
            for (int i = 0; i < System.Enum.GetValues(typeof(T)).Length; i++)
            {
                int layer = 1 << i;
                if (((int) (object) enumValue & layer) != 0)
                {
                    selectedElements.Add(enumValue);
                }
            }
    
            return selectedElements;
    
        }
    }
}
