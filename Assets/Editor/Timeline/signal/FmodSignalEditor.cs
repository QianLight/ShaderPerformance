using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

[CustomEditor(typeof(FModSignal))]
public class FmodSignalEditor : Editor
{

    FModSignal signal;
    int select = 0;
    string[] clips;

    SerializedObject so;
    SerializedProperty m_time;

    private void OnEnable()
    {
        signal = target as FModSignal;
        so = new SerializedObject(signal);
        m_time = so.FindProperty("m_Time");
        FetchAllFmodClip();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_time, true);
        EditorGUILayout.Space();
        signal.clip = EditorGUILayout.TextField("clip", signal.clip);
        if (clips != null)
        {
            select = EditorGUILayout.Popup(" ", select, clips);
            signal.clip = "event:/" + clips[select];
        }
        if (!CheckValid())
        {
            EditorGUILayout.HelpBox("clip is not config in graph", MessageType.Error);
        }
        EditorGUILayout.Space();
        signal.key = EditorGUILayout.TextField("key", signal.key);
        if (string.IsNullOrEmpty(signal.key))
        {
            EditorGUILayout.HelpBox("key can't be empty", MessageType.Error);
        }
        signal.param = EditorGUILayout.FloatField("param", signal.param);
        so.ApplyModifiedProperties();
    }

    private bool CheckValid()
    {
        bool valid = false;
        if (clips != null)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if ("event:/" + clips[i] == signal.clip)
                {
                    select = i;
                    valid = true;
                }
            }
        }
        if (!valid && string.IsNullOrEmpty(signal.clip))
        {
            select = 0;
            valid = true;
        }
        return valid;
    }

    private void FetchAllFmodClip()
    {
        List<string> list = new List<string>();
        PlayableDirector _director = TimelineEditor.inspectedDirector;
        if (_director && _director.playableAsset != null)
        {
            foreach (PlayableBinding pb in _director.playableAsset.outputs)
            {
                if (pb.sourceObject is PlayableTrack)
                {
                    PlayableTrack track = pb.sourceObject as PlayableTrack;
                    var clips = track.GetClips();
                    foreach (var clip in clips)
                    {
                        if (clip.asset is FmodPlayableAsset)
                        {
                            FmodPlayableAsset asset = clip.asset as FmodPlayableAsset;
                            string evnt = asset.clip;
                            list.Add(evnt.Replace("event:/", ""));
                        }
                    }
                }
            }
        }
        clips = list.ToArray();
    }

}