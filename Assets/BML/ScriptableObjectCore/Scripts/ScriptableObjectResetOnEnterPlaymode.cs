using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BML.ScriptableObjectCore.Scripts
{
    public static class ScriptableObjectResetOnEnterPlaymode
    {
        private static HashSet<string> _variablesToReset;
        
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            _variablesToReset = new HashSet<string> {};
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                foreach (string path in _variablesToReset)
                {
                    ScriptableVariableBase variableBase = AssetDatabase.LoadAssetAtPath<ScriptableVariableBase>(path);

                    if (variableBase != null)
                    {
                        variableBase.Reset();
                    }
                }
            }
        }
#endif
        
        public static void RegisterVariableToReset(string path)
        {
#if UNITY_EDITOR
            _variablesToReset.Add(path);
#endif
        }
        
        public static void UnRegisterVariableToReset(string path)
        {
#if UNITY_EDITOR
            _variablesToReset.Remove(path);
#endif
        }
    }
}