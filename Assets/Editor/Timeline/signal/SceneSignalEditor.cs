/********************************************************************
	created:	2021/08/02  11:09
	file base:	SceneSignalEditor
	author:		c a o   f e n g
	
	purpose:	多场景编辑器
*********************************************************************/
using CFEngine;
using CFEngine.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SceneSignal))]
public class SceneSignalEditor : Editor
{
    private SerializedObject m_so;
    private SceneSignal m_signal;

    private void OnEnable()
    {
        m_signal = target as SceneSignal;
        m_so = new SerializedObject(m_signal);
    }

    public override void OnInspectorGUI()
    {
        m_signal.m_UnLoadScene = GUILayout.Toggle(m_signal.m_UnLoadScene, "UnLoadScene", GUILayout.Width(100));
        m_signal.m_SceneName = EditorGUILayout.TextField("SceneName", m_signal.m_SceneName);
        m_signal.m_IsHiddenActiveScene = GUILayout.Toggle(m_signal.m_IsHiddenActiveScene, "IsHiddenActiveScene", GUILayout.Width(200));

        m_so.ApplyModifiedProperties();
        EditorUtility.SetDirty(m_signal);
    }
}
