using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class VirtualCameraTool
{
    private static IEnumerable<TimelineClip> shots;
    private static CinemachineTrack cineTrack;
    private static HashSet<AnimationTrack> recordTrack;

    private static void AnalyVCM(PlayableDirector dir)
    {
        var ta = dir.playableAsset;
        recordTrack = new HashSet<AnimationTrack>();
        foreach (var pb in ta.outputs)
        {
            switch (pb.sourceObject)
            {
                case CinemachineTrack _:
                    cineTrack = pb.sourceObject as CinemachineTrack;
                    shots = cineTrack.GetClips();
                    break;
                case AnimationTrack _:
                    var atrack = pb.sourceObject as AnimationTrack;
                    var b = dir.GetGenericBinding(atrack);
                    if (b != null & (b is Animator))
                    {
                        var tor = b as Animator;
                        var v = tor.gameObject.GetComponent<CinemachineVirtualCameraBase>();
                        if (v) recordTrack.Add(atrack);
                    }
                    break;
            }
        }
    }

    public static bool ProcessVCM(PlayableDirector dir, bool delete)
    {
        AnalyVCM(dir);
        var list = shots.ToList();
        list.Sort((x, y) => x.start.CompareTo(y.start));
        for (int i = 0; i < list.Count - 1; i++)
        {
            if ((list[i + 1].start - list[i].end) > 0.1f)
            {
                string tip = string.Format("not seqence vcm, {0} and {1} crossed", list[i].displayName, list[i + 1].displayName);
                EditorUtility.DisplayDialog("tip", tip, "ok");
                return false;
            }
        }
        ProcessVCM(list, dir);
        if (delete) DeleteShots(list, dir);
        return true;
    }


    private static void DeleteShots(List<TimelineClip> list, PlayableDirector dir)
    {
        TimelineClip first = list.First();
        HashSet<string> set = new HashSet<string>();
        for (int i = 1; i < list.Count; i++)
        {
            var b = DeleteTrack(list[i], dir);
            if (b) set.Add(b.name);
            cineTrack.RemoveClip(list[i]);
        }

        var tr = FindRef(first.asset as CinemachineShot, dir);
        foreach (var track in recordTrack)
        {
            if (track != tr)
            {
                Debug.Log("delte track: " + track.name + "  " + tr.name);
                if (track.infiniteClip)
                {
                    var b = dir.GetGenericBinding(track);
                    Object.DestroyImmediate(track.infiniteClip, true);
                    TrackModifier.DeleteTrack(dir.playableAsset as TimelineAsset, track);
                    set.Add(b.name);
                }
            }
        }

        DeleteVCInPrefab(set, dir);
        first.start = 0;
        first.duration = dir.duration;
    }

    private static Object DeleteTrack(TimelineClip clip, PlayableDirector dir)
    {
        AnimationTrack track = FindRef(clip.asset as CinemachineShot, dir);
        if (track.infiniteClip)
        {
            Object.DestroyImmediate(track.infiniteClip, true);
        }
        var bind = dir.GetGenericBinding(track);
        recordTrack.Remove(track);
        TrackModifier.DeleteTrack(dir.playableAsset as TimelineAsset, track);
        return bind;
    }


    private static void DeleteVCInPrefab(HashSet<string> set, PlayableDirector director)
    {
        string pat = AssetDatabase.GetAssetPath(director.playableAsset);
        pat = pat.Replace(".playable", ".prefab");
        var go = PrefabUtility.LoadPrefabContents(pat);
        var atrs = go.GetComponentsInChildren<Animator>();
        foreach (var ir in atrs)
        {
            foreach (var name in set)
            {
                if (ir.name == name)
                {
                    Object.DestroyImmediate(ir.gameObject);
                    break;
                }
            }
        }
        PrefabUtility.SaveAsPrefabAsset(go, pat);
        PrefabUtility.UnloadPrefabContents(go);
    }
    

    private static void ProcessVCM(List<TimelineClip> clips, PlayableDirector dir)
    {
        AnimationClip fistClip = null;
        for (int i = 0; i < clips.Count; i++)
        {
            CinemachineShot shot = clips[i].asset as CinemachineShot;
            AnimationTrack track = FindRef(shot, dir);
            if (track?.infiniteClip && shot != null)
            {
                var clip = track.infiniteClip;
                if (i == 0)
                {
                    fistClip = clip;
                    CutClip(clip, clips[i].end, track);
                    track.infiniteClipOffsetEulerAngles = Vector3.zero;
                    track.infiniteClipOffsetPosition = Vector3.zero;
                }
                else
                {
                    MergeClip(fistClip, clip, clips[i].start, clips[i].end, track);
                    track.muted = true;
                }
            }
        }
    }


    private static void CutClip(AnimationClip clip, double end, AnimationTrack track)
    {
        var binds = AnimationUtility.GetCurveBindings(clip);
        foreach (var bind in binds)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bind);
            HashSet<Keyframe> set = new HashSet<Keyframe>();
            var keys = curve.keys;
            int idx = 0;
            float offset = MapOffsetBind(bind, track);
            for (int i = 0; i < keys.Length; i++)
            {
                var key = curve.keys[i];
                key.value += offset;
                float t = key.time;
                if (t < end)
                {
                    set.Add(key);
                    idx = i + 1;
                }
                keys[i] = key;
            }
            var key2 = keys.Length > idx ? curve.keys[idx] : new Keyframe();
            key2.time = (float)end;
            key2.value = curve.Evaluate((float)end) + offset;
            set.Add(key2);
            curve.keys = set.ToArray();
            AnimationUtility.SetEditorCurve(clip, bind, curve);
        }
    }


    private static void MergeClip(AnimationClip c1, AnimationClip c2, double start, double end, AnimationTrack track)
    {
        var binds = AnimationUtility.GetCurveBindings(c1);
        Dictionary<EditorCurveBinding, AnimationCurve> map = new Dictionary<EditorCurveBinding, AnimationCurve>();
        foreach (var bind in binds)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(c1, bind);
            map.Add(bind, curve);
        }
        var binds2 = AnimationUtility.GetCurveBindings(c2);
        foreach (var bind in binds2)
        {
            Debug.Log(c2.name + " " + bind.propertyName + " " + track.infiniteClipOffsetPosition);
            float offset = MapOffsetBind(bind, track);
            AnimationCurve curve = AnimationUtility.GetEditorCurve(c2, bind);
            var keys = curve.keys;
            HashSet<Keyframe> set = new HashSet<Keyframe>();
            int ix1 = -1, ix2 = -1;
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].time >= start && keys[i].time < end)
                {
                    if (ix1 == -1) ix1 = Mathf.Max(0, i - 1);
                    keys[i].value += offset;
                    set.Add(keys[i]);
                    ix2 = i + 1;
                }
            }
            if (ix1 >= 0 && keys.Length > ix1)
            {
                var key = keys[ix1];
                key.time = (float)(start + 0.003);
                key.value = curve.Evaluate(key.time) + offset;
                set.Add(key);
            }
            if (ix2 >= 0)
            {
                var key = keys.Length > ix2 ? keys[ix2] : new Keyframe();
                key.time = (float)end;
                key.value = curve.Evaluate(key.time) + offset;
                set.Add(key);
            }
            var list = set.ToList();
            list.Sort((x, y) => x.time.CompareTo(y.time));

            var frames = list.ToArray();
            if (map.ContainsKey(bind))
            {
                MergeCurveFrames(map[bind], frames);
                AnimationUtility.SetEditorCurve(c1, bind, map[bind]);
            }
            else
            {
                curve.keys = frames;
                map.Add(bind, curve);
                AnimationUtility.SetEditorCurve(c1, bind, curve);
            }
        }
    }

    private static void MergeCurveFrames(AnimationCurve curve, Keyframe[] keys)
    {
        var list = new List<Keyframe>(curve.keys);
        list.AddRange(keys);
        curve.keys = list.ToArray();
    }

    private static AnimationTrack FindRef(CinemachineShot shot, PlayableDirector dir)
    {
        foreach (var it in recordTrack)
        {
            var bind = dir.GetGenericBinding(it) as Animator;
            var vm = dir.GetReferenceValue(shot.VirtualCamera.exposedName, out var valid);
            if (valid)
            {
                CinemachineVirtualCameraBase vc = vm as CinemachineVirtualCameraBase;
                if (vc.gameObject == bind.gameObject) return it;
            }
        }
        return null;
    }

    private static float MapOffsetBind(EditorCurveBinding bind, AnimationTrack track)
    {
        if (bind.propertyName == "m_LocalPosition.x")
        {
            return track.infiniteClipOffsetPosition.x;
        }
        else if (bind.propertyName == "m_LocalPosition.y")
        {
            return track.infiniteClipOffsetPosition.y;
        }
        else if (bind.propertyName == "m_LocalPosition.z")
        {
            return track.infiniteClipOffsetPosition.z;
        }
        else if (bind.propertyName == " localEulerAnglesRaw.x")
        {
            return track.infiniteClipOffsetEulerAngles.x;
        }
        else if (bind.propertyName == "m_LocalPosition.y")
        {
            return track.infiniteClipOffsetEulerAngles.y;
        }
        else if (bind.propertyName == "m_LocalPosition.z")
        {
            return track.infiniteClipOffsetEulerAngles.z;
        }
        return 0.0f;
    }
}