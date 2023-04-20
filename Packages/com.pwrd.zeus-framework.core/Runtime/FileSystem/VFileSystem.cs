/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using Zeus.Framework;

namespace Zeus.Core.FileSystem
{
    public static class VFileSystem
    {
#if UNITY_EDITOR
        private static bool _IsPlaying
        {
            get
            {
                return ZeusCore.IsPlaying;
            }
        }
#endif

        /// <summary>
        /// 根据virtualPath取得一个Stream（因为Android的StreamAssets是APK，所以不能返回FileStream）。
        /// virtualPath必须是相对路径
        /// </summary>
        public static Stream OpenFile(string virtualPath, FileMode fileMode, FileAccess fileAccess)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

            // FileMode和FileAccess不匹配。
            if ((fileMode == FileMode.Append || fileMode == FileMode.Create || fileMode == FileMode.CreateNew
                || fileMode == FileMode.Truncate || fileMode == FileMode.OpenOrCreate)
                && (fileAccess == FileAccess.Read))
            {
                throw new ArgumentException("'fileMode' and 'fileAccess' do not cooperate.");
            }

#if UNITY_EDITOR
            if (!_IsPlaying)
            {
                if (File.Exists(_GetAssetsFolderRealPath(virtualPath)) || fileMode == FileMode.Create
                    || fileMode == FileMode.CreateNew || fileMode == FileMode.OpenOrCreate)
                {
                    return File.Open(_GetAssetsFolderRealPath(virtualPath), fileMode, fileAccess, FileShare.Read);
                }
                else
                {
                    return File.Open(virtualPath, fileMode, fileAccess, FileShare.Read);
                }
            }
#endif

            if (fileAccess == FileAccess.Read)
            {
                // 先外后内的读。
                string outerPath = OuterPackage.GetRealPath(virtualPath);
                if (File.Exists(outerPath))
                {
                    return File.Open(outerPath, fileMode, fileAccess, FileShare.Read);
                }
#if UNITY_EDITOR
                else if (File.Exists(virtualPath))
                {
                    return File.Open(virtualPath, fileMode, fileAccess, FileShare.Read);
                }
#endif
                else
                {
                    return InnerPackage.OpenReadStream(virtualPath);
                }
            }
            else
            {
                string outerPath = OuterPackage.GetRealPath(virtualPath);
                FileUtil.EnsureFolder(outerPath);
                return File.Open(outerPath, fileMode, fileAccess, FileShare.Read);
            }
        }

        public static byte[] ReadAllBytes(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

#if UNITY_EDITOR
            if (!_IsPlaying)
            {
                if (File.Exists(_GetAssetsFolderRealPath(virtualPath)))
                {
                    return File.ReadAllBytes(_GetAssetsFolderRealPath(virtualPath));
                }
                else
                {
                    return File.ReadAllBytes(virtualPath);
                }
            }
#endif

            string outerPath = OuterPackage.GetRealPath(virtualPath);
            if (File.Exists(outerPath))
            {
                return File.ReadAllBytes(outerPath);
            }
#if UNITY_EDITOR
            else if (File.Exists(virtualPath))
            {
                return File.ReadAllBytes(virtualPath);
            }
#endif
            else
            {
                return InnerPackage.ReadAllBytes(virtualPath);
            }
        }

        public static string[] ReadAllLines(string virtualPath, Encoding encoding)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

#if UNITY_EDITOR
            if (!_IsPlaying)
            {
                if (File.Exists(_GetAssetsFolderRealPath(virtualPath)))
                {
                    return File.ReadAllLines(_GetAssetsFolderRealPath(virtualPath), encoding);
                }
                else
                {
                    return File.ReadAllLines(virtualPath, encoding);
                }
            }
#endif

            string outerPath = OuterPackage.GetRealPath(virtualPath);
            if (File.Exists(outerPath))
            {
                return File.ReadAllLines(outerPath, encoding);
            }
#if UNITY_EDITOR
            else if (File.Exists(virtualPath))
            {
                return File.ReadAllLines(virtualPath, encoding);
            }
#endif
            else
            {
#if ZEUS_FIRSTCOPY
                if (InnerPackage.ExistsFile(virtualPath))
                {
                    throw new ZeusException(string.Format("Impossible, '{0}' should be copied to outerpackage at the time of start.", virtualPath));
                }
                else
                {// 文件确实没有。
                    throw new FileNotFoundException(string.Format("Can not find file '{0}'", virtualPath), virtualPath);
                }
#else
                using (var stream = InnerPackage.OpenReadStream(virtualPath))
                {
                    List<string> listLine = new List<string>();
                    using (var streamReader = new StreamReader(stream, encoding))
                    {
                        while (streamReader.Peek() > -1)
                        {
                            listLine.Add(streamReader.ReadLine());
                        }
                    }
                    return listLine.ToArray();
                }
#endif
            }
        }

        public static string ReadAllText(string virtualPath, Encoding encoding)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

