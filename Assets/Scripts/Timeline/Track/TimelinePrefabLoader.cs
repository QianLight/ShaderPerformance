using CFEngine;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

partial class RTimeline
{
    private static List<TimelinePrefabData> m_timelinePrefabs = new List<TimelinePrefabData>();
    private static List<TimelinePrefabData> m_unloadRequest = new List<TimelinePrefabData>();
    public static Dictionary<PlayableDirector, XRuntimeFmod> m_fmodDict = new Dictionary<PlayableDirector, XRuntimeFmod>();

    public void LoadTimelinePrefab(TimelineContext context, DirectorBindingData data, Action<XGameObject, int> cb, System.Action<int, int> selfShadowCb)
    {
        string timelineName = context.name;
        AssetHandler asset = null;
        GameObject timelineGo = EngineUtility.LoadPrefab("timeline/" + timelineName, ref asset, 0, context.returnPool,null,true);
        //GameObject timelineGo = XResourceLoaderMgr.singleton.CreateFromPrefab("Timeline/" + timelineName, context.returnPool) as GameObject;
        if (timelineGo == null)
        {
            LoadMgr.singleton.Destroy(ref asset);
            XDebug.singleton.AddErrorLog("timeline is not exist:", timelineName);
            return;
        }
        Transform timelineRoot = timelineGo.transform.Find("timeline");
        PlayableDirector director = timelineRoot.GetComponent<PlayableDirector>();
        if (director == null)
        {
            LoadMgr.singleton.Destroy(ref asset);
            XDebug.singleton.AddErrorLog("timeline director is null:", timelineName);
            return;
        }
        if(context.m_isOverrideWrapMode)
        {
            director.extrapolationMode = (DirectorWrapMode)(context.m_directorWrapMode);
        }
        TimelinePrefabData timelineData = timelineRoot.GetComponent<TimelinePrefabData>();
        if (timelineData != null)
        {
            timelineData.asset = asset;
            timelineData.m_timelineName = context.name;
            timelineData.m_timelineContext = context;
            timelineData.m_playableDirector = director;
            timelineData.m_loadCb = cb;
            timelineData.m_selfShadowCb = selfShadowCb;
            timelineData.m_asyncRoles = new Dictionary<XGameObject, OrignalChar>();
            timelineData.m_fxGoPool = new AssetHandler[fx_max];
            timelineData.m_fxContextPool = new object[fx_max];

            m_timelinePrefabs.Add(timelineData);
            AnalyseTimeline(timelineData);
            LoadRoles(timelineData);
            LoadEffects(timelineData);
        }
    }

    public void UnloadTimelinePrefab(TimelineContext context)
    {
        for (int i = 0; i < m_timelinePrefabs.Count; ++i)
        {
            TimelinePrefabData timelinePrefabData = m_timelinePrefabs[i];
            if (timelinePrefabData != null && timelinePrefabData.m_timelineName.Equals(context.name))
            {
                RecycleSingleTimeline(timelinePrefabData);
                GameObject.Destroy(timelinePrefabData.gameObject);
                LoadMgr.singleton.Destroy(ref timelinePrefabData.asset);
                timelinePrefabData.asset = null;
                m_timelinePrefabs.Remove(timelinePrefabData);
                break;
            }
        }
    }

