using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using GMSDK;
using System.Linq;
using System.Reflection;
using GSDK;
using GSDK.UnityEditor;
using Debug = UnityEngine.Debug;

/// <summary>
/// 命令行编译
/// </summary>
public class CommandBuild
{
    const string unityVersion = "GMSDK/GSDK Unity Plugin Version: " + MainSDK.Version;
    [MenuItem(unityVersion, true, -20)]
    static bool ValidateSelection()
    {
        return false;
    }
    [MenuItem(unityVersion, false, -20)]
    public static void ShowUnityVersion()
    {

    }
    
#if UNITY_IOS
    [MenuItem("GMSDK/当前平台：iOS", true, -15)]
#elif UNITY_ANDROID
    [MenuItem("GMSDK/当前平台：Android", true, -15)]
#endif
    public static bool ValidatePlatformSelection()
    {
        return false;
    }
    
#if UNITY_IOS
    [MenuItem("GMSDK/当前平台：iOS", false, -15)]
#elif UNITY_ANDROID
    [MenuItem("GMSDK/当前平台：Android", false, -15)]
#endif
    public static void ShowCurrentPlatform() { }
    
    [MenuItem("GMSDK/SDK Config Settings/Show Settings", false, -12)]
    public static void ShowSDKConfig()
    {
        LoadPanelContent.GetInstance();
        Selection.activeObject = BaseSDKConfigSetting.Instance;
    }
    
    [MenuItem("GMSDK/SDK Config Settings/Set Language/Chinese", false, -11)]
    public static void ShowSDKConfigWithChinese()
    {
        LoadPanelContent.GetInstance().ChangeLanguage(LoadPanelContent.Language_Chinese);
        BaseSDKConfigSetting.Instance = null;
        Selection.activeObject = BaseSDKConfigSetting.Instance;
    }
    
    [MenuItem("GMSDK/SDK Config Settings/Set Language/English", false, -11)]
    public static void ShowSDKConfigWithEnglish()
    {
        LoadPanelContent.GetInstance().ChangeLanguage(LoadPanelContent.Language_English);
        BaseSDKConfigSetting.Instance = null;
        Selection.activeObject = BaseSDKConfigSetting.Instance;
    }

    [MenuItem("GMSDK/Change Build System To iOS", false, 2)]
    private static void SetBuildSettingiOS()
    {
        //Switch to iOS build target if not
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
    }

    [MenuItem("GMSDK/Gen Xcode Project", false, 3)]
    public static void BuildIOS()
    {
        Dictionary<string, string> cmdArgs = GMSDKUtil.ParseCommandLineArgs();

        if (Type.GetType("DebugCommandBuild") != null)
        {
            var region = cmdArgs.ContainsKey("region") ? cmdArgs["region"] : null;
            if (region == null)
            {
                region = "domestic";
            }
            GMSDKUtil.UpdateDefineSymbols(new Dictionary<string, bool>(){
                { "GM_Domestic", region != "overseas" },
                { "GM_Overseas", region == "overseas" },
            });
            
            var deployType = cmdArgs.ContainsKey("deploy-type") ? cmdArgs["deploy-type"] : null;
            if (deployType == null)
            {
                deployType = "adhoc";
            }
            GMSDKUtil.UpdateDefineSymbols(new Dictionary<string, bool>(){
                { "GM_APPID_Adhoc", region != "inhouse" },
                { "GM_APPID_Inhouse", region == "inhouse" },
            });
        }

        List<string> levels = new List<string>();

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            if (EditorBuildSettings.scenes[i].enabled)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                Debug.Log("==>" + scenePath);
                if (File.Exists(scenePath))
                {
                    levels.Add(scenePath);
                }
                else
                {
                    Debug.Log("==>not exist");
                }
            }
        }

        GMSDKUtil.UpdateDefineSymbols(GMSDKUtil.ParseCommandLineMacros());

        string exportPath = cmdArgs.ContainsKey("export-path") ? cmdArgs["export-path"] : null;

        SetBuildSettingiOS();
        UpdateIOSPlayerSettings();
        EditorUserBuildSettings.development = false;
        GMSDKEnv env = GMSDKEnv.Instance;

        if (exportPath == null)
        {
            exportPath = env.WORKSPACE + "/xcode";
        }
        BuildPipeline.BuildPlayer(levels.ToArray(), exportPath, BuildTarget.iOS, BuildOptions.Development/* | BuildOptions.AcceptExternalModificationsToPlayer*/);
    }

    private static void UpdateIOSPlayerSettings()
    {
#if UNITY_IOS
        
        SDKConfigModule _sdkConfigModule = BaseSDKConfigSetting.Instance._sdkConfigModule;

        //配置bundleID
        if (_sdkConfigModule.commonModule.iOS_bundleId.Length > 0)
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, _sdkConfigModule.commonModule.iOS_bundleId); 
        }
        if (_sdkConfigModule.commonModule.iOS_app_display_name.Length > 0)
        {
            PlayerSettings.productName = _sdkConfigModule.commonModule.iOS_app_display_name;
        }
