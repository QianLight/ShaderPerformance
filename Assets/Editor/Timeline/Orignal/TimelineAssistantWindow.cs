using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using XEditor;

public class TimelineAssistantWindow : EditorWindow
{
    private static int m_buttonWith = 200;
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        DrawTools();
        GUILayout.Space(10);
        DrawDocs();
        GUILayout.EndVertical();
    }

    private void DrawTools()
    {
        GUILayout.Label("辅助功能:");
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选中数据", GUILayout.Width(m_buttonWith))) EditorSelec(OriginalSetting.dataPat);
        if (GUILayout.Button("字幕配置", GUILayout.Width(m_buttonWith))) EditorSelec(OriginalSetting.linePat);
        if (GUILayout.Button("选中相机", GUILayout.Width(m_buttonWith)))
        {
            if (Camera.main == null)
            {
                EditorUtility.DisplayDialog("tip", "not found camera in current scene", "ok");
            }
            else
                Selection.activeObject = Camera.main.gameObject;
        }
        if (GUILayout.Button("选中TIMELINE", GUILayout.Width(m_buttonWith))) CreateTimeline.FocusDirector();
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("导出字幕", GUILayout.Width(m_buttonWith)))
        {
            ExportTimelines();
        }
        if (GUILayout.Button("导入字幕", GUILayout.Width(m_buttonWith)))
        {
            ImportTimelines();
        }
        if (GUILayout.Button("检查timeline资源", GUILayout.Width(m_buttonWith)))
        {
            CheckTimelineAssets();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void CheckTimelineAssets()
    {
        ActorLinesTable.singleton.Read(OriginalSetting.linePat);
        string dirPath = Application.dataPath + "/BundleRes/Timeline";
        DirectoryInfo dir = new DirectoryInfo(dirPath);
        FileInfo[] files = dir.GetFiles("*.prefab");
        for (int i = 0; i < files.Length; ++i)
        {
            EditorUtility.DisplayProgressBar(string.Format("{0}-{1}/{2}", "ProcessTimeline", i, files.Length), files[i].Name, ((float)i / files.Length));
            if (!files[i].Name.StartsWith("Orignal_")) continue;
            string filePath = "Assets/BundleRes/Timeline/" + files[i].Name;
            string timelineName = files[i].Name.TrimEnd(".prefab".ToCharArray());
            //if (files[i].Name.Equals("Orignal_Drama_A02_01_01.prefab"))
            {
                GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(filePath, typeof(GameObject));
                if (go != null)
                {
                    Transform timeline = go.transform.Find("timeline");
                    if (timeline == null) continue;
                    PlayableDirector director = timeline.GetComponent<PlayableDirector>();
                    if (director == null || director.playableAsset == null) continue;
                    foreach (var pb in director.playableAsset.outputs)
                    {
                        switch (pb.sourceObject)
                        {
                            case CustomAnimationTrack _:
                                var track = pb.sourceObject as CustomAnimationTrack;
                                var clips = track.GetClips();
                                foreach (var clip in clips)
                                {
                                    CustomAnimationAsset asset = clip.asset as CustomAnimationAsset;
                                    if (asset.clip == null)
                                    {
                                        Debug.LogError("timelineName=" + timelineName + " trackName =" + track.name + "  clipName=" + asset.name + " clip is null!");
                                    }
                                }
                                break;
                        }
                    }

                }
            }
        }
        ActorLinesTable.singleton.Write(OriginalSetting.linePat);
        EditorUtility.ClearProgressBar();
    }


    private void ExportTimelines()
    {
        ActorLinesTable.singleton.Read(OriginalSetting.linePat);
        string dirPath = Application.dataPath + "/BundleRes/Timeline";
        DirectoryInfo dir = new DirectoryInfo(dirPath);
        FileInfo[] files = dir.GetFiles("*.prefab");
        for (int i = 0; i < files.Length; ++i)
        {
            EditorUtility.DisplayProgressBar(string.Format("{0}-{1}/{2}", "ProcessTimeline", i, files.Length), files[i].Name, ((float)i / files.Length));
            if (!files[i].Name.StartsWith("Orignal_")) continue;
            string filePath = "Assets/BundleRes/Timeline/" + files[i].Name;
            string timelineName = files[i].Name.TrimEnd(".prefab".ToCharArray());
            //if (files[i].Name.Equals("Orignal_Drama_A02_01_02.prefab"))
            {
                GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(filePath, typeof(GameObject));
                if (go != null)
                {
                    Transform timeline = go.transform.Find("timeline");
                    if (timeline == null) continue;
                    PlayableDirector director = timeline.GetComponent<PlayableDirector>();
                    if (director == null || director.playableAsset == null) continue;
                    foreach (var pb in director.playableAsset.outputs)
                    {
                        switch (pb.sourceObject)
                        {
                            case UIDialogTrack _:
                                ActorLinesTable.singleton.RemoveItem(timelineName, 2);
                                var track = pb.sourceObject as UIDialogTrack;
                                var clips = track.GetClips();
                                int j = 0;
                                foreach (var clip in clips)
                                {
                                    UIDialogAsset asset = clip.asset as UIDialogAsset;
                                    ActorLine line = new ActorLine();
                                    line.idx = j++;
                                    line.timeline = timelineName;
                                    line.start = clip.start;
                                    line.dur = clip.duration;
                                    line.title = asset.content;
                                    line.type = 2;
                                    line.speaker = asset.speaker;
                                    line.m_head = asset.m_head;
                                    line.m_face = asset.m_face;
                                    line.m_isPause = asset.m_isPause;
                                    line.m_blackVisible = asset.m_blackVisible;
                                    line.m_isEmpty = asset.m_isEmpty;
                                    line.m_autoEnable = asset.m_autoEnable;
                                    line.m_skipEnable = asset.m_skipEnable;
                                    ActorLinesTable.singleton.AddLine(line);
                                }
                                break;
                            case UISubtitleTrack _:
                                ActorLinesTable.singleton.RemoveItem(timelineName, 1);
                                var track2 = pb.sourceObject as UISubtitleTrack;
                                var clips2 = track2.GetClips();
                                int j2 = 0;
                                foreach (var clip in clips2)
                                {
                                    UISubtitleAsset asset = clip.asset as UISubtitleAsset;
                                    ActorLine line = new ActorLine();
                                    line.idx = j2++;
                                    line.timeline = timelineName;
                                    line.start = clip.start;
                                    line.dur = clip.duration;
                                    line.title = asset.subTitle;
                                    line.type = 1;
                                    ActorLinesTable.singleton.AddLine(line);
                                }
                                break;
                            case UIPlayableTrack _:
                                var track3 = pb.sourceObject as UIPlayableTrack;
                                var clips3 = track3.GetClips();
                                int j3 = 0;
                                foreach (var clip in clips3)
                                {
                                    UIFadeAsset asset = clip.asset as UIFadeAsset;
                                    if (asset == null) break;
                                    if (j3 == 0)
                                    {
                                        ActorLinesTable.singleton.RemoveItem(timelineName, 3);
                                    }
                                    ActorLine line = new ActorLine();
                                    line.idx = j3++;
                                    line.timeline = timelineName;
                                    line.start = clip.start;
                                    line.dur = clip.duration;
                                    line.title = asset.content;
                                    line.type = 3;
                                    line.m_fadeInClip = asset.clip1;
                                    line.m_fadeOutClip = asset.clip2;
                                    line.m_bgPath = asset.m_bgTexturePath;
                                    ActorLinesTable.singleton.AddLine(line);
                                }
                                break;
                        }
                    }

                }
            }
        }
        ActorLinesTable.singleton.Write(OriginalSetting.linePat);
        EditorUtility.ClearProgressBar();
    }

    private void ImportTimelines()
    {
        ActorLinesTable.singleton.Read(OriginalSetting.linePat);
        string dirPath = Application.dataPath + "/BundleRes/Timeline";
        DirectoryInfo dir = new DirectoryInfo(dirPath);
        FileInfo[] files = dir.GetFiles("*.prefab");
        for (int i = 0; i < files.Length; ++i)
        {
            EditorUtility.DisplayProgressBar(string.Format("{0}-{1}/{2}", "ProcessTimeline", i, files.Length), files[i].Name, ((float)i / files.Length));
            if (!files[i].Name.StartsWith("Orignal_")) continue;
            string filePath = "Assets/BundleRes/Timeline/" + files[i].Name;
            string timelineName = files[i].Name.TrimEnd(".prefab".ToCharArray());
            //if (files[i].Name.Equals("Orignal_Drama_A02_01_02.prefab"))
            {
                GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(filePath, typeof(GameObject));
                if (go != null)
                {
                    Transform timeline = go.transform.Find("timeline");
                    if (timeline == null) continue;
                    PlayableDirector director = timeline.GetComponent<PlayableDirector>();
                    if (director == null || director.playableAsset == null) continue;
                    foreach (var pb in director.playableAsset.outputs)
                    {
                        switch (pb.sourceObject)
                        {
                            case UIDialogTrack _:
                                var list = ActorLinesTable.singleton.GetItem(timelineName, 2);
                                if (list.Count() > 0)
                                {
                                    var track = pb.sourceObject as UIDialogTrack;
                                    var clips = track.GetClips();
                                    ExternalManipulator.Delete(clips.ToArray());
                                    for (int j = 0; j < list.Length; j++)
                                    {
                                        var clip = ExternalManipulator.CreateClip(track, typeof(UIDialogAsset), list[j].dur, list[j].start);
                                        if (clip.asset != null)
                                        {
                                            UIDialogAsset aa = clip.asset as UIDialogAsset;
                                            aa.content = list[j].title;
                                            aa.idx = list[j].idx;
                                            aa.speaker = list[j].speaker;
                                            aa.m_head = list[j].m_head;
                                            aa.m_face = list[j].m_face;
                                            aa.m_isPause = list[j].m_isPause;
                                            aa.m_blackVisible = list[j].m_blackVisible;
                                            aa.m_isEmpty = list[j].m_isEmpty;
                                            aa.m_autoEnable = list[j].m_autoEnable;
                                            aa.m_skipEnable = list[j].m_skipEnable;
                                            clip.displayName = string.IsNullOrEmpty(aa.content) ? "<NULL>" : aa.content;

                                        }
                                    }
                                    EditorUtility.SetDirty(track);
                                }
                                break;
                            case UISubtitleTrack _:
                                var list2 = ActorLinesTable.singleton.GetItem(timelineName, 1);
                                if (list2.Count() > 0)
                                {
                                    var track2 = pb.sourceObject as UISubtitleTrack;
                                    var clips2 = track2.GetClips();
                                    ExternalManipulator.Delete(clips2.ToArray());
                                    for (int j = 0; j < list2.Length; j++)
                                    {
                                        var clip = ExternalManipulator.CreateClip(track2, typeof(UISubtitleAsset), list2[j].dur, list2[j].start);
                                        if (clip.asset != null)
                                        {
                                            UISubtitleAsset aa = clip.asset as UISubtitleAsset;
                                            aa.subTitle = list2[j].title;
                                            aa.idx = list2[j].idx;
                                            clip.displayName = string.IsNullOrEmpty(aa.subTitle) ? aa.idx.ToString() : aa.subTitle;
                                        }
                                    }
                                    EditorUtility.SetDirty(track2);
                                }
                                break;
                            case UIPlayableTrack _:
                                var track3 = pb.sourceObject as UIPlayableTrack;
                                var clips3 = track3.GetClips();
                                bool isFadeTrack = false;
                                foreach(var item in clips3)
                                {
                                    UIFadeAsset asset = item.asset as UIFadeAsset;
                                    if (asset != null)
                                    {
                                        isFadeTrack = true;
                                        break;
                                    }
                                }

                                if (isFadeTrack)
                                {
                                    var list3 = ActorLinesTable.singleton.GetItem(timelineName, 3);
                                    if (list3.Count() > 0)
                                    {
                                        ExternalManipulator.Delete(clips3.ToArray());
                                        for (int j = 0; j < list3.Length; j++)
                                        {
                                            var clip = ExternalManipulator.CreateClip(track3, typeof(UIFadeAsset), list3[j].dur, list3[j].start);
                                            if (clip.asset != null)
                                            {
                                                UIFadeAsset aa = clip.asset as UIFadeAsset;
                                                aa.content = list3[j].title;
                                                aa.clip1 = list3[j].m_fadeInClip;
                                                aa.clip2 = list3[j].m_fadeOutClip;
                                                aa.m_bgTexturePath = list3[j].m_bgPath;
                                            }
                                        }
                                        EditorUtility.SetDirty(track3);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
        EditorUtility.ClearProgressBar();
    }

    private void DrawDocs()
    {
        GUILayout.Label("参考文档:");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("演示文档 (陈偲雪)", GUILayout.Width(m_buttonWith))) OriginalSetting.OpenDoc();
        if (GUILayout.Button("动态加载/注意事项", GUILayout.Width(m_buttonWith))) OriginalSetting.OpenApi();
        if (GUILayout.Button("口型动作文档", GUILayout.Width(m_buttonWith))) OriginalSetting.OpenLipDoc();
        if (GUILayout.Button("对照表文档", GUILayout.Width(m_buttonWith))) OriginalSetting.OpenTimelineTableDoc();
        GUILayout.EndHorizontal();
    }

    private void EditorSelec(string path)
    {
        var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
        Selection.activeObject = obj;
    }
}