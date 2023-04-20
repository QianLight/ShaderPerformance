using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace XEditor
{

    [CustomEditor(typeof(UISubtitleTrack))]
    public class SubTitleTrackInspector : UnityEditor.Editor
    {
        UISubtitleTrack track;
        PlayableDirector director;
        string timeline;
        
        private bool debug = false;

        private void OnEnable()
        {
            track = target as UISubtitleTrack;
            director = GameObject.FindObjectOfType<PlayableDirector>();
            if (director != null)
            {
                timeline = director.playableAsset.name;
                ActorLinesTable.singleton.Read(OriginalSetting.linePat);
            }

            debug = false;
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("导入到timeline"))
            {
                Import();
            }

            if (GUILayout.Button("导出到表格"))
            {
                Export();
            }

            GUILayout.EndHorizontal();

            debug = EditorGUILayout.Foldout(debug, "debug");
            if (debug)
            {
                if (GUILayout.Button("clear"))
                {
                    Clear();
                }

                if (GUILayout.Button("Select"))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(OriginalSetting.linePat);
                }
            }
        }


        private void Clear()
        {
            var clips = track.GetClips();
            ExternalManipulator.Delete(clips.ToArray());
        }

        private void Import()
        {
            Clear();
            var list = ActorLinesTable.singleton.GetItem(timeline, 1);
            for (int i = 0; i < list.Length; i++)
            {
                var clip = ExternalManipulator.CreateClip(track, typeof(UISubtitleAsset), list[i].dur, list[i].start);
                if (clip.asset != null)
                {
                    UISubtitleAsset aa = clip.asset as UISubtitleAsset;
                    aa.subTitle = list[i].title;
                    aa.idx = list[i].idx;
                    clip.displayName = string.IsNullOrEmpty(aa.subTitle)  ? aa.idx.ToString() : aa.subTitle;
                }
            }

            EditorUtility.SetDirty(track);
        }


        private void Export()
        {
            ActorLinesTable.singleton.RemoveItem(timeline, 1);
            var clips = track.GetClips();
            int i = 0;
            foreach (var clip in clips)
            {
                UISubtitleAsset asset = clip.asset as UISubtitleAsset;
                ActorLine line = new ActorLine();
                line.idx = i++;
                line.timeline = timeline;
                line.start = clip.start;
                line.dur = clip.duration;
                line.title = asset.subTitle;
                line.type = 1;
                ActorLinesTable.singleton.AddLine(line);
            }

            ActorLinesTable.singleton.Write(OriginalSetting.linePat);
        }
    }
}