#endif
    }
    
    [MenuItem("GMSDK/Change Build System To Android", false, 103)]
    private static void SetBuildSettingAndroid()
    {
        BaseDeploy.SetBuildSettingAndroid();
    }

    [MenuItem("GMSDK/Gen Android Apk", false, 105)]
    public static string BuildAndroidApk()
    {
        string apkOutputPath = "";
        bool symbolsSwitch = false;
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildApkPath")
            {
                apkOutputPath = args[i + 1];
                break;
            }
            else if (args[i] == "-symbols")
            {
                bool.TryParse(args[i + 1], out symbolsSwitch);
            }
        }
        // EditorUserBuildSettings.androidCreateSymbolsZip = symbolsSwitch;
        if (apkOutputPath == "")
        {
            apkOutputPath = EditorUtility.SaveFilePanel("", "gcode", "gradleOut", "apk");
        }
        Debug.Log("output apk path:" + apkOutputPath);
#if UNITY_2019_3_OR_NEWER
        PlayerSettings.Android.useCustomKeystore = true;
#endif
        PlayerSettings.Android.keystoreName = "CI/bytedance.keystore";
        PlayerSettings.Android.keystorePass = "122699fang";
        PlayerSettings.Android.keyaliasName = "funnygallery";
        PlayerSettings.Android.keyaliasPass = "122699fang";
        EditorUserBuildSettings.development = false;
        //BuildOptions addoptions = BeforeBuild();
        new BaseDeploy().BuildAndroidApk(apkOutputPath);
        AfterBuild(apkOutputPath);
        return apkOutputPath;
    }
    
    [MenuItem("GMSDK/Build And Run Apk", false, 106)]
    public static void BuildAndRunApk()
    {
        string apkOutputPath = BuildAndroidApk();
        //安装apk并拉起MainActivity
        string mainActivity = "";
        mainActivity = "com.zxgn.gsdk/com.bytedance.unity.UnityPlayerActivity";
        
        
        Process proc = new Process
        {
            StartInfo =
            {
                FileName = "sh",
                Arguments = Path.Combine(GMSDKEnv.Instance.SubEditorPath("GMSDK"), "Scripts/DeployAndroid/installAndRunApk.sh") + " " + apkOutputPath + " " + mainActivity,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = false,
                ErrorDialog =  false,
                RedirectStandardInput = false ,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                WorkingDirectory =  null
            }
        };

        proc.Start();
        proc.WaitForExit();
        // var sr = proc.StandardOutput;
        // string str = sr.ReadToEnd();
        proc.Close();
       
    }
    

    
    

    /// <summary>
    /// 
    /// </summary>
    /// <returns>自定义配置打包选项</returns>
    public static BuildOptions BeforeBuild()
    {
        return BuildOptions.None;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="packagePath">打包输出路径,用于打包后对包体进行MD5计算,安全加固,自动上传cdn等处理</param>
    public static void AfterBuild(string packagePath)
    {
       
        
    }

    [MenuItem("GMSDK/GMSDK Unity Wiki", false, 201)]
    public static void OpenWiki()
    {
        string url = "http://doc.bytedance.net/docs/394/";
        Application.OpenURL(url);
    }
    
    [MenuItem("GMSDK/Download Tools", false, 301)]
    public static void ShowDownloadTool()
    {
        Selection.activeObject = DownloadSettings.Instance;
    }
    
    [MenuItem("GMSDK/Change Build System To PC", false, 401)]
    private static void SetBuildSettingPC()
    {
        //Switch to iOS build target if not
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
                BuildTarget.StandaloneWindows64);
        }
        //去掉 GMEnderOn 宏
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        string newSymbols = ClearEnderSymbols(symbols);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newSymbols);
    }
    
    [MenuItem("GMSDK/PC Config Settings", false, 401)]
    public static void EditPCConfig()
    {
        Selection.activeObject = PCConfigSettings.Instance;
    }
    
    [MenuItem("GMSDK/Gen Win32 Exe", false, 401)]
    public static void BuildWin32Exe()
    {
        BuildWinExe(false);
    }


    [MenuItem("GMSDK/Gen Win64 Exe", false, 401)]
    public static void BuildWin64Exe()
    {
        BuildWinExe(true);
    }

    
    private static void BuildWinExe(bool is64bit = true)
    {
        var exeOutputPath = "";
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildExePath")
            {
                exeOutputPath = args[i + 1];
            }
        }

        if (string.IsNullOrEmpty(exeOutputPath))
        {
            exeOutputPath = EditorUtility.SaveFilePanel("exe文件保存路径", "gcode", "test.exe", "exe");
        }
        Debug.Log("output exe path: " + exeOutputPath);
        DeployWindows.BuildWindowsExe(exeOutputPath, is64bit);
    }
    static String ClearEnderSymbols(String symbols)
    {
        return ClearSymbols(symbols, new List<string>() { "GMEnderOn"});
    }
    
    
    static String ClearSymbols(String symbols, List<string> arrRemoveSymbols)
    {
        String[] arrSymbol = symbols.Split(new char[1] { ';' });
        String newSymbols = "";
        bool bFirst = true;
        foreach (String s in arrSymbol)
        {
            bool found = false;
            foreach (String removeS in arrRemoveSymbols)
            {
                if (s == removeS)
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                continue;
            }
            else
            {
                if (bFirst)
                {
                    newSymbols += s;
                    bFirst = false;
                }
                else
                {
                    newSymbols = newSymbols + ";" + s;
                }

            }
        }
        return newSymbols;
	}
    [MenuItem("Assets/Sync Solution #&s")]
    public static void SyncSolution()
    {
        var editor = Type.GetType("UnityEditor.SyncVS, UnityEditor");
        var syncSolutionMethod = editor.GetMethod("SyncSolution", BindingFlags.Public | BindingFlags.Static);
        syncSolutionMethod.Invoke(null, null);
        Debug.Log("Solution synced!");
    }
}
