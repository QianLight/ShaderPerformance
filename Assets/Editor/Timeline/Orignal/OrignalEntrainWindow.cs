using CFEngine;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.Timeline;

namespace XEditor
{

    public class OrignalEntrainWindow : EditorWindow
    {
        private static string m_recentFilePath;//"Assets/Editor/Timeline/res/recent.txt";
        private static List<int> m_rencentTimelineIds = new List<int>();
        public static bool debugMode = false;
        public static bool m_isTimelinePrefab = false;
        const string prefix = "Orignal_";
        private string timeline = prefix, comment = "";
        private bool modify;
        private string search = "";
        private string path;
        private string[] scenes, timelines, comments;
        private string[] filterComments;
        private List<int> filterIndex = new List<int>();
        private static int selectScene, selectTimeline, selectFilter;
        private static int m_recentOpenIndex;
        private OCSData conf;
        private GUIStyle btnStyle;
        private GUIStyle m_btnStyleCustom;
        private GUIStyle m_btnStyleCustom2;
        private TimelineStatistic statistic;

        //new add
        private static float m_rencentItemHeight = 0;
        private static GUIStyle m_rencentItemStyle;
        private string m_inputTimelineControlName = "inputTimeline";
        private static string m_roleFilePath = "Assets/Editor/Timeline/res/roles.txt";
        private static string m_sceneFilePath = "Assets/Editor/Timeline/res/scenes.txt";

        private static List<RoleInfo> m_roles = new List<RoleInfo>();
        private static List<SceneInfo> m_scenes = new List<SceneInfo>();
        public static List<RoleInfo> m_selectedRoles = new List<RoleInfo>();
        public static bool m_createNewTimeline = false;
        private Vector2 m_uiScroll = Vector2.zero;
        private Vector2 m_rolesScroll = Vector2.zero;
        private Vector2 m_selectedRolesScroll = Vector2.zero;
        private Vector2 m_timelineScroll;
        private Vector2 m_scenesScroll;
        private bool m_showTimelines;
        private bool m_showScenes;
        private int m_currentSelectedRoleIndex = -1;
        private static List<string> m_trackDesc = new List<string>() {
            "位移轨道",
            "动作轨道1",
            "动作轨道2",
            "显隐轨道",
            "自定义轨道",

            "表情轨道",
            "眨眼轨道",
            "口型轨道",
            "看向轨道",
            "自定义轨道2",
        };

        public enum TrackType
        {
            ETransformTween = 0,
            EAnimationTrack1,
            EAnimationTrack2,
            EAnimationTrackShowHide,
            EAnimationCustom,

            EAnimationExpression,
            EAnimationEye,
            EAnimationMouth,
            EAnimationLookAt,
            EAnimationCustom2,
            Max,
        }

        public struct SceneInfo
        {
            public int m_id;
            public string m_scenePath;
            public string m_comment;
        }

        public struct RoleInfo
        {
            public int m_id;
            public string m_prefabName;
            public string m_comment;
            public Vector3 m_scale;

            public bool[] m_trackFlags;

            public void SelectAllDefaultTrack()
            {
                int len = (int)TrackType.Max;
                if (m_trackFlags == null) m_trackFlags = new bool[len];
                for (int i = 0; i < len; i++)
                {
                    m_trackFlags[i] = true;
                }
            }
        }

        [MenuItem(@"XEditor/Timeline/DramaWindow  %e")]
        static void DramaTimelineWindow()
        {
            m_recentFilePath = Application.persistentDataPath + "/timeline/recent.txt";

            var win = GetWindow(typeof(OrignalEntrainWindow));
            win.titleContent = new GUIContent("Timeline", OriginalSetting.Icon);
            win.Show();
            win.minSize = new Vector2(1200, 750);
            var s = GetWindow<TimelineStatistic>("", false, typeof(OrignalEntrainWindow));
            s.titleContent = new GUIContent("Statistic", OriginalSetting.Stc);
            var a = GetWindow<GenerateLipAnimWindow>("", false, typeof(OrignalEntrainWindow));
            a.titleContent = new GUIContent("LipSync", OriginalSetting.Lip);
            var v = GetWindow<TimelineSettings>("", false, typeof(OrignalEntrainWindow));
            v.titleContent = new GUIContent("Settings", OriginalSetting.Set);
            var assistant = GetWindow<TimelineAssistantWindow>("", false, typeof(OrignalEntrainWindow));
            assistant.titleContent = new GUIContent("Assistant");
            win.Focus();
            ReadRecentOpenCutsceneIDs();
            Init();
        }

