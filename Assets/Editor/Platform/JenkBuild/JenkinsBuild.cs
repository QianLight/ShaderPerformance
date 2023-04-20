using CFEngine;
using CFEngine.Editor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;
using UnityEditor.Compilation;
using System.Text.RegularExpressions;
using CFEngine.Quantify;
using Zeus.Build;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using UnityEditor.U2D;

public partial class JenkinsBuild
{
    static string _targetDir = "";
    static BuildTarget _target;
    static BuildTargetGroup _group;
    static BuildOptions _build = BuildOptions.None;
    static string _identifier
    {
        get
        {
            if (_target == BuildTarget.iOS)
            {
                return "com.ningyunet.cfgame";
            }
            return "com.pie.piegame";
        }
    }
    static string _product
    {
        //"航海王手游";//代号:SEA
        get
        {
            string appName = File.ReadAllText(Application.dataPath.Replace("Assets", "") + "Shell/AppName.txt");
            return appName;
        }
    }

    static string _version = "0.0.0";
    static string[] _scenes = null;

    public enum TPlatform { Win32, iOS, Android }
    public enum TChanel { Outer }

    static string _macro
    {
        get
        {
            string defineData = File.ReadAllText(Application.dataPath.Replace("Assets", "") + "Shell/macro.txt");
            defineData = defineData.Trim();
            defineData = Regex.Replace(defineData, @"\t|\n|\r", "");
            return defineData;
        }
    }


    static string _last_build_macro
    {
        get
        {
            string defineData = string.Empty;
            string filePath = Application.dataPath.Replace("Assets", "") + "Library/macro.txt";
            if (File.Exists(filePath))
            {
                defineData = File.ReadAllText(filePath);
            }
            defineData = defineData.Trim();
            defineData = Regex.Replace(defineData, @"\t|\n|\r", "");
            return defineData;
        }
        set
        {
            string filePath = Application.dataPath.Replace("Assets", "") + "Library/macro.txt";
            System.IO.File.WriteAllText(filePath, value, System.Text.Encoding.UTF8);
        }
    }


    // used by jenkins for fast building
    public static void XBuildAndroid()
    {
        SwitchPlatForm(TPlatform.Android, delegate ()
        {

            Build(false);
        });

    }


    [MenuItem("Tools/Build/Android")]
    public static void XGeneralBuildAndroid()
    {
        SwitchPlatForm(TPlatform.Android, delegate ()
        {
            Build(false);
        });
    }

    public static void BuildAndroid()
    {
        SwitchPlatForm(TPlatform.Android, delegate ()
        {
            BuildAB();
            Build(false);
        });

    }

    [MenuItem("Tools/Build/AndroidOptimize")]
    public static void BuildAndroidOptimize()
    {
        BuildAndroidOptimizeCommon(true);
    }

    [MenuItem("Tools/Build/BuildAndroidOptimizeNoAB")]
    public static void BuildAndroidOptimizeNoAB()
    {
        BuildAndroidOptimizeCommon(false);
    }

    [MenuItem("Tools/Build/AndroidOptimizeNoAtlases")]
    public static void BuildAndroidOptimizeNoAtlases()
    {
        BuildAndroidOptimizeCommon(true, false);
    }

