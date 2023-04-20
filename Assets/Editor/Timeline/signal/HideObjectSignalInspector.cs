using CFEngine;
using CFEngine.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HideObjectSignal))]
public class HideObjectSignalInspector : Editor
{
    private SerializedProperty m_time;
    private SerializedObject m_so;
    private HideObjectSignal m_signal;
     

    private void OnEnable()
    { 
        m_signal = target as HideObjectSignal;
        m_so = new SerializedObject(m_signal);
        m_time = serializedObject.FindProperty("m_Time");
        HideObjectSignal obj = (HideObjectSignal)target;
        if (obj.m_hideObjs != null)
        {
            for (int i = 0; i < obj.m_hideObjs.Count; ++i)
            {
                if (SFXSystem.animSfx.ContainsKey(obj.m_hideObjs[i].m_name))
                {
                    obj.m_hideObjs[i].m_go = SFXSystem.animSfx[obj.m_hideObjs[i].m_name];
                }
            }
        }
    } 

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_time, true);
        EditorGUILayout.Space();

        if (m_signal.m_hideObjs != null)
        {
            for (int i = 0; i < m_signal.m_hideObjs.Count; ++i)
            {
                m_signal.m_hideObjs[i].m_go = (GameObject)EditorGUILayout.ObjectField("go", m_signal.m_hideObjs[i].m_go, typeof(GameObject), true);
                EditorGUILayout.BeginHorizontal();
                m_signal.m_hideObjs[i].m_name = EditorGUILayout.TextField("name", m_signal.m_hideObjs[i].m_name);
                m_signal.m_hideObjs[i].m_visible = GUILayout.Toggle(m_signal.m_hideObjs[i].m_visible, "enable", GUILayout.Width(100));
                if (m_signal.m_hideObjs[i].m_go != null)
                {
                    //obj.m_hideObjs[i].m_name = obj.m_hideObjs[i].m_go.name;
                    SFXWrapper wrapper = m_signal.m_hideObjs[i].m_go.GetComponent<SFXWrapper>();
                    if (wrapper != null)
                    {
                        m_signal.m_hideObjs[i].m_name = wrapper.exString;
                    }
#if UNITY_EDITOR
                    ActiveObject activeObject = m_signal.m_hideObjs[i].m_go.GetComponent<ActiveObject>();
                    if (activeObject != null)
                    {
                        m_signal.m_hideObjs[i].m_name = activeObject.exString;
                    }
#endif
                }
                if (GUILayout.Button("X", GUILayout.Width(50)))
                {
                    m_signal.m_hideObjs.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("AddGo"))
        {
            if (m_signal.m_hideObjs == null) m_signal.m_hideObjs = new List<HideObjectInfo>();
            m_signal.m_hideObjs.Add(new HideObjectInfo());
        }
        m_so.ApplyModifiedProperties(); 
        EditorUtility.SetDirty(m_signal);  
    }
}