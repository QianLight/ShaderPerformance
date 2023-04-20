using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

[CustomEditor(typeof(Mp4Signal))]
public class Mp4SignalEditor : Editor
{
    Mp4Signal signal;
    Object video;
    bool useServer;
    SerializedObject so;
    SerializedProperty m_time;

    private void OnEnable()
    {
        signal = target as Mp4Signal;
        so = new SerializedObject(signal);
        m_time = so.FindProperty("m_Time");

        if (video == null && !string.IsNullOrEmpty(signal.movie))
        {
            if (signal.movie.StartsWith("http"))
            {
                useServer = true;
            }
        }
    }


    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_time, true);

        string compath = "Assets/BundleRes/Video/" + signal.movie + ".mp4";

        if (video == null && !string.IsNullOrEmpty(signal.movie))
        {
            if (!useServer)
                video = AssetDatabase.LoadAssetAtPath<VideoClip>(compath);
        }

        useServer = EditorGUILayout.Toggle("Use Server", useServer);

        if (useServer)
        {
            signal.movie = EditorGUILayout.TextField("url ", signal.movie);
        }
        else
        {
            video = EditorGUILayout.ObjectField("", video, typeof(VideoClip), false);
            if (video)
            {
                signal.movie = video.name;
            }

            if (!File.Exists(compath))
            {
                EditorGUILayout.HelpBox("Not found movie", MessageType.Error);
            }
            else
            {
                GUILayout.Label(signal.movie);
            }
        }
        so.ApplyModifiedProperties();
    }
}