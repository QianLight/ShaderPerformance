using CFEngine;
using CFEngine.Editor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;

namespace XEditor
{
    public class TimelineAssetTool
    {


        private static void Export<T>(IEnumerable<T> assets, Action<T> action)
            where T : UnityEngine.Object
        {
            float cnt = (float)assets.Count();
            int i = 0;
            foreach (var it in assets)
            {
                EditorUtility.DisplayProgressBar("export", "exporting " + it.name, i++ / cnt);
                action?.Invoke(it);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }


        /// <summary>
        /// timeline使用的ui过度动画, 导出给编辑器使用
        /// (我们的动作都有两份, 编辑器和运行时是分开的)
        /// </summary>
        [MenuItem("Assets/Timeline/Export_UI_Anim")]
        public static void MakeClipWraps()
        {
            var objs = Selection.objects;
            var clips = objs.
                Where(x => x is AnimationClip).
                Select(x => x as AnimationClip);
            Export(clips, MakeClipWrap);
            EditorUtility.DisplayDialog("note", "export anim job done", "ok");
        }



        private static void MakeClipWrap(AnimationClip clip)
        {
            var wrap = ScriptableObject.CreateInstance<AnimtionWrap>();
            wrap.clip = clip as AnimationClip;
            CommonAssets.CreateAsset<AnimtionWrap>("Assets/BundleRes/EditorAnimation/UI_timeline/", clip.name, ".asset", wrap);
        }


        /// <summary>
        /// 支持timeline录制的clip导出
        /// （运行时不再加载.playable文件，因此需要导出其内部的录制内容）
        /// </summary>
        [MenuItem("Assets/Timeline/Export_TimelineRecord")]
        public static void ExportRecords()
        {
            var objs = Selection.objects;
            var assets = objs.
                Where(x => x is TimelineAsset).
                Select(x => x as TimelineAsset);
            Export(assets, ExportRecord);
            EditorUtility.DisplayDialog("note", "export anim job done", "ok");
        }


        private static void ExportRecord(TimelineAsset asset)
        {
            string name = asset.name;
            var tracks = asset.GetRootTracks();
            var clips = tracks.
                Where(x => x is AnimationTrack).
                Select(x => x as AnimationTrack).
                Select(x => x.infiniteClip);


            string path = AssetDatabase.GetAssetPath(asset);
            path = path.Substring(0, path.LastIndexOf('/'));
            foreach (var clip in clips)
            {
                if (clip != null)
                {
                    Debug.Log(clip.name);
                    AnimationClip newClip = new AnimationClip();
                    EditorUtility.CopySerializedIfDifferent(clip, newClip);
                    AssetDatabase.CreateAsset(newClip, path + "/" + asset.name + "_" + clip.name + ".anim");
                }
            }
        }


        enum ExpType { VCamera, OVERIDE };

        struct Node
        {
            public ExpType type;
            public string name;
            public AnimationClip clip;

            public Node(ExpType t, string n, AnimationClip c)
            {
                type = t;
                name = n;
                clip = c;
            }
        };

        // [MenuItem("Assets/Tool/Timeline/Export_DirectorRecord")]
        public static void ExportVCRecord()
        {
            PlayableDirector playable = GameObject.FindObjectOfType<PlayableDirector>();
            if (playable != null)
            {
                TimelineAsset asset = playable.playableAsset as TimelineAsset;
                if (asset != null)
                {
                    var tracks = asset.GetRootTracks().Where(x => x is AnimationTrack);
                    List<Node> list = new List<Node>();
                    foreach (var it in tracks)
                    {
                        var tf = ExternalHelp.FetchAttachOfTrack(it);
                        if (tf)
                        {
                            AnimationTrack atrack = it as AnimationTrack;
                            if (atrack.infiniteClip != null && tf.gameObject.GetComponent<CinemachineVirtualCameraBase>())
                            {
                                list.Add(new Node(ExpType.VCamera, tf.name, atrack.infiniteClip));
                                Debug.Log(atrack.infiniteClip.name + "  " + tf.name);
                            }
                            var childs = atrack.GetChildTracks();
                            foreach (var child in childs)
                            {
                                if (child is AnimationTrack) //overide
                                {
                                    AnimationTrack animationTrack = child as AnimationTrack;
                                    if (animationTrack.infiniteClip != null)
                                        list.Add(new Node(ExpType.OVERIDE, tf.name, animationTrack.infiniteClip));
                                    break;
                                }
                            }
                        }
                    }
                    string prefix = "Assets/BundleRes/Animation/Main_Camera";
                    if (list.Count > 0)
                    {
                        if (!Directory.Exists(Application.dataPath + "/BundleRes/Animation/Main_Camera/" + asset.name))
                            AssetDatabase.CreateFolder(prefix, asset.name);
                    }
                    string editPrefix = "Assets/BundleRes/EditorAnimation/Main_Camera";
                    if (list.Count > 0)
                    {
                        if (!Directory.Exists(Application.dataPath + "/BundleRes/EditorAnimation/Main_Camera/" + asset.name))
                            AssetDatabase.CreateFolder(editPrefix, asset.name);
                    }
                    foreach (var it in list)
                    {
                        AnimationClip newClip = new AnimationClip();
                        EditorUtility.CopySerializedIfDifferent(it.clip, newClip);
                        string path = prefix + "/" + asset.name + "/" + it.name + ".anim";
                        Debug.Log(path);
                        AssetDatabase.CreateAsset(newClip, path);

                        var wrap = ScriptableObject.CreateInstance<AnimtionWrap>();
                        wrap.clip = (AnimationClip) newClip;
                        CommonAssets.CreateAsset<AnimtionWrap>(editPrefix + "/" + asset.name, it.name, ".asset", wrap);
                    }
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogError("timeline asset is null");
                }
            }
            else
            {
                Debug.Log("not found director in the scene");
            }
        }

    }

}