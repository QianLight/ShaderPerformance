/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using Zeus.Framework;

#if DOTS_BUILD
using Unity.Build;
using Unity.Build.Common;
using static Unity.Build.Common.SceneList;
using Unity.Build.Classic;
#endif

namespace Zeus.Build
{
    internal class XMLConfig
    {
        public string className = "";
        public string platformName = "";
    }

    public class BuildScript
    {
        static List<IBeforeBuild> _BeforeList = new List<IBeforeBuild>();
        static List<IInternalBeforeBuild> _InternalBeforeList = new List<IInternalBeforeBuild>();
        static List<IModifyPlayerSettings> _ModifySettingsList = new List<IModifyPlayerSettings>();
        static List<IAfterBuild> _AfterList = new List<IAfterBuild>();
        static List<IFinallyBuild> _FinallyList = new List<IFinallyBuild>();

        public static BuildPlayerOptions buildPlayerOptions;

        const string buildManifestName = "ZeusBuildManifest.xml";

        private static DateTime _lastBuildStepTime = DateTime.MinValue;
        private static string _lastBuildStepLog;

        private static void LogBuildStep(string buildLog)
        {
            if (DateTime.Now != DateTime.MinValue && _lastBuildStepLog != null)
            {
                Zeus.Core.Logger.Log(true, "ZeusBuild", _lastBuildStepLog, "costs:", (int)DateTime.Now.Subtract(_lastBuildStepTime).TotalMinutes, " min");
            }
            _lastBuildStepTime = DateTime.Now;
            _lastBuildStepLog = buildLog;
            Zeus.Core.Logger.Log(true, "ZeusBuild", "Starting: ", buildLog);
        }
        public static string[] Levels
        {
            get
            {
                List<string> levels = new List<string>();
                for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
                {
                    if (EditorBuildSettings.scenes[i].enabled)
                        levels.Add(EditorBuildSettings.scenes[i].path);
                }
                if (levels.Count == 0)
                    Debug.LogWarning("Warming: the enabled scenes's count is 0!!!");
                return levels.ToArray();
            }
        }

        //判断类被多个平台使用
        public static bool CheckPlatform(string platformName)
        {
            BuildTarget buildTarget = GetBuildTarget();
            bool isFitPlatform = false;
            platformName = platformName.Trim();
            if (platformName.Contains(","))
            {
                string[] platformArray = platformName.Split(',');
                for (int index = 0; index < platformArray.Length; index++)
                {
                    platformArray[index] = platformArray[index].Trim();
                    if (platformArray[index] == Enum.GetName(typeof(BuildTarget), buildTarget))
                    {
                        isFitPlatform = true;
                    }
                }
            }
            else
            {
                isFitPlatform = platformName == Enum.GetName(typeof(BuildTarget), buildTarget);
            }
            return isFitPlatform;
        }


        private static XmlDocument LoadBuildXmlConfig()
        {
            string manifestPath = null;
            try
            {
                XmlDocument Xdoc = new XmlDocument();  //实例化
                manifestPath = Application.dataPath + "/ZeusBuildManifest.xml";
                if (!File.Exists(manifestPath))
                {
                    manifestPath = Zeus.Framework.Core.PackageInfo.PackageFullPath + "/Editor/Build/ZeusBuildManifest.xml";
                }
                Xdoc.Load(manifestPath);  //加载XML 文件
                return Xdoc;
            }
            catch (Exception ex) //文件路径错误，抛出异常
            {
                Debug.LogError("Parse ZeusBuildManifest failed");
                Debug.LogException(ex);
                throw ex;
            }
        }

        private static List<XMLConfig> ParseBuildXmlConfig(XmlDocument Xdoc, string rootName)
        {
            List<XMLConfig> configList = new List<XMLConfig>();
            try
            {
                XmlElement root = Xdoc.DocumentElement;   //获取跟节点
                XmlNode dataRoot = root.SelectSingleNode(rootName);   //获取跟节点   
                foreach (var x1 in dataRoot.ChildNodes)
                {
                    //这里也可能是注释，判断下，方便测试
                    var element = x1 as XmlElement;
                    if(null == element)
                    {
                        continue;
                    }
                    XMLConfig config = new XMLConfig();
                    config.className = "";
                    config.platformName = "";
                    foreach (var node in element.ChildNodes)
                    {
                        var data = node as XmlElement;
                        if(null == data)
                        {
                            continue;
                        }
                        switch (data.Name)
                        {
                            case "class":
                                if (data.InnerText == "")
                                {
                                    throw new Exception(string.Format("Class InnetText not Exit!"));
                                }
                                config.className = data.InnerText;
                                break;
                            case "platform":
                                config.platformName = data.InnerText;
                                break;
                        }
                    }
                    configList.Add(config);
                }
                return configList;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw ex;
            }
        }

