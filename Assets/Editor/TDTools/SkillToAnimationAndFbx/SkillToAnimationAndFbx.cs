using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TDTools;
using CFEngine;
using CFUtilPoolLib;

namespace TDTools
{
    public class SkillToAnimationAndFbx : EditorWindow
    {
        private static SkillToAnimationAndFbx m_window;
        
        private string dataPath = "";
        private string inputPath = "";
        private string outputPath = "";

        private string currentCharacter;

        private List<string> skillList = new List<string>();
        private List<string> animationList = new List<string>();
        private List<string> tupleList = new List<string>();
        private List<bool> selectList = new List<bool>();
        private List<string> copyBuffer = new List<string>();

        private List<string> loweredSkillList = new List<string>();
        private List<string> loweredCopyBuffer = new List<string>();

        private Vector2 scrollViewPos;

        [MenuItem("Tools/TDTools/关卡相关工具/SkillToAnimaAndFbx %#q")]
        public static void Init()
        {
            m_window = GetWindow<SkillToAnimationAndFbx>("查看导出技能对应动作");
            m_window.minSize = new Vector2(800, 400);
            m_window.Show();
        }

        private void OnEnable()
        {
            dataPath = $"{Application.dataPath}/BundleRes/";
            inputPath = $"{Application.dataPath}/BundleRes/SkillPackage";
        }

        private void OnGUI()
        {
            // Input path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("技能目录：", inputPath);
            if (GUILayout.Button("选择技能目录", new GUILayoutOption[] { GUILayout.Width(100)}))
            {
                inputPath = EditorUtility.OpenFolderPanel("选择技能目录", inputPath, "");
                currentCharacter = inputPath.Split('/').Last();
                ReloadData();
            }
            EditorGUILayout.EndHorizontal();

            // Show the current loaded character 
            EditorGUILayout.LabelField("当前角色：", currentCharacter);

            // Output path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("导出目录：", outputPath);
            if (GUILayout.Button("选择导出目录", new GUILayoutOption[] { GUILayout.Width(100) }))
            {
                outputPath = EditorUtility.SaveFilePanel("选择导出目录", "", "", "csv");
            }
            EditorGUILayout.EndHorizontal();

            // Functional buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选"))
            {
                SetSelectAll(true);
            }
            if (GUILayout.Button("全不选"))
            {
                SetSelectAll(false);
            }
            if (GUILayout.Button("粘贴"))
            {
                PasteData();
            }
            EditorGUILayout.EndHorizontal();

            // View animations corresponding to skills
            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos, false, true);
            for (int i = 0; i < skillList.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                selectList[i] = EditorGUILayout.ToggleLeft(skillList[i], selectList[i], new GUILayoutOption[] { GUILayout.Width(300) });
                EditorGUILayout.LabelField(animationList[i]);
                EditorGUILayout.LabelField(tupleList[i]);
                if (GUILayout.Button("复制动作", new GUILayoutOption[] { GUILayout.Width(80) }))
                {
                    EditorGUIUtility.systemCopyBuffer = animationList[i];
                    ShowNotification(new GUIContent("复制成功！"), 2);
                }
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndScrollView();

            int selectCount = selectList.Sum(x => Convert.ToInt32(x));
            EditorGUILayout.LabelField($"已选择{selectCount}个");

            // The `Export` button
            if (GUILayout.Button("导出"))
            {
                Export();
            }
        }
        
        // Reload skill and animation data
        private void ReloadData()
        {
            skillList.Clear();
            animationList.Clear(); 
            tupleList.Clear();
            selectList.Clear();
            copyBuffer.Clear(); 

            loweredSkillList.Clear();
            loweredCopyBuffer.Clear();

            if (inputPath == null)
            {
                return;
            }

            string[] skillFiles = Directory.GetFiles(inputPath, "*.bytes", SearchOption.TopDirectoryOnly);

            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            var skillGraph = skillEditor.CurrentGraph as SkillGraph;

            for (int i = 0; i < skillFiles.Length; ++i)
            {
                string skill = Path.GetFileName(skillFiles[i]);
                skillList.Add(skill);
                loweredSkillList.Add(skill.ToLower());
                animationList.Add(GetAnimation(skillGraph, skillFiles[i]));
                tupleList.Add(GetTuple(skillGraph, skillFiles[i]));
                selectList.Add(false);
            }

            skillEditor.Close();
        }

