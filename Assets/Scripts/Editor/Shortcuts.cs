using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace BML.Scripts.Editor
{
    public class Shortcuts
    {
        // Alt + C
        [Shortcut("Clear Console", KeyCode.C, ShortcutModifiers.Alt)]
        public static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
        
        
        // Alt + F
        [Shortcut("Find Player In Scene View", KeyCode.F, ShortcutModifiers.Alt)]
        public static void FindPlayerInSceneView()
        {
            var playerInScene = GameObject.FindWithTag("Player");
            if (playerInScene == null)
            {
                Debug.LogWarning("Could not find 'Player'.");
                return;
            }
            Selection.activeTransform = playerInScene.transform;
            SceneView.lastActiveSceneView.FrameSelected();
        }
        
    }
}