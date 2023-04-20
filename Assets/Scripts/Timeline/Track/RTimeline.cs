using System;
using System.Collections.Generic;
using CFClient;
using CFEngine;
using CFUtilPoolLib;
using Cinemachine;
using ClientEcsData;
using FMODUnity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Profiling;
using UnityEngine.Timeline;
using static Cinemachine.CinemachineBrain;
using CStyle = Cinemachine.CinemachineBlendDefinition.Style;

public enum TimelineSettingID
{
    TimelineUI_Layer1 = 1,
    TimelineUI_Layer2 = 2,
    CutsceneDialog_Layer = 3,
    QTE_Layer = 4,
    Subtitle_Layer = 5,
}

public partial class RTimeline : XSingleton<RTimeline>, IRTimeline, IInterface, INotificationReceiver
{
    enum TimelineDataLoadState { UnLoad, Loading, LoadingFinish }
    //asset
    public static byte AssetType_Curve = DirectorHelper.EngineAssetCount;
    public static byte AssetType_FMOD = (byte)(AssetType_Curve + 1);
    public static byte AssetType_UI = (byte)(AssetType_FMOD + 1);
    public static byte AssetType_Camera = (byte)(AssetType_UI + 1);
    public static byte AssetType_Env = (byte)(AssetType_Camera + 1);
    public static byte AssetType_Cine = (byte)(AssetType_Env + 1);
    public static byte AssetType_Subtitle = (byte)(AssetType_Cine + 1);
    public static byte AssetType_CustomAnim = (byte)(AssetType_Subtitle + 1);
    public static byte AssetTypeCount = (byte)(AssetType_CustomAnim + 1);
    //track
    public static byte TrackType_UI = DirectorHelper.EngineTrackCount;
    public static byte TrackType_Camera = (byte)(TrackType_UI + 1);
    public static byte TrackType_Subtitle = (byte)(TrackType_Camera + 1);
    public static byte TrackType_Tip = (byte)(TrackType_Subtitle + 1);
    public static byte TrackType_CustomAnim = (byte)(TrackType_Tip + 1);
    public static byte TrackType_Dialog = (byte)(TrackType_CustomAnim + 1);
    public static byte TrackType_RenderEffect = (byte)(TrackType_Dialog + 1);
    public static byte TrackType_FmodPlable = (byte)(TrackType_RenderEffect + 1);
    public static byte TrackType_CustomPicture = (byte)(TrackType_FmodPlable + 1);
    public static byte TrackType_BoneRotate = (byte)(TrackType_CustomPicture + 1);
    public static byte TrackType_ControlPart = (byte)(TrackType_BoneRotate + 1);
    public static byte TrackType_CameraEffect = (byte)(TrackType_ControlPart + 1);
    public static byte TrackType_CharacterShadingSettings = (byte)(TrackType_CameraEffect + 1);

    public TimelineConfigData m_timelineConfig;
    private TimelineSettings m_timelineSettings = null;
    private TimelineContext m_context;
    public string m_timelineName = string.Empty;
    private int m_dynamicAnimationBindCount = 0;
    private static List<CinemachineVirtualCameraBase> cine_list = new List<CinemachineVirtualCameraBase>();
    private Dictionary<Guid, FmodPlayableBehaviour> facialDic = new Dictionary<Guid, FmodPlayableBehaviour>();

    public List<TrackAsset> CharactersGroups;

    private Dictionary<string, ICsFmod> fmod = new Dictionary<string, ICsFmod>();
    private PlayableDirector m_director;
    public GameObject dummyObject = null;
    public Transform dummyCamera = null;
    public bool isPlaying;
    private CinemachineBrain m_cineBrainComp = null;
    private static IXTimeline ixtl = null;
    private static TimelineDataLoadState loadState = TimelineDataLoadState.UnLoad;
    private static StreamResLoadCb timelineStreamDataCb = TimelineStreamDataCb;

    private float orignalTimelineTimer = 0;
    private float orignalTimeLimit = 0.3f;
    public static bool orignalTimerStart = false;
    public Action<OrignalTimelineData> timeOut;
    private List<Renderer> roleRenderers;
    private MaterialPropertyBlock mpb;

    private CinemachineVirtualCameraBase lastCam;
    private TimelineClip blendInClip;
    private float blendInTimer = 0;
    private bool startBlendIn = false;

    private float avgTimer = 0;
    public float avgWaitTime = 3.0f;
    public bool startAvgTimer = false;
    public Action initAvgBinding;

    private bool starDelayRebuildGraph = false;
    private float rebuildGraphTimer = 0;
    private int rebuildGraphFrameCount = 1000;

    public bool startTweenerCountDown = false;
    public double tweenerTimer = 0;

    static string timelinePrefix = "timeline/orignal_";
    public static string HEAD_BONE_PATH = "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Neck/Bip001 Head";

    public const uint fx_max = 64;
    private const float sync_time = 2;
    private uint p_maxFx;
    private List<TrackAsset> ch_tracks;
    private List<ControlTrack> fx_tracks;
    private OrignalTimelineData m_timelineData;
    private Action<XGameObject, int> m_callback;
    private Action<int, int> m_selfShadowCb;
    private CinemachineVirtualCameraBase m_gameActiveVC;
    private static AssetHandler[] fx_go_pool = new AssetHandler[fx_max];
    private static FxContext[] fx_ctx_pool = new FxContext[fx_max];
    private Dictionary<XGameObject, OrignalChar> m_asyncChars = new Dictionary<XGameObject, OrignalChar>();


    bool IXInterface.Deprecated { get; set; }

    public DirectorBindingData Data { get { return DirectorBindingData.Data; } }

    public Dictionary<string, ICsFmod> Fmod_dic { get { return fmod; } }

    public System.Object Arg { get { return m_context?.arg; } }

    public bool Evaluting { get; set; }

    public static IXTimeline Ixtl
    {
        get { return ixtl ?? (ixtl = XInterfaceMgr.singleton.GetInterface<IXTimeline>(XCommon.singleton.XHash("IXTimeline"))); }
    }

    public bool m_lastBgmSignalExecuted = false;
    public bool m_lastMusicEventExecuted = false;
    private TimelineBgmSignal m_lastBgmSignal;
    public FmodPlayableAsset m_lastMusicEvent = null;

    private List<SceneSignal> m_sceneSignals = new List<SceneSignal>(); //一对加载场景的信号
    public List<string> m_playedEvents = new List<string>();

    public PlayableDirector Director
    {
        get
        {
            if (m_director) return m_director;
            else if (EngineContext.director) return EngineContext.director;
            //  only worked in editor mode, while code recompile 
            return GameObject.FindObjectOfType<PlayableDirector>();
        }
    }

    /// <summary>
    /// SlowSignalEmitter设置过的timeline的播放速度
    /// </summary>
    private float m_directorSpeed = 1.0f;
    public float DirectorSpeed
    {
        get
        {
            return m_directorSpeed;
        }
        set
        {
            m_directorSpeed = value;
        }
    }

    /// <summary>
    /// 剧情对话的时候，控制timeline是否暂停
    /// </summary>
    private bool m_dialogIsPaused = false;
    public bool DialogIsPaused
    {
        get
        {
            return m_dialogIsPaused;
        }
        set
        {
            m_dialogIsPaused = value;
        }
    }


    private ScreenEffectAnimation m_screenEffectAnimation;
    public ScreenEffectAnimation ScreenEffectAnimation
    {
        get
        {
            //在编辑器下使用
            if (m_screenEffectAnimation == null)
            {
                m_screenEffectAnimation = GameObject.FindObjectOfType<ScreenEffectAnimation>();
            }
            return m_screenEffectAnimation;
        }
    }

    /// <summary>
    /// 初始化timline的配置，在globalcommon表里配置了一些数据，比如口型相关的数据。
    /// 还有对话文字打印速度的数据。在src的InitTimelineConfigData中调用。
    /// </summary>
    /// <param name="config"></param>
    public void InitTimelineConfig(TimelineConfigData config)
    {
        m_timelineConfig = config;
    }

    public void InitTimelineSettings(TimelineSettings config)
    {
        m_timelineSettings = config;
    }

    private TimelineSettings GetTimelineSettings()
    {
#if UNITY_EDITOR
        if (!EngineContext.IsRunning)
        {
            if (m_timelineSettings == null)
            {
                m_timelineSettings = new TimelineSettings();
                XTableReader.ReadFile(@"Table/TimelineSettings", m_timelineSettings);
            }
        }
#endif
        return m_timelineSettings;
    }

    public TimelineSettings.RowData GetTimelineSettingByID(TimelineSettingID key)
    {
        int id = (int)key;
        TimelineSettings timelineSettings = GetTimelineSettings();
        if (timelineSettings.Table == null) return null;
        for (int i = 0; i < timelineSettings.Table.Length; ++i)
        {
            if (id == timelineSettings.Table[i].ID) return timelineSettings.Table[i];
        }
        return null;
    }

    /// <summary>
    /// 进入场景之后的初始化
    /// </summary>
    /// <param name="context"></param>
    public void Start(EngineContext context)
    {
        if (context != null)
            m_director = EngineContext.director;
        InnerInit();
        ClearDic();
#if UNITY_EDITOR
        ForceLoadCharRenderer();
#endif        
    }

