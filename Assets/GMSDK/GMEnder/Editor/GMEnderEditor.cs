using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using System.Reflection;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;
using Timer = System.Timers.Timer;
#if UNITY_EDITOR
using System.Diagnostics;
using System.Timers;
using Ender.LitJson;
using Ender;

[InitializeOnLoad]
public class EnderEditorSettings : ScriptableObject
{
    private const string enderSettingsFileName = "EnderSettings.json";

    private static EnderEditorSettings _instance;
    private static bool enderOn = false;
    private static bool needRefresh = true;
    public static bool useMacro = false;

    public EnderSettingsModel model = new EnderSettingsModel();
    private static string settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), @"GMEnderSettings/");

    public static EnderEditorSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = CreateInstance<EnderEditorSettings>();
                if (Directory.Exists(settingsFolderPath))
                {
                    string fullPath = Path.Combine(settingsFolderPath,
                        enderSettingsFileName);
                    if (System.IO.File.Exists(fullPath))
                    {
                        string configSettingJson = File.ReadAllText(fullPath);
                        _instance.model = JsonMapper.ToObject<EnderSettingsModel>(configSettingJson);
                        EnderRemoteEditorManager.instance.RestoreStatus(_instance.model);
                    }
                }

                useMacro = (existClass("GMSDKUtil") || existClass("GMSDKEnv"));
            }

            return _instance;
        }
    }

    public static bool existClass(string className)
    {
        foreach (Assembly _a in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type _t in _a.GetTypes())
            {
                if ((_t.FullName == className) && _t.IsClass)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void save()
    {
        EditorUtility.SetDirty(this);

        if (!Directory.Exists(settingsFolderPath))
        {
            Directory.CreateDirectory(settingsFolderPath);
        }

        string fullPath = Path.Combine(settingsFolderPath,
            enderSettingsFileName);

        string json = JsonMapper.ToJson(model);
        File.WriteAllText(fullPath, json);
    }

    public void cleanAndclearFile()
    {
        model = new EnderSettingsModel();
        EditorUtility.SetDirty(this);

        if (!Directory.Exists(settingsFolderPath))
        {
            return;
        }

        DirectoryInfo dir = new DirectoryInfo(settingsFolderPath);
        dir.Delete(true);
    }

    public bool forceRefreshUIMacro()
    {
        EditorUtility.SetDirty(this);

        string[] path = UnityEditor.AssetDatabase.FindAssets("GMEnderEditor");
        if (path.Length == 0)
        {
            return false;
        }
        string filePath = AssetDatabase.GUIDToAssetPath(path[0]);
        if (!System.IO.File.Exists(filePath))
        {
            return false;
        }
        string fileContent = File.ReadAllText(filePath);
        if (fileContent.EndsWith("\n"))
        {
            fileContent = fileContent.Substring(0, fileContent.Length - 1);
        }
        else
        {
            fileContent = fileContent + "\n";
        }

        File.WriteAllText(filePath, fileContent);
        return true;
    }

    public void setRefresh()
    {
        needRefresh = true;
    }

    public bool isEnderOn()
    {
        if (needRefresh)
        {
            if (Directory.Exists(settingsFolderPath))
            {
                enderOn = true;
            }
            else
            {
                enderOn = false;
            }

            needRefresh = false;
        }

        bool checkMacro = !useMacro;
#if GMEnderOn
        checkMacro = true;

#endif
        return enderOn && checkMacro;
    }
}

[CustomEditor(typeof(EnderEditorSettings))]
public class GMEnderEditor : Editor
{
    private const string gameTestUrl = "https://game-test.nvsgames.cn/oauth2_fe/third-party?token={0}&deviceId={1}";
    public static EnderEditorSettings instance;
    public static string verifiedIP = "";
    public static bool isVerifying = false;
    public static bool showNotVerifiedTip = false;
    public static string notVerifiedTip = "";
    public static bool showForceRefresh = false;
    public static bool showForceRefreshSwitchTip = false;
    public static int tryTurnOnEnderCount = 0;
    public static string uploadMessage = "";

    //ender remote
    public static int currDeployStatus;
    public static bool isRemoteVerifying;
    public static string remoteVerifiedTip = "";
    public static bool clickVerify;
    public static bool shouldSave;
    public static bool isFailed;
    public static bool isDeploying;
    public static bool isOpen;
    private static Timer _timer;
    private static bool restored;

    public GUIStyle grayFontStyle = new GUIStyle();

    public delegate void enderRecvMsgCallback(string message);

    public delegate void enderConnectionsCallback(int count);

    private int callIndex;
    private static Dictionary<int, string> dicSyncCallResult = new Dictionary<int, string>();

    [DllImport("GMEnderEngine")]
    private static extern void CreateCronetEngine();

    [DllImport("GMEnderEngine")]
    private static extern void InitClientNet(string ip_addr, int port);

    [DllImport("GMEnderEngine")]
    private static extern void TcpClientSend(int msgType, string message);

    [DllImport("GMEnderEngine")]
    private static extern void DisconnectAndDestroyClient();

    [DllImport("GMEnderEngine")]
    private static extern bool HasEnderConnected();

    [DllImport("GMEnderEngine")]
    private static extern void setEnderAlohaMessageCallback(enderRecvMsgCallback callback);

    //[DllImport("GMEnderEngine")]
    //private static extern void setEnderConnectionsCallback(enderConnectionsCallback callback);

    public GMEnderEditor()
    {
        //UnityEngine.Debug.Log("GMEnderEditor init");
        callIndex = 0;
        EnderRemoteEditorManager.instance.SetEnderRemoteCallback(new EnderRemoteCallback());
        EnderRemoteEditorManager.instance.onInstallResult = RefreshDeployStatus;
    }

    private void OnEnable()
    {
        grayFontStyle.normal.textColor = new Color(0.302f, 0.302f, 0.302f);
    }

    ~GMEnderEditor()
    {
        //UnityEngine.Debug.Log("GMEnderEditor uninit");
        setEnderAlohaMessageCallback(null);
        ClearTimer();
    }

    public override void OnInspectorGUI()
    {
        instance = EnderEditorSettings.Instance;
        if (instance.model.currentDeployStatus == EnderRemoteConstants.DeployStatus.Done &&
            instance.model.deviceDeadLine <= EnderRemoteEditorManager.GetTimeStamp())
        {
            Debug.Log("cloud device is released");
            Release();
        }

        if (!restored && instance.model.currentDeployStatus == EnderRemoteConstants.DeployStatus.InstallPackage &&
            instance.model.deployProgress > 0 && instance.model.deployProgress < 1 && !isFailed)
        {
            Debug.Log("restore installing status");
            TimerElapsed();
            isDeploying = true;
            restored = true;
        }
        CommonInfoGUI();
    }

    private void CommonInfoGUI()
    {
        string enderOpenDesc = string.Empty;
        currDeployStatus = instance.model.currentDeployStatus;
        if (shouldSave)
        {
            instance.save();
            shouldSave = false;
        }

        if (instance.isEnderOn())
        {
            enderOpenDesc = "Ender已开启，点击关闭";
        }
        else
        {
            enderOpenDesc = "Ender已关闭，点击开启";
        }

        if (EnderEditorSettings.useMacro)
        {
            enderOpenDesc = enderOpenDesc + "(切换大约10秒钟)";
        }

        GUILayout.Label("Ender配置页面", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUILayout.Label("Ender平台是一款高效率的GSDK调试工具，可以允许游戏工作室同学在PC/Mac的Editor环境中直接对GSDK各个接口进行调试。",
        EditorStyles.wordWrappedLabel);
        if (Application.isPlaying)
        {
            EditorGUI.BeginDisabledGroup(true);
        }

        if (GUILayout.Button(enderOpenDesc))
        {
            if (instance.isEnderOn())
            {
                turnOffEnder();
                verifiedIP = "";
                isVerifying = false;
                showNotVerifiedTip = false;
                notVerifiedTip = "";
            }
            else
            {
                turnOnEnder();
            }
        }
        if (showForceRefresh)
        {
            EditorGUILayout.Space();
            GUILayout.Label("如果Ender无法开启，可点击下方按钮进行强制刷新。点击完成后，键盘按下 Alt(Command) + Tab键两次，切换Unity窗口实现刷新。",
            EditorStyles.wordWrappedLabel);
            if (!showForceRefreshSwitchTip)
            {
                if (GUILayout.Button("强制刷新"))
                {
                    bool ret = instance.forceRefreshUIMacro();
                    if (ret)
                    {
                        showForceRefreshSwitchTip = true;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Ender 强制刷新失败，请联系Ender负责同学周暄承");
                    }
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("请切换Unity窗口实现刷新");
                EditorGUI.EndDisabledGroup();
            }
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("更新config.json并构建App  (默认使用上次成功构建的参数)"))
        {
            EnderHttpUtils.UploadAndroidConfigFile((result) =>
            {
                if (result.Length == 0)
                {
                    uploadMessage = "config.json upload failed";
                }
                else
                {
                    var json = JsonMapper.ToObject(result);
                    uploadMessage = Regex.Unescape(json["message"].ToString());
                }
            });
        }
        if (instance.isEnderOn())
        {
            GUILayout.Space(20);
            EnderRemoteConstants.EnderType currentType =
                (EnderRemoteConstants.EnderType) EditorGUILayout.EnumPopup("选择Ender类型", instance.model.enderType);
            instance.model.enderType = currentType;
            instance.save();
        }
        if (Application.isPlaying)
        {
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (instance.isEnderOn())
        {
            tryTurnOnEnderCount = 0;
            showForceRefresh = false;
            showForceRefreshSwitchTip = false;
            if (instance.model.enderType == EnderRemoteConstants.EnderType.EnderLocal)
            {
                EditorGUILayout.LabelField("输入WarShip App的IP地址 (x.x.x.x)");

                string tmpIP = EditorGUILayout.TextField("IP地址", instance.model.ip);
                if (tmpIP != instance.model.ip)
                {
                    showNotVerifiedTip = false;
                }

                instance.model.ip = tmpIP;
                bool validIP = true;
                string[] arrIP = instance.model.ip.Split('.');
                if (arrIP.Length == 4)
                {
                    for (int i = 0; i < arrIP.Length; i++)
                    {
                        if (arrIP[i].Length == 0)
                        {
                            validIP = false;
                            break;
                        }

                        try
                        {
                            if (int.Parse(arrIP[i]) < 0 || int.Parse(arrIP[i]) >= 256)
                            {
                                validIP = false;
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            validIP = false;
                            break;
                        }
                    }
                }
                else
                {
                    if (instance.model.ip.Length != 0)
                    {
                        validIP = false;
                    }
                }

                if (!validIP)
                {
                    GUILayout.Label("IP输入无效", EditorStyles.miniBoldLabel);
                }

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("端口", 9898);
                EditorGUI.EndDisabledGroup();
                if (!validIP || instance.model.ip.Length == 0 || Application.isPlaying)
                {
                    EditorGUI.BeginDisabledGroup(true);
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("验证 (约2~3秒)"))
                {
                    if (isVerifying)
                    {
                        return;
                    }

                    isVerifying = true;
                    verifiedIP = "";
                    setEnderAlohaMessageCallback(HandleAlohaMsgFromNative);
                    CreateCronetEngine();
                    InitClientNet(instance.model.ip, 9898);
                    Thread.Sleep(1000);

                    if (HasEnderConnected())
                    {
                        bool success = false;
                        object versionObj = callEnder("getEnderNativeVersion", true);
                        if (versionObj != null)
                        {
                            string version = (string) versionObj;
                            if (!GMEnderCommandBuild.isWarshipAppVersionCompatible(version))
                            {
                                notVerifiedTip = "与Warship App版本号不一致，Unity端版本号：" + GMEnderCommandBuild.version +
                                                 ", Warship App版本号：" + version;
                            }
                            else
                            {
                                success = true;
                            }
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("[Ender] getEnderNativeVersion 接口访问失败");
                        }

                        if (success)
                        {
                            object platformObj = callEnder("getEnderPlatform", true);
                            if (platformObj != null)
                            {
                                int platform = (int) platformObj;
                                instance.model.isiOSPlatform = platform == 1;
                                success = true;
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("[Ender] getEnderPlatform 接口访问失败");
                            }
                        }

                        if (success)
                        {
                            verifiedIP = instance.model.ip;
                        }
                    }

                    isVerifying = false;
                    //只保留最后一次且验证成功的IP
                    instance.model.verified = verifiedIP.Length > 0;
                    if (!instance.model.verified)
                    {
                        showNotVerifiedTip = true;
                    }

                    instance.save();
                    setEnderAlohaMessageCallback(null);
                    DisconnectAndDestroyClient();
                }

                if (!validIP || instance.model.ip.Length == 0 || Application.isPlaying)
                {
                    EditorGUI.EndDisabledGroup();
                }

                if (verifiedIP == instance.model.ip && verifiedIP != "")
                {
                    if (instance.model.isiOSPlatform)
                    {
                        GUILayout.Label("IP已验证，可运行(iOS)", EditorStyles.miniBoldLabel);
                    }
                    else
                    {
                        GUILayout.Label("IP已验证，可运行(Android)", EditorStyles.miniBoldLabel);
                    }
                }
                else
                {
                    verifiedIP = "";
                    if (showNotVerifiedTip)
                    {
                        GUILayout.Label(notVerifiedTip.Length == 0 ? "IP验证失败" : notVerifiedTip,
                            EditorStyles.miniBoldLabel);
                    }
                }
            }
            else
            {
                string appId = EditorGUILayout.TextField("应用Id", instance.model.appId);
                instance.model.appId = appId;
                EnderRemoteEditorManager.appId = appId;
                bool disable = String.IsNullOrEmpty(appId);
                string sdkVersion = EditorGUILayout.TextField("GSDK版本", instance.model.sdkVersion);
                instance.model.sdkVersion = sdkVersion;
                disable |= String.IsNullOrEmpty(sdkVersion);
                if (!String.IsNullOrEmpty(sdkVersion) && !Regex.IsMatch(sdkVersion, @"^\d+\.\d+\.\d+(.\d+)?$"))
                {
                    GUILayout.Label("GSDK版本格式不正确", EditorStyles.miniBoldLabel);
                    disable = true;
                }

                EnderRemoteConstants.EnderPlatform currentPlatform =
                    (EnderRemoteConstants.EnderPlatform) EditorGUILayout.EnumPopup("平台", instance.model.platform);
                instance.model.platform = currentPlatform;

                if (instance.model.androidOsVersion > EnderRemoteEditorManager.OS_VERSION_NINE &&
                    currDeployStatus == EnderRemoteConstants.DeployStatus.Done)
                {
                    instance.model.dynamicCode = EditorGUILayout.TextField("动态码", instance.model.dynamicCode);
                    EnderRemoteEditorManager.dynamicCode = instance.model.dynamicCode;
                    instance.model.channelId = instance.model.dynamicCode;
                    if (!instance.model.remoteVerified)
                    {
                        shouldSave = true;
                    }
                }

                if (disable || Application.isPlaying)
                {
                    EditorGUI.BeginDisabledGroup(true);
                }

                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();

                if (currDeployStatus == EnderRemoteConstants.DeployStatus.Done)
                {
                    EditorGUI.BeginDisabledGroup(true);
                }

                if (GUILayout.Button("部署"))
                {
                    if (isDeploying)
                    {
                        Debug.Log("deploying, click invalid");
                        return;
                    }

                    isDeploying = true;
                    UnityEngine.Debug.Log(currDeployStatus);
                    switch (currDeployStatus)
                    {
                        case EnderRemoteConstants.DeployStatus.Init:
                            EnderRemoteEditorManager.instance.Init(appId, sdkVersion, currentPlatform,
                                RefreshDeployStatus);
                            break;
                        case EnderRemoteConstants.DeployStatus.GetInstallPackage:
                            EnderRemoteEditorManager.instance.QueryEnderRemoteInstallPackage(appId, sdkVersion,
                                currentPlatform, RefreshDeployStatus, false);
                            break;
                        case EnderRemoteConstants.DeployStatus.OccupyDevice:
                            EnderRemoteEditorManager.instance.OccupyEnderRemoteDevice(currentPlatform,
                                RefreshDeployStatus, false);
                            break;
                        case EnderRemoteConstants.DeployStatus.InstallPackage:
                            EnderRemoteEditorManager.instance.ReInstallToOtherDeviceIfNeed(currentPlatform,
                                RefreshDeployStatus);
                            break;
                    }
                }

                if (currDeployStatus == EnderRemoteConstants.DeployStatus.Done)
                {
                    EditorGUI.EndDisabledGroup();
                }

                if (currDeployStatus != EnderRemoteConstants.DeployStatus.Done || Application.isPlaying ||
                    (instance.model.androidOsVersion > EnderRemoteEditorManager.OS_VERSION_NINE &&
                     String.IsNullOrEmpty(instance.model.dynamicCode)))
                {
                    EditorGUI.BeginDisabledGroup(true);
                }


                if (GUILayout.Button("验证(约2~3秒)"))
                {
                    clickVerify = true;
                    if (isRemoteVerifying)
                    {
                        return;
                    }

                    isRemoteVerifying = true;

                    if (!instance.model.remoteVerified && 
                        instance.model.androidOsVersion > EnderRemoteEditorManager.OS_VERSION_NINE &&
                        !EnderRemoteEditorManager.instance.IsConnected())
                    {
                        EnderRemoteEditorManager.instance.InitRemoteNetTool(instance.model.appId);
                        Thread.Sleep(1500);
                    }

                    if (EnderRemoteEditorManager.instance.IsConnected())
                    {
                        bool success = false;
                        object versionObj = callEnder("getEnderNativeVersion", true);
                        if (versionObj != null)
                        {
                            string version = (string) versionObj;
                            if (!GMEnderCommandBuild.isWarshipAppVersionCompatible(version))
                            {
                                remoteVerifiedTip = "与Warship App版本号不一致，Unity端版本号：" + GMEnderCommandBuild.version +
                                                    ", Warship App版本号：" + version;
                            }
                            else
                            {
                                success = true;
                            }
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("[Ender] getEnderNativeVersion 接口访问失败");
                        }

                        if (success)
                        {
                            object platformObj = callEnder("getEnderPlatform", true);
                            if (platformObj != null)
                            {
                                int platform = (int) platformObj;
                                instance.model.isiOSPlatform = platform == 1;
                                success = true;
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("[Ender] getEnderPlatform 接口访问失败");
                            }

                            instance.model.remoteVerified = true;
                        }
                    }
                    else
                    {
                        remoteVerifiedTip = "当前未连接，请检查云设备Warship App是否打开，并同意信息保护协议";
                    }

                    isRemoteVerifying = false;
                    instance.save();

                    //成功后释放
                    if (instance.model.remoteVerified)
                    {
                        EnderRemoteEditorManager.instance.Release();
                    }
                }

                if (instance.model.remoteVerified)
                {
                    remoteVerifiedTip = instance.model.platform == EnderRemoteConstants.EnderPlatform.Android ? "已验证，可运行(Android)" : "已验证，可运行(iOS)";
                }
                else if (String.IsNullOrEmpty(remoteVerifiedTip))
                {
                    remoteVerifiedTip = "验证失败";
                }

                if (currDeployStatus != EnderRemoteConstants.DeployStatus.Done || Application.isPlaying ||
                    (instance.model.androidOsVersion > EnderRemoteEditorManager.OS_VERSION_NINE &&
                     String.IsNullOrEmpty(instance.model.dynamicCode)))
                {
                    EditorGUI.EndDisabledGroup();
                }

                GUILayout.EndHorizontal();
                string showText = currDeployStatus != EnderRemoteConstants.DeployStatus.Done ||
                                  (!clickVerify && !instance.model.remoteVerified)
                    ? GetErrorDesc()
                    : remoteVerifiedTip;
                if (!String.IsNullOrEmpty(showText))
                {
                    GUIStyle tipStyle = new GUIStyle {normal = {textColor = Color.red}};
                    GUILayout.Label(showText, isFailed ? tipStyle : EditorStyles.boldLabel);
                }

                GUILayout.Space(20);
                GUILayout.Label("部署流程", EditorStyles.largeLabel);
                var react = EditorGUILayout.BeginVertical();
                if (instance.model.deployProgress > 0)
                {
                   
                    react.height = 16;
                    react.x = 10;
                    react.y += 5;
                    EditorGUI.ProgressBar(react, (float) instance.model.deployProgress,
                        "部署进度");
                    GUILayout.Space(40);
                }
                EditorGUILayout.EndVertical();

                try
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(350));
                }
                catch (Exception e)
                {
                    //ignore 偶现一个布局异常，暂不知道原因，不影响主流程，先catch住
                }
                
                if (isFailed && currDeployStatus == EnderRemoteConstants.DeployStatus.GetInstallPackage)
                {
                    GUILayout.Label("① 获取Warship App  ×", grayFontStyle);
                }
                else if (currDeployStatus > EnderRemoteConstants.DeployStatus.GetInstallPackage)
                {
                    GUILayout.Label("① 获取Warship App  √", EditorStyles.boldLabel);
                    GUILayout.Label("构建时间：" + instance.model.packageBuildTime,
                        EditorStyles.boldLabel, GUILayout.Width(180));
                }
                else
                {
                    GUILayout.Label("① 获取Warship App", grayFontStyle);
                }

                GUILayout.EndHorizontal();
                drawVerticalLine();

                GUILayout.BeginHorizontal(GUILayout.Width(400));
                if (isFailed && currDeployStatus == EnderRemoteConstants.DeployStatus.OccupyDevice)
                {
                    GUILayout.Label("② 查找云设备  ×", grayFontStyle);
                }
                else if (currDeployStatus > EnderRemoteConstants.DeployStatus.OccupyDevice)
                {
                    GUILayout.Label("② 查找云设备  √", EditorStyles.boldLabel);
                    GUILayout.Space(80);
                    if (GUILayout.Button("打开", GUILayout.Width(60)))
                    {
                        isOpen = false;
                        StartDevice();
                    }

                    GUILayout.Space(10);
                    if (GUILayout.Button("续期", GUILayout.Width(60)))
                    {
                        EnderRemoteEditorManager.instance.AddTimeManual(instance.model.deviceSerial, newDeadLine =>
                        {
                            instance.model.deviceDeadLine = newDeadLine;
                            shouldSave = true;
                        });
                    }

                    if (instance.model.deviceDeadLine != 0)
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("设备过期时间：" + EnderRemoteEditorManager.GetDate(instance.model.deviceDeadLine));
                    }
                }
                else
                {
                    GUILayout.Label("② 查找云设备", grayFontStyle);
                }

                GUILayout.EndHorizontal();
                drawVerticalLine();

                GUILayout.BeginHorizontal(GUILayout.Width(305));
                if (isFailed && currDeployStatus == EnderRemoteConstants.DeployStatus.InstallPackage)
                {
                    GUILayout.Label("③ 部署App  ×", grayFontStyle);
                }
                else if (currDeployStatus > EnderRemoteConstants.DeployStatus.InstallPackage)
                {
                    GUILayout.Label("③ 部署App  √", EditorStyles.boldLabel);
                    if (GUILayout.Button("卸载并重新部署", GUILayout.Width(130)))
                    {
                        //重新设置对应变量
                        instance.model.dynamicCode = "";
                        EnderRemoteEditorManager.deviceSerial = instance.model.deviceSerial;
                        EnderRemoteEditorManager.dynamicCode = instance.model.dynamicCode;
                        EnderRemoteEditorManager.androidOsVersion = instance.model.androidOsVersion;
                        EnderRemoteEditorManager.deviceDeadLine = instance.model.deviceDeadLine;
                        EnderRemoteEditorManager.uid = instance.model.uid;
                        EnderRemoteEditorManager.instance.ReInstallNewPackage(appId, sdkVersion, currentPlatform,
                            RefreshDeployStatus);
                        if (instance.model.remoteVerified)
                        {
                            instance.model.remoteVerified = false;
                            instance.model.currentDeployStatus = EnderRemoteConstants.DeployStatus.InstallPackage;
                            shouldSave = true;
                            remoteVerifiedTip = "";
                            clickVerify = false;
                        }
                    }
                }
                else
                {
                    bool isInstalling = currDeployStatus == EnderRemoteConstants.DeployStatus.InstallPackage;
                    GUILayout.Label(
                        isInstalling ? "③ 正在部署，请稍后（约2~4分钟）" : "③ 部署App",
                        isInstalling ? EditorStyles.boldLabel : grayFontStyle);
                }

                GUILayout.EndHorizontal();
                drawVerticalLine();

                GUILayout.BeginHorizontal(GUILayout.Width(305));
                if (currDeployStatus == EnderRemoteConstants.DeployStatus.Done)
                {
                    GUILayout.Label("④ 完成部署  √", EditorStyles.boldLabel);
                    if (GUILayout.Button("释放云资源", GUILayout.Width(130)))
                    {
                        Release();
                    }
                }
                else
                {
                    GUILayout.Label("④ 完成部署", grayFontStyle);
                }

                GUILayout.EndHorizontal();

                if (disable || Application.isPlaying)
                {
                    EditorGUI.EndDisabledGroup();
                }
            }
            if (uploadMessage.Length > 0)
            {
                GUILayout.Label(uploadMessage, EditorStyles.miniBoldLabel);
            }
        }
    }

    private void Release()
    {
        EnderRemoteEditorManager.instance.Release();
        if (instance.model.deviceDeadLine > EnderRemoteEditorManager.GetTimeStamp())
        {
            EnderHttpRequestUtils.ReleaseEnderRemoteDevice(instance.model.deviceSerial);
        }

        currDeployStatus = EnderRemoteConstants.DeployStatus.Init;
        instance.model.ClearRemoteFlag();
        clickVerify = false;
        shouldSave = true;
        isDeploying = false;
        isFailed = false;
        ClearTimer();
    }

    private void drawVerticalLine()
    {
        for (int i = 0; i < 3; i++)
        {
            GUILayout.Space(-2);
            GUILayout.Label(" |");
            GUILayout.Space(-2);
        }
    }

    private class EnderRemoteCallback : IEnderRemoteCallback
    {
        public void HandleEnderRemoteMsgFromNative(string message)
        {
            //do nothing
        }

        public void HandleEnderRemoteAlohaMsgFromNative(string message)
        {
            HandleAlohaMsg(message);
        }

        public void HandleConnectionChange(int count)
        {
            //do nothing
        }
    }

    private void RefreshDeployStatus(int result)
    {
        currDeployStatus = result < 0 ? -result : result + 1;
        UpdateDeployProgress();
        isFailed = result < 0 && result > EnderRemoteConstants.DeployErrorStatus.NoError;
        if (isFailed && currDeployStatus != EnderRemoteConstants.DeployStatus.Done)
        {
            isDeploying = false;
        }

        UnityEngine.Debug.Log("current Deploy status: " + currDeployStatus);
        //部署信息存到config文件中

        if (instance == null || instance.model == null)
        {
            return;
        }

        instance.model.channelId = instance.model.androidOsVersion > EnderRemoteEditorManager.OS_VERSION_NINE
            ? instance.model.dynamicCode
            : EnderRemoteEditorManager.deviceSerial;
        instance.model.uid = EnderRemoteEditorManager.uid;
        instance.model.deviceSerial = EnderRemoteEditorManager.deviceSerial;
        instance.model.packageUrl = EnderRemoteEditorManager.packageUrl;
        instance.model.packageBuildTime = EnderRemoteEditorManager.packageBuildTime;
        instance.model.deviceDeadLine = EnderRemoteEditorManager.deviceDeadLine;
        instance.model.currentDeployStatus = currDeployStatus;
        instance.model.androidOsVersion = EnderRemoteEditorManager.androidOsVersion;
        shouldSave = true;
        if (currDeployStatus == EnderRemoteConstants.DeployStatus.Done)
        {
            Thread.Sleep(1000);
            StartDevice();
        }
    }

    private void UpdateDeployProgress()
    {
        switch (currDeployStatus)
        {
            case EnderRemoteConstants.DeployStatus.GetInstallPackage:
                instance.model.deployProgress = 0.1;
                break;
            case EnderRemoteConstants.DeployStatus.OccupyDevice:
                instance.model.deployProgress = 0.3;
                break;
            case EnderRemoteConstants.DeployStatus.InstallPackage:
                TimerElapsed();
                break;
            case EnderRemoteConstants.DeployStatus.Done:
                ClearTimer();
                instance.model.deployProgress = 1.0;
                break;
        }

        shouldSave = true;
    }

    private void TimerElapsed()
    {
        if (_timer == null)
        {
            _timer = new Timer {Enabled = true, Interval = 1000};
            _timer.AutoReset = true;
            _timer.Elapsed += TimerHandler;
            _timer.Start();
        }
        else
        {
            _timer.Start();
        }
    }

    private void TimerHandler(object source, ElapsedEventArgs e)
    {
        if (instance.model.deployProgress > 0.98)
        {
            ClearTimer();
        }
        else if (instance.model.deployProgress > 0.85)
        {
            instance.model.deployProgress += 0.003;
        }
        else
        {
            instance.model.deployProgress += 0.01;
        }
    }

    private void ClearTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }
    }

    private void StartDevice()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        EnderHttpRequestUtils.QueryDeviceJumpToken(token =>
        {
            if (String.IsNullOrEmpty(token))
            {
                Debug.LogError("QueryDeviceJumpToken is null");
                return;
            }

            Debug.Log("open cloud device serial: " + instance.model.deviceSerial);
            Process.Start(String.Format(gameTestUrl, token, instance.model.deviceSerial));
            isOpen = false;
        });
    }

    private string GetErrorDesc()
    {
        int error = -currDeployStatus;
        if (error != EnderRemoteConstants.DeployErrorStatus.NoError && !isFailed)
        {
            return "";
        }

        string desc = "";
        switch (error)
        {
            case EnderRemoteConstants.DeployErrorStatus.UnavailableInstallPackage:
                desc = "没有可用的安装包，请在GSDK Hub构建应用后重新点击部署";
                break;
            case EnderRemoteConstants.DeployErrorStatus.OccupyDeviceFailed:
                desc = "云设备占用失败，请处理后重新点击部署";
                break;
            case EnderRemoteConstants.DeployErrorStatus.InstallFailed:
                desc = "安装失败，请重新点击部署切换云设备重试";
                break;
            case EnderRemoteConstants.DeployErrorStatus.NoError:
                desc = instance.model.androidOsVersion > EnderRemoteEditorManager.OS_VERSION_NINE
                    ? "完成部署，请输入手机App端显示的动态码并开始进行验证"
                    : "完成部署，请开始进行验证";
                break;
        }

        return desc;
    }

    [MonoPInvokeCallback(typeof(enderRecvMsgCallback))]
    public static void HandleAlohaMsgFromNative(string message)
    {
        HandleAlohaMsg(message);
    }

    private static void HandleAlohaMsg(string message)
    {
        //注意线程切换
        //UnityEngine.Debug.Log("接收到消息：" + message);

        JsonData packet = JsonMapper.ToObject(message);
        //解析出message中的CallEnderID
        if (packet.ContainsKey("CallEnderID"))
        {
            int callID = GMEnderHelper.GetInt(packet, "CallEnderID");
            dicSyncCallResult.Add(callID, message);
            return;
        }
    }

    //[MonoPInvokeCallback(typeof(enderConnectionsCallback))]
    //public static void HandleEnderConnections(int count)
    //{
    //    //注意线程切换
    //    if (count == 1 && isVerifying)
    //    {
    //        verifiedIP = instance.model.ip;
    //    }
    //}

    private object callEnder(string methodName, bool hasRetValue)
    {
        if (instance.isEnderOn())
        {
            JsonData jsonData = new JsonData();
            jsonData["methodName"] = methodName;
            string retJson = callEnderSendMessage(jsonData, hasRetValue);
            //返回的是json
            if (retJson == null)
            {
                return null;
            }

            JsonData ret = JsonMapper.ToObject(retJson);
            int code = GMEnderHelper.GetInt(ret, "code");

            if (code != 0)
            {
                UnityEngine.Debug.LogError("[Ender] {errorcode:" + code + "}");
            }

            object value = GMEnderHelper.GetObject(ret, "value");
            return value;
        }
        else
        {
            return null;
        }
    }

    private string callEnderSendMessage(JsonData jsonData, bool hasRetValue)
    {
        if (jsonData == null)
        {
            return null;
        }

        int callID = callIndex;
        jsonData["CallEnderID"] = callIndex++;
        jsonData["CallEnderType"] = (int) GMCallEnderType.Aloha;
        jsonData["HasRetValue"] = hasRetValue;
        bool sendSuccess = sendMessage(jsonData.ToJson());
        //是否会出现return过快的情况？
        if (hasRetValue && sendSuccess)
        {
            Thread.Sleep(1500);
            string result = null;
            dicSyncCallResult.TryGetValue(callID, out result);
            dicSyncCallResult.Remove(callID);
            return result;
        }

        //返回json
        return null;
    }

    private bool sendMessage(string message)
    {
        if (instance.isEnderOn())
        {
            if (!HasEnderConnected() && !EnderRemoteEditorManager.instance.IsConnected())
            {
                UnityEngine.Debug.LogError("[Ender] 遇到了一些问题，Ender提前断开");
                return false;
            }

            if (HasEnderConnected())
            {
                TcpClientSend(1, message);
            }

            if (EnderRemoteEditorManager.instance.IsConnected())
            {
                EnderRemoteEditorManager.instance.SendMessage(1, message);
            }

            return true;
        }
        else
        {
            return false;
        }
    }


    private void turnOnEnder()
    {
        if (EnderEditorSettings.useMacro)
        {
            String symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
            String newSymbols = AddEnderSymbol(ClearEnderSymbols(symbols));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, newSymbols);

            symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            newSymbols = AddEnderSymbol(ClearEnderSymbols(symbols));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, newSymbols);

            symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            newSymbols = AddEnderSymbol(ClearEnderSymbols(symbols));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newSymbols);

            tryTurnOnEnderCount++;
            if (tryTurnOnEnderCount > 2)
            {
                showForceRefresh = true;
            }
        }

        instance.save();
        instance.setRefresh();
    }

    private void turnOffEnder()
    {
        if (EnderEditorSettings.useMacro)
        {
            String symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
            String newSymbols = ClearEnderSymbols(symbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, newSymbols);

            symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            newSymbols = ClearEnderSymbols(symbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, newSymbols);

            symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            newSymbols = ClearEnderSymbols(symbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newSymbols);
        }

        instance.cleanAndclearFile();
        instance.setRefresh();
    }

    static String ClearSymbols(String symbols, List<string> arrRemoveSymbols)
    {
        String[] arrSymbol = symbols.Split(new char[1] {';'});
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

    static String ClearEnderSymbols(String symbols)
    {
        return ClearSymbols(symbols, new List<String>() {"GMEnderOn"});
    }

    static String AddSymbol(String symbols, String aSymbol)
    {
        if (symbols.Length > 0)
        {
            return symbols + ";" + aSymbol;
        }
        else
        {
            return aSymbol;
        }
    }

    static String AddEnderSymbol(String symbols)
    {
        return AddSymbol(symbols, "GMEnderOn");
    }
}
#endif