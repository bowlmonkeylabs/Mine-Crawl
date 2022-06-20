using System.IO;
using UnityEngine;
using UnityEditor;

namespace BML.Build
{
    public class EditorBuilder : MonoBehaviour
    {
        [MenuItem("Build/Open Build Folder")]
        public static void OpenBuildFolder()
        {
            var pathParts = new string[] { Application.dataPath, "../", Builder.BuildsFolder };
            var buildsFolderAbsolutePath = Path.GetFullPath(
                Path.Combine(pathParts)
            );
            System.Diagnostics.Process.Start("explorer.exe", buildsFolderAbsolutePath);
        }
        
        [MenuItem("Build/Build Windows (Development)")]
        public static void BuildWindowsDevelopment()
        {
            Builder.BuildProject(
                BuildTarget.StandaloneWindows64,
                BuildOptions.Development,
                Builder.OverwriteMode.Overwrite
            );
        }

        [MenuItem("Build/Build Windows (Release)")]
        public static void BuildWindowsRelease()
        {
            Builder.BuildProject(
                BuildTarget.StandaloneWindows64,
                BuildOptions.None,
                Builder.OverwriteMode.Overwrite
            );
        }
        
        [MenuItem("Build/Build WebGL (Development)")]
        public static void BuildWebGlDevelopment()
        {
            Builder.BuildProject(
                BuildTarget.WebGL,
                BuildOptions.Development,
                Builder.OverwriteMode.Overwrite
            );
        }
        
        [MenuItem("Build/Build WebGL (Release)")]
        public static void BuildWebGlRelease()
        {
            Builder.BuildProject(
                BuildTarget.WebGL,
                BuildOptions.None,
                Builder.OverwriteMode.Overwrite
            );
        }
        
    }
}