    /// <summary>
    /// 离开场景之后的释放
    /// </summary>
    public void End()
    {
        RecycleOrignal();
        RecycleTimelines();
        ExternalHelp.Interface = null;
        m_director = null;
        ClearDic();
        loadState = TimelineDataLoadState.UnLoad;
        PerformanceMgr.TimelineLeave();
    }

    public void InnerInit()
    {
        ExternalHelp.Interface = this;
        DirectorHelper.singleton.globalReceiver = this;
        RSignal.InnerInit();
        if (DirectorHelper.createTrack == null)
        {
            DirectorHelper.createTrack = CreateTrack;
        }
        if (DirectorHelper.releaseTrack == null)
        {
            DirectorHelper.releaseTrack = ReleaseTrack;
        }
        if (DirectorHelper.createAsset == null)
        {
            DirectorHelper.createAsset = CreateAsset;
        }
        if (DirectorHelper.realeaseAsset == null)
        {
            DirectorHelper.realeaseAsset = RealeaseAsset;
        }
        if (DirectorHelper.loadBindObject == null)
        {
            DirectorHelper.loadBindObject = BindObject;
        }
        if (DirectorHelper.unLoadBindObject == null)
        {
            DirectorHelper.unLoadBindObject = UnBindObject;
        }
        if (DirectorHelper.evaluateAnim == null)
        {
            DirectorHelper.evaluateAnim = AnimCurveUtility.EvaluateAt;
        }
        if (DirectorHelper.evaluateAnimV3 == null)
        {
            DirectorHelper.evaluateAnimV3 = AnimCurveUtility.EvaluateAt;
        }
        if (DirectorHelper.evaluateAnimV2 == null)
        {
            DirectorHelper.evaluateAnimV2 = AnimCurveUtility.EvaluateAt;
        }
        if (DirectorHelper.evaluateAnimFloat == null)
        {
            DirectorHelper.evaluateAnimFloat = AnimCurveUtility.EvaluateAt;
        }
        loadState = TimelineDataLoadState.UnLoad;
        CinemachineMixer.GetMasterPlayableDirector = GetDirector;
    }

    public void Finish(bool pool)
    {
        if (Application.isPlaying && m_timelineData != null && m_timelineData.mode2 != FinishMode.INTERUPT)
        {
            if (blendInClip != null)
            {
                blendInClip.easeInDuration = 0;
                m_director.gameObject.SetActive(false);
                var clip = blendInClip.asset as CinemachineShot;
                CinemachineCore.Instance.RemoveActiveCamera(clip.vcb);
            }
        }

        CleanDirector(pool);
        if (AnimEnv.currentAE != null)
        {
            AnimEnv.currentAE.UnInit();
            AnimEnv.currentAE = null;
        }

        ScreenEffect ins = ScreenEffect.Instance();
        ScreenEffect.enabled = false;
        if (ins.transitionState != 0)
        {
            ScreenEffect.ReleaseRT();
            ins.transitionState = 0;
        }

        MFLensFlare.TimelineAvoid = false;
    }

    private PlayableDirector GetDirector()
    {
        return Director;
    }

    private static void TimelineStreamDataCb(ref StreamContext sc, LoadInstance li)
    {
        CFBinaryReader reader = LoadMgr.BeginRead(ref sc);
        if (reader != null)
        {
            DirectorHelper.singleton.PreLoad();
            XDirectorDataRead.Read(reader, DirectorHelper.singleton.strings);
            var path = li.loadHolder as string;
            if (DirectorBindingData.Data.useCine)
            {
                DirectorHelper.singleton.LoadCine(path);
            }
            DirectorHelper.singleton.Load(reader
#if UNITY_EDITOR
                , path
#endif
            );
            CinemachineTrack.vc = cine_list;
            LoadMgr.EndRead(reader);
        }
        loadState = TimelineDataLoadState.Loading;
    }

    public static void Load(string path)
    {
        cine_list.Clear();
        LoadMgr.loadContext.Init(null, path);
        LoadMgr.singleton.LoadData(path, timelineStreamDataCb);
    }

    public void PauseTimeline(TimelineContext context, bool isPause)
    {
        if (Director != null)
        {
            if (isPause) SetSpeed(0); //Director.playableGraph.GetRootPlayable(0).SetSpeed(0);
            else SetSpeed(1); //Director.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }
    }


    public void ControlUICallback(string arg)
    {
        if (m_context != null && m_context.m_controlUICallback != null) m_context.m_controlUICallback.Invoke(arg);
    }

    public void Update(float time)
    {
        if (loadState == TimelineDataLoadState.Loading)
        {
            loadState = TimelineDataLoadState.LoadingFinish;
            Ixtl?.LoadTimelineFinish(false);
        }
        if (isPlaying && EngineContext.IsRunning) LoadGlobalTimelineUpdate();
        if (EngineContext.IsRunning) LoadLocalTimelineUpdate();

        TimelineDialogUpdate(time);
        TimelineQTEUpdate(time);

        if (startBlendIn)
        {
            blendInTimer += time;
            if (blendInTimer > m_timelineData.blendInTime)
            {
                if (m_cineBrainComp != null)
                {
                    m_cineBrainComp.forceSetCam = false;
                    m_cineBrainComp.activeCam = null;
                    m_cineBrainComp.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 2f);
                }
                blendInTimer = 0;
                startBlendIn = false;
            }
        }

        if (startAvgTimer)
        {
            avgTimer += time;
            if (avgTimer > avgWaitTime)
            {
                avgTimer = 0;
                startAvgTimer = false;
                if (initAvgBinding != null)
                    initAvgBinding.Invoke();
            }
        }

        if (starDelayRebuildGraph && Time.frameCount == rebuildGraphFrameCount + 1)
        {
            DelayRebuildGraph();
            starDelayRebuildGraph = false;
        }

        if (Application.isPlaying && startTweenerCountDown)
        {
            tweenerTimer += time;
        }

        //不进入游戏，预览timeline，不开启MSAA
        if (!EngineContext.IsRunning)
        {
            EngineContext.m_needMSAA = false;
        }
#if UNITY_EDITOR
        CharacterShadowPreview();
#endif

        if(TimelineSpeedComponent.m_instance != null && !DialogIsPaused)
        {
            this.SetSpeed(TimelineSpeedComponent.m_instance.m_speed);
        }
    }

    private void CharacterShadowPreview()
    {
        Profiler.BeginSample("Timeline character shadow preview");
        //遍历角色应用阴影效果
        if (roleRenderers == null)
        {
            ForceLoadCharRenderer();
        }
        for (int i = roleRenderers.Count - 1; i >= 0; i--)
        {
            mpb ??= new MaterialPropertyBlock();
            if (roleRenderers[i] == null)
            {
                roleRenderers.RemoveAt(i);
                continue;
            }
            roleRenderers[i].GetPropertyBlock(mpb);
            Vector4 rootPos = roleRenderers[i].transform.parent.position;
            rootPos.w = 1;
            // mpb.SetVector(ShaderManager._RootPosWS, rootPos); // Takeshi: It is obsolete behavior.
            roleRenderers[i].SetPropertyBlock(mpb);
            mpb.Clear();
        }

        if (roleRenderers.Count == 0)
        {
            roleRenderers = null;
        }
        Profiler.EndSample();
    }
    public void ForceLoadCharRenderer()
    {
        var root = GameObject.Find("timeline_Role_Root");
        roleRenderers = new List<Renderer>();
        if (root == null) return;
        for (int i = 0; i < root.transform.childCount; i++)
        {
            var charTrans = root.transform.GetChild(i);
            for (int j = 0; j < charTrans.transform.childCount; j++)
            {
                var partTrans = charTrans.transform.GetChild(j);
                partTrans.TryGetComponent(out Renderer r);
                if (r)
                {
                    roleRenderers.Add(r);
                }
            }
        }
    }


    private void TimelineQTEUpdate(float time)
    {
        TimelineQteClickUI.singleton.Update(time);
        TimelineQteClickLoopUI.singleton.Update(time);
        TimelineQteContinueClickUI.singleton.Update(time);
        TimelinePvpUI.singleton.Update(time);
    }

    /// <summary>
    /// 剧情对话相关Update
    /// </summary>
    private void TimelineDialogUpdate(float time)
    {
        UndoPauseFmod();
        CheckAudioIsPlaying(time);
    }

    /// <summary>
    /// 如果剧情对话轨道，不在播放声音了，则将其播放速度恢复到1，继续正常播放。
    /// </summary>
    private void CheckAudioIsPlaying(float time)
    {
        //1.取消暂停
        XRuntimeFmod fmod = RTimeline.singleton.GetFmod(Director);

        if (fmod != null)
        {
            bool toneIsPlaying = fmod.IsPlaying(AudioChannel.Action) || fmod.IsPlaying(AudioChannel.Vocal);
            if (!toneIsPlaying)
            {
                Guid guid = fmod.GetEventGUID(AudioChannel.Action);
                if (guid == Guid.Empty)
                {
                    guid = fmod.GetEventGUID(AudioChannel.Vocal);
                }
                if (guid == Guid.Empty) return;
                RemoveFacialCurveFromDic(guid);

                if (TimelineDialog.singleton.IsAuto) //只有自动播放的时候，才能在语音不播放的之后，将m_timelineIsPaused置为false，手动控制的，在点击下一句之后，手动置为false
                {
                    DialogIsPaused = false;
                }
            }
        }

        //2.恢复速度
        if (Director != null && TimelineDialog.singleton.IsAuto && !DialogIsPaused && !TimelineChapterStartUI.singleton.isShow)
        {
            bool valid = Director.playableGraph.IsValid();
            if (valid)
            {
                SetSpeed(DirectorSpeed);  //恢复到SlowSignalEmitter设置的速度，而不是强制设置为1.0f
            }
        }

        //3.口型更新
        if (facialDic != null)
        {
            foreach (var facial in facialDic)
            {
                if (facial.Value != null && facial.Value.isPlaying)
                    facial.Value.UpdateFrame();
                else if (facial.Value != null)
                    facial.Value.RecordingFmod(null);
            }
        }
    }


