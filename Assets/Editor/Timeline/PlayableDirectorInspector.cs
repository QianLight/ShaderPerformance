using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CFEngine;
using CFEngine.Editor;
using CFUtilPoolLib;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace XEditor
{

    [CustomEditor (typeof (PlayableDirector))]
    public partial class PlayableDirectorInspector : Editor
    {
        private PlayableDirector _director = null;
        private string _config_path = string.Empty;
        private Dictionary<string, PlayableBaseEditor> _map = null;
        private string timelineName = "";
        public static bool loaded = false;
        public static string scene = string.Empty;
        private bool debugFolder = false;

        static EditorDirectorBindingData _data = new EditorDirectorBindingData ();

        public EditorDirectorBindingData EdtData => _data;

        public DirectorBindingData data => _data.data;

        public PlayableDirector Director { get { return _director; } }

        public bool isOrignal
        {
            get
            {
                if (_director != null && _director.playableAsset != null)
                {
                    return _director.playableAsset.name.StartsWith ("Orignal_");
                }
                return false;
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnEditorReload()
        {
            loaded = false;

            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                int cnt = canvas.transform.childCount;
                for (int i = cnt - 1; i >= 0; i--)
                {
                    var tf = canvas.transform.GetChild(i);
                    if (tf.gameObject.CompareTag("Timeline"))
                    {
                        GameObject.DestroyImmediate(tf.gameObject);
                    }

                }
            }
        }

        private void OnEnable ()
        {
            if (EngineContext.IsRunning)
                return;
            _director = target as PlayableDirector;

            if (_director != null && _director.playableAsset == null)
                return;
            EngineContext.director = _director;
            if (_director.playableAsset is TimelineAsset)
                DirectorHelper.timelineAsset = _director.playableAsset;

            _map = new Dictionary<string, PlayableBaseEditor> ();
            CreateTimeline.TimlineInit ();
            if (!isOrignal)
            {
                DirectorHelper.getAnimBindingObj = GetAnimBindingObj;
                CheckNull ();
                if (RTimeline.timelineEditorData == null)
                {
                    RTimeline.LoadTimelineConfig (timelineName);
                    if (RTimeline.timelineEditorData == null)
                    {
                        RTimeline.timelineEditorData = TimelineEditorData.CreateInstance<TimelineEditorData> ();
                    }
                    RTimeline.SyncCameraPos ();
                }
                else
                {
                    RTimeline.SyncCameraPos (false);
                }
            }
        }

        private void OnDisable ()
        {
            if (EngineContext.IsRunning)
                return;
            if (!isOrignal)
            {
                _data?.OnLoad (target as PlayableDirector);
            }
        }

        private void CheckNull ()
        {
            _director = target as PlayableDirector;
            timelineName = "empty";
            if (_director.playableAsset != null)
            {
                timelineName = _director.playableAsset.name;
                _config_path = string.Format ("Timeline/{0}", timelineName);
            }

            if (!loaded)
            {
                try
                {
                    loaded = true;
                    BeforeLoad ();
                    Reset ();
                    ParseFromFile ();
                    AfterLoad ();
                }
                catch (System.Exception e)
                {
                    DebugLog.AddErrorLog (e.StackTrace);
                }
            }
            if (string.IsNullOrEmpty (scene))
            {
                string path = EditorSceneManager.GetActiveScene ().path;
                int index = path.LastIndexOf (".");
                if (index >= 0)
                    path = path.Remove (index);
                scene = path;
            }
            if (DirectorHelper.singleton.cine == null)
            {
                GameObject go = GameObject.Find ("cineroot");
                DirectorHelper.singleton.cine = go;
            }
        }

        private void TimelineAssetGUI ()
        {
            if (DirectorHelper.GetDirector () != null)
            {
                CheckNull ();
            }
            GUILayout.Space (8);
            GUILayout.Label ("Director Binding Config", EditorStyles.boldLabel);

            GUILayout.Label ("Name:  " + timelineName);
            GUILayout.Label ("Path:  " + _config_path);
            GUILayout.Label ("Scene: " + scene.Replace ("Assets/Scenes/", string.Empty));
            data.cameraInitialLerp = GUILayout.Toggle (data.cameraInitialLerp, "   Start With Camera Lerp");
#if TIMELINEDEBUG
            DirectorBindingData.Pos = EditorGUILayout.Vector3Field ("offset-pos: ", DirectorBindingData.Pos);
            Vector3 angle = DirectorBindingData.Rot.eulerAngles;
            DirectorBindingData.Rot = Quaternion.Euler (EditorGUILayout.Vector3Field ("offset-rot: ", angle));
#endif
            if (data.cameraInitialLerp)
            {
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField ("        camera lerp time");
                if (data.lerpTime <= 0.01f) data.lerpTime = 0.3f;
                data.lerpTime = EditorGUILayout.FloatField (data.lerpTime);
                EditorGUILayout.EndHorizontal ();
                if (data.lerpTime < 0.01f)
                {
                    EditorGUILayout.HelpBox ("camera lerp time can not too short", MessageType.Error);
                }
            }
            data.isBreak = GUILayout.Toggle (data.isBreak, "   Enable Break");
            data.enableBGM = GUILayout.Toggle (data.enableBGM, "   Enable BGM");
            data.hideUI = GUILayout.Toggle (data.hideUI, "   Hide UI");
            data.endAudioParam=EditorGUILayout.IntField("  Audio End Param",data.endAudioParam);
            FinishMode mode = (FinishMode)EditorGUILayout.EnumPopup("  FinishMode", (FinishMode)data.finishmode);
            data.finishmode = (int) mode;
            GUILayout.BeginHorizontal ();
            GUILayout.Label ("hide layer");
            data.hidelayer = EditorGUILayout.MaskField (data.hidelayer, GameObjectLayerHelper.hideLayerStr);
            GUILayout.EndHorizontal ();
            if (RTimeline.timelineEditorData != null)
            {
                var tdd = RTimeline.timelineEditorData;
                GUILayout.BeginHorizontal ();
                EditorGUILayout.ObjectField ("camerInitPos", RTimeline.camerInit, typeof (GameObject), true);
                GUILayout.EndHorizontal ();

                EditorGUI.BeginChangeCheck ();
                GUILayout.BeginHorizontal ();
                tdd.cameraStartPos = EditorGUILayout.Vector3Field ("pos", tdd.cameraStartPos);
                GUILayout.EndHorizontal ();
                GUILayout.BeginHorizontal ();
                tdd.cameraStartRot = EditorGUILayout.Vector3Field ("rot", tdd.cameraStartRot);
                GUILayout.EndHorizontal ();
                if (EditorGUI.EndChangeCheck ())
                {
                    if (RTimeline.camerInit != null)
                    {
                        var trans = RTimeline.camerInit.transform;
                        trans.position = tdd.cameraStartPos;
                        tdd.cameraStartRot = trans.eulerAngles;
                    }
                }
            }

            // data.scene = scene;
            GUIItems ();
            GUILayout.BeginHorizontal ();
            if (GUILayout.Button ("load"))
            {
                DirectorHelper.Stop ();
                if (DirectorHelper.timelineAsset is TimelineAsset)
                    _director.playableAsset = DirectorHelper.timelineAsset;
                DoLoad ();
            }
            if (GUILayout.Button ("loadFromDisk"))
            {
                DirectorHelper.Stop ();
                if (DirectorHelper.timelineAsset is TimelineAsset)
                    _director.playableAsset = DirectorHelper.timelineAsset;
                DoLoad (true);
            }

            if (GUILayout.Button ("save"))
            {
                DoSave (false);
            }
            if (GUILayout.Button ("save as"))
            {
                DoSave ();
            }
            GUILayout.EndHorizontal ();
        }

        private void Display()
        {
            var director = EngineContext.director;
            if (director == null) OnEnable();
            if (director != null && director.playableAsset != null)
            {
                foreach (PlayableBinding pb in director.playableAsset.outputs)
                {
                    if (pb.sourceObject is IDisplayTrack)
                    {
                        (pb.sourceObject as IDisplayTrack).GUIDisplayName();
                    }
                }
            }
        }

        private void OptimizeBindings()
        {
            var dir = target as PlayableDirector;
            if (dir?.playableAsset)
            {
                Dictionary<Object, Object> map = new Dictionary<Object, Object>();
                foreach (var pb in dir.playableAsset.outputs)
                {
                    var key = pb.sourceObject;
                    if (key != null)
                    {
                        var v = dir.GetGenericBinding(key);
                        if (v) map.Add(key, v);
                    }
                }
                BeforeLoad();
                foreach (var it in map)
                {
                    dir.SetGenericBinding(it.Key, it.Value);
                }
                dir.RebuildGraph();
            }
        }

        private void OptimizeRenferences()
        {
            var dir = target as PlayableDirector;
            TimelineAsset timeline = dir.playableAsset as TimelineAsset;
            IEnumerable<TrackAsset> tracks = timeline.GetOutputTracks();

            Dictionary<PropertyName, Object> map = new Dictionary<PropertyName, Object>();
            foreach (var track in tracks)
            {
                var clips = track.GetClips();
                foreach (var clip in clips)
                {
                    foreach (FieldInfo fieldInfo in clip.asset.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(ExposedReference<>))
                        {
                            PropertyName oldExposedName = (PropertyName)fieldInfo.FieldType.GetField("exposedName").GetValue(fieldInfo.GetValue(clip.asset));
                            Object oldExposedValue = dir.GetReferenceValue(oldExposedName, out var isValid);
                            if (!isValid) continue;
                            map.Add(oldExposedName, oldExposedValue);
                        }
                    }
                }
            }
            // clean all
            SerializedProperty sp = serializedObject.FindProperty("m_ExposedReferences.m_References");
            if (sp != null)
            {
                sp.arraySize = 0;
                serializedObject.ApplyModifiedProperties();
            }

            foreach (var it in map)
            {
                dir.SetReferenceValue(it.Key, it.Value);
            }
            dir.RebuildGraph();
        }
 

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Display();
            if (EngineContext.IsRunning)
                return;
            _director = target as PlayableDirector;
            if (_director.playableAsset == null)
                return;
            if (isOrignal)
            {
                EditorStateHelper.SetupEdit(_director);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("OptimizeBindings")) OptimizeBindings();
                if (GUILayout.Button("OptimizeReferences")) OptimizeRenferences();
                GUILayout.EndHorizontal();
                return;
            }
            TimelineAssetGUI();

            debugFolder = EditorGUILayout.Foldout(debugFolder, "debug");
            if (debugFolder)
            {
                if (_director.playableAsset != null)
                {
                    GUILayout.BeginHorizontal();

                    var state = DirectorHelper.GetPlayState();
                    if (state == DirectPlayState.Playing)
                    {
                        if (GUILayout.Button("pause"))
                        {
                            DirectorHelper.Pause();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("play"))
                        {
                            if (DirectorHelper.timelineAsset is TimelineAsset)
                                DirectorAsset.instance.name = DirectorHelper.timelineAsset.name;
                            DirectorHelper.Play();
                        }
                    }
                    if (GUILayout.Button("reset"))
                    {
                        DirectorHelper.Reset();
                    }
                    if (GUILayout.Button("unload"))
                    {
                        DirectorHelper.Stop();
                        if (DirectorHelper.timelineAsset is TimelineAsset)
                            _director.playableAsset = DirectorHelper.timelineAsset;
                        RTimeline.UnloadTmpObject();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("time:{0:F3}", _director.time));
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("open"))
                {
                    CreateTimeline.LoadTimeline(false);
                }
                GUILayout.EndHorizontal();
            }
        }

        private void GUIItems ()
        {
            if (_map == null) OnEnable ();
            if (_director.playableAsset != null)
            {
                int index = 0;
                foreach (PlayableBinding pb in _director.playableAsset.outputs)
                {
                    PlayableBaseEditor editor = null;
                    if (!_map.TryGetValue (pb.streamName, out editor))
                    {
                        switch (pb.sourceObject)
                        {
                            case AnimationTrack _:
                                editor = new PlayableAnimEditor (index, this);
                                break;
                            case ActivationTrack _:
                                editor = new PlayableActivationEditor (index, this);
                                break;
                            case ControlTrack _:
                                editor = new PlayableControlEditor (index, this);
                                break;
                        }

                        if (editor != null)
                            _map.Add (pb.streamName, editor);
                    }
                    if (editor != null)
                    {
                        editor.editor = this;
                        editor.OnInspectorGUI (pb);
                    }
                    GUILayout.Space (2);
                    index++;
                }
            }
        }

        private void Reset ()
        {
            if (_map != null)
            {
                foreach (var item in _map)
                {
                    item.Value.Reset ();
                }
            }
        }

        private void DoSave (bool saveas = true)
        {
            if (RTimeline.timelineEditorData != null)
            {
                data.cameraInitialPos = RTimeline.timelineEditorData.cameraStartPos;
                data.cameraInitialRot = RTimeline.timelineEditorData.cameraStartRot;
            }
            var timelineAsset = _director.playableAsset;
            if (timelineAsset != null)
            {
                string name = _director.playableAsset.name;
                string path = AssetDatabase.GetAssetPath (timelineAsset);
                if (path.StartsWith ("Assets/"))
                {
                    string save_path = "";
                    if (saveas)
                    {
                        save_path = EditorUtility.SaveFilePanel ("Select timeline config file",
                            OriginalSetting.LIB, _director != null ? name : "new", "playable");
                        save_path = save_path.Replace ("\\", "/");
                        int index = save_path.IndexOf (OriginalSetting.LIB);
                        if (index >= 0)
                        {
                            save_path = save_path.Substring (index);
                            name = save_path.Replace (".playable", "");
                        }
                        else
                        {
                            DebugLog.AddErrorLog2 ("save path error:{0}", save_path);
                            return;
                        }
                    }
                    else
                    {
                        save_path = string.Format ("{0}{1}.playable", OriginalSetting.LIB, name);
                    }
                    if (string.IsNullOrEmpty (save_path)) return;

                    //cinemachine
                    SaveCine (save_path);
                    //save playable asset
                    string newName = save_path.Substring (save_path.LastIndexOf ("/") + 1);
                    if (path.StartsWith (OriginalSetting.LIB))
                    {
                        //in timleline folder     
                        string nameWithExt = name + ".playable";
                        if (newName == nameWithExt)
                        {
                            //same asset
                            CommonAssets.SaveAsset (timelineAsset);
                            _director.playableAsset = timelineAsset;
                        }
                        else
                        {
                            //rename
                            AssetDatabase.CopyAsset (path, save_path);
                            AssetDatabase.SaveAssets ();
                            AssetDatabase.Refresh ();
                            _director.playableAsset = AssetDatabase.LoadAssetAtPath<TimelineAsset> (save_path);
                        }
                    }
                    else
                    {
                        //new asset, move to timeline folder
                        AssetDatabase.RenameAsset (path, newName);
                        AssetDatabase.SaveAssets ();
                        AssetDatabase.Refresh ();
                    }
                    DoLoad ();
                    DirectorHelper.timelineAsset = _director.playableAsset;
                    //save record animation
                    TimelineAssetTool.ExportVCRecord ();
                    //save bytes
                    Save (save_path);
                    //save editor data
                    RTimeline.timelineEditorData.defaultSceneName = scene;
                    string editorDataPath = AssetDatabase.GetAssetPath (RTimeline.timelineEditorData);
                    if (editorDataPath.StartsWith ("Assets/"))
                    {
                        if (RTimeline.timelineEditorData.name == name)
                        {
                            CommonAssets.SaveAsset (RTimeline.timelineEditorData);
                        }
                        else
                        {
                            save_path = save_path.Replace (".playable", ".asset");
                            AssetDatabase.CopyAsset (editorDataPath, save_path);
                            AssetDatabase.SaveAssets ();
                            AssetDatabase.Refresh ();
                        }
                    }
                    else
                    {
                        save_path = save_path.Replace (".playable", ".asset");
                        RTimeline.timelineEditorData.name = name;
                        CommonAssets.CreateAsset<TimelineEditorData> (save_path, ".asset", RTimeline.timelineEditorData);
                    }
                    EditorUtility.DisplayDialog ("warning", "save success !", "ok");
                }
                else
                {
                    DebugLog.AddErrorLog ("asset is not timelineAsset");
                }
            }

        }

        private void SaveCine (string path)
        {
            if (data.useCine)
            {
                GameObject go = GameObject.Find ("cineroot");
                if (go != null && !string.IsNullOrEmpty (path))
                {
                    path = path.Replace (".playable", ".prefab");
                    bool success = false;
                    GameObject prefab = PrefabUtility.SaveAsPrefabAsset (go, path, out success);
                    if (success) Selection.activeObject = prefab;
                }
            }
        }

        private string GetConfigName (string path)
        {
            FileInfo file = new FileInfo (path);
            return file.Name.Substring (0, file.Name.LastIndexOf ('.'));
        }

        private void BeforeLoad ()
        {
            SerializedProperty sp = serializedObject.FindProperty ("m_SceneBindings");
            if (sp != null)
            {
                sp.arraySize = 0;
                serializedObject.ApplyModifiedProperties ();
            }
        }
        private void AfterLoad ()
        {
            if (_director.playableAsset != null)
            {
                _director.RebuildGraph ();
                DirectorHelper.PostBindTrack ();
            }
        }
        private void DoLoad (bool fromDisk = false)
        {
            if (!string.IsNullOrEmpty (_config_path))
            {
                BeforeLoad ();
                foreach (PlayableBinding pb in _director.playableAsset.outputs)
                {
                    if (_map.ContainsKey (pb.streamName))
                    {
                        _map[pb.streamName].OnLoad (pb);
                    }
                }
                // TimelineAssetTool.ExportVCRecord ();
                if (fromDisk)
                {
                    ParseFromFile ();
                }
                else
                {
                    _data.OnLoad (_director);
                    Save ("", true, SaveToMemoryFinish);
                }
                AfterLoad ();
            }
        }

        private void ParseFromFile ()
        {
            Reset ();
            string filePath = string.Format ("{0}/{1}.bytes",
                AssetsConfig.instance.ResourcePath, _config_path);
            if (File.Exists (filePath))
            {
                RTimeline.Load (_config_path);
            }
            _data.OnParse ();
        }

        private bool CheckCustomCameraTrack ()
        {
            var director = target as PlayableDirector;
            var list = director.playableAsset.outputs;
            return list.Any (x => x.sourceObject is CameraPlayableTrack);
        }

        delegate void SaveFinishCb (MemoryStream stream);
        private void SaveToMemoryFinish (MemoryStream stream)
        {
            byte[] buffer = stream.GetBuffer ();
            CFBinaryReader reader = CFBinaryReader.Get ();
            reader.InitByte (buffer, 0, (int) stream.Length, false);
            DirectorHelper.singleton.PreLoad ();
            //copy str
            for (int i = 0; i < DirectorHelper.stringList.Count; ++i)
            {
                DirectorHelper.singleton.strings[i] = DirectorHelper.stringList[i].str;
            }
            DirectorHelper.singleton.Load (reader, _director.playableAsset.name);
            CFBinaryReader.Return (reader);
        }
        public bool GetAnimBindingObj (string streamName, out string path)
        {
            path = "";
            DirectorAnimBinding bind = EdtData.GetNewAnimBinding (streamName);
            if (bind.type == PlayableAnimType.VCamera)
            {
                var cineRoot = GameObject.Find ("cineroot");
                if (cineRoot)
                {
                    var obj = cineRoot.transform.GetChild ((int) bind.val);
                    if (obj != null)
                        path = string.Format ("Assets/BundleRes/Animation/Main_Camera/{0}/{1}.anim", _director.playableAsset.name, obj.name);
                    return obj != null;
                }
            }
            else if (bind.type == PlayableAnimType.Entity)
            {
                path = string.Format ("Assets/BundleRes/Animation/Main_Camera/{0}/{1}.anim", _director.playableAsset.name, bind.val.ToString ());
                return true;
            }
            else if (bind.type == PlayableAnimType.Player)
            {
                path = string.Format ("Assets/BundleRes/Animation/Main_Camera/{0}/101.anim", _director.playableAsset.name);
                return true;
            }
            else if (bind.type == PlayableAnimType.Dummy)
            {
                string prefix = "dummy";
                if (bind.type == PlayableAnimType.Dummy) prefix = "maindummy";
                path = string.Format ("Assets/BundleRes/Animation/Main_Camera/{0}/1}.anim", _director.playableAsset.name, prefix);
                return true;
            }
            return false;
        }

        private void Save (string path, bool toMemory = false, SaveFinishCb cb = null)
        {
            var timelineAsset = _director.playableAsset as TimelineAsset;
            if (timelineAsset != null)
            {
                FetchUsedAssets ();

                if (toMemory)
                {
                    using (MemoryStream ms = new MemoryStream (1024 * 1024))
                    {
                        BinaryWriter bw = new BinaryWriter (ms);
                        WritePreload (null);
                        DirectorHelper.Save (timelineAsset, bw);
                        if (cb != null)
                        {
                            ms.Position = 0;
                            cb (ms);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty (path))
                {
                    path = path.Replace (".playable", ".bytes");
                    using (FileStream fs = new FileStream (path, FileMode.Create))
                    {
                        BinaryWriter bw = new BinaryWriter (fs);
                        WritePreload (bw);
                        RTimeline.timelineEditorData.headLength = (int) bw.BaseStream.Length;
                        _data.WriteToFile (_director, bw);
                        DirectorHelper.Save (timelineAsset, bw);
                    }
                }
            }
            else
            {
                DebugLog.AddErrorLog ("not valid timeline asset");
            }
        }

    }

}