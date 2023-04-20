/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;


namespace Zeus.Framework.Asset
{
    public static class AssetListHelper
    {
        public const string ExtraPrefix = "[";
        private static Dictionary<string, Func<string, List<string>>> _prefix2Func;
        
        public static string BundleListPath = Application.dataPath + "/ZeusSetting/BundleList";
        private static string _parentDirectory = null;
        private static string _settingPath;

        public struct FileSequence
        {
            public string fileName;
            public bool isBundle;
            public FileSequence(string fileName, bool isBundle)
            {
                this.fileName = fileName;
                this.isBundle = isBundle;
            }
        }
        static AssetListHelper()
        {
            _settingPath = Path.Combine(Application.dataPath, SubpackageSetting.SettingPath);
            InitTag2LogFile();
            InitPrefix2Func();
        }

        [Serializable]
        private class ListClass
        {
            public List<string> list;
            public ListClass(List<string> list)
            {
                this.list = list;
            }
        }

        public static bool SaveAssetList(List<string> list, string tag, bool deleteOldLog = false)
        {
            if (!_isTag2LogFilesInit)
            {
                InitTag2LogFile();
            }
            if (string.IsNullOrEmpty(_parentDirectory))
            {
                _parentDirectory = SubpackageSetting.LoadSetting().assetListOutputPath;
            }
            if (deleteOldLog)
            {
                List<string> fileList;
                if (_tag2LogFiles.TryGetValue(tag, out fileList))
                {
                    foreach (string file in fileList)
                    {
                        File.Delete(file);
                    }
                }
            }
            string path = Path.Combine(_parentDirectory, tag + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff") + ".json");
            Zeus.Core.FileUtil.EnsureFolder(path);
            string content = JsonUtility.ToJson(new ListClass(list), true);
            File.WriteAllText(path, content);
            Debug.Log("Save successfully, file path: " + path);
 
            _isTag2LogFilesInit = false;
            return true;
        }

        public static List<string> GetFirstPackageBundleList(string firstPackageTag)
        {
            List<string> assetList = LoadAssetListFromFiles(new string[1] { firstPackageTag });
            return GetBundleListFromAssetList(assetList);
        }

        static string[] _files;
        static Dictionary<string, List<string>> _tag2LogFiles;
        static bool _isTag2LogFilesInit = false;
        
        public static void Refresh()
        {
            _isTag2LogFilesInit = false;
        }

        public static List<string> LoadAssetListFromFiles(string[] tags, bool isParsing = true)
        {
            if (tags.Length == 1)
            {
                var childTags = TagAsset.LoadTagAsset().GetChildNames(tags[0]);
                if (childTags != null)
                {
                    tags = childTags;
                }
            }
            
            //如果要计算的tag中包含首包 则同时计算标记进首包的tag; 不包含则去除标记进首包的tag中的资源
            if (isParsing)
            {
                var tagsAddToFirstPackage = TagAsset.LoadTagAsset().tagsAddToFirstPackage;
                var addedTags = new HashSet<string>();
                bool withFirstPackageTag = false;
                foreach (var tag in tags)
                {
                    addedTags.Add(tag);
                    if (tag == AssetBundleLoaderSetting.FirstPackageTag)
                    {
                        foreach (var fpTag in tagsAddToFirstPackage)
                        {
                            addedTags.Add(fpTag);
                        }
                        withFirstPackageTag = true;
                    }
                }
                if (!withFirstPackageTag)
                {
                    foreach (var fpTag in tagsAddToFirstPackage)
                    {
                        addedTags.Remove(fpTag);
                    }
                }
                tags = addedTags.ToArray();
            }

           
            if(!_isTag2LogFilesInit)
            {
                InitTag2LogFile();
            }
            if (File.Exists(_settingPath))
            {
                string settingContent = File.ReadAllText(_settingPath);
                _parentDirectory = UnityEngine.JsonUtility.FromJson<SubpackageSetting>(settingContent).assetListOutputPath;
            }
            if (string.IsNullOrEmpty(_parentDirectory))
            {
                Debug.LogError("Please set assetlsit output path in Zeus->Setting->Asset->Subpackage->资源统计");
                return new List<string>();
            }
            if(!Directory.Exists(_parentDirectory))
            {
                Debug.LogError("There isn't any asset log file.");
                return new List<string>();
            }
            List<string> assetList = new List<string>();
            HashSet<string> assetHashSet = new HashSet<string>();
            List<List<string>> assetListList = new List<List<string>>();
            int maxLength = 0;
            if (_files.Length > 0)
            {
                foreach (string tag in tags)
                {
                    if (tag == AssetBundleLoaderSetting.OthersTag)
                    {
                        AssetBundleUtils.ReInit();
                        var allAssets = AssetBundleUtils.GetAssetMapBundles().Keys;
                        List<string> tagsForRecord;
                        if (SubpackageWindow.TagAsset != null)
                        {
                            tagsForRecord = SubpackageWindow.TagAsset.TagListForRecord;
                        }
                        else
                        {
                            tagsForRecord = TagAsset.LoadTagAsset().TagListForRecord;
                        }
                        tagsForRecord.Remove(AssetBundleLoaderSetting.OthersTag);
                        var recordedAssetList = LoadAssetListFromFiles(tagsForRecord.ToArray());

                        assetList.AddRange(allAssets.Except(recordedAssetList).ToList());
                        // var recordedBundleList = GetBundleListFromAssetList(recordedAssetList);
                        //
                        // var assetFromBundle = new List<string>();
                        // var bundleMapAssets = AssetBundleUtils.GetBundleMapAssets();
                        // foreach (var bundle in recordedBundleList)
                        // {
                        //     List<string> currentAsset;
                        //     if (bundleMapAssets.TryGetValue(bundle, out currentAsset))
                        //     {
                        //         assetFromBundle.AddRange(currentAsset);
                        //     }
                        // }
                        //
                        // var subpackageAssetSet = new HashSet<string>(assetFromBundle);
                        // foreach (var asset in allAssets)
                        // {
                        //     if (!subpackageAssetSet.Contains(asset))
                        //     {
                        //         assetList.Add(asset);
                        //     }
                        // }
                    }
                    else
                    {
                        List<string> fileList;
                        if(_tag2LogFiles.TryGetValue(tag, out fileList))
                        {
                            foreach (string file in fileList)
                            {
                                string content = File.ReadAllText(file);
                                ListClass listClass = JsonUtility.FromJson<ListClass>(content);
                                if (listClass.list.Count > maxLength)
                                {
                                    maxLength = listClass.list.Count;
                                }
                                assetListList.Add(listClass.list);
                            }
                            for (int i = 0; i < maxLength; i++)
                            {
                                for (int j = 0; j < assetListList.Count; j++)
                                {
                                    if (i < assetListList[j].Count)
                                    {
                                        if (assetHashSet.Add(assetListList[j][i]))
                                        {
                                            assetList.Add(assetListList[j][i]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("No asset list file.");
                return new List<string>();
            }

            if (isParsing)
            {
                assetList = ParsingAssetList(assetHashSet, assetList);
            }
            return assetList;
        }
        
        
        private static List<string> ParsingAssetList(HashSet<string> assetHashSet, List<string> assetList)
        {
            var parsedAssetList = new List<string>();
            foreach (var current in assetList)
            {
                if (current.StartsWith(ExtraPrefix))
                {
                    foreach (var pair in _prefix2Func.Where(pair => current.StartsWith(pair.Key)))
                    {
                        parsedAssetList.AddRange(pair.Value(current).Where(assetHashSet.Add));
                        break;
                    }
                }
                else
                {
                    parsedAssetList.Add(current);
                }
            }
            return parsedAssetList;
        }
        
        private static List<string> RegexTranslate(string fullStr)
        {
            List<string> result = new List<string>();
            
            string regexStr;
            try
            {
                regexStr = fullStr.Split('#')[1];
            }
            catch (Exception)
            {
                throw new Exception("regex模式结构错误, 请检查 " + fullStr);
            }
            var directoryPath = fullStr.Split('#')[0].Substring(7);
            if (Directory.Exists(directoryPath))
            {
                Regex regex = new Regex(regexStr);
                foreach (string file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(file) == ".meta" || Path.GetExtension(file) == ".cs")
                    {
                        continue;
                    }

                    if (!regex.Match(Path.GetFileName(file)).Success)
                    {
                        continue;
                    }

                    string assetPath = AssetPathHelper.GetShortPath(file);
                    result.Add(assetPath);
                }
            }
            return result;
        }

        private static List<string> DirectoryTranslate(string current)
        {
            List<string> result = new List<string>();

            var directoryPath = current.Substring(11);
            if (Directory.Exists(directoryPath))
            {
                foreach (string file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(file) == ".meta" || Path.GetExtension(file) == ".cs")
                    {
                        continue;
                    }

                    string assetPath = AssetPathHelper.GetShortPath(file);
                    result.Add(assetPath);
                }
            }
            return result;
        }

        private static void InitTag2LogFile()
        {
            if (string.IsNullOrEmpty(_parentDirectory))
            {
                _parentDirectory = SubpackageSetting.LoadSetting().assetListOutputPath;
            }
            if (!Directory.Exists(_parentDirectory))
            {
                Directory.CreateDirectory(_parentDirectory);
            }
            _files = Directory.GetFiles(_parentDirectory);
            _tag2LogFiles = new Dictionary<string, List<string>>();
            foreach (string file in _files)
            {
                string fileName = Path.GetFileName(file);
                int index = fileName.IndexOf('_');
                if (index == -1)
                {
                    Debug.LogError($"The name of file \"{file}\" is invalid.");
                    continue;
                }
                string tag = fileName.Substring(0, index);
                List<string> fileList;
                if (!_tag2LogFiles.TryGetValue(tag, out fileList))
                {
                    fileList = new List<string>();
                    _tag2LogFiles.Add(tag, fileList);
                }
                fileList.Add(file);
            }
            _isTag2LogFilesInit = true;
        }

        private static void InitPrefix2Func()
        {
            _prefix2Func = new Dictionary<string, Func<string, List<string>>>
            {
                {"[regex]", RegexTranslate},
                {"[directory]", DirectoryTranslate}
            };
            
            CustomAssetSign assetSign = null;
            var assembly = Assembly.Load("Assembly-CSharp-Editor");
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.BaseType != typeof(CustomAssetSign))
                    continue;
                assetSign = Activator.CreateInstance(type) as CustomAssetSign;
                break;
            }
            if (assetSign != null)
            {
                _prefix2Func.Add(assetSign.Prefix, assetSign.GetAssetList);
            }
        }

        public static List<string> GetBundleListFromAssetList(List<string> assetList)
        {
            List<string> bundleList = new List<string>();
            HashSet<string> bundleHashSet = new HashSet<string>();
            foreach (string path in assetList)
            {
                string abName;
                string assetName;
                if (path.StartsWith(TagDetailWindow.STREAMING_ASSET_PATH)) //跳过来自StreamingAsset的资源
                {
                    continue;
                }
                if (!AssetBundleUtils.TryGetAssetBundleName(path, out abName, out assetName))
                {
                    Debug.LogError("AssetFromBundleLoader:Not Found path in ab:" + path);
                }
                else
                {
                    if (bundleHashSet.Add(abName))
                    {
                        bundleList.Add(abName);
                    }
                    foreach (string dependency in AssetBundleUtils.GetAllDependencies(abName))
                    {
                        if (bundleHashSet.Add(dependency))
                        {
                            bundleList.Add(dependency);
                        }
                    }
                }
            }
            return bundleList;
        }
        
        public static List<string> GetOtherAssetListFromAssetList(List<string> assetList)
        {
            List<string> otherAssetList = new List<string>();
            HashSet<string> otherAssetHashSet = new HashSet<string>();
            foreach (string path in assetList)
            {
                if (!path.StartsWith(TagDetailWindow.STREAMING_ASSET_PATH)) //跳过不来自StreamingAsset的资源
                {
                    continue;
                }
                if (!File.Exists(Path.Combine(Application.dataPath, path)))
                {
                    Debug.LogError("AssetFromStreamingLoader:Not Found The Asset From:" + path);
                }
                else
                {
                    if (otherAssetHashSet.Add(path))
                    {
                        otherAssetList.Add(path);
                    }
                }
            }
            return otherAssetList;
        }

        public static List<string> GetAllFileFromAssetList(List<string> assetList)
        {
            List<string> fileList = new List<string>();
            HashSet<string> fileHashSet = new HashSet<string>();
            foreach (string path in assetList)
            {
                string abName;
                if (path.StartsWith(TagDetailWindow.STREAMING_ASSET_PATH))
                {
                    if (!File.Exists(Path.Combine(Application.dataPath, path)))
                    {
                        Debug.LogError("AssetFromStreamingLoader:Not Found The Asset From:" + path);
                    }
                    else
                    {
                        if (fileHashSet.Add(path))
                        {
                            fileList.Add(path);
                        }
                    }
                }
                else
                {
                    if (!AssetBundleUtils.TryGetAssetBundleName(path, out abName, out _))
                    {
                        Debug.LogError("AssetFromBundleLoader:Not Found path in ab:" + path);
                    }
                    else
                    {
                        if (fileHashSet.Add(abName))
                        {
                            fileList.Add(abName);
                        }
                        foreach (string dependency in AssetBundleUtils.GetAllDependencies(abName))
                        {
                            if (fileHashSet.Add(dependency))
                            {
                                fileList.Add(dependency);
                            }
                        }
                    }
                }
            }
            return fileList;
        }

        /// <summary>
        /// 根据AssetList获取排除掉excludeSet中Bundle的BundleList，常用于获取二包Bundle时排除首包Bundle
        /// </summary>
        /// <param name="assetList"></param>
        /// <param name="excludeSet">需要排除掉的Bundle集合</param>
        /// <returns></returns>
        public static List<string> GetBundleSequenceFromAssetList(List<string> assetList, HashSet<string> excludeSet)
        {
            var bundleSequence = GetBundleListFromAssetList(assetList);

            var finalList = new List<string>();
            foreach (var bundleName in bundleSequence)
            {
                if (!excludeSet.Contains(bundleName))
                {
                    finalList.Add(bundleName);
                }
            }
            return finalList;
        }
        
        /// <summary>
        /// 根据AssetList获取排除掉excludeSet中Bundle的BundleList,同时包括非Bundle资源
        /// </summary>
        /// <param name="assetList"></param>
        /// <param name="excludeSet">需要排除掉的Bundle集合</param>
        /// <returns></returns>
        public static List<FileSequence> GetAllSequenceFromAssetList(List<string> assetList, HashSet<string> excludeSet)
        {
            var allSequence = GetAllFileFromAssetList(assetList);

            List<FileSequence> assetSequence = new List<FileSequence>();
            foreach (var file in allSequence)
            {
                if (!excludeSet.Contains(file))
                {
                    assetSequence.Add(new FileSequence(file, !file.StartsWith(TagDetailWindow.STREAMING_ASSET_PATH)));
                }
            }
            return assetSequence;
        }

        /// <summary>
        /// 获取ExcludeList存储的Bundle之外的Bundle列表，可用于获取未被记录到的Bundle列表
        /// </summary>
        /// <param name="tag2BundleSequence"></param>
        /// <param name="excludeList"></param>
        /// <returns></returns>
        public static List<string> GetUnrecordedBundleList(List<string> excludeList)
        {
            HashSet<string> unrecordedBundleSet = new HashSet<string>(AssetBundleUtils.GetAllAssetBundles());
            foreach (string bundle in excludeList)
            {
                unrecordedBundleSet.Remove(bundle);
            }
            return new List<string>(unrecordedBundleSet);
        }

        public static List<string> GetSharedBundlesFormTags(string[] tags, Dictionary<string, List<string>> tag2Bundles)
        {
            List<string> sharedBundles = new List<string>();
            Dictionary<string, int> bundle2Count = new Dictionary<string, int>();
            if (tags.Length > 1)
            {
                foreach (string tag in tags)
                {
                    List<string> bundles = tag2Bundles[tag];
                    foreach (string bundle in bundles)
                    {
                        int count;
                        if (bundle2Count.TryGetValue(bundle, out count))
                        {
                            if (count == 1)
                            {
                                bundle2Count[bundle]++;
                                sharedBundles.Add(bundle);
                            }
                        }
                        else
                        {
                            bundle2Count.Add(bundle, 1);
                        }
                    }
                }
            }
            return sharedBundles;
        }
    }
}