    /// <summary>
    /// 非运行时，点击预览，如果当前运行时间(director.time)，不在剧情对话条区间内，则将asset.m_isPause置为false，表示未被暂停过。
    /// </summary>
    private void UndoPauseFmod()
    {
        if (Application.isPlaying) return;
        GameObject directorGo = GameObject.Find("timeline");
        if (directorGo == null) return;
        PlayableDirector director = directorGo.GetComponent<PlayableDirector>();
        foreach (var pb in director.playableAsset.outputs)
        {
            if (pb.sourceObject is FmodPlayableTrack)
            {
                FmodPlayableTrack track = pb.sourceObject as FmodPlayableTrack;
                var clips = track.GetClips();
                foreach (var item in clips)
                {
                    FmodPlayableAsset asset = item.asset as FmodPlayableAsset;
                    if (director.time < item.start || director.time > item.end)
                    {
                        asset.m_isPause = false;
                    }
                }
            }
        }
    }

    public void Play(bool isOrigin)
    {
        //cinemachine一定在play的时候激活 而不是load结束的时候 否则镜头会抖动
        if (EngineContext.director == null)
        {
            XDebug.singleton.AddWarningLog("EngineContext.director is null");
            return;
        }
        this.isPlaying = true;

        InitCinemachineFollowAndLookAt(this.m_timelineData);

        m_cineBrainComp.enabled = true;
        DirectorHelper.isOrignal = isOrigin;
        this.Evaluting = false;
        //fmod.Clear();
        if (!isOrigin)
            DirectorHelper.Play();

        if (m_context.arg != null && m_cineBrainComp != null)
        {
            CinemachineCore.CameraUpdatedEvent.AddListener(LateUpdateCallback);
        }

        if (Application.isPlaying)
        {
            SkillHelper.ResetVignettes();
        }
    }

    // CinemachineBrain 的LateUpdate中设置摄像机位置之后关闭loading
    private void LateUpdateCallback(CinemachineBrain brain)
    {
        if (m_context?.arg is List<object> objs)
        {
            TimelineArgType argType = (TimelineArgType)(objs[0]);
            if (argType == TimelineArgType.PVP)
            {
                PVPData pvpData = (PVPData)(objs[objs.Count - 1]);
                pvpData?.callback?.Invoke();
            }
            CinemachineCore.CameraUpdatedEvent.RemoveListener(LateUpdateCallback);
        }
    }

    public void ManuUnloadAssets(bool includeDirector)
    {
        if (includeDirector) CleanDirector(false);
    }

    public void RealUnloadCine()
    {
        DirectorHelper.singleton.CheckUnloadCine();
    }

    public void RealUnload()
    {
        if (m_timelineData != null)
        {
            LoadMgr.singleton.Destroy(ref m_timelineData.asset);
            m_timelineData = null;
        }
    }

    private void CleanDirector(bool pool)
    {
        if (m_director != null)
        {
            TimelineQteClickUI.singleton.StopEvent();
            TimelineQteClickLoopUI.singleton.StopEvent();
            TimelineQteContinueClickUI.singleton.StopEvent();

            if (!m_lastBgmSignalExecuted) //如果是打断方式播放，则要执行最后一个控制音量的信号
            {
                ControlBgm(m_lastBgmSignal);
                m_lastBgmSignalExecuted = true;
                m_lastBgmSignal = null;
            }

            if (!m_lastMusicEventExecuted)
            {
                XRuntimeFmod bgmFmod = XAudioMgr.singleton.m_sceneBgmFmod as XRuntimeFmod;
                if (bgmFmod != null && m_lastMusicEvent != null)
                {
                    bool isValid = bgmFmod.IsValid(AudioChannel.Music);
                    XDebug.singleton.AddGreenLog("play logicMusic event=" + m_lastMusicEvent.clip);
                    bgmFmod.StartEvent(m_lastMusicEvent.clip, AudioChannel.LogicMusic, false);
                    m_lastMusicEventExecuted = true;
                    m_lastMusicEvent = null;
                }
            }

            StopFmod(m_director, TimelinePlayContext.context.finishMode == FinishMode.INTERUPT);
            ReturnFmod(m_director);

            for (int i = 0; i < m_sceneSignals.Count; ++i)
            {
                if (m_sceneSignals[i] != null && !m_sceneSignals[i].m_executed)
                {
                    m_sceneSignals[i].LoadOrUnLoadScene();
                }
            }
            m_sceneSignals.Clear();

            if (pool)
            {
                var tf = m_director.transform.parent;
                if (m_cineBrainComp != null) m_cineBrainComp.m_DefaultBlend.m_Time = 0;
                tf?.gameObject.SetActive(false);
            }
            else if (!DirectorHelper.isOrignal)
            {
                m_director.playableAsset = null;
                m_director.enabled = false;
            }
        }
        this.isPlaying = false;
        RecycleOrignal();
        DirectorHelper.singleton.UnLoad();
        TimelineUIMgr.ReturnAll();
        cine_list.Clear();
        ClearDic();

        if (SmartShadow.Instance != null && !SmartShadow.Instance.enabled) SmartShadow.Instance.SetEnable(true);
    }

