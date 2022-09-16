using UnityEditor;
using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class SelectionUtils
    {
        public static bool InSelection(Transform transform)
        {
#if !UNITY_EDITOR
            return false;
#else
            for (int i = 0; i < Selection.transforms.Length; i++)
            {
                var current = Selection.transforms[i];
                if (current == transform)
                {
                    return true;
                }
            }
            return false;
#endif
        }
        
        public static bool InSelection(Transform[] transforms)
        {
#if !UNITY_EDITOR
            return false;
#else
            for (int i = 0; i < Selection.transforms.Length; i++)
            {
                var current = Selection.transforms[i];
                for (int j = 0; j < transforms.Length; j++)
                {
                    if (current == transforms[j])
                    {
                        return true;
                    }
                }
            }
            return false;
#endif
        }
    }
}