#if UNITY_EDITOR
            if (!_IsPlaying)
            {
                if (File.Exists(_GetAssetsFolderRealPath(virtualPath)))
                {
                    return File.ReadAllText(_GetAssetsFolderRealPath(virtualPath), encoding);
                }
                else
                {
                    return File.ReadAllText(virtualPath, encoding);
                }
            }
#endif

            string outerPath = OuterPackage.GetRealPath(virtualPath);
            if (File.Exists(outerPath))
            {
                return File.ReadAllText(outerPath, encoding);
            }
#if UNITY_EDITOR
            else if (File.Exists(virtualPath))
            {
                return File.ReadAllText(virtualPath, encoding);
            }
#endif
            else
            {
#if ZEUS_FIRSTCOPY
                if (InnerPackage.ExistsFile(virtualPath))
                {
                    throw new ZeusException(string.Format("Impossible, '{0}' should be copied to outerpackage at the time of start.", virtualPath));
                }
                else
                {// 文件确实没有。
                    throw new FileNotFoundException(string.Format("Can not find file '{0}'", virtualPath), virtualPath);
                }
#else
                using (var stream = InnerPackage.OpenReadStream(virtualPath))
                {
                    using (var streamReader = new StreamReader(stream, encoding))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
#endif
            }
        }

        public static string GetRealPath(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

#if UNITY_EDITOR
            if (!_IsPlaying)
            {
                if (File.Exists(virtualPath))
                {
                    return virtualPath;
                }
                else
                {
                    return GetAssetsFolderRealPath(virtualPath);
                }
            }
#endif

            string outerPath = OuterPackage.GetRealPath(virtualPath);
            //包外不存在且包内存在，返回包内路径，否则返回包外路径。
            if (!File.Exists(outerPath) && InnerPackage.ExistsFile(virtualPath))
            {
                return InnerPackage.GetFullPath(virtualPath);
            }
#if UNITY_EDITOR
            else if (File.Exists(virtualPath))
            {
                return virtualPath;
            }
#endif
            else
            {
                return outerPath;
            }
        }

        public static bool Exists(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

            return ExistsFile(virtualPath) || ExistsDirectory(virtualPath);
        }

        public static bool ExistsFile(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

#if UNITY_EDITOR
            if (!_IsPlaying)
            {
                return File.Exists(_GetAssetsFolderRealPath(virtualPath)) || File.Exists(virtualPath);
            }
#endif

            string outerPath = OuterPackage.GetRealPath(virtualPath);
#if UNITY_EDITOR
            return File.Exists(outerPath) || InnerPackage.ExistsFile(virtualPath) || File.Exists(virtualPath);
#else
            return File.Exists(outerPath) || InnerPackage.ExistsFile(virtualPath);
#endif
        }

        public static bool ExistsDirectory(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

#if UNITY_EDITOR
            if (!_IsPlaying)
            {
                return Directory.Exists(_GetAssetsFolderRealPath(virtualPath)) || Directory.Exists(virtualPath);
            }
#endif

            string outerPath = OuterPackage.GetRealPath(virtualPath);
#if UNITY_EDITOR
            return Directory.Exists(outerPath) || Directory.Exists(virtualPath) || Directory.Exists(virtualPath);
#else
            return Directory.Exists(outerPath) || InnerPackage.ExistsDirectory(virtualPath);
#endif
        }


        /// <summary>
        /// 创建文本文件以及其所在路径
        /// </summary>
        public static void WriteAllText(string virtualPath, string content = "")
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }
            if (!Exists(virtualPath))
            {
                string dictoryPath = "";
                int index = virtualPath.LastIndexOf("/", StringComparison.Ordinal);
                if (index >= 1)
                {
                    dictoryPath = virtualPath.Substring(0, index);
#if UNITY_EDITOR
                    if (!_IsPlaying)
                    {
                        Directory.CreateDirectory(_GetAssetsFolderRealPath(dictoryPath));
                        return;
                    }
#endif
                }
            }
            string realPath = GetRealPath(virtualPath);
            StreamWriter sw = new StreamWriter(realPath);
            sw.Write(content);
            sw.Close();
        }

        /// <summary>
        /// 创建所有文件夹及子文件夹。
        /// </summary>
        public static void CreateDirectory(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

#if UNITY_EDITOR
            if (!_IsPlaying)
            {
                Directory.CreateDirectory(_GetAssetsFolderRealPath(virtualPath));
                return;
            }
#endif

            string outerPath = OuterPackage.GetRealPath(virtualPath);
            Directory.CreateDirectory(outerPath);
        }

        /// <summary>
        /// 创建所有父文件夹及子文件夹。
        /// </summary>
        public static void CreateParentDirectory(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

            int index = virtualPath.LastIndexOf("/", StringComparison.Ordinal);
            if (index >= 1)
            {
                virtualPath = virtualPath.Substring(0, index);
            }
            else
            {
                return;
            }

#if UNITY_EDITOR
            if (!_IsPlaying)
            {
                Directory.CreateDirectory(_GetAssetsFolderRealPath(virtualPath));
                return;
            }
#endif

            string outerPath = OuterPackage.GetRealPath(virtualPath);
            Directory.CreateDirectory(outerPath);
        }

        private static bool _CanWrite(string virtualPath)
        {
            string outerPath = OuterPackage.GetRealPath(virtualPath);
            if (InnerPackage.ExistsFile(virtualPath))
            {
                if (!File.Exists(outerPath))
                {
                    return false;
                }
            }
            if (!File.Exists(outerPath) && !File.Exists(virtualPath))
            {
                return false;
            }
            else
            {
                string realPath = GetRealPath(virtualPath);
                FileAttributes attributes = File.GetAttributes(realPath);
                if ((attributes & FileAttributes.ReadOnly) == attributes)
                {
                    return false;
                }
            }
            return true;
        }

        public static string GetAssetsFolderRealPath(string virtualPath)
        {
#if UNITY_EDITOR
            if (_IsPlaying)
            {
                throw new ZeusException("This method can't run in play mode.");
            }
#endif
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

            return _GetAssetsFolderRealPath(virtualPath);
        }

        public static string GetInnerVirtualPath(string fullPath)
        {
#if UNITY_EDITOR
            if (_IsPlaying)
            {
                throw new ZeusException("This method can't run in play mode.");
            }
#endif
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new ArgumentNullException("realPath");
            }

            string root = PathUtil.FormatPathSeparator(Application.dataPath + "/");

            fullPath = PathUtil.FormatPathSeparator(fullPath);

            int index = fullPath.IndexOf(root);
            if (index == -1)
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid full path!", fullPath));
            }

            return fullPath.Substring(index + root.Length);
        }

        private static string _GetAssetsFolderRealPath(string virtualPath)
        {
            return "Assets/" + virtualPath;
        }

        public static string GetBuildinSettingPath(string fileName)
        {
            return "ZeusSetting/BuildinSetting/" + fileName;
        }

        public static string GetEditorSettingPath(string fileName)
        {
            return "ZeusSetting/EditorSetting/" + fileName;
        }

        public static string GetInApkSettingPath(string fileName)
        {
            return "ZeusSetting/Resources/" + fileName;
        }

        public static string GetZeusSettingPath(string fileName)
        {
            return "ZeusSetting/" + fileName;
        }

        public static string GetRealZeusSettingPath(string fileName)
        {
#if UNITY_EDITOR
            if (!_IsPlaying)
            {
                return _GetAssetsFolderRealPath(GetZeusSettingPath(fileName));
            }
#endif
            return GetRealPath(GetZeusSettingPath(fileName));
        }

        private static string GetFileMoveRecorderPath()
        {
            return OuterPackage.GetRealPath("FileMoveRecorder.bytes");
        }

        private static string[] tempArr = new string[1];
        private static object _lock = new object();
        public static void MoveFileOnNextLanch(string src, string dest)
        {
            lock (_lock)
            {
                string recorderPath = GetFileMoveRecorderPath();
                tempArr[0] = src + "," + dest;
                File.AppendAllLines(recorderPath, tempArr);
            }
        }

        internal static void MoveFile()
        {
            string recordfilePath = GetFileMoveRecorderPath();
            if (File.Exists(recordfilePath))
            {
                string[] lines = File.ReadAllLines(recordfilePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    int index = lines[i].IndexOf(",");
                    if (index <= 0)
                    {
                        continue;
                    }
                    string src = lines[i].Substring(0, index);
                    string dest = lines[i].Substring(index + 1);
                    if (File.Exists(src))
                    {
                        File.Copy(src, dest, true);
                        File.Delete(src);
                    }
                }
                File.Delete(recordfilePath);
            }
        }

        public static bool IsObbBuild
        {
            get
            {
                return InApkSetting.LoadSetting().obbMode != ZeusObbBuild.None;
            }
        }

        /// <summary>
        /// 校验包外文件(不能在Zeus框架的文件系统初始化前调用)
        /// </summary>
        /// <param name="onFinish">校验结束的回调。</param>
        /// <param name="checkType">All：校验全部文件；FirstPackage：只校验首包文件；SubPackage：只校验分包文件</param>
        public static void CheckOuterPackageFile(Action<OuterPackageFileCheckResult> onFinish, OuterPackageFileCheckType checkType = OuterPackageFileCheckType.All)
        {
            OuterPackageFileChecker.CheckFiles(onFinish, checkType);
        }
    }
}