    public void SetDummyCamera(GameObject dummyObject, Transform dummyCamera)
    {
        this.dummyObject = dummyObject;
        this.dummyCamera = dummyCamera;
    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
#if UNITY_EDITOR
        if (context is NotifyContext c)
        {
            DebugLog.AddLog2("reveiver signal:{0} time:{1:F2}", notification.GetType().Name, c.time);
        }
        if (m_director == null)
            m_director = RTimeline.singleton.Director; //如果使用EngineContext.director则在PlayableDirectorInspector line78行赋值，但是全屏播放则没有赋值
#endif
        if (m_director)
        {
            switch (notification)
            {
                case JumpSignalEmmiter jumpSignalEmmiter:
                    m_director.time = jumpSignalEmmiter.jumpTime;

                    if (m_cineBrainComp != null)
                    {
                        m_cineBrainComp.forceSetCam = false;
                        m_cineBrainComp.activeCam = null;
                        m_cineBrainComp.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 2f);
                    }
                    break;
                case SlowSignalEmitter slowSignal:
                    var graph = m_director.playableGraph;
                    if (graph.IsValid())
                    {
                        Playable playable = graph.GetRootPlayable(0);
                        if (playable.IsValid())
                        {
                            ///playable.SetSpeed(slowSignal.slowRate);
                            SetSpeed(slowSignal.slowRate);
                            DirectorSpeed = slowSignal.slowRate;
                        }
                    }
                    break;
                case ActiveSignalEmmiter activeSignalEmmiter:
                    TrackAsset track = ExternalHelp.GetRootTrack(activeSignalEmmiter);
                    Transform tf = ExternalHelp.FetchAttachOfTrack(m_director, track);
                    tf?.gameObject.SetActive(activeSignalEmmiter.Active);
                    break;
                case DirectorSignalEmmiter sig:
                    RSignal.OnSignal(sig);
                    //ClearDic(); //如果在播放语音的时候，有其他信号触发，则会清除掉口型，这个是不对的
                    break;
            }
        }
    }

    public INotificationReceiver FetchGlobalReceiver() { return this; }

    #region  callback
    static DirectorTrackAsset CreateTrack(byte type)
    {
        if (type == TrackType_Camera ||
            type == TrackType_UI ||
            type == TrackType_Subtitle ||
            type == TrackType_Tip ||
            type == TrackType_Dialog)
        {
            return SharedScriptObjectPool<DirectorUberTrack>.Get();
        }
        else if (type == DirectorHelper.TrackType_Cine)
        {
            return SharedScriptObjectPool<XCineTrack>.Get();
        }
        return null;
    }

    static void ReleaseTrack(DirectorTrackAsset track) { }

    static DirectPlayableAsset CreateAsset(byte type, CFBinaryReader reader)
    {
        if (type == AssetType_FMOD)
        {
            return SharedScriptObjectPool<FmodPlayableAsset>.Get();
        }
        else if (type == AssetType_UI)
        {
            return SharedScriptObjectPool<UIPlayerAsset>.Get();
        }
        else if (type == AssetType_Camera)
        {
            return SharedScriptObjectPool<CameraPlayableAsset>.Get();
        }
        else if (type == AssetType_Cine)
        {
            return LoadCineAsset(reader);
        }
        return null;
    }

    private static DirectPlayableAsset LoadCineAsset(CFBinaryReader reader)
    {
        var shot = SharedScriptObjectPool<CinemachineShot>.Get();
        var go = DirectorHelper.singleton.cine;
        string child = reader.ReadString();
        var tf = go.transform.Find(child);
        if (tf != null)
        {
            var vc = tf.gameObject.GetComponent<CinemachineVirtualCameraBase>();
            shot.OnLoad(vc);
            cine_list.Add(vc);
        }
        return shot;
    }

    static void RealeaseAsset(DirectPlayableAsset asset)
    {
        switch (asset)
        {
            case FmodPlayableAsset fmodPlayableAsset:
                SharedScriptObjectPool<FmodPlayableAsset>.Release(fmodPlayableAsset);
                break;
            case UIPlayerAsset uiPlayerAsset:
                SharedScriptObjectPool<UIPlayerAsset>.Release(uiPlayerAsset);
                break;
            case CameraPlayableAsset cameraPlayableAsset:
                SharedScriptObjectPool<CameraPlayableAsset>.Release(cameraPlayableAsset);
                break;
        }
    }

    static void BindObject(byte trackType, DirectorTrackAsset track, string streamName)
    {
        if (trackType == DirectorHelper.TrackType_Animation ||
            trackType == DirectorHelper.TrackType_AnimClip)
        {
            XAnimtionTrack.BindObject(track, streamName);
        }
        else if (trackType == DirectorHelper.TrackType_Active)
        {
            XActiveTrack.BindObject(track, streamName);
        }
        else if (trackType == DirectorHelper.TrackType_Control)
        {
            XControlTrack.BindObject(track, streamName);
        }
    }

    static void UnBindObject(ICallbackData data)
    {
        if (data is XAnimtionTrack)
        {
            XAnimtionTrack.UnBindObject(data as XAnimtionTrack);
        }
        else if (data is XActiveTrack)
        {
            XActiveTrack.UnBindObject(data as XActiveTrack);
        }
        else if (data is XControlTrack)
        {
            XControlTrack.UnBindObject(data as XControlTrack);
        }
    }

    public void SetTrackAnimClip(string trackName, int clipIndex, string resPath)
    {
        var tracks = DirectorHelper.singleton.tracks;
        var clips = DirectorHelper.singleton.clips;
        if (tracks != null && clips != null)
        {
            for (int i = 0; i < tracks.Length; ++i)
            {
                var track = tracks[i];
                if (track is DirectorAnimationTrack && track.streamName == trackName)
                {
                    var animTrack = track as DirectorAnimationTrack;
                    int index = track.clipStart + clipIndex;
                    if (clipIndex >= 0 && index < track.clipEnd && index < clips.Length)
                    {
                        var clip = clips[index];
                        if (clip != null)
                        {
                            var animAsset = clip.asset as DirectorAnimationPlayableAsset;
                            if (animAsset != null)
                            {
                                animAsset.LoadAnim(resPath);
                                return;
                            }
                        }
                    }
                    DebugLog.AddWarningLog2("set anim clip error:{0} {1} {2}", trackName, clipIndex.ToString(), resPath);
                    break;
                }
            }
        }
    }
    public void SetOrigalAniClip(string trackName, int clipIndex, AnimationClip xclip)
    {
        foreach (PlayableBinding pb in m_director.playableAsset.outputs)
        {
            if (pb.sourceObject is AnimationTrack && pb.streamName.Equals(trackName))
            {
                AnimationTrack track = pb.sourceObject as AnimationTrack;
                int i = 0;
                foreach (var clip in track.GetClips())
                {
                    if (clipIndex == i)
                    {
                        (clip.asset as AnimationPlayableAsset).clip = xclip;
                        break;
                    }
                    i++;
                }
            }
        }
    }

    public void SetMute(UnityEngine.Object obj, bool mute)
    {
        var track = obj as TrackAsset;
        if (track != null)
        {
            track.muted = mute;
            starDelayRebuildGraph = true;
            rebuildGraphFrameCount = Time.frameCount;
            //DelayRebuildGraph();
        }
    }

    private void DelayRebuildGraph()
    {
        double t0 = Director.time;
        Director.RebuildGraph();
        Director.time = t0;
        if (this.isPlaying)
            Director.Play();
    }

    #endregion

    #region facial

    public void AddFacialToDic(Guid guid, FmodPlayableBehaviour facial)
    {
        if (facialDic == null)
            return;

        //Guid tmpGuid;
        foreach (var dic in facialDic)
        {
            RemoveFacialCurveFromDic(dic.Key);
        }

        if (facialDic.ContainsKey(guid))
        {
            facialDic[guid] = facial;
        }
        else
        {
            facialDic.Add(guid, facial);
        }

        facialDic[guid].isPlaying = true;
    }


    public void RemoveFacialCurveFromDic(Guid guid)
    {
        if (facialDic != null && facialDic.ContainsKey(guid) && facialDic[guid] != null)
        {
            facialDic[guid].Release();
            facialDic[guid].isPlaying = false;
        }
    }

    public void ClearDic()
    {
        if (facialDic == null)
            return;
        facialDic.Clear();
    }

    #endregion

    public void ControlBgm(TimelineBgmSignal signal)
    {
        if (signal != null && m_context != null && m_context.m_controlBgmCallback != null)
        {
            TimelineBgmData data = signal.GetTimelineBgmData();
            m_context.m_controlBgmCallback.Invoke(data);
        }
    }

    public void ControlBlack(float[] times)
    {
        if (times == null || times.Length < 3 || m_context == null || m_context.m_controlBlackCallblack == null) return;
        m_context.m_controlBlackCallblack(times);
    }

    public void QTEMissCallback()
    {
        if (m_context != null && m_context.m_qteMissCallback != null)
        {
            m_context.m_qteMissCallback();
        }
    }

    public void ControlSkipCallback(bool active)
    {
        if (m_context != null && m_context.m_controlSkipCallback != null)
        {
            m_context.m_controlSkipCallback(active);
        }
    }

    public void JumpToCertainTime(float time)
    {
        if (Director == null) return;
        Director.time = time;
    }

    public void SetSpeed(float speed)
    {
        if (Director == null) return;
        var graph = Director.playableGraph;
        if (graph.IsValid())
        {
            Playable playable = graph.GetRootPlayable(0);
            playable.SetSpeed(speed);
            //XDebug.singleton.AddErrorLog("director setspeed =" + speed + "  " + Time.realtimeSinceStartup);
        }
    }


    /// <summary>
    /// 加载timline的入口
    /// </summary>
    /// <param name="context">加载timeline的配置说明</param>
    /// <param name="bindingData">读取timline的配置，带出给src中使用</param>
    /// <param name="cb"></param>
    /// <param name="selfShadowCb"></param>
    /// <param name="director"></param>
    /// <returns></returns>
    public void LoadTimeline(TimelineContext context,
                            DirectorBindingData bindingData,
                            Action<XGameObject, int> cb,
                            System.Action<int, int> selfShadowCb,
                            out PlayableDirector director)
    {
        Init(context, cb, selfShadowCb);
        director = null;
        AssetHandler asset = null;
        bool legal = CheckTimelineData(ref asset);

        if (Application.isPlaying && m_timelineData.mode2 != FinishMode.INTERUPT)
            TimelinePlayContext.context.loadTimelineFinish = SetBlendCameraEnable;

        if (legal)
        {
            director = m_director;

            m_director.extrapolationMode = DirectorWrapMode.Hold; //全局的timeline设置为hold模式，是为了计算自动销毁的时机，在src的XTimeline的Update中
            InitDirectorBindingData(bindingData, asset);          //有个默认的逻辑是，如果时间非常靠近timeline结束了，则自动销毁，为啥是hold呢，原因这里的if (Mathf.Abs(tick) < 1e-4)
            AnalyzeTimeline();                                    //而一帧是0.02s，所以如果不是hold模式，则有可能造成不能销毁的情况，而此时必须点击跳过按钮。
            BindAnimationForDynamicRole();
            LoadChars();
            LoadFxs();
            MFLensFlare.TimelineAvoid = false;
            PerformanceMgr.TimelineEnter(); //设置骨骼精细度
            if (m_context.m_justLoad)
            {
                m_director.time = -0.0333f; //保证第0帧的信号和clip不执行，等真正播放的时候，才执行
                SetSpeed(0); //m_justLoad=true，表示只是加载资源，然后等待播放PauseTimeline(TimelineCreateContext, false)进行播放
            }
        }
        else
        {
            LoadMgr.singleton.Destroy(ref asset);
        }
    }

    /// <summary>
    /// 加载非原生的timeline，这是历史遗留问题，后面将统一使用原生的timeline。
    /// </summary>
    /// <param name="context"></param>
    /// <param name="director"></param>
    public void Load(TimelineContext context,
                            DirectorBindingData bindingData,
                            Action<XGameObject, int> cb,
                            System.Action<int, int> selfShadowCb,
                            out PlayableDirector director)
    {
        Init(context, cb, selfShadowCb);
        director = EngineContext.director;
        m_director = director;
        m_director.extrapolationMode = DirectorWrapMode.Hold;
        string path = "timeline/" + context.name.ToLower();
        Load(path);
    }

    private void Init(TimelineContext context, Action<XGameObject, int> cb, Action<int, int> selfShadowCb)
    {
        m_context = context;
        m_callback = cb;
        m_selfShadowCb = selfShadowCb;
        p_maxFx = 0;
        m_lastBgmSignalExecuted = false;
        m_lastBgmSignal = null;
        m_lastMusicEventExecuted = false;
        m_lastMusicEvent = null;
        m_asyncChars.Clear();
        isPlaying = false;
        m_directorSpeed = 1.0f;
        m_dynamicAnimationBindCount = 0;
        DirectorHelper.isOrignal = false;
        DirectorHelper.singleton.CheckUnloadCine();

        m_cineBrainComp = EngineContext.director.gameObject.GetComponent<CinemachineBrain>();
        if (m_cineBrainComp == null)
        {
            m_cineBrainComp = EngineContext.director.gameObject.AddComponent<CinemachineBrain>();
        }
    }

    /// <summary>
    /// 检查timeline是否合法
    /// 1.是否是原生的timeline，名字约定以Orignal开头，这单词拼写错误，历史原因
    /// 2.prefab是否存在
    /// 3.timline上是否有PlayableDirector和OrignalTimelineData脚本
    /// 4.如果都合法，判断加载出来的prefab是否激活，未激活，则将其激活，并且重置初始位置为零点。
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    private bool CheckTimelineData(ref AssetHandler asset)
    {
        m_timelineName = m_context.name;
        GameObject prefab = EngineUtility.LoadPrefab("timeline/" + m_timelineName, ref asset, 0, m_context.returnPool, null, true);
        if (prefab == null)
        {
            XDebug.singleton.AddErrorLog(m_timelineName + " timeline does not exist!");
            return false;
        }

        Transform timeline = prefab.transform.Find("timeline");
        m_director = timeline.GetComponent<PlayableDirector>();
        m_timelineData = timeline.GetComponent<OrignalTimelineData>();
        Transform effect = prefab.transform.Find("TimelineScreenEffect");
        if (effect != null)
        {
            m_screenEffectAnimation = effect.GetComponent<ScreenEffectAnimation>();
        }

        if (m_director == null)
        {
            XDebug.singleton.AddErrorLog(m_timelineName + " has no PlayableDirector Component!");
            return false;
        }

        if (m_timelineData == null)
        {
            XDebug.singleton.AddErrorLog(m_timelineName + " has no OrignalTimelineData Component!");
            return false;
        }

        if (!prefab.activeSelf)
        {
            prefab.SetActive(true);
            prefab.transform.localPosition = Vector3.zero;
        }

        orignalTimerStart = true;
        orignalTimelineTimer = 0;

        InitCinemachineFollowAndLookAt(this.m_timelineData);
        return true;
    }

    private void SetCameraPriority(int p, bool ac)
    {
        GameObject[] objList = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        for (int i = 0; i < objList.Length; ++i)
        {
            if (objList[i].GetComponent<FreeLookController>() != null)
            {
                FreeLookController CFreeLook = objList[i].GetComponent<FreeLookController>();
                CFreeLook.Enable = ac;
                CFreeLook.FreeLook.Priority = p;
                CFreeLook.FreeLook.UpdateVcamPoolStatus();
                break;
            }

            if (objList[i].GetComponent<SoloModeCameraController>() != null)
            {
                SoloModeCameraController CSoloMix = objList[i].GetComponent<SoloModeCameraController>();
                CSoloMix.Enable = ac;
                CSoloMix.mixCamera.Priority = p;
                CSoloMix.mixCamera.UpdateVcamPoolStatus();
                break;
            }
        }
    }

    private CinemachineVirtualCameraBase GetActiveCameraPriority()
    {
        GameObject[] objList = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        for (int i = 0; i < objList.Length; ++i)
        {
            FreeLookController CFreeLook = objList[i].GetComponent<FreeLookController>();
            SoloModeCameraController CSoloMix = objList[i].GetComponent<SoloModeCameraController>();


            if (CFreeLook != null && CFreeLook.enabled == true)
            {
                return CFreeLook.FreeLook;
            }

            if (CSoloMix != null && CSoloMix.enabled == true)
            {
                return CSoloMix.mixCamera;
            }
        }

        return null;
    }

    private void SetBlendCameraEnable()
    {
        if (!Application.isPlaying)
            return;

        CinemachineTrack track = null;
        if (RTimeline.singleton.Director.playableAsset == null)
            return;

        foreach (var pb in RTimeline.singleton.Director.playableAsset.outputs)
        {
            if (pb.sourceObject is CinemachineTrack obj)
            {
                track = obj;
            }
        }

        if (m_timelineData != null && m_timelineData.mode2 != FinishMode.INTERUPT)
        {
            var activeCam = GetActiveCameraPriority();
            if (m_cineBrainComp != null)
            {
                m_cineBrainComp.forceSetCam = true;
                m_cineBrainComp.activeCam = activeCam;

                m_cineBrainComp.m_DefaultBlend.m_Style = (CStyle)m_timelineData.BlendInMode;
                m_cineBrainComp.m_DefaultBlend.m_Time = m_timelineData.blendInTime;
            }
            startBlendIn = true;

            if (track != null)
            {
                var list = track.GetClipList();

                if (list.Count > 0)
                {
                    blendInClip = list[0];
                    blendInClip.easeInDuration = m_timelineData.blendInTime;

                    CinemachineShot shot = (CinemachineShot)list[list.Count - 1].asset;
                    lastCam = shot.vcb;
                }
            }
        }
    }

    /// <summary>
    /// 根据timeline上脚本OrignalTimelineData初始化src将要使用到的数据DirectorBindingData
    /// </summary>
    /// <param name="data"></param>
    /// <param name="asset"></param>
    private void InitDirectorBindingData(DirectorBindingData data, AssetHandler asset)
    {
        m_timelineData.asset = asset;
        data.hidelayer = m_timelineData.hidelayer;
        data.enableBGM = m_timelineData.enableBGM;
        data.isBreak = m_timelineData.isBreak;
        data.showAutoButton = m_timelineData.showAutoButton;
        data.m_useNewSkip = m_timelineData.m_useNewSkip;
        data.alwaysBreak = m_timelineData.alwaysShowBreak;
        data.hideUI = m_timelineData.hideUI;
        data.shadowNotMove = m_timelineData.shadowNotMove;
        data.endAudioParam = m_timelineData.endAudioParam;
        data.enableSetBgmVolume = m_timelineData.enableSetBgmVolume;
        data.bgmMaxVolume = m_timelineData.bgmMaxVolume;
        data.Finishmode = m_timelineData.mode;
        data.clearEffectData = m_timelineData.clearEffectData;

        data.finishArg = (((int)m_timelineData.blendOutTime * 10) << 16) + m_timelineData.BlendOutMode;
    }

    private void AnalyzeTimeline()
    {
        FindLastBGMSignalAndSceneSginalAndMusicEvent();

        if (m_director && m_director.playableAsset)
        {
            ch_tracks = new List<TrackAsset>();
            fx_tracks = new List<ControlTrack>();
            foreach (var pb in m_director.playableAsset.outputs)
            {
                switch (pb.sourceObject)
                {
                    case AnimationTrack _:
                    case ActivationTrack _:
                    case CustomAnimationTrack _:
                    case RenderEffectTrack _:
                    case TransformTweenTrack _:
                    case BoneRotateTrack _:
                    case ControlPartTrack _:
                    case CharacterShadingSettingsTrack _:
                        var atrack = pb.sourceObject as TrackAsset;
                        ch_tracks.Add(atrack);
                        break;
                    case ControlTrack _:
                        fx_tracks.Add(pb.sourceObject as ControlTrack);
                        break;
                }
            }
        }

    }

    /// <summary>
    /// 打断的时候，最后一个signal可能没有执行到，所以要强制执行一次
    /// </summary>
    private void FindLastBGMSignalAndSceneSginalAndMusicEvent()
    {
        if (m_director && m_director.playableAsset)
        {
            TimelineAsset timelineAsset = m_director.playableAsset as TimelineAsset;
            if (timelineAsset != null && timelineAsset.markerTrack != null)
            {
                IEnumerable<IMarker> markers = timelineAsset.markerTrack.GetMarkers();
                foreach (var item in markers)
                {
                    TimelineBgmSignal signal = item as TimelineBgmSignal;
                    if (signal != null && signal.m_isLastSignal)
                    {
                        m_lastBgmSignal = signal;
                        continue;
                    }

                    SceneSignal sceneSignal = item as SceneSignal;
                    if (sceneSignal != null)
                    {
                        sceneSignal.m_executed = false;
                        m_sceneSignals.Add(sceneSignal);
                    }
                }
            }

            ///找最后一个musicevent，跳过的时候执行
            foreach (var pb in m_director.playableAsset.outputs)
            {
                if (pb.sourceObject is FmodPlayableTrack)
                {
                    FmodPlayableTrack track = pb.sourceObject as FmodPlayableTrack;
                    if (track != null)
                    {
                        AudioChannel channel = track.m_audioChannel;
                        if (channel == AudioChannel.LogicMusic)
                        {
                            var list = track.GetClipList();
                            if (list != null && list.Count > 0 && list[list.Count - 1] != null)
                            {
                                m_lastMusicEvent = list[list.Count - 1].asset as FmodPlayableAsset;
                                XDebug.singleton.AddGreenLog("timeline last music event =" + m_lastMusicEvent.clip);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void LoadChars()
    {
        if (m_timelineData.chars != null)
        {
            for (int i = 0; i < m_timelineData.chars.Length; i++)
            {
                var ch = m_timelineData.chars[i];
                if (ch.sync) LoadChar(ch);
            }
        }
    }

    /// <summary>
    /// 加载局部的timeline，可以有多个，这种timeline用于烘托场景的气氛，可以有角色、特效、动画等等。
    /// 和全局的timeline的不同点在于，全局timeline只能同时存在一个，这个也是历史原因。所以后面在扩展的时候
    /// 写了一个TimelinePrefabLoader（它是RTimeline的部分类）用于同时加载多个timeline使用。
    /// </summary>
    private void LoadLocalTimelineUpdate()
    {
        for (int i = 0; i < m_timelinePrefabs.Count; ++i)
        {
            LoadSingleLocalTimeline(m_timelinePrefabs[i]);
        }
    }

    private void LoadSingleLocalTimeline(TimelinePrefabData timelineData)
    {
        float t = (float)timelineData.m_playableDirector.time;
        DirectorWrapMode wrapMode = timelineData.m_playableDirector.extrapolationMode;
        if (timelineData != null && timelineData.chars != null)
        {
            for (int i = 0; i < timelineData.chars.Length; i++)
            {
                var ch = timelineData.chars[i];
                var st = ch.GetState(t);
                if (!ch.sync)
                {
                    if (st == CharLoadState.Load)
                    {
                        LoadRole(ch, timelineData);
                    }
                }

                if (wrapMode != DirectorWrapMode.Loop && st == CharLoadState.Unload) //TimelinePrefab如果是循环模式，则不要卸载，等待切场景时卸载。
                {
                    UnloadRole(ch, i, timelineData);
                }
            }
            for (int i = 0; i < timelineData.m_maxFxCount; i++)
            {
                FxContext ctx = timelineData.m_fxContextPool[i] as FxContext;
                if (ctx != null && !ctx.sync && timelineData.m_fxGoPool[i] == null)
                {
                    if (t >= ctx.data.loadtime) LoadEffect(ctx, timelineData);
                }
            }
        }
    }

    private void LoadGlobalTimelineUpdate()
    {
        if (m_director == null) return;
        float t = (float)m_director.time;
        if (m_timelineData?.chars != null)
        {
            for (int i = 0; i < m_timelineData.chars.Length; i++)
            {
                var ch = m_timelineData.chars[i];
                var st = ch.GetState(t);
                if (!ch.sync)
                {
                    if (st == CharLoadState.Load)
                    {
                        LoadChar(ch);
                    }
                }
                if (st == CharLoadState.Unload)
                {
                    UnloadChar(ch, i);
                }
            }
            for (int i = 0; i < p_maxFx; i++)
            {
                var ctx = fx_ctx_pool[i];
                if (ctx != null && !ctx.sync && fx_go_pool[i] == null)
                {
                    if (t >= ctx.data.loadtime) LoadFx(ctx);
                }
            }
        }
    }

    private void LoadFxs()
    {
        if (fx_tracks != null)
        {
            int j = 0;
            for (int i = 0; i < fx_tracks.Count; i++)
            {
                if (j > fx_max)
                {
                    p_maxFx = fx_max;
                    XDebug.singleton.AddWarningLog("timeline fx overange, max: " + fx_max);
                    break;
                }

                var track = fx_tracks[i];
                var clips = track.GetClips();
                foreach (var c in clips)
                {
                    var asset = c.asset as ControlPlayableAsset;
                    if (asset?.fxData != null)
                    {
                        var dat = asset.fxData;
                        if (!string.IsNullOrEmpty(dat.path))
                        {
                            var obj = asset.sourceGameObject.Resolve(m_director);
                            if (obj != null) continue;

                            var ctx = SharedObjectPool<FxContext>.Get();
                            fx_ctx_pool[j] = ctx;
                            ctx.data = dat;
                            ctx.fx_indx = j++;
                            ctx.asset = asset;
                            ctx.sync = dat.loadtime < sync_time;
                            if (ctx.sync) LoadFx(ctx);
                        }
                    }
                }
            }
            p_maxFx = (uint)j;
        }
    }

    class FxContext : ISharedObject
    {
        public ControlFxData data;
        public int fx_indx;
        public ControlPlayableAsset asset;
        public bool sync;

        public void Reset()
        {
            asset = null;
            data = null;
        }
    }

    private void LoadFx(FxContext ctx)
    {
        string pat = ctx.data.path;

        bool isNewName = LoadMgr.singleton.isUseBundle;

#if !UNITY_EDITOR  //打包的时候 资源被sfx重定向
        isNewName=true;
#endif

        if (isNewName)
        {
            pat = pat.Substring(pat.LastIndexOf('/'));
            pat = "runtime/sfx" + pat.ToLower();
        }

        AssetHandler fxAsset = null;
        var go = EngineUtility.LoadPrefab(pat, ref fxAsset, 0, false, null, true); //timeline里的特效不使用池子
        //var go = XResourceLoaderMgr.singleton.CreateFromAsset<GameObject>(pat, ".prefab");
        LoadFxCallback(go, fxAsset, ctx);
    }

    private void LoadFxCallback(GameObject go, AssetHandler fxAsset, FxContext context)
    {
        if (go != null && context != null)
        {
            SetSFXLayerMask(go);
            go.SetActive(false);
            ControlFxData dat = context.data;

            if (!string.IsNullOrEmpty(dat.avatar))
            {
                BindFx2Char(dat, go, context.asset);
            }
            go.transform.localPosition = dat.pos;
            go.transform.localRotation = dat.rot;
            go.transform.localScale = dat.scale;

            ControlPlayableAsset asset = context.asset;
            asset?.Rebuild(go, m_director);
            SharedObjectPool<FxContext>.Release(context);
            int idx = context.fx_indx;
            fx_go_pool[idx] = fxAsset;
            fx_ctx_pool[idx] = null;
        }
        else
        {
            if (context != null)
            {
                if (context.data != null)
                {
                    XDebug.singleton.AddErrorLog("load fx error prefab not find:" + context.data.path);
                }
                int idx = context.fx_indx;
                fx_ctx_pool[idx] = null;
            }
            LoadMgr.singleton.Destroy(ref fxAsset);
        }
    }

    private void SetSFXLayerMask(GameObject go)
    {
        ParticleSystemRenderer[] renderers = go.GetComponentsInChildren<ParticleSystemRenderer>();
        for (int i = 0; i < renderers.Length; ++i)
        {
            renderers[i].renderingLayerMask = DefaultGameObjectLayer.SRPLayer_DefaultMask;
        }
    }


    private void BindFx2Char(ControlFxData dat, GameObject fx, ControlPlayableAsset asset)
    {
        for (int i = 0; i < m_timelineData.chars?.Length; i++)
        {
            var ch = m_timelineData.chars[i];
            if (ch == null) continue;
            if (asset.m_roleIndex != -1 && i == (asset.m_roleIndex - 1))
            {
                if (ch.xobj != null)
                {
                    var child = ch.xobj.Find(dat.bonePath);
                    if (child)
                    {
                        fx.transform.parent = child;
                        break;
                    }
                }
            }
            else if (ch.prefab == dat.avatar)
            {
                if (ch.xobj != null)
                {
                    var child = ch.xobj.Find(dat.bonePath);
                    if (child)
                    {
                        fx.transform.parent = child;
                    }
                }
            }
        }
    }


    private void LoadChar(OrignalChar ch)
    {
        if (ch.tracks != null && ch.xobj == null)
        {
            ref var cc = ref GameObjectCreateContext.createContext;
            cc.Reset();
            cc.m_usePool = false; //如果资源是同名的，在场景里就有的，则依然还会使用池子，因为src那边加载prefab，默认使用池子，timeline的获取的是老的uoi，其usePool已经被标记为true。

            int index = Array.IndexOf(m_timelineData.chars, ch);
            if (m_context.m_dynamicRoleInfos != null && index < m_context.m_dynamicRoleInfos.Count)
            {
                cc.name = m_context.m_dynamicRoleInfos[index] == null ? String.Empty : m_context.m_dynamicRoleInfos[index].m_prefab;
                ch.scale = m_context.m_dynamicRoleInfos[index].m_scale;
                XDebug.singleton.AddGreenLog("loadprefab=" + cc.name);
            }
            else
            {
                cc.name = ch.prefab;
            }
            ch.m_realPrefab = cc.name;
            if (string.IsNullOrEmpty(cc.name)) return;

            cc.flag.SetFlag(GameObjectCreateContext.Flag_SetPrefabName | GameObjectCreateContext.Flag_NotSyncPos);
            cc.renderLayerMask = (uint)ch.Layer;
            cc.immediate = true;
            cc.cb = LoadCharCallback;
            var xgo = XGameObject.CreateXGameObject(ref cc, true);
            xgo.EndLoad(ref cc);
            PostLoadChar(xgo, ch, true);
        }
    }

    private bool LoadCharCallback(XGameObject go)
    {
        if (m_asyncChars.ContainsKey(go))
        {
            PostLoadChar(go, m_asyncChars[go], false);
            m_asyncChars.Remove(go);
        }
        return true;
    }

    private void PostLoadChar(XGameObject xgo, OrignalChar ch, bool sync)
    {
        var tf = xgo.Find("");
        if (tf == null)
        {
            if (sync)
                m_asyncChars.Add(xgo, ch);
            else
                XDebug.singleton.AddErrorLog("timeline load char failed, check refrence assets and export: ", xgo.prefabName);
            return;
        }

        if (ch.root) tf.parent = ch.root;
        tf.localPosition = ch.pos;
        tf.localRotation = Quaternion.Euler(ch.rot);
        tf.localScale = ch.scale;
        xgo.Ator.cullingMode = AnimatorCullingMode.AlwaysAnimate;   //ch.cull;

        Animator animator = tf.GetComponent<Animator>();
        if (animator != null) animator.applyRootMotion = ch.m_applyRootMotion;

        ch.xobj = xgo;
        int idx = Array.IndexOf(m_timelineData.chars, ch);
        m_callback?.Invoke(xgo, idx);

        if (ch.layer >= 0 && ch.layer < GameObjectLayerHelper.showLayers.Length)
        {
            xgo.SetRenderLayerMask((uint)(1 << GameObjectLayerHelper.showLayers[ch.layer]));
        }

        //bind track
        BindTrack(ch, tf);

        //改层for_liufanyu
        ControlLayer(tf, "Role");

        //set hide and show parts
        ControlPartsVisible(xgo, ch.parts, ref ch.m_partsFlag, false, true);
        ControlPartsVisible(xgo, ch.m_showParts, ref ch.m_showPartsFlag, true, true);

        //if need look at
        InitLookAtInfo(tf, ch);

        //if need facial expression
        LoadFacialAnimationClips(tf, ch);

        //if need self shadow
        AddSelfShadowComponent(tf, ch, idx);
    }

    /// <summary>
    /// 将加载好的角色绑定到指定的轨道
    /// </summary>
    /// <param name="ch"></param>
    /// <param name="tf"></param>
    private void BindTrack(OrignalChar ch, Transform tf)
    {
        for (int i = 0; i < ch.tracks.Length; i++)
        {
            int index = ch.tracks[i];
            if (index >= 0 && index < ch_tracks.Count)
            {
                TrackAsset track = ch_tracks[index];
                GameObject go = tf.gameObject;                                  //如果轨道类型为角色，则使用tf.gameObject进行绑定
                if (track.m_trackAssetType == TrackAssetType.Facial)            //如果轨道类型为表情，则使用ch.m_facialCurveGo进行绑定
                {
                    go = ch.m_facialCurveGo;
                }
                else if (track.m_trackAssetType == TrackAssetType.LookAt)       //如果轨道类型为看向，则使用ch.m_lookAtTargetGo进行绑定
                {
                    go = ch.m_lookAtTargetGo;
                }
                m_director.SetGenericBinding(track, go);
            }
        }
    }

    /// <summary>
    /// pvp的角色是动态加载的，动作也是动态绑定，但是绑定动作后，要进行RebuildGraph才能生效
    /// 最好是在load完timeline结束后立即进行绑定动画，然后RebuildGraph，否则会出现一帧闪的情况
    /// </summary>
    private void BindAnimationForDynamicRole()
    {
        if (m_timelineData.chars != null && m_context.m_dynamicRoleInfos != null && m_context.m_dynamicRoleInfos.Count > 0)
        {
            for (int i = 0; i < m_timelineData.chars.Length; i++)
            {
                OrignalChar ch = m_timelineData.chars[i];
                for (int j = 0; j < ch.tracks.Length; j++)
                {
                    int trackIndex = ch.tracks[j];
                    if (trackIndex >= 0 && trackIndex < ch_tracks.Count)
                    {
                        TrackAsset track = ch_tracks[trackIndex];
                        int index = Array.IndexOf(m_timelineData.chars, ch);
                        var clips = track.GetClips();

                        foreach (var item in clips)
                        {
                            AnimationPlayableAsset asset = item.asset as AnimationPlayableAsset;
                            AssetHandler animHandle = null;
#if UNITY_EDITOR
                            LoadMgr.GetAssetHandler(ref animHandle, "Assets/BundleRes/" + m_context.m_dynamicRoleInfos[index].m_animation, ResObject.ResExt_Anim);
                            LoadMgr.loadContext.Init(null, null, LoadMgr.LoadForceImmediate | LoadMgr.UseFullPath);
#else
            LoadMgr.GetAssetHandler(ref animHandle, m_context.m_dynamicRoleInfos[index].m_animation, ResObject.ResExt_Anim);
            LoadMgr.loadContext.Init(null, null, LoadMgr.LoadForceImmediate);
#endif
                            LoadMgr.singleton.LoadAsset<AnimationClip>(animHandle, ResObject.ResExt_Anim, true);
                            asset.clip = animHandle.obj as AnimationClip;
                            m_context.m_dynamicRoleInfos[index].m_animationAssetHander = animHandle;
                        }
                    }
                }
            }
            Director.RebuildGraph(); //must rebuild
        }
    }

    private void ControlLayer(Transform tf, string layerName)
    {
        Renderer[] children = tf.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < children.Length; ++i)
        {
            children[i].gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    /// <summary>
    ///  /// <summary>
    /// 人物加载好之后，控制part隐藏或者显示；在Unload的时候，要记得恢复
    /// parts——隐藏的部位数组
    /// m_showParts——显示的部位数组
    /// <param name="xgo">模型对象</param>
    /// <param name="parts">显示隐藏变量数组</param>
    /// <param name="flags">在initFlag为true的时候，要记录模型的初始化part的显示状态，在initFlag为false的时候，表示要进行恢复状态</param>
    /// <param name="visible">在initFlag为true的时候，使用visible设置部件的显示与隐藏</param>
    /// <param name="initFlag">如果initFlag为true，则记录初始显示的值</param>
    /// </summary>
    private void ControlPartsVisible(XGameObject xgo, string[] parts, ref bool[] flags, bool visible, bool initFlag)
    {
        if (parts == null) return;
        int len = parts.Length;
        if (initFlag) flags = new bool[len];
        for (int i = 0; i < len; i++)
        {
            Transform t = xgo.Find(parts[i]);
            if (t == null) continue;
            if (initFlag)
            {
                flags[i] = t.gameObject.activeSelf; //record visibility when character loaded.
                t.gameObject.SetActive(visible);
                SkinnedMeshRenderer skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null) skinnedMeshRenderer.enabled = visible;
            }
            else
            {
                t.gameObject.SetActive(flags[i]); //use flags to revert visibility
                SkinnedMeshRenderer skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null) skinnedMeshRenderer.enabled = flags[i];
            }
        }
    }

    private void AddSelfShadowComponent(Transform roleTransform, OrignalChar data, int roleIndex)
    {
        DynamicSelfShadow comp = roleTransform.GetComponent<DynamicSelfShadow>();
        if (data.m_selfShadow)
        {
            if (comp == null) comp = roleTransform.gameObject.AddComponent<DynamicSelfShadow>();
            if (data.m_selfShadow && data.m_selfShadowGo != null)
            {
                DynamicSelfShadowCurve curve = data.m_selfShadowGo.GetComponent<DynamicSelfShadowCurve>();
                comp.m_curve = curve;
            }
            comp.m_roleIndex = roleIndex;
            comp.m_selfShadowCb = m_selfShadowCb;
        }
        else
        {
            if (comp != null)
            {
                GameObject.Destroy(comp);
            }
        }
    }

    private void InitLookAtInfo(Transform roleTransform, OrignalChar data)
    {
        if (data.m_lookAtTargetGo != null)
        {
            LookAt comp = roleTransform.GetComponent<LookAt>();
            if (comp == null) comp = roleTransform.gameObject.AddComponent<LookAt>();
            comp.m_self = roleTransform;
            Transform head = roleTransform.Find(HEAD_BONE_PATH);
            comp.m_head = head;
            comp.m_lookTarget = data.m_lookAtTargetGo.transform;
        }
    }

    private void LoadFacialAnimationClips(Transform roleTransform, OrignalChar data)
    {
        if (data.m_facialClips != null && data.m_facialClips.Length > 0)
        {
            int len = data.m_facialClips.Length;
            FacialExpression comp = null;
            if (data.m_facialCurveGo != null)
            {
                FacialExpressionCurve curveComp = data.m_facialCurveGo.GetComponent<FacialExpressionCurve>();
                comp = roleTransform.GetComponent<FacialExpression>();
                if (comp == null) comp = roleTransform.gameObject.AddComponent<FacialExpression>();
                comp.m_curve = curveComp;
                comp.m_clips = new List<FacialAnimationClip>();

                data.m_clips = new List<AssetHandler>();
                for (int i = 0; i < len; ++i)
                {
                    if (!string.IsNullOrEmpty(data.m_facialClips[i]))
                    {
                        // load animation clip
                        AnimationClip animationClip = LoadAnimationClip(data.m_facialClips[i], data);
                        FacialAnimationClip clip = new FacialAnimationClip();
                        FacialClipType facialClipType = FacialClipType.idle;
                        if (data.m_facialClipTypes != null && i < data.m_facialClipTypes.Length)
                        {
                            facialClipType = (FacialClipType)data.m_facialClipTypes[i];
                        }
                        clip.m_clipType = facialClipType;
                        clip.m_clip = animationClip;
                        clip.m_fold = false;
                        comp.m_clips.Add(clip);
                    }
                }
                comp.Init();
            }
        }
    }

    private AnimationClip LoadAnimationClip(string path, OrignalChar data)
    {
        AnimationClip animationClip = null;
        AssetHandler animHandle = null;
#if UNITY_EDITOR
        LoadMgr.GetAssetHandler(ref animHandle, "Assets/BundleRes/" + path, ResObject.ResExt_Anim);
        LoadMgr.loadContext.Init(null, null, LoadMgr.LoadForceImmediate | LoadMgr.UseFullPath);
#else
        LoadMgr.GetAssetHandler(ref animHandle, path, ResObject.ResExt_Anim);
        LoadMgr.loadContext.Init(null, null, LoadMgr.LoadForceImmediate);
#endif
        LoadMgr.singleton.LoadAsset<AnimationClip>(animHandle, ResObject.ResExt_Anim, true);
        animationClip = animHandle.obj as AnimationClip;
        data.m_clips.Add(animHandle);
        return animationClip;
    }

    private void RecycleOrignal()
    {
        ch_tracks?.Clear();
        fx_tracks?.Clear();
        m_asyncChars.Clear();
        m_gameActiveVC = null;

        if (m_timelineData?.chars != null)
        {
            for (int i = 0; i < m_timelineData.chars.Length; i++)
            {
                var ch = m_timelineData.chars[i];
                UnloadChar(ch, i);
            }
        }

        if (m_context != null && m_context.m_dynamicRoleInfos != null)
        {
            for (int i = 0; i < m_context.m_dynamicRoleInfos.Count; ++i)
            {
                LoadMgr.singleton.Destroy(ref m_context.m_dynamicRoleInfos[i].m_animationAssetHander);
            }
        }

        for (int i = 0; i < fx_max; i++)
        {
            var asset = fx_go_pool[i];
            if (asset != null)
            {
                LoadMgr.singleton.Destroy(ref asset);
                fx_go_pool[i] = null;
            }
            if (fx_ctx_pool[i] != null)
            {
                SharedObjectPool<FxContext>.Release(fx_ctx_pool[i]);
                fx_ctx_pool[i] = null;
            }
        }
    }

    private bool UnloadChar(OrignalChar ch, int i)
    {
        if (ch.xobj != null)
        {
            Transform tf = ch.xobj.Find("");
            if (tf == null) return false;
            ControlLayer(tf, "Default");
            ControlPartsVisible(ch.xobj, ch.parts, ref ch.m_partsFlag, true, false);
            ControlPartsVisible(ch.xobj, ch.m_showParts, ref ch.m_showPartsFlag, false, false);
            UnloadLookAtInfo(ch);
            UnloadFacialAnimationClips(ch);
            UnloadSelfShadowComponent(ch);
            m_callback?.Invoke(ch.xobj, -(i + 1));
            XGameObject.DestroyXGameObject(ch.xobj);
            ch.xobj = null;
            return true;
        }
        return false;
    }

    private void UnloadLookAtInfo(OrignalChar data)
    {
        if (data.m_lookAtTargetGo != null)
        {
            Transform roleTrans = data.xobj.Find("");
            if (roleTrans == null) return;
            LookAt comp = roleTrans.GetComponent<LookAt>();
            if (comp != null)
            {
                GameObject.Destroy(comp);
                comp = null;
            }
        }
    }


    private void UnloadFacialAnimationClips(OrignalChar data)
    {
        if (data.xobj != null && data.m_clips != null)
        {
            Transform roleTrans = data.xobj.Find("");
            if (roleTrans == null) return;
            FacialExpression comp = roleTrans.GetComponent<FacialExpression>();
            if (comp != null)
            {
                GameObject.Destroy(comp);
                comp = null;
            }

            for (int i = 0; i < data.m_clips.Count; ++i)
            {
                AssetHandler ah = data.m_clips[i];
                LoadMgr.singleton.Destroy(ref ah);
                data.m_clips[i] = null;
            }
            data.m_clips.Clear();
            data.m_clips = null;
        }
    }

    private void UnloadSelfShadowComponent(OrignalChar data)
    {
        if (data.m_selfShadow)
        {
            Transform roleTrans = data.xobj.Find("");
            if (roleTrans == null) return;
            DynamicSelfShadow comp = roleTrans.GetComponent<DynamicSelfShadow>();
            if (comp != null)
            {
                GameObject.Destroy(comp);
                comp = null;
            }
        }
    }

    /// <summary>
    /// 暂时未用到
    /// </summary>
    private void BlendGameCamera()
    {
        bool blend = m_timelineData.mode == FinishMode.NORMAL || m_timelineData.mode2 == FinishMode.NORMAL;
        if (m_gameActiveVC && blend)
        {
            CinemachineMixer.GetMasterPlayableDirector = GetDirector;
            CinemachineMixer.GetEditorVCInfo = SetVCInfo;
        }
    }


    private bool SetVCInfo(out CStyle style1, out float t1, out CinemachineVirtualCameraBase v1,
                    out CStyle style2, out float t2, out CinemachineVirtualCameraBase v2)
    {
        if (m_timelineData != null)
        {
            t1 = m_timelineData.blendInTime;
            t2 = m_timelineData.blendOutTime;
            style1 = (CStyle)m_timelineData.BlendInMode;
            style2 = (CStyle)m_timelineData.BlendOutMode;
            v1 = m_timelineData.mode2 == FinishMode.NORMAL ? m_gameActiveVC : null;
            v2 = m_timelineData.mode == FinishMode.NORMAL ? m_gameActiveVC : null;
            return true;
        }
        else
        {
            t1 = t2 = 0;
            v1 = v2 = null;
            style1 = style2 = CStyle.Custom;
            return false;
        }
    }

    /// <summary>
    /// 给某个角色使用某种特殊的效果，effectID是效果的id，这个美术制作
    /// </summary>
    /// <param name="go"></param>
    /// <param name="effectID"></param>
    /// <returns></returns>
    public EffectInst BeginEffect(GameObject go, uint effectID)
    {
        if (m_context != null && m_context.m_actors != null && m_context.m_actors.ContainsKey(go))
        {
            IEntityHandler entity = m_context.m_actors[go];
            return RenderEffectSystem.BeginEffect(entity, (short)effectID);
        }
        return null;
    }

    /// <summary>
    /// 结束角色效果
    /// </summary>
    /// <param name="go"></param>
    /// <param name="effectInst"></param>
    public void EndEffect(GameObject go, EffectInst effectInst)
    {
        if (m_context != null && m_context.m_actors != null && m_context.m_actors.ContainsKey(go))
        {
            IEntityHandler entity = m_context.m_actors[go];
            RenderEffectSystem.EndEffect(entity, effectInst);
        }
    }

    public void InitCinemachineFollowAndLookAt(OrignalTimelineData data)
    {
        CinemachineTrack track = null;
        if (RTimeline.singleton.Director == null || RTimeline.singleton.Director.playableAsset == null || data == null)
            return;

        foreach (var pb in RTimeline.singleton.Director.playableAsset.outputs)
        {
            if (pb.sourceObject is CinemachineTrack obj)
            {
                track = obj;
            }
        }

        if (track != null)
        {
            var list = track.GetClipList();

            if (list.Count > 0)
            {
                if (m_timelineData != null && Application.isPlaying && m_timelineData.BlendInMode != 0)
                {
                    list[0].easeInDuration = m_timelineData.blendInTime;
                }
            }

            for (int i = 0; i < list.Count; ++i)
            {
                var camAsset = list[i].asset as CinemachineShot;
                if (camAsset.vcb == null)
                {
                    RTimeline.singleton.Director.RebuildGraph();
                }

                if (camAsset.vcb != null)
                {
                    if (data.cinemachineFollow != null && data.cinemachineFollow.Length > 0 && !string.IsNullOrEmpty(data.cinemachineFollow[i]))
                    {
                        string path = data.cinemachineFollow[i];
                        if (!RTimeline.singleton.isPlaying)
                        {
                            path = "timeline_Role_Root/" + data.cinemachineFollow[i];
                        }
                        camAsset.vcb.FollowPath = path;
                    }
                    if (data.cinemachineLookAt != null && data.cinemachineLookAt.Length > 0 && !string.IsNullOrEmpty(data.cinemachineLookAt[i]))
                    {
                        string path = data.cinemachineLookAt[i];
                        if (!RTimeline.singleton.isPlaying)
                        {
                            path = "timeline_Role_Root/" + data.cinemachineLookAt[i];
                        }
                        camAsset.vcb.LookAtPath = path;
                    }
                }
            }
        }
    }

    public static GameObject GetLookatAndFollowObj(string path, GameObject[] objs)
    {
        GameObject obj = null;
        if (Application.isPlaying && objs != null)
        {
            for (int j = 0; j < objs.Length; j++)
            {
                string[] paths = path.Split('/');
                if (paths.Length == 1 && objs[j].name == paths[0])
                {
                    obj = objs[j];
                    break;
                }
                else if (paths.Length > 1 && objs[j].name == paths[0])
                {
                    string tmp = path.Replace(paths[0] + "/", string.Empty);
                    obj = objs[j].transform.Find(tmp).gameObject;
                    break;
                }
            }
        }
        else if (!Application.isPlaying)
        {
            obj = GameObject.Find(path);
        }
        return obj;
    }

    public void GrabScreen()
    {
        if (ScreenEffectAnimation != null)
        {
            ScreenEffectAnimation.Grab();
        }
    }
}