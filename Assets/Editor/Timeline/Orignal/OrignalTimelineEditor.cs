using CFEngine;
using CFEngine.Editor;
using CFUtilPoolLib;
using Cinemachine;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.CFUI;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace XEditor
{

    using CStyle = CinemachineBlendDefinition.Style;

    [CustomEditor(typeof(OrignalTimelineData))]
    public class OrignalTimelineEditor : Editor
    {
        public static OriginalSyncLoadEditor m_loader;
        public static bool m_loaded;
        public static OrignalTimelineData data;
        bool toolFolder;
        private bool cinemachineCamsFolder;
        PlayableAsset from, dest;

        GUIStyle boldLableStyle;

        GameObject edit_vc;
        CinemachineVirtualCameraBase fadeIn, fadeOut;

        static TimelineAvgConfig m_AvgConfig;

        private void OnEnable()
        {
            data = target as OrignalTimelineData;
            RTimeline.singleton.timeOut = InitLookAtAndFollow;

            //Init();
            if (!Application.isPlaying)
            {
                EngineContext.director = data.gameObject.GetComponent<PlayableDirector>();
                SetupLoader();
                RTimeline.singleton.Start(EngineContext.instance);
                RTimeline.singleton.InitCinemachineFollowAndLookAt(data);
            }

            if (!Application.isPlaying && !OrignalEntrainWindow.debugMode && !m_loaded)
            {
                GlobalContex.AddEngineEvent(OnEngineLoad, true);
                m_loaded = true;

                var env = GameObject.FindObjectOfType<EnvironmentExtra>();
                if (env) env.forceIgnore = true;

                EditorApplication.playModeStateChanged -= PlayModeChange;
                EditorApplication.playModeStateChanged += PlayModeChange;
            }

            EditorStateHelper.AddPlayListener(TimelinePlayModeChange);
        }

        public static void Init()
        {
            ShaderManager.Init(); //临时方案
            CFUICallbackRegister.UIDefault = ShaderManager.UIDefault;
            m_loaded = false;
            m_loader = null;
        }

        private void OnEngineLoad()
        {
            if (!Application.isPlaying)
            {
                if (m_loader != null)
                {
                    m_loader.LoadChars();
                    m_loader.LoadAllFx();
                    SFXSystem.InitHideObject();
                    HideStartPoint();
                }
            }

            RTimeline.orignalTimerStart = true;
        }

        private void HideStartPoint()
        {
            GameObject go = GameObject.Find("startPoint");
            if (go != null) go.SetActive(false);
        }

        private static void TimelinePlayModeChange(bool play)
        {
            if (!Application.isPlaying)
            {
                //if (play) CleanCanvasUI();
            }
        }

        public static void PlayModeChange(PlayModeStateChange state)
        {
            RTimeline.orignalTimerStart = true;

            var dir = GameObject.FindObjectOfType<PlayableDirector>();
            if (dir != null)
            {
                Selection.activeGameObject = dir.gameObject;
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    //CleanCanvasUI();
                }
            }
        }

        private static void CleanCanvasUI()
        {
            var can = TimelineUIMgr.canvas;
            if (can != null)
            {
                var root = can.transform;
                int cnt = root.childCount;

                List<GameObject> deleted = new List<GameObject>();
                for (int i = 0; i < cnt; i++)
                {
                    var child = root.GetChild(i);
                    Debug.Log(child.name);
                    if (child.name != "UICamera" && child.name != "CFEventSystem")
                    {
                        deleted.Add(child.gameObject);
                    }
                }
                TimelineUIMgr.ReturnAll();
                foreach (var it in deleted)
                {
                    if (!PrefabUtility.IsPartOfAnyPrefab(it))
                        GameObject.DestroyImmediate(it);
                }
            }
        }

        private void SetupLoader()
        {
            if (m_loader != null)
            {
                if (data.chars != null && m_loader.GetCharSize(out var size))
                {
                    if (data.chars.Length != size)
                    {
                        m_loader = null;
                    }
                }
            }    

            if(m_loader == null)
                m_loader = new OriginalSyncLoadEditor(data);
        }

        public override void OnInspectorGUI()
        {
            if (boldLableStyle == null)
            {
                boldLableStyle = new GUIStyle(EditorStyles.label);
                boldLableStyle.fontStyle = FontStyle.Bold;
                boldLableStyle.fontSize = 14;
            }

            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.indentLevel++;
                data.isBreak = GUILayout.Toggle(data.isBreak, "   可以跳过");
                data.m_useNewSkip = GUILayout.Toggle(data.m_useNewSkip, "   是否使用新的跳过");
                data.alwaysShowBreak = GUILayout.Toggle(data.alwaysShowBreak, "   老是显示跳过");

                data.showAutoButton = GUILayout.Toggle(data.showAutoButton, "   显示自动");
                //data.enableBGM = GUILayout.Toggle(data.enableBGM, "   开启音效");
                
                data.hideUI = GUILayout.Toggle(data.hideUI, "   隐藏游戏UI");
                data.clearEffectData = GUILayout.Toggle(data.clearEffectData, "   是否清理战场特效");
                
                data.shadowNotMove = GUILayout.Toggle(data.shadowNotMove, "  ShadowNotMove");
                data.m_hasNextTimeline = GUILayout.Toggle(data.m_hasNextTimeline, "  是否有下一个timeline");

                EditorGUI.indentLevel--;
                //data.endAudioParam = EditorGUILayout.IntField("结束时Fmod参数", data.endAudioParam);

                /*data.enableSetBgmVolume = GUILayout.Toggle(data.enableSetBgmVolume, "   设置BGM音量大小");
                if (data.enableSetBgmVolume)
                {
                    data.bgmMaxVolume = EditorGUILayout.FloatField("背景音音量", data.bgmMaxVolume);
                    data.bgmMaxVolume = Mathf.Clamp(data.bgmMaxVolume, 0, 1);
                }
                else
                {
                    data.bgmMaxVolume = 1;
                }*/

                GUILayout.BeginHorizontal();
                GUILayout.Label("hide layer");
                data.hidelayer = EditorGUILayout.MaskField(data.hidelayer, GameObjectLayerHelper.hideLayerStr);
                GUILayout.EndHorizontal();

                GuiVirutalCam();

                m_loader?.OnGUI();
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(data);
            }
            GUILayout.Space(4);
            toolFolder = EditorGUILayout.Foldout(toolFolder, "Tool", XEditorUtil.folderStyle);
            if (toolFolder)
            {
                GUILayout.Space(4);
                GUILayout.Label("1. Copy Rebind");
                EditorGUILayout.BeginVertical(EditorStyles.textField);
                {
                    from = (PlayableAsset)EditorGUILayout.ObjectField("from", from, typeof(PlayableAsset), false);
                    dest = (PlayableAsset)EditorGUILayout.ObjectField("dest", dest, typeof(PlayableAsset), false);
                    if (GUILayout.Button("Do it!")) CopyCheckRebind();
                    EditorGUILayout.EndVertical();
                }
                GUILayout.Space(4);
                GUILayout.Label("2. Find Missing Script");
                EditorGUILayout.HelpBox(OriginalSetting.findMissing, MessageType.Info);
                if (GUILayout.Button("Ouput Invalid"))
                {
                    var go = data.transform.parent.gameObject;
                    int i = 0;
                    XEditorUtil.RecersiceModifyGameObject(go, ref i);
                }
                GUILayout.Space(4);
                GUILayout.Label("3. Merge Virtual Camera");
                EditorGUILayout.HelpBox(OriginalSetting.mergeVc, MessageType.Info);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Merge Only")) Merge(false);
                if (GUILayout.Button("Merge & Delete")) Merge(true);
                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                GUILayout.Label("4. ControlTimelineSpeed");
                if (GUILayout.Button("CreateSpeedGo")) CreateSpeedGo();
            }

            GUILayout.Space(4);
            cinemachineCamsFolder = EditorGUILayout.Foldout(cinemachineCamsFolder, "CinemachineCams", XEditorUtil.folderStyle);
            if (cinemachineCamsFolder)
            {
                if (GUILayout.Button("LoadCinemachineLookatAndFollow"))
                {
                    RTimeline.singleton.InitCinemachineFollowAndLookAt(data);
                }

                if (RTimeline.singleton.Director != null)
                {                  
                   CinemachineTrack track = null;
                    foreach (var pb in RTimeline.singleton.Director.playableAsset.outputs)
                    {
                        if (pb.sourceObject is CinemachineTrack obj)
                        {
                            track = obj;
                        }
                    }

                    if(track != null)
                    {
                        var list = track.GetClipList();
                        for (int i = 0; i < list.Count; ++i)
                        {
                            var camAsset = list[i].asset as CinemachineShot;
                            string logStr = string.Empty;
                            if (camAsset.vcb != null)
                            {
                                if (!string.IsNullOrEmpty(data.cinemachineFollow[i]) || !string.IsNullOrEmpty(data.cinemachineLookAt[i]))
                                {
                                    GUILayout.Label(camAsset.vcb.name);
                                }
                                if (!string.IsNullOrEmpty(data.cinemachineFollow[i]))
                                {
                                    logStr = "    Follow: " + data.cinemachineFollow[i];
                                    GUILayout.Label(logStr);
                                    GUILayout.Space(4);
                                }
                                if (!string.IsNullOrEmpty(data.cinemachineLookAt[i]))
                                {
                                    logStr = "    LookAt: " + data.cinemachineLookAt[i];
                                    GUILayout.Label(logStr);
                                    GUILayout.Space(4);
                                }
                            }
                            GUILayout.Space(4);
                        }
                    }
                }
            }

            GUILayout.Space(8);

            //if (GUILayout.Button("AVG")) CreateAvgTracks();
            if (GUILayout.Button("Save Timeline", XEditorUtil.boldButtonStyle)) Save();
        }

        enum AvgTrackType
        {
            Pos,
            Animation,
            Expression,
            Dialog,
            Aduio,
            Active,
            BlendMask,//过渡
            Subtitle,//背景字幕
        }


        const float DialogLength = 1;
        const float DialogOffset = 0.5f;

        public static void CreateAvgTracks()
        {
            m_AvgConfig = new TimelineAvgConfig();
            if (XTableReader.ReadFile("Table/TimelineAvgConfig", m_AvgConfig))
            {
                TimelineAsset timelineAsset = RTimeline.singleton.Director.playableAsset as TimelineAsset;
                TrackAsset trackAsset = null;

                var tracks = timelineAsset.GetRootTracks();
                foreach (var track in tracks)
                {
                    if (track.name == "Lines[字幕]")
                    {
                        var chiledTracks = track.GetChildTracks();
                        foreach (var child in chiledTracks)
                        {
                            if (child is UIDialogTrack)
                            {
                                trackAsset = child;
                                break;
                            }
                        }
                        var clips = trackAsset.GetClipList();
                        ExternalManipulator.Delete(clips.ToArray());
                        CreateAvgTrack(AvgTrackType.Dialog, trackAsset);
                    }
                    else if (track.name == "Audio[音频]")
                    {
                        var chiledTracks = track.GetChildTracks();
                        foreach (var child in chiledTracks)
                        {
                            if (child is FmodPlayableTrack)
                            {
                                trackAsset = child;
                                break;
                            }
                        }
                        var clips = trackAsset.GetClipList();
                        ExternalManipulator.Delete(clips.ToArray());
                        CreateAvgTrack(AvgTrackType.Aduio, trackAsset);
                    }
                    else if (track.name == "UI[界面]")
                    {
                        var chiledTracks = track.GetChildTracks();
                        foreach (var child in chiledTracks)
                        {
                            if (child is UIPlayableTrack)
                            {
                                trackAsset = child;
                                break;
                            }
                        }
                        var clips = trackAsset.GetClipList();
                        ExternalManipulator.Delete(clips.ToArray());
                        CreateAvgTrack(AvgTrackType.BlendMask, trackAsset);
                    }                   
                }
            }

            RTimeline.singleton.avgWaitTime = 3.0f;
            RTimeline.singleton.initAvgBinding = InitAllTrackBinding;
            RTimeline.singleton.startAvgTimer = true;
        }

        private static List<int> facialIdx = new List<int>();
        private static void CreateAvgTrack(AvgTrackType type, TrackAsset trackAsset)
        {
            if (trackAsset == null)
                return;
            float startTime = 0.0f;
            facialIdx.Clear();

            switch (type)
            {
                case AvgTrackType.Dialog:
                    for (int i = 0; i < m_AvgConfig.Table.Length; i++)
                    {
                        var clip = ExternalManipulator.CreateClip(trackAsset, typeof(UIDialogAsset), 1, startTime);
                        clip.duration = DialogLength;
                        startTime += DialogLength + DialogOffset;
                        if (clip.asset != null)
                        {
                            var data = m_AvgConfig.Table[i];
                            UIDialogAsset aa = clip.asset as UIDialogAsset;
                            aa.content = data.Content;
                            aa.idx = (int)data.Index;
                            aa.speaker = data.Speaker;
                            aa.m_isPause = true;
                            clip.displayName = string.IsNullOrEmpty(aa.content) ? "<NULL>" : aa.content;
                        }
                    }

                    EditorUtility.SetDirty(trackAsset);
                    break;

                case AvgTrackType.Aduio:
                    for (int i = 0; i < m_AvgConfig.Table.Length; i++)
                    {
                        if (string.IsNullOrEmpty(m_AvgConfig.Table[i].Voices))
                        {
                            continue;
                        }

                        FmodPlayableTrack track = trackAsset as FmodPlayableTrack;
                        track.m_trackAssetType = TrackAssetType.Actor;
                        track.m_audioChannel = AudioChannel.Action;

                        var clip = ExternalManipulator.CreateClip(trackAsset, typeof(FmodPlayableAsset), 1, startTime);
                        clip.duration = DialogLength;
                        startTime += DialogLength + DialogOffset;
                        if (clip.asset != null)
                        {
                            var data = m_AvgConfig.Table[i];
                            FmodPlayableAsset aa = clip.asset as FmodPlayableAsset;
                            aa.clip = data.Voices;

                            for (int g = 0; g < RTimeline.singleton.CharactersGroups.Count; g++)
                            {
                                if(m_AvgConfig.Table[i].Speaker == RTimeline.singleton.CharactersGroups[g].name)
                                {
                                    facialIdx.Add(g);
                                }
                            }
                        }
                    }

                    EditorUtility.SetDirty(trackAsset);
                    break;

                case AvgTrackType.BlendMask:
                    for (int i = 0; i < m_AvgConfig.Table.Length; i++)
                    {
                        DramaTransition tansitionConfig = new DramaTransition();
                        var data = m_AvgConfig.Table[i];
                        if (XTableReader.ReadFile("Table/DramaTransition", tansitionConfig))
                        {
                            if (data.ScreenTransition[0] == 0)
                                continue;

                            float time = 0;
                            AssetHandler animHandle = null;
                            if(data.ScreenTransition[0] == 2) 
                            {
                                time = DialogLength;
                            }
                            var rowData = tansitionConfig.GetByID(data.ScreenTransition[0]);
                            string path = rowData.TimelineAnimName;

                            LoadMgr.GetAssetHandler(ref animHandle, path, ResObject.ResExt_Anim);
                            LoadMgr.loadContext.Init(null, null, LoadMgr.LoadForceImmediate | LoadMgr.UseFullPath);
                            LoadMgr.singleton.LoadAsset<AnimationClip>(animHandle, ResObject.ResExt_Anim, true);
                            var animationClip = animHandle.obj as AnimationClip;

                            var clip = ExternalManipulator.CreateClip(trackAsset, typeof(UIPlayerAsset), 1, startTime + time);
                            startTime += DialogLength + DialogOffset;
                            UIPlayerAsset aa = clip.asset as UIPlayerAsset;
                            aa.clip = animationClip;
                        }
                    }

                    EditorUtility.SetDirty(trackAsset);
                    break;              
            }
        }

        private static void CreateCharacterTrack(AvgTrackType type)
        {
            AVGRoles avgRoles = new AVGRoles();
            AVGAnimations anim = new AVGAnimations();
            AVGExpression animEx = new AVGExpression();

            XTableReader.ReadFile("Table/AVGRoles", avgRoles);
            XTableReader.ReadFile("Table/AVGAnimations", anim);
            XTableReader.ReadFile("Table/AVGExpression", animEx);

            switch (type)
            {               
                case AvgTrackType.Active:
                    float startTime = 0.0f;
                    for (int i = 0; i < m_AvgConfig.Table.Length; i++)
                    {
                        uint[] roles = m_AvgConfig.Table[i].RoleId;

                        for (int j = 0; j < roles.Length; j++)
                        {
                            var data = avgRoles.GetByPresentID(roles[j]);
                            if (data == null)
                                continue;

                            for (int g = 0; g < RTimeline.singleton.CharactersGroups.Count; g++)
                            {
                                if (data.Name == RTimeline.singleton.CharactersGroups[g].name)
                                {
                                    var chiledTracks = RTimeline.singleton.CharactersGroups[g].GetChildTracks();
                                    foreach (var child in chiledTracks)
                                    {
                                        if (child.name.Contains("显隐轨道"))
                                        {
                                            var clip = ExternalManipulator.CreateClip(child, typeof(ActivationPlayableAsset), DialogLength + 0.1f, startTime);
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        startTime += DialogLength + DialogOffset;
                    }
                    break;
                case AvgTrackType.Pos:
                    startTime = 0.0f;
                    Transform[] posTrans = new Transform[3];
                    posTrans[0] = data.transform.Find("TemplatePosition/Pos_cam/Pos_1");
                    posTrans[1] = data.transform.Find("TemplatePosition/Pos_cam/Pos_2");
                    posTrans[2] = data.transform.Find("TemplatePosition/Pos_cam/Pos_3");

                    for (int i = 0; i < m_AvgConfig.Table.Length; i++)
                    {
                        uint[] roles = m_AvgConfig.Table[i].RoleId;

                        for (int j = 0; j < roles.Length; j++)
                        {
                            var data = avgRoles.GetByPresentID(roles[j]);
                            if (data == null)
                                continue;
                            for (int g = 0; g < RTimeline.singleton.CharactersGroups.Count; g++)
                            {
                                if (data.Name == RTimeline.singleton.CharactersGroups[g].name)
                                {
                                    var chiledTracks = RTimeline.singleton.CharactersGroups[g].GetChildTracks();
                                    foreach (var child in chiledTracks)
                                    {
                                        if (child.name.Contains("位移轨道"))
                                        {
                                            int posIdx = j;
                                            var clip = ExternalManipulator.CreateClip(child, typeof(TransformTweenClip), DialogLength, startTime);
                                            TransformTweenClip asset = clip.asset as TransformTweenClip;

                                            asset.startLocation = new ExposedReference<Transform>();
                                            asset.endLocation = new ExposedReference<Transform>();

                                            asset.startLocation.exposedName = UnityEditor.GUID.Generate().ToString();
                                            asset.endLocation.exposedName = UnityEditor.GUID.Generate().ToString();

                                            RTimeline.singleton.Director.SetReferenceValue(asset.startLocation.exposedName, posTrans[posIdx]);
                                            RTimeline.singleton.Director.SetReferenceValue(asset.endLocation.exposedName, posTrans[posIdx]);
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        startTime += DialogLength + DialogOffset;
                    }
                    break;

                case AvgTrackType.Animation:
                    startTime = 0.0f;                   
                    for (int i = 0; i < m_AvgConfig.Table.Length; i++)
                    {
                        uint[] roles = m_AvgConfig.Table[i].RoleId;

                        for (int j = 0; j < roles.Length; j++)
                        {
                            var data = avgRoles.GetByPresentID(roles[j]);
                            if (data == null)
                                continue;

                            for (int g = 0; g < RTimeline.singleton.CharactersGroups.Count; g++)
                            {
                                if (data.Name == RTimeline.singleton.CharactersGroups[g].name)
                                {
                                    var chiledTracks = RTimeline.singleton.CharactersGroups[g].GetChildTracks();
                                    foreach (var child in chiledTracks)
                                    {
                                        if (child.name.Contains("动作轨道2"))
                                        {
                                            var roleAnims = m_AvgConfig.Table[i].RoleAnimation;
                                            if (roleAnims == null || roleAnims.Length == 0)
                                                continue;

                                            var rowData = anim.GetByID(roleAnims[j]);
                                            if (rowData == null)
                                                continue;

                                            string path = rowData.Path;

                                            var animationClip = GetAniClipByPath(path);
                                            if (animationClip != null)
                                            {
                                                var clip = ExternalManipulator.CreateClip(child, typeof(AnimationPlayableAsset), animationClip.length, startTime);
                                                AnimationPlayableAsset aa = clip.asset as AnimationPlayableAsset;
                                                aa.clip = animationClip;
                                                clip.displayName = animationClip.name;
                                            }
                                        }
                                        else if (child.name == "自定义轨道")
                                        {
                                            var customAnims1 = m_AvgConfig.Table[i].RoleCustomAnimation;
                                            if (customAnims1 == null || customAnims1.Length == 0)
                                                continue;

                                            var rowData = anim.GetByID(customAnims1[j]);
                                            if (rowData == null)
                                                continue;

                                            string path = rowData.Path;

                                            var animationClip = GetAniClipByPath(path);
                                            if(animationClip != null)
                                            {
                                                var clip = ExternalManipulator.CreateClip(child, typeof(CustomAnimationAsset), animationClip.length, startTime);
                                                CustomAnimationAsset aa = clip.asset as CustomAnimationAsset;
                                                aa.clip = animationClip;
                                                clip.displayName = animationClip.name;
                                            }
                                        }
                                        else if (child.name == "自定义轨道2")
                                        {
                                            var customAnims2 = m_AvgConfig.Table[i].RoleExpressionAnimation;

                                            if (customAnims2 == null || customAnims2.Length == 0)
                                                continue;

                                            var rowData = animEx.GetByID(customAnims2[j]);
                                            if (rowData == null)
                                                continue;

                                            string path = rowData.Path;

                                            var animationClip = GetAniClipByPath(path);
                                            if (animationClip != null)
                                            {
                                                var clip = ExternalManipulator.CreateClip(child, typeof(CustomAnimationAsset), animationClip.length, startTime);
                                                CustomAnimationAsset aa = clip.asset as CustomAnimationAsset;
                                                aa.clip = animationClip;
                                                clip.displayName = animationClip.name;
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        startTime += DialogLength + DialogOffset;
                    }
                    break;

                case AvgTrackType.Expression:
                    startTime = 0.0f;
                    for (int i = 0; i < m_AvgConfig.Table.Length; i++)
                    {
                        uint[] roles = m_AvgConfig.Table[i].RoleId;

                        for (int j = 0; j < roles.Length; j++)
                        {
                            var data = avgRoles.GetByPresentID(roles[j]);
                            if (data == null)
                                continue;

                            for (int g = 0; g < RTimeline.singleton.CharactersGroups.Count; g++)
                            {
                                if (data.Name == RTimeline.singleton.CharactersGroups[g].name)
                                {
                                    var chiledTracks = RTimeline.singleton.CharactersGroups[g].GetChildTracks();
                                    foreach (var child in chiledTracks)
                                    {
                                        if (child.name.Contains("表情轨道"))
                                        {
                                            var anims = m_AvgConfig.Table[i].RoleExpression;
                                            if (anims == null || anims.Length == 0)
                                                continue;

                                            var rowData = animEx.GetByID(anims[j]);

                                            if (rowData == null)
                                                continue;

                                            string path = rowData.Path;

                                            var animationClip = GetAniClipByPath(rowData.Path);
                                            if (animationClip != null)
                                            {
                                                var clip = ExternalManipulator.CreateClip(child, typeof(AnimationPlayableAsset), animationClip.length, startTime);
                                                AnimationPlayableAsset aa = clip.asset as AnimationPlayableAsset;
                                                aa.clip = animationClip;
                                                clip.displayName = animationClip.name;
                                            }
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        startTime += DialogLength + DialogOffset;
                    }
                    break;
            }
        }

        private static AnimationClip GetAniClipByPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            AssetHandler animHandle = null;
            LoadMgr.GetAssetHandler(ref animHandle, path, ResObject.ResExt_Anim);
            LoadMgr.loadContext.Init(null, null, LoadMgr.LoadForceImmediate | LoadMgr.UseFullPath);
            LoadMgr.singleton.LoadAsset<AnimationClip>(animHandle, ResObject.ResExt_Anim, true);
            var animationClip = animHandle.obj as AnimationClip;
            return animationClip;
        }

        private static void InitAllTrackBinding()
        {
            if (RTimeline.singleton.CharactersGroups == null)
                return;

            CinemachineVirtualCamera cam = data.transform.parent.Find("cineroot/CM vcam1").GetComponent<CinemachineVirtualCamera>();
            cam.m_Lens.FieldOfView = m_AvgConfig.Table[0].CamFov;

            Transform cameraTrans = data.transform.Find("TemplatePosition/Pos_cam");
            cameraTrans.localPosition = new Vector3(m_AvgConfig.Table[0].CameraPos[0], m_AvgConfig.Table[0].CameraPos[1], m_AvgConfig.Table[0].CameraPos[2]);
            cameraTrans.localEulerAngles = new Vector3(m_AvgConfig.Table[0].CameraRot[0], m_AvgConfig.Table[0].CameraRot[1], m_AvgConfig.Table[0].CameraRot[2]);

            Transform rolePartent = GameObject.Find("timeline_Role_Root").transform;
            for (int i = 0; i < rolePartent.childCount; i++)
            {
                var curve = rolePartent.GetChild(i).GetComponent<FacialExpression>();
                curve.activeFlush = true;
                if (curve != null)
                {
                    FacialExpressionInspector.LoadAllClip(curve);
                }
            }
            m_loader.SaveAllFacialClips();

            for (int i = 0; i < RTimeline.singleton.CharactersGroups.Count; i++)
            {
                var role = data.chars[i];
                var chiledTracks = RTimeline.singleton.CharactersGroups[i].GetChildTracks();
                foreach (var child in chiledTracks)
                {
                   if (child.name.Contains("表情轨道") || child.name.Contains("自定义轨道2"))
                    {
                        RTimeline.singleton.Director.SetGenericBinding(child, role.m_facialCurveGo);
                    }
                    else if (child.name.Contains("看向轨道"))
                    {
                        RTimeline.singleton.Director.SetGenericBinding(child, role.m_lookAtTargetGo);
                    }
                }
            }

            TimelineAsset timelineAsset = RTimeline.singleton.Director.playableAsset as TimelineAsset;

            var tracks = timelineAsset.GetRootTracks();
            foreach (var track in tracks)
            {
                if (track.name == "Camera[镜头]")
                {
                    var chiledTracks = track.GetChildTracks();
                    foreach (var child in chiledTracks)
                    {
                        if (child is TransformTweenTrack)
                        {
                            RTimeline.singleton.Director.SetGenericBinding(child, data.transform.parent.Find("cineroot/CM vcam1"));
                        }
                        else if (child is AnimationTrack)
                        {
                            if (child.name.Equals("Lighting Track"))
                            {
                                RTimeline.singleton.Director.SetGenericBinding(child, data.transform.parent.Find("timeline/volume").GetComponent<Animator>());
                            }
                            else
                            {
                                RTimeline.singleton.Director.SetGenericBinding(child, data.transform.parent.Find("timeline/DOF_AVG").GetComponent<Animator>());
                            }
                        }
                    }
                }
                else if (track.name == "Audio[音频]")
                {
                    var chiledTracks = track.GetChildTracks();
                    TrackAsset trackAsset = null;
                    foreach (var child in chiledTracks)
                    {
                        if (child is FmodPlayableTrack)
                        {
                            trackAsset = child;
                            break;
                        }
                    }
                    var clips = trackAsset.GetClipList();
                    for (int i = 0; i < facialIdx.Count; i++)
                    {
                        FmodPlayableAsset aa = clips[i].asset as FmodPlayableAsset;
                        var curve = rolePartent.GetChild(facialIdx[i]).GetComponent<FacialExpression>();
                        aa.facialCurve = curve.m_curve;
                        aa.curvePath = PlayableFmodEditor.GetGameObjectPath(curve.m_curve.transform, false);
                        aa.curvePath1 = PlayableFmodEditor.GetGameObjectPath(curve.m_curve.transform, true);
                    }
                }
            }

            CreateCharacterTrack(AvgTrackType.Active);
            CreateCharacterTrack(AvgTrackType.Pos);
            CreateCharacterTrack(AvgTrackType.Animation);
            CreateCharacterTrack(AvgTrackType.Expression);

            RTimeline.singleton.avgWaitTime = 1.0f;
            RTimeline.singleton.initAvgBinding = ()=> 
            {
                EditorUtility.SetDirty(data);
                m_loader?.OnSave();
                var go = data.transform.parent.gameObject;
                bool saveSucc;
                string save_pat = OriginalSetting.LIB + go.name + ".prefab";
                PrefabUtility.ApplyPrefabInstance(go, InteractionMode.UserAction);
                PrefabUtility.SaveAsPrefabAsset(go, save_pat, out saveSucc);

                if (saveSucc)
                {
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    EditorUtility.DisplayDialog("error", "Save timeline failed, check prefab first", "ok");
                }
            };
            RTimeline.singleton.startAvgTimer = true;
        }

        private void GuiVirutalCam()
        {
            GUILayout.Space(4);
            EditorGUILayout.BeginVertical(EditorStyles.textField);
            GUILayout.Label("Virtual Camera", boldLableStyle);
            GUILayout.Space(4);
            if (edit_vc == null) edit_vc = GameObject.Find(OriginalSetting.EditorVC);
            if (edit_vc)
            {
                EditorGUILayout.ObjectField("Instance", edit_vc, typeof(GameObject), true);
                var c1 = edit_vc.transform.Find("fadein");
                var c2 = edit_vc.transform.Find("fadeout");
                if (c1 != null)
                    fadeIn = c1.GetComponent<CinemachineVirtualCameraBase>();
                if (c2 != null)
                    fadeOut = c2.GetComponent<CinemachineVirtualCameraBase>();
                EditorGUILayout.Space();
            }

            data.mode2 = (FinishMode)EditorGUILayout.EnumPopup("start", data.mode2);
            if (data.mode2 == FinishMode.NORMAL)
            {
                EditorGUILayout.ObjectField("FadeIn", fadeIn, typeof(CinemachineVirtualCameraBase), true);
                var b1 = (CStyle)data.BlendInMode;
                b1 = (CStyle)EditorGUILayout.EnumPopup("blend", b1);
                data.BlendInMode = (int)b1;
                if (b1 != CStyle.Cut)
                    data.blendInTime = EditorGUILayout.FloatField("duration", data.blendInTime);
            }
            EditorGUILayout.Space();
            data.mode = (FinishMode)EditorGUILayout.EnumPopup("end", data.mode);
            if (data.mode == FinishMode.NORMAL)
            {
                EditorGUILayout.ObjectField("FadeOut", fadeOut, typeof(CinemachineVirtualCameraBase), true);
                var b2 = (CStyle)data.BlendOutMode;
                b2 = (CStyle)EditorGUILayout.EnumPopup("blend", b2);
                data.BlendOutMode = (int)b2;
                if (b2 != CStyle.Cut)
                    data.blendOutTime = EditorGUILayout.FloatField("duration", data.blendOutTime);
            }
            SyncCinemachine(data, fadeIn, fadeOut);
            EditorGUILayout.EndVertical();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnReload()
        {
            if (!Application.isPlaying)
            {
                OrignalTimelineData data = GameObject.FindObjectOfType<OrignalTimelineData>();
                var fade = GameObject.Find(OriginalSetting.EditorVC);

                if (data != null && fade != null)
                {
                    var fadeIn = fade.transform.GetChild(0).GetComponent<CinemachineVirtualCameraBase>();
                    var fadeOut = fade.transform.GetChild(1).GetComponent<CinemachineVirtualCameraBase>();
                    SyncCinemachine(data, fadeIn, fadeOut);
                }

            }
        }

        private static void SyncCinemachine(OrignalTimelineData data, CinemachineVirtualCameraBase fadeIn,
            CinemachineVirtualCameraBase fadeOut)
        {
            CinemachineMixer.GetMasterPlayableDirector = () => RTimeline.singleton.Director;
            CinemachineMixer.GetEditorVCInfo = (out CStyle style1,
                       out float t1,
                       out CinemachineVirtualCameraBase v1,
                       out CStyle style2,
                       out float t2,
                       out CinemachineVirtualCameraBase v2) =>
            {
                t1 = data.blendInTime;
                t2 = data.blendOutTime;
                style1 = (CStyle)data.BlendInMode;
                style2 = (CStyle)data.BlendOutMode;
                v1 = data.mode2 == (int)FinishMode.NORMAL ? fadeIn : null;
                v2 = data.mode == (int)FinishMode.NORMAL ? fadeOut : null;
                return true;
            };
        }


        private void Merge(bool delete)
        {
            var dir = RTimeline.singleton.Director;
            VirtualCameraTool.ProcessVCM(dir, delete);
            dir.RebuildGraph();
            EditorUtility.SetDirty(dir.playableAsset);
            var pat = AssetDatabase.GetAssetPath(dir.playableAsset);
            AssetDatabase.ImportAsset(pat);
            Selection.activeGameObject = null;
        }

        private void CreateSpeedGo()
        {
            var dir = GameObject.FindObjectOfType<PlayableDirector>();
            if(dir != null && dir.transform.parent != null)
            {
                TimelineSpeedComponent[] speedComps = dir.transform.parent.GetComponentsInChildren<TimelineSpeedComponent>();
                if (speedComps.Length > 0) return;
                GameObject go = new GameObject("TimelineSpeed");
                go.transform.SetParent(dir.transform.parent);
                go.AddComponent<TimelineSpeedComponent>();
                go.AddComponent<Animator>();
            }
        }


        private void Save()
        {
            SaveCinemachineFollowAndLookAt();

            EditorUtility.SetDirty(data);
            m_loader?.OnSave();
            var go = data.transform.parent.gameObject;
            string pat = AssetDatabase.GetAssetPath(go);
            bool saveSucc;
            string save_pat = OriginalSetting.LIB + go.name + ".prefab";
            PrefabUtility.ApplyPrefabInstance(go, InteractionMode.UserAction);
            PrefabUtility.SaveAsPrefabAsset(go, save_pat, out saveSucc);

            if (saveSucc)
            {
                AssetDatabase.SaveAssets();
            }
            else
            {
                EditorUtility.DisplayDialog("error", "Save timeline failed, check prefab first", "ok");
            }
        }


        private void CopyCheckRebind()
        {
            var _director = EngineContext.director;
            if (from && dest && _director)
            {
                _director.playableAsset = from;
                Queue<Object> stack = new Queue<Object>();
                foreach (PlayableBinding pb in _director.playableAsset.outputs)
                {
                    switch (pb.sourceObject)
                    {
                        case AnimationTrack _:
                            Debug.Log(pb.sourceObject + "  " + pb.streamName);
                            var atrack = pb.sourceObject as AnimationTrack;
                            var bind = _director.GetGenericBinding(pb.sourceObject);
                            stack.Enqueue(bind);
                            break;
                    }
                }
                _director.playableAsset = dest;
                foreach (PlayableBinding pb in _director.playableAsset.outputs)
                {
                    switch (pb.sourceObject)
                    {
                        case AnimationTrack _:
                            _director.SetGenericBinding(pb.sourceObject, stack.Dequeue());
                            break;
                    }
                }
            }
        }


        [MenuItem("XEditor/Timeline/OrginTimelineCheck _F3", priority = 3)]
        private static bool CheckAssetValid()
        {
            PlayableDirector[] director = GameObject.FindObjectsOfType<PlayableDirector>();

            if (Application.isPlaying) return false;

            bool error_occur = false;
            string title = "timeline error";

            if (director == null || director.Length == 0)
            {
                error_occur = true;
                EditorUtility.DisplayDialog(title, "not found director in scene", "OK");
            }
            else if (director.Length > 1)
            {
                error_occur = true;
                EditorUtility.DisplayDialog(title, "found multy director in scene", "OK");
            }
            else
            {
                var dir = director[0];
                if (dir.playableAsset == null)
                {
                    error_occur = true;
                    EditorUtility.DisplayDialog(title, "not found asset in director", "ok");
                }
                else if (!dir.playableAsset.name.StartsWith("Orignal_"))
                {
                    error_occur = true;
                    EditorUtility.DisplayDialog(title, "not match name regular", "ok");
                }
            }
            Camera camera = Camera.main;
            if (!error_occur)
            {
                if (camera == null)
                {
                    EditorUtility.DisplayDialog(title, "not found camera in scene", "ok");
                }
                else
                {
                    var env = camera.GetComponent<EnvironmentExtra>();
                    if (env == null)
                    {
                        EditorUtility.DisplayDialog(title, "not found EnvironmentExtra in camera", "ok");
                    }
                }
            }
            return true;
        }

        public void InitLookAtAndFollow(OrignalTimelineData tmp)
        {
            if(tmp == null)
            {
                RTimeline.singleton.InitCinemachineFollowAndLookAt(data);
            }
        }

        private void SaveCinemachineFollowAndLookAt()
        {
            cinemachineCamsFolder = true;
            CinemachineTrack track = null;
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

                data.cinemachineFollow = new string[list.Count];
                data.cinemachineLookAt = new string[list.Count];

                for (int i = 0; i < list.Count; ++i)
                {
                    var camAsset = list[i].asset as CinemachineShot;
                    if (camAsset.vcb != null)
                    {
                        if(camAsset.vcb.Follow != null)
                            data.cinemachineFollow[i] = GetPath(camAsset.vcb.Follow);

                        if (camAsset.vcb.LookAt != null)
                            data.cinemachineLookAt[i] = GetPath(camAsset.vcb.LookAt);
                    }
                }
            }
        }


        public static string GetPath(Transform trans)
        {
            if (trans == null)
            {
                UnityEngine.Debug.Log("对象为空" + trans.name);
            }
            StringBuilder tempPath = new StringBuilder(trans.name);
            Transform tempTra = trans;
            string g = "/";
            while (tempTra.parent != null)
            {
                if (tempTra.parent.name == "timeline_Role_Root")
                    break;
                tempTra = tempTra.parent;
                tempPath.Insert(0, tempTra.name + g);
            }
            return tempPath.ToString();
        }
    }
}