using UnityEngine;
using UnityEditor;

namespace BML.Build
{
    public class EditorBuilder : MonoBehaviour
    {
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
    }
}