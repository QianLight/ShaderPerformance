/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Zeus.Framework;
using Zeus.Build;
using Zeus.Core;
using FileUtil = Zeus.Core.FileUtil;
using System;
using System.Xml;
using Zeus.Core.FileSystem;
using ZeusFlatBuffers;
using System.IO.Compression;

namespace Zeus.Core
{
    public class GameBuildProcessor : IModifyPlayerSettings, IBeforeBuild, IInternalBeforeBuild, IFinallyBuild
    {
        private const string TempIgnoreFolderDir = "_StreamingAssets";
        //只支持一级目录，多级目录如"Zeus/Lua"不支持
        private static string[] ObbExcludeDirectory = { "Cri", "Inner" };

        public static bool carryAssetBundle;
        public static string version;
        private static bool _isCombineFile;

        public static Dictionary<string, string> DictionaryIncludeBuildPath
        {
            get
            {
                return _dictionaryIncludeBuildPath;
            }
        }

        public static Dictionary<string, bool> DictionaryExcludeBuildPath
        {
            get
            {
                return _dictionaryExcludeBuildPath;
            }
        }
        /// <summary>
        /// key:文件夹 value：文件夹相对StreamAssets的路径
        /// </summary>
        private static Dictionary<string, string> _dictionaryIncludeBuildPath = new Dictionary<string, string>();
        /// <summary>
        /// key:文件  value：
        /// </summary>
        private static Dictionary<string, bool> _dictionaryExcludeBuildPath = new Dictionary<string, bool>();
        /// <summary>
        /// key:文件  value：文件相对StreamAssets的路径
        /// </summary>
        private static Dictionary<string, string> _dictionaryIncludeBuildFileDict = new Dictionary<string, string>();

        private static void SaveDictionary(Dictionary<string, bool> dict, string path)
        {
            string text = string.Empty;
            foreach (KeyValuePair<string, bool> kv in dict)
            {
                text += kv.Key + "\t" + kv.Value.ToString() + "\n";
            }
            File.WriteAllText(path, text);
        }