        // Return the animation(s) in accordance with the given skill
        private string GetAnimation(SkillGraph skillGraph, string skillPath)
        {
            List<string> animation = new List<string>();

            skillGraph.OpenData(skillPath);

            foreach (var node in skillGraph.configData.AnimationData)
            {
                string temp = $"{node.ClipPath.Split('/').Last()}";
                animation.Add(temp);
            }
                       
            return string.Join(" ; ", animation);
        }

        // Return the (animation, fbx) tuples according to the given skill
        private string GetTuple(SkillGraph skillGraph, string skillPath)
        {
            List<string> tuple = new List<string>();

            skillGraph.OpenData(skillPath);

            foreach (var node in skillGraph.configData.AnimationData)
            {
                string temp = $"{node.ClipPath.Split('/').Last()}";
                AnimtionWrap wrap = AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Editor" + node.ClipPath + ".asset", typeof(CFEngine.AnimtionWrap)) as AnimtionWrap;
                string fbxName = GetFbxName(wrap);
                tuple.Add(fbxName);
            }

            return string.Join(" ; ", tuple);
        }

        private string GetFbxName (AnimtionWrap wrap)
        {
            if (wrap != null)
            {
                AnimationClip t_clip = wrap.clip;

                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(t_clip, out string t_guid, out long t_localid))
                {
                    string fbxName = AssetDatabase.GUIDToAssetPath(t_guid).Split('/').Last();
                    return fbxName;
                }
                else return "";    
            }
            else return "";  
        }

        // Select all (value=true) or none (value=false)
        private void SetSelectAll(bool value)
        {
            for (int i = 0; i < skillList.Count; ++i)
            {
                selectList[i] = value;
            }
        }

        // Paste the copy buffer to the selection area
        private void PasteData()
        {
            string temp = EditorGUIUtility.systemCopyBuffer.Replace('\r'.ToString(), null);
            copyBuffer = temp.Split('\n').ToList();

            for (int i = copyBuffer.Count - 1; i >= 0; --i)
            {
                if (string.IsNullOrEmpty(copyBuffer[i])) 
                {
                    copyBuffer.RemoveAt(i);
                }
            }

            for (int i = 0; i < copyBuffer.Count; ++i)
            {
                copyBuffer[i] += ".bytes";
                loweredCopyBuffer.Add(copyBuffer[i].ToLower());
            }

            SetSelectAll(false);
            List<string> notExistList = new List<string>();

            for (int i = 0; i < skillList.Count; ++i)
            {
                if (loweredCopyBuffer.Contains(loweredSkillList[i]))
                {
                    selectList[i] = true;
                }
            }

            for (int i = 0; i < copyBuffer.Count; ++i)
            {
                if (!loweredSkillList.Contains(loweredCopyBuffer[i]))
                {
                    notExistList.Add(copyBuffer[i]);
                }
            }

            if (notExistList.Count > 0)
            {
                string notification = string.Join("\n", notExistList);
                ShowNotification(new GUIContent("以下技能不存在：\n" + notification), 5);
            }
        }

        // Export the selected skills & animations data & fbx data
        private void Export()
        {
            if (outputPath == "")
            {
                ShowNotification(new GUIContent("导出目录不能为空！"), 2);
                return;
            }

            TextWriter sw = new StreamWriter(outputPath);

            for (int i = 0; i < copyBuffer.Count; ++i)
            {
                string outputString = "";
                int index = loweredSkillList.IndexOf(loweredCopyBuffer[i]);

                if (index == -1)
                {
                    outputString = copyBuffer[i] + ',' + "";
                }
                else
                {
                    // NOTE: items in the copy buffer must not be selected in the skill list
                    if (selectList[index])
                    {
                        outputString = skillList[index] + ',' + animationList[index] + ',' + tupleList[index];
                        selectList[index] = false;
                    }
                }

                if (outputString != "")
                {
                    sw.WriteLine(outputString);
                }
            }

            for (int i = 0; i < selectList.Count; ++i)
            {
                if (selectList[i])
                {
                    sw.WriteLine(skillList[i] + ',' + animationList[i] + ',' + tupleList[i]);
                    selectList[i] = false;
                }
            }
            
            ShowNotification(new GUIContent("导出成功！"), 3);

            copyBuffer.Clear();
            loweredCopyBuffer.Clear();
            sw.Close();
        }
    }
}