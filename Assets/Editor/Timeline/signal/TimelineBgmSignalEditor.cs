using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor(typeof(TimelineBgmSignal))]
public class TimelineBgmSignalEditor : Editor
{
    private TimelineBgmSignal m_signal;
    private SerializedObject m_so;
    private SerializedProperty m_time;

    private SerializedProperty m_pauseOrStop;
    private SerializedProperty m_pause;
    private SerializedProperty m_stop;
    private SerializedProperty m_paramStr;
    private SerializedProperty m_bgmVolume;

    private SerializedProperty m_useBgmSettings;
    private SerializedProperty m_pauseOrStopEnv;
    private SerializedProperty m_pauseEnv;
    private SerializedProperty m_stopEnv;
    private SerializedProperty m_paramStrEnv;
    private SerializedProperty m_bgmVolumeEnv;
    private SerializedProperty m_environmentVolume;
    private SerializedProperty m_isLastSignal;
    private GUIStyle m_tempFontStyle = new GUIStyle();


    private void OnEnable()
    {
        m_tempFontStyle.normal.textColor = Color.yellow;
        m_tempFontStyle.fontSize = 20;

        m_signal = target as TimelineBgmSignal;
        m_so = new SerializedObject(m_signal);
        m_time = m_so.FindProperty("m_Time");

        m_pauseOrStop = m_so.FindProperty("m_pauseOrStop");
        m_pause = m_so.FindProperty("m_pause");
        m_stop = m_so.FindProperty("m_stop");
        m_paramStr = m_so.FindProperty("m_paramStr");
        m_bgmVolume = m_so.FindProperty("m_bgmVolume");

        m_useBgmSettings = m_so.FindProperty("m_useBgmSettings");
        m_pauseOrStopEnv = m_so.FindProperty("m_pauseOrStopEnv");
        m_pauseEnv = m_so.FindProperty("m_pauseEnv");
        m_stopEnv = m_so.FindProperty("m_stopEnv");
        m_paramStrEnv = m_so.FindProperty("m_paramStrEnv");
        m_environmentVolume = m_so.FindProperty("m_environmentVolume");
        m_isLastSignal = m_so.FindProperty("m_isLastSignal");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_time, true);
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("bgm settings", m_tempFontStyle);
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(m_pauseOrStop, true, GUILayout.Width(500));
        EditorGUILayout.PropertyField(m_pause, true, GUILayout.Width(500));
        EditorGUILayout.PropertyField(m_stop, true, GUILayout.Width(500));
        EditorGUILayout.PropertyField(m_paramStr, true, GUILayout.Width(500));
        EditorGUILayout.PropertyField(m_bgmVolume, true, GUILayout.Width(500));
        EditorGUILayout.Space(10);


        EditorGUILayout.PropertyField(m_useBgmSettings, true, GUILayout.Width(500));

        if(!m_useBgmSettings.boolValue)
        {
            EditorGUILayout.LabelField("env settings", m_tempFontStyle);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(m_pauseOrStopEnv, true, GUILayout.Width(500));
            EditorGUILayout.PropertyField(m_pauseEnv, true, GUILayout.Width(500));
            EditorGUILayout.PropertyField(m_stopEnv, true, GUILayout.Width(500));
            EditorGUILayout.PropertyField(m_paramStrEnv, true, GUILayout.Width(500));
            EditorGUILayout.PropertyField(m_environmentVolume, true, GUILayout.Width(500));

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(m_isLastSignal, true, GUILayout.Width(500));
        }
        m_so.ApplyModifiedProperties();
    }
}