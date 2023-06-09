﻿#define CONSTANT_PATH

using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace Cinemachine.Editor
{
    /// <summary>
    /// This is a collection of utilities surrounding ScriptableObjects
    /// </summary>
    public class ScriptableObjectUtility : ScriptableObject
    {
        /// <summary>
        /// The default relative path to the root directory where Cinemachine is installed
        /// </summary>
        public static string kPackageRoot = "Assets/Engine/Package/com.unity.cinemachine";

        /// <summary>Get the Cinemachine package install path.  Works whether CM is
        /// a packman package or an ordinary asset.</summary>
        public static string CinemachineInstallPath
        {
            get
            {
                // First see if we're a UPM package - use some asset that we expect to find
                string path = Path.GetFullPath(kPackageRoot + "/Editor/EditorResources/cm_logo_sm.png");
                int index = path.LastIndexOf("/Editor");
                if (index < 0 || !File.Exists(path))
                {
                    // Try as an ordinary asset
                    ScriptableObject dummy = ScriptableObject.CreateInstance<ScriptableObjectUtility>();
                    path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(dummy));
                    if (path.Length > 0)
                        path = Path.GetFullPath(path);
                    DestroyImmediate(dummy);
                }
                path = path.Replace('\\', '/'); // because of GetFullPath()
                index = path.LastIndexOf("/Editor");
                if (index >= 0)
                    path = path.Substring(0, index);
                if (path.Length > 0)
                    path = Path.GetFullPath(path);  // stupid backslashes
                return path;
            }
        }

        /// <summary>Get the Cinemachine package install path.  Works whether CM is
        /// a packman package or an ordinary asset.</summary>
        public static string CinemachineRealativeInstallPath
        {
            get
            {
                // 2021年3月25日 张欣颖
                // 项目第一次导入时，ScriptableObjectUtility的Library cache有可能坏掉，需要重新载入，
                // 从而导致该获取路径的方法失效，并报大量(5位数)的warning导致Unity卡死。
                // 所以这里改成更稳固的方式，直接写死路径以确保不影响他人工作。
#if CONSTANT_PATH
                return kPackageRoot;
#else
                ScriptableObject dummy = ScriptableObject.CreateInstance<ScriptableObjectUtility>();
                var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(dummy));
                DestroyImmediate(dummy);
                var index = path.LastIndexOf("/Editor");
                if (index >= 0)
                    path = path.Substring(0, index);
                return path;
#endif
            }
        }

        /// <summary>Create a scriptable object asset</summary>
        /// <typeparam name="T">The type of asset to create</typeparam>
        /// <param name="assetPath">The full path and filename of the asset to create</param>
        /// <returns>The newly-created asset</returns>
        public static T CreateAt<T>(string assetPath) where T : ScriptableObject
        {
            return CreateAt(typeof(T), assetPath) as T;
        }

        /// <summary>Create a scriptable object asset</summary>
        /// <param name="assetType">The type of asset to create</param>
        /// <param name="assetPath">The full path and filename of the asset to create</param>
        /// <returns>The newly-created asset</returns>
        public static ScriptableObject CreateAt(Type assetType, string assetPath)
        {
            ScriptableObject asset = ScriptableObject.CreateInstance(assetType);
            if (asset == null)
            {
                Debug.LogError("failed to create instance of " + assetType.Name + " at " + assetPath);
                return null;
            }
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        /// <summary>Create a ScriptableObject asset</summary>
        /// <typeparam name="T">The type of asset to create</typeparam>
        /// <param name="prependFolderName">If true, prepend the selected asset folder name to the asset name</param>
        /// <param name="trimName">If true, remove instances of the "Asset", "Attributes", "Container" strings from the name</param>
        public static void Create<T>(bool prependFolderName = false, bool trimName = true) where T : ScriptableObject
        {
            string className = typeof(T).Name;
            string assetName = className;
            string folder = GetSelectedAssetFolder();

            if (trimName)
            {
                string[] standardNames = new string[] { "Asset", "Attributes", "Container" };
                foreach (string standardName in standardNames)
                {
                    assetName = assetName.Replace(standardName, "");
                }
            }

            if (prependFolderName)
            {
                string folderName = Path.GetFileName(folder);
                assetName = (string.IsNullOrEmpty(assetName) ? folderName : string.Format("{0}_{1}", folderName, assetName));
            }

            Create(className, assetName, folder);
        }

        private static ScriptableObject Create(string className, string assetName, string folder)
        {
            ScriptableObject asset = ScriptableObject.CreateInstance(className);
            if (asset == null)
            {
                Debug.LogError("failed to create instance of " + className);
                return null;
            }

            asset.name = assetName ?? className;

            string assetPath = GetUnusedAssetPath(folder, asset.name);
            AssetDatabase.CreateAsset(asset, assetPath);

            return asset;
        }

        private static string GetSelectedAssetFolder()
        {
            if ((Selection.activeObject != null) && AssetDatabase.Contains(Selection.activeObject))
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                string assetPathAbsolute = string.Format("{0}/{1}", Path.GetDirectoryName(Application.dataPath), assetPath);

                if (Directory.Exists(assetPathAbsolute))
                {
                    return assetPath;
                }
                else
                {
                    return Path.GetDirectoryName(assetPath);
                }
            }

            return "Assets";
        }

        private static string GetUnusedAssetPath(string folder, string assetName)
        {
            for (int n = 0; n < 9999; n++)
            {
                string assetPath = string.Format("{0}/{1}{2}.asset", folder, assetName, (n == 0 ? "" : n.ToString()));
                string existingGUID = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(existingGUID))
                {
                    return assetPath;
                }
            }

            return null;
        }
    }
}
