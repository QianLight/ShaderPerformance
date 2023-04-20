#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;
using UnityEngine.CFUI;
using CFEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace XEditor
{
    public class XReactEntranceWindow : EditorWindow
    {
        public static readonly string Rtp = "Assets/BundleRes/ReactPackage/";
        private const int RENDER_TEXTURE_HEIGHT = 512;

        public static XReactEntranceWindow Instance = null;

        [MenuItem(@"XEditor/React/ReactLineEditor %j")]
        static void ReactLineCreater()
        {
            var Instance = EditorWindow.GetWindow<XReactEntranceWindow>(@"ReactLine Editor", true);
            Instance.position = new Rect(500, 400, 1000, 500);
            Instance.wantsMouseMove = true;
            Instance.Show();
            Instance.Repaint();
        }



        /* BOX RECT */
        public Rect BGRect = new Rect(new Vector2(0, 0), new Vector2(1500, 1000));
        public Rect boxRect = new Rect(new Vector2(100, 300), new Vector2(500, 200));
        public Rect basicInfoRect = new Rect(new Vector2(10, 10), new Vector2(500, 100));
        public Rect TimeLineRect = new Rect(new Vector2(30, 450), new Vector2(600, 100));
      

        public int ToolBarHeight;

        /* GUIContent */
        //public GUIContent BGContent = new GUIContent("");        
        //public GUIContent SaveContent = new GUIContent("保存", "保存Rect配置");

        GUIStyle hotStyle = null;

        /* gap */
        public float NormalGap = 10f;
        public float MiniGap = 5f;

        public float NormalTextHeight = 30f;
        
        public bool IsHot { get; set; }

        public static XReactCreatWindow openWid;
        public static XReactMainWindow mainWid;
        XReactTextWindow textWid;

        public XReactDataSet ReactDataSet = null;
        public string LastScript = null;

        bool _try_attach = false;

        bool isResScene = false;

        public enum ProcessState
        {
            CreatOrOpen,
            Editor,
        }

        public ProcessState CurState = ProcessState.CreatOrOpen;
     

        public virtual void OnEnable()
        {
            if (!isResScene)
                NewScene();
            Instance = this;
            ReactCommon.Init();

            if (mainWid == null)
            {
                mainWid = new XReactMainWindow();
                mainWid.Init(this);
            }

            if (openWid == null)
            {
                openWid = new XReactCreatWindow();
                openWid.Init(this);

                if (!string.IsNullOrEmpty(LastScript))
                {
                    _try_attach = true;
                }
            }
            EditorApplication.playModeStateChanged += ChangedPlaymodeState;
            //Debug.LogError("OnEnable");
        }

        public virtual void OnDisable()
        {
            EditorApplication.playModeStateChanged -= ChangedPlaymodeState;
            openWid = null;
            mainWid = null;
            //Debug.LogError("OnDisable");
        }
        public void OnDestroy()
        {
            if (Application.isPlaying)
                EditorApplication.ExecuteMenuItem("Edit/Play");
        }
        void ChangedPlaymodeState(PlayModeStateChange obj)
        {

            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    CLoad(LastScript);
                    HotLoad();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    //CheckSave();
                    break;
            }
        }

        private void OnClose()
        {
            //Debug.LogError("OnClose");

            CheckSave();

            openWid = null;
            mainWid = null;
            if (textWid != null)
            {
                textWid.Close();
                textWid = null;
            }
        }

        #region GUI

        //BeginArea
        Rect area = new Rect(0, 0, 20, 1000);

        void OnGUI()
        {
            if (_try_attach)
            {
                // CLoad(LastScript);
                // TryAttach();
                _try_attach = false;
            }

            ToolBarHeight = DrawToolBar();            

            BGRect.y = ToolBarHeight;
            BeginWindows();
            DrawInspectorWindow();
            EndWindows();
        }


        void DrawInspectorWindow()
        {
            Handles.BeginGUI();

            if (mainWid != null && mainWid.Window)
                mainWid.InitGUI();

            if (openWid != null)
            {
                if (ReactDataSet == null)
                    CurState = ProcessState.CreatOrOpen;

                if (XReactDataHostBuilder.hoster == null)
                    IsHot = false;
            }

            switch (CurState)
            {
                case ProcessState.CreatOrOpen:
                    {        
                        if (openWid != null)
                            openWid.OnGUI();
                    }
                    break;
                case ProcessState.Editor:
                    {
                        if (mainWid != null)
                            mainWid.OnGUI();
                    }
                    break;
            }

            Handles.EndGUI();
        }

        public virtual int DrawToolBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            { 
                if (GUILayout.Button("New", EditorStyles.toolbarButton))
                {
                    ToolBarNewClicked();
                }
                if (GUILayout.Button("Open", EditorStyles.toolbarButton))
                {
                    ToolBarOpenClicked();
                }

                if (CurState == ProcessState.Editor)
                {
                    if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                    {
                        ToolBarSaveClicked();
                    }
                    if (GUILayout.Button("Text", EditorStyles.toolbarButton))
                    {
                        ToolBarTextClicked();
                    }

                    if (GUILayout.Button("Attach", EditorStyles.toolbarButton))
                    {
                        ToolBarAttachClicked();
                    }

                    //if(GUILayout.Button("NewScene", EditorStyles.toolbarButton))
                    //{
                    //    NewScene();
                    //}

                    if (hotStyle == null)
                    {
                        hotStyle = new GUIStyle(EditorStyles.toolbarButton);
                    }

                    if (hotStyle != null)
                    {
                        hotStyle.normal.textColor = IsHot ? Color.green : ReactCommon.InitColor;
                    }

                    if (GUILayout.Button("Hot", hotStyle))
                    {
                        if (!IsHot)
                            HotLoad();
                        else
                        {
                            if (EditorUtility.DisplayDialog("Load", "重新加载？", "Load", "Cancel"))
                            {
                                HotLoad();
                            }
                        }
                    }
                   
                }
                else
                {
                    if (GUILayout.Button("Attach", EditorStyles.toolbarButton))
                    {
                        ToolBarAttachClicked();
                    }
                }

                GUILayout.FlexibleSpace();
                if (isResScene)
                {                    
                    ToolBarExtra();
                }
                else
                {
                    GUILayout.Label("");
                }
                GUILayout.Space(20);

            }
            GUILayout.EndHorizontal();
            return 20;
        }


        public void CheckSave()
        {
            if (ReactDataSet == null) return;

            if (EditorUtility.DisplayDialog("保存", "需要保存吗？", "Save", "Close"))
            {
                Debug.Log("Save!");
                Save();
            }
            else
            {
                Debug.Log("Close!");
            }
        }

        public void ToolBarNewClicked()
        {
            CheckSave();

            CurState = ProcessState.CreatOrOpen;

        }

        public void ToolBarOpenClicked()
        {
            //CheckSave();

            Open();
        }

        public void ToolBarSaveClicked()
        {
            Save();
        }

        public void ToolBarExtra()
        {
            if (GUILayout.Button(Application.isPlaying ? "Stop" : "Play", EditorStyles.toolbarButton))
            {
                //CheckSave();
                EditorApplication.ExecuteMenuItem("Edit/Play");
                GUIUtility.ExitGUI();
            }
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Pause", EditorStyles.toolbarButton))
                {
                    EditorApplication.ExecuteMenuItem("Edit/Pause");
                    GUIUtility.ExitGUI();
                }
                if (GUILayout.Button("Step", EditorStyles.toolbarButton))
                {
                    EditorApplication.ExecuteMenuItem("Edit/Step");
                    GUIUtility.ExitGUI();
                }
            }
        }

        void ToolBarTextClicked()
        {
            if (textWid == null)
            {
                if (ReactDataSet != null)
                {
                    textWid = EditorWindow.GetWindow<XReactTextWindow>(ReactDataSet.ReactData.Name + ".bytes");
                    textWid.position = new Rect(position.x + position.width, position.y, 500, position.height);
                    textWid.wantsMouseMove = true;
                    textWid.Init(this);
                    textWid.Show();
                    textWid.Repaint();
                }
                
            }
            else
            {
                textWid.Close();
                textWid = null;
            }
        }

        void ToolBarAttachClicked()
        {
            TryAttach();
        }
        #endregion

        //Open file data  
        public void CLoad(string path)
        {
            //ReactDataSet = null;
            //TryAttach();
            //if (ReactDataSet == null)
            //{
                OpenResFile(path);
                LoadReact(path, out ReactDataSet);
            //}

            LastScript = path;
            CurState = XReactEntranceWindow.ProcessState.Editor;
            minSize = new Vector2(1000f, 600f);
            //Window.position = new Rect(Window.position.x, Window.position.y, 1000f, 600f);
        }

        public void Open()
        {
            string file = EditorUtility.OpenFilePanel("Select react file", XEditorPath.Rtp, "bytes");

            if (file.Length != 0)
            {
                OpenResFile(file);
                LoadReact(file, out ReactDataSet);

                LastScript = file;

                CurState = XReactEntranceWindow.ProcessState.Editor;
                minSize = new Vector2(1000f, 600f);
                //Window.position = new Rect(Window.position.x, Window.position.y, 1000f, 600f);
            }
        }

        public void OpenResFile(string file)
        {
            var fi = new FileInfo(file);
            var parentDir = fi.Directory.Parent.Name;
            mainWid.resMap.Clear();
            string assetPath = string.Format("{0}Config/ReactPackage/{1}.asset",
                LoadMgr.singleton.editorResPath, fi.Name.Replace(".bytes", ""));
            if (File.Exists(assetPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<ResRedirectConfig>(assetPath);
                foreach (var res in asset.resPath)
                {
                    mainWid.resMap[res.name + res.ext] = res.physicPath;
                }
            }
        }

        public static bool LoadReact(string pathwithname, out XReactDataSet DataSet)
        {
            return XReactDataHostBuilder.singleton.Load(pathwithname, out DataSet);
        }


        public void TryAttach()
        {
            var ReactHoster = GameObject.FindObjectOfType<XReactEntity>();
            if (ReactHoster != null)
            {
                Attach(ReactHoster);
            }
        }

        public void Attach(XReactEntity host)
        {
            ReactDataSet = host.ReactDataSet;

            //Debug.Log("Attach....");

            XReactDataHostBuilder.AttachHost(host.gameObject);
            XReactDataHostBuilder.HotBuildEx(ReactDataSet.ConfigData, ref ReactDataSet);
            IsHot = true;
            CurState = XReactEntranceWindow.ProcessState.Editor;
        }

        #region Save

        public void Save()
        {
            string file = GetDataFileWithPath();

            ReactDataSet.ReactData.Name = ReactDataSet.ReactDataExtra.ScriptFile;

            if (ReactDataSet.ReactDataExtra.ReactClip != null)
            {
                string path = AssetDatabase.GetAssetPath(ReactDataSet.ReactDataExtra.ReactClip).Remove(0, 17);
                ReactDataSet.ReactData.ClipName = path.Remove(path.LastIndexOf('.'));
            }
            else
            {
                ReactDataSet.ReactData.ClipName = "";
            }

            if (ReactDataSet.ReactDataExtra.ReactClip2 != null)
            {
                string path = AssetDatabase.GetAssetPath(ReactDataSet.ReactDataExtra.ReactClip2).Remove(0, 17);
                ReactDataSet.ReactData.ClipName2 = path.Remove(path.LastIndexOf('.'));
            }
            else
            {
                ReactDataSet.ReactData.ClipName2 = "";
            }


            StripData(ReactDataSet.ReactData);
            foreach (var track in mainWid.trackDraws)
            {
                track.OnSave();
            }
            XDataIO<XReactData>.singleton.SerializeData(file, ReactDataSet.ReactData);
            XDataIO<XReactConfigData>.singleton.SerializeData(XEditorPath.GetCfgFromSkp(file), ReactDataSet.ConfigData);
            SaveResFile(ReactDataSet.ReactData.Name);
            //XReactDataBuilder.Time = File.GetLastWriteTime(file);
        }
        private void SaveResFile(string name)
        {
            string assetPath = string.Format("{0}Config/ReactPackage/{1}.asset",
                LoadMgr.singleton.editorResPath, name);
            ResRedirectConfig srcAsset = null;
            if (File.Exists(assetPath))
            {
                srcAsset = AssetDatabase.LoadAssetAtPath<ResRedirectConfig>(assetPath);
            }
            else if (mainWid.resMap.Count > 0)
            {
                srcAsset = ScriptableObject.CreateInstance<ResRedirectConfig>();
                srcAsset.name = name;
            }
            if (srcAsset != null)
            {
                srcAsset.resPath.Clear();
                foreach (var res in mainWid.resMap)
                {

                    srcAsset.resPath.Add(new ResPathRedirect()
                    {
                        name = Path.GetFileNameWithoutExtension(res.Key),
                        ext = Path.GetExtension(res.Key),
                        physicPath = res.Value
                    });
                }
                EditorCommon.CreateAsset<ResRedirectConfig>(assetPath, ".asset", srcAsset);
            }
        }

        private string GetDataFileWithPath()
        {
            return ReactDataSet.ReactDataExtra.ScriptPath + ReactDataSet.ReactDataExtra.ScriptFile + ".bytes";
        }

        private void StripData(XReactData data)
        {
            if (string.IsNullOrEmpty(data.Name)) data.Name = null;
            if (string.IsNullOrEmpty(data.ClipName)) data.ClipName = null;

            ///////////////////////////////////////////////////
            if (data.Audio != null && data.Audio.Count > 0)
            {
                foreach (XAudioData a in data.Audio)
                {
                    if (string.IsNullOrEmpty(a.Clip)) a.Clip = null;
                }
            }
            else
                data.Audio = null;
            ///////////////////////////////////////////////////
            if (data.Fx != null && data.Fx.Count > 0)
            {
                foreach (XFxData f in data.Fx)
                {
                    if (string.IsNullOrEmpty(f.Fx)) f.Fx = null;
                    if (string.IsNullOrEmpty(f.Bone)) f.Bone = null;
                    if (!f.Alone)
                    {
                        f.RotX = 0;
                        f.RotY = 0;
                        f.RotZ = 0;
                    }
                }
            }
            else
                data.Fx = null;
            ///////////////////////////////////////////////////
            if (data.BoneShakeData != null && data.BoneShakeData.Count > 0)
            {
                foreach (XBoneShakeData b in data.BoneShakeData)
                {
                    if (string.IsNullOrEmpty(b.Bone)) b.Bone = null;
                }
            }
        }

        #endregion

        #region hotLoad

        public void RefreshHost()
        {
            var ReactHoster = GameObject.FindObjectOfType<XReactEntity>();
            if (ReactHoster != null)
            {
                ReactHoster.ReactDataSet = ReactDataSet;
                XReactDataHostBuilder.HotBuildEx(ReactDataSet.ConfigData, ref ReactDataSet);
            }
        }
        public void HotLoad()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("tips", "需启动Unity后才能Hot", "ok");
                return;
            }
            CLoad(LastScript);
            if (ReactDataSet != null && isResScene)
            {
                    IsHot = XReactDataHostBuilder.singleton.HotLoad(ReactDataSet);

                    GameObject camera = GameObject.Find("Main Camera").gameObject;
                    if (XReactDataHostBuilder.hoster != null)
                    {
                        camera.transform.position = XReactDataHostBuilder.hoster.transform.position + new Vector3(0f, 1.5f, -5f);
                        XReactDataHostBuilder.hoster.transform.eulerAngles = new Vector3(0f, -180f, 0f);
                    }
            }
        }

        void NewScene()
        {
            Scene scene = EditorSceneManager.GetActiveScene();

            if (!string.IsNullOrEmpty(scene.name) && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                isResScene = false;
            }
            else
            {

                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.name = "Ground";
                plane.layer = LayerMask.NameToLayer("Terrain");
                plane.transform.position = new Vector3(0, -0.01f, 0);
                plane.transform.localScale = new Vector3(1, 1, 1);
                plane.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                //plane.GetComponent<Renderer>().sharedMaterial.SetColor("_Color", new Color(90 / 255.0f, 90 / 255.0f, 90 / 255.0f));
                Texture icon = AssetDatabase.LoadAssetAtPath("Assets/Editor/React/ReactRes/CalibrationFloor.png", typeof(Texture)) as Texture;
                if (icon != null)
                    plane.GetComponent<Renderer>().sharedMaterial.SetTexture("_BaseMap", icon);


                GameObject.DestroyImmediate(GameObject.Find("Main Camera"));
                GameObject go = AssetDatabase.LoadAssetAtPath("Assets/Editor/EditorResources/MainCamera.prefab", typeof(GameObject)) as GameObject;
                GameObject camera = GameObject.Instantiate<GameObject>(go, null);
                camera = camera.transform.Find("Main Camera").gameObject;
                camera.name = "Main Camera";


                GameObject cam = new GameObject("cam");
                GameObject swivel = new GameObject("swivel"); swivel.transform.SetParent(cam.transform);
                GameObject stick = new GameObject("stick"); stick.transform.SetParent(swivel.transform);

                cam.transform.position = Vector3.zero;
                swivel.transform.localPosition = Vector3.zero;
                stick.transform.localPosition = Vector3.zero;
                camera.transform.SetParent(stick.transform);

                isResScene = true;
            }
            if (!isResScene)
            {
                Debug.LogError("Load failed..");
            }
        }

    }


        #endregion

}
#endif