        private static void CreateBuildSteps<T>(List<XMLConfig> configList, ref List<T> stepList) where T : class
        {
            //遍历所有的ClassDic和Assembly，找与ZeusBuildManifest.xml中对应的类
            string className = "";
            string platformName = "";
            for (int i = 0; i < configList.Count; i++)
            {
                className = configList[i].className.Trim();
                if(string.IsNullOrEmpty(className))
                {
                    continue;
                }
                platformName = configList[i].platformName;
                //判读适用的平台
                if (string.IsNullOrEmpty(platformName) || CheckPlatform(platformName))
                {
                    Type targetType = null;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        targetType = assembly.GetType(className);
                        if (targetType != null)
                        {
                            //异常判断：是否只有默认构造体
                            try
                            {
                                System.Object o = Activator.CreateInstance(targetType);
                                T createClass = o as T;
                                //判断是否继承接口
                                if (createClass == null)
                                {
                                    throw new Exception(string.Format("Class {0} is not {1} , it not inhert the interface {2}", className, typeof(T), typeof(T)));
                                }
                                else
                                {
                                    stepList.Add(createClass);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                                throw new Exception(string.Format("Default constructor not found for class ： {0} , Please make sure {1} only has default constructor!", className, className));
                            }
                            //同一个配置类不应该在两个assemble，所以找到第一个即可
                            break;
                        }
                    }
                    if (targetType == null)
                    {
                        throw new Exception(string.Format("Please check ZeusBuildManifest.xml, Class: {0} not found", className));
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        private static void LoadBuildSteps<T>(string rootName, ref List<T> buildSteps) where T : class
        {
            XmlDocument Xdoc = LoadBuildXmlConfig();
            List<XMLConfig> configList = ParseBuildXmlConfig(Xdoc, rootName);
            CreateBuildSteps<T>(configList, ref buildSteps);
        }

        private static void CallModifyPlayerSettings(BuildTarget target, ref List<IModifyPlayerSettings> buildSteps)
        {
            if (buildSteps.Count <= 0) return;

            for (int i = 0; i < buildSteps.Count; i++)
            {
                LogBuildStep("ModifyPlayerSettings " + buildSteps[i].GetType().ToString());
                buildSteps[i].OnModifyPlayerSettings(target);
            }
        }

        private static void CallBeforBuild(BuildTarget target, ref List<IBeforeBuild> buildSteps, string outputPath)
        {
            if (buildSteps.Count <= 0) return;

            for (int i = 0; i < buildSteps.Count; i++)
            {
                LogBuildStep("BeforeBuild " + buildSteps[i].GetType().ToString());
                buildSteps[i].OnBeforeBuild(target, outputPath);
            }
        }

        private static void CallInternalBeforeBuild(BuildTarget target, ref List<IInternalBeforeBuild> buildSteps, string locationPathName)
        {
            if (buildSteps.Count <= 0) return;

            for (int i = 0; i < buildSteps.Count; i++)
            {
                LogBuildStep("InternalBeforeBuild " + buildSteps[i].GetType().ToString());
                buildSteps[i].OnInternalBeforeBuild(target, locationPathName);
            }
        }

        private static void CallAfterBuild(BuildTarget target, ref List<IAfterBuild> buildSteps, string locationPathName)
        {
            if (buildSteps.Count <= 0) return;

            for (int i = 0; i < buildSteps.Count; i++)
            {
                LogBuildStep("AfterBuild " + buildSteps[i].GetType().ToString());
                buildSteps[i].OnAfterBuild(target, locationPathName);
            }
        }

        private static void CallFinallyBuild(BuildTarget target, ref List<IFinallyBuild> buildSteps, string locationPathName)
        {
            if (buildSteps.Count <= 0) return;

            for (int i = 0; i < buildSteps.Count; i++)
            {
                LogBuildStep("FinallyBuild " + buildSteps[i].GetType().ToString());
                buildSteps[i].OnFinallyBuild(target, locationPathName);
            }
        }

        /// <summary>
        /// 获取包含文件名的完整输出路径
        /// </summary>
        private static string GetLocationPathName(BuildTarget buildTarget, string outputPath)
        {
            string packageName = GlobalBuild.Default.packageName;
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.PACKAGE_NAME, ref packageName);
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
                    {
                        return PathUtil.CombinePath(outputPath, packageName);
                    }
                    else
                    {
                        bool exportAAB = false;
                        if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.EXPORT_AAB, ref exportAAB) && exportAAB)
                        {
                            if (!packageName.EndsWith(".aab"))
                            {
                                packageName += ".aab";
                            }
                            return PathUtil.CombinePath(outputPath, packageName);
                        }
                        else
                        {
                            if (!packageName.EndsWith(".apk"))
                            {
                                packageName += ".apk";
                            }
                            return PathUtil.CombinePath(outputPath, packageName);
                        }
                    }
                case BuildTarget.iOS:
                    return PathUtil.CombinePath(outputPath, packageName);
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return PathUtil.CombinePath(outputPath, packageName, packageName + ".exe");
                default:
                    throw new Exception(string.Format("Platform Error! Can't Build {0}!", buildTarget));
            }
        }
        /// <summary>
        /// 根据【包含文件名的完整输出路径】获取打包输出路径
        /// 与【GetLocationPathName(BuildTarget buildTarget, string outputPath)】函数作用相反
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <param name="locationPathName"></param>
        /// <returns></returns>
        public static string GetOutputPath(BuildTarget buildTarget, string locationPathName)
        {
            string packageName = GlobalBuild.Default.packageName;
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.PACKAGE_NAME, ref packageName);
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
                    {
                        return locationPathName.Substring(0, locationPathName.LastIndexOf(packageName) - 1);
                    }
                    else
                    {
                        bool exportAAB = false;
                        if(CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.EXPORT_AAB, ref exportAAB) && exportAAB)
                        {
                            if (!packageName.EndsWith(".aab"))
                            {
                                packageName += ".aab";
                            }
                            return locationPathName.Substring(0, locationPathName.LastIndexOf(packageName) - 1);
                        }
                        if (!packageName.EndsWith(".apk"))
                        {
                            packageName += ".apk";
                        }
                        return locationPathName.Substring(0, locationPathName.LastIndexOf(packageName) - 1);
                    }
                case BuildTarget.iOS:
                    return locationPathName.Substring(0, locationPathName.LastIndexOf(packageName) - 1);
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return locationPathName.Substring(0, locationPathName.LastIndexOf(Path.Combine(packageName, packageName + ".exe")) - 1);
                default:
                    throw new Exception(string.Format("Platform Error! Can't Build {0}!", buildTarget));
            }
        }

        private static BuildTarget GetBuildTarget()
        {
            string platform = GlobalBuild.Default.Platform;
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.PLATFORM, ref platform);
            switch (platform.ToLower())
            {
                case GlobalBuild.Platform.ANDROID:
                    return BuildTarget.Android;
                case GlobalBuild.Platform.IOS:
                    return BuildTarget.iOS;
                case GlobalBuild.Platform.WINDOWS:
                    return BuildTarget.StandaloneWindows;
                case GlobalBuild.Platform.WINDOWS64:
                    return BuildTarget.StandaloneWindows64;
                default:
                    throw new Exception(string.Format("Platform Error! Can't find Platform named {0}!", platform));
            }
        }

        public static void BuildPlayerInBatchMode_Internal(string buildProperties)
        {
            EnsureBuildConfigs();
            CommandLineArgs.Initialize(buildProperties);
            BuildTarget buildTarget = GetBuildTarget();
            buildPlayerOptions.target = buildTarget;
            BuildPlayer(buildTarget);
        }
        
        public static void BuildPlayerInBatchMode()
        {
            BuildPlayerInBatchMode_Internal(null);
        }

        public static void EnsureBuildConfigs()
        {
            if (!Directory.Exists("_Build"))
            {
                Framework.PackageUtility.CopyPackageFiles(Framework.Core.PackageInfo.PackageName, @"Editor\Build\_Build", "*", "_Build");
            }
        }

        public static bool BuildPlayerInGraphicMode()
        {
            BuildTarget buildTarget = GetBuildTarget();
            buildPlayerOptions.target = buildTarget;
            switch (buildTarget)
            {
#if UNITY_5
                case BuildTarget.StandaloneOSXUniversal:
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
#else
                case BuildTarget.StandaloneOSX:
#endif
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, buildTarget);
                    buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
                    break;
                case BuildTarget.Android:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, buildTarget);
                    buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
                    break;
                case BuildTarget.iOS:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, buildTarget);
                    buildPlayerOptions.targetGroup = BuildTargetGroup.iOS;
                    break;
                default:
                    throw new Exception(string.Format("BuildTarget Error! not support BuildTarget {0}!", buildTarget.ToString()));
            }
            return BuildPlayer(buildTarget);
        }
        private static void InitBuildSteps()
        {
            //清除上次的build log
            _lastBuildStepTime = new DateTime(0);
            _lastBuildStepLog = null;
            //用读取XML
            _ModifySettingsList.Clear();
            _BeforeList.Clear();
            _InternalBeforeList.Clear();
            _AfterList.Clear();
            _FinallyList.Clear();
            LoadBuildSteps<IModifyPlayerSettings>("ModifyPlayerSettings", ref _ModifySettingsList);
            LoadBuildSteps<IBeforeBuild>("BeforeBuild", ref _BeforeList);
            LoadBuildSteps<IInternalBeforeBuild>("InternalBeforeBuild", ref _InternalBeforeList);
            LoadBuildSteps<IAfterBuild>("AfterBuild", ref _AfterList);
            LoadBuildSteps<IFinallyBuild>("FinallyBuild", ref _FinallyList);
        }

        private static void DeleteOutputPathFiles(string deletePath)
        {
            //打包之前删除旧的
            if (System.IO.File.Exists(deletePath))
            {
                //是文件
                System.IO.File.Delete(deletePath);
            }
            else if (System.IO.Directory.Exists(deletePath))
            {
                bool delete = true;
                CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_DELETE_PROJECT_OUTPUT_FOLDER, ref delete);
                if (delete)
                {
                    //是目录
                    System.IO.Directory.Delete(deletePath, true);
                }
            }
            else
            {
                try
                {
                    var directory = new DirectoryInfo(deletePath);
                    directory.Create();
                    directory.Delete();
                }
                catch (Exception)
                {
                    throw new Exception("输出路径 " + deletePath + " 无效,请修改 OUTPUT_PATH");
                }
            }
        }

        private static void PackPackagePatch(string outputPath)
        {

            string fileName = "assets";
            string filePath = outputPath + "/" + fileName + ".packagepatch";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            DirectoryInfo Folder = new DirectoryInfo(Application.streamingAssetsPath);

            foreach (FileInfo file in Folder.GetFiles())
            {
                if ("meta" == file.Name.Substring(0, file.Name.LastIndexOf('.')))
                {
                    file.Delete();
                }
            }
            string[] invalidFileType = { ".meta" };
            Zeus.Core.ZipUtil.ZipFileFromDirectoryExceptInvalidFileTypes(Application.streamingAssetsPath, filePath, invalidFileType);
        }

        public static bool BuildPlayer(BuildTarget buildTarget)
        {
            InitBuildSteps();
            //  这是完整的输出路径目录
            string outputPath = GlobalBuild.Default.OutputPath;
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.OUTPUT_PATH, ref outputPath);
            try
            {
                //调用xml方法
                CallModifyPlayerSettings(buildTarget, ref _ModifySettingsList);
                //  这才是包含文件名的完整输出路径
                buildPlayerOptions.locationPathName = GetLocationPathName(buildTarget, outputPath);
                DeleteOutputPathFiles(buildPlayerOptions.locationPathName);

                //  Tag1:记录当前isGoogleAndroidProject，在正式打包前重新设置，以免有人在MidifyPlayerSettings后再改导致输出路径错误的问题出现。
                bool isGoogleAndroidProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;

                //BeforeBuild(buildTarget, outputPath);
                CallBeforBuild(buildTarget, ref _BeforeList, outputPath);
                if (string.IsNullOrEmpty(buildPlayerOptions.locationPathName))
                {
                    throw new Exception(string.Format("You must initialize and set {0}.buildPlayerOptions before PreprocessBuild. You can do it at IBeforeBuild/BeforeBuildAttribute.", typeof(BuildScript).FullName));
                }

                //InternalBeforeBuild(buildTarget, outputPath);
                CallInternalBeforeBuild(buildTarget, ref _InternalBeforeList, outputPath);
                bool isPackagePatch = false;
                CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_PACKAGE_PATCH, ref isPackagePatch);
                if (isPackagePatch)
                {
                    PackPackagePatch(outputPath);
                    return true;
                }
                else
                {
                    //  Tag1
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = isGoogleAndroidProject;
                    // Unity在BuildPlayer时IPreProcessBuild和IPostProcessBuild / PostProcessBuildAttribute中抛出的异常只捕捉异常信息，并不会终止BuildPlayer抛出异常。
                    //Debug.Log("buildPlayerOptions.locationPathName; >>>>>>>>>>>>>>>>>>>>>>>>>>>\n" + buildPlayerOptions.locationPathName);                
                    bool buildSuc = false;
					bool exportAAB = false;
                    if(buildTarget != BuildTarget.Android || !CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.EXPORT_AAB, ref exportAAB))
                    {
                        exportAAB = false;
                    }
                    if(exportAAB)
                    {
                        //aab只能导出工程，直接导出aab的机制不再支持
                        buildSuc = AAB.AABUtility.ExportAndroidProjectForAAB(buildPlayerOptions);
                    }
                    else
                    {
                        //这里重置下，防止其他逻辑影响打apk
                        EditorUserBuildSettings.buildAppBundle = false;
#if UNITY_2018_1_OR_NEWER

#if DOTS_BUILD
                        buildSuc = BuildDots(buildTarget);
#else
                        Encrypt.EncryptAssetBundle.SetMaskAssetBundle(false);
                        UnityEditor.Build.Reporting.BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
                        Encrypt.EncryptAssetBundle.SetMaskAssetBundle(true);
                        buildSuc = buildReport.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
                        if (!buildSuc)
                        {
                            Debug.LogError("Build failed or Cancelled ");
                        }
#endif

#else
                        string error = BuildPipeline.BuildPlayer(buildPlayerOptions);
                        buildSuc = string.IsNullOrEmpty(error);
                        if (!buildSuc)
                        {
                            Debug.LogError("Build failed: " + error);
                        }
#endif
                    }
                    //AfterBuild(buildTarget, buildPlayerOptions.locationPathName);
                    CallAfterBuild(buildTarget, ref _AfterList, buildPlayerOptions.locationPathName);
                    // 如果是Windows，打开输出文件夹——————————未经严格验证
                    bool isOpenFolder = true;
                    CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.IS_OPEN_OUTPUT_FOLDER, ref isOpenFolder);
                    if (buildSuc && Environment.OSVersion.Platform < PlatformID.Unix && isOpenFolder)
                        System.Diagnostics.Process.Start("Explorer", PathUtil.FormatPathSeparator(System.IO.Path.GetDirectoryName(buildPlayerOptions.locationPathName)));//路径必须把/换成\
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Build Failed! There is an exception!");
                Debug.LogException(ex);
                return false;
            }
            finally
            {
                //FinallyBuild(buildTarget, buildPlayerOptions.locationPathName);
                CallFinallyBuild(buildTarget, ref _FinallyList, buildPlayerOptions.locationPathName);
            }
        }

