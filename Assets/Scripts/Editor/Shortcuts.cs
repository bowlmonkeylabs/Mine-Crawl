using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace BML.Scripts.Editor
{
    public class Shortcuts
    {
        // Alt + K
        [Shortcut("Clear Console", KeyCode.K, ShortcutModifiers.Alt)]
        public static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
        
    }
}