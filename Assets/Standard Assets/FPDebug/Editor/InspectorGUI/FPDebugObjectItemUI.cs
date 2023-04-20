using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FPDebugObjectItem))]
public class FPDebugObjectItemUI : Editor
{
    FPDebugUIBase volumeUi = null;
    FPDebugUIBase materialUi = null;
    FPDebugObjectItem obj;
    private void OnEnable()
    {
        obj = target as FPDebugObjectItem;
        volumeUi = new FPDebugVolumeUI(obj);
        materialUi = new FPDebugMaterialUI(obj);
    }
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        EditorGUILayout.LabelField("Remote ID:", obj.RemoteID.ToString());
        if(!volumeUi.OnInspectorGUI())
        {
            materialUi.OnInspectorGUI();
        }
        serializedObject.ApplyModifiedProperties();
    }
    private void OnDisable()
    {
        volumeUi.OnDisable();
        volumeUi = null;
        materialUi.OnDisable();
        materialUi = null;
    }
}
