using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Editor.TDTools.SaveWholeFileSkill
{
    internal class SaveWholeFileSkill : EditorWindow
    {
        public static SaveWholeFileSkill Instance;
        private static SkillEditor skillEditor;
        string FilePath;
        string SkillPath;
        private void OnEnable()
        {
            SkillPath = Application.dataPath + "/BundleRes/SkillPackage";
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("目标文件夹：", FilePath);
            if (GUILayout.Button("...", new GUILayoutOption[] { GUILayout.Width(50) }))
            {
                FilePath = EditorUtility.OpenFolderPanel("技能文件夹目录", SkillPath, "");

            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);
            if (SkillEditor.Instance == null)
            {
                EditorGUILayout.HelpBox("需要打开SkillEditor本工具才能正常工作", MessageType.Info);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("save"))
            {
                SaveAllSkill();
            }
            EditorGUILayout.EndHorizontal();
        }

        [MenuItem("Tools/TDTools/关卡相关工具/SaveWholeFileSkill %&d")]
        public static void ShowWindow()
        {
            Instance = GetWindow<SaveWholeFileSkill>("一键存储文件夹下所有技能");
            skillEditor = GetWindow<SkillEditor>();
            Instance.Focus();
        }

        private void SaveAllSkill()
        {
            List<string> AllSkillNames = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(FilePath);
            foreach (FileInfo file in directoryInfo.GetFiles())                     //"C:/Users/user/Desktop/Project/OPProject/Assets/BundleRes/SkillPackage/Role_Ace/Role_Ace_attack1.bytes"
            {
                if (file.Extension == ".bytes")
                {
                    AllSkillNames.Add(file.Name);
                }
            }
            foreach (string skillname in AllSkillNames)
            {
                skillEditor.SaveSkill(FilePath + "/" + skillname);
            }

        }
    }
}