        private static void Init()
        {
            m_rencentItemStyle = new GUIStyle();
            m_rencentItemStyle.alignment = TextAnchor.MiddleLeft;
            m_rencentItemStyle.fixedHeight = 20;
            m_rencentItemStyle.normal.textColor = Color.black;
            ReadAllRoles(m_roleFilePath);
        }

        private void OnEnable()
        {
            modify = false;
            if (conf == null) conf = AssetDatabase.LoadAssetAtPath<OCSData>(OriginalSetting.dataPat);
            //SearchScene();
            ReadScenes();
            SearchCS();
            FilterSearch();
            TimelineSettings.ApplySetting();
            m_selectedRoles.Clear();
            m_currentSelectedRoleIndex = -1;
        }

        private void SearchScene()
        {
            string pref = "Assets/Scenes/Scenelib";
            DirectoryInfo di = new DirectoryInfo(pref);
            FileInfo[] files = di.GetFiles("*.unity", SearchOption.AllDirectories);
            int idx = di.FullName.IndexOf("Assets\\");
            scenes = files.Select(x => x.FullName.Replace(".unity", "").Substring(idx + pref.Length + 1)).ToArray();
        }

        private void ReadScenes()
        {
            if (!File.Exists(m_sceneFilePath))
            {
                SearchScene();
                WriteScenes();
                return;
            }
            using (FileStream fs = new FileStream(m_sceneFilePath, FileMode.Open))
            {
                m_scenes.Clear();
                StreamReader reader = new StreamReader(fs, Encoding.Unicode);
                string line = reader.ReadLine();
                line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    SceneInfo row = new SceneInfo();
                    string[] strs = line.Split('\t');
                    row.m_id = int.Parse(strs[0]);
                    row.m_scenePath = strs[1];
                    row.m_comment = strs[2];
                    m_scenes.Add(row);
                }
                scenes = new string[m_scenes.Count];
                for (int i = 0; i < m_scenes.Count; ++i)
                {
                    scenes[i] = m_scenes[i].m_scenePath;
                }
            }
        }

