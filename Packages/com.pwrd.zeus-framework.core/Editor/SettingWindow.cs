/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using Zeus.Attributes;
using Zeus.Core.Collections;
using UnityEditor;
using UnityEngine;


namespace Zeus.Core
{
    [InitializeOnLoad]
    static class VFileSettingInit
    {
        static VFileSettingInit()
        {
            ZeusSetting.GetInstance();
        }
    }
    internal class SettingWindow : EditorWindow
    {

        [MenuItem("Zeus/SettingWindow", false, 2)]
        static void Init()
        {
            SettingWindow window = (SettingWindow)EditorWindow.GetWindow(typeof(SettingWindow));
            window.Show();
        }
        private static void Open()
        {
            GetWindow<SettingWindow>();
        }

        private void OnEnable()
        {
            titleContent.text = "ZeusSetting";
        }

        static void SaveSettings()
        {
            ZeusSetting.GetInstance().Save();
        }

        private void OnGUI()
        {
            ZeusSetting vFileSetting = ZeusSetting.GetInstance();
            EditorGUILayout.LabelField("Include Build Path");
            for (int i = 0; i < vFileSetting.includeBuildSourcePaths.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Source：", GUILayout.Width(60));
                vFileSetting.includeBuildSourcePaths[i] = EditorGUILayout.TextField(vFileSetting.includeBuildSourcePaths[i]);

                if (GUILayout.Button("Browse", GUILayout.Width(80)))
                {
                    string temp = EditorUtility.OpenFolderPanel("Select Diectory", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        string formatTemp = temp.Replace("\\", "/");
                        string relativeDir = formatTemp.Replace(Application.dataPath.Replace("\\", "/") + "/", "");
                        vFileSetting.includeBuildSourcePaths[i] = relativeDir;
                        vFileSetting.includeBuildTargetPaths[i] = relativeDir;
                    }
                }

                EditorGUILayout.LabelField("Target:  StreamingAssets/", GUILayout.Width(150));
                if (vFileSetting.includeBuildTargetPaths.Count <= i)
                {
                    vFileSetting.includeBuildTargetPaths.Add("");
                }
                vFileSetting.includeBuildTargetPaths[i] = EditorGUILayout.TextField(vFileSetting.includeBuildTargetPaths[i]);


                if (GUILayout.Button("Delete", GUILayout.Width(80)))
                {
                    vFileSetting.includeBuildSourcePaths.RemoveAt(i);
                    vFileSetting.includeBuildTargetPaths.RemoveAt(i);
                }
                EditorGUILayout.Space();
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("New"))
            {
                vFileSetting.includeBuildSourcePaths.Add("");
                vFileSetting.includeBuildTargetPaths.Add("");
            }
            EditorGUILayout.Space();


            EditorGUILayout.LabelField("Exclude Build Path");
            for (int i = 0; i < vFileSetting.excludeBuildSourcePaths.Count; i++)
            {
                GUILayout.BeginHorizontal();
                vFileSetting.excludeBuildSourcePaths[i] = EditorGUILayout.TextField(vFileSetting.excludeBuildSourcePaths[i], GUILayout.Width(400));
                if (GUILayout.Button("Browse", GUILayout.Width(80)))
                {
                    string temp = EditorUtility.OpenFolderPanel("Select Diectory", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        string formatTemp = temp.Replace("\\", "/");
                        string relativeDir = formatTemp.Replace(Application.dataPath.Replace("\\", "/") + "/", "");
                        vFileSetting.excludeBuildSourcePaths[i] = relativeDir;
                    }
                }
                if (GUILayout.Button("Delete", GUILayout.Width(80)))
                {
                    vFileSetting.excludeBuildSourcePaths.RemoveAt(i);
                }
                EditorGUILayout.Space();
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("New"))
            {
                vFileSetting.excludeBuildSourcePaths.Add("");
            }
            EditorGUILayout.Space();

            SaveSettings();
        }
    }

}
