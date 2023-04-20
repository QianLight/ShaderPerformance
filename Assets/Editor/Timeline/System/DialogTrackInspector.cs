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
    [CustomEditor(typeof(UIDialogTrack))]
    public class DialogTrackInspector : UnityEditor.Editor
    {
        UIDialogTrack track;
        PlayableDirector director;
        string timeline;

        private bool debug = false;

        private void OnEnable()
        {
            track = target as UIDialogTrack;
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
            var list = ActorLinesTable.singleton.GetItem(timeline, 2);
            for (int i = 0; i < list.Length; i++)
            {
                var clip = ExternalManipulator.CreateClip(track, typeof(UIDialogAsset), list[i].dur, list[i].start);
                if (clip.asset != null)
                {
                    UIDialogAsset aa = clip.asset as UIDialogAsset;
                    aa.content = list[i].title;
                    aa.idx = list[i].idx;
                    aa.speaker = list[i].speaker;
                    aa.m_head = list[i].m_head;
                    aa.m_face = list[i].m_face;
                    aa.m_isPause = list[i].m_isPause;
                    aa.m_blackVisible = list[i].m_blackVisible;
                    aa.m_isEmpty = list[i].m_isEmpty;
                    aa.m_autoEnable = list[i].m_autoEnable;
                    aa.m_skipEnable = list[i].m_skipEnable;
                    clip.displayName = string.IsNullOrEmpty(aa.content) ? "<NULL>" : aa.content;
                }
            }

            EditorUtility.SetDirty(track);
        }


        private void Export()
        {
            ActorLinesTable.singleton.RemoveItem(timeline, 2);
            var clips = track.GetClips();
            int i = 0;
            foreach (var clip in clips)
            {
                UIDialogAsset asset = clip.asset as UIDialogAsset;
                ActorLine line = new ActorLine();
                line.idx = i++;
                line.timeline = timeline;
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

            ActorLinesTable.singleton.Write(OriginalSetting.linePat);
        }
    }
}