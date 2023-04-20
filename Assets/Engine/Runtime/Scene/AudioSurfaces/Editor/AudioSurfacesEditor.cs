using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    [CustomEditor(typeof(AudioSurfaces), true)]
    public class AudioSurfacesEditor: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical("地表音频", "window");
            SerializedProperty sufaceType = serializedObject.FindProperty("sufaceType");
            EditorGUILayout.PropertyField(sufaceType, false);

            SerializedProperty debugColor = serializedObject.FindProperty("debugColor");
            EditorGUILayout.PropertyField(debugColor, false);
            
            DrawSimpleList(serializedObject.FindProperty("textures"), true);
            GUILayout.EndVertical();

            //if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        void DrawSimpleList(SerializedProperty list, bool useDraBox)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Box("地表图片", GUILayout.ExpandWidth(true));
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(list, false);
            //GUILayout.Box(list.arraySize.ToString("00"));       
            GUILayout.EndHorizontal();

            if (list.isExpanded)
            {
                if (useDraBox)
                    DrawDragBox(list);
                EditorGUILayout.Separator();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("添加"))
                {
                    list.arraySize++;
                }
                if (GUILayout.Button("删除"))
                {
                    list.arraySize = 0;
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();

                for (int i = 0; i < list.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("-"))
                    {
                        RemoveElementAtIndex(list, i);
                    }

                    if (i < list.arraySize && i >= 0)
                        EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent("", null, ""));

                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void RemoveElementAtIndex(SerializedProperty array, int index)
        {
            if (index != array.arraySize - 1)
            {
                array.GetArrayElementAtIndex(index).objectReferenceValue = array.GetArrayElementAtIndex(array.arraySize - 1).objectReferenceValue;
            }
            array.arraySize--;
        }

        void DrawDragBox(SerializedProperty list)
        {
            //var dragAreaGroup = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
            GUI.skin.box.alignment = TextAnchor.MiddleCenter;
            GUI.skin.box.normal.textColor = Color.white;
            //GUILayout.BeginVertical("window");
            GUILayout.Box("拖动图片到这里!", "box", GUILayout.MinHeight(50), GUILayout.ExpandWidth(true));
            var dragAreaGroup = GUILayoutUtility.GetLastRect();
            //GUILayout.EndVertical();
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dragAreaGroup.Contains(Event.current.mousePosition))
                        break;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var dragged in DragAndDrop.objectReferences)
                        {
                            var tex = dragged as Texture;
                            if (tex == null)
                                continue;
                            if (CheckHasSame(list, tex)) continue;
                            
                            list.arraySize++;
                            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = tex;
                        }
                    }
                    serializedObject.ApplyModifiedProperties();
                    Event.current.Use();
                    break;
            }
        }

        private bool CheckHasSame(SerializedProperty list, Texture tex)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                var itm = list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (itm == tex) return true;
            }
            
            return false;
        }

    }
}
