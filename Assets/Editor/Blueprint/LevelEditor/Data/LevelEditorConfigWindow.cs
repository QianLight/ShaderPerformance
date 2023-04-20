using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using CFUtilPoolLib;

namespace LevelEditor
{
    class LevelEditorConfigWindow : EditorWindow
    {
        LevelEditor _editor;
        List<LevelWallData> _wallData;

        private static GUIStyle redText = null;
        // private static GUIStyle whiteText = null;

        public GUIStyle GetRedTextSyle()
        {
            if (redText == null)
            {
                redText = new GUIStyle("Label");
                redText.normal.textColor = Color.red;
            }
            return redText;
        }



        public void SetDataSource(List<LevelWallData> wallData)
        {
            _wallData = wallData;
        }

        public void SetMainEditor(LevelEditor editor)
        {
            _editor = editor;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Relocate", new GUILayoutOption[] { GUILayout.Width(120f) }))
            {
                _editor.RelocateMonster();
            }

            EditorGUILayout.EndHorizontal();

            if(_wallData != null && _wallData.Count > 0)
            {
                EditorGUILayout.LabelField("Wall:");
                for(int i = 0; i < _wallData.Count; ++i)
                {
                    if(_wallData[i].wallType == (int)WallType.normal)
                    {
                        EditorGUILayout.BeginHorizontal();
                        bool bNameDuplicate = WallNameExists(_wallData[i].name, i);
                        //GUI.color = bNameDuplicate ? Color.red : Color.white;

                        if(bNameDuplicate)
                            EditorGUILayout.LabelField(_wallData[i].name, GetRedTextSyle(), new GUILayoutOption[] { GUILayout.Width(160f) });
                        else
                            EditorGUILayout.LabelField(_wallData[i].name, new GUILayoutOption[] { GUILayout.Width(160f) });

                        EditorGUILayout.LabelField(_wallData[i].position.ToString(), new GUILayoutOption[] { GUILayout.Width(160f) });
                        string strOn = _wallData[i].on ? "on" : "off";
                        EditorGUILayout.LabelField(strOn, new GUILayoutOption[] { GUILayout.Width(80f) });
                        string strType = _wallData[i].type == 0 ? "Box" : "Sphere";
                        EditorGUILayout.LabelField(strType, new GUILayoutOption[] { GUILayout.Width(80f) });
                        EditorGUILayout.LabelField(_wallData[i].size.ToString(), new GUILayoutOption[] { GUILayout.Width(160f) });

                        EditorGUILayout.EndHorizontal();
                    }
                    
                }
            }
 
            if (_wallData != null && _wallData.Count > 0)
            {
                EditorGUILayout.Space();
                GUI.color = Color.white;
                EditorGUILayout.LabelField("PlayerWall:");
                for (int i = 0; i < _wallData.Count; ++i)
                {
                    if (_wallData[i].wallType == (int)WallType.player)
                    {
                        EditorGUILayout.BeginHorizontal();
                        bool bNameDuplicate = WallNameExists(_wallData[i].name, i);

                        if (bNameDuplicate)
                            EditorGUILayout.LabelField(_wallData[i].name, GetRedTextSyle(), new GUILayoutOption[] { GUILayout.Width(160f) });
                        else
                            EditorGUILayout.LabelField(_wallData[i].name, new GUILayoutOption[] { GUILayout.Width(160f) });

                        EditorGUILayout.LabelField(_wallData[i].position.ToString(), new GUILayoutOption[] { GUILayout.Width(160f) });
                        string strOn = _wallData[i].on ? "on" : "off";
                        EditorGUILayout.LabelField(strOn, new GUILayoutOption[] { GUILayout.Width(80f) });
                        string strType = _wallData[i].type == 0 ? "Box" : "Sphere";
                        EditorGUILayout.LabelField(strType, new GUILayoutOption[] { GUILayout.Width(80f) });
                        EditorGUILayout.LabelField(_wallData[i].size.ToString(), new GUILayoutOption[] { GUILayout.Width(160f) });
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

        }

        private bool WallNameExists(string name, int index)
        {
            if (_wallData == null) return false;

            for(int i = 0; i <_wallData.Count; ++i)
            {
                if (i != index && name == _wallData[i].name) return true;
            }
            return false;
        }
    }
}
