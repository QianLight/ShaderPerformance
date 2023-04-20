using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using EditorNode;
using BluePrint;

namespace TDTools
{
    internal class CheckOverTimeWarningNode : BluePrintNode
    {
        static string filepath = Application.dataPath + "/BundleRes/SkillPackage";
        static string outputpath = Application.dataPath + "/Editor/TDTools/CheckOverTimeWarningNode/result.txt";
        static bool None = true;
        static Dictionary<int, float> MyTrggleTime = new Dictionary<int, float>();
        static List<float> ScriptTransList = new List<float>();
        public virtual  BaseSkillGraph GetRoot { get { return (BaseSkillGraph)Root; } }

        [MenuItem("Tools/TDTools/关卡相关工具/CheckOverTimeWarningNode")]
        public static void JudgeWarningtime()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(filepath);
            DirectoryInfo[] SkillPackageNames = directoryInfo.GetDirectories();
            List<string> AllSkillNames = new List<string>();
            FileStream s = new FileStream(outputpath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(s);

            foreach (DirectoryInfo skillPackageName in SkillPackageNames)
            {
                foreach (FileInfo file in skillPackageName.GetFiles())
                {
                    if (file.Extension == ".bytes")
                    {
                        var graph = SkillGraphDataManager.GetSkillGraph(filepath + "/" + skillPackageName.Name + "/" + file.Name);
                        int WaringNodeCount = 0;

                        for (int i = 0; i < graph.widgetList.Count; ++i) (graph.widgetList[i] as BaseSkillNode).TriggerTime = -1;
                        for (int i = 0; i < graph.widgetList.Count; ++i)
                        {
                            (graph.widgetList[i] as BaseSkillNode).CalcTriggerTime();
                            string a = graph.widgetList[i].GetType().ToString();
                            if (a == "EditorNode.WarningNode")
                            {
                                MyTrggleTime.Add(WaringNodeCount, (graph.widgetList[i] as BaseSkillNode).TriggerTime);
                                WaringNodeCount++;
                            }
                            else if (a == "EditorNode.ScriptTransNode" && (graph.widgetList[i] as BaseSkillNode).TriggerTime >= 0)
                            {
                                ScriptTransList.Add((graph.widgetList[i] as BaseSkillNode).TriggerTime);
                            }
                        }
                        WaringNodeCount = 0;

                        foreach (var node in graph.configData.WarningData)
                        {

                            float warningruntime = MyTrggleTime[WaringNodeCount] + node.LifeTime;

                            float graphruntime = graph.Length;
                            if (ScriptTransList.Count > 0)
                            {
                                foreach (float t in ScriptTransList)
                                {
                                    if (graphruntime > t)
                                        graphruntime = t;
                                }
                            }
                            

                            if(warningruntime > graphruntime)
                            {
                                sw.Write($"{file.Name}\t WarningNode{node.Index}\t 结束时间：{MyTrggleTime[WaringNodeCount]}+{node.LifeTime}\t 脚本时长：{graphruntime}\t 时间超出：{warningruntime - graphruntime}s\n");
                                None = false;                               
                            }
                            WaringNodeCount++;
                        }
                        MyTrggleTime.Clear();
                        ScriptTransList.Clear();
                    }
                }
            }
            if (None)
            {
                sw.Write($"完全不存在warning节点时长超出脚本时长的情况");
            }
            sw.Close();
        }
    //    public static void JudgeWarningtime1()
    //    {
    //        DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/BundleRes/SkillPackage/Monster_Alvida/");
    //        FileStream s = new FileStream(outputpath, FileMode.Create, FileAccess.Write);
    //        StreamWriter sw = new StreamWriter(s);

    //            foreach (FileInfo file in directoryInfo.GetFiles())
    //            {
    //                if (file.Extension == ".bytes")
    //                {
    //                    var graph = SkillGraphDataManager.GetSkillGraph(filepath + "/" + "Monster_Alvida" + "/" + file.Name);
    //                    int WaringNodeCount = 0;

    //                    for (int i = 0; i < graph.widgetList.Count; ++i) (graph.widgetList[i] as BaseSkillNode).TriggerTime = -1;
    //                    for (int i = 0; i < graph.widgetList.Count; ++i)
    //                    {
    //                        (graph.widgetList[i] as BaseSkillNode).CalcTriggerTime();
    //                        string a = graph.widgetList[i].GetType().ToString();
    //                        if (a == "EditorNode.WarningNode")
    //                        {
                                
    //                            MyTrggleTime.Add(WaringNodeCount, (graph.widgetList[i] as BaseSkillNode).TriggerTime);
    //                            WaringNodeCount++;
    //                        }
    //                    }
    //                    WaringNodeCount = 0;

    //                    foreach (var node in graph.configData.WarningData)
    //                    {
    //                        float warningruntime = MyTrggleTime[WaringNodeCount] + node.LifeTime;
                            
    //                        float graphruntime = graph.Length;

                       
    //                        sw.Write($"{file.Name}\t WarningNode{node.Index}\t 结束时间：{MyTrggleTime[WaringNodeCount]}+{node.LifeTime}\t 脚本时长：{graphruntime}\t 时间超出：{warningruntime - graphruntime}s\n");
    //                        None = false;
    //                        WaringNodeCount++;
    //                    }
    //                MyTrggleTime.Clear();
    //                }
    //            }
            
    //        if (None)
    //        {
    //            sw.Write($"完全不存在warning节点时长超出脚本时长的情况");
    //        }
    //        sw.Close();
    //    }
    }
}
