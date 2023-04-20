using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using CFUtilPoolLib;
using System.Collections.Generic;
using System;

namespace XEditor
{
    public enum SearchType
    {
        Fast,
        SlowButFull
    }

    public class SkillHashLookUp : MonoBehaviour
    {
        [MenuItem(@"XEditor/LookUp skill hash")]
        static void LookUp()
        {
            EditorWindow.GetWindow(typeof(XLookUp));
        }
    }

    public class XLookUp : EditorWindow
    {
        private string hash = null;
        //private string name = null;

        private string m_name = "no match";
        private uint uhash = 0;

        private SearchType _type = SearchType.Fast;

        void OnGUI()
        {
            hash = EditorGUILayout.TextField("Hash Value", hash);
            EditorGUILayout.LabelField("Skill Name", m_name);
            _type = (SearchType)EditorGUILayout.EnumPopup(_type);

            if (GUILayout.Button("Match"))
            {
                m_name = "no match";
                Match();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            name = EditorGUILayout.TextField("Skill Name", name);
            uhash = XCommon.singleton.XHash(name);
            EditorGUILayout.TextField("Hash Value (uint)", uhash.ToString());
            EditorGUILayout.TextField("Hash Value (int)", ((int)uhash).ToString());
        }

        void Match()
        {
            if (string.IsNullOrEmpty(hash)) return;

            switch (_type)
            {
                case SearchType.Fast:
                    {
                        m_name = "no match";
                        {
                            SkillListForRole.RowData[] list = XSkillReader.RoleSkill.Table;
                            for (int i = 0; i < list.Length; i++)
                            {
                                if (XCommon.singleton.XHash(list[i].SkillScript).ToString() == hash)
                                {
                                    m_name = list[i].SkillScript;
                                    break;
                                }
                            }
                        }
                        if (m_name == "no match")
                        {
                            SkillListForEnemy.RowData[] list = XSkillReader.EnemySkill.Table;
                            for (int i = 0; i < list.Length; i++)
                            {
                                if (XCommon.singleton.XHash(list[i].SkillScript).ToString() == hash)
                                {
                                    m_name = list[i].SkillScript;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case SearchType.SlowButFull:
                    {
                        DirectoryInfo TheFolder = new DirectoryInfo(@"Assets\BundleRes\SkillPackage");
                        ProcessFolder(TheFolder);
                        TheFolder = new DirectoryInfo(@"Assets\BundleRes\HitPackage");
                        ProcessFolder(TheFolder);
                    }
                    break;
            }
        }

        void ProcessFolder(DirectoryInfo dir)
        {
            FileInfo[] fileInfo = dir.GetFiles();

            foreach (FileInfo file in fileInfo)
                ProcessFile(file);

            foreach (DirectoryInfo sub_dir in dir.GetDirectories())
                ProcessFolder(sub_dir);
        }

        void ProcessFile(FileInfo file)
        {
            if (file.FullName.EndsWith(".bytes"))
            {
                try
                {
                    if (file.FullName.Contains("SkillPackage"))
                    {
                        EcsData.XSkillData data = DataIO.DeserializeEcsData<EcsData.XSkillData>(file.FullName);
                        Process(data);
                    }
                    else
                    {
                        EcsData.XHitData data = DataIO.DeserializeEcsData<EcsData.XHitData>(file.FullName);
                        Process(data);
                    }
                    //XSkillData data = XDataIO<XSkillData>.singleton.DeserializeData(file.FullName.Substring(file.FullName.IndexOf(@"Assets\Resources\SkillPackage\")));
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("Error", file.FullName + " " + e.Message, "OK");
                    Debug.Log(file.FullName + " " + e.Message);
                }
            }
        }

        void Process(EcsData.XSkillData data)
        {
            //EditorGUILayout.LabelField("Processing...", data.Name);
            if (XCommon.singleton.XHash(data.Name).ToString() == hash)
            {
                m_name = data.Name;
            }
        }

        void Process(EcsData.XHitData data)
        {
            //EditorGUILayout.LabelField("Processing...", data.Name);
            if (XCommon.singleton.XHash(data.Name).ToString() == hash)
            {
                m_name = data.Name;
            }
        }
    }
}