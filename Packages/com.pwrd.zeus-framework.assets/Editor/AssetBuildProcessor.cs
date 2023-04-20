/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Zeus.Build;
using Zeus.Core;
using Zeus.Core.FileSystem;
using Zeus.Framework.Http;
using FileUtil = Zeus.Core.FileUtil;
using Object = System.Object;

namespace Zeus.Framework.Asset
{
    public class AssetBuildProcessor : IModifyPlayerSettings, IBeforeBuild, IAfterBuild, IFinallyBuild
    {
        private static readonly string StartScenePath = "Assets/Scene/Start.unity";
        private static EditorBuildSettingsScene[] EditorSettingScenesOldValue;
        private static AssetManagerSetting _oriSetting;



        private const string SubpackageExtension = ".subpackage";
        private const string ChunkExtension = DownloadService.ChunkExtension;
        private const string ChunkFolder = "Chunk";
        private const string TempFolder = "_Temp";
        private const string OriginBundleFolder = "Origin";
        private const string OthersBundleFolder = "Others";
        private string SubpackageFolder = "OtherAssetBundles";
        private const string NormalLevel = "Normal"; //用来标记需要放入所有Level的Bundle
        private static AssetManagerSetting _setting;

        private static Dictionary<string, List<string>> _tag2BundleSequence;//Tag中包含Parent与Child
        private static Dictionary<string, List<string>> _tag2OtherSequence;//Tag中只有Child
        private static Dictionary<string, List<AssetListHelper.FileSequence>> _tag2AllFileSequence;//Tag中包含Parent与Child
        private static List<string> _tagSequence;//Tag中包含Parent与Child

        private string _chunkListName;

        class Bundle2ChunkInfo
        {
            public Dictionary<string, string> bundle2ChunkName;
            public Dictionary<string, long> bundle2ChunkFrom;
            public Dictionary<string, string> other2ChunkName;
            public Bundle2ChunkInfo()
            {
                bundle2ChunkName = new Dictionary<string, string>();
                bundle2ChunkFrom = new Dictionary<string, long>();
                other2ChunkName = new Dictionary<string, string>();
            }
        }
        private Dictionary<string, Bundle2ChunkInfo> _level2ChunkInfo;
        private Dictionary<string, string> _chunkName2FinalName;
        private Dictionary<string, int> _chunkName2Crc32;
        private Dictionary<string, long> _chunkFinalName2Size;
        private Dictionary<string, List<string>> _tag2ChunkName;//Tag中包含Parent与Child
        private Dictionary<string, HashSet<string>> _chunkName2Level;
        private Dictionary<string, string> _chunkStr2Chunk;

        private static Dictionary<string, long> _bundleName2Size;

        private volatile Exception ChunkCompressException;

        private KeepAliveBuildHelper _keepAliveBuildHelper = new KeepAliveBuildHelper();
        private UnityHttpDownloaderBuildHelper _unityHttpDownloaderBuildHelper = new UnityHttpDownloaderBuildHelper();


        /// <summary>
        /// 将Build目标平台的AssetBundle放到包中。
        /// </summary>
        public void OnModifyPlayerSettings(BuildTarget target)
        {
            _oriSetting = AssetManagerSetting.LoadSetting();
            OverrideSettingByCommandLineArg(target);
            RemoveScenes();
            ModifyBundleBuildPath(target);
            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle && _setting.bundleLoaderSetting.isSupportBackgroundDownload)
            {
                _keepAliveBuildHelper.OnModifyPlayerSettings(target);
            }
        }

        public void RemoveScenes()
        {
            RemoveInvalidScene();
            EditorSettingScenesOldValue = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length];
            System.Array.Copy(EditorBuildSettings.scenes, EditorSettingScenesOldValue, EditorBuildSettings.scenes.Length);