        private static Dictionary<string, bool> ReadDictionary(string path)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    string[] args = line.Split('\t');
                    result[args[0]] = bool.Parse(args[1]);
                }
            }
            File.Delete(path);
            return result;
        }

        private static void CollectBuildPathInfo()
        {
            Clear();
            // 添加用户定义包含目录
            var vfileSetting = ZeusSetting.GetInstance();
            for (int i = 0; i < vfileSetting.includeBuildSourcePaths.Count; i++)
            {
                string path = vfileSetting.includeBuildSourcePaths[i];
                if (!string.IsNullOrEmpty(path))
                {
                    string target = vfileSetting.includeBuildTargetPaths[i];
                    if (string.IsNullOrEmpty(target))
                    {
                        target = path;
                    }
                    AddIncludeBuildPath(Path.Combine(Application.dataPath, path), target);
                }
                else
                {
                    Debug.LogWarning("includeBuildPaths is Empty.Index is" + i);
                }
            }

            // 添加用户定义剔除目录
            for (int i = 0; i < vfileSetting.excludeBuildSourcePaths.Count; i++)
            {
                string path = vfileSetting.excludeBuildSourcePaths[i];
                if (string.IsNullOrEmpty(path))
                {
                    AddExcludeBuildPath(Path.Combine(Application.dataPath, path));
                }
                else
                {
                    Debug.LogWarning("excludeBuildPaths is Empty.Index is" + i);
                }
            }
        }

        public void OnModifyPlayerSettings(BuildTarget buildTarget)
        {
            CollectBuildPathInfo();
            ModifyInApkSetting();
            ModifyFileSystemSetting();
        }

        private void ModifyInApkSetting()
        {
            InApkSetting setting = InApkSetting.LoadSetting();
            setting.obbMode = GlobalBuild.Default.AndroidBuildObb;
            string obbBuildStr = setting.obbMode.ToString();
            if (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.ANDROID_BUILD_OBB, ref obbBuildStr))
            {
                try
                {
                    setting.obbMode = (ZeusObbBuild)Enum.Parse(typeof(ZeusObbBuild), obbBuildStr);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError("The Obb argument is " + obbBuildStr);
                    throw e;
                }
            }
            bool isAab = false;
            if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.EXPORT_AAB, ref isAab))
            {
                try
                {
                    setting.obbMode = ZeusObbBuild.AAB;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError("The Obb argument is " + obbBuildStr);
                    throw e;
                }
            }

            InApkSetting.SaveSetting(setting);
        }

        private void ModifyFileSystemSetting()
        {
            FileSystemSettingConfig setting = FileSystemSetting.LoadLocalConfig();
            _isCombineFile = setting.isCombineFile;
            if(CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_COMBINE_FILE, ref _isCombineFile))
            {
                setting.isCombineFile = _isCombineFile;
            }
            FileSystemSetting.SaveLocalConfig(setting);
        }

        public void OnBeforeBuild(BuildTarget target, string outputPath)
        {
            _ProcessBuildPath();
        }

        public void OnInternalBeforeBuild(BuildTarget target, string outputPath)
        {
            ProcessIgnoreFolder();
            bool isPackagePatch = false;
            CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_PACKAGE_PATCH, ref isPackagePatch);
            if (_isCombineFile && !isPackagePatch)
            {
                CombineFile(outputPath);
            }

            if (UseZeusBuildObb())
            {
                OBBBeforeBuild(outputPath);
            }
        }

        private void ProcessIgnoreFolder()
        {
            if(Directory.Exists(TempIgnoreFolderDir))
            {
                Directory.Delete(TempIgnoreFolderDir, true);
            }
            foreach (string dir in Directory.GetDirectories(Application.streamingAssetsPath, "*", SearchOption.AllDirectories))
            {
                if(Directory.Exists(dir))
                {
                    if (Path.GetFileName(dir).StartsWith("."))
                    {
                        string targetPath = Path.Combine(TempIgnoreFolderDir, dir.Replace(Application.streamingAssetsPath, "").Substring(1));
                        Zeus.Core.FileUtil.MoveDirectory(dir, Path.GetDirectoryName(targetPath), true, false);
                    }
                }
            }
        }

        private void CheckContradictionDirectory(HashSet<string> needCombinedSet)
        {
            foreach(var dir in ObbExcludeDirectory)
            {
                if(needCombinedSet.Contains(dir))
                {
                    Debug.LogWarning($"The directory \"{dir}\" need to remain in APK, can't be combined into VFileContent.");
                    needCombinedSet.Remove(dir);
                }
            }
        }

        #region CombineFile

        private static string[] NeedCombinedDir = { "LuaProject", "Bundles", "Zeus", "Tolua" };
        private const string TempStreamingAssetsDirectoryName = "TempStreamingAssets";
        private const long VfileContentMaxSize = 1L * ZeusConstant.GB + 1014 * ZeusConstant.MB; //1.99GB
        private void CombineFile(string outputPath)
        {
            string streamingPath = Application.streamingAssetsPath;
            DeleteAllVfilContent();
            string vfileContentPath = Path.Combine(streamingPath, InnerPackage._VFileContent);
            string tempPath = Path.Combine(outputPath, TempStreamingAssetsDirectoryName);
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            HashSet<string> DirectorySet = new HashSet<string>(NeedCombinedDir);
            //检查打OBB时是否有需要留在包里，不能合并的文件
            if (UseZeusBuildObb())
            {
                CheckContradictionDirectory(DirectorySet);
            }

            List<Dictionary<string, VFileEntry>> vfileEntryDics = new List<Dictionary<string, VFileEntry>>();
            byte[] buffer = new byte[1024];
            ulong fileOffset = 0;
            FileStream fsWrite = null;
            int contentIndex = 0;
            try
            {
                fsWrite = File.Open(vfileContentPath + contentIndex.ToString(), FileMode.Create, FileAccess.Write);
                vfileEntryDics.Add(new Dictionary<string, VFileEntry>());
                foreach (string dir in Directory.GetDirectories(streamingPath))
                {
                    string dirName = Path.GetFileName(dir);
                    if (!DirectorySet.Contains(dirName))
                    {
                        continue;
                    }
                    foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                    {
                        if (Path.GetExtension(file) == ".meta")
                        {
                            continue;
                        }
                        FileInfo fileInfo = new FileInfo(file);
                        if(fileInfo.Length > VfileContentMaxSize)
                        {
                            continue;
                        }
                        if (fileOffset + (ulong)fileInfo.Length > VfileContentMaxSize)
                        {
                            fsWrite.Close();
                            fileOffset = 0;
                            fsWrite = File.Open(vfileContentPath + (++contentIndex).ToString(), FileMode.Create, FileAccess.Write);
                            vfileEntryDics.Add(new Dictionary<string, VFileEntry>());
                        }
                        using (FileStream fsRead = fileInfo.OpenRead())
                        {
                            long numToRead = fileInfo.Length;
                            while (numToRead > 0)
                            {
                                int readCount = fsRead.Read(buffer, 0, buffer.Length);
                                if (readCount == 0)
                                {
                                    break;
                                }
                                fsWrite.Write(buffer, 0, readCount);
                                numToRead -= readCount;
                            }
                        }
                        string fileRelativePath = file.Replace(streamingPath, "").Replace("\\", "/").Substring(1);
                        FileUtil.MoveFile(file, Path.Combine(tempPath, fileRelativePath), true);
                        vfileEntryDics[contentIndex].Add(fileRelativePath, new VFileEntry(contentIndex, fileOffset, (uint)fileInfo.Length));
                        fileOffset += (ulong)fileInfo.Length;
                    }
                    FileUtil.DeleteEmptyDirectory(dir);
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                fsWrite.Close();
                DeleteAllVfilContent();
                throw (e);
            }
            fsWrite.Close();
            if (fileOffset == 0)
            {
                File.Delete(vfileContentPath + contentIndex.ToString());
            }
            BuildFlatBuffersFile(vfileEntryDics);
        }

        private void DeleteAllVfilContent()
        {
            foreach (string file in Directory.GetFiles(Application.streamingAssetsPath))
            {
                if (Path.GetFileName(file).Contains(InnerPackage._VFileContent))
                {
                    File.Delete(file);
                }
            }
        }

        private void BuildFlatBuffersFile(List<Dictionary<string, VFileEntry>> vfileEntryDics)
        {
            int contentCount = vfileEntryDics.Count;
            var builder = new FlatBufferBuilder(1);
            var vfileIndexOffsets = new Offset<VFileContentEntryFB>[contentCount];
            int entrysCount = 0;
            for (int i = 0; i < contentCount; ++i)
            {
                entrysCount += vfileEntryDics[i].Count;
                StringOffset contentName = builder.CreateString(InnerPackage._VFileContent + i.ToString());
                var vfileEntryOffsets = new Offset<VFileEntryFB>[vfileEntryDics[i].Count];
                int j = 0;
                foreach (var pair in vfileEntryDics[i])
                {
                    var entry = pair.Value;
                    StringOffset path = builder.CreateString(pair.Key);
                    vfileEntryOffsets[j++] = VFileEntryFB.CreateVFileEntryFB(builder, path, entry.Offset, entry.Length);
                }
                VectorOffset vfileEntryVec = VFileContentEntryFB.CreateEntrysVector(builder, vfileEntryOffsets);
                vfileIndexOffsets[i] = VFileContentEntryFB.CreateVFileContentEntryFB(builder, contentName, vfileEntryVec);
            }
            VectorOffset indexOffset = VFileContentFB.CreateEntrysVector(builder, vfileIndexOffsets);
            VFileContentFB.StartVFileContentFB(builder);
            VFileContentFB.AddFileEntryCount(builder, entrysCount);
            VFileContentFB.AddEntrys(builder, indexOffset);
            var endOffset = VFileContentFB.EndVFileContentFB(builder);
            VFileContentFB.FinishVFileContentFBBuffer(builder, endOffset);
            File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, InnerPackage._VFileIndex), builder.SizedByteArray());
        }

        #endregion


        #region BuildOBB

        private bool UseZeusBuildObb()
        {
            ZeusObbBuild obbBuild = GlobalBuild.Default.AndroidBuildObb;
            string obbBuildStr = string.Empty;
            if (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.ANDROID_BUILD_OBB, ref obbBuildStr))
            {
                try
                {
                    obbBuild = (ZeusObbBuild)Enum.Parse(typeof(ZeusObbBuild), obbBuildStr);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError("The Obb argument is " + obbBuildStr);
                    throw e;
                }
            }
            if (obbBuild == ZeusObbBuild.Zeus)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OBBBeforeBuild(string outputPath)
        {
            if (Directory.Exists(Path.Combine(outputPath, "_TempOBB")))
            {
                Directory.Delete(Path.Combine(outputPath, "_TempOBB"), true);
            }
            string targetDir = Path.Combine(outputPath, "_TempOBB/assets");
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
            }
            Directory.CreateDirectory(targetDir);
            MoveFilesOBB(targetDir);
        }

        private void MoveFilesOBB(string desDir)
        {
            string srcDir = Application.streamingAssetsPath;
            HashSet<string> excludeDirSet = new HashSet<string>(ObbExcludeDirectory);
            foreach (string dir in Directory.GetDirectories(srcDir))
            {
                string dirName = Path.GetFileName(dir);
                if (dirName.StartsWith(".") || excludeDirSet.Contains(dirName))
                {
                    continue;
                }
                FileUtil.MoveDirectory(dir, desDir, true, false);
            }
            foreach(string file in Directory.GetFiles(srcDir))
            {
                if (Path.GetExtension(file) == ".meta")
                {
                    continue;
                }
                string desPath = Path.Combine(desDir, Path.GetFileName(file));
                FileUtil.MoveFile(file, desPath, true);
            }
        }

        private void OBBFinallyBuild(BuildTarget target, string outputDir, string packageName)
        {
            string targetDir = Path.Combine(outputDir, "_TempOBB/assets");
            string srcZipDir = Path.Combine(outputDir, "_TempOBB");
            string assetDir = Path.Combine(srcZipDir, "assets");
            string buildIDFile = string.Empty;
            try
            {
                //创建build - id命名的文件
                string buildId = GetBuildIDFromAndroidManifest();
                if (string.IsNullOrEmpty(buildId))
                {
                    throw new Exception("Get build-id failed.");
                }
                buildIDFile = Path.Combine(assetDir, buildId);
                File.Create(buildIDFile).Close();//Unity官方打出来的obb文件会有一个buildid命名的空白文件，不知道具体用途，此处为了统一，所以也添加了一个
                string bundleNumber;
                switch (target)
                {
                    case BuildTarget.iOS:
                        bundleNumber = PlayerSettings.iOS.buildNumber;
                        break;
                    case BuildTarget.Android:
                        bundleNumber = PlayerSettings.Android.bundleVersionCode.ToString();
                        break;
                    default:
                        bundleNumber = "1";
                        break;
                }
                string obbNameWithoutPrefix = bundleNumber + "." + PlayerSettings.applicationIdentifier + ".obb";
                string targetPath = Path.Combine(outputDir, "main." + obbNameWithoutPrefix);
                BuildPatchOBB(assetDir, obbNameWithoutPrefix);
                ZipUtil.ZipFileFromDirectory(srcZipDir, targetPath, ICSharpCode.ZeusSharpZipLib.Zip.UseZip64.Off);//不能使用Zip64压缩算法，否则会导致app闪退
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (!string.IsNullOrEmpty(buildIDFile) && File.Exists(buildIDFile))
                {
                    File.Delete(buildIDFile);
                }
                RecoverFiles(targetDir, Application.streamingAssetsPath);
                Directory.Delete(srcZipDir, true);
            }
        }

        private void BuildPatchOBB(string assetPath, string obbNameWithoutPrefix)
        {
            string[] files = Directory.GetFiles(assetPath, InnerPackage._VFileContent + "*", SearchOption.TopDirectoryOnly);
            if(files.Length > 1)
            {
                string firstVfileContentPath = Path.Combine(assetPath, InnerPackage._VFileContent + "0");
                if(File.Exists(firstVfileContentPath))
                {
                    string targetPath = Path.Combine(assetPath + "/../..", "patch." + obbNameWithoutPrefix);
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                    FileUtil.MoveFile(firstVfileContentPath, targetPath, true);
                }
                else
                {
                    Debug.LogError($"There are multiple VfileContent files, but the first one \"{InnerPackage._VFileContent + "0"}\" is missing.");
                }
            }
        }

        private static void RecoverFiles(string srcDir, string desDir)
        {
            if(Directory.Exists(srcDir))
            {
                foreach (string file in Directory.GetFiles(srcDir, "*", SearchOption.AllDirectories))
                {
                    string relativePath = file.Replace(srcDir, "").Substring(1);
                    string desPath = Path.Combine(desDir, relativePath);
                    FileUtil.MoveFile(file, desPath, false);
                }
                Directory.Delete(srcDir, true);
            }
        }

        private string GetBuildIDFromAndroidManifest()
        {
#if UNITY_2019_1_OR_NEWER
            string manifestPath = "Temp/StagingArea/UnityManifest.xml";
#else
            string manifestPath = "Temp/StagingArea/AndroidManifest.xml";
#endif
            if (!File.Exists(manifestPath))
            {
                return string.Empty;
            }
            using (Stream stream = File.Open(manifestPath, FileMode.Open, FileAccess.Read))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);
                XmlNodeList list = doc.ChildNodes;
                XmlNode rootNode = doc.SelectSingleNode("manifest/application");
                foreach (XmlNode node in rootNode.ChildNodes)
                {
                    XmlAttributeCollection attributes = node.Attributes;
                    if (attributes == null)
                    {
                        continue;
                    }
                    XmlAttribute attribute = attributes["android:name"];
                    if (attribute == null)
                    {
                        continue;
                    }
                    else
                    {
                        if (attribute.Value == "unity.build-id")
                        {
                            return attributes["android:value"].Value;
                        }
                    }
                }
                return string.Empty;
            }
        }

        #endregion

        //[Build.FinallyBuild]
        public void OnFinallyBuild(BuildTarget target, string outputPath)
        {
            string packageName = GlobalBuild.Default.packageName;
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.PACKAGE_NAME, ref packageName);
            try
            {
                string outputDir = BuildScript.GetOutputPath(target, outputPath);
                if (UseZeusBuildObb())
                {
                    OBBFinallyBuild(target, outputDir, packageName);
                }

                bool isPackagePatch = false;
                CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_PACKAGE_PATCH, ref isPackagePatch);
                if (_isCombineFile && !isPackagePatch)
                {
                    RecoverFiles(Path.Combine(outputDir, TempStreamingAssetsDirectoryName), Application.streamingAssetsPath);
                    DeleteAllVfilContent();
                    if (File.Exists(Path.Combine(Application.streamingAssetsPath, InnerPackage._VFileIndex)))
                    {
                        File.Delete(Path.Combine(Application.streamingAssetsPath, InnerPackage._VFileIndex));
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                Debug.LogWarning(string.Format("Restore--> include:{0}, exclude{1}", _dictionaryIncludeBuildPath.Count, _dictionaryExcludeBuildPath.Count));
                RecoverIgnoreFolder();
                _RestoreBuildPath();
                Clear();
            }
        }

        private void RecoverIgnoreFolder()
        {
            if (Directory.Exists(TempIgnoreFolderDir))
            {
                foreach (string dir in Directory.GetDirectories(TempIgnoreFolderDir))
                {
                    Zeus.Core.FileUtil.MoveDirectory(dir, Application.streamingAssetsPath, true, false);
                }
                Directory.Delete(TempIgnoreFolderDir, true);
            }
        }

        public static void Clear()
        {
            _dictionaryIncludeBuildPath.Clear();
            _dictionaryExcludeBuildPath.Clear();
        }

        /// <summary>
        /// 将目录添加到打包包含路径中
        /// </summary>
        /// <param name="directory">目录的绝对路径</param>
        /// <param name="targetRPath">相对于StreamingAssets的目标相对目录</param>
        public static void AddIncludeBuildPath(string directory, string targetRPath)
        {
            if (_dictionaryIncludeBuildPath == null) _dictionaryIncludeBuildPath = new Dictionary<string, string>();
            string fullPath = Path.GetFullPath(directory);

            string separator = Path.DirectorySeparatorChar.ToString();
            if (!fullPath.EndsWith(separator))
            {
                fullPath = fullPath + separator;
            }
            if (!Directory.Exists(fullPath))
            {
                UnityEngine.Debug.LogError("cant find file " + fullPath);
            }
            else if (_dictionaryIncludeBuildPath.ContainsKey(fullPath))
            {
                UnityEngine.Debug.LogWarning("file " + fullPath + " is already added");
            }
            else
            {
                _dictionaryIncludeBuildPath[fullPath] = targetRPath;
            }
        }

        /// <summary>
        /// 将文件添加到打包包含路径中
        /// </summary>
        /// <param name="sourceRPath">文件的绝对路径</param>
        /// <param name="destRPath">相对于StreamingAssets的目标相对路径</param>
        public static void AddIncludeBuildFile(string sourceRPath, string destRPath)
        {
            if (_dictionaryIncludeBuildFileDict == null) _dictionaryIncludeBuildFileDict = new Dictionary<string, string>();
            if (_dictionaryIncludeBuildFileDict.ContainsKey(sourceRPath))
            {
                Debug.LogWarning("AddIncludeBuildFile duplicate : " + sourceRPath);
            }
            _dictionaryIncludeBuildFileDict[sourceRPath] = destRPath;
        }
        
        public static void ClearIncludeBuildFile()
        {
            _dictionaryIncludeBuildFileDict.Clear();
        }

        public static void AddExcludeBuildPath(string path)
        {
            if (_dictionaryExcludeBuildPath == null) _dictionaryExcludeBuildPath = new Dictionary<string, bool>();
            _dictionaryExcludeBuildPath[path] = true;
        }

        private void _ProcessBuildPath()
        {
            if (_dictionaryIncludeBuildPath.Count <= 0 && _dictionaryIncludeBuildFileDict.Count <= 0)
            {
                return;
            }

            _ProcessBuildPathDic();

            Debug.LogWarning(string.Format("Rename--> include:{0}, exclude{1}", _dictionaryIncludeBuildPath.Count, _dictionaryExcludeBuildPath.Count));

            if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
            {
                AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            }

            Dictionary<string, string> includeFiles = new Dictionary<string, string>();
            foreach (string directory in _dictionaryIncludeBuildPath.Keys)
            {
                string targetRoot = _dictionaryIncludeBuildPath[directory];
                AddDirToIncludeFilesRecursive(directory, targetRoot, directory, ref includeFiles);
            }
            foreach (string excludePath in _dictionaryExcludeBuildPath.Keys)
            {
                includeFiles.Remove(excludePath);
            }
            foreach (var pair in _dictionaryIncludeBuildFileDict)
            {
                if (includeFiles.ContainsKey(pair.Key))
                {
                    Debug.LogWarning("_ProcessBuildPath duplicate include file " + pair.Key);
                }
                includeFiles[pair.Key] = pair.Value;
            }
            foreach (var pair in includeFiles)
            {
                if(Path.GetFileName(pair.Key) == ".DS_Store")
                {
                    continue;
                }
                string desPath = Path.Combine(Application.streamingAssetsPath, pair.Value);
                if (!Directory.Exists(Path.GetDirectoryName(desPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(desPath));
                }
                FileTool.TryHardLinkCopy(pair.Key, desPath, true);
            }
            AssetDatabase.Refresh();
        }

        static private void AddDirToIncludeFilesRecursive(string topDir, string targetRoot, string directory, ref Dictionary<string, string> includeFiles)
        {
            foreach (string subDir in Directory.GetDirectories(directory))
            {
                if (Path.GetFileName(subDir).StartsWith("."))
                {
                    continue;
                }
                AddDirToIncludeFilesRecursive(topDir, targetRoot, subDir, ref includeFiles);
            }
            foreach (string file in Directory.GetFiles(directory))
            {
                if (Path.GetExtension(file) == ".meta")
                {
                    continue;
                }
                string targetRelativePath = file.Replace(topDir, "");
                targetRelativePath = Path.Combine(targetRoot, targetRelativePath);
                includeFiles.Add(file, targetRelativePath);
            }
        }

        private void _ProcessBuildPathDic()
        {
            _SimplifyBuildPath(_dictionaryIncludeBuildPath);
            _SimplifyBuildPath(_dictionaryExcludeBuildPath);
            _FormatBuildPath(_dictionaryIncludeBuildPath);
            _FormatBuildPath(_dictionaryExcludeBuildPath);
            _FormatBuildPath(_dictionaryIncludeBuildFileDict);
        }

        private void _FormatBuildPath(Dictionary<string, string> dictionary)
        {
            Dictionary<string, string> tempDic = new Dictionary<string, string>(dictionary);
            dictionary.Clear();
            foreach (var pair in tempDic)
            {
                string formatKey = PathUtil.FormatPathSeparator(pair.Key);
                string formatValue = PathUtil.FormatPathSeparator(pair.Value);
                dictionary.Add(formatKey, formatValue);
            }
        }
        private void _FormatBuildPath(Dictionary<string, bool> dictionary)
        {
            Dictionary<string, bool> tempDic = new Dictionary<string, bool>(dictionary);
            dictionary.Clear();
            foreach (var pair in tempDic)
            {
                string formatKey = PathUtil.FormatPathSeparator(pair.Key);
                dictionary.Add(formatKey, pair.Value);
            }
        }

        private void _RestoreBuildPath()
        {
            ClearStreamingAssetsBundlePath();
            if (AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
            {
                foreach (var pair in _dictionaryIncludeBuildFileDict)
                {
                    string desPath = Application.streamingAssetsPath + "/" + pair.Value;
                    if (File.Exists(desPath))
                    {
                        File.Delete(desPath);
                    }
                }

                foreach (string relativeRoot in _dictionaryIncludeBuildPath.Values)
                {
                    string newRoot = relativeRoot;
                    if (relativeRoot.Contains(Path.DirectorySeparatorChar.ToString()))
                    {
                        newRoot = newRoot.Substring(0, newRoot.IndexOf(Path.DirectorySeparatorChar));
                    }
                    if(newRoot == "..")
                    {
                        Debug.LogError($"Wrong format of path \"{relativeRoot}\", please don't use \"..\" in the head of a path.");
                        continue;
                    }
                    AssetDatabase.DeleteAsset(Path.Combine("Assets/StreamingAssets", newRoot));
                }
                if (Directory.GetFiles(Application.dataPath + "/StreamingAssets").Length < 1)
                {
                    //空文件夹就删掉
                    AssetDatabase.DeleteAsset("Assets/StreamingAssets");
                }
                AssetDatabase.Refresh();
            }
        }

        private void ClearStreamingAssetsBundlePath()
        {
            AssetDatabase.DeleteAsset("Assets/StreamingAssets/Bundles");
        }

        /// <summary>
        /// 简化路径，剔除被包含在父目录里的子目录
        /// </summary>
        private void _SimplifyBuildPath<T>(Dictionary<string, T> includeBuildPath)
        {
            string[] listNeedRemove = PathUtil.GetChildPaths(includeBuildPath.Keys);

            foreach (string pathNeedRemove in listNeedRemove)
            {
                includeBuildPath.Remove(pathNeedRemove);
            }
        }

    }
}
