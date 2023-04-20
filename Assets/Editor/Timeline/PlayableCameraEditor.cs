using CFEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace XEditor
{

    [CustomEditor(typeof(CameraPlayableAsset))]
    public class PlayableCameraEditor : Editor
    {
        private float length;
        private bool folder;
        private AnimationTrack[] tracks;
        private TimelineClip xclip;
        private string[] names;
        private int select;
        private Vector3 offset;
        private Vector3 RotOffset;
        private OCSData conf;
        private string dirName;
        const int max = 42;

        public string DirName
        {
            get
            {
                if (string.IsNullOrEmpty(dirName))
                {
                    var dir = RTimeline.singleton.Director;
                    dirName = dir?.playableAsset?.name;
                }
                return dirName;
            }
        }

        private void OnEnable()
        {
            PlayableDirector dir = TimelineEditor.inspectedDirector;
            if (dir != null && dir.playableAsset != null)
            {
                var tr = dir.playableAsset.outputs.Where(x => x.sourceObject is AnimationTrack);
                int i = 0;
                List<string> list = new List<string>();
                List<AnimationTrack> td = new List<AnimationTrack>();
                foreach (var it in tr)
                {
                    AnimationTrack track = it.sourceObject as AnimationTrack;
                    var bind = dir.GetGenericBinding(track);
                    if (bind != null && bind is Animator)
                    {
                        var vc = (bind as Animator).transform.GetComponent<Cinemachine.CinemachineVirtualCamera>();
                        if (vc != null)
                        {
                            list.Add(bind.name);
                            td.Add(track);
                        }
                    }
                    i++;
                }
                names = list.ToArray();
                tracks = td.ToArray();
            }
        }
        
        private void SearchClip()
        {
            PlayableDirector dir = TimelineEditor.inspectedDirector;
            if (xclip == null && dir != null)
            {
                if (dir.playableAsset != null)
                {
                    var tr = dir.playableAsset.outputs.Where(x => x.sourceObject is CameraPlayableTrack);
                    foreach (var it in tr)
                    {
                        CameraPlayableTrack track = it.sourceObject as CameraPlayableTrack;
                        foreach (var c in track.GetClips())
                        {
                            if (c.asset == target)
                            {
                                xclip = c;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            CameraPlayableAsset asset = target as CameraPlayableAsset;
            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            asset._clip = (AnimationClip)EditorGUILayout.ObjectField("Camera Clip", asset._clip, typeof(AnimationClip), false);
            if (EditorGUI.EndChangeCheck()) asset.SetEditorClip(asset._clip);

            asset.path = DrawAnim(asset._clip, AssetsPath.GetAssetPath(asset._shakeClip, out _));

            GUILayout.Space(8);
            EditorGUI.BeginChangeCheck();
            asset._shakeClip = (AnimationClip)EditorGUILayout.ObjectField("Shake Clip", asset._shakeClip, typeof(AnimationClip), false);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(asset);
            }

            asset.shake = DrawAnim(asset._shakeClip, AssetsPath.GetAssetPath(asset._shakeClip, out _));
            if (!string.IsNullOrEmpty(asset.shake) &&
                !asset.shake.ToLower().Contains("shake"))
            {
                EditorGUILayout.HelpBox("The animation is invalid", MessageType.Error);
            }

            GUILayout.Space(12);
            folder = EditorGUILayout.Foldout(folder, "转换工具");
            if (folder)
            {
                if (conf == null)
                {
                    conf = AssetDatabase.LoadAssetAtPath<OCSData>(OriginalSetting.dataPat);
                    conf.SearchVCM(DirName, out offset, out RotOffset);
                }
                if (names == null || names.Length <= 0) OnEnable();
                GUILayout.Space(4);
                select = EditorGUILayout.Popup(select, names);

                offset = EditorGUILayout.Vector3Field("位置偏移", offset);
                RotOffset = EditorGUILayout.Vector3Field("角度偏移", RotOffset);

                GUILayout.Space(4);
                if (GUILayout.Button("do it"))
                {
                    conf.UpdateVCM(DirName, offset, RotOffset);
                    SearchClip();
                    var track = tracks[select];
                    var clip = track.infiniteClip;
                    if (clip == null)
                    {
                        track.CreateInfiniteClip("3ds max camera clip");
                        clip = track.infiniteClip;
                    }
                    else
                    {
                        clip.ClearCurves();
                    }
                    float dur = (float)xclip.duration;
                    int frames = (int)(30 * dur);
                    AnimationCurve p_x = new AnimationCurve();
                    AnimationCurve p_y = new AnimationCurve();
                    AnimationCurve p_z = new AnimationCurve();
                    AnimationCurve r_x = new AnimationCurve();
                    AnimationCurve r_y = new AnimationCurve();
                    AnimationCurve r_z = new AnimationCurve();
                    float _x = 0, _y = 0, _z = 0, _qx = 0, _qy = 0, _qz = 0;
                    for (int i = 0; i < frames; i++)
                    {
                        float t = i / 30.0f;
                        Vector3 pos = Vector3.zero, rot = Vector3.zero;
                        RecordAnim(t, out pos, out rot);
                        rot += RotOffset;
                        pos += offset;
                        t = t + (float)xclip.start;
                        Debug.Log("add frame: " + t + " pos: " + pos + " rot: " + rot);

                        Add(pos.x, ref _x, t, p_x);
                        Add(pos.y, ref _y, t, p_y);
                        Add(pos.z, ref _z, t, p_z);
                        Add(rot.x, ref _qx, t, r_x);
                        Add(rot.y, ref _qy, t, r_y);
                        Add(rot.z, ref _qz, t, r_z);
                    }
                    clip.SetCurve("", typeof(Transform), "localPosition.x", p_x);
                    clip.SetCurve("", typeof(Transform), "localPosition.y", p_y);
                    clip.SetCurve("", typeof(Transform), "localPosition.z", p_z);
                    clip.SetCurve("", typeof(Transform), "localEulerAngles.x", r_x);
                    clip.SetCurve("", typeof(Transform), "localEulerAngles.y", r_y);
                    clip.SetCurve("", typeof(Transform), "localEulerAngles.z", r_z);
                    EditorUtility.SetDirty(clip);
                    EditorUtility.SetDirty(TimelineEditor.inspectedAsset);
                    TimelineEditor.Refresh(RefreshReason.ContentsModified);
                    TimelineEditor.inspectedDirector.RebuildGraph();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved | RefreshReason.ContentsModified);
                }
            }
            GUILayout.EndVertical();
        }

        private bool Add(float x, ref float _x, float t, AnimationCurve curve)
        {
            if (Mathf.Abs(x - _x) > 1e-4)
            {
                _x = x;
                var frame = new Keyframe(t, _x, 0, 0);
                curve.AddKey(frame);
                return true;
            }
            return false;
        }


        private GameObject _dummyObject = null;
        private Transform _dummyCamera = null;
        private void SetupDummy()
        {
            if (_dummyObject == null)
            {
                _dummyObject = GameObject.Find("DummyCamera");
            }
            if (_dummyObject == null)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(
                    string.Format("{0}/Prefabs/Cinemachine/DummyCamera.prefab", AssetsConfig.instance.ResourcePath));
                _dummyObject = (GameObject)PrefabUtility.InstantiatePrefab(go);
            }
            if (_dummyObject != null)
            {
                _dummyCamera = _dummyObject.transform.GetChild(0);
            }
        }

        private void RecordAnim(float time, out Vector3 pos, out Vector3 rot)
        {
            CameraPlayableAsset asset = target as CameraPlayableAsset;
            SetupDummy();
            pos = Vector3.zero;
            rot = Vector3.zero;
            if (asset._clip != null)
            {
                asset._clip.SampleAnimation(_dummyObject, time);
                Vector3 forward = -_dummyCamera.right;
                Quaternion q = Quaternion.LookRotation(forward, _dummyCamera.up);
                pos = _dummyCamera.position;
                rot = q.eulerAngles;
            }
        }

        private string DrawAnim(Object anim, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                GUILayout.Label(path);
            }

            if (anim != null)
            {
                if (anim.name.Length > max)
                {
                    GUILayout.Label("     " + anim.name.Substring(0, max) + "..");
                }
                else
                {
                    GUILayout.Label("     " + anim.name);
                }
                length = ((AnimationClip)anim).length;
                GUILayout.Label("    length: " + length + "s");

                if (!anim.name.ToLower().Contains("camera"))
                {
                    EditorGUILayout.HelpBox("The animation is not camera clip", MessageType.Error);
                }
                return anim.name;
            }
            return string.Empty;
        }
    }
}