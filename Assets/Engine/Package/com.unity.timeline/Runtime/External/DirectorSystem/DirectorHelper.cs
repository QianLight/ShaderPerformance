using System;
using System.Collections.Generic;
using System.Diagnostics;
using CFEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public enum DirectPlayState
    {
        Stop,
        Pause,
        Playing
    }

    // #if UNITY_EDITOR
    [System.Serializable]
    // #endif
    public struct EnvRefParam
    {
        public int envType;
        public int paramID;
        public byte valueMask;

        // #if UNITY_EDITOR
        [System.NonSerialized]
        // #endif
        public int curveIndex;

        public void Load (CFBinaryReader reader)
        {
            envType = reader.ReadInt32 ();
            paramID = reader.ReadInt32 ();
            valueMask = reader.ReadByte ();
            curveIndex = reader.ReadInt32 ();
        }
#if UNITY_EDITOR
        public static void SaveEnvParam (System.IO.BinaryWriter bw, ref EnvRefParam erp)
        {
            bw.Write (erp.envType);
            bw.Write (erp.paramID);
            bw.Write (erp.valueMask);
            bw.Write (erp.curveIndex);
        }
#endif
    }

#if UNITY_EDITOR
    [System.Serializable]
#endif
    public struct CustomCurveRef
    {
        public CurveContext context;
#if UNITY_EDITOR
        public AnimationCurve[] curve;
#endif
        public void Reset ()
        {
            context.curveIndex = -1;
        }
    }

    public struct RecordAnimSampler
    {
        public AnimationClip clip;
        public TrackAsset track;
        public GameObject obj;
        public void Bind (PlayableDirector dp)
        {
            if (dp != null && obj == null && track != null)
            {
                Animator ator = dp.GetGenericBinding (track) as Animator;
                obj = ator != null?ator.gameObject : null;
            }
        }
        public void Reset ()
        {
            clip = null;
            track = null;
            obj = null;
        }
    }

    [System.Serializable]
    public struct TimelineShadowTarget
    {
        public string path;
        //[NonSerialized]
        //public XCsActor actor;
    }

    [System.Serializable]
    public class TimelineSelfShadowConfig
    {
        public float start;
        public float end;
        public TimelineShadowTarget a0;
        public TimelineShadowTarget a1;
        public TimelineShadowTarget a2;
        public TimelineShadowTarget a3;
        public SelfShadowConfig config;
        public uint flag = 0;
        public static uint Flag_Show = 0x00000001;
#if UNITY_EDITOR
        [NonSerialized]
        public Transform shadowLight;
        [NonSerialized]
        public MonoBehaviour dummyBox;
#endif
        public int InTime (float time)
        {
            if (time < start)
            {
                return -1;
            }
            else if (time >= end)
            {
                return 1;
            }
            return 0;
        }
    }

    public delegate void EvaluateAnimCb (AnimCurveShare animCurve, ref CurveContext context, float time, ref Vector4 v);
    public delegate void EvaluateAnimV3Cb (AnimCurveShare animCurve, ref CurveContext context, float time, ref Vector3 v);
    public delegate void EvaluateAnimV2Cb (AnimCurveShare animCurve, ref CurveContext context, float time, ref Vector2 v);
    public delegate void EvaluateAnimFloatCb (AnimCurveShare animCurve, ref CurveContext context, float time, ref float v);
    public delegate DirectPlayableAsset CreateAssetCb (byte type, CFBinaryReader reader);
    public delegate void ReleaseAssetCb (DirectPlayableAsset asset);
    public delegate DirectorTrackAsset CreateTrackCb (byte type);
    public delegate void ReleaseTrackCb (DirectorTrackAsset track);
    public delegate void LoadBindObjectCb (byte trackType, DirectorTrackAsset track, string streamName);
    public delegate void UnLoadBindObjectCb (ICallbackData loadData);
    public delegate DirectorSignalEmmiter CreateSignalCb (byte type);
    public delegate void ReleaseSignalCb (DirectorSignalEmmiter signal);
    public partial class DirectorHelper : CFSingleton<DirectorHelper>
    {
        public int Version = 0;
        public string[] strings;
        public byte assetCount;
        // public int strCount;
        public AnimCurveShare animCurve;
        public GameObject cine;
        private AssetHandler cineAH;

        public EnvRefParam[] envParams;
        public int envParamCount = 0;
        public DirectorSignalEmmiter[] signals;
        public short signalCount = 0;
        public DirectorClip[] clips;
        public short clipCount = 0;
        public DirectorTrackAsset[] tracks;
        public byte trackCount = 0;
        public int loadResCount = 0;
        public INotificationReceiver globalReceiver;

        #region animation record
        public RecordAnimSampler[] recordList = new RecordAnimSampler[maxRecordList];
        public int recordCount = 0;
        public static int maxRecordList = 10;

        public List<TimelineSelfShadowConfig> selfShadowConfigs;
        public int selfShadowConfigIndex = -1;
        public TimelineSelfShadowConfig selfShadowConfig = null;
        #endregion

        public static int playCount = 0;
        public static bool isOrignal = true;
        public static PlayableAsset timelineAsset;
        public static EvaluateAnimCb evaluateAnim;
        public static EvaluateAnimV3Cb evaluateAnimV3;
        public static EvaluateAnimV2Cb evaluateAnimV2;
        public static EvaluateAnimFloatCb evaluateAnimFloat;
        public static CreateAssetCb createAsset;
        public static ReleaseAssetCb realeaseAsset;
        public static CreateSignalCb createSignal;
        public static ReleaseSignalCb realeaseSignal;
        public static CreateTrackCb createTrack;
        public static ReleaseTrackCb releaseTrack;
        public static LoadBindObjectCb loadBindObject;
        public static UnLoadBindObjectCb unLoadBindObject;

        public static byte AssetType_Animation = 0;
        public static byte AssetType_Active = (byte) (AssetType_Animation + 1);
        public static byte AssetType_Control = (byte) (AssetType_Active + 1);
        public static byte AssetType_Anchor = (byte) (AssetType_Control + 1);
        public static byte AssetType_BoneFx = (byte) (AssetType_Anchor + 1);
        public static byte AssetType_AnimationClip = (byte) (AssetType_BoneFx + 1);
        public static byte EngineAssetCount = (byte) (100);

        public static byte TrackType_Mark = 0;
        public static byte TrackType_Animation = (byte) (TrackType_Mark + 1);
        public static byte TrackType_Active = (byte) (TrackType_Animation + 1);
        public static byte TrackType_Control = (byte) (TrackType_Active + 1);
        public static byte TrackType_Playable = (byte) (TrackType_Control + 1);
        public static byte TrackType_Anchor = (byte) (TrackType_Playable + 1);
        public static byte TrackType_BoneFx = (byte) (TrackType_Anchor + 1);
        public static byte TrackType_EmptyAnimation = (byte) (TrackType_BoneFx + 1);
        public static byte TrackType_Cine = (byte) (TrackType_EmptyAnimation + 1);
        public static byte TrackType_AnimClip = (byte) (TrackType_Cine + 1);
        public static byte EngineTrackCount = (byte) (100);

        public static int ExternalType_Light = 0;
        public static int ExternalType_SelfShadow = 1;
        public void Init ()
        {
            if (strings == null)
                strings = new string[64];
            if (envParams == null)
                envParams = CFAllocator.Allocate<EnvRefParam> (128);
            if (signals == null)
                signals = CFAllocator.Allocate<DirectorSignalEmmiter> (128);
            if (clips == null)
                clips = CFAllocator.Allocate<DirectorClip> (64);
            if (tracks == null)
                tracks = CFAllocator.Allocate<DirectorTrackAsset> (64);
            if (animCurve == null)
            {
                animCurve = new AnimCurveShare ();
            }
            animCurve.Init (512 * 5, 512);
        }

        public void Uninit ()
        {
            if (animCurve != null)
                animCurve.Uninit ();
#if UNITY_EDITOR
            curveIndex.Clear ();
#endif

        }

        public static PlayableDirector GetDirector ()
        {
            //             #if UNITY_EDITOR
            //             if()
            // #endif
            return EngineContext.director;
        }
        public static DirectPlayState GetPlayState ()
        {
            var director = GetDirector ();
            if (director != null && director.playableGraph.IsValid ())
            {
                if (director.playableGraph.IsPlaying ())
                {
                    return DirectPlayState.Playing;
                }
                return DirectPlayState.Pause;
            }
            return DirectPlayState.Stop;
        }
        public static void Play ()
        {
            var director = GetDirector ();
            if (director != null)
            {
                bool needRebuild = playCount == 0;
                PlayableAsset playableAsset = null;
                if (isOrignal)
                {
                    playableAsset = timelineAsset;
                }
                else
                {
                    playableAsset = DirectorAsset.instance;
                }
                if (director.playableAsset != playableAsset)
                {
                    director.playableAsset = playableAsset;
                    needRebuild = false;
                }
                if (needRebuild)
                    director.RebuildGraph ();
                director.enabled = true;
                director.Play ();
                playCount++;
                // PlayableDirector.ResetFrameTiming ();
            }
        }

        public static void Pause ()
        {
            var director = GetDirector ();
            if (director != null)
            {
                director.Pause ();
            }
        }

        public static void Reset ()
        {
            var director = GetDirector ();
            if (director != null)
            {
                director.time = 0;
                DirectorAsset.instance.SetSpeed (1);
            }
        }

        public static void Stop (bool enable = true)
        {
            var director = GetDirector ();
            if (director != null)
            {
                director.Stop ();
                director.enabled = enable;
                Reset ();
            }
        }
        public void BindObject (byte trackType, DirectorTrackAsset track, string streamName)
        {
            if (track.parentTrackIndex == -1 && loadBindObject != null)
            {
                loadBindObject (trackType, track, streamName);
            }
        }

        public void UnBindObject (DirectorTrackAsset track, ICallbackData loadData)
        {
            if (track.parentTrackIndex == -1 && unLoadBindObject != null)
            {
                unLoadBindObject (loadData);
            }
        }
        private void Bind (DirectorTrackAsset track, UnityEngine.Object obj)
        {
            if (obj is Transform)
            {
                track.BindTransform = obj as Transform;
                track.IsActive = track.BindTransform.gameObject.activeSelf;
            }
            else if (obj is Animator)
            {
                track.BindTransform = (obj as Animator).transform;
                track.IsActive = track.BindTransform.gameObject.activeSelf;
                if (track is DirectorAnimationTrack)
                {
                    (track as DirectorAnimationTrack).animator = (obj as Animator);
                }
            }
        }

        public void BindObject2Track (DirectorTrackAsset track, UnityEngine.Object obj, ICallbackData cbData)
        {
            var director = GetDirector ();
            director.SetGenericBinding (track, obj);
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                TrackAsset ta = FindTrack (track);
                if (ta != null)
                {
                    director.SetGenericBinding (ta, obj);
                }
            }
#endif            
            track.LoadCbData = cbData;
            Bind (track, obj);

        }
        public void PostBind ()
        {
            for (int i = 0; i < trackCount; ++i)
            {
                var t = tracks[i];
                if (t != null &&
                    t.parentTrackIndex >= 0 &&
                    t.parentTrackIndex < trackCount)
                {
                    var track = tracks[t.parentTrackIndex];
                    Bind (t, track.BindTransform);
                }
            }
        }
        public void PreLoad ()
        {
            Init ();
            for (int i = 0; i < strings.Length; ++i)
            {
                strings[i] = null;
            }
        }

        public void BindLight (ref RecordAnimSampler record)
        {
            // if (record.obj != null)
            // {
            //     SFXMgr.singleton.BindLight (record.obj, null);
            // }
        }

        public void LoadCine (string name)
        {
#if UNITY_EDITOR
            string gname = "cineroot";
            cine = GameObject.Find (gname);
            if (cine == null)
            {
#endif
                LoadMgr.singleton.Destroy(ref cineAH);
                cine = EngineUtility.LoadPrefab(name, ref cineAH, 0, false,null,true);
                if (cine != null)
                {
                    var tran = cine.transform;
                    tran.position = Vector3.zero;
                    tran.rotation = Quaternion.identity;
                }
#if UNITY_EDITOR
            }
            cine.name = gname;
            cine.tag = "Timeline";
#endif
        }

        public void Load (CFBinaryReader reader
#if UNITY_EDITOR
            , string name
#endif
        )
        {
#if UNITY_EDITOR
            DebugLog.DebugStream (reader,
                string.Format ("debug load director asset:{0}", name),
                DebugSream.Clear);
#endif
            Version = reader.ReadInt32 ();
            //strings
            short strCount = reader.ReadInt16 ();
            assetCount = reader.ReadByte ();
            DebugLog.DebugStream (reader, string.Format ("debug asset count:{0} str count:{1}", assetCount, strCount));
            short strEnd = (short) (assetCount + strCount);
#if UNITY_EDITOR
            string readStr = "";
#endif
            for (short i = assetCount; i < strEnd; ++i)
            {
                strings[i] = reader.ReadString ();
#if UNITY_EDITOR
                readStr += strings[i] + "\r\n";
#endif

            }
#if UNITY_EDITOR
            DebugLog.DebugStream (reader, string.Format ("load str:{0}", readStr));
#endif
            //head
            animCurve.Load (reader);
#if UNITY_EDITOR
            if (Version >= Timeline_Version_Env)
#endif
            {
                envParamCount = reader.ReadInt16 ();
                for (short i = 0; i < envParamCount; ++i)
                {
                    ref var erp = ref envParams[i];
                    erp.Load (reader);
                }
            }

            var directorAsset = DirectorAsset.instance;
            directorAsset.m_Duration = reader.ReadDouble ();
            //clips
            clipCount = reader.ReadInt16 ();
            for (short i = 0; i < clipCount; ++i)
            {
                var clip = clips[i];
                if (clip == null)
                {
                    clip = new DirectorClip ();
                    clips[i] = clip;
                }
                clip.index = i;
                clip.Load (reader);
                DebugLog.DebugStream (reader, string.Format ("clips index {0}", i));
            }
            DebugLog.DebugStream (reader, string.Format ("clips count {0}", clipCount));

            //tracks
            trackCount = reader.ReadByte ();
            for (int i = 0; i < trackCount; ++i)
            {
                byte trackType = reader.ReadByte ();
                short parentIndex = reader.ReadInt16 ();
                var track = CreateTrack (trackType);
                if (track != null)
                {
                    track.trackType = trackType;
                    track.trackIndex = i;
                    track.parentTrackIndex = parentIndex;
                    track.Load (reader);
                }
                else
                {
                    DebugLog.AddErrorLog2 ("error track type:{0}", trackType.ToString ());
                    DebugLog.DebugStream (reader, string.Format ("track index {0} count {1}", i, trackCount));
                    return;
                }
                tracks[i] = track;
            }
            PostBind ();
            DebugLog.DebugStream (reader, string.Format ("tracks count {0}", trackCount));
            //signals
            signalCount = reader.ReadInt16 ();
            for (short i = 0; i < signalCount; ++i)
            {
                byte signalType = reader.ReadByte ();
                var signal = CreateSignal (signalType);
                if (signal != null)
                {
                    signal.signalType = signalType;
                    signal.Load (reader);
                }
                else
                {
                    DebugLog.AddErrorLog2 ("error signal type:{0}", signalType.ToString ());
                    DebugLog.DebugStream (reader, string.Format ("signal index {0}", i));
                    return;
                }
                signals[i] = signal;
            }
            DebugLog.DebugStream (reader, string.Format ("signal count {0}", signalCount), DebugSream.Output);
        }

        public void CheckUnloadCine ()
        {
            if (cine)
            {
                //ExternalHelp.UnSafeDestroy (cine, false, true);
                cine = null;
            }
            LoadMgr.singleton.Destroy(ref cineAH);
        }

        public void UnLoad ()
        {
            if (cine)
            {
                cine.SetActive (false);
            }
            if (isOrignal)
            {
                timelineAsset = null;
            }
            else
            {
                if (strings != null)
                {
                    for (int i = 0; i < strings.Length; ++i)
                    {
                        strings[i] = null;
                    }
                }
                if (clips != null)
                {
                    for (int i = 0; i < clips.Length; ++i)
                    {
                        var clip = clips[i];
                        if (clip != null)
                        {
                            clip.Reset ();
                        }
                    }
                }
                if (signals != null)
                {
                    for (int i = 0; i < signals.Length; ++i)
                    {
                        var signal = signals[i];
                        if (signal != null)
                        {
                            ReleaseSignal (signal);
                            signals[i] = null;
                        }
                    }
                }

                if (tracks != null)
                {
                    var director = GetDirector ();
                    for (int i = 0; i < tracks.Length; ++i)
                    {
                        var track = tracks[i];
                        if (track != null)
                        {
                            if(director != null)
                            {
                                director.ClearGenericBinding(track);
                            }
                            ReleaseTrack (track);
                            tracks[i] = null;
                        }
                    }
                }
                playCount = 0;
                assetCount = 0;
                envParamCount = 0;
                signalCount = 0;
                clipCount = 0;
                trackCount = 0;
                loadResCount = 0;
                DirectorAsset.instance.Reset ();

            }
            for (int i = 0; i < recordList.Length; ++i)
            {
                ref var record = ref recordList[i];
                record.Reset ();
            }
            recordCount = 0;
            // SFXMgr.singleton.ClearBindSfx ();

            if (selfShadowConfigs != null)
            {
                selfShadowConfigs = null;
                selfShadowConfigIndex = -1;
                selfShadowConfig = null;
                WorldSystem.ClearSelfShadow ();
            }

        }
        public bool IsLoadFinish ()
        {
            return loadResCount == 0;
        }

        public string ReadStr (CFBinaryReader reader)
        {
            short index = reader.ReadInt16 ();
            if (strings != null && index >= 0 && index < strings.Length)
            {
                return strings[index];
            }
            return null;
        }
        public string GetStr (int index)
        {
            if (strings != null && index >= 0 && index < strings.Length)
            {
                return strings[index];
            }
            return null;
        }
        public void LoadCurve (CFBinaryReader reader, ref CustomCurveRef c)
        {
            c.context.curveIndex = reader.ReadInt32 ();
        }

        public void ResetCurve (ref CustomCurveRef c)
        {
            if (animCurve != null)
                animCurve.GetCurveContext (ref c.context);
        }

        public void SampleCurve (ref CustomCurveRef curve, float time, ref Vector4 v)
        {
#if UNITY_EDITOR
            if (useEngineAnimCurve)
            {
                if (curve.curve != null)
                {
                    if (curve.curve.Length > 0)
                        v.x = curve.curve[0].Evaluate (time);
                    if (curve.curve.Length > 1)
                        v.y = curve.curve[1].Evaluate (time);
                    if (curve.curve.Length > 2)
                        v.z = curve.curve[2].Evaluate (time);
                    if (curve.curve.Length > 4)
                        v.w = curve.curve[3].Evaluate (time);
                }
            }
            else
#endif
            {
                evaluateAnim (animCurve, ref curve.context, time, ref v);
            }
        }

        public void SampleCurve (ref CustomCurveRef curve, float time, ref Vector3 v)
        {
#if UNITY_EDITOR
            if (useEngineAnimCurve)
            {
                if (curve.curve != null)
                {
                    if (curve.curve.Length > 0)
                        v.x = curve.curve[0].Evaluate (time);
                    if (curve.curve.Length > 1)
                        v.y = curve.curve[1].Evaluate (time);
                    if (curve.curve.Length > 2)
                        v.z = curve.curve[2].Evaluate (time);
                }
            }
            else
#endif
            {
                evaluateAnimV3 (animCurve, ref curve.context, time, ref v);
            }

        }

        public void SampleCurve (ref CustomCurveRef curve, float time, ref float v)
        {
#if UNITY_EDITOR
            if (useEngineAnimCurve)
            {
                if (curve.curve != null && curve.curve.Length > 0)
                {
                    v = curve.curve[0].Evaluate (time);
                }
            }
            else
#endif
            {
                evaluateAnimFloat (animCurve, ref curve.context, time, ref v);
            }

        }

        [Conditional ("UNITY_EDITOR")]
        public void RegisterDrawCurve (int posIndex, int rotIndex)
        {
#if UNITY_EDITOR
            if (posIndex >= 0)
            {
                if (!curveIndex.Exists (x => x.posIndex == posIndex && x.rotIndex == rotIndex))
                {
                var dcc = new DrawCurveContext ()
                {
                posIndex = posIndex,
                rotIndex = rotIndex
                    };
                    curveIndex.Add (dcc);
                    DirectorHelper.singleton.animCurve.RegisterDrawCurve (dcc);
                }
            }
#endif
        }

        public static DirectPlayableAsset CreateAsset (byte type, CFBinaryReader reader)
        {
            if (type == AssetType_Animation)
            {
                return SharedScriptObjectPool<DirectorAnimationPlayableAsset>.Get ();
            }
            else if (type == AssetType_Active)
            {
                return SharedScriptObjectPool<DirectorActivationAsset>.Get ();
            }
            else if (type == AssetType_Control)
            {
                return SharedScriptObjectPool<DirectorControlAsset>.Get ();
            }
            else if (type == AssetType_AnimationClip)
            {
                return SharedScriptObjectPool<AnimationClipPlayAsset>.Get ();
            }
            else if (createAsset != null)
                return createAsset (type, reader);
            return null;
        }

        public static void ReleaseAsset (DirectPlayableAsset asset)
        {
            if (asset is DirectorAnimationPlayableAsset)
            {
                SharedScriptObjectPool<DirectorAnimationPlayableAsset>.Release (asset as DirectorAnimationPlayableAsset);
            }
            else if (asset is DirectorActivationAsset)
            {
                SharedScriptObjectPool<DirectorActivationAsset>.Release (asset as DirectorActivationAsset);
            }
            else if (asset is DirectorControlAsset)
            {
                SharedScriptObjectPool<DirectorControlAsset>.Release (asset as DirectorControlAsset);
            }
            else if (asset is AnimationClipPlayAsset)
            {
                SharedScriptObjectPool<AnimationClipPlayAsset>.Release (asset as AnimationClipPlayAsset);
            }
            else if (realeaseAsset != null)
                realeaseAsset (asset);
            else
            {
                DebugLog.AddErrorLog2 ("unknow asset type:{0}", asset.GetType ().Name);
            }
        }

        public static DirectorSignalEmmiter CreateSignal<T> ()
        where T : DirectorSignalEmmiter,
        new ()
        {
#if UNITY_EDITOR
            return SharedScriptObjectPool<T>.Get ();
#else
            return SharedObjectPool<T>.Get ();
#endif
        }

        public static DirectorSignalEmmiter CreateSignal (byte type)
        {
            if (type == DirectorSignalEmmiter.SignalType_Active ||
                type == DirectorSignalEmmiter.SignalType_Jump ||
                type == DirectorSignalEmmiter.SignalType_Slow)
            {
                return CreateSignal<DirectorSignalEmmiter> ();
            }
            else if (createSignal != null)
                return createSignal (type);
            return null;
        }

        public static void ReleaseSignal<T> (DirectorSignalEmmiter signal)
        where T : DirectorSignalEmmiter,
        new ()
        {
#if UNITY_EDITOR
            SharedScriptObjectPool<T>.Release (signal as T);
#else
            SharedObjectPool<T>.Release (signal as T);
#endif
        }

        public static void ReleaseSignal (DirectorSignalEmmiter signal)
        {
            if (signal is DirectorSignalEmmiter &&
                signal.signalType < DirectorSignalEmmiter.EngineSignalCount)
            {
                ReleaseSignal<DirectorSignalEmmiter> (signal);
            }
            else if (realeaseSignal != null)
                realeaseSignal (signal);
            else
            {
                DebugLog.AddErrorLog2 ("unknow signal type:{0}", signal.GetType ().Name);
            }
        }
        public static DirectorTrackAsset CreateTrack (byte type)
        {
            if (type == TrackType_Animation)
            {
                return SharedScriptObjectPool<DirectorAnimationTrack>.Get ();
            }
            else if (type == TrackType_Active)
            {
                return SharedScriptObjectPool<DirectorActivationTrack>.Get ();
            }
            else if (type == TrackType_Playable ||
                type == TrackType_Control ||
                type == TrackType_Anchor ||
                type == TrackType_BoneFx ||
                type == TrackType_Mark ||
                type == TrackType_AnimClip)
            {
                return SharedScriptObjectPool<DirectorUberTrack>.Get ();
            }
            else if (createTrack != null)
                return createTrack (type);
            return null;
        }
        public static void ReleaseTrack (DirectorTrackAsset track)
        {
            if (track is DirectorAnimationTrack)
            {
                SharedScriptObjectPool<DirectorAnimationTrack>.Release (track as DirectorAnimationTrack);
            }
            else if (track is DirectorActivationTrack)
            {
                SharedScriptObjectPool<DirectorActivationTrack>.Release (track as DirectorActivationTrack);
            }
            else if (track is DirectorUberTrack)
            {
                SharedScriptObjectPool<DirectorUberTrack>.Release (track as DirectorUberTrack);
            }
            else if (releaseTrack != null)
                releaseTrack (track);
            else
            {
                DebugLog.AddErrorLog2 ("unknow asset type:{0}", track.GetType ().Name);
            }
        }

    }
}