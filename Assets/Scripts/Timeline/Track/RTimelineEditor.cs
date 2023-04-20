#if UNITY_EDITOR
using CFEngine;
using CFUtilPoolLib;
using System.IO;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using XEditor;

public delegate void AnimOnLoad (ref DirectorAnimBinding bind, DirectorTrackAsset track, ICallbackData cbData);
public delegate void ControlOnLoad (ref DirectorControlBinding bind, DirectorTrackAsset track, ICallbackData cbData);
public delegate void ActiveOnLoad (ref DirectorActiveBinding bind, DirectorTrackAsset track, ICallbackData cbData);
public partial class RTimeline
{
    static readonly GUIContent AddAttachTrackMenuItem = EditorGUIUtility.TrTextContent ("Add Attach Track");
    static readonly GUIContent AddFxTrackMenuItem = EditorGUIUtility.TrTextContent ("Add Fx Track");
    static readonly GUIContent AddUberTrackMenuItem = EditorGUIUtility.TrTextContent ("Add Uber Track");
    static OnDrawGizmoCb drawGizmo;
    public static TimelineEditorData timelineEditorData;
    public static GameObject timelineRoot;
    public static GameObject camerInit;
    public static AnimOnLoad animationTrackOnLoad;
    public static ControlOnLoad controlTrackOnLoad;

    public static void InitEditor ()
    {
        DirectorHelper.InitEditor (AssetTypeCount);
        DirectorHelper.savePlayableFun[AssetType_FMOD] = FmodPlayableAsset.SaveAsset;
        DirectorHelper.savePlayableFun[AssetType_UI] = UIPlayerAsset.SaveAsset;
        DirectorHelper.savePlayableFun[AssetType_Camera] = CameraPlayableAsset.SaveAsset;
        DirectorHelper.savePlayableFun[AssetType_Cine] = SaveCine;
        DirectorHelper.savePlayableFun[AssetType_Subtitle] = UISubtitleAsset.SaveAsset;

        DirectorHelper.getAssetType = GetAssetType;

        drawGizmo = DirectorHelper.OnDrawGizmos;
        EnvironmentExtra.RegisterDrawGizmo (drawGizmo);
        CommonTrackDrawer.cb = AddTrackContextMenu;
    }

    static void SaveCine (BinaryWriter bw, PlayableAsset asset, bool presave)
    {
        if (!presave)
        {
            CinemachineShot shot = asset as CinemachineShot;
            shot.OnSave (bw);
        }
    }

    static byte GetAssetType (UnityEngine.Object obj)
    {
        if (obj is FmodPlayableAsset)
        {
            return AssetType_FMOD;
        }
        else if (obj is UIPlayerAsset)
        {
            return AssetType_UI;
        }
        else if (obj is CameraPlayableAsset)
        {
            return AssetType_Camera;
        }
        else if (obj is CinemachineShot)
        {
            return AssetType_Cine;
        }
        return 255;
    }
    public static void InitTimelineGo ()
    {
        timelineRoot = GameObject.Find ("_TimelineTmp");
        if (timelineRoot == null)
        {
            timelineRoot = new GameObject ("_TimelineTmp");
        }
        camerInit = GameObject.Find ("_TimelineCamerInit");
        if (camerInit == null)
        {
            camerInit = new GameObject ("_TimelineCamerInit");
        }
        RTimeline.singleton.InnerInit ();
    }

    public static void LoadTimelineConfig (string name)
    {
        string path = string.Format ("{0}/Timeline/{1}.asset", AssetsConfig.instance.ResourcePath, name);
        RTimeline.timelineEditorData = AssetDatabase.LoadAssetAtPath<TimelineEditorData> (path);
    }

    public static void SyncCameraPos (bool fromData = true)
    {
        if (timelineEditorData != null)
        {
            var trans = camerInit.transform;
            if (fromData)
            {
                trans.position = timelineEditorData.cameraStartPos;
                trans.rotation = Quaternion.Euler (timelineEditorData.cameraStartRot);
            }
            else
            {
                timelineEditorData.cameraStartPos = trans.position;
                timelineEditorData.cameraStartRot = trans.rotation.eulerAngles;
            }

        }
    }

    public static Animator LoadDummy (string prefix, string trackName, DirectorAnimBinding bind, uint id)
    {
        string goName = string.Format ("track{0}_{1}_{2}", trackName, prefix, id);
        string goPath = string.Format ("_TimelineTmp/{0}", goName);
        GameObject go = GameObject.Find (goPath);
        if (go == null)
        {
            go = new GameObject (goName);
            go.transform.parent = timelineRoot.transform;
        }
        go.transform.position = bind.pos;
        go.transform.rotation = Quaternion.Euler (bind.rot);
        go.transform.localScale = bind.scale;
        Transform t = go.transform.Find (id.ToString ());
        if (t == null)
        {
            var dummy = XAnimationLibrary.GetDummy (id);
            if (dummy != null)
            {
                var dummyGo = PrefabUtility.InstantiatePrefab (dummy) as GameObject;
                t = dummyGo.transform;
                dummyGo.name = id.ToString ();
                t.parent = go.transform;
            }
            else
            {
                DebugLog.AddErrorLog ("timeline config error, not find entity id: " + id);
            }
        }
        if (t != null)
        {
            t.localPosition = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
            return t.GetComponent<Animator> ();
        }
        return null;
    }

    public static GameObject LoadPrefab(string prefix, int trackIndex, int id, string path)
    {
        string goName = string.Format("track{0}_{1}_{2}", trackIndex, prefix, id);
        string goPath = string.Format("_TimelineTmp/{0}", goName);
        GameObject go = GameObject.Find(goPath);
        if (go == null)
        {
            go = new GameObject(goName);
            go.transform.parent = timelineRoot.transform;
        }
        string name = path.Substring(path.LastIndexOf("/") + 1);
        Transform t = go.transform.Find(name);
        if (t == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BundleRes/" + path + ".prefab");
            if (prefab != null)
            {
                var dummyGo = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                t = dummyGo.transform;
                dummyGo.name = name;
                t.parent = go.transform;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                if (prefix == "particle")
                {
                    var tl = dummyGo.transform.GetChild(0);
                    tl.gameObject.SetActive(false);
                }
                else
                    dummyGo.SetActive(false);
                return dummyGo;
            }
            else
            {
                DebugLog.AddErrorLog("timeline config error, not find prefab: " + path);
            }
        }
        SkinnedMeshRenderer[] skms = t.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skms != null)
        {
            foreach (var skm in skms)
            {
                skm.updateWhenOffscreen = true;
            }
        }
        return t.gameObject;
    }

    public static void UnloadTmpObject ()
    {
        EditorCommon.DestroyChildObjects (timelineRoot);
        TimelineUIMgr.ReturnAll ();
    }
    static void AddTrackContextMenu (GenericMenu menu, TrackAsset track)
    {
        if (track is AnimationTrack)
        {
            // menu.AddItem (AddAttachTrackMenuItem, false, parentTrack =>
            // {
            //     var list = track.GetChildTracks () as List<TrackAsset>;
            //     int count = list != null?list.Count : 0;
            //     CommonTrackDrawer.AddSubTrack (typeof (AttachTrack), "Attach " + count.ToString (), track);
            // }, track);
        }
    }
}
#endif