    public static void BuildAndroidOptimizeCommon(bool isAb,bool isAtlase=true)
    {
        if (isAtlase)
        {
            //刷新ui图集缓存
            SpriteAtlasUtility.PackAllAtlases(BuildTarget.Android);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EngineUtility.IsBuildingGame = true;
        EditorSettings.cacheServerMode = CacheServerMode.Disabled;
        EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
        EditorUserBuildSettings.installInBuildFolder = true;

        SwitchPlatForm(TPlatform.Android, delegate ()
        {
            PrecompiledScripts();
            if (isAb)
                BuildAB();
            Build(false);

            EditorSettings.cacheServerMode = CacheServerMode.Enabled;
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            EngineUtility.IsBuildingGame = false;
        });

    }

    public static void BuildAndroidScriptOnly()
    {

        EngineUtility.IsBuildingGame = true;
        EditorSettings.cacheServerMode = CacheServerMode.Disabled;
        EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
        EditorUserBuildSettings.installInBuildFolder = true;

        SwitchPlatForm(TPlatform.Android, delegate ()
        {
            PrecompiledScripts();
            BuildScriptOnly(false);

            EditorSettings.cacheServerMode = CacheServerMode.Enabled;
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            EngineUtility.IsBuildingGame = false;
        });

    }


    [MenuItem("Tools/Build/Win32")]
    public static void XBuildWin32()
    {
        SwitchPlatForm(TPlatform.Win32, delegate ()
        {
            Build(false);
        });

    }

    public static void BuildWin32()
    {
        //刷新ui图集缓存
        SpriteAtlasUtility.PackAllAtlases(BuildTarget.StandaloneWindows64);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EngineUtility.IsBuildingGame = true;
        EditorSettings.cacheServerMode = CacheServerMode.Disabled;
        EditorSettings.spritePackerMode = SpritePackerMode.Disabled;

        SwitchPlatForm(TPlatform.Win32, delegate ()
        {
            PrecompiledScripts();
            BuildAB();
            Build(false);
            EditorSettings.cacheServerMode = CacheServerMode.Enabled;
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            EngineUtility.IsBuildingGame = false;
        });

    }

    [MenuItem("Tools/Build/iOS")]
    public static void XBuildIOS()
    {
        SwitchPlatForm(TPlatform.iOS, delegate ()
        {
            Build(false);
        });

    }

    public static void BuildIOS()
    {

        //刷新ui图集缓存
        SpriteAtlasUtility.PackAllAtlases(BuildTarget.iOS);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EngineUtility.IsBuildingGame = true;
        EditorSettings.cacheServerMode = CacheServerMode.Disabled;
        EditorSettings.spritePackerMode = SpritePackerMode.Disabled;

        SwitchPlatForm(TPlatform.iOS, delegate ()
        {
            PrecompiledScripts();
            BuildAB();
            Build(false);
            EditorSettings.cacheServerMode = CacheServerMode.Enabled;
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            EngineUtility.IsBuildingGame = false;
        });

    }

    [MenuItem("Tools/Build/Bundle")]
    public static void BuildAB()
    {
        SceneMissCalc.SearchAll();

        bool isRebuildAll = false;
        if (string.IsNullOrEmpty(_last_build_macro) || (_macro.Contains("MASK_ASSET_BUNDLE") != _last_build_macro.Contains("MASK_ASSET_BUNDLE")))
            isRebuildAll = true;
        Debug.Log("isRebuildAll:" + isRebuildAll);

        JianxiuResourceTools.ReplaceJianxiuDirectory();//替换监修文件夹

        if (!_macro.Contains("NO_ASSETBUNDLE"))
        {
            Encrypt.EncryptAssetBundle.SetMaskAssetBundle(true);
            UnityEngine.AssetGraph.AssetGraphEditorWindow.BuileAssetBundle(EditorUserBuildSettings.activeBuildTarget, isRebuildAll);
            Encrypt.EncryptAssetBundle.AssetBundleEncrypt((int)EditorUserBuildSettings.activeBuildTarget);
        }
        else
        {
            CFEngine.Editor.BuildBundleConfig.instance.BuildBundle("", -1, CFEngine.Editor.BuildType.PreBuild, false);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //BuildBundle.BuildAllAssetBundlesWithList ();

        //写入文件
        _last_build_macro = _macro;
    }

    /// <summary>
    /// 通过打一个空场景bundle检测是否存在代码编译报错
    /// </summary>
    public static void PrecompiledScripts()
    {
        BuildBundleConfig.RecordTime("StartPrecompiledScripts");

        string outDir = "Bundles/tmp";
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }
        AssetBundleBuild[] buildList = new AssetBundleBuild[1];
        buildList[0].assetBundleName = "PrecompiledScripts";
        string[] bundleAssets = new string[2];
        bundleAssets[0] = @"Assets\Scenes\Default\empty.unity";
        buildList[0].assetNames = bundleAssets;
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outDir, buildList, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        if (manifest == null)
        {
            BuildBundleConfig.RecordTime("Build failed or Cancelled ");
            throw new Exception("Precompiled All Scripts Error.");
        }
        else
        {
            string filepath = Application.dataPath.Replace("Assets", "") + outDir;
            if (Directory.Exists(filepath))
            {
                UnityEngine.AssetGraph.FileUtility.DeleteDirectory(filepath, true);
            }
        }

        BuildBundleConfig.RecordTime("EndPrecompiledScripts");
    }

    [MenuItem("Tools/Build/Bundle(Log)")]
    public static void BuildABWithLog()
    {
        BuildBundle.BuildAllAssetBundlesWithList("", false);
    }

    private static void PlayerSetting_Common()
    {
        PlayerSettings.companyName = "perfectworld";
        PlayerSettings.productName = _product;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.useAnimatedAutorotation = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.SplashScreen.show = true;
        PlayerSettings.SetApplicationIdentifier(_group, _identifier);


        if (_macro.Contains("MT_RENDERING"))
            PlayerSettings.SetMobileMTRendering(_group, true);
        else
            PlayerSettings.SetMobileMTRendering(_group, false);

        if (_macro.Contains("SHADER_HALF"))
            ShaderStripperTool.ShaderHalfOpen();

        if (_macro.Contains("IL2CPP_CONFIG_DEBUG"))
            PlayerSettings.SetIl2CppCompilerConfiguration(_group, Il2CppCompilerConfiguration.Debug);
        else
            PlayerSettings.SetIl2CppCompilerConfiguration(_group, Il2CppCompilerConfiguration.Release);

        TextAsset version = Resources.Load<TextAsset>("version");
        if (version != null)
            _version = version.text.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
        PlayerSettings.bundleVersion = _version;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private static void PlayerSetting_SwitchPlatform(BuildTarget target, BuildTargetGroup group)
    {
        _target = target;
        _group = group;
        var t = EditorUserBuildSettings.selectedStandaloneTarget;
        var g = EditorUserBuildSettings.selectedBuildTargetGroup;
        if (_target != t || _group != g)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(_group, _target);
        }

    }

    private static void PlayerSetting_Win32()
    {
        PlayerSetting_SwitchPlatform(BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone);
        PlayerSetting_Common();
        PlayerSettings.productName = "OnePiece";  //overide 
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1218;
        PlayerSettings.defaultScreenHeight = 563;
        _targetDir = Path.Combine(Application.dataPath.Replace("/Assets", ""), "Win32");
        PlayerSettings.SetScriptingBackend(_group, ScriptingImplementation.Mono2x);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, _macro);
        PlayerSettings.SetApiCompatibilityLevel(_group, ApiCompatibilityLevel.NET_4_6);
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Standalone, ManagedStrippingLevel.Disabled);
        //PlayerSettings.strippingLevel = StrippingLevel.Disabled;
    }

    private static void PlayerSetting_iOS()
    {

        PlayerSetting_SwitchPlatform(BuildTarget.iOS, BuildTargetGroup.iOS);
        PlayerSetting_Common();
        _targetDir = Path.Combine(Application.dataPath.Replace("/Assets", ""), "IOS");
        PlayerSettings.iOS.buildNumber = _version;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.accelerometerFrequency = 0;
        PlayerSettings.iOS.locationUsageDescription = "location access";
        PlayerSettings.iOS.cameraUsageDescription = "camera access";
        PlayerSettings.iOS.microphoneUsageDescription = "microphone access";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, _macro);
        PlayerSettings.SetApiCompatibilityLevel(_group, ApiCompatibilityLevel.NET_4_6);
        PlayerSettings.aotOptions = "nrgctx-trampolines=4096,nimt-trampolines=4096,ntrampolines=4096";
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        // PlayerSettings.iOS.targetOSVersionString = "9.0";
        PlayerSettings.stripEngineCode = false;
        //PlayerSettings.strippingLevel = StrippingLevel.StripByteCode;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Disabled);
        PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.FastButNoExceptions;
        PlayerSettings.gcIncremental = false;
        if (_macro.Contains("_USE_DEV_BUILD"))
        {
            EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Debug;
        }
        else
        {
            EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Release;
        }
        
    }

    private static void PlayerSetting_Android()
    {
        PlayerSetting_SwitchPlatform(BuildTarget.Android, BuildTargetGroup.Android);
        PlayerSetting_Common();
        _targetDir = Path.Combine(Application.dataPath.Replace("/Assets", ""), "Android");
        //int bundleVersionCode = int.Parse(System.DateTime.Now.ToString("yyMMddHH"));
        int bundleVersionCode = int.Parse(System.DateTime.Now.ToString("yy")) * 1000 + System.DateTime.Now.DayOfYear;
        PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        BuildBundleConfig.RecordTime("PlayerSettings.Android.bundleVersionCode: " + bundleVersionCode);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        PlayerSettings.Android.forceSDCardPermission = true;
        PlayerSettings.Android.forceInternetPermission = true;
        PlayerSettings.Android.androidTVCompatibility = false;

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, _macro);

        PlayerSettings.SetApiCompatibilityLevel(_group, ApiCompatibilityLevel.NET_4_6);

        // PlayerSettings.strippingLevel = StrippingLevel.Disabled;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Disabled);
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        PlayerSettings.Android.keystoreName = Application.dataPath + "/Editor/Platform/JenkBuild/android.keystore";
        PlayerSettings.Android.keystorePass = "XCvis8RGbw";
        PlayerSettings.Android.keyaliasName = "yunstudio";
        PlayerSettings.Android.keyaliasPass = "XCvis8RGbw";
        PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.ScaleToFill;
    }

    private static void CleanRenderEnv()
    {
        UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset = null;
    }

    private static void Build(bool direct)
    {
        BuildBundleConfig.RecordTime("StartBuild");
        //CleanRenderEnv();
        _scenes = FindEnabledEditorScenes();
        EditorUserBuildSettings.SwitchActiveBuildTarget(_group, _target);
        if (Directory.Exists(_targetDir))
        {
            try
            {
                Directory.Delete(_targetDir, true);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        Directory.CreateDirectory(_targetDir);
        BuildBundleConfig.RecordTime("StartPriorBuild");
        OnPriorBuild(direct);
        string lastName = "";
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
                lastName = ".apk";
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                lastName = ".exe";
                break;
        }

        string dest = Path.Combine(_targetDir, "cfgame" + lastName);
        BuildBundleConfig.RecordTime("StartPlayerBuild:" + _build + "  ");


        if (EditorUserBuildSettings.development)
        {
            _build |= BuildOptions.Development;
            if (EditorUserBuildSettings.connectProfiler)
                _build |= BuildOptions.ConnectWithProfiler;
            if (EditorUserBuildSettings.allowDebugging)
                _build |= BuildOptions.AllowDebugging;
        }

        ZeusSettings();
        CleanIL2CPPCache();
        Zeus.Build.BuildScript.BuildPlayer(_target);
        //var res = BuildPipeline.BuildPlayer (_scenes, dest, _target, _build);
        BuildBundleConfig.RecordTime("StartPostBuild");
        if (!direct) OnPostBuild();
        AssetDatabase.Refresh();
        //EditorUtility.DisplayDialog("Package Build Finish", "Package Build Finish!(" + res.summary + ")", "OK");
        //HelperEditor.Open(_targetDir);
        if (_macro.Contains("BUILD_REPORT"))
        {
            List<string> allLineArgs = new List<string>();
            allLineArgs.AddRange(Environment.GetCommandLineArgs());
            int nIndex = allLineArgs.IndexOf("-logFile");
            string editorLogPath = string.Empty;
            if (nIndex >= 0)
            {
                editorLogPath = allLineArgs[++nIndex];
            }

            Debug.Log("editorLogPath:" + editorLogPath);

            string pathBuildReport = BuildReportTool.ReportGenerator.CreateReport(editorLogPath);
            Debug.Log("pathBuildReport:" + pathBuildReport);

            try
            {
                string xlsxPath = editorLogPath.Replace(".log", ".xlsx");
                PieGameExcel.PieGameExcelShows(xlsxPath);
            }
            catch (Exception ep)
            {
                Debug.LogError(ep);
            }
        }


        BuildBundleConfig.RecordTime("EndBuild");
    }


    private static void BuildScriptOnly(bool direct)
    {
        BuildBundleConfig.RecordTime("StartBuild");
        //CleanRenderEnv();
        _scenes = FindEnabledEditorScenes();
        EditorUserBuildSettings.SwitchActiveBuildTarget(_group, _target);
        if (Directory.Exists(_targetDir))
        {
            try { Directory.Delete(_targetDir, true); }
            catch (System.Exception e) { Debug.Log(e.Message); }
        }
        Directory.CreateDirectory(_targetDir);
        BuildBundleConfig.RecordTime("StartPriorBuild");
        if (!direct) StreammingFileBuild.ProcessStreamingFiles(_target);
        string lastName = "";
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
                lastName = ".apk";
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                lastName = ".exe";
                break;
        }
        string dest = Path.Combine(_targetDir, "cfgame" + lastName);
        BuildBundleConfig.RecordTime("StartPlayerBuild:" + _build + "  ");


        if (EditorUserBuildSettings.development)
        {
            _build |= BuildOptions.Development;
            if (EditorUserBuildSettings.connectProfiler)
                _build |= BuildOptions.ConnectWithProfiler;
            if (EditorUserBuildSettings.allowDebugging)
                _build |= BuildOptions.AllowDebugging;
        }

        ZeusSettings();
        CleanIL2CPPCache();

        List<string> levels = new List<string>();
        levels.Add(@"assets\scenes\default\empty.unity");
        BuildPipeline.BuildPlayer(levels.ToArray(), dest, _target, BuildOptions.BuildScriptsOnly);

        //var res = BuildPipeline.BuildPlayer (_scenes, dest, _target, _build);
        BuildBundleConfig.RecordTime("StartPostBuild");
        if (!direct) OnPostBuild();
        AssetDatabase.Refresh();
        //EditorUtility.DisplayDialog("Package Build Finish", "Package Build Finish!(" + res.summary + ")", "OK");
        //HelperEditor.Open(_targetDir);
        BuildBundleConfig.RecordTime("EndBuild");
    }

    private static void CleanIL2CPPCache()
    {
        /// fix Unity Bug https://forum.unity.com/threads/android-il2cpp-build-crashes-while-select-both-arm64-and-armv7-architecture.1113217/
        string[] dirs = Directory.GetDirectories(Application.dataPath.Replace("Assets", "Library"));
        for (int i = 0; i < dirs.Length; ++i)
        {
            if (dirs[i].ToLower().Contains("il2cpp"))
                Directory.Delete(dirs[i], true);
        }
    }

    private static void ZeusSettings()
    {
        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.OUTPUT_PATH, _targetDir);
        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.PACKAGE_NAME, "cfgame");
        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.PROVISIONING_PROFILE_SPECIFIER, "cfgame");

        TextAsset content = Resources.Load<TextAsset>("downloadserver");
        string hotfixUrl = "";
        string hotfixControlDataUrl = "";
        string subpackageUrl = "";
        string url = "";
        if (content != null)
        {
            url = content.text.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
            hotfixUrl = url;
            hotfixControlDataUrl = url;
            subpackageUrl = url;
        }

        content = Resources.Load<TextAsset>("packinfo");
        if (content != null)
        {
            if (url.EndsWith("/hot/"))
            {
                url = url + content.text.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "") + "/";
                hotfixUrl = url;
                hotfixControlDataUrl = url;
                subpackageUrl = url;
            }
            else
            {
                hotfixUrl = string.Format(url, "hotfix", "");
                hotfixControlDataUrl = hotfixUrl.Replace("lf-game-lf", "rt-game-lf");
                subpackageUrl = string.Format(url, "subpackage", "bytedance/subpackage/");
            }
        }

        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.BUNDLE_VERSION, _version);
        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.SUBPACKAGE_SERVER_URL + "0", subpackageUrl);
        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.HOTFIX_SERVER_URL + "0", hotfixUrl);
        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.HOTFIX_CONTROL_DATA_URL + "0", hotfixControlDataUrl);
        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.HOTFIX_INDEPENDENT_CONTROL_DATA_URL, true.ToString());
        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.HOTFIX_OPEN, true.ToString());
        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.USE_BUNDLELOADER, true.ToString());
        CommandLineArgs.Add(GlobalBuild.CmdArgsKey.HOTFIX_VERSION, PlayerSettings.Android.bundleVersionCode.ToString());
        if (EditorUserBuildSettings.development)
        {
            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_DEVELOPMENT_BUILD, true.ToString());
            if (EditorUserBuildSettings.connectProfiler)
                CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_AUTOCONNECT_PROFILER, true.ToString());
            else CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_AUTOCONNECT_PROFILER, false.ToString());
            if (EditorUserBuildSettings.allowDebugging)
                CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_ALLOW_DEBUGGING, true.ToString());
            else CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_ALLOW_DEBUGGING, false.ToString());
            if (EditorUserBuildSettings.buildWithDeepProfilingSupport)
                CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_DEEP_PROFILING, true.ToString());
            else CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_DEEP_PROFILING, false.ToString());
        }
        else
        {
            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_DEVELOPMENT_BUILD, false.ToString());
            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_AUTOCONNECT_PROFILER, false.ToString());
            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_ALLOW_DEBUGGING, false.ToString());
        }

        if (_macro.Contains("MONO2X"))
        {
            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.SCRIPTING_BACKEND, "Mono");
            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.ANDROID_TARGET_ARCHITECTURES, "ARMv7");
        }
        else
        {
            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.SCRIPTING_BACKEND, "IL2CPP");

            string specialArchitectures = "";

            if (_macro.Contains("ARMv7"))
            {
                specialArchitectures = "ARMv7";
            }
            else if (_macro.Contains("ARM64"))
            {
                specialArchitectures = "ARM64";
            }

            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.ANDROID_TARGET_ARCHITECTURES, specialArchitectures);
        }

        if (_macro.Contains("ANDROID_PROJECT"))
        {
            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_BUILD_ANDROID_PROJECT, true.ToString());
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            PlayerSettings.stripEngineCode = false;
        }
        else
        {
            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_BUILD_ANDROID_PROJECT, false.ToString());
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        }
    }

    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;

            if (scene.path.Contains("Assets/Scenes/entrance.unity"))
                scenes.Add(scene);
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        return EditorScenes.ToArray();
    }


    private static Action OnUnityScripsCompilingCompletedCallBack = null;
    public static void SwitchPlatForm(TPlatform platformType, Action callBack)
    {
        Debug.Log("SwitchPlatForm:" + platformType);


        // CompilationPipeline.compilationFinished += compilationFinished;
        OnUnityScripsCompilingCompletedCallBack = callBack;
        //EditorApplication.update += Update;
        switch (platformType)
        {
            case TPlatform.Win32:
                PlayerSetting_Win32();
                break;
            case TPlatform.iOS:
                PlayerSetting_iOS();
                break;
            case TPlatform.Android:
                PlayerSetting_Android();
                break;
            default:
                Debug.Log("Unknown platform " + platformType);
                break;
        }

        compilationFinished(null);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (OnUnityScripsCompilingCompletedCallBack != null) OnUnityScripsCompilingCompletedCallBack();
        OnUnityScripsCompilingCompletedCallBack = null;
    }

    private static void compilationFinished(System.Object obj)
    {

        //CompilationPipeline.compilationFinished -= compilationFinished;

        Debug.Log("compilationFinished:" + _macro);

        _build = BuildOptions.None;

        if (_macro.Contains("_USE_DEV_BUILD"))
        {
            EditorUserBuildSettings.development = true;
            _build |= BuildOptions.Development;
            Debug.Log("_USE_DEV_BUILD!");

            if (_macro.Contains("_USE_DEEP_PROFILER"))
            {
                EditorUserBuildSettings.buildWithDeepProfilingSupport = true;
                _build |= BuildOptions.EnableDeepProfilingSupport;
                Debug.Log("_USE_DEEP_PROFILER!");
            }
            else
            {
                EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
            }
        }
        else
        {
            EditorUserBuildSettings.development = false;
        }


        if (_macro.Contains("_SRC_DEBUG_BUILD"))
        {
            EditorUserBuildSettings.allowDebugging = true;
            _build |= BuildOptions.AllowDebugging;
        }
        else
            EditorUserBuildSettings.allowDebugging = false;


        if (_macro.Contains("_CON_PRF_BUILD"))
        {
            EditorUserBuildSettings.connectProfiler = true;
            _build |= BuildOptions.ConnectWithProfiler;
        }
        else
            EditorUserBuildSettings.connectProfiler = false;

        if (_macro.Contains("ENCRYPT_ASSET_BUNDLE"))
            Encrypt.EncryptAssetBundle.BundleEncrypt = true;
        else
            Encrypt.EncryptAssetBundle.BundleEncrypt = false;

        if (_macro.Contains("MASK_ASSET_BUNDLE"))
            Encrypt.EncryptAssetBundle.Mask_Asset_Bundle = true;
        else
            Encrypt.EncryptAssetBundle.Mask_Asset_Bundle = false;

        if (_macro.Contains("DIVIDE_PACKAGE_CONFIG"))
        {
            CopyDividePackage();
        }
    }

    public static  void  CopyDividePackage()
    {
        var srcFolder = "./DividePackageConfig";
        var desFolder = "./AssetListLog";
        var allFile = Directory.GetFiles(srcFolder, "*.json", SearchOption.AllDirectories);
        for (int i = 0; i < allFile.Length; i++)
        {
              
            var desFile = allFile[i].Replace(srcFolder, desFolder);
            Debug.Log($"拷贝分包文件 {allFile[i]}");

            if (File.Exists(desFile))
            {
                FileUtil.ReplaceFile(allFile[i], desFile);
            }
            else
            {
                var folder = Path.GetDirectoryName(desFile);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                FileUtil.CopyFileOrDirectory(allFile[i], desFile);
            }
        }
    }
    
    private static void Update()
    {
        if (OnUnityScripsCompilingCompletedCallBack != null && !EditorApplication.isCompiling)
        {


        }
    }

    private static void BuildOuterPackFileList(ref string fileStr, string path, string searchPattern)
    {
        string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, path), searchPattern, SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
            if (!string.IsNullOrEmpty(fileStr)) fileStr += '|';
            fileStr += files[i].Replace(Path.Combine(Application.streamingAssetsPath, path), path);
        }
    }
    [MenuItem("Tools/Build/Test")]
    private static void ZeusPreBuild()
    {
        if (!Directory.Exists(Path.Combine(Application.streamingAssetsPath, "bundleres")))
            Directory.Move(Path.Combine(Application.streamingAssetsPath, "Bundles/assets/bundleres"), Path.Combine(Application.streamingAssetsPath, "bundleres"));

        string str = string.Empty;
        BuildOuterPackFileList(ref str, "bundleres", "*bytes");
        BuildOuterPackFileList(ref str, "lua", "*.lua.txt");

        System.IO.File.WriteAllText(Path.Combine(Application.dataPath, "Resources/filelist.bytes"), str);
        
        //         if (!Directory.Exists(Path.Combine(Application.streamingAssetsPath,"update")))
        //             Directory.CreateDirectory(Path.Combine(Application.streamingAssetsPath, "update"));
        // 
        //         string conf = HelperEditor.basepath + "/Shell/zip.txt";
        //         string[] lines = File.ReadAllLines(conf);
        //         for (int i = 0; i < lines.Length; i++)
        //         {
        //             if (!string.IsNullOrEmpty(lines[i]) && lines[i].StartsWith("update"))
        //             {
        //                 var from = Path.Combine(Path.Combine(Application.streamingAssetsPath, "Bundles/assets/bundleres"), Path.GetFileName(lines[i]));
        //                 var dest = Path.Combine(Application.streamingAssetsPath, lines[i]);
        //                 if (Directory.Exists(from))
        //                 {
        //                     Directory.Move(from, dest);
        //                     File.Delete(from + ".meta");
        //                 }
        //             }
        //         }
        AssetDatabase.Refresh();
    }

    private static bool OnPriorBuild(bool direct)
    {
        if (!direct) StreammingFileBuild.ProcessStreamingFiles(_target);
        ZeusPreBuild();
        // if (_target == BuildTarget.Android)
        // {
        //     Dll2Bytes(false);
        //     TextAsset data = Resources.Load<TextAsset>("CFClient");
        //     return data != null && data.bytes.Length > 0;
        // }
        if (_target == BuildTarget.iOS)
        {
            iOS_ECS(false);
        }
        return true;
    }

    private static void OnPostBuild()
    {
        // if (_target == BuildTarget.Android)
        // {
        //     Dll2Bytes (true);
        // }
        if (_target == BuildTarget.iOS)
        {
            iOS_ECS(true);
        }
    }

    private static void iOS_ECS(bool reverse)
    {
        string dst = "Assets/Lib/XEcsGamePlay.dll";
        string src = "Assets/Lib/XEcsGamePlay.dll.iOS";
        string mdb = "Assets/Lib/XEcsGamePlay.dll.mdb";
        string tmp = "Assets/Lib/XEcsGamePlay.dll.bkup";
        AssetDatabase.DeleteAsset(mdb);
        if (reverse)
        {
            AssetDatabase.MoveAsset(dst, src);
            AssetDatabase.MoveAsset(tmp, dst);
            AssetDatabase.ImportAsset(src);
            AssetDatabase.ImportAsset(dst);
        }
        else
        {
            AssetDatabase.MoveAsset(dst, tmp);
            AssetDatabase.MoveAsset(src, dst);
            AssetDatabase.ImportAsset(tmp);
            AssetDatabase.ImportAsset(dst);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void Dll2Bytes(bool reverse)
    {
        string src = "Assets/Lib/CFClient.dll";
        string dst = "Assets/Resources/CFClient.bytes";
        if (reverse)
        {
            AssetDatabase.MoveAsset(dst, src);
            AssetDatabase.ImportAsset(src);
        }
        else
        {
            AssetDatabase.MoveAsset(src, dst);
            AssetDatabase.ImportAsset(dst);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void BuildAndroidAnalysis()
    {
        BuildBundleConfig.RecordTime("Start BuildAndroidAnalysis");
        AndroidApkAnalysis();
        BundleBuildAnalysis();
        BuildBundleConfig.RecordTime("End BuildAndroidAnalysis");
    }

    private static void AndroidApkAnalysis()
    {
        BuildBundleConfig.RecordTime("AndroidApkAnalysis Start");

        //apk路径
        string apkPath = Application.dataPath.Replace("Assets", "") + "Android/cfgame.apk";

        //apk大小
        PrintFileVersionInfo(apkPath);

        //计算apk文件数量
        FileInZipCount(apkPath);

        BuildBundleConfig.RecordTime("AndroidApkAnalysis End");
    }

    private static void BundleBuildAnalysis()
    {
        BuildBundleConfig.RecordTime("Start BundleBuildAnalysis");
        string platformName = UnityEngine.AssetGraph.BuildTargetUtility.TargetToAssetBundlePlatformName(_target);

        //var exportPath = GetExportPath(m_exportPath[target]);
        var exportPath = Application.dataPath.Replace("Assets", "Bundles/") + platformName;
        Debug.Log("exportPath:" + exportPath);

        //AB包总大小
        //AB包总数量
        GetDirectorySize(exportPath);

        //每个AB包里的文件数量

        var path = Directory.GetParent(Application.dataPath) + "/" + Zeus.Framework.Asset.AssetBundleUtils._GetBundleRootPath() + Zeus.Framework.Asset.AssetBundleUtils.AssetMapNameXml;
        Dictionary<string, string> _assetMapBundles;
        Dictionary<string, List<string>> _bundleMapAsset;
        Zeus.Framework.Asset.AssetBundleUtils._LoadAssetMapXML(path,out _assetMapBundles, out _bundleMapAsset);
        //foreach (var item in _assetMapBundles)
        //{
        //    Debug.Log(item.Key + ", " + item.Value);
        //}

        foreach (var item in _bundleMapAsset)
        {
            uint count = 0;
            foreach (var item1 in item.Value)
            {
                count += 1;
            }
            Debug.Log("AB包里的文件数量:"+item.Key+" count:"+count);
        }
    }

    /// <summary>
    /// 获取文件夹大小
    /// </summary>
    /// <param name="dirPath"></param>
    /// <returns></returns>
    static void GetDirectorySize(string dirPath)
    {
        if (!System.IO.Directory.Exists(dirPath))
            return ;
        long len = 0;
        long count = 0;
        //获取di目录中所有文件的大小
        foreach (FileInfo item in GetDirectoryFile(dirPath))
        {
            if (item.Extension == ".ab")
            {
                len += item.Length;
                count += 1;
            }
        }
        Debug.Log("AB包总大小:" + System.Math.Ceiling(len / 1024.0 / 1024.0) + " M");
        Debug.Log("AB包总数量:" + count);
    }

    static List<FileInfo> GetDirectoryFile(string dirPath)
    {
        if (!System.IO.Directory.Exists(dirPath))
            return null;
        DirectoryInfo di = new DirectoryInfo(dirPath);
        List<FileInfo> fileInfo = new List<FileInfo>();
        foreach (FileInfo item in di.GetFiles())
        {
            if (item.Extension == ".ab")
            {
                fileInfo.Add(item);
            }
        }

        DirectoryInfo[] dis = di.GetDirectories();
        if (dis.Length > 0)
        {
            for (int i = 0; i < dis.Length; i++)
            {
                fileInfo.AddRange(GetDirectoryFile(dis[i].FullName));
            }
        }
        return fileInfo;
    }



    /// <summary>
    /// 打印指定文件的大小信息
    /// </summary>
    /// <param name="path">指定文件的路径</param>
    static void PrintFileVersionInfo(string path)
    {
        System.IO.FileInfo fileInfo = null;
        try
        {
            fileInfo = new System.IO.FileInfo(path);
            if (fileInfo != null && fileInfo.Exists)
            {
                Debug.Log("文件大小=" + System.Math.Ceiling(fileInfo.Length / 1024.0 / 1024.0) + " M");
            }
            else
            {
                Debug.Log("指定的文件路径不正确!");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    /// <summary>
    /// 根据压缩包路径读取此压缩包内文件个数
    /// </summary>
    /// <param name="strAimPath"></param>
    /// <returns></returns>
    private static void FileInZipCount(string strAimPath)
    {
        FileStream fsFile_ = null;
        ICSharpCode.ZeusSharpZipLib.Zip.ZipFile zipFile_ = null;
        try
        {
            fsFile_ = new FileStream(strAimPath, FileMode.Open);
            zipFile_ = new ICSharpCode.ZeusSharpZipLib.Zip.ZipFile(fsFile_);
            //foreach (ZipEntry z in zipFile_)
            //{
            //    Debug.LogError(z.Name);
            //}

            long l_New = zipFile_.Count;
            Debug.Log("apk文件数量" + l_New);

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        finally
        {
            if (zipFile_ != null)
            {
                zipFile_.Close();
            }
            if (fsFile_ != null)
                fsFile_.Close();
        }
    }
}

public partial class JenkinsBuild
{
    [InitializeOnLoadMethod]
    static void OnlyLoadProject()
    {
        if (Application.isBatchMode)
        {
            var onlyLoad = bool.TryParse(Environment.GetEnvironmentVariable("OnlyLoadProject")?.ToLower(), out bool value) ? value : false;

            MonoBehaviour.print($"InitializeOnLoadMethod OnlyLoadProject : {onlyLoad}");

            if (onlyLoad)
            {
                try
                {
                    UnityEngine.AssetGraph.AssetGraphEditorWindow.BuileAssetBundle(EditorUserBuildSettings.activeBuildTarget, false);
                    EditorApplication.Exit(0);
                }
                catch (Exception)
                {
                    EditorApplication.Exit(-1);
                }
            }
        }
    }
}