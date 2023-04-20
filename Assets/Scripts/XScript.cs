#define PIPELINE_URP

using Assets.Scripts.FMOD;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEngine.UI;
#endif
using CFEngine;
using UnityEngine.Rendering;
using CFClient.GSDK;
using System;
using System.IO;
using CFClient;
using CFUtilPoolLib.GSDK;
using UnityEngine.Scripting;


public class XScript : MonoBehaviour
{
    private SDKSystem sdk = null;
    private LuaBehaviour lua = null;
    private bool needWait = false;

//#if UNITY_EDITOR
//    [System.NonSerialized]
    public uint debugFlag = 0;

    public bool openLog = true;
//#endif

//#if UNITY_ANDROID && !UNITY_EDITOR

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void DelCrashShaderCache()
    {
        string crashShaderCrashPath = Path.Combine(Application.temporaryCachePath, "UnityShaderCache",
            "15eba25636e9ef87fe763a3f2d58e6a3");
        if (File.Exists(crashShaderCrashPath))
        {
            File.Delete(crashShaderCrashPath);
        }
        Debug.Log("del crash shader cache file");
    }
//#endif

    private void Awake()
    {
        DelCrashShaderCache();
        
        EngineUtility.DisableGC();

#if UNITY_EDITOR
        System.AppDomain.CurrentDomain.UnhandledException += U3dhandledExceptionEventHandler;
#endif

#if POCO_OPEN
        var poco = new GameObject("Poco");
        PocoManager po = poco.AddComponent<PocoManager>();
#endif

#if UNITY_EDITOR
        UnityEngine.Debug.unityLogger.logEnabled = true;
        if (!openLog)
            UnityEngine.Debug.unityLogger.filterLogType = LogType.Exception;
        DebugLog.SetFlag((EDebugFlag) debugFlag, true);
#else
#if DISABLE_LOG
            UnityEngine.Debug.unityLogger.logEnabled = true; 
            UnityEngine.Debug.unityLogger.filterLogType = LogType.Exception;
#else
            UnityEngine.Debug.unityLogger.logEnabled = true; 
#endif
#endif

#if SHOW_FPS_FLAG
        DebugLog.SetFlag(EDebugFlag.GUILog, true);
#endif
#if MASK_ASSET_BUNDLE
            if (Application.maskAssetBundle != true)
            {
                Application.maskAssetBundle = true;
            }
#endif
        if (Application.platform != RuntimePlatform.WindowsEditor
#if UNITY_EDITOR
            || UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.AADebug
#endif
        )
            DebugLog.SetFlag(EDebugFlag.FixedTime, true);
#if SHOW_WATERMARK
         DebugLog.SetFlag(EDebugFlag.WaterMark, true);
#endif
        EngineContext.IsRunning = Application.isPlaying;

#if UNITY_EDITOR
        var obj = Resources.Load("UI/DevopsUI");
        var ui = GameObject.Instantiate(obj) as GameObject;
#elif DEVELOPMENT_BUILD
        var obj = Resources.Load("UI/DevopsUI");
        var ui = GameObject.Instantiate(obj) as GameObject;
#endif
    }


#if UNITY_EDITOR
    void U3dhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
    {
        Debug.LogError("U3dhandledExceptionEventHandler:" + sender + "  " + e);
    }
#endif
    void Start()
    {
#if PIPELINE_URP
        EngineContext.UseUrp = true;
        ShaderManager.USE_ALLSHADERPROPERTYS = true;
#else
        GraphicsSettings.renderPipelineAsset = null;
#endif


#if UNITY_EDITOR
        //UIScene.InitDebug ();
        EnvArea.InitDebug();
        WorldSystem.initEditData = EnvironmentExtra.InitEditorData;
#endif
        sdk = new SDKSystem();
        lua = new LuaBehaviour();

        XInterfaceMgr.singleton.AttachInterface<RTimeline>(XInterfaceMgr.RTimelineID, RTimeline.singleton);
        XInterfaceMgr.singleton.AttachInterface<XNativeInfo>(XInterfaceMgr.XNativeID, new XNativeInfo());
        XInterfaceMgr.singleton.AttachInterface<SDKSystem>(XInterfaceMgr.SDKID, sdk);
        XInterfaceMgr.singleton.AttachInterface<IStudioListener>(XInterfaceMgr.StudioListenerID,
            XStudioListener.Instance);
        XInterfaceMgr.singleton.AttachInterface<ILevelInterface>(XInterfaceMgr.LevelID, LevelTools.singleton);
        XInterfaceMgr.singleton.AttachInterface<GSDKSystem>(XInterfaceMgr.GSDKID, GSDKSystem.singleton);
        XInterfaceMgr.singleton.AttachInterface<PostTreatmentSystem>(XInterfaceMgr.PostTreatmentID,
            PostTreatmentSystem.singleton);
        XInterfaceMgr.singleton.AttachInterface<XPerformanceMgr>(XInterfaceMgr.PerformanceID, new XPerformanceMgr());
        XInterfaceMgr.singleton.AttachInterface<SpineSystem>(XInterfaceMgr.SpineMgrID, SpineSystem.singleton);
        EngineContext.renderManager = RenderingManager.instance;
        TimelineAsset.getAnimEnv = AnimEnv.GetAnimEnv;
        InitZeusFramework();
        XUpdater.XUpdater.singleton.PreInit();
        XUpdater.XShell.singleton.InitGSDK();
        UnityGetiOSPermissionsStateBridge.Init();

#if SHOW_FPS_FLAG
        //CFCommand.singleton.RegisterCmd("remote", FPDebug.singleton.StartShaderSwap);
        //CFCommand.singleton.RegisterCmd("profiler", RuntimeProfilerCtrl.Instance.SetActive);
        XInterfaceMgr.singleton.AttachInterface<FPDebug>(XInterfaceMgr.FPDebugID, FPDebug.singleton);
#endif

#if UNITY_ANDROID && !NO_SPLIT_APK


#if !UNITY_EDITOR
        TextAsset packinfo = Resources.Load<TextAsset>("packinfo");
        if(packinfo!=null)
        {
        Android.ExtractInBackground(packinfo.text);
        needWait = true;
        }
#endif
#endif


        if (!needWait) Initial();
    }

