using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public enum BuildPlatform
    {
        IOS = 0x0001,
        Android = 0x0002,
        PC = 0x0004,
    }

    public enum BundleType
    {
        INVALID_ASSET,
        SINGLE_BUNDLE,
        REFERENCE_ASSET,
        PACKAGE_BUNDLE,
        SCENE_REDIRECT_ASSET,
        CONFIG_REDIRECT_ASSET,
        SCENE_ASSET,
        EXPORT_ASSET,
        PREFAB_REDIRECT_ASSET,
        PREFAB_ASSET,
        NUM
    }

    public class BuildBundleContext
    {
        public HashSet<string> pathKey = new HashSet<string>();
        public HashSet<string> physicPathKey = new HashSet<string>();
        public Dictionary<string, List<string>> packageBundle = new Dictionary<string, List<string>>();
        public BundleLogger bundleLogger = new BundleLogger();
        public int count = 0;
        public void Clear()
        {
            pathKey.Clear();
            physicPathKey.Clear();
            packageBundle.Clear();
            bundleLogger.Clear();
            count = 0;
        }
    }

    public delegate void BuildBundleRuleCb(BuildBundleContext context, string path, BundlePath bp);
    [Serializable]
    public class BundleRule
    {
        public string path = "";
        public BundleType op = BundleType.INVALID_ASSET;
        public uint buildPlatform = uint.MaxValue;
        public static BuildBundleRuleCb[] ruleCb = new BuildBundleRuleCb[(int)BundleType.NUM]
        {
            BBR_InvalidAsset,
            BBR_SingleAsset,
            BBR_SingleAsset,
            BBR_PackageAsset,
            BBR_SceneRedirectAsset,
            BBR_ConfigRedirectAsset,
            BBR_SceneAsset,
            BBR_ExportAsset,
            BBR_PrefabRedirectAsset,
            BBR_PrefabAsset,
        };

        private static void BBR_InvalidAsset(BuildBundleContext context, string path, BundlePath bp)
        {
            context.bundleLogger.AddInvalidFile(path);
        }

        private static void BBR_SingleAsset(BuildBundleContext context, string path, BundlePath bp)
        {
            bp.AddBundle(context, path, path);
        }

        private static void BBR_ExportAsset(BuildBundleContext context, string path, BundlePath bp)
        {
            var preFix = LoadMgr.singleton.BundlePath;
            int index = path.IndexOf(preFix);
            if (index < 0)
            {
                preFix = LoadMgr.singleton.EngineResPath.ToLower();
                index = path.IndexOf(preFix);
            }
            else
            {

            }
            if (index >= 0)
            {
                index += preFix.Length;
                string relativeSrc = path.Substring(index);
                string src = string.Format("{0}{1}", preFix, relativeSrc);
                string des = string.Format("Assets/StreamingAssets/Bundles/assets/bundleres/{0}", relativeSrc);
                des = des.Replace("\\", "/");
                index = des.LastIndexOf("/");
                if (index >= 0)
                {
                    string dir = des.Substring(0, index);
                    EditorCommon.CreateDir(dir);
                }
                if (File.Exists(src))
                {
                    if ((BuildBundleConfig.buildType & BuildType.PreBuild) != 0)
                        File.Copy(src, des, true);
                    DebugLog.AddEngineLog2("copy file from {0} to {1}", src, des);
                }
                else
                {
                    DebugLog.AddErrorLog2("file not exist:{0}", src);
                }
            }
            else
            {
                DebugLog.AddErrorLog2("path not valid:{0}", path);
            }

        }

        private static void BBR_PackageAsset(BuildBundleContext context, string path, BundlePath bp)
        {
            if (bp.assetNames == null)
            {
                bp.assetNames = new List<string>();
            }
            bp.assetNames.Add(path);
            context.bundleLogger.AddBundleNameList(path, path);
        }

        private static void BBR_SceneRedirectAsset(BuildBundleContext context, string path, BundlePath bp)
        {
            try
            {
                string sceneName = Path.GetFileNameWithoutExtension(path);
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    BinaryReader br = new BinaryReader(fs);
                    br.ReadInt32();//version
                    br.ReadInt32(); //headLength
                    br.ReadInt32(); //chunkCount
                    br.ReadInt32(); //splatCount                    
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; ++i)
                    {
                        string resName = br.ReadString();
                        string physicDir = br.ReadString();
                        int dirType = br.ReadInt32();
                        physicDir = physicDir.Replace("\\", "/");
                        string physicPath = (physicDir + resName);
                        if (File.Exists(physicPath))
                        {
                            string virtualPath = "";
                            if (dirType == SceneSerialize.ReDirectRes.LogicPath_Common)
                            {
                                virtualPath = string.Format("{0}{1}{2}",
                                    LoadMgr.singleton.BundlePath,
                                    LoadMgr.singleton.runtimeResDir, resName);
                            }
                            else if (dirType == SceneSerialize.ReDirectRes.LogicPath_SceneRes)
                            {
                                virtualPath = string.Format("{0}Scene/{1}/{2}",
                                    LoadMgr.singleton.BundlePath,
                                    sceneName, resName);
                            }

                            virtualPath = virtualPath.ToLower();
                            bp.AddBundle(context, virtualPath, physicPath);
                        }
                        else
                        {
                            DebugLog.AddErrorLog2("res not exist:{0}\r\nscene:{1}", physicPath, path);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                DebugLog.AddErrorLog(e.Message + path);
            }

        }
        private static void BBR_ConfigRedirectAsset(BuildBundleContext context, string path, BundlePath bp)
        {
            ResRedirectConfig config = AssetDatabase.LoadAssetAtPath<ResRedirectConfig>(path);
            if (config != null)
            {
                for (int i = 0; i < config.resPath.Count; ++i)
                {
                    var rp = config.resPath[i];
                    if (rp.ext != ".bytes" && rp.buildBundle)
                    {
                        string physicPath = rp.physicPath + rp.name + rp.ext;
                        if (File.Exists(physicPath))
                        {
                            string name = "";
                            if (string.IsNullOrEmpty(rp.targetPath))
                            {

                                name = string.Format("{0}{1}{2}{3}", LoadMgr.singleton.BundlePath,
                                    LoadMgr.singleton.runtimeResDir, rp.name, rp.ext);
                            }
                            else
                            {
                                name = string.Format("{0}{1}{2}{3}", LoadMgr.singleton.BundlePath,
                                    rp.targetPath, rp.name, rp.ext);
                            }
                            bp.AddBundle(context, name, physicPath.ToLower());
                        }
                    }

                }

            }
        }
        private static void BBR_SceneAsset(BuildBundleContext context, string path, BundlePath bp)
        {
            int index = path.LastIndexOf('.');
            bp.AddBundle(context, path.Substring(0, index), path);

        }
        private static void BBR_PrefabRedirectAsset(BuildBundleContext context, string path, BundlePath bp)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    BinaryReader br = new BinaryReader(fs);
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; ++i)
                    {
                        string resName = br.ReadString();
                        string physicDir = br.ReadString();
                        int dirType = br.ReadInt32();
                        physicDir = physicDir.Replace("\\", "/");
                        string physicPath = (physicDir + resName);
                        if (File.Exists(physicPath))
                        {
                            string virtualPath = string.Format("{0}{1}{2}",
                                LoadMgr.singleton.BundlePath,
                                LoadMgr.singleton.runtimeResDir, resName);

                            virtualPath = virtualPath.ToLower();
                            bp.AddBundle(context, virtualPath, physicPath);
                        }
                        else
                        {
                            DebugLog.AddErrorLog2("res not exist:{0}\r\nscene:{1}", physicPath, path);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                DebugLog.AddErrorLog(e.Message + path);
            }
        }

        private static void BBR_PrefabAsset(BuildBundleContext context, string path, BundlePath bp)
        {
            var config = AssetDatabase.LoadAssetAtPath<EditorPrefabData>(path);
            if (config != null)
            {
                for (int i = 0; i < config.prefabFolder.Count; ++i)
                {
                    var pf = config.prefabFolder[i];
                    for (int j = 0; j < pf.configs.Count; ++j)
                    {
                        var c = pf.configs[j];
                        string physicPath = null;
                        if (c.des != null)
                        {
                            physicPath = AssetDatabase.GetAssetPath(c.des);
                        }
                        else if (c.src != null)
                        {
                            physicPath = AssetDatabase.GetAssetPath(c.src);
                        }
                        if (!string.IsNullOrEmpty(physicPath))
                        {
                            bp.AddBundle(context, physicPath, physicPath);
                        }

                    }
                }
            }
        }
    }

    [Serializable]
    public class BundlePath : BaseFolderHash
    {
        public string dir;
        public string assetBundleName;
        [NonSerialized]
        public List<string> assetNames;
        public List<BundleRule> rules = new List<BundleRule>();
        public bool enable = true;
        [NonSerialized]
        public List<AssetBundleBuild> bundleList = new List<AssetBundleBuild>();
        [NonSerialized]
        public bool bundleFolder = false;
        public void PreBuild(BuildBundleContext context)
        {
            if (assetNames != null)
            {
                assetNames.Clear();
            }
            bundleList.Clear();
        }

        public void PostBuild(BuildBundleContext context)
        {
            if (!string.IsNullOrEmpty(assetBundleName) &&
                assetNames != null && assetNames.Count > 0)
            {
                if (!context.packageBundle.TryGetValue(assetBundleName, out List<string> names))
                {
                    names = new List<string>();
                    context.packageBundle.Add(assetBundleName, names);
                }
                names.AddRange(assetNames);
                //bundleList.Add (new AssetBundleBuild ()
                //{
                //    assetBundleName = LoadMgr.singleton.BundlePath + assetBundleName,
                //        assetNames = assetNames.ToArray ()
                //});
            }
        }

        public void AddBundle(BuildBundleContext context, string name, string path)
        {
            if (!context.pathKey.Contains(name))
            {
                string physicPathLow = path.ToLower();
                if (!context.physicPathKey.Contains(physicPathLow))
                {
                    AssetBundleBuild build = new AssetBundleBuild();

                    build.assetBundleName = name;
                    build.assetNames = new string[] { path };
                    build.addressableNames = new string[] { "default" };
                    bundleList.Add(build);
                    context.count++;
                    context.bundleLogger.AddBundleNameList(name, path);
                    context.pathKey.Add(name);
                    context.physicPathKey.Add(physicPathLow);
                }
                else
                {
                    DebugLog.AddErrorLog2("duplicate path:{0}\r\nres:{1}", name, path);
                }

            }
        }
    }

    public class ReportItem
    {
        public string name = "";
        public long sizeInByte = 0;
    }
    public class ReportGroup
    {
        public int groupType = 0;
        public long sizeInByte = 0;
        public List<ReportItem> items = new List<ReportItem>();
    }
    public enum BuildType
    {
        TestBulld = 0,
        PreBuild = 0x0001,
        ABBuild = 0x0002,
        BuildAll = PreBuild | ABBuild
    }
    public class EditorAbInfo
    {
        public string abName;
        public string fullPath;
        public FlagMask flag;
        public List<int> childAbIndex;
        public int folderIndex = -1;
    }
    public class BuildBundleConfig : AssetBaseConifg<BuildBundleConfig>
    {
        public List<BundlePath> configs = new List<BundlePath>();
        public List<string> outPackagePath = new List<string>();
        public BuildBundleContext context = new BuildBundleContext();
        public List<PreBuildPreProcess> buildProcess = new List<PreBuildPreProcess>();
        public Dictionary<string, ReportGroup> resReport = new Dictionary<string, ReportGroup>();
        public long timeHash = 0;

        public static BuildType buildType = BuildType.TestBulld;

        // 打ab之前执行
        public void OnPrebuildBundle(string preBuildName)
        {
            AssetDatabase.StartAssetEditing();

            PreBuildPreProcess.count = 0;
            buildProcess.Clear();
            var types = EngineUtility.GetAssemblyType(typeof(PreBuildPreProcess));
            foreach (var t in types)
            {
                var process = Activator.CreateInstance(t) as PreBuildPreProcess;
                if (process != null)
                {
                    buildProcess.Add(process);
                }
            }
            buildProcess.Sort((x, y) => x.Priority - y.Priority);
            foreach (var bp in buildProcess)
            {
                if (string.IsNullOrEmpty(preBuildName) ||
                    bp.GetType().Name == preBuildName)
                    using (bp)
                        bp.PreProcess();
            }
            if (PreBuildPreProcess.build)
            {
                //timeHash = DateTime.Now.ToFileTime();
                timeHash = 1;
                string versionPath = string.Format("{0}/Version/PackageBytesVersion.bytes",
                    AssetsConfig.instance.ResourcePath);
                if (File.Exists(versionPath))
                    AssetDatabase.DeleteAsset(versionPath);
                using (FileStream fs = new FileStream(versionPath, FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(timeHash);
                }
                AssetDatabase.ImportAsset(versionPath, ImportAssetOptions.ForceUpdate);
            }
            AssetDatabase.StopAssetEditing();


        }

        private void BuildBundle(BundlePath bp, DirectoryInfo di)
        {
            foreach (var subDi in di.GetDirectories("*.*", SearchOption.TopDirectoryOnly))
            {
                string name = subDi.Name.ToLower();

                if (name.Contains("editor")) continue;
                if (name.Contains("test")) continue;
                if (name.Contains("testignore")) continue;

                BuildBundle(bp, subDi);
            }

            foreach (var fi in di.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(p => !p.Extension.Equals(".meta")))
            {
                string name = fi.Name.ToLower();

                if (name.Contains("test")) continue;
                if (name.Contains("testignore")) continue;

                string fullName = fi.FullName.ToLower();
                fullName = fullName.Replace('\\', '/');
                int index = fullName.IndexOf("assets/");
                fullName = fullName.Substring(index);

                foreach (var rule in bp.rules)
                {
                    if (fullName.EndsWith(rule.path.ToLower()) && !context.pathKey.Contains(fullName))
                    {
                        var ruleCb = BundleRule.ruleCb[(int)rule.op];
                        if (ruleCb == null) continue;

                        bool build = false;
#if UNITY_IOS
                        build = (rule.buildPlatform & (uint)BuildPlatform.IOS) != 0;
#elif UNITY_ANDROID
                        build = (rule.buildPlatform & (uint) BuildPlatform.Android) != 0;
#else
                        build = (rule.buildPlatform & (uint)BuildPlatform.PC) != 0;
#endif

                        if (build)
                        {
                            ruleCb(context, fullName, bp);
                        }

                        context.pathKey.Add(fullName);
                    }
                }
            }
        }

        public static void RecordTime(string str)
        {
            var now = System.DateTime.Now;
            DebugLog.AddEngineLog2("======================{0}:{1}-{2}-{3}_{4}-{5}-{6}======================",
                    str,
                    now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        }
        public List<AssetBundleBuild> BuildBundle(string preBuildName = "", int configIndex = -1,
            BuildType buildType = BuildType.BuildAll,
            bool removeManifest = true, bool buildSingle = false,bool multiProcess=false)
        {
            RecordTime("StartBuildAB");
            AssetsImporter.skipAutoImport = true;
            BuildBundleConfig.buildType = buildType;
            context.Clear();
            context.bundleLogger.quiet = true;
            bool needCopyManifest = false;
            string spath = buildSingle ? "Assets/StreamingAssets/TestBundles" : "Assets/StreamingAssets/Bundles";
            if (!AssetDatabase.IsValidFolder(spath))
            {
                if (buildSingle)
                {
                    AssetDatabase.CreateFolder("Assets/StreamingAssets", "TestBundles");
                }
                else
                {
                    AssetDatabase.CreateFolder("Assets/StreamingAssets", "Bundles");
                }

            }

            if (buildType != BuildType.TestBulld)
            {
                if ((buildType & BuildType.PreBuild) != 0 ||
                    buildType == BuildType.TestBulld)
                {
                    PreBuildPreProcess.build = buildType != BuildType.TestBulld;

                    OnPrebuildBundle(preBuildName);

                    RecordTime("EndPreBuildAB");
                }
                else
                {
                    PreBuildPreProcess.build = false;
                }
            }

            void run(BundlePath bp)
            {
                if (!bp.enable) return;

                DirectoryInfo di = new DirectoryInfo(bp.dir);
                bp.PreBuild(context);
                BuildBundle(bp, di);
                bp.PostBuild(context);
            };

            if (configIndex >= 0 && configIndex < configs.Count)
            {
                run(configs[configIndex]);
            }
            else
            {
                foreach (var bp in configs)
                {
                    run(bp);
                    needCopyManifest |= !bp.enable;
                }
            }

            List<AssetBundleBuild> bundleList = new List<AssetBundleBuild>();
            foreach (var kvp in context.packageBundle)
            {
                if (!context.pathKey.Contains(kvp.Key))
                {
                    AssetBundleBuild abb = new AssetBundleBuild();
                    abb.assetBundleName = LoadMgr.singleton.BundlePath + kvp.Key;
                    abb.assetNames = kvp.Value.ToArray();
                    if (kvp.Value.Count == 1)
                    {
                        abb.addressableNames = new string[] { "default" };
                    }
                    bundleList.Add(abb);
                    context.count++;
                    context.bundleLogger.AddBundleNameList(kvp.Key, kvp.Key);
                    context.pathKey.Add(kvp.Key);
                }
            }

            for (int i = 0; i < configs.Count; ++i)
            {
                var bp = configs[i];
                if (string.IsNullOrEmpty(bp.assetBundleName))
                    bundleList.AddRange(bp.bundleList);
            }

            if ((buildType & BuildType.ABBuild) != 0)
            {
                // OnPrebuildBundle (index == -1 ? build : false);
                
                RecordTime("StartPostBuildAB");
                object manifestAsset = null;
                if (multiProcess)
                {
                    manifestAsset = MultiProcessBuild.BuildPipeline.BuildAssetBundles(spath, bundleList.ToArray(),
                       BuildAssetBundleOptions.ChunkBasedCompression |
                       BuildAssetBundleOptions.DeterministicAssetBundle,
                       EditorUserBuildSettings.activeBuildTarget);
                }
                else
                {
                    manifestAsset = BuildPipeline.BuildAssetBundles(spath, bundleList.ToArray(),
                       BuildAssetBundleOptions.ChunkBasedCompression |
                       BuildAssetBundleOptions.DeterministicAssetBundle,
                       EditorUserBuildSettings.activeBuildTarget);
                }
                EditorUtility.ClearProgressBar();
                AssetDatabase.RemoveUnusedAssetBundleNames();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                //if (removeManifest)
                //{
                //    try
                //    {
                //        DirectoryInfo di = new DirectoryInfo(spath);
                //        FileInfo[] files = di.GetFiles("*.manifest", SearchOption.AllDirectories);

                //        for (int i = 0; i < files.Length; ++i)
                //        {
                //            if (files[i].Name != "Bundles.manifest")
                //                files[i].Delete();
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        DebugLog.AddErrorLog(e.Message);
                //    }
                //}

                EditorUtility.DisplayDialog("build bundle finish", " build finish", "OK");
                string configPath = "Assets/StreamingAssets/Bundles/assets/bundleres";
                // context.bundleLogger.BuildLog ()
                RecordTime("StartPostBuild");
                BuildReport(configPath);
                GenManifestData(manifestAsset);
            }
            AssetsImporter.skipAutoImport = false;

            RecordTime("EndBuildAB");
            return bundleList;
        }

        private void ProcessABDep(EditorAbInfo abInfo,
            Dictionary<string, string[]> depMap,
            Dictionary<string, int> abMap)
        {
            if (depMap.TryGetValue(abInfo.fullPath, out var depStrs))
            {
                for (int j = 0; j < depStrs.Length; j++)
                {
                    var depStr = depStrs[j];
                    var depBundlePath = depStr.Replace(cutString, String.Empty);
                    if (depBundlePath != "cfshader" && depBundlePath != "cfpostprocess")
                    {
                        if (abMap.TryGetValue(depBundlePath, out var index))
                        {
                            abInfo.childAbIndex.Add(index);
                        }
                        else
                        {
                            DebugLog.AddErrorLog2("dep ab not find {0}", depBundlePath);
                        }
                    }
                }
            }
        }

        private string[] GetAllAssetBundles(object manifestAsset)
        {
            if(manifestAsset is AssetBundleManifest)
            {
                return (manifestAsset as AssetBundleManifest).GetAllAssetBundles();
            }
            else if(manifestAsset is MultiProcessBuild.AssetBundleManifest)
            {
                return (manifestAsset as MultiProcessBuild.AssetBundleManifest).GetAllAssetBundles();
            }

            return null;
        }
        private string[] GetAllDependencies(object manifestAsset,string path)
        {
            if (manifestAsset is AssetBundleManifest)
            {
                return (manifestAsset as AssetBundleManifest).GetAllDependencies(path);
            }
            else if (manifestAsset is MultiProcessBuild.AssetBundleManifest)
            {
                return (manifestAsset as MultiProcessBuild.AssetBundleManifest).GetAllDependencies(path);
            }

            return null;
        }
        static string cutString = "assets/bundleres/";
        public void GenManifestData(object manifestAsset)
        {
            if (manifestAsset != null)
            {


                string configPath = "Assets/StreamingAssets/Bundles/assets/bundleres";

                string target = string.Format("{0}/{1}.bytes", configPath);//, BundleMgr.ManifestFilePath);
                string[] all_bundles = GetAllAssetBundles(manifestAsset);
                try
                {
                    List<EditorAbInfo> abDepList = new List<EditorAbInfo>();
                    List<EditorAbInfo> abNoDepList = new List<EditorAbInfo>();
                    List<EditorAbInfo> abNoDepLargeList = new List<EditorAbInfo>();//dep more than 255
                    List<EditorAbInfo> abTotalList = new List<EditorAbInfo>();
                    Dictionary<string, int> abMap = new Dictionary<string, int>();
                    Dictionary<string, string[]> depMap = new Dictionary<string, string[]>();
                    List<string> folderPath = new List<string>();
                    List<string> outPackagePathCopy = new List<string>();

                    //on android exclude package path
#if UNITY_ANDROID
                    outPackagePathCopy.AddRange(outPackagePath);
#endif

                    for (int i = 0; i < all_bundles.Length; i++)
                    {
                        var path = all_bundles[i];
                        var bundlePath = path.Replace(cutString, String.Empty);
                        var abInfo = new EditorAbInfo()
                        {
                            fullPath = bundlePath,
                        };

                        //analyze dir info, compress data
                        int folderIndex = -1;
                        int index = bundlePath.LastIndexOf("/");
                        if (index > 0)
                        {
                            string folder = bundlePath.Substring(0, index);
                            folderIndex = folderPath.IndexOf(folder);
                            if (folderIndex == -1)
                            {
                                folderIndex = folderPath.Count;
                                folderPath.Add(folder);
                            }
                            bundlePath = bundlePath.Substring(index + 1);
                        }
                        abInfo.abName = bundlePath;
                        abInfo.folderIndex = folderIndex;

                        //in or out package on android
                        // bool inPackage = true;
                        // for (int j = 0; j < outPackagePathCopy.Count; ++j)
                        // {
                        //     if (!string.IsNullOrEmpty(outPackagePathCopy[j]) &&
                        //         abInfo.fullPath.StartsWith(outPackagePathCopy[j]))
                        //     {
                        //         inPackage = false;
                        //         break;
                        //     }
                        // }
                        //abInfo.flag.SetFlag(AbInfo.Flag_InPackage, inPackage);

                        //analyze dependency
                        string[] dependencies = GetAllDependencies(manifestAsset,path);
                        if (dependencies.Length > 0)
                        {
                            depMap.Add(abInfo.fullPath, dependencies);
                            if (dependencies.Length > 255)
                            {
                                abNoDepLargeList.Add(abInfo);
                            }
                            else
                            {
                                abDepList.Add(abInfo);
                            }
                            abInfo.childAbIndex = new List<int>();
                        }
                        else
                        {
                            abNoDepList.Add(abInfo);
                        }


                        //abMap.Add(bundlePath, i);
                    }

                    //ab has no dependency
                    for (int i = 0; i < abNoDepList.Count; i++)
                    {
                        var abInfo = abNoDepList[i];
                        abMap.Add(abInfo.fullPath, abTotalList.Count);
                        abTotalList.Add(abNoDepList[i]);
                    }
                    //ab has dependency <255
                    for (int i = 0; i < abDepList.Count; i++)
                    {
                        var abInfo = abDepList[i];
                        abMap.Add(abInfo.fullPath, abTotalList.Count);
                        abTotalList.Add(abDepList[i]);
                    }
                    //ab has dependency >255
                    for (int i = 0; i < abNoDepLargeList.Count; i++)
                    {
                        var abInfo = abNoDepLargeList[i];
                        abMap.Add(abInfo.fullPath, abTotalList.Count);
                        abTotalList.Add(abNoDepLargeList[i]);
                    }
                    //set dep ab info
                    for (int i = 0; i < abDepList.Count; i++)
                    {
                        ProcessABDep(abDepList[i], depMap, abMap);
                    }
                    for (int i = 0; i < abNoDepLargeList.Count; i++)
                    {
                        ProcessABDep(abNoDepLargeList[i], depMap, abMap);
                    }

                    using (FileStream fs = File.Create(target))
                    {
                        BinaryWriter bw = new BinaryWriter(fs);
                        //save dir str
                        bw.Write(folderPath.Count);
                        for (int i = 0; i < folderPath.Count; i++)
                        {
                            bw.Write(folderPath[i]);
                        }
                        //save all ab info
                        bw.Write(abTotalList.Count);
                        for (int i = 0; i < abTotalList.Count; i++)
                        {
                            var abInfo = abTotalList[i];
                            bw.Write(abInfo.abName);
                            short folderIndex = (short)abInfo.folderIndex;
                            bw.Write(folderIndex);
                            //bw.Write(abInfo.flag.HasFlag(AbInfo.Flag_InPackage));
                        }
                        bw.Write(abDepList.Count);
                        bw.Write(abNoDepLargeList.Count);
                        //save dep ab info
                        for (int i = 0; i < abDepList.Count; i++)
                        {
                            var abInfo = abDepList[i];
                            bw.Write((byte)abInfo.childAbIndex.Count);
                            for (int j = 0; j < abInfo.childAbIndex.Count; ++j)
                            {
                                int index = abInfo.childAbIndex[j];
                                bw.Write(index);
                            }
                        }
                        //large ab dep > 255                        
                        for (int i = 0; i < abNoDepLargeList.Count; i++)
                        {
                            var abInfo = abNoDepLargeList[i];
                            bw.Write((short)abInfo.childAbIndex.Count);
                            for (int j = 0; j < abInfo.childAbIndex.Count; ++j)
                            {
                                int index = abInfo.childAbIndex[j];
                                bw.Write(index);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    DebugLog.AddErrorLog(e.Message);
                }
            }
        }

        public void GenManifestData()
        {
            AssetBundle manifestBundle = AssetBundle.LoadFromFile("Assets/StreamingAssets/Bundles/Bundles");
            if (manifestBundle != null)
            {
                var manifest = (AssetBundleManifest)manifestBundle.LoadAsset("AssetBundleManifest");
                GenManifestData(manifest);
                manifestBundle.Unload(true);
            }

            //var manifest = AssetDatabase.LoadAssetAtPath<AssetBundleManifest>("Assets/StreamingAssets/Bundles/Bundles.manifest");
            //GenManifestData(manifest);
        }

        public void RePatch()
        {
            //DirectoryInfo di = new DirectoryInfo("Assets/StreamingAssets/TestBundles/assets/bundleres/");
            //var files = di.GetFiles("*.*", SearchOption.AllDirectories);
            //for (int i = 0; i < files.Length; ++i)
            //{
            //    var f = files[i];
            //    if (f.Extension != ".meta"&& f.Extension != ".manifest")
            //    {
            //        var relativePath = f.FullName.Replace(di.FullName, "").Replace("\\","/").ToLower();
            //        DebugLog.AddErrorLog(relativePath);
            //    }
            //}

            string path = "Assets/manifest.bytes";
            if(File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    string newpath = string.Format("Assets/StreamingAssets/manifest.bytes");
                    using (FileStream wfs = new FileStream(newpath, FileMode.Create))
                    {
                        BinaryWriter bw = new BinaryWriter(wfs);

                        var _client = reader.ReadString();
                        bw.Write(_client);
                        int cnt = reader.ReadInt32();
                        List<string> abpath = new List<string>();
                        //bw.Write(cnt);
                        for (int i = 0; i < cnt; i++)
                        {
                            var ab = reader.ReadString();
                            abpath.Add(ab);
                        }
                        DirectoryInfo di = new DirectoryInfo("Assets/StreamingAssets/TestBundles/assets/bundleres/");
                        var files = di.GetFiles("*.*", SearchOption.AllDirectories);
                        for (int i = 0; i < files.Length; ++i)
                        {
                            var f = files[i];
                            if (f.Extension != ".meta" && f.Extension != ".manifest")
                            {
                                var relativePath = f.FullName.Replace(di.FullName, "").Replace("\\", "/").ToLower();
                                //DebugLog.AddErrorLog(relativePath);
                                abpath.Add(relativePath);
                            }
                        }
                        bw.Write(abpath.Count);
                        for (int i = 0; i < abpath.Count; i++)
                        {
                            bw.Write(abpath[i]);
                        }
                    }

                }
            }
        }

        private void BuildReport(string path)
        {
            resReport.Clear();
            long totalSize = 0;
            DirectoryInfo di = new DirectoryInfo(path);
            var subDirs = di.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < subDirs.Length; ++i)
            {
                var subDir = subDirs[i];
                var group = new ReportGroup() { groupType = 0 };
                resReport.Add(subDir.Name, group);

                var files = subDir.GetFiles("*.*", SearchOption.AllDirectories);
                for (int j = 0; j < files.Length; ++j)
                {
                    var file = files[j];
                    string ext = file.Extension.ToLower();
                    if (ext != ".manifest" && ext != ".meta")
                    {
                        ReportGroup typeGroup;
                        if (!resReport.TryGetValue(ext, out typeGroup))
                        {
                            typeGroup = new ReportGroup() { groupType = 1 };
                            resReport.Add(ext, typeGroup);
                        }
                        string filename = file.FullName.Replace("\\", "/");
                        int index = filename.IndexOf(path);
                        if (index > 0)
                        {
                            filename = filename.Substring(index + path.Length);
                        }

                        var ri = new ReportItem()
                        {
                            name = filename,
                            sizeInByte = file.Length
                        };
                        typeGroup.items.Add(ri);
                        typeGroup.sizeInByte += ri.sizeInByte;
                        group.items.Add(ri);
                        group.sizeInByte += ri.sizeInByte;
                        totalSize += ri.sizeInByte;
                    }
                }
            }

            var bankgroup = new ReportGroup() { groupType = 1 };
            resReport.Add(".bank", bankgroup);
            var parentDi = di.Parent;
            var bankfiles = parentDi.GetFiles("*.bank", SearchOption.TopDirectoryOnly);

            for (int j = 0; j < bankfiles.Length; ++j)
            {
                var file = bankfiles[j];
                bankgroup.sizeInByte += file.Length;
                totalSize += bankgroup.sizeInByte;
                var ri = new ReportItem()
                {
                    name = file.Name,
                    sizeInByte = file.Length
                };
                bankgroup.items.Add(ri);
            }

            try
            {
                using (FileStream fs = new FileStream("Assets/BuildLog.txt", FileMode.Create))
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("==========================build bundle report==========================");
                    sb.AppendLine("time hash:" + timeHash.ToString());
                    sb.AppendLine("\t\t\tTotal Size: " + EditorUtility.FormatBytes(totalSize));
                    var sw = new StreamWriter(fs);
                    sb.AppendLine("==========================folder group==========================");
                    BuildReport(0, false, sb, sw);
                    sb.AppendLine();
                    sb.AppendLine("==========================res group==========================");
                    BuildReport(1, true, sb, sw);
                    sw.Write(sb.ToString());
                }

            }
            catch (Exception e)
            {
                DebugLog.AddErrorLog("save build log error:" + e.Message);
            }

        }

        private void BuildReport(int groupType, bool outputFileInfo, System.Text.StringBuilder sb, StreamWriter sw)
        {
            var it = resReport.GetEnumerator();
            while (it.MoveNext())
            {
                var current = it.Current;
                var v = current.Value;
                if (v.groupType == groupType)
                {
                    sb.Append("Group:");
                    sb.Append(current.Key);
                    int spaceCount = 40 - current.Key.Length;
                    for (int i = 0; i < spaceCount; ++i)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(string.Format("Count: {0} Size: {1} {2} Bytes\r\n",
                        v.items.Count.ToString(),
                        EditorUtility.FormatBytes(v.sizeInByte),
                        v.sizeInByte.ToString()));
                    if (outputFileInfo)
                    {
                        sb.AppendLine();
                        v.items.Sort((x, y) => x.sizeInByte.CompareTo(y.sizeInByte));
                        for (int i = v.items.Count - 1; i >= 0; --i)
                        {
                            var item = v.items[i];
                            sb.Append("\t");
                            sb.Append(item.name);
                            spaceCount = 80 - item.name.Length;
                            for (int j = 0; j < spaceCount; ++j)
                            {
                                sb.Append(" ");
                            }
                            sb.Append(string.Format("Size: {0} {1} Bytes\r\n",
                                EditorUtility.FormatBytes(item.sizeInByte),
                                item.sizeInByte.ToString()));
                        }
                    }
                }
            }
            // DebugLog.AddLog2 (sb.ToString ());
        }

        [MenuItem("Assets/Tool/TestAB")]
        public static void TestAB()
        {
            var obj = Selection.activeObject;
            if (obj != null)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var ab = AssetBundle.LoadFromFile(path);
                if (ab != null)
                {
                    var name = ab.GetAllAssetNames()[0];
                    DebugLog.AddEngineLog2(name);
                    ab.Unload(true);
                }

            }
        }
    }
}