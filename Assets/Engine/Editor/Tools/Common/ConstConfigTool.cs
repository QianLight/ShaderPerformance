using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public partial class AssetsConfigTool
    {
        private List<SerializedProperty> constPropertyList;
        private List<SerializedProperty> constListPropertyList;
        private List<SerializedProperty> commonPropertyList;
        private SerializedObject serializedObject;
        private Vector2 resScroll = Vector2.zero;
        private void InitConst ()
        {
            serializedObject = new SerializedObject (config);
            constPropertyList = new List<SerializedProperty> ();
            constListPropertyList = new List<SerializedProperty> ();
            commonPropertyList = new List<SerializedProperty> ();

            Type t = typeof (AssetsConfig);
            FieldInfo[] fields = t.GetFields ();
            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo fi = fields[i];
                var atttrs = fi.GetCustomAttributes (typeof (HideInInspector), false);
                if (atttrs != null && atttrs.Length > 0)
                    continue;
                if (fi.FieldType == typeof (string))
                {
                    SerializedProperty sp = serializedObject.FindProperty (fi.Name);
                    if (sp != null)
                    {
                        constPropertyList.Add (sp);
                    }
                }
                else if (fi.FieldType == typeof (System.Array))
                {
                    SerializedProperty sp = serializedObject.FindProperty (fi.Name);
                    if (sp != null)
                    {
                        constListPropertyList.Add (sp);
                    }
                }
                else
                {
                    SerializedProperty sp = serializedObject.FindProperty (fi.Name);
                    if (sp != null)
                    {
                        commonPropertyList.Add (sp);
                    }
                }
            }
        }
        private void ConstValuesGUI ()
        {
            if (config.folder.FolderGroup ("Const", "Const", 10000))
            {
                if (constPropertyList != null)
                {
                    for (int i = 0; i < constPropertyList.Count; ++i)
                    {
                        SerializedProperty sp = constPropertyList[i];
                        EditorGUI.BeginChangeCheck ();
                        var str = EditorGUILayout.TextField (sp.displayName, sp.stringValue);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            Undo.RecordObject (config, sp.name);
                            sp.stringValue = str;
                        }
                    }
                }
                if (constListPropertyList != null)
                {
                    for (int i = 0; i < constListPropertyList.Count; ++i)
                    {
                        SerializedProperty sp = constListPropertyList[i];
                        EditorGUILayout.PropertyField (sp, true);
                    }
                }
                if (commonPropertyList != null)
                {
                    resScroll = EditorGUILayout.BeginScrollView (resScroll, GUILayout.MinHeight (500));
                    for (int i = 0; i < commonPropertyList.Count; ++i)
                    {
                        SerializedProperty sp = commonPropertyList[i];
                        EditorGUILayout.BeginHorizontal ();
                        EditorGUILayout.PropertyField (sp, true, GUILayout.MinWidth (1000));
                        EditorGUILayout.EndHorizontal ();
                    }
                    EditorGUILayout.EndScrollView ();
                }
                serializedObject.ApplyModifiedProperties ();
            }

        }
    }
}