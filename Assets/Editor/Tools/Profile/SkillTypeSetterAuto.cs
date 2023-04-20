using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CFEngine.Editor;

namespace CFEngine.Editor
{
    public class SkillTypeSetterAuto : EditorWindow
    {
        public static SkillTypeSetterAuto instance;
        private List<string> _files;
        private string skillPath;
        private List<SkillProfileType> typeList;
        private List<SkillProfileType> docList;
        private List<SkillProfileType> result;
        private List<string> invalidList;
        private string info;
        private Vector2 filePos;
        private Vector2 resultPos;
        private Vector2 invalidPos;
        public static bool isOn;
        private int pathID;
        
        public static void ShowWindow()
        {
            if(!instance)instance = GetWindow<SkillTypeSetterAuto>("技能类型批量匹配赋值工具");
            instance.Focus();
            isOn = true;
        }

        public static void CloseWindow()
        {
            instance.Close();
            isOn = false;
        }
        private void OnEnable()
        {
            result = new List<SkillProfileType>();
            invalidList = new List<string>();
            pathID = 0;
        }

        private void OnGUI()
        {
            string[] folderPath = Directory.GetDirectories(Application.dataPath + "/BundleRes/SkillPackage/");
            for (int i = 0; i < folderPath.Length; i++)
            {
                folderPath[i] = Path.GetFileName(folderPath[i]);
            }
            pathID = EditorGUILayout.Popup("列表", pathID, folderPath);
            skillPath = folderPath[pathID];
            typeList = EditorSFXData.instance.skillTypeByFolder;
            docList = EditorSFXData.instance.skillTypeByDoc;
            using (new EditorGUILayout.HorizontalScope())
            {
                
                // if (GUILayout.Button("查找"))
                // {
                //     ReadNewFile();
                // }

                if (GUILayout.Button("匹配"))
                {
                    result.Clear();
                    invalidList.Clear();
                    ReadNewFile();
                    if (_files.Count > 0)
                    {
                        MatchType(false);
                    }
                    else
                    {
                        info = "无可用匹配\n";
                    }
                }

                if (GUILayout.Button("强制缺省匹配"))
                {
                    result.Clear();
                    invalidList.Clear();
                    ReadNewFile();
                    if (_files.Count > 0)
                    {
                        MatchType(true);
                    }
                    else
                    {
                        info = "无可用匹配\n";
                    }
                }
            
                if (GUILayout.Button("保存到列表"))
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        EditorSFXData.instance.skillTypeByDoc.Add(result[i]);
                    }

                    // List<SkillProfileType> list = EditorSFXData.instance.skillTypeByDoc;
                    EditorSFXData.instance.skillTypeByDoc =  
                        EditorSFXData.instance.skillTypeByDoc.Where(
                            (x, i) =>  
                                EditorSFXData.instance.skillTypeByDoc.FindIndex(
                                    n => 
                                        n.skillName == x.skillName) 
                                == i).ToList();
                    EditorSFXData.instance.Save();
                    info = "添加完毕";
                }
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(info, MessageType.Info);
            EditorGUILayout.Space(10);
    
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("读取技能列表");
                    using (var scroll = new EditorGUILayout.ScrollViewScope(filePos, false, true))
                    {
                        filePos = scroll.scrollPosition;
                        if (_files != null)
                        {
                            for (int i = 0; i < _files.Count; i++)
                            {
                                EditorGUILayout.LabelField(_files[i]);
                            }
                        }
                    }
                    // filePos = EditorGUILayout.BeginScrollView(filePos, false, true);
                    // EditorGUILayout.EndScrollView();
                }
                string[] option = new string[EditorSFXData.instance.profileLevels];
                for (int i = 0; i < EditorSFXData.instance.profileLevels; i++)
                {
                    option[i] = EditorSFXData.instance.settingType[i].exampleInfo;
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("匹配结果列表");
                    using (var scroll = new EditorGUILayout.ScrollViewScope(resultPos, false, true))
                    {
                        resultPos = scroll.scrollPosition;
                        if (result != null)
                        {
                            for (int i = 0; i < result.Count; i++)
                            {
                                // using (new EditorGUILayout.HorizontalScope())
                                // {
                                var skillProfileType = result[i];
                                skillProfileType.skillType = EditorGUILayout.Popup(skillProfileType.skillName, skillProfileType.skillType, option);
                                result[i] = skillProfileType;
                                // }
                            }
                        }
                    }
                    // resultPos = EditorGUILayout.BeginScrollView(resultPos, false, true);
                    // EditorGUILayout.EndScrollView();
                }
                
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("无效匹配列表");
                    using (var scroll = new EditorGUILayout.ScrollViewScope(invalidPos, false, true))
                    {
                        invalidPos = scroll.scrollPosition;
                        string total = String.Empty;
                        if (invalidList != null)
                        {
                            for (int i = 0; i < invalidList.Count; i++)
                            {
                                total += invalidList[i] + "\n";
                            }

                        }

                        EditorGUILayout.TextArea(total);
                    }
                    // invalidPos = EditorGUILayout.BeginScrollView(invalidPos, false, true);
                   
                    // EditorGUILayout.EndScrollView();
                }
                
            }
        }

        private void ReadNewFile()
        {
            _files = new List<string>();
            string totalPath = Application.dataPath + "/BundleRes/SkillPackage/" + skillPath;
            var paths = Directory.GetFiles(totalPath);
            for (var index = 0; index < paths.Length; index++)
            {
                string path = paths[index];
                if (System.IO.Path.GetExtension(path) == ".bytes")
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _files.Add(path);
                }
            }
        }

        private void MatchType(bool isForce)
        {
            for (int i = 0; i < _files.Count; i++)
            {
                SingleMatch(_files[i], isForce);
            }

            info = "匹配完成";
        }

        private void SingleMatch(string target, bool isForce)
        {
            SkillProfileType oldmatch = new SkillProfileType(){skillName = "", skillType = 0};

            for (int i = 0; i < docList.Count; i++)
            {
                if (target.Equals(docList[i].skillName, StringComparison.CurrentCultureIgnoreCase))
                {
                    oldmatch = docList[i];
                    result.Add(new SkillProfileType(){skillName = target, skillType = oldmatch.skillType});
                    return;
                }
            }
            for (int i = 0; i < typeList.Count; i++)
            {
                if (IgnoreContains(target, typeList[i].skillName, StringComparison.OrdinalIgnoreCase))
                {
                    if (oldmatch.skillName == String.Empty)
                    {
                        oldmatch = typeList[i];
                    }
                    else
                    {
                        if (IgnoreContains(typeList[i].skillName,oldmatch.skillName, StringComparison.OrdinalIgnoreCase))
                        {
                            oldmatch = typeList[i];
                        }
                    }   
                }
            }

            if (oldmatch.skillName != String.Empty)
            {
                result.Add(new SkillProfileType(){skillName = target, skillType = oldmatch.skillType}); 
            }
            else
            {
                if (isForce)
                {
                    result.Add(new SkillProfileType(){skillName = target, skillType = oldmatch.skillType}); 
                }
                else
                {
                    invalidList.Add(target);
                }
            }
        }
        
        private bool IgnoreContains(string source, string value, StringComparison comparisonType)
        {
            return (source.IndexOf(value, comparisonType) >= 0);
        }
    }
}