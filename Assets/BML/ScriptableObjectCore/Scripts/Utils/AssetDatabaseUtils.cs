using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace BML.ScriptableObjectCore.Scripts.Utils
{
    public static class AssetDatabaseUtils
    {
    
#if UNITY_EDITOR
    
        /// <summary>
        /// Get a list of asset relative paths for the given path and extension
        /// </summary>
        /// <param name="path"></param>
        /// <param name="includeSubdirectories"></param>
        /// <returns></returns>
        public static List<string> GetAssetRelativePaths(string path, bool includeSubdirectories, string extension = ".asset")
        {
            List<string> assetRelativePaths = new List<string>();
            var info = new DirectoryInfo(path);
            var fileInfo = info.GetFiles();
            foreach (var file in fileInfo)
            {
                if (file.Extension.Equals(extension))
                {
                    assetRelativePaths.Add($"{path}/{file.Name}");
                }
            }

            if (includeSubdirectories)
            {
                string[] subdirectoryNames = AssetDatabase.GetSubFolders(path);
                subdirectoryNames.ForEach(d => Debug.Log(d)); 
                foreach (var subDir in subdirectoryNames)
                {
                    assetRelativePaths = assetRelativePaths.Union(GetAssetRelativePaths(subDir, true).ToList()).ToList();
                }
            }

            return assetRelativePaths;
        }
        
        public static IEnumerable<T> FindAndLoadAssetsOfType<T>(string folderPath = null, bool includeSubdirectories = true) where T : Object
        {
            string[] searchInFolders = (string.IsNullOrEmpty(folderPath) ? null : new string[] { folderPath }); 
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}", searchInFolders)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(assetPath =>
                {
                    string assetFolderPath = Path.GetDirectoryName(assetPath);
                    bool inRootFolder = assetFolderPath == folderPath;
                    return includeSubdirectories || inRootFolder;
                })
                .Select(assetPath => AssetDatabase.LoadAssetAtPath<T>(assetPath));
        }
    
#endif
    }
}