    private void Initial()
    {
#if AERIAL
        XDebug.singleton.showAerial = true;
#endif
        XDebug.singleton.AddBlueLog("sdkmanager init begin");

        sdk.Start(transform);
        XDebug.singleton.AddBlueLog("sdkmanager init end");
        //GSDKSystem.singleton.Initialize(this.InitGSDKCallback);
        lua?.Start();
        ThreadManager.Init();
        WorldSystem.InitContext();

        XUpdater.XShell.singleton.Awake();
        XUpdater.XShell.singleton.Start();
    }

    private void InitZeusFramework()
    {
        ZeusAssetManager.singleton.assetManager = new AssetManagerInterface();
        ZeusAssetManager.singleton.outerPackage = new OuterPackageInterface();
        ZeusAssetManager.singleton.zeusFramework = new ZeusFrameworkInterface();
        ZeusAssetManager.singleton.vFileSystem = new VFileSystemInterface();
        ZeusAssetManager.singleton.hotfixService = new HotfixServiceInterface();
    }

    //private void InitGSDKCallback(Result result)
    //{
    //    if (result.IsSuccess)
    //    {
    //        Debug.Log("GSDK Init Success");
    //    }
    //    else
    //    {
    //        Debug.Log("GSDK Init Error : " + result.Error);
    //    }
    //}

    void Update()
    {
#if UNITY_EDITOR
        DebugLog.SetFlag((EDebugFlag) debugFlag, true);
#endif
        XUpdater.XShell.singleton.Update();
        lua?.Update();
        GSDKSystem.singleton.UpdateCallAfterClickAgree();
    }

    void LateUpdate()
    {
        XUpdater.XShell.singleton.PostUpdate();
    }

    void OnApplicationQuit()
    {
        DirectorHelper.singleton.Uninit();
        FMODUnity.RuntimeStudioEventEmitter.isQuitting = true;
        sdk?.OnApplicationQuit();
        lua?.OnApplicationQuit();
        XUpdater.XShell.singleton.Quit();

#if UNITY_EDITOR
        UnityEngine.CFUI.SpriteUtility.StaticInit();
#endif
    }

    private void OnDestroy()
    {
        lua?.OnDestroy();
    }

    private IGameSysMgr mGameSysMgr = null;

    private IGameSysMgr GameSysMgr
    {
        get
        {
            if (mGameSysMgr == null || mGameSysMgr.Deprecated)
                mGameSysMgr = XInterfaceMgr.singleton.GetInterface<IGameSysMgr>(XCommon.singleton.XHash("IGameSysMgr"));
            return mGameSysMgr;
        }
    }

    private void OnApplicationPause(bool pause)
    {
        sdk?.OnApplicationPause(pause);
        GameSysMgr?.GamePause(pause);
        lua?.OnApplicationPause(pause);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        XUpdater.XShell.singleton.OnApplicationFocus(hasFocus);
        //GSDKSystem.singleton.OnApplicationFocus(hasFocus);
    }

    public void NativeCallback(string msg)
    {
        if (sdk != null && sdk.NativeCallback(msg))
        {
            Initial();
        }
    }
}