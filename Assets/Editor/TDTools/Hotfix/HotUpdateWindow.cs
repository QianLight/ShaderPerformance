using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using CFClient;
using Debug = UnityEngine.Debug;


namespace TDTools
{
    public class HotUpdateWindow : EditorWindow
    {
        private const string CommitKey = "AUTOCOPY_COMMITID";
        public int inPutPresentID;
        private string outPutName = "1";
        private string SkillPath = "";
        public uint ID = 1;
        public uint searchPresentID = 1;
        private bool isShow = false;
        private string copySkillName;
        public Vector2 listScrollViewPos;
        private bool isPlayer = false;
        private bool isMonster = false;
        private bool needCommitID = false;
        private string commitID = "";
        private string inPutID = "ID";
        protected bool isRuntime = false;

        // 将窗口添加到Tools菜单
        [MenuItem("Tools/TDTools/通用工具/策划热更工具 %&.")]
        public static void ShowWindow()
        {
            //显示窗口
            HotUpdateWindow hotUpdate = EditorWindow.GetWindow<HotUpdateWindow>(typeof(HotUpdateWindow));
            // abc
        }

        public void OnEnable()
        {
            commitID = EditorPrefs.GetString(CommitKey, "");
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("角色"))
            {
                isPlayer = true;
                isMonster = false;
                inPutID = "PartnerID";
            }
            if (GUILayout.Button("怪物"))
            {
                isMonster = true;
                isPlayer = false;
                inPutID = "MonsterPresentID";
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            inPutPresentID = EditorGUILayout.IntField(inPutID, inPutPresentID);
            EditorGUILayout.EndHorizontal();         
            needCommitID = EditorGUILayout.Toggle("是否输入CommitID模式", needCommitID);
            if(needCommitID)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("拉取当前服务器包"))
                {
                    string line;
                    string res_sha_Path = "";
                    try
                    {
                        string Copyconfig_Path = Application.dataPath;                                               //先找私服的位置
                        Copyconfig_Path = Copyconfig_Path.Replace("/Assets", "/Tools/AutoCopy/CopyConfig.txt");
                        using FileStream s = new FileStream(Copyconfig_Path, FileMode.OpenOrCreate, FileAccess.Read);
                        StreamReader sr = new StreamReader(s);                    
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith("ServerProjectPath"))
                            {
                                string[] t = line.Split('=');
                                res_sha_Path = $"{t[1]}res_sha.txt";
                                break;
                            }
                        }                     
                    }
                    catch
                    {
                        Debug.Log("获取信息失败，未能找到私服路径");
                    }                             
                    try
                    {
                        using FileStream s2 = new FileStream(res_sha_Path, FileMode.OpenOrCreate, FileAccess.Read);       //再扒res_sha的内容
                        StreamReader sr2 = new StreamReader(s2);
                        commitID = sr2.ReadLine();
                    }
                    catch 
                    {
                        Debug.Log("获取信息失败，访问私服失败");
                    }
                    
                }
                commitID = EditorGUILayout.TextField(commitID);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();              
                if (GUILayout.Button("打开自动copy程序"))
                {
                    OpenAotuCopyWithCommitID();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("打开自动copy程序"))
                {
                    OpenAotuCopy();
                }
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("RunTimeLoad（暂时没用）"))
            {
                RuntimeLoad.ReimportSkill();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            isRuntime = Application.isPlaying && XClientNetwork.singleton.XLoginStep == XLoginStep.Playing;

            if (GUILayout.Button("搜索"))
            {
                if (isPlayer)
                {
                    ReadPartnerInfo();
                }

                if (isMonster)
                {
                    ReadMonsterInfo();
                }
            }

            if (GUILayout.Button("还原"))
            {
                string reload_gm_Path = Application.dataPath;
                reload_gm_Path = reload_gm_Path.Replace("/Assets", "/Tools/AutoCopy/reload_gm.txt");
                FileStream s = new FileStream(reload_gm_Path, FileMode.Truncate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(s);
                sw.Write("reload ai\nreload level\nreload alltable");
                sw.Close();

                EditorUtility.DisplayDialog("还原成功", "reload_gm 还原到了没有技能信息的状态", "确定");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();            

            if (isRuntime)
            {
                if (GUILayout.Button("Reload Skill"))
                {
                    string reload_gm_Path = Application.dataPath;
                    reload_gm_Path = reload_gm_Path.Replace("/Assets", "/Tools/AutoCopy/reload_gm.txt");
                    FileStream s = new FileStream(reload_gm_Path, FileMode.OpenOrCreate, FileAccess.Read);
                    StreamReader sr = new StreamReader(s);
                    for (int i = 0; i < 3; i++)                         //写死成跳三行效率比较好，但是后续如果要加reload的类型需要改这里
                    {
                        sr.ReadLine();
                    }
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        #if USE_GM
                        CFCommand.singleton.ProcessClientCommand(line);
                        CFCommand.singleton.ProcessServerCommand(line);
                        #endif
                    }
                    sr.Close();

                    EditorUtility.DisplayDialog("热更成功", "已在客户端和服务器双端进行了更新", "确定");
                }
            }                   
            

            EditorGUILayout.EndHorizontal();
            if (isShow || copySkillName != null)
            {
                if (!isRuntime)
                {
                    EditorGUILayout.HelpBox("现在不在运行状态！ 需要登陆以后才会显示Reload_Skill 哦", MessageType.Info);
                }
                listScrollViewPos = EditorGUILayout.BeginScrollView(listScrollViewPos, false, true);
                GUILayout.TextArea("技能更新步骤：\n1.点击“打开自动copy程序”，输入 1\n2.在运行时打开此工具，点击“Reload Skill”\n3.建议每换一个ID点一次还原");
                EditorGUILayout.EndScrollView();
                openWindos();
            }
            else
            {
                GUILayout.Label("更新技能请:\n①选择类型\n②输入ID\n③点击 搜索");
            }
        }
        //读取partnerinfo
        public void ReadPartnerInfo()
        {
            XEntityPresentationReader.Reload();
            uint ID = (uint)inPutPresentID;
            searchPresentID = PartnerInfoReader.GetPresentIDByID(ID);
            SkillPath = XEntityPresentationReader.GetSkillLocByPresentId(searchPresentID);
            outPutName = SkillPath.Replace("/", "");
            //Debug.Log(outPutName);
            FIndFileType parterFIndFileType = new FIndFileType();
            if (outPutName != "")
            {
                parterFIndFileType.filePathName = $"{Application.dataPath}/BundleRes/SkillPackage" + "/" + outPutName;
                parterFIndFileType.FindFiles();
                //parterFIndFileType.WriteFiles();
                parterFIndFileType.copySkillName(ref copySkillName);
                if (copySkillName != null)
                {
                    EditorGUIUtility.systemCopyBuffer = copySkillName;
                    isShow = true;

                }

                copySkillName = copySkillName.Replace(";", "\n");
                string reload_gm_Path = Application.dataPath;
                reload_gm_Path = reload_gm_Path.Replace("/Assets", "/Tools/AutoCopy/reload_gm.txt");
                FileStream s = new FileStream(reload_gm_Path, FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(s);
                sw.Write("\n" + copySkillName);
                sw.Close();
            }
            else
            {
                EditorUtility.DisplayDialog("错误！", "输入的partnerID不存在！", "确定");
            }
        }
        public void ReadMonsterInfo()
        {
            UnitAIInfoReader.Reload();
            uint id = (uint)inPutPresentID;
            string r1 = UnitAIInfoReader.ConvertToString(id);
            r1 = r1.Replace(";", "\n");
            if (r1 != "")
            {
                copySkillName = r1;
                EditorGUIUtility.systemCopyBuffer = copySkillName;
                if (r1 != null)
                {
                    isShow = true;
                }
    
                string reload_gm_Path = Application.dataPath;
                reload_gm_Path = reload_gm_Path.Replace("/Assets", "/Tools/AutoCopy/reload_gm.txt");
                FileStream s = new FileStream(reload_gm_Path, FileMode.Append, FileAccess.Write);              
                StreamWriter sw = new StreamWriter(s);
                sw.Write("\n"+ r1);
                sw.Close();
            }
            else
            {
                EditorUtility.DisplayDialog("错误！", "输入的monsterPresentID不存在！", "确定");
            }
        }
        private void AddSkillToRes(List<string> result, string[] skills)
        {
            if (skills != null)
                result.AddRange(skills);
        }
        public void openWindos()
        {
            if (isShow == true)
            {
                EditorUtility.DisplayDialog("技能脚本复制成功", "技能脚本更新指令已复制在剪切板上并且已经在reload_gm内更新！", "确定");
                isShow = false;
            }
        }
        //打开自动拷贝软件
        public void OpenAotuCopy()
        {
            string aotuCopyFilePath = Application.dataPath;
            aotuCopyFilePath = aotuCopyFilePath.Replace("/Assets", "/Tools/AutoCopy/AutoCopyNet.exe");
            Debug.Log(aotuCopyFilePath);
            Process.Start(aotuCopyFilePath, "Tools/AutoCopy/");

        }

        public void OpenAotuCopyWithCommitID()
        {
            EditorPrefs.SetString(CommitKey, commitID);
            string aotuCopyFilePath = Application.dataPath;
            aotuCopyFilePath = aotuCopyFilePath.Replace("/Assets", "/Tools/AutoCopy/AutoCopyNet.exe");
            Debug.Log(aotuCopyFilePath);
            Process.Start(aotuCopyFilePath, $"Tools/AutoCopy/ {commitID}");
        }
    }
}


