using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;



[CustomEditor(typeof(UISignal))]
public class UISignalEditor : Editor
{
    private UISignal m_signal;
    private SerializedObject m_so;
    private SerializedProperty m_time;
    private SerializedProperty m_sign;
    private SerializedProperty m_arg;
    private SerializedProperty m_duration;

    private void OnEnable()
    {
        m_signal = target as UISignal;
        m_so = new SerializedObject(m_signal);
        m_time = m_so.FindProperty("m_Time");
        m_sign = m_so.FindProperty("m_sign");
        m_arg = m_so.FindProperty("m_arg");
        m_duration = m_so.FindProperty("m_duration");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_time, true);
        EditorGUILayout.PropertyField(m_sign, true);
        EditorGUILayout.PropertyField(m_arg, true);
        if (m_signal.m_sign == UISign.CHAPTER_START)
        {
            EditorGUILayout.PropertyField(m_duration, true);
        }
        m_so.ApplyModifiedProperties();
        EditorUtility.SetDirty(m_signal);
    }
}