        private void WriteScenes()
        {
            using (FileStream writer = new FileStream(m_sceneFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                StreamWriter sw = new StreamWriter(writer, Encoding.Unicode);
                for (int i = 0; i < scenes.Length; ++i)
                {
                    string str = i + "\t" + scenes[i] + "\t" + string.Empty;
                    sw.WriteLine(str);
                }
                sw.Close();
            }
        }

        internal struct TimelineNameComparer : IComparer<FileInfo>
        {
            public int Compare(FileInfo x, FileInfo y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }

        private void SearchCS()
        {
            DirectoryInfo dir1 = new DirectoryInfo(OriginalSetting.LIB);
            FileInfo[] files1 = dir1.GetFiles("*.playable", SearchOption.AllDirectories);

            DirectoryInfo dir2 = new DirectoryInfo(OriginalSetting.m_unusedTimelineDir);
            FileInfo[] files2 = dir2.GetFiles("*.playable", SearchOption.AllDirectories);

            List<FileInfo> allTimelines = new List<FileInfo>();
            allTimelines.AddRange(files1);
            allTimelines.AddRange(files2);
            allTimelines.Sort(new TimelineNameComparer());

            var t = allTimelines.Select(x => x.Name.Replace(".playable", "")).Where(x => conf.Exist(x)).ToList();
            t.Add("< NONE >");
            t.Reverse();
            t.Sort();
            timelines = t.ToArray();

            comments = new string[timelines.Length];
            for (int i = 0; i < timelines.Length; i++)
            {
                var comt = conf.SearchComment(timelines[i]);
                comt = string.IsNullOrEmpty(comt) ? "" : " [" + comt + "]";
                comments[i] = timelines[i] + comt;
            }
        }

        private void FilterSearch()
        {
            filterIndex.Clear();
            if (!string.IsNullOrEmpty(search))
            {
                List<string> list_cmt = new List<string>();
                string sr = search.ToLower();
                for (int i = 0; i < comments.Length; i++)
                {
                    if (comments[i].ToLower().Contains(sr))
                    {
                        list_cmt.Add(comments[i]);
                        filterIndex.Add(i);
                    }
                }
                filterComments = list_cmt.ToArray();
                if (filterIndex.Count > 0)
                {
                    selectFilter = 0;
                    selectTimeline = filterIndex[selectFilter];
                    timeline = selectTimeline == 0 ? prefix : timelines[selectTimeline];
                }
            }
            else
            {
                filterComments = comments;
                for (int i = 0; i < comments.Length; i++)
                    filterIndex.Add(i);
                selectTimeline = filterIndex[selectFilter];
                timeline = selectTimeline == 0 ? prefix : timelines[selectTimeline];
            }
        }

        private void InitStyle()
        {
            if (btnStyle == null)
            {
                btnStyle = new GUIStyle(GUI.skin.button);
                btnStyle.fontStyle = FontStyle.Bold;
                btnStyle.fontSize = 20;
            }

            if (m_btnStyleCustom == null)
            {
                m_btnStyleCustom = new GUIStyle(GUI.skin.button);
                m_btnStyleCustom.alignment = TextAnchor.MiddleLeft;
            }

            if (m_btnStyleCustom2 == null)
            {
                m_btnStyleCustom2 = new GUIStyle(GUI.skin.button);
                m_btnStyleCustom2.alignment = TextAnchor.MiddleLeft;
            }
        }

        void DrawSearch(float width)
        {
            string tmp = search;
            search = XEditorUtil.GuiSearch(search, GUILayout.MinWidth(width));
            if (tmp != search)
            {
                FilterSearch();
            }
        }

        void OnGUI()
        {
            if (EditorApplication.isCompiling) Close();

            InitStyle();

            GUILayout.BeginVertical();
            m_uiScroll = GUILayout.BeginScrollView(m_uiScroll);

            GUILayout.Space(5);
            GUILayout.Label("剧情编辑器", XEditorUtil.titleLableStyle);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            debugMode = GUILayout.Toggle(debugMode, "  <debug mode>");
            GUILayout.Space(10);
            m_isTimelinePrefab = GUILayout.Toggle(m_isTimelinePrefab, "  <timeline prefab>");
            GUILayout.Space(10);
            if (GUILayout.Button("delete recent"))
            {
                if (File.Exists(m_recentFilePath))
                {
                    File.Delete(m_recentFilePath);
                    m_rencentTimelineIds.Clear();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);



            DrawSearch(position.size.x);

            GUILayout.Space(10);
            EditorGUI.BeginChangeCheck();

            GUI.SetNextControlName(m_inputTimelineControlName);
            timeline = EditorGUILayout.TextField("输入剧情ID:", timeline);
            if (timeline != timelines[selectTimeline]) selectTimeline = 0;
            if (EditorGUI.EndChangeCheck())
            {
                search = "";
                selectTimeline = 0;
                if (timelines.Contains(timeline))
                {
                    for (int i = 0; i < timelines.Length; i++)
                    {
                        if (timelines[i] == timeline)
                        {
                            selectTimeline = i;
                            break;
                        }
                    }
                }
            }
            DrawRecentOpenTimelines();
            GUILayout.Space(m_rencentItemHeight + 10);

            if (GUILayout.Button("选择timeline", btnStyle, GUILayout.Height(30)))
            {
                m_showTimelines = !m_showTimelines;
            }

            DrawTimelines();
            GUILayout.Space(8);

            if (selectTimeline > 0)
            {
                modify = EditorGUILayout.Toggle("修改默认路径", modify);
            }
            GUILayout.Space(8);
            if (selectTimeline == 0 || modify)
            {
                //selectScene = EditorGUILayout.Popup("目标场景选择:", selectScene, scenes);
                DrawScenes();
                GUILayout.Space(4);
                string sceneName = string.Empty;
                if (selectScene >= 0 && selectScene < scenes.Length) sceneName = scenes[selectScene];
                EditorGUILayout.TextField("选择的场景:", sceneName);
                comment = EditorGUILayout.TextField("剧情标注:", comment);
            }

            GUILayout.BeginHorizontal();

            if (selectTimeline == 0 && GUILayout.Button("创建剧情", btnStyle, GUILayout.Height(30), GUILayout.Width(600)))
            {
                UnityEngine.CFUI.SpriteUtility.StaticInit();
                OrignalTimelineEditor.Init();
                TimelinePrefabDataEditor.Init();
                Create();
                OrignalTimelineEditor.m_loader = new OriginalSyncLoadEditor(OrignalTimelineEditor.data);
            }

            if (selectTimeline == 0 && GUILayout.Button("创建AVG剧情", btnStyle, GUILayout.Height(30), GUILayout.Width(600)))
            {
                TimelineAvgConfig m_AvgConfig = new TimelineAvgConfig();
                if (XTableReader.ReadFile("Table/TimelineAvgConfig", m_AvgConfig))
                {
                    UnityEngine.CFUI.SpriteUtility.StaticInit();
                    OrignalTimelineEditor.Init();
                    CreateAVG(m_AvgConfig);
                    OrignalTimelineEditor.m_loader = new OriginalSyncLoadEditor(OrignalTimelineEditor.data);
                }
            }

            GUILayout.EndHorizontal();

            if (selectTimeline > 0 && GUILayout.Button("载入剧情", btnStyle, GUILayout.Height(30)))
            {
                UnityEngine.CFUI.SpriteUtility.StaticInit();
                Open();
                AddToRecent();
                WriteRecentOpenCutsceneIDs();
            }

            if (selectTimeline == 0)
            {
                DrawTimelineTemplate();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawScenes()
        {
            if (GUILayout.Button("选择场景", btnStyle, GUILayout.Height(30)))
            {
                m_showScenes = !m_showScenes;
            }
            if (m_showScenes)
            {
                m_scenesScroll = GUILayout.BeginScrollView(m_scenesScroll, GUILayout.Height(400));
                int len = scenes.Length;
                for (int i = 0; i < len; ++i)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(i.ToString("000"), GUILayout.Width(50));
                    if (GUILayout.Button(scenes[i] + "[" + m_scenes[i].m_comment + "]", m_btnStyleCustom, GUILayout.Width(position.size.x - 50)))
                    {
                        selectScene = i;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
        }

        private void DrawTimelineTemplate()
        {
            GUILayout.BeginHorizontal();
            DrawAllRoles();
            DrawSelectedRoles();
            DrawSelectedRoleInfo();
            GUILayout.EndHorizontal();
        }

        public static void ReadAllRoles(string path)
        {
            m_roles.Clear();
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                StreamReader reader = new StreamReader(fs, Encoding.Unicode);
                string line = reader.ReadLine();
                line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    RoleInfo row = new RoleInfo();
                    string[] strs = line.Split('\t');
                    row.m_id = int.Parse(strs[0]);
                    row.m_prefabName = strs[1];
                    row.m_comment = strs[2];
                    m_roles.Add(row);
                }
            }
        }


        private void DrawAllRoles()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("选择你需要的角色:", GUILayout.Width(500));
            m_rolesScroll = GUILayout.BeginScrollView(m_rolesScroll, false, true, GUILayout.Width(500), GUILayout.Height(400));
            for (int i = 0; i < m_roles.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                RoleInfo roleInfo = m_roles[i];
                GUILayout.Label(i.ToString("000"), GUILayout.Width(50));
                string buttonName = roleInfo.m_prefabName + "[" + roleInfo.m_comment + "]";
                if (GUILayout.Button(buttonName, m_btnStyleCustom, GUILayout.Width(350)))
                {

                }
                if (GUILayout.Button("add", GUILayout.Width(50)))
                {
                    roleInfo.SelectAllDefaultTrack();
                    m_selectedRoles.Add(roleInfo);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawSelectedRoles()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("已经选择的角色:", GUILayout.Width(500));
            m_selectedRolesScroll = GUILayout.BeginScrollView(m_selectedRolesScroll, GUILayout.Width(600), GUILayout.Height(400));
            for (int i = 0; i < m_selectedRoles.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                RoleInfo roleInfo = m_selectedRoles[i];
                GUILayout.Label(i.ToString("000"), GUILayout.Width(50));
                string buttonName = roleInfo.m_prefabName + "[" + roleInfo.m_comment + "]";
                if (GUILayout.Button(buttonName, m_btnStyleCustom, GUILayout.Width(350)))
                {
                    m_currentSelectedRoleIndex = i;
                }
                if (GUILayout.Button("remove", GUILayout.Width(60)))
                {
                    m_selectedRoles.RemoveAt(i);
                    m_currentSelectedRoleIndex = -1;
                }
                if (GUILayout.Button("up", GUILayout.Width(50)))
                {
                    SwapSelectedRole(i, i - 1);
                }
                if (GUILayout.Button("down", GUILayout.Width(50)))
                {
                    SwapSelectedRole(i, i + 1);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void SwapSelectedRole(int i, int j)
        {
            if (i == 0 && j < 0) return;
            if (i == m_selectedRoles.Count - 1 && j >= m_selectedRoles.Count) return;
            RoleInfo temp = m_selectedRoles[i];
            m_selectedRoles[i] = m_selectedRoles[j];
            m_selectedRoles[j] = temp;
        }

        private void DrawSelectedRoleInfo()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("设置角色的初始轨道信息:", GUILayout.Width(400));
            if (m_currentSelectedRoleIndex < 0 || m_currentSelectedRoleIndex >= m_selectedRoles.Count) return;
            GUILayout.Space(5);
            RoleInfo roleInfo = m_selectedRoles[m_currentSelectedRoleIndex];
            string title = m_currentSelectedRoleIndex.ToString("000");
            title += "_" + roleInfo.m_prefabName + "[" + roleInfo.m_comment + "]";
            GUILayout.Label("角色:" + title, GUILayout.Width(400));
            for (int i = 0; i < roleInfo.m_trackFlags.Length; ++i)
            {
                roleInfo.m_trackFlags[i] = GUILayout.Toggle(roleInfo.m_trackFlags[i], new GUIContent(m_trackDesc[i]), GUILayout.Width(200));
            }
            GUILayout.EndVertical();
        }


        private void DrawTimelines()
        {
            if (m_showTimelines)
            {
                m_timelineScroll = GUILayout.BeginScrollView(m_timelineScroll, GUILayout.Height(400));
                int len = filterComments.Length;
                for (int i = 0; i < len; ++i)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(i.ToString("000"), GUILayout.Width(50));
                    if (GUILayout.Button(filterComments[i], m_btnStyleCustom, GUILayout.Width(position.size.x - 50)))
                    {
                        string name = GUI.GetNameOfFocusedControl();
                        if (name.Equals(m_inputTimelineControlName))
                        {
                            GUI.FocusControl(string.Empty);
                        }
                        selectTimeline = filterIndex[i];
                        timeline = selectTimeline == 0 ? prefix : timelines[selectTimeline];
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
        }

        private void DrawRecentOpenTimelines()
        {
            m_rencentItemHeight = 0;
            string controlName = GUI.GetNameOfFocusedControl();
            if (!controlName.Equals(m_inputTimelineControlName)) return;
            if (m_rencentTimelineIds.Count <= 0) return;

            m_rencentItemHeight = m_rencentItemStyle.fixedHeight * m_rencentTimelineIds.Count;
            m_rencentItemHeight += 10;
            GUILayout.BeginHorizontal();
            GUILayout.Space(150);
            GUILayout.Label("最近打开");
            GUILayout.EndHorizontal();

            GUILayout.BeginArea(new Rect(150, 120, 1000, m_rencentItemHeight), EditorStyles.textArea);
            {
                for (int i = 0; i < m_rencentTimelineIds.Count; ++i)
                {
                    int index = m_rencentTimelineIds[i];
                    string name = comments[index];
                    if (GUILayout.Button(name))
                    {
                        if (index < comments.Length)
                        {
                            selectTimeline = index;
                            timeline = selectTimeline == 0 ? prefix : timelines[selectTimeline];
                        }
                        GUI.FocusControl(string.Empty);
                    }
                }
            }
            GUILayout.EndArea();
        }

        private static void AddToRecent()
        {
            m_rencentTimelineIds.Remove(selectTimeline);
            m_rencentTimelineIds.Insert(0, selectTimeline);
            while (m_rencentTimelineIds.Count > 5)
            {
                int last = m_rencentTimelineIds.Count - 1;
                m_rencentTimelineIds.RemoveAt(last);
            }
        }

        private static void ReadRecentOpenCutsceneIDs()
        {
            if (!File.Exists(m_recentFilePath))
            {
                return;
            }
            m_rencentTimelineIds.Clear();
            using (FileStream fs = new FileStream(m_recentFilePath, FileMode.Open))
            {
                StreamReader reader = new StreamReader(fs, Encoding.Unicode);
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null && !string.IsNullOrEmpty(line))
                {
                    int index = 0;
                    if (int.TryParse(line, out index))
                    {
                        m_rencentTimelineIds.Add(index);
                    }
                }
            }
        }

        private static void WriteRecentOpenCutsceneIDs()
        {
            string dir = Application.persistentDataPath + "/timeline";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (FileStream writer = new FileStream(m_recentFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                StreamWriter sw = new StreamWriter(writer, Encoding.Unicode);
                for (int i = 0; i < m_rencentTimelineIds.Count; ++i)
                {
                    sw.WriteLine(m_rencentTimelineIds[i]);
                }
                sw.Close();
            }
        }

        private void Create()
        {
            if (timelines.Contains(timeline))
            {
                EditorUtility.DisplayDialog("tip", "the timeline has exist", "ok");
            }
            else if (!string.IsNullOrEmpty(timeline))
            {
                if (string.IsNullOrEmpty(scenes[selectScene]))
                {
                    EditorUtility.DisplayDialog("tip", "Not config in OriginalCSData", "ok");
                }
                else
                {
                    conf.OpenScene(scenes[selectScene]);
                    CreateTimeline.FindOrLoadTimelineGameobject();
                    GameObject go = new GameObject(timeline);
                    GameObject cs = new GameObject("timeline");
                    cs.transform.parent = go.transform;
                    cs.transform.localPosition = Vector3.zero;
                    cs.transform.localRotation = Quaternion.identity;
                    cs.transform.localScale = Vector3.one;

                    conf.AddorUpdate(timeline, scenes[selectScene], comment);
                    //OrignalTimelineEditor.Reset();
                    AddItem(timeline, comment);

                    string nep = OriginalSetting.LIB + timeline + ".playable";
                    AssetDatabase.CopyAsset(OriginalSetting.tmpPat, nep);
                    PlayableAsset asset = AssetDatabase.LoadAssetAtPath<PlayableAsset>(nep);
                    //asset.name = timeline;
                    var dirctor = cs.AddComponent<PlayableDirector>();
                    dirctor.playableAsset = asset;

                    //根据模板初始化timeline
                    InitTimelineByTemplate(cs, asset);

                    Selection.activeObject = cs;
                    PrefabUtility.SaveAsPrefabAssetAndConnect(go, OriginalSetting.LIB + timeline + ".prefab", InteractionMode.UserAction);
                    AssetDatabase.SaveAssets();

                    EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("tip", "drama id is null", "ok");
            }
        }

        private void CreateAVG(TimelineAvgConfig m_AvgConfig)
        {
            if (timelines.Contains(timeline))
            {
                EditorUtility.DisplayDialog("tip", "the timeline has exist", "ok");
            }
            else if (!string.IsNullOrEmpty(timeline))
            {
                string sceneName = m_AvgConfig.Table[0].ScenePath;

                if (string.IsNullOrEmpty(scenes[selectScene]))
                {
                    EditorUtility.DisplayDialog("tip", "Not config in OriginalCSData", "ok");
                }
                else
                {
                    conf.OpenScene(sceneName);
                    CreateTimeline.FindOrLoadTimelineGameobject();

                    GameObject prefab = LoadMgr.singleton.LoadAssetImmediate<GameObject>("TimelineUnused/Orignal_AVGsample", ".prefab");

                    GameObject go = GameObject.Instantiate(prefab);
                    go.name = timeline;
                    GameObject cs = go.transform.Find("timeline").gameObject;//new GameObject("timeline");
                    cs.transform.parent = go.transform;
                    cs.transform.localPosition = Vector3.zero;
                    cs.transform.localRotation = Quaternion.identity;
                    cs.transform.localScale = Vector3.one;

                    conf.AddorUpdate(timeline, sceneName, comment);
                    AddItem(timeline, comment);

                    string nep = OriginalSetting.LIB + timeline + ".playable";
                    AssetDatabase.CopyAsset(OriginalSetting.tmpAvgPat, nep);
                    PlayableAsset asset = AssetDatabase.LoadAssetAtPath<PlayableAsset>(nep);
                    var dirctor = cs.GetComponent<PlayableDirector>();
                    dirctor.playableAsset = asset;

                    m_selectedRoles.Clear();

                    AVGRoles avgRoles = new AVGRoles();
                    if (XTableReader.ReadFile("Table/AVGRoles", avgRoles))
                    {
                        List<uint> roles = new List<uint>();
                        for (int i = 0; i < m_AvgConfig.Table.Length; i++)
                        {
                            for (int r = 0; r < m_AvgConfig.Table[i].RoleId.Length; r++)
                            {
                                if (m_AvgConfig.Table[i].RoleId[r] != 0)
                                {
                                    if (!roles.Contains(m_AvgConfig.Table[i].RoleId[r]))
                                        roles.Add(m_AvgConfig.Table[i].RoleId[r]);
                                }
                            }
                        }
                        for (int i = 0; i < roles.Count; i++)
                        {
                            AVGRoles.RowData data = avgRoles.GetByPresentID(roles[i]);
                            RoleInfo role = new RoleInfo
                            {
                                m_id = (int)data.PresentID,
                                m_prefabName = data.Prefab,
                                m_comment = data.Name,
                                m_scale = data.Scale * Vector3.one
                            };

                            role.SelectAllDefaultTrack();
                            role.m_trackFlags[(int)TrackType.EAnimationTrack1] = false;
                            role.m_trackFlags[(int)TrackType.EAnimationLookAt] = false;
                            role.m_trackFlags[(int)TrackType.EAnimationMouth] = false;
                            role.m_trackFlags[(int)TrackType.EAnimationEye] = false;
                            m_selectedRoles.Add(role);
                        }
                    }

                    //根据模板初始化timeline
                    InitTimelineByTemplate(cs, asset);

                    Selection.activeObject = cs;
                    PrefabUtility.SaveAsPrefabAssetAndConnect(go, OriginalSetting.LIB + timeline + ".prefab", InteractionMode.UserAction);
                    AssetDatabase.SaveAssets();

                    EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");

                    OrignalTimelineEditor.CreateAvgTracks();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("tip", "drama id is null", "ok");
            }
        }

        private void InitTimelineByTemplate(GameObject go, PlayableAsset playableAsset)
        {
            m_createNewTimeline = true;
            OrignalTimelineData comp = null;
            if (m_isTimelinePrefab)
            {
                go.AddComponent<TimelinePrefabData>();
            }
            else
            {
                comp = go.AddComponent<OrignalTimelineData>();

            }
            if (comp == null)
            {
                comp = go.GetComponent<OrignalTimelineData>();
            }

            TimelineAsset timelineAsset = playableAsset as TimelineAsset;
            TrackAsset charactorGroups = timelineAsset.GetTrackByIndex(1) as TrackAsset;
            if (charactorGroups == null) return;
            int len = m_selectedRoles.Count;

            if (RTimeline.singleton.CharactersGroups == null)
                RTimeline.singleton.CharactersGroups = new List<TrackAsset>();
            RTimeline.singleton.CharactersGroups.Clear();
            if (comp != null && comp.chars == null) comp.chars = new OrignalChar[len];
            for (int i = 0; i < len; ++i)
            {
                comp.chars[i] = new OrignalChar();
                CreateOneRoleTracks(timelineAsset, charactorGroups, i, comp);
            }
        }

        private void CreateOneRoleTracks(TimelineAsset timelineAsset, TrackAsset charactorGroups, int index, OrignalTimelineData data)
        {
            RoleInfo roleInfo = m_selectedRoles[index];
            string trackGroupName = roleInfo.m_comment;
            TrackAsset newGroup = timelineAsset.CreateTrack(typeof(GroupTrack), charactorGroups, trackGroupName);
            //TrackAsset expressionTrack = null;
            RTimeline.singleton.CharactersGroups.Add(newGroup);
            int total = 0;
            for (int i = 0; i < roleInfo.m_trackFlags.Length; ++i)
            {
                bool flag = roleInfo.m_trackFlags[i];
                if (!flag) continue;
                TrackType trackType = (TrackType)(i);
                TrackAsset trackAsset = null;
                switch (trackType)
                {
                    case TrackType.EAnimationTrack1:
                    case TrackType.EAnimationTrack2:
                        trackAsset = timelineAsset.CreateTrack(typeof(AnimationTrack), newGroup, m_trackDesc[i]);
                        break;
                    case TrackType.EAnimationTrackShowHide:
                        trackAsset = timelineAsset.CreateTrack(typeof(ActivationTrack), newGroup, m_trackDesc[i]);
                        break;
                    case TrackType.EAnimationCustom:
                    case TrackType.EAnimationCustom2:
                        trackAsset = timelineAsset.CreateTrack(typeof(CustomAnimationTrack), newGroup, m_trackDesc[i]);
                        if (trackType == TrackType.EAnimationCustom2)
                            trackAsset.m_trackAssetType = TrackAssetType.Facial;
                        break;
                    case TrackType.EAnimationExpression:
                        trackAsset = timelineAsset.CreateTrack(typeof(AnimationTrack), newGroup, m_trackDesc[i]);
                        trackAsset.m_trackAssetType = TrackAssetType.Facial;
                        break;
                    case TrackType.EAnimationEye:
                    case TrackType.EAnimationMouth:
                        trackAsset = timelineAsset.CreateTrack(typeof(AnimationTrack), newGroup, m_trackDesc[i]);
                        trackAsset.m_trackAssetType = TrackAssetType.Facial;
                        break;
                    case TrackType.EAnimationLookAt:
                        trackAsset = timelineAsset.CreateTrack(typeof(AnimationTrack), newGroup, m_trackDesc[i]);
                        trackAsset.m_trackAssetType = TrackAssetType.LookAt;
                        break;
                    case TrackType.ETransformTween:
                        trackAsset = timelineAsset.CreateTrack(typeof(TransformTweenTrack), newGroup, m_trackDesc[i]);
                        break;


                }
                if (trackAsset != null)
                {
                    if (data.chars[index].m_trackAssets == null)
                    {
                        data.chars[index].m_trackAssets = new List<object>();
                    }

                    data.chars[index].m_trackAssets.Add(trackAsset);
                    total++;
                }
            }
            data.chars[index].tracks = new int[total];
        }

        private void AddItem(string timeline, string comment)
        {
            XEditorUtil.Add(ref timelines, timeline);
            XEditorUtil.Add(ref comments, timeline + " [" + comment + "]");
        }

        private void Open()
        {
            if (!string.IsNullOrEmpty(timeline))
            {
                if (timelines.Contains(timeline))
                {
                    if (modify) conf.AddorUpdate(timeline, scenes[selectScene], comment);

                    string scene = modify ? scenes[selectScene] : conf.SearchPath(timeline);
                    if (string.IsNullOrEmpty(scene))
                    {
                        EditorUtility.DisplayDialog("tip", "Not config in OriginalCSData", "ok");
                    }
                    else
                    {
                        conf.OpenScene(scene);
                        CreateTimeline.FindOrLoadTimelineGameobject();
                        //OrignalTimelineEditor.Reset();
                        OrignalTimelineEditor.Init();
                        TimelinePrefabDataEditor.Init();
                        string path = OriginalSetting.LIB + timeline + ".prefab";
                        if (!File.Exists(path))
                        {
                            path = OriginalSetting.m_unusedTimelineDir + timeline + ".prefab";
                        }
                        var go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        var gg = PrefabUtility.InstantiatePrefab(go);
                        var dir = (gg as GameObject).GetComponentInChildren<PlayableDirector>();

                        if (dir != null)
                        {
                            Selection.activeObject = dir.gameObject;
                            dir.time = 0.01d;
                        }
                        LoadVirtualCam(timeline);


                        SetCameraOverlay();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("tip", "the timeline is not exist", "ok");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("tip", "drama id is null", "ok");
            }
        }

        private void LoadVirtualCam(string timeline)
        {
            var vc = conf.SearchVirtualCam(timeline);
            if (!string.IsNullOrEmpty(vc))
            {
                string pat = OriginalSetting.vcPath + vc;
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(pat) as GameObject;
                if (go != null)
                {
                    var gg = PrefabUtility.InstantiatePrefab(go);
                    gg.name = OriginalSetting.EditorVC;
                }
            }
        }

        private void SetCameraOverlay()
        {
            if (Camera.main == null) return;

            UniversalAdditionalCameraData mainCamera = Camera.main.GetComponent<UniversalAdditionalCameraData>();

            UniversalAdditionalCameraData[] allCameras = GameObject.FindObjectsOfType<UniversalAdditionalCameraData>();

            for (int i = 0; i < allCameras.Length; i++)
            {
                UniversalAdditionalCameraData cam = allCameras[i];
                if (cam.renderType == CameraRenderType.Overlay)
                {
                    mainCamera.cameraStack.Add(cam.GetComponent<Camera>());
                }
            }
        }
    }
}