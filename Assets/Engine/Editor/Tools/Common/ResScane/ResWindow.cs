using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CFEngine.Editor
{
    public class ResWindow : EditorWindow
    {
        private ResItem item;
        private bool parentFolder = true;
        private Vector2 parentScroll;
        private bool childFolder = true;
        private Vector2 childScroll;
        private ESortType sortType = ESortType.ResType;
        private string search = "";
        private string searchLow = "";

        private static Dictionary<string, ResWindow> resWindowCache = new Dictionary<string, ResWindow>();
        public static void Open(ResItem item)
        {
            if(!resWindowCache.TryGetValue(item.nameWithExtLow,out var resWindow))
            {
                resWindow = CreateWindow<ResWindow>(item.nameWithExt);
                resWindow.name = item.nameWithExt;
                resWindow.item = item;
                if (item.relative != null)
                {
                    ResItem.Sort(item.relative.sortList, ESortType.ResType);
                }
                resWindowCache.Add(item.nameWithExtLow, resWindow);
            }


            resWindow.Show();
            resWindow.Focus();
        }
        private void OnDisable()
        {
            if (item != null)
                resWindowCache.Remove(item.nameWithExtLow);
        }
        void OnGUI()
        {
            var rect = this.position;
            if (item != null)
            {
                GUILayout.BeginHorizontal("HelpBox");
                GUILayout.Space(30);
                EditorGUI.BeginChangeCheck();
                search = EditorGUILayout.TextField("", search, "SearchTextField", GUILayout.MaxWidth(300));
                if (EditorGUI.EndChangeCheck())
                {
                    searchLow = search;
                }
                GUILayout.Label("", "SearchCancelButtonEmpty");
                GUILayout.EndHorizontal();
                if (item.parents != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    parentFolder = EditorGUILayout.Foldout(parentFolder, string.Format("Parents({0})", item.parents.Count.ToString()));
                    EditorGUILayout.EndHorizontal();
                    if (parentFolder)
                    {
                        EditorCommon.BeginScroll(ref parentScroll, item.parents.Count, 20, -1, rect.width - 20);
                        foreach (var ri in item.parents)
                        {
                            if (ri.nameWithExtLow.Contains(searchLow))
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(string.Format("{0} - {1}", ri.nameWithExt, ri.stateStr), GUILayout.MaxWidth(rect.width - 120));
                                if (GUILayout.Button("View", GUILayout.MaxWidth(80)))
                                {
                                    Open(ri);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorCommon.EndScroll();
                    }
                }
                
                if (item.relative != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    childFolder = EditorGUILayout.Foldout(childFolder, string.Format("Childs({0})", item.relative.sortList.Count.ToString()));
                    EditorGUILayout.LabelField(item.stateStr, GUILayout.MaxWidth(300));
                    EditorGUI.BeginChangeCheck();
                    sortType = (ESortType)EditorGUILayout.EnumPopup("SortType", sortType);
                    if(EditorGUI.EndChangeCheck())
                    {
                        ResItem.Sort(item.relative.sortList, sortType);
                    }
                    EditorGUILayout.EndHorizontal();
                    if (childFolder)
                    {
                        int lastType = -1;
                        EditorCommon.BeginScroll(ref childScroll, item.relative.sortList.Count, 20, 600, rect.width - 20);
                        foreach(var ri in item.relative.sortList)
                        {
                            if (ri.nameWithExtLow.Contains(searchLow))
                            {
                                if (sortType == ESortType.ResType)
                                {
                                    if (lastType != ri.resType)
                                    {
                                        EditorGUILayout.LabelField(
                                            string.Format("==================={0}====================",ResObject.GetExt(ri.resType)));
                                    }
                                    lastType = ri.resType;
                                }                                
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(string.Format("{0} - {1}", ri.nameWithExt, ri.stateStr), GUILayout.MaxWidth(rect.width - 120));
                                if (GUILayout.Button("View", GUILayout.MaxWidth(80)))
                                {
                                    Open(ri);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorCommon.EndScroll();
                    }
   
                }
            }
        }
    }
}