#if DOTS_BUILD

        private static bool BuildDots(BuildTarget buildTarget)
        {
            var conf = GetDotsBuildConf(buildTarget);
            var platform = conf.GetComponent<ClassicBuildProfile>();
            if (null == platform.Platform)
            {
                Debug.LogError("Unsupported Platform : " + buildTarget);
                return false;
            }

            if (platform.Platform is MissingPlatform)
            {
                Debug.LogError("Missing Platform Package : " + platform.Platform.DisplayName);
                return false;
            }

            var result = conf.Build();
            result.LogResult();
            return result.Succeeded;
        }

        //获取DOTS打包的配置
        private static BuildConfiguration GetDotsBuildConf(BuildTarget buildTarget)
        {
            var conf = new BuildConfiguration();

            //设置公司名和项目名
            var settings = new GeneralSettings();
            string companyName = GlobalBuild.Default.companyName;
            string productName = GlobalBuild.Default.productName;
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.COMPANY_NAME, ref companyName);
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.PRODUCT_NAME, ref productName);
            settings.CompanyName = companyName;
            settings.ProductName = productName;
            conf.SetComponent(settings);

            //设置场景
            var sceneList = new SceneList();
            var scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled)
                    continue;

                SceneInfo info = new SceneInfo();
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[i].path);
                info.Scene = GlobalObjectId.GetGlobalObjectIdSlow(sceneAsset);
                sceneList.SceneInfos.Add(info);
            }
            conf.SetComponent(sceneList);

            //设置输出路径
            var dir = new OutputBuildDirectory();
            string outputPath = GlobalBuild.Default.OutputPath;
            CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.OUTPUT_PATH, ref outputPath);
            dir.OutputDirectory = outputPath + Path.DirectorySeparatorChar + companyName;
            conf.SetComponent(dir);

            //设置平台
            var profile = new ClassicBuildProfile();
            Platform platform = null;
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    platform = Platform.GetPlatformByName("Android");
                    break;
                case BuildTarget.iOS:
                    platform = Platform.GetPlatformByName("IOS");
                    break;
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    platform = Platform.GetPlatformByName("Windows");
                    break;
            }
            profile.Platform = platform;
            conf.SetComponent(profile);

            return conf;
        }
#endif

    }
}
#endif