            //修改ZeusAssetManagerSetting.json  
            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle)
            {
                List<string> scenesList = AssetManagerSetting.LoadSetting().bundleLoaderSetting.ScenesInBuild;
                if (scenesList.Count > 0)
                {
                    EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[scenesList.Count];
                    for (int i = 0; i < scenes.Length; ++i)
                    {
                        scenes[i] = new EditorBuildSettingsScene(scenesList[i], true);
                    }
                    EditorBuildSettings.scenes = scenes;
                }
                else
                {
                    EditorBuildSettings.scenes = new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(StartScenePath, true) };
                }
            }
        }

        //去除EditorBuildSettings中被删除的Scene
        private static void RemoveInvalidScene()
        {
            if (EditorBuildSettings.scenes.Length > 0)
            {
                List<EditorBuildSettingsScene> newScenes = new List<EditorBuildSettingsScene>();
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                    if (string.IsNullOrWhiteSpace(scene.path))
                    {
                        continue;
                    }
                    string fullPath = Path.GetFullPath(scene.path);
                    if (File.Exists(fullPath))
                    {
                        newScenes.Add(scene);
                    }
                }
                EditorBuildSettings.scenes = newScenes.ToArray();
                newScenes.Clear();
            }
        }

        private void OverrideSettingByCommandLineArg(BuildTarget target)
        {
            _setting = AssetManagerSetting.LoadSetting();
            
            bool useBundleLoader = false;
            if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.USE_BUNDLELOADER, ref useBundleLoader))
            {
                //修改ZeusAssetManagerSetting.json  
                if (useBundleLoader)
                {
                    _setting.assetLoaderType = AssetLoaderType.AssetBundle;
                }
            }

            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle)
            {
                if (target == BuildTarget.Android)
                {
                    bool isFillFirstPackage = false;
                    if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_FILL_FIRSTPACKAGE_ANDROID, ref isFillFirstPackage))
                    {
                        _setting.bundleLoaderSetting.isFillFirstPackageAndroid = isFillFirstPackage;
                        if (isFillFirstPackage)
                        {
                            int fillSize = 0;
                            if (CommandLineArgs.TryGetInt(GlobalBuild.CmdArgsKey.FILL_FIRSTPACKAGE_SIZE_ANDROID, ref fillSize))
                            {
                                _setting.bundleLoaderSetting.TargetAndroidAssetSizeInMB = fillSize;
                            }
                        }
                    }
                }
                else if (target == BuildTarget.iOS)
                {
                    bool isFillFirstPackage = false;
                    if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_FILL_FIRSTPACKAGE_IOS, ref isFillFirstPackage))
                    {
                        _setting.bundleLoaderSetting.isFillFirstPackageiOS = isFillFirstPackage;
                        if (isFillFirstPackage)
                        {
                            int fillSize = 0;
                            if (CommandLineArgs.TryGetInt(GlobalBuild.CmdArgsKey.FILL_FIRSTPACKAGE_SIZE_IOS, ref fillSize))
                            {
                                _setting.bundleLoaderSetting.TargetiOSAssetSizeInMB = fillSize;
                            }
                        }
                    }
                }
            }
            bool useSubpackage = false;
            if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.USE_SUBPACKAGE, ref useSubpackage))
            {
                _setting.bundleLoaderSetting.useSubPackage = useSubpackage;
            }

            bool supportBackgroundDownload = true;
            if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.SUPPORT_BACKGROUND_DOWNLOAD, ref supportBackgroundDownload))
            {
                _setting.bundleLoaderSetting.isSupportBackgroundDownload = supportBackgroundDownload;
            }
            if (_setting.bundleLoaderSetting.isSupportBackgroundDownload)
            {
                bool backgroundDownload = true;
                if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.SUBPACKAGE_BACKGROUND_DOWNLOAD, ref backgroundDownload))
                {
                    _setting.bundleLoaderSetting.isBackgroundDownloadAllowed = backgroundDownload;
                }
            }
            else
            {
                _setting.bundleLoaderSetting.isBackgroundDownloadAllowed = false;
            }

            string url = string.Empty;
            int index = 0;
            List<string> urlList = new List<string>();
            while (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.SUBPACKAGE_SERVER_URL + index.ToString(), ref url))
            {
                urlList.Add(url);
                ++index;
            }
            urlList.RemoveAll(t => string.IsNullOrEmpty(t));
            if (urlList.Count > 0)
            {
                _setting.bundleLoaderSetting.remoteURL = urlList;
            }
            AssetManagerSetting.SaveSetting(_setting);
        }

        public void AddCustomUrls(string[] array)
        {
            _setting.bundleLoaderSetting.remoteURL.AddRange(array);
            AssetManagerSetting.SaveSetting(_setting);
        }

        private void ModifyBundleBuildPath(BuildTarget target)
        {
            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle)
            {
                AssetBundleUtils.ReInit();
                AssetBundleUtils.BuildFlatBuffer();
                HashSet<string> subPackageBundleSet;
                if (_setting.bundleLoaderSetting.useSubPackage)
                {
                    AssetListHelper.Refresh();
                    List<string> includeAssetBundleList = AssetListHelper.GetFirstPackageBundleList(TagAsset.LoadTagAsset().TagListWithoutChild[0]);
                    //根据不同的分包下载策略，生成不同的首包列表和二包列表，比如分模块下载策略，未统计的Bundle将打到首包内
                    GenerateBundleSequenceAndFirstPackageBundleList(_setting.bundleLoaderSetting.mode, ref includeAssetBundleList, target);
                    GenerateOtherSequence();
                    FillFirstPackage(ref includeAssetBundleList);
                    subPackageBundleSet = new HashSet<string>(AssetBundleUtils.GetAllAssetBundles());
                    foreach (string abName in includeAssetBundleList)
                    {
                        subPackageBundleSet.Remove(abName);
                    }
                }
                else
                {
                    _tagSequence = new List<string>();
                    _tag2BundleSequence = new Dictionary<string, List<string>>();
                    _tag2OtherSequence = new Dictionary<string, List<string>>();
                    subPackageBundleSet = new HashSet<string>();
                    subPackageBundleSet.Add("SubpackageBundleInfo");
                }
                subPackageBundleSet.Add(AssetBundleUtils.AssetMapNameXml);
                subPackageBundleSet.Add(AssetBundleUtils.Bundle2AtlasXml);
                subPackageBundleSet.Add(AssetBundleUtils.MD5VersionXml);

                var bundleRootPath = EditorAssetBundleUtils.GetPathRoot(target);
                string editorBundleDirectory = Path.GetFullPath(bundleRootPath);
                var files = Directory.GetFiles(editorBundleDirectory, "*", SearchOption.AllDirectories);
                GameBuildProcessor.ClearIncludeBuildFile();
                foreach (var filePath in files)
                {
                    if (Path.GetExtension(filePath) == ".manifest")
                    {
                        continue;
                    }
                    string bundleName = Path.GetFileName(filePath);
                    if (subPackageBundleSet != null && subPackageBundleSet.Contains(bundleName))
                    {
                        continue;
                    }
                    string prePath = string.Empty;
                    if (_setting.bundleLoaderSetting.useBundleScatter)
                    {
                        prePath = AssetBundleUtils.GetBundleScatterFolder(bundleName);
                    }
                    string targetPath = PathUtil.CombinePath(bundleRootPath, prePath, bundleName);
                    GameBuildProcessor.AddIncludeBuildFile(filePath, targetPath);
                }
            }
        }

        private void GenerateBundleSequenceAndFirstPackageBundleList(SubpackageMode mode, ref List<string> firstPackagBundleList, BuildTarget target)
        {
            AssetBundleUtils.ReInit();
            Dictionary<string, List<string>> tag2AssetList = new Dictionary<string, List<string>>();
            TagAsset tagAsset = TagAsset.LoadTagAsset();
            _tagSequence = tagAsset.TagListWithoutRoot;//Tag中包含Parent与Child
            foreach (string tag in _tagSequence)
            {
                tag2AssetList.Add(tag, AssetListHelper.LoadAssetListFromFiles(new string[1] { tag }));//Tag中包含Parent与Child
            }
            if (mode == SubpackageMode.AddUnrecordedAssetToFirstpackage)
            {
                if (tagAsset.manuallyOtherTag)
                {
                    throw new Exception("选择将未记录资源加入首包时,不能再手动指定Others Tag的位置!");
                }
                var allAssets = AssetBundleUtils.GetAssetMapBundles().Keys;
                var subpackageAssets = AssetListHelper.LoadAssetListFromFiles(tagAsset.TagListOnlyParent.ToArray());
                var subpackageAssetSet = new HashSet<string>(subpackageAssets);
                var firstPackageAssets = new List<string>();
                foreach (var asset in allAssets)
                {
                    if (!subpackageAssetSet.Contains(asset))
                    {
                        firstPackageAssets.Add(asset);
                    }
                }
                firstPackagBundleList.Clear();
                firstPackagBundleList.AddRange(AssetListHelper.GetBundleListFromAssetList(firstPackageAssets));
            }

            _tag2BundleSequence = new Dictionary<string, List<string>>();//Tag中包含Parent与Child
            foreach (var pair in tag2AssetList)
            {
                List<string> templ = AssetListHelper.GetBundleSequenceFromAssetList(pair.Value, new HashSet<string>(firstPackagBundleList));
                _tag2BundleSequence.Add(pair.Key, templ);
            }
            _tag2AllFileSequence = new Dictionary<string, List<AssetListHelper.FileSequence>>();//Tag中包含Parent与Child
            foreach (var pair in tag2AssetList)
            {
                var templ = AssetListHelper.GetAllSequenceFromAssetList(pair.Value, new HashSet<string>(firstPackagBundleList));
                _tag2AllFileSequence.Add(pair.Key, templ);
            }

            if (mode == SubpackageMode.CreateOtherTagForUnrecordedAsset)
            {
                if (!tagAsset.manuallyOtherTag)
                {
                    List<string> recordedBundle = new List<string>();
                    foreach (var pair in _tag2BundleSequence)
                    {
                        if (tagAsset.TagListOnlyParent.Contains(pair.Key))//去 ChildTag
                        {
                            recordedBundle.AddRange(pair.Value);
                        }
                    }
                    recordedBundle.AddRange(firstPackagBundleList);
                    var unrecordedBundles = AssetListHelper.GetUnrecordedBundleList(recordedBundle);
                    var fileSequences = new List<AssetListHelper.FileSequence>();
                    foreach (var bundle in unrecordedBundles)
                    {
                        fileSequences.Add(new AssetListHelper.FileSequence(bundle, true));
                    }
                    _tag2BundleSequence.Add(AssetBundleLoaderSetting.OthersTag, unrecordedBundles);
                    _tag2AllFileSequence.Add(AssetBundleLoaderSetting.OthersTag, fileSequences);
                    _tagSequence.Add(AssetBundleLoaderSetting.OthersTag);
                }
                else
                {
                    List<string> recordedBundle = new List<string>();
                    foreach (var pair in _tag2BundleSequence)
                    {
                        if (tagAsset.TagListOnlyParent.Contains(pair.Key))//去 ChildTag
                        {
                            recordedBundle.AddRange(pair.Value);
                        }
                    }
                    recordedBundle.AddRange(firstPackagBundleList);
                    var unrecordedBundles = AssetListHelper.GetUnrecordedBundleList(recordedBundle);
                    if (unrecordedBundles.Count != 0)
                    {
                        throw new Exception("UnrecordedBundles are not collected!");
                    }
                }
            }
            _bundleName2Size = GetAssetBundleSize(firstPackagBundleList);//计算全部bundle的size, 首包bundle列表只能在这里拿到
        }

        private void GenerateOtherSequence()
        {
            Dictionary<string, List<string>> tag2AssetList = new Dictionary<string, List<string>>();//Tag中只有Child
            _tag2OtherSequence = new Dictionary<string, List<string>>();
            TagAsset tagAsset = TagAsset.LoadTagAsset();
            foreach (string tag in tagAsset.TagListOnlyChild)
            {
                tag2AssetList.Add(tag, AssetListHelper.LoadAssetListFromFiles(new string[1] { tag }));
            }
            var otherHash = new HashSet<string>();
            foreach (var pair in tag2AssetList)
            {
                List<string> others = AssetListHelper.GetOtherAssetListFromAssetList(pair.Value);
                for (int i = others.Count - 1; i >= 0; i--)
                {
                    var other = others[i];
                    if (!otherHash.Add(other))
                    {
                        others.RemoveAt(i);
                        Debug.Log("" + pair.Key + " skip:  " + other);
                    }
                }
                _tag2OtherSequence.Add(pair.Key, others);
            }
        }

        protected Dictionary<string, long> GetAssetBundleSize(List<string> firstPackagBundleList)
        {
            string pathRoot = EditorAssetBundleUtils.GetPathRoot();
            Dictionary<string, long> bundle2Size = new Dictionary<string, long>();
            foreach (string bundle in firstPackagBundleList)
            {
                if (!bundle2Size.ContainsKey(bundle))
                {
                    string bundlePath = Path.Combine(pathRoot, bundle);
                    bundle2Size.Add(bundle, new FileInfo(bundlePath).Length);
                }
            }
            foreach (var pair in _tag2BundleSequence)
            {
                foreach (string bundle in pair.Value)
                {
                    if (!bundle2Size.ContainsKey(bundle))
                    {
                        string bundlePath = Path.Combine(pathRoot, bundle);
                        bundle2Size.Add(bundle, new FileInfo(bundlePath).Length);
                    }
                }
            }
            return bundle2Size;
        }

        private void FillFirstPackage(ref List<string> firstPackageBundleList)
        {
#if UNITY_ANDROID
            if (!_setting.bundleLoaderSetting.isFillFirstPackageAndroid)
            {
                return;
            }
#elif UNITY_IOS
            if (!_setting.bundleLoaderSetting.isFillFirstPackageiOS)
            {
                return;
            }
#else
            return;
#endif
            Func<string, bool, long> GetSize = (string fileName, bool isBundle) =>
            {
                if (isBundle)
                {
                    return _bundleName2Size[fileName];
                }
                var fileInfo = new FileInfo(Path.Combine(Application.dataPath, fileName));
                return fileInfo.Length;
            };

            long firstPackageMaxSize = 0;
#if UNITY_ANDROID
            firstPackageMaxSize = (long)_setting.bundleLoaderSetting.TargetAndroidAssetSizeInMB * Zeus.Core.ZeusConstant.MB;
#elif UNITY_IOS
            firstPackageMaxSize = (long)_setting.bundleLoaderSetting.TargetiOSAssetSizeInMB * Zeus.Core.ZeusConstant.MB;
#endif
            long fpSize = 0;
            HashSet<string> fpBundleSet = new HashSet<string>(firstPackageBundleList);
            foreach (string bundle in firstPackageBundleList)
            {
                fpSize += _bundleName2Size[bundle];
            }
            int index = 0;
            string currentTag = string.Empty;
            List<string> removeTagList = new List<string>();
            foreach (string tag in _tagSequence)
            {
                List<AssetListHelper.FileSequence> files;
                if (_tag2AllFileSequence.TryGetValue(tag, out files))
                {
                    for (index = 0; index < files.Count; ++index)
                    {
                        if ((fpSize += GetSize(files[index].fileName, files[index].isBundle)) < firstPackageMaxSize)
                        {
                            if (fpBundleSet.Add(files[index].fileName))
                            {
                                firstPackageBundleList.Add(files[index].fileName);
                            }
                            else
                            {
                                fpSize -= GetSize(files[index].fileName, files[index].isBundle);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (fpSize > firstPackageMaxSize)
                    {
                        currentTag = tag;
                        break;
                    }
                    else
                    {
                        removeTagList.Add(tag);
                    }
                }
            }
            foreach (string tag in removeTagList)
            {
                _tag2BundleSequence.Remove(tag);
                _tag2AllFileSequence.Remove(tag);
                _tag2OtherSequence.Remove(tag);
                _tagSequence.Remove(tag);
            }
            if (!string.IsNullOrEmpty(currentTag))
            {
                List<string> newBundleList = new List<string>();
                List<string> newOtherList = new List<string>();

                var newBundleListFile = new List<AssetListHelper.FileSequence>();
                for (int i = index; i < _tag2AllFileSequence[currentTag].Count; ++i)
                {
                    newBundleListFile.Add(_tag2AllFileSequence[currentTag][i]);
                    if (_tag2AllFileSequence[currentTag][i].isBundle)
                    {
                        newBundleList.Add(_tag2AllFileSequence[currentTag][i].fileName);
                    }
                    else
                    {
                        newOtherList.Add(_tag2AllFileSequence[currentTag][i].fileName);
                    }
                }
                _tag2AllFileSequence[currentTag] = newBundleListFile;
                _tag2BundleSequence[currentTag] = newBundleList;
                _tag2OtherSequence[currentTag] = newOtherList;
            }
            ExcludeFirstPackageBundlesFromSubpackage(firstPackageBundleList);
        }

        private void ExcludeFirstPackageBundlesFromSubpackage(List<string> firstPackagBundleList)
        {
            HashSet<string> fpBundleSet = new HashSet<string>(firstPackagBundleList);
            List<string> removeTagList = new List<string>();
            foreach (string tag in _tagSequence)
            {
                List<string> newBundles = new List<string>();

                List<string> bundles;
                if (_tag2BundleSequence.TryGetValue(tag, out bundles))
                {
                    foreach (string bundle in bundles)
                    {
                        if (!fpBundleSet.Contains(bundle))
                        {
                            newBundles.Add(bundle);
                        }
                    }
                    if (newBundles.Count == 0)
                    {
                        removeTagList.Add(tag);
                    }
                    else
                    {
                        _tag2BundleSequence[tag] = newBundles;
                    }
                }
            }
            foreach (string tag in removeTagList)
            {
                _tag2BundleSequence.Remove(tag);
                _tagSequence.Remove(tag);
            }

            foreach (string tag in _tagSequence)
            {
                var newBundlesFile = new List<AssetListHelper.FileSequence>();
                List<AssetListHelper.FileSequence> files;
                if (_tag2AllFileSequence.TryGetValue(tag, out files))
                {
                    foreach (var file in files)
                    {
                        if (!fpBundleSet.Contains(file.fileName))
                        {
                            newBundlesFile.Add(file);
                        }
                    }
                    if (newBundlesFile.Count == 0)
                    {
                        removeTagList.Add(tag);
                    }
                    else
                    {
                        _tag2AllFileSequence[tag] = newBundlesFile;
                    }
                }
            }
            foreach (string tag in removeTagList)
            {
                _tag2AllFileSequence.Remove(tag);
            }
        }

        public void OnBeforeBuild(BuildTarget target, string locationPathName)
        {
            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle)
            {
                if (_setting.bundleLoaderSetting.useSubPackage)
                {
                    SubpackageFolder += DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff");
                    string fullPath = Path.Combine(locationPathName, SubpackageFolder);
                    ProcessSubpackageAssetBundle(fullPath);
                    bool skipExportSubpackage = false;
                    CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.SKIP_EXPORT_SUBPACKAGE, ref skipExportSubpackage);
                    if (!skipExportSubpackage)
                    {
                        GenerateBundleInfoList(Path.Combine(fullPath, ChunkFolder), _chunkFinalName2Size);
                        // if (!VFileSystem.Exists(SubPackageBundleInfoContainer.SubpackageBundleInfoPath))
                        // {
                        //     throw new Exception("Generate bundle info failed.");
                        // }
                        CompressExcludeAssetBundle(locationPathName);
                        GenerateCDNUrlListFile(locationPathName);
                    }
                }
                _RenameResources();
                if (_setting.bundleLoaderSetting.isSupportBackgroundDownload)
                {
                    _keepAliveBuildHelper.OnBeforeBuild(target);
                }
                _unityHttpDownloaderBuildHelper.OnBeforeBuild(target);
            }
            else
            {
                GameBuildProcessor.AddExcludeBuildPath(Application.dataPath + "/" + SubPackageBundleInfoContainer.SubpackageBundleInfoPath);
            }
        }

        private void ProcessSubpackageAssetBundle(string locationPathName)
        {
            bool skipExportSubpackage = false;
            CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.SKIP_EXPORT_SUBPACKAGE, ref skipExportSubpackage);
            if (skipExportSubpackage)
            {
                CopyIncludeOtherAsset();
                MoveIncludeOtherAsset(Path.Combine(locationPathName, OthersBundleFolder));
            }
            else
            {
                CopyExcludeAssetBundle(Path.Combine(locationPathName, OriginBundleFolder));
                CombineExcludeAssetBundle(Path.Combine(locationPathName, TempFolder), _setting.bundleLoaderSetting.AssetBundleChunkSizeInMB);
                CopyIncludeOtherAsset();
                MoveIncludeOtherAsset(Path.Combine(locationPathName, OthersBundleFolder));
                CompressChunks(Path.Combine(locationPathName, TempFolder), Path.Combine(locationPathName, ChunkFolder));
                GenerateChunkList(Path.Combine(locationPathName, ChunkFolder));
                if (_setting.bundleLoaderSetting.useBundleScatter)
                {
                    ScatterBundlePath(Path.Combine(locationPathName, OriginBundleFolder));
                }
            }
        }

        protected void CopyExcludeAssetBundle(string locationPathName)
        {
            if (!Directory.Exists(locationPathName))
            {
                Directory.CreateDirectory(locationPathName);
            }
            foreach (var pair in _tag2BundleSequence)
            {
                foreach (string assetBundle in pair.Value)
                {
                    string sourceBundlePath = Path.Combine(EditorAssetBundleUtils.GetPathRoot(), assetBundle);
                    FileTool.TryHardLinkCopy(sourceBundlePath, locationPathName + "/" + assetBundle, true);
                }
            }
        }

        /// <summary>
        /// 移动二包列表中的other资源 打入二包
        /// </summary>
        /// <param name="locationPathName"></param>
        private void MoveIncludeOtherAsset(string locationPathName)
        {
            foreach (var pair in _tag2OtherSequence)
            {
                foreach (string otherAsset in pair.Value)
                {
                    string sourceBundlePath = Path.Combine(Application.dataPath, otherAsset);
                    var firstIndex = otherAsset.IndexOf("/") + 1;
                    var lastIndex = otherAsset.LastIndexOf("/");
                    var relativePath = firstIndex > lastIndex ? "" : otherAsset.Substring(firstIndex, lastIndex - firstIndex);
                    if (!Directory.Exists(locationPathName + "/" + relativePath))
                    {
                        Directory.CreateDirectory(locationPathName + "/" + relativePath);
                    }
                    File.Move(sourceBundlePath, locationPathName + "/" + otherAsset.Substring(firstIndex));
                }
            }
        }

        /// <summary>
        /// 拷贝二包列表中的other资源 用于打包后还原
        /// </summary>
        private void CopyIncludeOtherAsset()
        {
            var streamingAssetsTempPath = GlobalBuild.BuildConst.ZEUS_BUILD_PATH_STREAMING_TEMP;
            foreach (var pair in _tag2OtherSequence)
            {
                foreach (string otherAsset in pair.Value)
                {
                    string sourceBundlePath = Path.Combine(Application.dataPath, otherAsset);
                    var firstIndex = otherAsset.IndexOf("/") + 1;
                    var lastIndex = otherAsset.LastIndexOf("/");
                    var relativePath = firstIndex > lastIndex ? "" : otherAsset.Substring(firstIndex, lastIndex - firstIndex);
                    if (!Directory.Exists(streamingAssetsTempPath + "/" + relativePath))
                    {
                        Directory.CreateDirectory(streamingAssetsTempPath + "/" + relativePath);
                    }
                    FileTool.TryHardLinkCopy(sourceBundlePath, streamingAssetsTempPath + "/" + otherAsset.Substring(firstIndex), true);
                }
            }
        }

        /// <summary>
        /// 打包后还原other资源
        /// </summary>
        private void MoveBackIncludeOtherAsset()
        {
            var streamingAssetsTempPath = GlobalBuild.BuildConst.ZEUS_BUILD_PATH_STREAMING_TEMP;
            if (Directory.Exists(streamingAssetsTempPath))
            {
                foreach (var pair in _tag2OtherSequence)
                {
                    foreach (string otherAsset in pair.Value)
                    {
                        var sourcePath = Path.Combine(streamingAssetsTempPath,
                            otherAsset.Substring(otherAsset.IndexOf("/") + 1));
                        File.Move(sourcePath, Path.Combine(Application.dataPath, otherAsset));
                    }
                }
                Directory.Delete(streamingAssetsTempPath, true);
            }
        }

        protected void ScatterBundlePath(string locationPathName)
        {
            var bundles = Directory.GetFiles(locationPathName);
            foreach (string sourcePath in bundles)
            {
                string bundleName = Path.GetFileName(sourcePath);
                string targetPath = CalcScatterBundlePath(locationPathName, bundleName);
                FileUtil.EnsureFolder(targetPath);
                File.Move(sourcePath, targetPath);
            }
        }

        private string CalcScatterBundlePath(string folderPath, string bundleName)
        {
            string scatterFolderName = AssetBundleUtils.GetBundleScatterFolder(bundleName);
            string result = PathUtil.CombinePath(folderPath, scatterFolderName, bundleName);
            return result;
        }

        class ChunkData
        {
            public List<string> bundleList = new List<string>();
            public List<string> otherAssetList = new List<string>();
            public List<AssetListHelper.FileSequence> fileList = new List<AssetListHelper.FileSequence>();
            public long totalSize = 0;
            public string GetFileStr()
            {
                string str = "";
                foreach (var file in fileList)
                {
                    str += file.fileName;
        }
                return str;
            }
        }

        private void CombineExcludeAssetBundle(string locationPathName, int chunkSizeInMB)
        {
            if (!Directory.Exists(locationPathName))
            {
                Directory.CreateDirectory(locationPathName);
            }
            _level2ChunkInfo = new Dictionary<string, Bundle2ChunkInfo>();
            foreach (var level in GetAllLevels())
            {
                _level2ChunkInfo.Add(level, new Bundle2ChunkInfo());
            }
            _chunkName2FinalName = new Dictionary<string, string>();
            _chunkName2Crc32 = new Dictionary<string, int>();
            _chunkFinalName2Size = new Dictionary<string, long>();
            _tag2ChunkName = new Dictionary<string, List<string>>();
            _chunkName2Level = new Dictionary<string, HashSet<string>>();
            _chunkStr2Chunk = new Dictionary<string, string>();

            long maxChunkSize = chunkSizeInMB * 1024 * 1024;

            string sourceDirectory = EditorAssetBundleUtils.GetPathRoot();
            int totalChunkNum = 0;

            TagAsset tagAsset = TagAsset.LoadTagAsset();
            foreach (var arg in tagAsset.args)
            {
                if (arg.type == TagNodeType.Parent && _tag2AllFileSequence.ContainsKey(arg.title))
                {
                    _tag2ChunkName.Add(arg.title, DoCombine(_tag2AllFileSequence[arg.title], sourceDirectory, locationPathName, maxChunkSize, ref totalChunkNum));
                }
            }
            foreach (var arg in tagAsset.args)
            {
                if (arg.type == TagNodeType.Normal && _tag2AllFileSequence.ContainsKey(arg.title))
                {
                    _tag2ChunkName.Add(arg.title, DoCombine(_tag2AllFileSequence[arg.title], sourceDirectory, locationPathName, maxChunkSize, ref totalChunkNum));
                }
            }
            if (_setting.bundleLoaderSetting.mode == SubpackageMode.CreateOtherTagForUnrecordedAsset && _tag2AllFileSequence.Count > 0 && !tagAsset.manuallyOtherTag)
            {
                _tag2ChunkName.Add(AssetBundleLoaderSetting.OthersTag, DoCombine(_tag2AllFileSequence[AssetBundleLoaderSetting.OthersTag], sourceDirectory, locationPathName, maxChunkSize, ref totalChunkNum));
            }
        }

        private List<string> DoCombine(List<AssetListHelper.FileSequence> files, string sourceDirectory,
            string locationPathName, long maxChunkSize, ref int totalChunkNum)
        {
            Dictionary<string, List<ChunkData>> chunkLists = new Dictionary<string, List<ChunkData>>();
            foreach (string assetLevel in GetAllLevels())
            {
                chunkLists.Add(assetLevel, new List<ChunkData> { new ChunkData() });
            }

            List<string> chunkNameList = new List<string>();
            HashSet<string> chunkNameSet = new HashSet<string>();
            ChunkData chunkData = null;

            foreach (var pair in chunkLists)
            {
                var chunkLevel = pair.Key;
                var chunkList = pair.Value;
                foreach (var file in files)
                {
                    var bundleLevel = GetBundleLevel(file.fileName);
                    if (chunkLevel != NormalLevel && bundleLevel != chunkLevel && bundleLevel != NormalLevel)
                    {
                        continue;
                    }

                    if (chunkList.Count == 1)
                    {
                        chunkData = chunkList[0];
                    }

                    if (file.isBundle) //bundle的处理
                    {
                        var bundle = file;
                        //检查是否这个Bundle已经被合并到某个Chunk了
                        string chunkName;
                        if (_level2ChunkInfo[chunkLevel].bundle2ChunkName.TryGetValue(bundle.fileName, out chunkName))
                        {
                            if (chunkNameSet.Add(chunkName))
                            {
                                chunkNameList.Add(chunkName);
                            }

                            continue;
                        }

                        var bundlePath = Path.Combine(sourceDirectory, bundle.fileName);
                        var fileInfo = new FileInfo(bundlePath);
                        if (fileInfo.Length > maxChunkSize)
                        {
                            if (chunkData.totalSize == 0)
                            {
                                chunkData.bundleList.Add(bundle.fileName);
                                chunkData.fileList.Add(bundle);
                                chunkData.totalSize += fileInfo.Length;
                                chunkData = new ChunkData();
                                chunkList.Add(chunkData);
                            }
                            else
                            {
                                var aloneChunk = new ChunkData();
                                aloneChunk.bundleList.Add(bundle.fileName);
                                aloneChunk.fileList.Add(bundle);
                                aloneChunk.totalSize += fileInfo.Length;
                                chunkList.Add(aloneChunk);
                            }
                        }
                        else
                        {
                            chunkData.bundleList.Add(bundle.fileName);
                            chunkData.fileList.Add(bundle);
                            chunkData.totalSize += fileInfo.Length;
                            if (chunkData.totalSize > maxChunkSize)
                            {
                                chunkData = new ChunkData();
                                chunkList.Add(chunkData);
                            }
                        }
                    }
                    else //other资源的处理
                    {
                        string chunkName;
                        if (_level2ChunkInfo[chunkLevel].other2ChunkName.TryGetValue(file.fileName, out chunkName))
                        {
                            if (chunkNameSet.Add(chunkName))
                            {
                                chunkNameList.Add(chunkName);
                            }

                            continue;
                        }

                        if (chunkData.totalSize == 0)
                        {
                            chunkData.otherAssetList.Add(file.fileName);
                            chunkData.fileList.Add(file);
                            chunkData = new ChunkData();
                            chunkList.Add(chunkData);
                        }
                        else
                        {
                            var aloneChunk = new ChunkData();
                            aloneChunk.otherAssetList.Add(file.fileName);
                            aloneChunk.fileList.Add(file);
                            chunkList.Add(aloneChunk);
                        }
                    }
                }
            }

            RedundantFileCheckSumInfo ckeckSumInfo = RedundantFileCheckSumInfo.CreateNewOrLoadInfos();
            byte[] buffer = new byte[1024];
            foreach (var pair in chunkLists)
            {
                var chunkList = pair.Value;
                var chunkLevel = pair.Key;
                for (int i = 0; i < chunkList.Count; i++)
                {
                    chunkData = chunkList[i];
                    if (chunkData.bundleList.Count + chunkData.otherAssetList.Count > 0)
                    {
                        if (_chunkStr2Chunk.ContainsKey(chunkData.GetFileStr()))
                        {
                            var lastLevel = _chunkName2Level[_chunkStr2Chunk[chunkData.GetFileStr()]].First();
                            _chunkName2Level[_chunkStr2Chunk[chunkData.GetFileStr()]].Add(chunkLevel);
                            foreach (var bundleName in chunkData.bundleList)
                            {
                                _level2ChunkInfo[chunkLevel].bundle2ChunkFrom[bundleName] = _level2ChunkInfo[lastLevel].bundle2ChunkFrom[bundleName];
                                _level2ChunkInfo[chunkLevel].bundle2ChunkName[bundleName] = _level2ChunkInfo[lastLevel].bundle2ChunkName[bundleName];
                            }
                            foreach (var otherName in chunkData.otherAssetList)
                            {
                                _level2ChunkInfo[chunkLevel].other2ChunkName[otherName] = _level2ChunkInfo[lastLevel].other2ChunkName[otherName];
                            }
                            continue;
                        }

                        string chunkName = "chunk_" + (i + totalChunkNum) + ChunkExtension;
                        _chunkStr2Chunk.Add(chunkData.GetFileStr(), chunkName);
                        _chunkName2Level[chunkName] = new HashSet<string> { chunkLevel };
                        chunkNameList.Add(chunkName);
                        string chunkPath = Path.Combine(locationPathName, chunkName);
                        long bundlefrom = 0;
                        using (FileStream fsWrite = File.Open(chunkPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            foreach (var bundleName in chunkData.bundleList)
                            {
                                var bundlePath = Path.Combine(sourceDirectory, bundleName);

                                if (_setting.bundleLoaderSetting.useBundleScatter)
                                {
                                    ckeckSumInfo.Update(CalcScatterBundlePath(sourceDirectory, bundleName), bundlePath,
                                        true);
                                }
                                else
                                {
                                    ckeckSumInfo.Update(bundlePath, bundlePath, true);
                                }

                                var fileInfo = new FileInfo(bundlePath);

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

                                _level2ChunkInfo[chunkLevel].bundle2ChunkFrom[bundleName] = bundlefrom;
                                bundlefrom += fileInfo.Length;
                                _level2ChunkInfo[chunkLevel].bundle2ChunkName[bundleName] = chunkName;
                            }

                            foreach (var otherAssetPath in chunkData.otherAssetList)
                            {
                                var bundlePath = Path.Combine(Application.dataPath, otherAssetPath);

                                var relativePath =
                                    otherAssetPath.Substring(otherAssetPath.IndexOf("StreamingAssets") + 16);
                                ckeckSumInfo.Update(relativePath, bundlePath, true);

                                var fileInfo = new FileInfo(bundlePath);

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

                                _level2ChunkInfo[chunkLevel].other2ChunkName[otherAssetPath] = chunkName;
                            }
                        }
                    }
                    totalChunkNum += chunkList.Count;
                }
            }

            ckeckSumInfo.SaveFB();
            return chunkNameList;
        }

        internal class CompressPathArg
        {
            public string file;
            public string targetDir;

            public CompressPathArg(string file, string targetDir)
            {
                this.file = file;
                this.targetDir = targetDir;
            }
        }

        protected void CompressChunks(string srcDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            string compressMethod = "None";
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.BUNDLE_COMPRESS_METHOD, ref compressMethod);
            BundleCompressMethod method = (BundleCompressMethod)Enum.Parse(typeof(BundleCompressMethod), compressMethod);

            switch (method)
            {
                case BundleCompressMethod.None:
                    {
                        foreach (string file in Directory.GetFiles(srcDir))
                        {
                            File.Move(file, targetDir + "/" + Path.GetFileName(file));
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            Directory.Delete(srcDir, true);
            RenameChunks(targetDir);
        }

        private void RenameChunks(string directory)
        {
            HashSet<string> chunkSet = new HashSet<string>();
            //计算块文件的Md5和CRC码
            
                foreach (string chunkPath in Directory.GetFiles(directory, "*" + ChunkExtension))
                {
                    string chunkName = Path.GetFileName(chunkPath);
                    int crc32 = CRC32Util.GetCrc32FromFile(chunkPath);
                    string name = MD5Util.GetMD5FromFile(chunkPath) + "_" + crc32.ToString("x");
                    if (!chunkSet.Add(name))
                    {
                        foreach (var level in GetAllLevels())
                        {
                            string conflictingFile1 = null;
                            string conflictingFile2 = null;
                            foreach (var chunkPair in _chunkName2FinalName)
                            {
                                if (name != chunkPair.Value)
                                    continue;
                                foreach (var pair in _level2ChunkInfo[level].other2ChunkName)
                                {
                                    if (chunkPair.Key != pair.Value)
                                        continue;
                                    conflictingFile1 = pair.Key;
                                    break;
                                }
    
                                if (conflictingFile1 == null)
                                {
                                    foreach (var pair in _level2ChunkInfo[level].bundle2ChunkName)
                                    {
                                        if (chunkPair.Key != pair.Value)
                                            continue;
                                        conflictingFile1 = pair.Key;
                                        break;
                                    }
                                }
                            }
    
                            foreach (var pair in _level2ChunkInfo[level].other2ChunkName)
                            {
                                if (chunkName != pair.Value)
                                    continue;
                                conflictingFile2 = pair.Key;
                                break;
                            }
    
                            if (conflictingFile2 == null)
                            {
                                foreach (var pair in _level2ChunkInfo[level].bundle2ChunkName)
                                {
                                    if (chunkName != pair.Value)
                                        continue;
                                    conflictingFile2 = pair.Key;
                                    break;
                                }
                            }
    
                            throw new Exception("资源中存在两个完全相同的文件, 请检查: " + conflictingFile1 + " 与 " + conflictingFile2);
                        }
                    }
                    _chunkName2FinalName[chunkName] = name;
                    _chunkName2Crc32[chunkName] = crc32;
                _chunkFinalName2Size[name + ChunkExtension] = new FileInfo(chunkPath).Length;
                File.Move(chunkPath, directory + "/" + name + ChunkExtension);
            }
        }

        private void GenerateChunkList(string targetDir)
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(targetDir, "TempName"));
            StreamWriter sw = new StreamWriter(fileInfo.Create());
            foreach (string chunkPath in Directory.GetFiles(targetDir, "*" + ChunkExtension))
            {
                sw.Write(Path.GetFileName(chunkPath));
                sw.Write("\n");
            }
            sw.Flush();
            sw.Close();
            
            _chunkListName = MD5Util.GetMD5FromFile(fileInfo.FullName) + "_" + CRC32Util.GetCrc32FromFile(fileInfo.FullName).ToString("x") + ".chunklist";
            fileInfo.MoveTo(Path.Combine(targetDir, _chunkListName));

            var parentDir = fileInfo.Directory?.Parent?.Parent?.FullName;
            if (parentDir != null)
            {
                fileInfo = fileInfo.CopyTo(Path.Combine(parentDir, "Chunk文件列表.txt"), true);
                using (sw = new StreamWriter(File.Open(fileInfo.FullName, FileMode.Append, FileAccess.Write)))
                {
                    sw.Write(_chunkListName);
                }
            }
        }

        private void GenerateBundleInfoList(string chunkPath, Dictionary<string, long> chunkName2Size)
        {
            string compressMethod = "None";
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.BUNDLE_COMPRESS_METHOD, ref compressMethod);
            BundleCompressMethod method = (BundleCompressMethod)Enum.Parse(typeof(BundleCompressMethod), compressMethod);
            SubPackageBundleInfoContainer bundleInfoContainer = new SubPackageBundleInfoContainer();

            var levels = GetAllLevels();
            foreach (string level in levels)
            {
                //统计Bundle信息
                List<SubPackageBundleInfo> bundleInfos = new List<SubPackageBundleInfo>();
                foreach (var pair in _tag2BundleSequence)
                {
                    foreach (string bundle in pair.Value)
                    {
                        string bundlePath = Path.Combine(EditorAssetBundleUtils.GetPathRoot(), bundle);
                        int crc32 = CRC32Util.GetCrc32FromFile(bundlePath);
                        try
                        {
                            bundleInfos.Add(new SubPackageBundleInfo(bundle, crc32, (uint)_bundleName2Size[bundle],
                                _chunkName2FinalName[_level2ChunkInfo[level].bundle2ChunkName[bundle]] + ChunkExtension,
                                (uint)_level2ChunkInfo[level].bundle2ChunkFrom[bundle]));
                        }
                        catch(KeyNotFoundException)
                        {
                            //Debug.Log("");
                        }
                    }
                }
                //统计OtherAsset信息
                List<SubPackageOtherInfo> otherAssetInfos = new List<SubPackageOtherInfo>();
                foreach (var pair in _tag2OtherSequence)
                {
                    foreach (string otherAsset in pair.Value)
                    {
                        var otherAssetRelativePath = otherAsset.Substring(otherAsset.IndexOf("/") + 1);
                        otherAssetInfos.Add(new SubPackageOtherInfo(otherAssetRelativePath,
                            _chunkName2FinalName[_level2ChunkInfo[level].other2ChunkName[otherAsset]] +
                            ChunkExtension));
                    }
                }
                //统计Chunk信息
                Dictionary<string, List<SubPackageChunkInfo>> tag2ChunkInfo = new Dictionary<string, List<SubPackageChunkInfo>>();
                foreach (var tag in _tagSequence)
                {
                    List<SubPackageChunkInfo> chunkList = new List<SubPackageChunkInfo>();
                    tag2ChunkInfo.Add(tag, chunkList);
                    foreach (string chunkName in _tag2ChunkName[tag])
                    {
                        if (_chunkName2Level[chunkName].Contains(level))
                        {
                            string name = _chunkName2FinalName[chunkName] + ChunkExtension;
                            chunkList.Add(new SubPackageChunkInfo(name, _chunkName2Crc32[chunkName], (uint)_chunkFinalName2Size[name], method));
                        }
                    }
                }
                bundleInfoContainer.Init(bundleInfos, otherAssetInfos, tag2ChunkInfo, _tagSequence, _chunkListName);
                bundleInfoContainer.Save(level == NormalLevel ? null : level);
                
                Debug.Log("Generate bundle info list successfully.");
            }
        }

        private void CompressExcludeAssetBundle(string locationPathName)
        {
            string subpackageNameArg = "packagename";
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.SUBPACKAGE_NAME, ref subpackageNameArg);
            string subpackageName;
            switch (subpackageNameArg.ToLower())
            {
                case "packagename":
                    string packageName = GlobalBuild.Default.packageName;
                    CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.PACKAGE_NAME, ref packageName);
                    if (packageName.Contains("."))
                    {
                        packageName = packageName.Substring(0, packageName.LastIndexOf("."));
                    }
                    subpackageName = packageName + SubpackageExtension;
                    break;
                case "chunklistmd5":
                    subpackageName = AssetManager.GetChunkListMD5() + SubpackageExtension;
                    break;
                default:
                    subpackageName = subpackageNameArg;
                    break;
            }

            string zipFileName = locationPathName + "/" + subpackageName;
            if (File.Exists(zipFileName))
            {
                File.Delete(zipFileName);
            }
            string subpackageDirectory = Path.Combine(locationPathName, SubpackageFolder);
            ZipUtil.ZipFileFromDirectory(subpackageDirectory, zipFileName);
            Directory.Delete(subpackageDirectory, true);
        }

        private void GenerateCDNUrlListFile(string outputDirectory)
        {
            string directory = Path.Combine(outputDirectory, "CdnUrlList");
            Directory.CreateDirectory(directory);
            new SubpackageEditorUtil().GenerateURLListOfSubpackage(directory, _setting.bundleLoaderSetting.remoteURL);
        }

        public void OnAfterBuild(BuildTarget target, string locationPathName)
        {
            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle && _setting.bundleLoaderSetting.useSubPackage)
            {
                string fullPath = Path.Combine(Path.GetDirectoryName(locationPathName), SubpackageFolder);
                UploadBundle(fullPath);
                if (_setting.bundleLoaderSetting.isSupportBackgroundDownload)
                {
                    _keepAliveBuildHelper.OnAfterBuild(target, locationPathName);
                }
            }
        }

        private void UploadBundle(string sourceFolder)
        {
            bool uploadBundle = false;
            CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.UPLOAD_BUNDLE, ref uploadBundle);
            if (uploadBundle)
            {
                string uploadUtilArg = "all";
                CommandLineArgs.TryGetString("UploadUtil", ref uploadUtilArg);
                if (uploadUtilArg == "All")
                {
                    foreach (string util in Enum.GetNames(typeof(UploadUtil)))
                    {
                        if (util.Equals(UploadUtil.All.ToString()))
                            continue;
                        Type currentUploadUtilType = Assembly.GetExecutingAssembly().GetType("Zeus.Framework.Asset." + util.ToString() + "UploadUtil");
                        IUploadUtil uploadUtil = Activator.CreateInstance(currentUploadUtilType) as IUploadUtil;
                        uploadUtil.UploadBundle(sourceFolder);
                    }
                }
                else
                {
                    Type currentUploadUtilType = Assembly.GetExecutingAssembly().GetType("Zeus.Framework.Asset." + uploadUtilArg + "UploadUtil");
                    if (currentUploadUtilType == null)
                    {
                        throw new Exception(string.Format("Wrong type of upload util named \"{0}\"", uploadUtilArg + "UploadUtil"));
                    }
                    IUploadUtil uploadUtil = Activator.CreateInstance(currentUploadUtilType) as IUploadUtil;
                    uploadUtil.UploadBundle(sourceFolder);
                }
            }
        }

        private void _RenameResources()
        {
            if (!_setting.bundleLoaderSetting.isRemoveResourcesRootFolder || string.IsNullOrEmpty(_setting.bundleLoaderSetting.resourcesRootFolder))
            {
                return;
            }
            string resourcesRelativePath = Path.Combine("Assets", _setting.bundleLoaderSetting.resourcesRootFolder);
            string tempResourcesRelativePath = Path.Combine(Path.GetDirectoryName(resourcesRelativePath), "." + Path.GetFileName(resourcesRelativePath));
            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle)
            {
                if (AssetDatabase.IsValidFolder(resourcesRelativePath))
                {
                    Directory.Move(resourcesRelativePath, tempResourcesRelativePath);
                }
                else
                {
                    Debug.LogError($"The folder \"{resourcesRelativePath}\" is not a valid folder");
                }
            }
        }

        private void _RestoreResources()
        {
            if (!_setting.bundleLoaderSetting.isRemoveResourcesRootFolder || string.IsNullOrEmpty(_setting.bundleLoaderSetting.resourcesRootFolder))
            {
                return;
            }
            string resourcesRelativePath = Path.Combine("Assets", _setting.bundleLoaderSetting.resourcesRootFolder);
            string tempResourcesRelativePath = Path.Combine(Path.GetDirectoryName(resourcesRelativePath), "." + Path.GetFileName(resourcesRelativePath));
            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle)
            {
                if (Directory.Exists(tempResourcesRelativePath))
                {
                    Directory.Move(tempResourcesRelativePath, resourcesRelativePath);
                }
            } 
        }

        private void RecoverSetting()
        {
            AssetManagerSetting.SaveSetting(_oriSetting);
        }

        public void OnFinallyBuild(BuildTarget target, string outputPath)
        {
            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle)
            {
                if (_setting.bundleLoaderSetting.isSupportBackgroundDownload)
                {
                    _keepAliveBuildHelper.OnFinallyBuild(target, outputPath);
                }
                _RestoreResources();
                EditorBuildSettings.scenes = EditorSettingScenesOldValue;
            }
            RecoverSetting();

            MoveBackIncludeOtherAsset();
        }

        public static void CreateSubpackage(string locationPathName, BuildTarget target)
        {
            RedundantFileCheckSumInfo.DeleteOldDataFile();
            new AssetBuildProcessor().Create(locationPathName, target);
        }

        public static List<string> GetAllLevels()
        {
            List<string> result;
            if (_setting.bundleLoaderSetting.enableAssetLevel && _setting.bundleLoaderSetting.enableAssetLevel_GenerateAll || !_setting.bundleLoaderSetting.enableAssetLevel)
            {
                result = new List<string>{NormalLevel};
            }
            else
            {
                result = new List<string>();
            }
            result.AddRange(AssetLevelManager.Instance.GetAllLevels());
            return result;
        }

        public static string GetBundleLevel(string bundleName)
        {
            if (AssetBundleUtils.GetBundleMapAssets().TryGetValue(bundleName, out var assetList))
            {
                bool hasNormalAsset = false;
                var bundleLevels = new HashSet<string>();
                foreach (var asset in assetList)
                {
                    var assetLevel = AssetLevelManager.Instance.GetLevelByAsset(asset);
                    bundleLevels.Add(assetLevel);
                    if (assetLevel == AssetLevelConfigs.DefaultMapperLevelName)
                    {
                        hasNormalAsset |= !AssetLevelManager.Instance.HasVariantAsset(asset);
                    }
                }

                if (!hasNormalAsset && bundleLevels.Count == 1)
                {
                    return bundleLevels.First();
                }
            }
            return NormalLevel;
        }

        private void Create(string locationPathName, BuildTarget target)
        {
            try
            {
                _setting = AssetManagerSetting.LoadSetting();

                AssetListHelper.Refresh();
                AssetBundleUtils.ReInit();
                List<string> includeAssetBundleList = AssetListHelper.GetFirstPackageBundleList(TagAsset.LoadTagAsset().TagListWithoutChild[0]);
                //根据不同的分包下载策略，生成不同的首包列表和二包列表，比如分模块下载策略，未统计的Bundle将打到首包内
                GenerateBundleSequenceAndFirstPackageBundleList(_setting.bundleLoaderSetting.mode, ref includeAssetBundleList, target);
                GenerateOtherSequence();
                FillFirstPackage(ref includeAssetBundleList);
                SubpackageFolder += DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff");
                string fullPath = Path.Combine(locationPathName, SubpackageFolder);
                ProcessSubpackageAssetBundle(fullPath);
                GenerateBundleInfoList(Path.Combine(fullPath, ChunkFolder), _chunkFinalName2Size);
                // if (!VFileSystem.Exists(SubPackageBundleInfoContainer.SubpackageBundleInfoPath))
                // {
                //     throw new Exception("Generate bundle info failed.");
                // }
                CompressExcludeAssetBundle(locationPathName);
                GenerateCDNUrlListFile(locationPathName);
            }
            catch (Exception)
            {
                MoveBackIncludeOtherAsset();
                throw;
            }
        }
    }
}