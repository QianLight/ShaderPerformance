/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using FileUtil = Zeus.Core.FileUtil;

namespace Zeus.Framework
{
    public static class PackageUtility
    {
#if UNITY_EDITOR
        /// <summary>
        /// 将插件包下的所有.json配置文件拷贝到 Asset/ZeusSetting/BuildinSetting/包名 下，返回是否拷贝成功
        /// </summary>
        /// <param name="packageName">插件包名称</param>
        /// <returns>是否拷贝成功</returns>
        public static bool CopySettingFileToBuildinSetting(string packageName)
        {
            return CopyPackageFiles(packageName, "Runtime/Setting", "*.json", "Assets/ZeusSetting/BuildinSetting/" + packageName);
        }

        /// <summary>
        /// 将插件包下的所有.json配置文件拷贝到 Asset/ZeusSetting/EditorSetting/包名 下，返回是否拷贝成功
        /// </summary>
        /// <param name="packageName">插件包名称</param>
        /// <returns>是否拷贝成功</returns>
        public static bool CopySettingFileToEditorSetting(string packageName)
        {
            return CopyPackageFiles(packageName, "Runtime/Setting", "*.json", "Assets/ZeusSetting/EditorSetting/" + packageName);
        }

        /// <summary>
        /// 将插件包下的所有.json配置文件拷贝到 Asset/ZeusSetting 下，返回是否拷贝成功
        /// </summary>
        /// <param name="packageName">插件包名称</param>
        /// <returns>是否拷贝成功</returns>
        public static bool CopySettingFile(string packageName)
        {
            return CopyPackageFiles(packageName, "Runtime/Setting", "*.json", "Assets/ZeusSetting/");
        }

        /// <summary>
        /// 从名字为packageName的插件包拷贝所有符合条件的文件到目标路径
        /// </summary>
        /// <param name="packageName">插件包名称</param>
        /// <param name="subPath">子目录</param>
        /// <param name="sourceFilePattern">源文件格式</param>
        /// <param name="destDirectory">目标路径</param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static bool CopyPackageFiles(string packageName, string subPath, string sourceFilePattern, string destDirectory, SearchOption searchOption = SearchOption.AllDirectories)
        {
            string pkgRelativePath = GetPackageRelativePath(packageName);
            if (string.IsNullOrEmpty(pkgRelativePath))
            {
                Debug.LogError($"Can't get relative path of package \"{packageName}\".");
                return false;
            }
            pkgRelativePath = PathUtil.FormatPathSeparator(pkgRelativePath);
            subPath = PathUtil.FormatPathSeparator(subPath);
            if (pkgRelativePath == null)
            {
                Debug.LogError(string.Format("Copy file failed! Can't get path of package \"{0}\"", packageName));
                return false;
            }
            else
            {
                string[] directorys = Directory.GetDirectories(pkgRelativePath, subPath);
                if (directorys.Length < 1)
                {
                    Debug.LogError(string.Format("Copy file failed! Can't get subPath \"{0}\" of package \"{1}\"", subPath, packageName));
                    return false;
                }
                int fileCount = 0;
                foreach (string dir in directorys)
                {
                    string directory = dir;
                    string[] files = Directory.GetFiles(directory, sourceFilePattern, searchOption);
                    fileCount += files.Length;
                    foreach (string f in files)
                    {
                        if(f.EndsWith(".meta"))
                        {
                            continue;
                        }
                        string fileRelativePath = f.Replace(directory, "").Substring(1);
                        string destPath = Path.Combine(destDirectory, fileRelativePath);
                        if (File.Exists(destPath))
                        {
                            Debug.LogError(string.Format("{0} already exits!", destPath));
                            continue;
                        }
                        FileUtil.EnsureFolder(destPath);
                        File.Copy(f, destPath);
                    }
                }
                if (fileCount < 1)
                {
                    Debug.LogError("No matching file.");
                    return false;
                }
            }
            AssetDatabase.Refresh();
            return true;
        }

        /// <summary>
        /// 从名字为packageName的插件包拷贝文件到目标路径
        /// </summary>
        /// <param name="packageName">插件包名称</param>
        /// <param name="sourceFile">源文件</param>
        /// <param name="destFile">目标文件</param>
        /// <returns></returns>
        public static bool CopyPackageFile(string packageName, string sourceFile, string destFile)
        {
            if (File.Exists(destFile))
            {
                Debug.LogError(string.Format("{0} already exits!", destFile));
            }
            string pkgRelativePath = GetPackageRelativePath(packageName);
            if (pkgRelativePath == null)
            {
                Debug.LogError(string.Format("Copy file failed! Can't get path of package \"{0}\"", packageName));
                return false;
            }
            else
            {
                string[] files = Directory.GetFiles(pkgRelativePath, sourceFile, SearchOption.AllDirectories);
                if (files.Length < 1)
                {
                    Debug.LogError("No matching file.");
                    return false;
                }
                else if (files.Length > 1)
                {
                    Debug.LogError("Matching multiple files.");
                    return false;
                }
                FileUtil.EnsureFolder(destFile);
                File.Copy(files[0], destFile);
            }
            AssetDatabase.Refresh();
            return true;
        }

        public static string GetPackageRelativePath(string packageName)
        {
            string projectPath = Directory.GetCurrentDirectory().Replace("\\", "/");
            string relativePath = GetPackageFullPath(packageName).Replace(projectPath, "").Substring(1);
            return relativePath;
        }

        public static string GetPackageFullPath(string packageName)
        {
            string packageRoot = Path.Combine("Packages", packageName);
            // Check for potential UPM package
            string fullPath = Path.GetFullPath(packageRoot);
            if (Directory.Exists(fullPath))
            {
                return fullPath.Replace("\\", "/");
            }

            fullPath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(fullPath))
            {
                // Search default location for development package
                fullPath = Path.Combine(fullPath, "Assets/Packages", packageName);
                if (Directory.Exists(fullPath))
                {
                    return fullPath.Replace("\\", "/");
                }

                // Search for potential alternative locations in the user project
                string[] matchingPaths = Directory.GetDirectories(fullPath, packageName, SearchOption.AllDirectories);
                if (matchingPaths.Length > 0) return matchingPaths[0].Replace("\\", "/");
            }
            Debug.LogError("Get full path failed!");
            return null;
        }
#endif
    }
}


