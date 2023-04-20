/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
using UnityEngine;
using ICSharpCode.ZeusSharpZipLib.Zip;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Zeus.Core.FileSystem
{
    public static class InnerPackage
    {
        private static string _ROOT_BACKSLASH = "Assets/";
        private static bool _isAab;

        public const string _VFileIndex = "_vfileIndexV2.fb";
        public const string _VFileContent = "_vfileContent";

        private static Dictionary<string, VFileEntry> _vFileEntrys;

#if !UNITY_EDITOR
        private static string[] _VFileContentPath;
#if UNITY_ANDROID
        private static string[] _VFileContentEntryName;
        private static ZipFile _ZipFile;
        private static ZipFile _installTimeZipFile;
        private static Dictionary<string, ZipEntry> _zipFileEntryDict;
        /// <summary>
        /// 运行时不一定被初始化，防止报错，提前初始化一下
        /// </summary>
        private static Dictionary<string, ZipEntry> _installTimeZipFileEntryDict = new Dictionary<string, ZipEntry>();
        private static Dictionary<string, ZipEntry> _zipDirectoryEntryDict;
        private static string entryNamePrefix = "assets/";
        private static string alternativeEntryNamePrefix = "assets/";
        private static string _patchOBBPath = null;
        private static string _installTimeApk = null;
#endif
#endif

        internal static void Init()
        {
#if UNITY_EDITOR
            _ROOT_BACKSLASH = "Assets/";
#else
#if UNITY_ANDROID
            var aabMode = InApkSetting.LoadSetting().obbMode;
            _isAab = aabMode == ZeusObbBuild.AAB;
            _ROOT_BACKSLASH = Application.streamingAssetsPath + "/";
            if (Application.dataPath.EndsWith("obb"))
            {
                string fileName = Path.GetFileName(Application.dataPath);
                string obbDirName = Path.GetDirectoryName(Application.dataPath);
                string patchObbPath = Path.Combine(obbDirName, "patch" + fileName.Substring(4));
                if (File.Exists(patchObbPath))
                {
                    _patchOBBPath = patchObbPath;
                }
            }
            var isAabUniversalApk = _isAab && NoInstallTimeApk();
            _zipDirectoryEntryDict = new Dictionary<string, ZipEntry>();
            var installTimePath = GetInstallTimeApk();
            //初始化base包的zipEntry
            AppendZipeFileEntries(ref _ZipFile, ref _zipFileEntryDict, Application.dataPath);
            if(_isAab && !isAabUniversalApk)
            {
                //初始化install_time包的zipEntry
                AppendZipeFileEntries(ref _installTimeZipFile, ref _installTimeZipFileEntryDict, installTimePath);
            }
            //设置各种不同情况的读取路径
            if (_isAab)
            {
                if (isAabUniversalApk)
                {
                    _ROOT_BACKSLASH = Application.streamingAssetsPath + "/";
                }
                else
                {
                    _ROOT_BACKSLASH = Application.dataPath.Replace("base.apk", "split_install_time.apk!assets/");
                }
            }
#else
            _ROOT_BACKSLASH = Application.streamingAssetsPath + "/";
#endif
            if (FileSystemSetting.LoadLocalConfig().isCombineFile)
            {
                LoadVFileEntrysFromFB();
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void AppendZipeFileEntries(ref ZipFile zipFile, ref Dictionary<string, ZipEntry> zipFileEntryDict, string apkPath)
        {
            zipFile = new ZipFile(apkPath);
            if(null == zipFileEntryDict)
            {
                zipFileEntryDict = new Dictionary<string, ZipEntry>((int)zipFile.Count);
            }
            for (int i = 0; i < zipFile.Count; i++)
            {
                ZipEntry entry = zipFile.GetEntryWithoutClone(i);
                if (entry.IsFile)
                {
                    zipFileEntryDict[entry.Name] = entry;
                }
                else
                {
                    Debug.LogError("HasDirectory");
                    _zipDirectoryEntryDict[entry.Name] = entry;
                }
            }
        }

        private static bool NoInstallTimeApk()
        {
            var installTimeApk = GetInstallTimeApk();
            //先到install_time里拿下资源，如果有，说明用的是aab生成的apk，所有的资源都在install_time里，否则在base.apk里
            return !File.Exists(installTimeApk);
        }

        private static string GetInstallTimeApk()
        {
            if(!string.IsNullOrEmpty(_installTimeApk))
            {
                return _installTimeApk;
            }
            //data/app/package name-1/base.apk，没有找到文档能确定这里一定是base.apk，保险起见，这里把最后一个斜杠后面的内容替换掉
            var dataPath = Application.dataPath;
            var lastSlash = dataPath.LastIndexOf("/");
            var apkName = dataPath.Substring(lastSlash + 1, dataPath.Length - lastSlash - 1);
            _installTimeApk = Application.dataPath.Replace(apkName, "split_install_time.apk");
            return _installTimeApk;
        }

#endif

#if !UNITY_EDITOR
        private static void LoadVFileEntrysFromFB()
        {
            byte[] bytes = ReadAllBytes(_VFileIndex);
            ZeusFlatBuffers.ByteBuffer buffer = new ZeusFlatBuffers.ByteBuffer(bytes);
            VFileContentFB contentFB = VFileContentFB.GetRootAsVFileContentFB(buffer);

            int vfileContentCount = contentFB.EntrysLength;
#if UNITY_ANDROID
            _VFileContentEntryName = new string[vfileContentCount];
#endif
            _VFileContentPath = new string[vfileContentCount];
            _vFileEntrys = new Dictionary<string, VFileEntry>(contentFB.FileEntryCount);
            for (int i = 0; i < vfileContentCount; ++i)
            {
                VFileContentEntryFB indexEntry = contentFB.Entrys(i).Value;
                string contentFileName = indexEntry.ContentFile;
#if UNITY_ANDROID
                _VFileContentEntryName[i] = BuildEntryName(contentFileName);
#endif
                _VFileContentPath[i] = _ROOT_BACKSLASH + contentFileName;
                for (int j = 0; j < indexEntry.EntrysLength; ++j)
                {
                    VFileEntryFB entryFB = indexEntry.Entrys(j).Value;
                    VFileEntry entry = new VFileEntry(i, entryFB.Offset, entryFB.Length);
                    _vFileEntrys.Add(entryFB.Path, entry);
                }
            }
        }
#endif

        /// <summary>
        /// 获取内部全路径
        /// </summary>
        public static string GetFullPath(string virtualPath)
        {
            return _ROOT_BACKSLASH + virtualPath;
        }

        /// <summary>
        /// 获取读取流，因为Android APK内部的文件只能通过流读取（没有路径）。
        /// </summary>
        public static Stream OpenReadStream(string virtualPath)
        {
#if UNITY_EDITOR
            return File.Open(_ROOT_BACKSLASH + virtualPath, FileMode.Open, FileAccess.Read, FileShare.Read);
#else
#if UNITY_ANDROID
            // Android需要从APK中解压读取。
            ZipEntry entry = null;
            VFileEntry vfileEntry = new VFileEntry();

            if (TryGetValueFromVFileContent(virtualPath, out vfileEntry))
            {
                if(!string.IsNullOrEmpty(_patchOBBPath) && vfileEntry.VfileContentIndex == 0)
                {
                    Stream stream = File.Open(_patchOBBPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    return new SubStream(stream, (long)vfileEntry.Offset, (long)vfileEntry.Length);
                }
                else if (_zipFileEntryDict.TryGetValue(_VFileContentEntryName[vfileEntry.VfileContentIndex], out entry))
                {
                    return new SubStream(_ZipFile.GetInputStream(entry), (long)vfileEntry.Offset, (long)vfileEntry.Length);
                }
                else if(_installTimeZipFileEntryDict.TryGetValue(_VFileContentEntryName[vfileEntry.VfileContentIndex], out entry))
                {
                    return new SubStream(_installTimeZipFile.GetInputStream(entry), (long)vfileEntry.Offset, (long)vfileEntry.Length);
                }
                else
                {
                    Debug.LogError($"Can't find file \"{_VFileContentEntryName[vfileEntry.VfileContentIndex]}\" in archive.");
                    return null;
                }
            }
            else if (_zipFileEntryDict.TryGetValue(BuildEntryName(virtualPath), out entry))
            {
                return _ZipFile.GetInputStream(entry);
            }
            else if (_installTimeZipFileEntryDict.TryGetValue(BuildEntryName(virtualPath), out entry))
            {
                return _installTimeZipFile.GetInputStream(entry);
            }

            else
            {
                Debug.LogError($"Can't find file \"{virtualPath}\" in archive or vfileContent.");
                return null;
            }
#else
            VFileEntry vfileEntry = new VFileEntry();
            if(TryGetValueFromVFileContent(virtualPath, out vfileEntry))
            {
                if (File.Exists(_VFileContentPath[vfileEntry.VfileContentIndex]))
                {
                    Stream stream = File.Open(_VFileContentPath[vfileEntry.VfileContentIndex], FileMode.Open, FileAccess.Read, FileShare.Read);
                    return new SubStream(stream, (long)vfileEntry.Offset, (long)vfileEntry.Length);
                }
                else
                {
                    return null;
                }
            }
            else if(File.Exists(_ROOT_BACKSLASH + virtualPath))
            {
                return File.Open(_ROOT_BACKSLASH + virtualPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                Debug.LogError($"Can't find file \"{virtualPath}\".");
                return null;
            }
#endif
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static string BuildEntryName(string virtualPath)
        {
            return entryNamePrefix + virtualPath;
        }
#endif

        public static bool ExistsFile(string virtualPath)
        {
#if UNITY_EDITOR
            return File.Exists(_ROOT_BACKSLASH + virtualPath);
#else
#if UNITY_ANDROID
            // Android需要从APK中解压读取。
            return ContainsInVFileContent(virtualPath) 
                || _zipFileEntryDict.ContainsKey(BuildEntryName(virtualPath)) 
                || _installTimeZipFileEntryDict.ContainsKey(BuildEntryName(virtualPath));
#else
            return (ContainsInVFileContent(virtualPath)) || File.Exists(_ROOT_BACKSLASH + virtualPath);
#endif
#endif
        }

        internal static bool ExistsDirectory(string virtualPath)
        {
#if UNITY_EDITOR
            return Directory.Exists(_ROOT_BACKSLASH + virtualPath);
#else
#if UNITY_ANDROID
            // Android需要从APK中解压读取。
            return (ContainsInVFileContent(virtualPath)) || _zipDirectoryEntryDict.ContainsKey(BuildEntryName(virtualPath));
#else
            return (ContainsInVFileContent(virtualPath)) || Directory.Exists(_ROOT_BACKSLASH + virtualPath);
#endif
#endif
        }

        public static byte[] ReadAllBytes(string virtualPath)
        {
#if UNITY_EDITOR
            return File.ReadAllBytes(_ROOT_BACKSLASH + virtualPath);
#else
            VFileEntry vfileEntry = new VFileEntry();
            if(_vFileEntrys != null &&_vFileEntrys.TryGetValue(virtualPath, out vfileEntry))
            {
#if UNITY_ANDROID
                // Android需要从APK中解压读取。
                ZipEntry entry = null;
                if(!string.IsNullOrEmpty(_patchOBBPath) && vfileEntry.VfileContentIndex == 0)
                {
                    using (var stream = File.Open(_patchOBBPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        byte[] buff = new byte[vfileEntry.Length];
                        StreamUtils.ReadBytes(stream, buff, vfileEntry.Offset, buff.Length);
                        return buff;
                    }
                }
                else if (_zipFileEntryDict.TryGetValue(_VFileContentEntryName[vfileEntry.VfileContentIndex], out entry))
                {
                    byte[] buff = new byte[vfileEntry.Length];
                    Stream stream =  _ZipFile.GetInputStream(entry);
                    StreamUtils.ReadBytes(stream, buff, vfileEntry.Offset, buff.Length);
                    return buff;
                }
                else if (_installTimeZipFileEntryDict.TryGetValue(_VFileContentEntryName[vfileEntry.VfileContentIndex], out entry))
                {
                    byte[] buff = new byte[vfileEntry.Length];
                    Stream stream =  _installTimeZipFile.GetInputStream(entry);
                    StreamUtils.ReadBytes(stream, buff, vfileEntry.Offset, buff.Length);
                    return buff;
                }
                else
                {
                    Debug.LogError($"Can't find file \"{_VFileContentEntryName[vfileEntry.VfileContentIndex]}\" in archive.");
                    return null;
                }
#else
                using (var stream = File.Open(_VFileContentPath[vfileEntry.VfileContentIndex], FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] buff = new byte[vfileEntry.Length];
                    StreamUtils.ReadBytes(stream, buff, vfileEntry.Offset, buff.Length);
                    return buff;
                }
#endif
            }
            else
            {
#if UNITY_ANDROID
                // Android需要从APK中解压读取。
                ZipEntry entry = null;
                if (_zipFileEntryDict.TryGetValue(BuildEntryName(virtualPath), out entry))
                {
                    Stream stream =  _ZipFile.GetInputStream(entry);
                    return StreamUtils.ReadAllBytes(stream);
                }
                else if (_installTimeZipFileEntryDict.TryGetValue(BuildEntryName(virtualPath), out entry))
                {
                    Stream stream =  _installTimeZipFile.GetInputStream(entry);
                    return StreamUtils.ReadAllBytes(stream);
                }
                else
                {
                    Debug.LogError($"Can't find file \"{virtualPath}\" in archive or vfileContent.");
                    return null;
                }
#else
                using (var stream = File.Open(_ROOT_BACKSLASH + virtualPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return StreamUtils.ReadAllBytes(stream);
                }
#endif
            }
#endif
        }


        public static bool TryGetFileEntry(string bundlePath, out string filePath, out ulong offset)
        {
#if !UNITY_EDITOR
            VFileEntry entry;
            if (bundlePath.StartsWith(_ROOT_BACKSLASH))
            {
                string relativeBundlePath = bundlePath.Replace(_ROOT_BACKSLASH, "");
                if (TryGetValueFromVFileContent(relativeBundlePath, out entry))
                {
#if UNITY_ANDROID
                    if(!string.IsNullOrEmpty(_patchOBBPath) && entry.VfileContentIndex == 0)
                    {
                        filePath = _patchOBBPath;
                        offset = entry.Offset;
                        return true;
                    }
#endif
                    filePath = _VFileContentPath[entry.VfileContentIndex];
                    offset = entry.Offset;
                    return true;
                }
            }
#endif
            filePath = bundlePath;
            offset = 0;
            return false;
        }

#if !UNITY_EDITOR
        private static bool ContainsInVFileContent(string virtualPath)
        {
            if (_vFileEntrys == null)
            {
                return false;
            }
            else
            {
                return _vFileEntrys.ContainsKey(virtualPath);
            }
        }

        private static bool TryGetValueFromVFileContent(string virtualPath, out VFileEntry entry)
        {
            if (_vFileEntrys == null)
            {
                entry = new VFileEntry();
                return false;
            }
            else
            {
                return _vFileEntrys.TryGetValue(virtualPath, out entry);
            }
        }
#endif

        public static bool IsInnerPath(string path)
        {
            return path.StartsWith(_ROOT_BACKSLASH);
        }
    }
}