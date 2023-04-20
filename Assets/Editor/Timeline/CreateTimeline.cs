using System;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using CFEngine.Editor;
using Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace XEditor
{

    public class CreateTimeline
    {

        [MenuItem("XEditor/Timeline/Origin", priority = 1)]
        public static void FindOrLoadTimelineGameobject()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                var go = cam.gameObject;
                TimelineTool tool = go.GetComponent<TimelineTool>();
                if (tool != null) GameObject.DestroyImmediate(tool);
                PlayableDirector dir = cam.GetComponent<PlayableDirector>();
                if (dir != null) GameObject.DestroyImmediate(dir);
                CinemachineBrain brain = cam.GetComponent<CinemachineBrain>();
                if (brain == null) go.AddComponent<CinemachineBrain>();
                EnvironmentExtra env = cam.GetComponent<EnvironmentExtra>();
                env.loadGameAtHere = false;
            }
            CleanEnv();
            LoadTimelineObject();
        }

        private static void LoadTimelineObject()
        {
            var audio = GameObject.FindObjectOfType<FmodPlayableUtils>();
            if (audio == null)
            {
                var xg = new GameObject(OriginalSetting.str_audio);
                xg.AddComponent<FmodPlayableUtils>();
            }
            GameObject go = GameObject.Find("Canvas");
            if (go == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/Timeline/res/Timeline.prefab");
                go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                go.name = "Canvas";
            }
        }

        [MenuItem("XEditor/Timeline/Create", priority = 1)]
        public static void MakeTimeline()
        {
            var director = EngineContext.director;
            if (director != null)
            {
                var oldAsset = director.playableAsset;
                string path = AssetDatabase.GetAssetPath(oldAsset);
                if (path.StartsWith("Assets/"))
                {
                    if (EditorUtility.DisplayDialog("warning", "Save Timeline asset?", "OK", "Cancel"))
                    {
                        CommonAssets.SaveAsset(oldAsset);
                    }
                }
                TimelineAsset asset = TimelineAsset.CreateInstance<TimelineAsset>();
                asset.name = "tmpTimelineAsset";
                asset.editorSettings.fps = 30.0f;
                asset = CommonAssets.CreateAsset<TimelineAsset>(OriginalSetting.LIB + asset.name + ".playable", ".playable", asset);
                DirectorHelper.timelineAsset = asset;
                director.playableAsset = asset;
                director.RebuildGraph();
                CheckScene(director);
                MakeCineRoot();
            }
            else
            {
                DebugLog.AddErrorLog("no EngineContext,scene not init");
            }
        }

        private static PlayableDirector FindDirector()
        {
            GameObject go = GameObject.Find("Main Camera");
            if (go != null)
            {
                PlayableDirector director;
                director = go.GetComponent<PlayableDirector>();
                if (director == null)
                {
                    director = go.AddComponent<PlayableDirector>();
                }
                return director;
            }
            else
            {
                EditorUtility.DisplayDialog("error", "There is not camera in the scene that you selectd", "ok");
            }
            return null;
        }

        [MenuItem("XEditor/Timeline/CreateFromScene", priority = 1)]
        public static void MakeTimelineFromScene()
        {
            string scene = OpenScene();
            if (!string.IsNullOrEmpty(scene))
            {
                TimlineInit();
                PlayableDirector director = FindDirector();
                if (director != null)
                {
                    TimelineAsset asset = TimelineAsset.CreateInstance<TimelineAsset>();
                    asset.name = "tmpTimelineAsset";
                    asset.editorSettings.fps = 30.0f;
                    asset = CommonAssets.CreateAsset<TimelineAsset>(OriginalSetting.LIB + asset.name + ".playable", ".playable", asset);
                    DirectorHelper.timelineAsset = asset;
                    director.playableAsset = asset;
                    director.RebuildGraph();
                    CheckScene(director);
                    MakeCineRoot();
                }
            }
        }
        public static void TimlineInit()
        {
            RTimeline.InitEditor();
            RTimeline.InitTimelineGo();
            RTimeline.animationTrackOnLoad = PlayableAnimEditor.OnLoad;
            RTimeline.controlTrackOnLoad = PlayableControlEditor.OnLoad;
            //TimelineTool.AddTimelineTool();
        }
        public static void LoadTimeline(bool openScene)
        {
            string path = EditorUtility.OpenFilePanel("Select timeline config file", OriginalSetting.LIB, "playable");
            if (!string.IsNullOrEmpty(path))
            {
                FileInfo info = new FileInfo(path);
                string name = info.Name.Split('.')[0];
                if (openScene)
                {
                    LoadConfigScene(name);
                    CleanEnv();
                    TimlineInit();
                }
                PlayableDirector director = FindDirector();
                GameObject go = GameObject.Find("Main Camera");
                if (director != null)
                {
                    string timelineAssetPath = string.Format("{0}/Timeline/{1}.playable", AssetsConfig.instance.ResourcePath, name);
                    director.playableAsset = AssetDatabase.LoadAssetAtPath<PlayableAsset>(timelineAssetPath);
                    DirectorHelper.timelineAsset = director.playableAsset;
                    director.RebuildGraph();
                    Selection.activeGameObject = go;
                    CheckScene(director);
                }
                else
                {
                    DebugLog.AddErrorLog("no camera");
                }
            }
        }

        [MenuItem("XEditor/Timeline/Open", priority = 2)]
        public static void OpenTimeline()
        {
            LoadTimeline(false);
        }

        [MenuItem("XEditor/Timeline/OpenFromScene", priority = 2)]
        public static void OpenTimelineByNew()
        {
            LoadTimeline(true);
        }

        [MenuItem("XEditor/Timeline/SelectDirector _F4", priority = 3)]
        public static void FocusDirector()
        {
            var dir = GameObject.FindObjectOfType<PlayableDirector>();
            if (dir != null)
            {
                Selection.activeGameObject = dir.gameObject;
            }
            else
            {
                EditorUtility.DisplayDialog("tip", "not found timeline in current scene", "ok");
            }
        }

        private static void LoadConfigScene(string name)
        {
            RTimeline.LoadTimelineConfig(name);
            var ted = RTimeline.timelineEditorData;
            if (ted != null && !string.IsNullOrEmpty(ted.defaultSceneName))
            {
                EditorSceneManager.OpenScene(ted.defaultSceneName + ".unity");
            }
        }

        private static string OpenScene()
        {
            string dep = "Assets/Scenes";
            string file = EditorUtility.OpenFilePanel("Select unity scene file", dep, "unity");
            string _scene = string.Empty;
            if (file.Length != 0)
            {
                file = file.Remove(file.LastIndexOf("."));
                _scene = file.Remove(0, file.IndexOf(dep));
                EditorSceneManager.OpenScene(_scene + ".unity");
            }
            return _scene;
        }

        private static void CheckScene(PlayableDirector director)
        {
            if (GlobalContex.ee != null)
            {
                GlobalContex.ee.loadGameAtHere = false;
                GlobalContex.ee.forceIgnore = true;
            }
            PlayableDirectorInspector.loaded = false;
            var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            PlayableDirectorInspector.scene = s.path.Replace(".unity", "");
            director.timeUpdateMode = DirectorUpdateMode.GameTime;
            director.enabled = true;
            SetupCinemachine();
            LoadTimelineObject();
            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
        }

        private static void MakeCineRoot()
        {
            string name = "cineroot";
            if (!GameObject.Find(name))
            {
                GameObject go = new GameObject(name);
                if (RTimeline.timelineRoot != null)
                {
                    go.transform.parent = RTimeline.timelineRoot.transform;
                }
                go.tag = "Timeline";
                DirectorHelper.singleton.cine = go;
            }
        }

        private static void SetupCinemachine()
        {
            CinemachineBrain[] brains = UnityEngine.Object.FindObjectsOfType(
                typeof(CinemachineBrain)) as CinemachineBrain[];
            if (brains == null || brains.Length == 0)
            {
                Camera cam = Camera.main;
                if (cam == null)
                {
                    Camera[] cams = UnityEngine.Object.FindObjectsOfType(
                        typeof(Camera)) as Camera[];
                    if (cams != null && cams.Length > 0)
                        cam = cams[0];
                }
                if (cam != null)
                {
                    Undo.AddComponent<CinemachineBrain>(cam.gameObject);
                }
            }
        }

        [MenuItem("XEditor/Timeline/CleanEnv", priority = 1)]
        public static void CleanEnv()
        {
            GameObject go = GameObject.Find("_TimelineTmp");
            if (go != null) GameObject.DestroyImmediate(go);
            go = GameObject.Find("_TimelineCamerInit");
            if (go != null) GameObject.DestroyImmediate(go);
            var goes = GameObject.FindGameObjectsWithTag("Timeline");
            for (int i = 0; i < goes.Length; i++)
            {
                XEditorUtil.RemoveInstHierarchy(goes[i]);
            }
            go = GameObject.Find(OriginalSetting.str_ch);
            if (go != null) GameObject.DestroyImmediate(go);
            go = GameObject.Find(OriginalSetting.str_fx);
            if (go != null) GameObject.DestroyImmediate(go);

            CleanSceneActors();
        }

        private static void CleanSceneActors()
        {
            var actors = GameObject.FindObjectsOfType<Animator>();
            if (!OrignalEntrainWindow.debugMode)
            {
                foreach (var ac in actors)
                {
                    var go = ac.gameObject;
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(go) && !IsSceneAsset(go.transform))
                    {
                        if (go.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                        {
                            Debug.Log("timeline clean env actor: " + go.name);
                            try
                            {
                                GameObject.DestroyImmediate(go);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("destroy failed: " + go.name + " message:" + e.Message);
                            }
                        }
                    }
                }
            }
        }

        private static bool IsSceneAsset(Transform tf)
        {
            while(tf.parent)
            {
                tf = tf.parent;
            }
            return tf.name.Trim() == "EditorScene";
        }
    }

    public partial class BuildTimeline : PreBuildPreProcess
    {
        public override string Name { get { return "Timeline"; } }

        public override int Priority
        {
            get
            {
                return 3;
            }
        }
        public override void PreProcess ()
        {
            base.PreProcess ();
//#if !UNITY_ANDROID
            if (build)
            {

                string dir = "Assets/StreamingAssets/Bundles/assets/bundleres/timeline";

                DirectoryInfo targetDi = new DirectoryInfo (dir);
                if (targetDi.Exists)
                {
                    targetDi.Delete (true);
                }
                AssetDatabase.SaveAssets ();
                AssetDatabase.Refresh ();
            }

            List<string> path = new List<string> ();
            DirectoryInfo di = new DirectoryInfo (string.Format ("{0}/Timeline", AssetsConfig.instance.ResourcePath));
            FileInfo[] files = di.GetFiles ("*.bytes", SearchOption.TopDirectoryOnly);
            if (files != null && files.Length > 0)
            {
                for (int i = 0; i < files.Length; ++i)
                {
                    var fi = files[i];
                    string filename = fi.Name.Replace (".bytes", "").ToLower ();
                    path.Add (filename);
                    string filePath = string.Format ("Timeline/{0}", fi.Name);
                    filePath = filePath.ToLower ();
                    CopyFile (filePath, filePath);
                }
            }
            if (build)
            {
                try
                {
                    string configPath = string.Format ("{0}/Config/TimelineList.bytes", AssetsConfig.instance.ResourcePath);

                    using (FileStream fs = new FileStream (configPath, FileMode.Create))
                    {
                        BinaryWriter bw = new BinaryWriter (fs);
                        short count = (short) path.Count;
                        bw.Write (count);
                        for (int i = 0; i < path.Count; ++i)
                        {
                            bw.Write (path[i]);
                            int headLength = 0;
                            bw.Write (headLength);
                        }
                    }
                    AssetDatabase.ImportAsset (configPath, ImportAssetOptions.ForceUpdate);
                }
                catch (Exception e)
                {
                    DebugLog.AddErrorLog (e.Message);
                }
            }
//#endif
            }

        }
}