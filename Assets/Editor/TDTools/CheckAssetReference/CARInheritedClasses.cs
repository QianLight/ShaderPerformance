using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.Timeline;
using System;
using System.IO;
using XEditor;
using LevelEditor;
using BluePrint;


namespace TDTools
{
    [Serializable]
    public class TimelineConfigAsset : ConfigAsset
    {
        [NonSerialized] OCSData ocs;
        [NonSerialized] readonly string ocsPath = "Assets/Editor/Timeline/res/OriginalCSData.asset";

        public TimelineConfigAsset () : base () 
        {
            
        }

        public TimelineConfigAsset (ConfigAssetEnum type,
                                    string name,
                                    int depth,
                                    int id,
                                    string i_path) : base (type, name, depth, id)
        {
            path = i_path;
            if (ocs == null)
                ocs = AssetDatabase.LoadAssetAtPath<OCSData> (ocsPath);
        }

        public override void Open ()
        {
            if (ocs == null)
                ocs = AssetDatabase.LoadAssetAtPath<OCSData> (ocsPath);

            if (!path.EndsWith (".playable"))
            {
                try
                {
                    System.Diagnostics.Process.Start (Application.dataPath + "/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
            else
            {
                try
                {
                    LoadTimeline ();
                }
                catch (IOException e)
                {
                    Debug.Log ($"{e.GetType().Name}: " +
                                    "Something went wrong when loading " +
                                    $"timeline {path}.");
                }
            }
        }

        void LoadTimeline ()
        {
            string fullPath = Application.dataPath + "/" + path;
            string timeline = Path.GetFileNameWithoutExtension (path);
            string scene = ocs.SearchPath (timeline);

            if (!fullPath.EndsWith (".playable")) return;

            if (String.IsNullOrEmpty (scene))
            {
                Debug.Log ($"Timeline {timeline} is not configured " +
                                    "in OriginalCSData.");
                return;
            }

            ocs.OpenScene (scene);
            CreateTimeline.FindOrLoadTimelineGameobject ();
            UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(OriginalSetting.LIB + timeline + ".prefab");
            UnityEngine.Object instantiation = PrefabUtility.InstantiatePrefab (go);
            PlayableDirector director = (go as GameObject).GetComponentInChildren<PlayableDirector>();
            OrignalTimelineEditor.Init ();
            if (director != null)
            {
                Selection.activeObject = director.gameObject;
                director.time = 0.01d;
            }

            LoadVirtualCam (timeline);
            SetCameraOverlay ();
        }

        void LoadVirtualCam (string timeline)
        {
            string vc = ocs.SearchVirtualCam (timeline);
            if (!String.IsNullOrEmpty (vc))
            {
                string vcPath = OriginalSetting.vcPath + vc;
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(vcPath) as GameObject;
                if (go != null)
                {
                    UnityEngine.Object instantiation = PrefabUtility.InstantiatePrefab (go);
                    instantiation.name = OriginalSetting.EditorVC;
                }
            }
        }

        void SetCameraOverlay ()
        {
            if (Camera.main == null)
                return;
            UniversalAdditionalCameraData mainCamera = Camera.main.GetComponent<UniversalAdditionalCameraData>();
            UniversalAdditionalCameraData[] allCameras = GameObject.FindObjectsOfType<UniversalAdditionalCameraData>();

            for (int i = 0; i < allCameras.Length; ++i)
            {
                UniversalAdditionalCameraData cam = allCameras[i];
                if (cam.renderType == CameraRenderType.Overlay)
                    mainCamera.cameraStack.Add (cam.GetComponent<Camera>());
            }
        }
    }

    [Serializable]
    public class LevelConfigAsset : ConfigAsset
    {
        public LevelConfigAsset () : base () 
        {

        }

        public LevelConfigAsset (ConfigAssetEnum type,
                                 string name,
                                 int depth,
                                 int id,
                                 string i_path) : base (type, name, depth, id)
        {
            path = i_path;
        }

        public override void Open ()
        {
            if (!path.EndsWith (".cfg"))
            {
                try
                {
                    System.Diagnostics.Process.Start (Application.dataPath + "/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
            else
            {
                try
                {
                    var levelEditor = EditorWindow.GetWindow<LevelEditor.LevelEditor>();
                    levelEditor.ExternalLoad (Application.dataPath + "/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
        }
    }

    [Serializable]
    public class BehitConfigAsset : ConfigAsset
    {
        public BehitConfigAsset () : base () 
        {

        }

        public BehitConfigAsset (ConfigAssetEnum type,
                                 string name,
                                 int depth,
                                 int id,
                                 string i_path) : base (type, name, depth, id)
        {
            path = i_path;
        }

        public override void Open ()
        {
            if (!path.EndsWith (".bytes"))
            {
                try
                {
                    System.Diagnostics.Process.Start (Application.dataPath + "/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
            else
            {
                try
                {
                    var behitEditor = EditorWindow.GetWindow<BehitEditor>();
                    behitEditor.OpenFile (Application.dataPath + "/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
        }
    }

    [Serializable]
    public class SkillConfigAsset : ConfigAsset
    {
        public SkillConfigAsset () : base () 
        {

        }

        public SkillConfigAsset (ConfigAssetEnum type,
                                 string name,
                                 int depth,
                                 int id,
                                 string i_path) : base (type, name, depth, id)
        {
            path = i_path;
        }

        public override void Open ()
        {
            if (!path.EndsWith (".bytes"))
            {
                try
                {
                    System.Diagnostics.Process.Start (Application.dataPath + "/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
            else
            {
                try
                {
                    var skillEditor = EditorWindow.GetWindow<SkillEditor>();
                    skillEditor.OpenFile (Application.dataPath + "/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
        }
    }

    [Serializable]
    public class ModelArtAsset : ArtAsset
    {
        public ModelArtAsset () : base ()
        {

        }
        
        public ModelArtAsset (ArtAssetEnum type,
                              string name,
                              int depth,
                              int id,
                              string i_path,
                              List<ConfigAsset> i_references = null) : base (type, name, depth, id)
        {
            path = i_path;
            if (i_references != null)
                references = i_references;
        }

        public override void Open ()
        {
            if (!path.EndsWith (".prefab"))
            {
                try
                {
                    System.Diagnostics.Process.Start (Application.dataPath + "/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
            else
            {
                try
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath ("Assets/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
        }
    }

    [Serializable]
    public class AnimationArtAsset : ArtAsset
    {
        public AnimationArtAsset () : base ()
        {

        }
        
        public AnimationArtAsset (ArtAssetEnum type,
                                  string name,
                                  int depth,
                                  int id,
                                  string i_path,
                                  List<ConfigAsset> i_references = null) : base (type, name, depth, id)
        {
            path = i_path;
            if (i_references != null)
                references = i_references;
        }

        public override void Open ()
        {
            if (!path.EndsWith (".anim"))
            {
                try
                {
                    System.Diagnostics.Process.Start (Application.dataPath + "/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
            else
            {
                try
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath ("Assets/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
        }
    }

    [Serializable]
    public class EffectArtAsset : ArtAsset
    {
        public EffectArtAsset () : base ()
        {

        }
        
        public EffectArtAsset (ArtAssetEnum type,
                               string name,
                               int depth,
                               int id,
                               string i_path,
                               List<ConfigAsset> i_references = null) : base (type, name, depth, id)
        {
            path = i_path;
            if (i_references != null)
                references = i_references;
        }

        public override void Open ()
        {
            if (!path.EndsWith (".prefab"))
            {
                try
                {
                    System.Diagnostics.Process.Start (Application.dataPath + "/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
            else
            {
                try
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath ("Assets/" + path);
                }
                catch (IOException e)
                {
                    Debug.Log ($"Reading {path} error: {e.Message}");
                }
            }
        }
    }
}