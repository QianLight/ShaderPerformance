using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


[CustomEditor(typeof(CameraMgr), true)]
public class SCameraMgrEditor : Editor
{

    CameraMgr targetObj;

    void OnEnable()
    {
        targetObj = target as CameraMgr;

    }
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        base.OnInspectorGUI();
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("RootCanvas"));
        if (GUILayout.Button("»»Ö÷Ïà»ú"))
        {
            targetObj.SetCancasCamera();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
