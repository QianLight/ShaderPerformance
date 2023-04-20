/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using UnityEngine;
using System.Collections.Generic;


namespace Zeus.Framework
{
    public class PathUtil
    {
        private static string s_DataPath = null;
        private static char[] s_DirectorySeparatorChars;

        /// <summary>
        /// 将路径的分隔符替换为系统默认分隔符
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FormatPathSeparator(string path)
        {
            if (Path.DirectorySeparatorChar.Equals('/'))
            {
                path = path.Replace('\\', Path.DirectorySeparatorChar);
            }
            else
            {
                path = path.Replace('/', Path.DirectorySeparatorChar);
            }
            return path;
        }

        public static string DataPath
        {
            get
            {
                if (s_DataPath != null)
                    return s_DataPath;

                string game = AppConfig.AppName;
                if (Application.isMobilePlatform)
                {
                    s_DataPath = Application.dataPath + "/" + game + "/";
                    return s_DataPath;
                }

                if (AppConfig.IsDebugMode)
                {
                    s_DataPath = Application.dataPath + "/" + AppConfig.AssetDir + "/";
                    return s_DataPath;
                }

                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    int i = Application.dataPath.LastIndexOf("/");
                    s_DataPath = Application.dataPath.Substring(0, i + 1) + game + "/";
                    return s_DataPath;
                }

                return s_DataPath;
            }
        }

        public static string ContentPath()
        {
            string path = string.Empty;
            if (Application.platform == RuntimePlatform.Android)
            {
                path = "jar:file://" + Application.dataPath + "!/assets/";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                path = Application.dataPath + "/Raw/";
            }
            else
            {
                path = Application.dataPath + "/" + AppConfig.AssetDir + "/";
            }

            return path;
        }

        public static string GetRelativePath()
        {
            if (Application.isEditor)
            {
                return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/" + "/";
            }
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
            {
                return "file:///" + DataPath;
            }
            else
            {
                return "file://" + Application.streamingAssetsPath + "/";
            }
        }

        /// <summary>
        /// 获取相对于 Environment.CurrentDirectory 的相对路径
        /// <para>注：该函数会执行 GetCurrentPlatformPath(string) 将path中的 '/' 或 '\' 转换为符合当前平台的目录分隔符</para>
        /// </summary>
        public static string GetRelativePath(string path)
        {
            return GetRelativePath(System.Environment.CurrentDirectory, path);
        }

        /// <summary>
        /// 获取相对于 workDirectory 的相对路径
        /// <para>注：该函数会执行 GetCurrentPlatformPath(string) 将path中的 '/' 或 '\' 转换为符合当前平台的目录分隔符</para>
        /// </summary>
        public static string GetRelativePath(string workDirectory, string path)
        {
            workDirectory = FormatPathSeparator(workDirectory);
            path = FormatPathSeparator(path);
            if (!string.IsNullOrEmpty(workDirectory) && path.ToLower().StartsWith(workDirectory.ToLower()))
            {
                int offset = (workDirectory.EndsWith("/") || workDirectory.EndsWith("\\")) ? 0 : 1;
                path = path.Substring(workDirectory.Length + offset);
            }
            return path;
        }

        /// <summary>
        /// 获取文件后缀名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathExt(string path)
        {
            int index = path.LastIndexOf('.');
            return path.Substring(index + 1);
        }

        /// <summary>
        /// 判断childPath是否为parentPath的子路径
        /// </summary>
        public static bool IsChildPath(string childPath, string parentPath)
        {
            if (childPath == parentPath || string.IsNullOrEmpty(childPath) || string.IsNullOrEmpty(parentPath))
            {
                return false;
            }

            return childPath.StartsWith(parentPath) && childPath[parentPath.Length] == '/';
        }

        /// <summary>
        /// 注意：path的目录不能以"/"结尾
        /// </summary>
        public static string[] GetChildPaths(IEnumerable<string> targetPaths)
        {
            List<string> listChildPath = new List<string>();
            foreach (string path1 in targetPaths)
            {
                if (!string.IsNullOrEmpty(path1))
                {
                    foreach (string path2 in targetPaths)
                    {// 找爸爸
                        if (IsChildPath(path1, path2))
                        {// path1是path2的孩子
                            listChildPath.Add(path1);
                        }
                    }
                }
            }

            return listChildPath.ToArray();
        }

        public static string GetPathParentDirectory(string path)
        {
            int index = path.LastIndexOf('/');
            if (index == -1 || index == path.Length - 1)
            {
                return null;
            }

            return path.Substring(0, index);
        }


        public static void CreateDirectoryRecu(string fullPath)
        {

            //bool bexistfile = false;
            if (File.Exists(fullPath))
            {
                //bexistfile = true;
            }
            else //判断路径中的文件夹是否存在
            {
                string dirpath = fullPath.Substring(0, fullPath.LastIndexOf('/'));
                string[] pathes = dirpath.Split('/');
                if (pathes.Length > 1)
                {
                    string path = pathes[0];
                    for (int i = 1; i < pathes.Length; i++)
                    {
                        path += "\\" + pathes[i];
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取子目录集合，并去重
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string[] GetChildDirectories(IEnumerable<string> paths, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            List<string> result = new List<string>();
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    result.AddRange(Directory.GetDirectories(FormatPathSeparator(path), "*", searchOption));
                }
            }
            Zeus.ZeusUtil.StripRepeat(result);
            return result.ToArray();
        }

        /// <summary>
        /// 获取有效的目录集合，并移除不存在的目录或嵌套的子目录
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string[] GetDirectories(IEnumerable<string> paths)
        {
            List<string> result = new List<string>();
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    result.Add(FormatPathSeparator(path));
                }
            }
            if (result.Count > 1)
            {
                var tempPaths = result.ToArray();
                for (int i = 0; i < tempPaths.Length; i++)
                {
                    string[] dirs = Directory.GetDirectories(tempPaths[i], "*", SearchOption.AllDirectories);
                    foreach (var dir in dirs)
                    {
                        if (result.Contains(dir))
                            result.Remove(dir);
                        if (result.Count <= 1)
                            break;
                    }
                }
                Zeus.ZeusUtil.StripRepeat(result);
            }
            return result.ToArray();
        }

        /// <summary>
        /// 获取所有文件，并去重
        /// </summary>
        public static string[] GetAllFiles(IEnumerable<string> paths, string searchPattern = "*")
        {
            List<string> result = new List<string>();
            foreach (var path in GetDirectories(paths))
            {
                result.AddRange(Directory.GetFiles(FormatPathSeparator(path), searchPattern, SearchOption.AllDirectories));
            }
            return result.ToArray();
        }

        /// <summary>
        /// 合并路径，如果
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string CombinePath(params string[] args)
        {
            string result = Path.Combine(args);
            return FormatPathSeparator(result);
        }
    }
}
