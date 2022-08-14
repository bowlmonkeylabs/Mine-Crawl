using UnityEditor;
using UnityEngine;

namespace BML.Scripts.Utils
{
    public static class ApplicationUtils
    {
        public static bool IsPlaying_EditorSafe =>
            #if UNITY_EDITOR
            EditorApplication.isPlayingOrWillChangePlaymode;
            #else
            Application.isPlaying;
            #endif
    }
}