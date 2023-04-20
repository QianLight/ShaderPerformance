using CFEngine;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace XEditor
{
    public enum TrackType
    {
        Animation,
        Transform,
        CustomAnimation,
        Activation,
    }

    [System.Serializable]
    public class EditorOrigChar : OrignalChar
    {
        public UnityEngine.Object tf, obj;
        public bool folder;
        public bool m_facialFolder;
        public TrackAsset[] trackAssets;

        public bool InitFacialOncreat;
        public bool InitLookAtOncreat;
        public XEditor.TrackType m_selectTrackType;

        public GameObject m_root;

        public EditorOrigChar() { }

        public EditorOrigChar(OrignalChar ch)
        {
            this.folder = false;
            this.InitFacialOncreat = ch.m_facialCurveGo != null;
            this.InitLookAtOncreat = false;

            this.prefab = ch.prefab;
            this.comment = ch.comment;
            this.layer = ch.layer;
            this.cull = ch.cull;
            this.pos = ch.pos;
            this.rot = ch.rot;
            this.scale = ch.scale;
            this.root = ch.root;
            this.tracks = ch.tracks;
            this.xobj = ch.xobj;
            this.loadtime = ch.loadtime;
            this.unloadtime = ch.unloadtime;
            this.parts = ch.parts;
            this.m_showParts = ch.m_showParts;
            this.m_facialCurveGo = ch.m_facialCurveGo;
            this.m_facialClips = ch.m_facialClips;
            this.m_facialClipTypes = ch.m_facialClipTypes;
            this.m_lookAtTargetGo = ch.m_lookAtTargetGo;
            this.m_selfShadowGo = ch.m_selfShadowGo;
            this.m_selfShadow = ch.m_selfShadow;
            this.m_applyRootMotion = ch.m_applyRootMotion;
        }

        public void Save(List<TrackAsset> ch_tracks)
        {
            int len = trackAssets?.Length ?? 0;
            tracks = new int[len];
            for (int i = 0; i < len; i++)
            {
                tracks[i] = ch_tracks.IndexOf(trackAssets[i]);
            }
        }
    }

    public partial class OriginalSyncLoadEditor
    {
        private string proc_fx = string.Empty;
        private bool chFolder, fxFolder;
        private GameObject ch_root, fx_root;
        private int fx_statistic = 0;
        private EditorOrigChar[] m_char_data;
        private OrignalTimelineData m_orig_data;

        private List<TrackAsset> ch_tracks;
        private List<ControlTrack> fx_tracks;
        private HashSet<Transform> shadow_tfs = new HashSet<Transform>();
        private string[] ch_track_desc;

        private PlayableDirector director { get { return RTimeline.singleton.Director; } }

        static GUIStyle labelStyle;
        static readonly GUIContent newTrack = new GUIContent("New", "添加新的Track");
        static readonly GUIContent add = new GUIContent("Add", "添加数据Track");
        static readonly GUIContent bindIt = new GUIContent("Bind", "绑定Object到所有指定的Track");
        static readonly GUIContent pinIt = new GUIContent("Pin", "弹跳显示加载的对象");
        static readonly GUIContent syncIt = new GUIContent("Sync", "自动获取场景对象的位置和旋转");
        static readonly GUIContent root = new GUIContent("parent", "指定父节点(主要适用角色跟随相机一起动的情况)， 可以不指定");
        static readonly GUIContent initPos = new GUIContent("init pos", "动态加载初始位置");
        static readonly GUIContent initRot = new GUIContent("init rot", "动态加载初始旋转");
        static readonly GUIContent initScale = new GUIContent("init scale", "动态加载初始缩放");
        static readonly GUIContent layer = new GUIContent("layer", "用来指定渲染层级Layer");
        static readonly GUIContent culling = new GUIContent("Culling mode", "动画更新方式， 尽量不要选Always(比较耗)");
        static readonly GUIContent comment = new GUIContent("Comment", "备注");
        static readonly GUIContent prefab = new GUIContent("prefab", "直接拖拽工具导出的prefab到右侧，也可以手动输入名字");
        static readonly GUIContent initFacial = new GUIContent("Facial", "初始化表情相关组件");
        static readonly GUIContent initLookAt = new GUIContent("LookAt", "初始化看向相关组件");
        static readonly GUIContent initSelfShadow = new GUIContent("Shadow", "初始化自阴影组件");
        const int FACIAL_CLIP_COUNT = 5;

        private void AnalyDep()
        {
            if (director)
            {
                SetupRoot();
                ch_tracks.Clear();
                fx_tracks.Clear();
                foreach (var pb in director.playableAsset.outputs)
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
                ch_track_desc = ch_tracks.ConvertAll(x => x.GetStreamName()).ToArray();
            }
        }

        public OriginalSyncLoadEditor(OrignalTimelineData data)
        {
            TimelineGlobalConfig.Instance.ReadConfig();
            TimelineStateListener.Instance.ClearFmods();

            if (OrignalEntrainWindow.m_createNewTimeline)
            {
                int roleCount = OrignalEntrainWindow.m_selectedRoles.Count;
                if (data.chars == null)
                {
                    data.chars = new OrignalChar[roleCount];
                }
                for (int i = 0; i < roleCount; ++i)
                {
                    OrignalEntrainWindow.RoleInfo roelInfo = OrignalEntrainWindow.m_selectedRoles[i];
                    if (data.chars[i] == null)
                    {
                        data.chars[i] = new OrignalChar();
                    }
                    data.chars[i].prefab = roelInfo.m_prefabName;
                    //data.chars[i].tracks = new int[2];
                }
                OrignalEntrainWindow.m_createNewTimeline = false;
            }

            proc_fx = string.Empty;
            ch_tracks = new List<TrackAsset>();
            fx_tracks = new List<ControlTrack>();
            m_orig_data = data;

            int len = data?.chars?.Length ?? 0;
            m_char_data = new EditorOrigChar[len];
            for (int i = 0; i < len; i++)
            {
                m_char_data[i] = new EditorOrigChar(data.chars[i]);

                if (i < OrignalEntrainWindow.m_selectedRoles.Count)
                {
                    OrignalEntrainWindow.RoleInfo roelInfo = OrignalEntrainWindow.m_selectedRoles[i];
                    if (roelInfo.m_trackFlags[(int)OrignalEntrainWindow.TrackType.EAnimationExpression] ||
                        roelInfo.m_trackFlags[(int)OrignalEntrainWindow.TrackType.EAnimationEye] ||
                        roelInfo.m_trackFlags[(int)OrignalEntrainWindow.TrackType.EAnimationMouth])
                    {
                        m_char_data[i].InitFacialOncreat = true;
                    }

                    if (roelInfo.m_trackFlags[(int)OrignalEntrainWindow.TrackType.EAnimationLookAt])
                    {
                        m_char_data[i].InitLookAtOncreat = true;
                    }
                }
            }
            chFolder = true;
            fxFolder = true;
            AnalyDep();


            for (int i = 0; i < len; i++)
            {
                var ch = m_char_data[i];
                ch.trackAssets = new TrackAsset[ch.tracks.Length];
                if (data.chars[i].m_trackAssets != null)
                {
                    for (int j = 0; j < data.chars[i].m_trackAssets.Count; j++)
                    {
                        ch.trackAssets[j] = data.chars[i].m_trackAssets[j] as TrackAsset;
                        ch.tracks[j] = ch_tracks.IndexOf(ch.trackAssets[j]);
                    }
                }
                else
                {
                    for (int j = 0; j < ch.tracks.Length; j++)
                    {
                        int idx = ch.tracks[j];
                        if (ch_tracks.Count > idx && idx >= 0)
                        {
                            ch.trackAssets[j] = ch_tracks[idx];
                        }
                    }
                }
            }
        }

        public bool GetCharSize(out int size)
        {
            size = m_char_data?.Length ?? 0;
            return m_char_data != null;
        }

        private void SetupRoot()
        {
            ch_root = GameObject.Find(OriginalSetting.str_ch);
            if (ch_root == null) ch_root = new GameObject(OriginalSetting.str_ch);
            fx_root = GameObject.Find(OriginalSetting.str_fx);
            if (fx_root == null) fx_root = new GameObject(OriginalSetting.str_fx);
        }

        public void LoadChars()
        {
            SetupRoot();
            shadow_tfs.Clear();
            for (int i = 0; i < m_char_data.Length; ++i)
            {
                if (m_char_data[i] == null)
                    continue;

                LoadChar(m_char_data[i], i);

                if (m_char_data[i].InitFacialOncreat)
                {
                    this.InitFacial(m_char_data[i]);
                }

                if (m_char_data[i].InitLookAtOncreat)
                {
                    this.InitLookAt(m_char_data[i]);
                }

                OrignalTimelineEditor.data.chars[i].m_facialCurveGo = m_char_data[i].m_facialCurveGo;
                OrignalTimelineEditor.data.chars[i].m_lookAtTargetGo = m_char_data[i].m_lookAtTargetGo;
            }
        }

        public void OnGUI()
        {
            AnalyDep();
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.fontStyle = FontStyle.Bold;
            }
            var style = XEditorUtil.folderStyle;

            EditorGUILayout.Space();
            chFolder = EditorGUILayout.Foldout(chFolder, "Role", style);
            if (chFolder) { GuiChars(); SaveChars(); }

            GUILayout.Space(4);
            fxFolder = EditorGUILayout.Foldout(fxFolder, "SFX", style);
            if (fxFolder) GuiFxs();

            //不能直接覆盖，因为其他也会有地方会往里面加投影的对象，需要add进去
            foreach (var s in shadow_tfs)
                ShadowModify.extraShadowList.Add(s);
        }

        public void OnSave()
        {
            ProcessFx();
            SaveUnloadTime();
            if (fx_statistic > RTimeline.fx_max)
            {
                var mx = RTimeline.fx_max;
                var t = string.Format("fx count overange, current:{0} max:{1}", fx_statistic, mx);
                EditorUtility.DisplayDialog("warn", t, "ok");
            }
        }

        private void SaveUnloadTime()
        {
            for (int i = 0; i < m_char_data.Length; ++i)
            {
                if (m_char_data[i] == null)
                    continue;
                AnalyChLoadTime(m_char_data[i], i);
            }
        }


        public void SaveAllFacialClips()
        {
            for (int i = 0; i < m_char_data.Length; i++)
            {
                if(m_char_data[i].tf != null)
                {
                    var roleTrans = m_char_data[i].tf as Transform;
                    InitFacialClipsPath(m_char_data[i], roleTrans);
                }
            }

            SaveChars();
        }

        private void SaveChars()
        {
            if (!Application.isPlaying)
            {
                int len = m_char_data.Length;
                m_orig_data.chars = new OrignalChar[len];
                for (int i = 0; i < m_char_data.Length; i++)
                {
                    m_char_data[i].Save(ch_tracks);
                    m_orig_data.chars[i] = m_char_data[i];
                }
                var dir = RTimeline.singleton.Director;
                if (dir)
                {
                    var data = dir.GetComponent<OrignalTimelineData>();
                    if (data) data.chars = m_orig_data.chars;
                }
            }
        }

        private void GuiChars()
        {
            GUILayout.Space(4);
            for (int i = 0; i < m_char_data.Length; i++)
            {
                if (GuiChar(m_char_data[i], i)) break;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("AddRole"))
            {
                EditorOrigChar ch = new EditorOrigChar();
                XEditorUtil.Add(ref m_char_data, ch);
            }
            if (GUILayout.Button("LoadAll"))
            {
                LoadChars();
                RTimeline.singleton.ForceLoadCharRenderer();
            }
            GUILayout.EndHorizontal();
        }

        private bool GuiChar(EditorOrigChar ch, int i)
        {
            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            string name = string.IsNullOrEmpty(ch.comment) ? ch.prefab : ch.comment;

            if (string.IsNullOrEmpty(name)) { name = "Empty"; ch.folder = true; }
            EditorGUI.indentLevel++;
            ch.folder = EditorGUILayout.Foldout(ch.folder, " " + (i + 1) + ". " + name);
            EditorGUI.indentLevel--;
            if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
            {
                m_char_data = XEditorUtil.Remv(m_char_data, i);
                GUIUtility.ExitGUI();
                return true;
            }
            EditorGUILayout.EndHorizontal();

            if (name.Equals("Empty")) EditorGUILayout.HelpBox("Empty, not config", MessageType.Error);

            if (ch.folder)
            {
                EditorGUILayout.BeginVertical(EditorStyles.textField);

                EditorGUILayout.BeginHorizontal();
                ch.prefab = EditorGUILayout.TextField(prefab, ch.prefab);
                ch.obj = EditorGUILayout.ObjectField("", ch.obj, typeof(GameObject), true, GUILayout.MaxWidth(32));
                if (ch.obj != null)
                {
                    ch.prefab = ch.obj.name;
                    ch.obj = null;
                }
                EditorGUILayout.EndHorizontal();

                ch.comment = EditorGUILayout.TextField(comment, ch.comment);
                ch.cull = (AnimatorCullingMode)EditorGUILayout.EnumPopup(culling, ch.cull);
                ch.layer = EditorGUILayout.Popup(layer, ch.layer, GameObjectLayerHelper.showLayerStr);
                ch.pos = EditorGUILayout.Vector3Field(initPos, ch.pos);
                ch.rot = EditorGUILayout.Vector3Field(initRot, ch.rot);
                ch.scale = EditorGUILayout.Vector3Field(initScale, ch.scale);
                ch.root = (Transform)EditorGUILayout.ObjectField(root, ch.root, typeof(Transform), true);
                GUILayout.Label("load time:" + ch.loadtime.ToString("f1") + "   unload time: " + ch.unloadtime.ToString("f1"));

                ch.m_selfShadow = GUILayout.Toggle(ch.m_selfShadow, "self shadow");
                if (ch.m_selfShadow)
                {
                    ch.m_selfShadowGo = (GameObject)EditorGUILayout.ObjectField("selfShadowGo", ch.m_selfShadowGo, typeof(GameObject), true);
                }

                ch.m_applyRootMotion = GUILayout.Toggle(ch.m_applyRootMotion, "applyRootMotion");

                GuiHidePart(ch);
                DrawLookAtTarget(ch);
                DrawFacialClips(ch);
                GuiChBind(ch, i);
                EditorGUILayout.EndVertical();
            }
            return false;
        }

        private void DrawLookAtTarget(EditorOrigChar data)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            data.m_lookAtTargetGo = (GameObject)EditorGUILayout.ObjectField("lookAtGo", data.m_lookAtTargetGo, typeof(GameObject), true);
            EditorGUILayout.EndVertical();
        }

        private void DrawFacialClips(EditorOrigChar data)
        {
            EditorGUI.indentLevel++;
            data.m_facialFolder = EditorGUILayout.Foldout(data.m_facialFolder, "facialSettings");
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            if (data.m_facialFolder)
            {
                Transform roleTrans = null;
                if (data.tf is GameObject)
                {
                    roleTrans = (data.tf as GameObject).transform;
                }
                else
                {
                    roleTrans = (Transform)data.tf;
                }

                if (GUILayout.Button("SaveFacialClip", GUILayout.MaxWidth(100)))
                {
                    InitFacialClipsPath(data, roleTrans);
                }
                data.tf = EditorGUILayout.ObjectField("roleGo", roleTrans, typeof(GameObject), true);
                data.m_facialCurveGo = (GameObject)EditorGUILayout.ObjectField("curveGo", data.m_facialCurveGo, typeof(GameObject), true);

                if (data.m_facialClips != null)
                {
                    for (int i = 0; i < data.m_facialClips.Length; ++i)
                    {
                        if (string.IsNullOrEmpty(data.m_facialClips[i]))
                        {
                            data.m_facialClips[i] = string.Empty;
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (i < data.m_facialClipTypes.Length)
                        {
                            EditorGUILayout.LabelField(((FacialClipType)data.m_facialClipTypes[i]).ToString(), GUILayout.Width(100));
                        }
                        else
                        {
                            EditorGUILayout.LabelField(string.Empty);
                        }
                        /*data.m_facialClips[i] = */
                        EditorGUILayout.LabelField(data.m_facialClips[i]);
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        private void InitFacialClipsPath(EditorOrigChar data, Transform roleTrans)
        {
            if (roleTrans != null)
            {
                FacialExpression facialExpression = roleTrans.gameObject.GetComponent<FacialExpression>();
                List<FacialAnimationClip> clips = facialExpression.m_clips;
                data.m_facialClips = new string[facialExpression.m_clips.Count];
                data.m_facialClipTypes = new int[facialExpression.m_clips.Count];
                for (int i = 0; i < clips.Count; ++i)
                {
                    string path = string.Empty;
                    if (clips[i].m_clip != null)
                        path = AssetDatabase.GetAssetPath(clips[i].m_clip);
                    path = path.Replace("Assets/BundleRes/", string.Empty);
                    int index = path.IndexOf('.');
                    if (index > 0)
                    {
                        path = path.Substring(0, index);
                        data.m_facialClips[i] = path;
                    }
                    data.m_facialClipTypes[i] = (int)clips[i].m_clipType;
                }
            }
        }


        private void GuiHidePart(EditorOrigChar ch)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("hide part", labelStyle);
            int size = ch.parts?.Length ?? 0;
            size = EditorGUILayout.IntField("size", size);
            List<string> parts = new List<string>();
            if (ch.parts != null) parts.AddRange(ch.parts);
            if (parts.Count < size)
                for (int i = parts.Count; i < size; i++) parts.Add(string.Empty);
            else if (parts.Count > size)
                parts.RemoveRange(size, parts.Count - size);
            for (int i = 0; i < size; i++)
            {
                parts[i] = EditorGUILayout.TextField("item" + i, parts[i]);
            }
            ch.parts = parts.ToArray();


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("show part", labelStyle);
            size = ch.m_showParts?.Length ?? 0;
            size = EditorGUILayout.IntField("size", size);
            List<string> showParts = new List<string>();
            if (ch.m_showParts != null) showParts.AddRange(ch.m_showParts);
            if (showParts.Count < size)
                for (int i = showParts.Count; i < size; i++) showParts.Add(string.Empty);
            else if (showParts.Count > size)
                showParts.RemoveRange(size, showParts.Count - size);
            for (int i = 0; i < size; i++)
            {
                showParts[i] = EditorGUILayout.TextField("item" + i, showParts[i]);
            }
            ch.m_showParts = showParts.ToArray();
            EditorGUILayout.EndVertical();
        }

        private void Bind(EditorOrigChar ch, int chTrackIndex, int trackIndex)
        {
            TrackAsset targetTrack = ch_tracks[trackIndex];
            ch.trackAssets[chTrackIndex] = targetTrack;

            for (int i = 0; i < m_char_data.Length; i++)
            {
                if (ch == m_char_data[i]) continue;

                for (int j = 0; j < m_char_data[i].trackAssets.Length; ++j)
                {
                    if (targetTrack == m_char_data[i].trackAssets[j])
                    {
                        m_char_data[i].trackAssets = XEditorUtil.Remv(m_char_data[i].trackAssets, j);
                        break;
                    }
                }
            }
        }

        private void GuiChBind(EditorOrigChar ch, int i)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("binding track", labelStyle);

            int cnt = ch.trackAssets?.Length ?? 0;
            for (int j = 0; j < cnt; j++)
            {
                EditorGUILayout.BeginHorizontal();
                if (ch.trackAssets[j] == null) // delete by designer or arter in graph 
                {
                    ch.trackAssets = XEditorUtil.Remv(ch.trackAssets, j);
                    break;
                }
                int k = ch_tracks.IndexOf(ch.trackAssets[j]);

                k = EditorGUILayout.Popup(" track" + j, k, ch_track_desc);
                ch.trackAssets[j] = ch_tracks[k];
                //EditorGUILayout.ObjectField(ch.trackAssets[j], typeof(TrackAsset));
                ch.trackAssets[j].m_trackAssetType = (TrackAssetType)EditorGUILayout.EnumPopup(ch.trackAssets[j].m_trackAssetType, GUILayout.Width(100));
                if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                {
                    ch.trackAssets = XEditorUtil.Remv(ch.trackAssets, j);
                    GUIUtility.ExitGUI();

                    //if (j >= 0 && j < ch.trackAssets.Length)
                    //{
                    //    TrackAsset trackAsset = ch.trackAssets[j];
                    //    if (trackAsset != null)
                    //    {
                    //        PlayableDirector playableDirector = RTimeline.singleton.Director;
                    //        if (playableDirector != null)
                    //        {
                    //            TimelineAsset asset = playableDirector.playableAsset as TimelineAsset;
                    //            asset.DeleteTrack(trackAsset);
                    //            ch_tracks.Remove(trackAsset);
                    //            TimelineStateListener.Instance.RefreshTimelineWindow();
                    //        }
                    //        ch.trackAssets = XEditorUtil.Remv(ch.trackAssets, j);
                    //        GUIUtility.ExitGUI();
                    //    }
                    //}
                }
                EditorGUILayout.EndHorizontal();
            }

            if (cnt <= 0) EditorGUILayout.HelpBox("Empty, you need add track to Bind", MessageType.Warning);

            ch.m_selectTrackType = (XEditor.TrackType)EditorGUILayout.EnumPopup(ch.m_selectTrackType, GUILayout.Width(100));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            /*if (GUILayout.Button("resetFacial", GUILayout.MaxWidth(70)))
            {
                InitFacial(ch);
            }*/
            if (GUILayout.Button(initSelfShadow, GUILayout.MaxWidth(70)))
            {
                InitSelfShadow(ch);
            }
            if (GUILayout.Button(initLookAt, GUILayout.MaxWidth(70)))
            {
                InitLookAt(ch);
            }
            if (GUILayout.Button(initFacial, GUILayout.MaxWidth(70)))
            {
                InitFacial(ch);
            }
            if (GUILayout.Button(newTrack, GUILayout.MaxWidth(60)))
            {
                //if (ch_tracks.Count > 0)
                {
                    //XEditorUtil.Add(ref ch.trackAssets, ch_tracks[0]);
                    TrackAsset trackAsset = null;
                    GroupTrack group = null;
                    if (ch.trackAssets.Length > 0) group = ch.trackAssets[0].GetGroup();
                    PlayableDirector playableDirector = RTimeline.singleton.Director;
                    if (playableDirector != null)
                    {
                        TimelineAsset asset = playableDirector.playableAsset as TimelineAsset;
                        switch (ch.m_selectTrackType)
                        {
                            case TrackType.Animation:
                                trackAsset = asset.CreateTrack(typeof(AnimationTrack), group, ch.m_selectTrackType.ToString());
                                break;
                            case TrackType.Transform:
                                trackAsset = asset.CreateTrack(typeof(TransformTweenTrack), group, ch.m_selectTrackType.ToString());
                                break;
                            case TrackType.CustomAnimation:
                                trackAsset = asset.CreateTrack(typeof(CustomAnimationTrack), group, ch.m_selectTrackType.ToString());
                                break;
                            case TrackType.Activation:
                                trackAsset = asset.CreateTrack(typeof(ActivationTrack), group, ch.m_selectTrackType.ToString());
                                break;
                        }
                        if (trackAsset != null)
                        {
                            TimelineStateListener.Instance.RefreshTimelineWindow();
                            ch_tracks.Add(trackAsset);
                            XEditorUtil.Add(ref ch.trackAssets, trackAsset);
                        }
                    }
                }
                //else
                //{
                //    EditorUtility.DisplayDialog("tip", "create animation track in graph first please!", "ok");
                //}
            }
            if (GUILayout.Button(add, GUILayout.MaxWidth(60)))
            {
                if (ch_tracks.Count > 0)
                    XEditorUtil.Add(ref ch.trackAssets, ch_tracks[0]);
                else
                    EditorUtility.DisplayDialog("tip", "create animation track in graph first please!", "ok");
            }
            if (GUILayout.Button(bindIt, GUILayout.MaxWidth(60)))
            {
                SetupRoot();
                LoadChar(ch, i, true);
            }
            if (GUILayout.Button(pinIt, GUILayout.MaxWidth(60)))
            {
                EditorGUIUtility.PingObject(ch.tf);
            }
            if (GUILayout.Button(syncIt, GUILayout.MaxWidth(60)))
            {
                if (ch.tf != null && ch.tf is Transform)
                {
                    var tf = ch.tf as Transform;
                    ch.pos = tf.localPosition;
                    ch.rot = tf.localEulerAngles;
                    ch.scale = tf.localScale;
                }
                else
                {
                    EditorUtility.DisplayDialog("tip", "load first please!", "ok");
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void InitSelfShadow(EditorOrigChar data)
        {
            if (!data.m_selfShadow) data.m_selfShadow = true;
            Transform timelineRoot = m_orig_data.transform.parent;
            if (data.m_selfShadowGo == null)
            {
                InitCustomRoot(data, timelineRoot);
                data.m_selfShadowGo = new GameObject(data.prefab + "_shadow");
                data.m_selfShadowGo.transform.SetParent(data.m_root.transform);
            }
            Animator animator = data.m_selfShadowGo.GetComponent<Animator>();
            if (animator == null) animator = data.m_selfShadowGo.AddComponent<Animator>();
            DynamicSelfShadowCurve cuveComp = data.m_selfShadowGo.GetComponent<DynamicSelfShadowCurve>();
            if (cuveComp == null) cuveComp = data.m_selfShadowGo.AddComponent<DynamicSelfShadowCurve>();
            if (data.tf != null)
            {
                Transform roleTrans = null;
                if (data.tf is GameObject)
                {
                    roleTrans = (data.tf as GameObject).transform;
                }
                else
                {
                    roleTrans = (Transform)data.tf;
                }
                DynamicSelfShadow selfShadow = roleTrans.GetComponent<DynamicSelfShadow>();
                if (selfShadow == null) selfShadow = roleTrans.gameObject.AddComponent<DynamicSelfShadow>();
                selfShadow.m_curve = cuveComp;
            }
        }

        private void InitLookAt(EditorOrigChar data)
        {
            Transform roleTrans = null;
            if (data.tf is GameObject)
            {
                roleTrans = (data.tf as GameObject).transform;
            }
            else
            {
                roleTrans = (Transform)data.tf;
            }
            LookAt lookAtComp = roleTrans.GetComponent<LookAt>();
            if (lookAtComp == null) lookAtComp = roleTrans.gameObject.AddComponent<LookAt>();
            lookAtComp.m_self = roleTrans;
            Transform head = roleTrans.Find(RTimeline.HEAD_BONE_PATH);
            lookAtComp.m_head = head;

            Transform timelineRoot = m_orig_data.transform.parent;
            if (data.m_lookAtTargetGo == null)
            {
                InitCustomRoot(data, timelineRoot);
                data.m_lookAtTargetGo = new GameObject(data.prefab + "_lookAt");
                data.m_lookAtTargetGo.AddComponent<Animator>();
                data.m_lookAtTargetGo.transform.SetParent(data.m_root.transform);
                lookAtComp.m_lookTarget = data.m_lookAtTargetGo.transform;
            }
        }


        private void InitFacial(EditorOrigChar data)
        {
            Transform timelineRoot = m_orig_data.transform.parent;
            if (data.m_facialCurveGo == null)
            {
                InitCustomRoot(data, timelineRoot);
                data.m_facialCurveGo = new GameObject(data.prefab + "_facial");
                data.m_facialCurveGo.transform.SetParent(data.m_root.transform);
            }
            Animator animator = data.m_facialCurveGo.GetComponent<Animator>();
            if (animator == null) animator = data.m_facialCurveGo.AddComponent<Animator>();

            if (animator.runtimeAnimatorController == null)
            {
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Controller/XAnimator.controller", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
            }

            FacialExpressionCurve cuveComp = data.m_facialCurveGo.GetComponent<FacialExpressionCurve>();
            if (cuveComp == null) cuveComp = data.m_facialCurveGo.AddComponent<FacialExpressionCurve>();
            if (data.tf != null)
            {
                Transform roleTrans = null;
                if (data.tf is GameObject)
                {
                    roleTrans = (data.tf as GameObject).transform;
                }
                else
                {
                    roleTrans = (Transform)data.tf;
                }
                FacialExpression facialExpression = roleTrans.GetComponent<FacialExpression>();
                if (facialExpression == null) facialExpression = roleTrans.gameObject.AddComponent<FacialExpression>();
                facialExpression.m_curve = cuveComp;
                //facialExpression.m_clips = new FacialAnimationClip[FACIAL_CLIP_COUNT];
                //for temp
                //string[] clips = new string[] {
                //    "Assets/BundleRes/Animation/TestFacial/idle",
                //    "Assets/BundleRes/Animation/TestFacial/eye0",
                //    "Assets/BundleRes/Animation/TestFacial/eye1",
                //    "Assets/BundleRes/Animation/TestFacial/mouth0",
                //    "Assets/BundleRes/Animation/TestFacial/mouth1",
                //};
                //for (int i = 0; i < FACIAL_CLIP_COUNT; ++i)
                //{
                //    facialExpression.m_clips[i] = new FacialAnimationClip();
                //    facialExpression.m_clips[i].m_clipType = (FacialClipType)(i);
                //    facialExpression.m_clips[i].m_clip = LoadAnimationClip(clips[i]);
                //}
            }
        }

        private void InitCustomRoot(EditorOrigChar data, Transform timelineRoot)
        {
            string rootName = data.prefab + "_custom";
            Transform root = timelineRoot.Find(rootName);
            if (root != null)
            {
                data.m_root = root.gameObject;
            }
            if (data.m_root == null)
            {
                data.m_root = new GameObject(rootName);
                data.m_root.transform.SetParent(timelineRoot);
            }
        }

        private void LoadChar(EditorOrigChar ch, int index, bool RemoveOtherTrack = false)
        {
            if (ch.trackAssets != null)
            {
                Transform tf = null;
                if (ch.tf != null)
                {
                    tf = ch.tf as Transform;
                }
                else
                {
                    ref var cc = ref GameObjectCreateContext.createContext;
                    cc.Reset();
                    cc.name = ch.prefab;

                    cc.renderLayerMask = (uint)ch.Layer;
                    cc.flag.SetFlag(GameObjectCreateContext.Flag_SetPrefabName);
                    cc.immediate = true;

                    var xgo = XGameObject.CreateXGameObject(ref cc, true);
                    xgo.EndLoad(ref cc);
                    tf = xgo.Find("");
                    if (tf == null)
                    {
                        Debug.LogError("load char error: " + ch.prefab);
                        return;
                    }
                    xgo.Ator.cullingMode = ch.cull;
                }

                if(tf != null)
                {
                    //改层for_liufanyu
                    Renderer[] children = tf.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < children.Length; ++i)
                    {
                        children[i].gameObject.layer = LayerMask.NameToLayer("Role");
                    }
                }

                tf.parent = ch.root ? ch.root : ch_root.transform;

                tf.localPosition = ch.pos;
                tf.localRotation = Quaternion.Euler(ch.rot);
                tf.localScale = ch.scale;
                var actor = tf.gameObject.GetComponent<Animator>();
                if (actor != null) actor.applyRootMotion = ch.m_applyRootMotion;
                if (ch.parts != null)
                {
                    for (int i = 0; i < ch.parts.Length; i++)
                    {
                        var child = tf.Find(ch.parts[i]);
                        child?.gameObject.SetActive(false);
                        if (child != null)
                        {
                            Renderer renderer = child.GetComponent<Renderer>();
                            if (renderer != null) renderer.enabled = false;
                        }
                    }
                }

                if (ch.m_showParts != null)
                {
                    for (int i = 0; i < ch.m_showParts.Length; i++)
                    {
                        var child = tf.Find(ch.m_showParts[i]);
                        child?.gameObject.SetActive(true);
                        if (child != null)
                        {
                            Renderer renderer = child.GetComponent<Renderer>();
                            if (renderer != null) renderer.enabled = true;
                        }
                    }
                }

                for (int i = 0; i < ch.tracks.Length; i++)
                {
                    var t = ch.trackAssets[i];
                    if (t == null)
                        continue;

                    if (RemoveOtherTrack)
                    {
                        for (int k = 0; k < m_char_data.Length; k++)
                        {
                            if (ch == m_char_data[k]) continue;

                            if (m_char_data[k].trackAssets != null)
                            {
                                for (int j = 0; j < m_char_data[k].trackAssets.Length; ++j)
                                {
                                    if (t == m_char_data[k].trackAssets[j])
                                    {
                                        m_char_data[k].trackAssets = XEditorUtil.Remv(m_char_data[k].trackAssets, j);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //var obj = director.GetGenericBinding(t);          
                    GameObject go = tf.gameObject;                           //如果轨道类型为角色，则使用tf.gameObject进行绑定
                    if (t.m_trackAssetType == TrackAssetType.Facial)         //如果轨道类型为表情，则使用ch.m_facialCurveGo进行绑定
                    {
                        go = ch.m_facialCurveGo;
                    }
                    else if (t.m_trackAssetType == TrackAssetType.LookAt)    //如果轨道类型为看向，则使用ch.m_lookAtTargetGo进行绑定
                    {
                        go = ch.m_lookAtTargetGo;
                    }
                    director.SetGenericBinding(t, go);
                }
                ch.tf = tf;
                shadow_tfs.Add(tf);

                BindLookAt(tf, ch);
                BindFacialAnimationClip(tf, ch);
                BindSelfShadow(tf, ch);
                TimelinePrefabInspector comp = tf.gameObject.GetComponent<TimelinePrefabInspector>();
                if (comp == null) comp = tf.gameObject.AddComponent<TimelinePrefabInspector>();
                comp.m_name = ch.prefab + "_" + (index + 1);
            }
            AnalyChLoadTime(ch, index);
        }

        private void BindSelfShadow(Transform roleTransform, EditorOrigChar data)
        {
            DynamicSelfShadow comp = roleTransform.GetComponent<DynamicSelfShadow>();
            if (data.m_selfShadow)
            {
                if (comp == null) comp = roleTransform.gameObject.AddComponent<DynamicSelfShadow>();
                if (data.m_selfShadowGo != null)
                {
                    DynamicSelfShadowCurve curve = data.m_selfShadowGo.GetComponent<DynamicSelfShadowCurve>();
                    comp.m_curve = curve;
                }
            }
        }

        private void BindLookAt(Transform roleTransform, EditorOrigChar data)
        {
            if (data.m_lookAtTargetGo != null)
            {
                LookAt comp = roleTransform.GetComponent<LookAt>();
                if (comp == null) comp = roleTransform.gameObject.AddComponent<LookAt>();
                comp.m_self = roleTransform;
                Transform head = roleTransform.Find(RTimeline.HEAD_BONE_PATH);
                comp.m_head = head;
                comp.m_lookTarget = data.m_lookAtTargetGo.transform;
            }
        }


        private void BindFacialAnimationClip(Transform roleTransform, EditorOrigChar data)
        {
            if (data.m_facialClips != null && data.m_facialClips.Length > 0)
            {
                int len = data.m_facialClips.Length;
                FacialExpression comp = roleTransform.GetComponent<FacialExpression>();
                if (comp == null) comp = roleTransform.gameObject.AddComponent<FacialExpression>();
                if (data.m_facialCurveGo != null)
                {
                    FacialExpressionCurve curveComp = data.m_facialCurveGo.GetComponent<FacialExpressionCurve>();
                    comp.m_curve = curveComp;
                    comp.m_clips = new List<FacialAnimationClip>();

                    for (int i = 0; i < len; ++i)
                    {
                        if (!string.IsNullOrEmpty(data.m_facialClips[i]))
                        {
                            // load animation clip
                            AnimationClip animationClip = LoadAnimationClip("Assets/BundleRes/" + data.m_facialClips[i]);
                            FacialAnimationClip clip = new FacialAnimationClip();
                            FacialClipType facialClipType = FacialClipType.idle;
                            if (data.m_facialClipTypes != null && i < data.m_facialClipTypes.Length)
                            {
                                facialClipType = (FacialClipType)data.m_facialClipTypes[i];
                            }
                            clip.m_clipType = facialClipType;
                            clip.m_clip = animationClip;
                            comp.m_clips.Add(clip);
                        }
                    }
                    comp.Init();
                }
            }
        }

        private AnimationClip LoadAnimationClip(string path)
        {
            AnimationClip animationClip = null;
            AssetHandler animHandle = null;
            LoadMgr.GetAssetHandler(ref animHandle, path, ResObject.ResExt_Anim);
            LoadMgr.loadContext.Init(null, null, LoadMgr.LoadForceImmediate | LoadMgr.UseFullPath);
            LoadMgr.singleton.LoadAsset<AnimationClip>(animHandle, ResObject.ResExt_Anim);
            animationClip = animHandle.obj as AnimationClip;
            return animationClip;
        }

        /// <summary>
        /// 这个函数的意思，是分析某个角色的加载和卸载时间，遍历所有和角色相关的动画片段，找到最早开始的start和最晚结束的end，作为加载和卸载的时机
        /// 但这个函数有隐含的潜规则，如果一个动画片段很短，但是是loop或hold模式，则在此动画片段之后的一段时间，会自动销毁角色，这有时候会造成角色丢失的现象
        /// 此时有两个解决方法：
        /// 1.延迟动画片段，改资源的方式
        /// 2.在代码AnalyChLoadTime中动画片段是否为hold和loop模式，如果是，则将其销毁时间变为和timline的总时长一致。
        /// 3.在有blendshap的时候，如果角色被提前回收，mesh为null，此时会报错，而且没有堆栈信息，原因是mesh找不到了。解决方法，重新保存timline，使用2的规则。
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="index"></param>
        private void AnalyChLoadTime(EditorOrigChar ch, int index)
        {
            float t_1 = float.MaxValue, t_2 = 0;
            for (int i = 0; i < ch.tracks.Length; i++)
            {
                var t = ch.trackAssets[i];
                var clips = t?.GetClips();
                if (clips != null)
                    foreach (var clip in clips)
                    {
                        var t1 = clip.start;
                        var t2 = clip.end;
                        if (clip.postExtrapolationMode == TimelineClip.ClipExtrapolation.Hold || clip.postExtrapolationMode == TimelineClip.ClipExtrapolation.Loop)
                        {
                            t2 = (director.duration);
                        }
                        if (t1 < t_1) t_1 = (float)t1;
                        if (t2 > t_2) t_2 = (float)t2;
                    }
            }
            if (t_1 == float.MaxValue) t_1 = 0;
            else t_1 = Mathf.Max(0, t_1 - 0.1f/*UnityEngine.Random.Range(0, 0.2f)*/); //不要random了，防止多个模型显示的时候，随机提前加载，导致多个模型，不是同时出现在镜头里。
            ch.loadtime = t_1;

            if (t_2 == 0) t_2 = (float)director.duration;
            else t_2 += UnityEngine.Random.Range(0, 0.2f);
            ch.unloadtime = t_2;
            if (index >= 0 && index < m_orig_data.chars.Length) //同步到OrignalTimelineData中的chars数据成员，否则只保存在m_char_data无效
            {
                m_orig_data.chars[index].loadtime = ch.loadtime;
                m_orig_data.chars[index].unloadtime = ch.unloadtime;
            }
        }
    }
}