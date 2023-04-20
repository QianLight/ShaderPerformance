using System.Net.Http;
using System;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Contracts;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.CFUI;
using CFUtilPoolLib;

namespace UIAnalyer
{
    public class UIRenameEditor : EditorWindow 
    {
        private bool m_disable_select = false;
        private string m_file_old = string.Empty;
        private string m_message_tips = string.Empty;

        private string m_oldName = string.Empty;
        private string m_newName = string.Empty;

        private UISourceTreeItem m_sourceItem = null;
        private Action<string, UISourceTreeItem> m_applyCall = null;

        public static void ShowSourceRename(UISourceTreeItem data, Action<string, UISourceTreeItem> applyCall)
        {
            UIRenameEditor rename = EditorWindow.GetWindow<UIRenameEditor>("Rename");
            rename.minSize = new Vector2(700,130);
            rename.maxSize = new Vector2(700,130);
            rename.m_sourceItem = data;
            rename.m_applyCall = applyCall;
            rename.m_file_old = data.Source.fullname;
            rename.m_oldName = UISourceUtils.GetFileName(data.Source.fullname);
            rename.m_disable_select = true;
            rename.Show();
        }
            
  
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Source File:", GUILayout.Width(80));
            if (!m_disable_select || string.IsNullOrEmpty(m_file_old))
            { 
                m_file_old = EditorGUILayout.TextField(m_file_old);
                if (GUILayout.Button("Browse"))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Texture", Application.dataPath + "/BundleRes/UI", ".png");
                    string need = "Assets/";
                    int index = path.IndexOf(need);
                    if (index >= 0)
                    {
                        m_file_old = path.Substring(index + need.Length);
                    }
                    else
                    {
                        m_message_tips = "无效的路径!";
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField(m_file_old, GUILayout.Width(position.width - 90));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Old Name:", GUILayout.Width(80));
            EditorGUILayout.LabelField(m_oldName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New Name:",GUILayout.Width( 80));
            m_newName = EditorGUILayout.TextField(m_newName, GUILayout.Width(position.width - 90));
            EditorGUILayout.EndHorizontal();

            bool disable = string.IsNullOrEmpty(m_newName);
            EditorGUI.BeginDisabledGroup(disable);
            if (GUILayout.Button("Apply"))
            {

                if(m_applyCall != null)
                {
                    m_applyCall.Invoke(m_newName, m_sourceItem);
                    this.Close();
                    return;
                }
            }
            EditorGUI.EndDisabledGroup();
            if (disable || string.IsNullOrEmpty(m_message_tips))
            {
                m_message_tips = "名字为空";
            }
            if (!string.IsNullOrEmpty(m_message_tips))
            {
                EditorGUILayout.HelpBox(m_message_tips, MessageType.Warning, true);
            }

            EditorGUILayout.EndVertical();
        }
    }
}