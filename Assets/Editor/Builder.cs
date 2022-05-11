using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace BML.Build
{
    public static class Builder
    {
        private static string _buildsFolder = @"./Builds";

        private struct BuildTargetInfo
        {
            public string extension;
            public BuildTargetInfo(string extension) {
                this.extension = extension;
            }
        }
        private static Dictionary<BuildTarget, BuildTargetInfo> buildTargetInfo =
            new Dictionary<BuildTarget, BuildTargetInfo> {
                { BuildTarget.StandaloneWindows, new BuildTargetInfo(".exe") },
                { BuildTarget.StandaloneWindows64, new BuildTargetInfo(".exe") },
                // TODO below this line I do not know the correct extensions
                // { BuildTarget.StandaloneLinux64, (extension:"") },
                // { BuildTarget.StandaloneOSX, (extension:"") },
                // { BuildTarget.WebGL, (extension:"") },
            };

        #region Build methods
        public enum OverwriteMode
        {
            Abort,
            Overwrite,
            Increment,
        }
        public static void BuildProject(
            BuildTarget buildTarget,
            BuildOptions buildOptions,
            OverwriteMode overwriteMode = OverwriteMode.Abort
        ) {
            // Check if build target is known
            bool buildTargetIsKnown = buildTargetInfo.ContainsKey(buildTarget);
            if (!buildTargetIsKnown)
            {
                throw new System.Exception("I don't know how to handle this build target. Please update the Builder script to suppport this.");
            }

            var buildName = GenerateBuildName(_buildsFolder, buildTarget);

            // Check if build name already exists
            bool alreadyExists = UnityEngine.Windows.File.Exists(buildName.fullPath);
            // var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            if (alreadyExists)
            {
                switch (overwriteMode)
                {
                case OverwriteMode.Abort:
                    throw new Exception("Requested build already exists. Clear the existing folder or build with overwrite enabled.");
                    // break;
                case OverwriteMode.Overwrite:
                    // Delete the old folder
                    // TODO change this to build to a temp path, and only overwrite the old build if new one succeeds
                    // var tempPath = UnityEditor.FileUtil.GetUniqueTempPathInProject();
                    var deleted = UnityEditor.FileUtil.DeleteFileOrDirectory(buildName.fullPath);
                    break;
                case OverwriteMode.Increment:
                    throw new NotImplementedException("OverwriteMode.Increment is not yet implemented.");
                    // break;
                }
            }

            var scenes = EditorBuildSettings.scenes.Select((EditorBuildSettingsScene scene) => scene.path).ToArray();

            var options = new BuildPlayerOptions
            {
                scenes = scenes, 
                target = buildTarget,
                locationPathName = buildName.fullPath,
                options = buildOptions,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
                throw new System.Exception("Build failed");
            }
        }

        public static void BuildProjectCommandLine()
        {
            BuildTarget? buildTarget =  null;
            BuildOptions? buildOptions = null;
            OverwriteMode? overwriteMode = null;

            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                int argValueIndex;
                string argValue;
                switch (arg)
                {
                case "-buildTarget":
                    argValueIndex = i + 1;
                    if (argValueIndex >= args.Length) throw new ArgumentException("Usage: -buildTarget <BuildTarget>");
                    argValue = args[argValueIndex];
                    try {
                        buildTarget = Enum.Parse(typeof(BuildTarget), argValue) as BuildTarget?;
                    } catch (Exception e) {
                        throw new ArgumentException("Failed to parse build target");
                    }
                    if (!buildTarget.HasValue) throw new ArgumentException("Invalid build target");
                    break;
                case "-buildOptions":
                    argValueIndex = i + 1;
                    if (argValueIndex >= args.Length) throw new ArgumentException("Usage: -buildOptions <BuildOptions>");
                    argValue = args[argValueIndex];
                    BuildOptions? option = null;
                    try {
                        option = Enum.Parse(typeof(BuildOptions), argValue) as BuildOptions?;
                    } catch (Exception e) {
                        throw new ArgumentException("Failed to parse build option");
                    }
                    if (!option.HasValue) throw new ArgumentException("Invalid build target");
                    if (!buildOptions.HasValue) buildOptions = option;
                    else buildOptions = buildOptions.Value | option.Value;
                    break;
                case "-overwriteMode":
                    argValueIndex = i + 1;
                    if (argValueIndex >= args.Length)
                    {
                        throw new ArgumentException("Usage: -overwriteMode <OverwriteMode>");
                    }
                    argValue = args[argValueIndex];
                    overwriteMode = Enum.Parse(typeof(OverwriteMode), argValue) as OverwriteMode?;
                    break;
                }
            }

            if (buildTarget == null) throw new ArgumentException("No build target specified.");
            if (buildOptions == null) buildOptions = BuildOptions.None;
            if (overwriteMode == null) overwriteMode = OverwriteMode.Abort;

            BuildProject(buildTarget.Value, buildOptions.Value, overwriteMode.Value);
        }
        #endregion

        #region Helpers
        private static (string folderName, string executableName, string fullPath) GenerateBuildName(
            string rootBuildFolder, BuildTarget buildTarget
        ) {
            var projectName = Application.productName;
            var projectVersion = Application.version;
            var folderName = $"{projectName}_{projectVersion}_{buildTarget.ToString()}";

            var executableExtension = buildTargetInfo[buildTarget].extension;
            var executableName = $"{projectName}_{projectVersion}{executableExtension}";

            var pathParts = new string[] { rootBuildFolder, folderName, executableName };
            var fullPath = System.IO.Path.Combine(pathParts);

            return (folderName, executableName, fullPath);
        }

        private static void Shout(string message)
        {
            Debug.Log("-- SHOUT ---------------------");
            Debug.Log(message);
            Debug.Log("------------------------------");
        }
        #endregion
    }
}