    private void AnalyseTimeline(TimelinePrefabData timelineData)
    {
        timelineData.m_roleTracks = new List<object>();
        timelineData.m_fxTracks = new List<object>();

        foreach (var pb in timelineData.m_playableDirector.playableAsset.outputs)
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
                    timelineData.m_roleTracks.Add(pb.sourceObject);
                    break;
                case ControlTrack _:
                    timelineData.m_fxTracks.Add(pb.sourceObject);
                    break;
            }
        }
    }

    private void LoadRoles(TimelinePrefabData timelineData)
    {
        if (timelineData.chars == null) return;
        int len = timelineData.chars.Length;
        for (int i = 0; i < len; i++)
        {
            var ch = timelineData.chars[i];
            if (ch.sync) LoadRole(ch, timelineData);
        }
    }

    private void LoadRole(OrignalChar ch, TimelinePrefabData timelineData)
    {
        if (ch.tracks != null && ch.xobj == null)
        {
            ref var cc = ref GameObjectCreateContext.createContext;
            cc.Reset();
            cc.name = ch.prefab;
            cc.flag.SetFlag(GameObjectCreateContext.Flag_SetPrefabName | GameObjectCreateContext.Flag_NotSyncPos);
            cc.renderLayerMask = (uint)ch.Layer;
            cc.immediate = true;
            cc.m_param = timelineData;
            cc.cb = LoadRoleCallback;
            var xgo = XGameObject.CreateXGameObject(ref cc,true);
            xgo.EndLoad(ref cc);
            PostLoadRole(xgo, ch, true);
        }
    }

    private bool UnloadRole(OrignalChar ch, int i, TimelinePrefabData timelineData)
    {
        if (ch.xobj != null)
        {
            UnloadLookAtInfo(ch);
            UnloadFacialAnimationClips(ch);
            UnloadSelfShadowComponent(ch);
            timelineData.m_loadCb?.Invoke(ch.xobj, -(i + 1));
            XGameObject.DestroyXGameObject(ch.xobj);
            ch.xobj = null;
            return true;
        }
        return false;
    }

    private void LoadEffects(TimelinePrefabData timelineData)
    {
        int j = 0;
        for (int i = 0; i < timelineData.m_fxTracks.Count; i++)
        {
            if (j > fx_max)
            {
                timelineData.m_maxFxCount = fx_max;
                XDebug.singleton.AddWarningLog("timeline fx overange, max: " + fx_max);
                break;
            }

            var track = timelineData.m_fxTracks[i];
            var clips = (track as ControlTrack).GetClips();
            foreach (var c in clips)
            {
                var asset = c.asset as ControlPlayableAsset;
                if (asset?.fxData != null)
                {
                    var data = asset.fxData;
                    if (!string.IsNullOrEmpty(data.path))
                    {
                        var obj = asset.sourceGameObject.Resolve(timelineData.m_playableDirector);
                        if (obj != null) continue;

                        var ctx = SharedObjectPool<FxContext>.Get();
                        timelineData.m_fxContextPool[j] = ctx;
                        ctx.data = data;
                        ctx.fx_indx = j++;
                        ctx.asset = asset;
                        ctx.sync = data.loadtime < sync_time;
                        if (ctx.sync) LoadEffect(ctx, timelineData);
                    }
                }
            }
        }
        timelineData.m_maxFxCount = (uint)j;
    }

    private void LoadEffect(FxContext ctx, TimelinePrefabData timelineData)
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
        var go = EngineUtility.LoadPrefab(pat, ref fxAsset,0,true,null,true);
        //var go = XResourceLoaderMgr.singleton.CreateFromAsset<GameObject>(pat, ".prefab");
        LoadEffectCallback(go, fxAsset, ctx, timelineData);
    }

    private void LoadEffectCallback(GameObject go, AssetHandler fxAsset, FxContext context, TimelinePrefabData timelineData)
    {
        if (go != null && context != null)
        {
            go.SetActive(false);
            ControlFxData data = context.data;

            if (!string.IsNullOrEmpty(data.avatar))
            {
                BindEffect2Role(data, go, timelineData);
            }
            go.transform.localPosition = data.pos;
            go.transform.localRotation = data.rot;
            go.transform.localScale = data.scale;

            ControlPlayableAsset asset = context.asset;
            asset?.Rebuild(go, timelineData.m_playableDirector);
            SharedObjectPool<FxContext>.Release(context);
            int idx = context.fx_indx;
            timelineData.m_fxGoPool[idx] = fxAsset;
            timelineData.m_fxContextPool[idx] = null;
        }
        else
        {
            LoadMgr.singleton.Destroy(ref fxAsset);
        }
    }

    private void BindEffect2Role(ControlFxData data, GameObject fx, TimelinePrefabData timelineData)
    {
        if (timelineData.chars != null)
        {
            int len = timelineData.chars.Length;
            for (int i = 0; i < len; i++)
            {
                var ch = timelineData.chars[i];
                if (ch?.prefab == data.avatar)
                {
                    if (ch.xobj != null)
                    {
                        var child = ch.xobj.Find(data.bonePath);
                        if (child)
                        {
                            fx.transform.parent = child;
                        }
                    }
                }
            }
        }
    }

    private bool LoadRoleCallback(XGameObject go)
    {
        TimelinePrefabData timelineData = go.m_param as TimelinePrefabData;
        if (timelineData.m_asyncRoles.ContainsKey(go))
        {
            PostLoadRole(go, timelineData.m_asyncRoles[go], false);
            timelineData.m_asyncRoles.Remove(go);
        }
        return true;
    }

    private void PostLoadRole(XGameObject xgo, OrignalChar ch, bool sync)
    {
        TimelinePrefabData timelineData = xgo.m_param as TimelinePrefabData;
        var tf = xgo.Find("");
        if (tf == null)
        {
            if (sync)
                timelineData.m_asyncRoles.Add(xgo, ch);
            else
                XDebug.singleton.AddErrorLog("timeline load char failed, check refrence assets and export: ", xgo.prefabName);
            return;
        }

        if (ch.root) tf.parent = ch.root;
        tf.localPosition = ch.pos;
        tf.localRotation = Quaternion.Euler(ch.rot);
        tf.localScale = ch.scale;
        xgo.Ator.cullingMode = AnimatorCullingMode.AlwaysAnimate; //ch.cull;
        for (int i = 0; i < ch.tracks.Length; i++)
        {
            if (i < ch.tracks.Length && ch.tracks[i] < timelineData.m_roleTracks.Count)
            {
                TrackAsset t = timelineData.m_roleTracks[ch.tracks[i]] as TrackAsset;
                timelineData.m_playableDirector.SetGenericBinding(t, tf.gameObject);
            }
        }
        int len = ch.parts?.Length ?? 0;
        for (int i = 0; i < len; i++)
        {
            var t = xgo.Find(ch.parts[i]);
            t?.gameObject.SetActive(false);
            if (t != null)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = t.gameObject.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null) skinnedMeshRenderer.enabled = false;
            }
        }

        len = ch.m_showParts?.Length ?? 0;
        for (int i = 0; i < len; i++)
        {
            var t = xgo.Find(ch.m_showParts[i]);
            t?.gameObject.SetActive(true);
            if (t != null)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = t.gameObject.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null) skinnedMeshRenderer.enabled = true;
            }
        }

        ch.xobj = xgo;
        int idx = Array.IndexOf(timelineData.chars, ch);
        timelineData.m_loadCb?.Invoke(xgo, idx);

        //if need look at
        InitLookAtInfo(tf, ch);

        //if need facial expression
        LoadFacialAnimationClips(tf, ch);

        //if need self shadow
        AddSelfShadowComponent(tf, ch, idx);
    }

    public void RecycleTimelines()
    {
        for (int i = 0; i < m_timelinePrefabs.Count; ++i)
        {
            RecycleSingleTimeline(m_timelinePrefabs[i]);
        }
        m_timelinePrefabs.Clear();
    }

    public void RecycleSingleTimeline(TimelinePrefabData timelineData)
    {
        timelineData.m_roleTracks.Clear();
        timelineData.m_fxTracks.Clear();
        timelineData.m_asyncRoles.Clear();

        if (timelineData?.chars != null)
        {
            for (int i = 0; i < timelineData.chars.Length; i++)
            {
                var ch = timelineData.chars[i];
                UnloadRole(ch, i, timelineData);
            }
        }


        for (int i = 0; i < timelineData.m_maxFxCount; i++)
        {
            var fxAsset = timelineData.m_fxGoPool[i];
            if (fxAsset != null)
            {
                LoadMgr.singleton.Destroy(ref fxAsset);
                //if (timelineData.m_timelineContext.returnPool)
                //    XResourceLoaderMgr.singleton.UnSafeDestroy(timelineData.m_fxGoPool[i]);
                //else
                //    XResourceLoaderMgr.singleton.UnSafeDestroy(timelineData.m_fxGoPool[i], false, true);
                timelineData.m_fxGoPool[i] = null;

            }
            if (timelineData.m_fxContextPool[i] != null)
            {
                SharedObjectPool<FxContext>.Release(timelineData.m_fxContextPool[i] as FxContext);
                timelineData.m_fxContextPool[i] = null;
            }
        }
    }


    public XRuntimeFmod GetFmod(PlayableDirector director)
    {
        if (director == null) return null;
        if (m_fmodDict.ContainsKey(director))
        {
            return m_fmodDict[director];
        }
        else
        {
            XRuntimeFmod fmod = new XRuntimeFmod(); //XRuntimeFmod.GetFMOD(); //因为不要停止口型和sfx的声音，所以这里就不还回池子了，否则即使自己不停止，当被别人用的时候，也会停止口型的声音
            m_fmodDict.Add(director, fmod);
            return fmod;
        }
    }

    public void ReturnFmod(PlayableDirector director)
    {
        if (m_fmodDict.ContainsKey(director))
        {
            m_fmodDict[director] = null; 
            m_fmodDict.Remove(director);
        }
    }

    public void StopFmod(PlayableDirector director, bool isJump)
    {
        if (m_fmodDict.ContainsKey(director))
        {
            //stop action and vocal channel, these two channels are used for tone.
            m_fmodDict[director].Stop(AudioChannel.Action);
            m_fmodDict[director].Stop(AudioChannel.Vocal); 
            m_fmodDict[director].Stop(AudioChannel.Skill);
            m_fmodDict[director].Stop(AudioChannel.Behit);
            if(isJump) //在跳过的模式下才去停止音效，否则自然播放结束
            {
                m_fmodDict[director].Stop(AudioChannel.SFX, RTimeline.singleton.m_playedEvents);
            }
            RTimeline.singleton.m_playedEvents.Clear();
        }
    }

    public bool IsEventPlaying(PlayableDirector director, AudioChannel channel)
    {
        if (m_fmodDict.ContainsKey(director))
        {
            return m_fmodDict[director].IsPlaying(channel);
        }
        return false;
    }
}