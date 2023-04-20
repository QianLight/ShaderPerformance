using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using LevelEditor;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using CFUtilPoolLib;
using BluePrint;

namespace TDTools
{
    public class LevelCutsceneExporter : EditorWindow
    {
        private static readonly string LevelFilePath = "/BundleRes/Table/";
        private static readonly string WaveDataExporterWinUxmlPath = "Assets/Editor/TDTools/LevelWaveDataExport/LevelWaveDataExporter.uxml";

        //private static Dictionary<uint, HeadDialogData> talkDic = new Dictionary<uint, HeadDialogData>();

        [MenuItem("Tools/TDTools/关卡相关工具/关卡脚本导出导回【文案】")]
        public static void ShowWindow()
        {
            var win = GetWindowWithRect<LevelCutsceneExporter>(new Rect(0, 0, 600, 400));
            win.Show();
        }
        private void OnEnable()
        {
            MapListReader.Reload();
            //LevelHelper.ReadLevelCustomConfig();
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(WaveDataExporterWinUxmlPath);//读取uxml文件
            vta.CloneTree(rootVisualElement);//rootVisualElement 标签下的根窗口 在rootVisualElement下实例化或克隆VisualTreeAsset
            rootVisualElement.Q<Button>("SaveBtn").RegisterCallback<MouseUpEvent>(obj => { SaveFile(); });
            rootVisualElement.Q<Button>("OpenBtn").RegisterCallback<MouseUpEvent>(obj => { EditorUtility.RevealInFinder($"{Application.dataPath}/Editor/TDTools/LevelWaveDataExport/LevelWaveDataExporter.cs"); });
            Button Importer = new Button(()=>DoImporter("bubble"));
            Button Importer1 = new Button(() => DoImporter("Notice"));
            Button Importer2 = new Button(() => DoImporter("Mission"));
            Button Importer3 = new Button(() => DoImporter("stringbuild"));
            Button Importer4 = new Button(() => DoImporter("callUI"));
            Button Importer5 = new Button(() => DoImporter("ShowBossName"));
            Importer.name = "ImpBtn";
            Importer1.name = "ImpBtn1";
            Importer2.name = "ImpBtn2";
            Importer.text = "文案(bubble)导入关卡脚本";
            Importer1.text = "文案(notice)导入关卡脚本";
            Importer2.text = "文案(mission)导入关卡脚本";
            Importer3.text = "文案(stringbuild)导入关卡脚本";
            Importer4.text = "文案(callUI)导入关卡脚本";
            Importer5.text = "文案(ShowBossName)导入关卡脚本";
            rootVisualElement.Add(Importer);
            rootVisualElement.Add(Importer1);
            rootVisualElement.Add(Importer2);
            rootVisualElement.Add(Importer3);
            rootVisualElement.Add(Importer4);
            rootVisualElement.Add(Importer5);
        }

        private void SaveFile()
        {
            DoExporter();
        }
        public void DoExporter()
        {
            MapListReader.Reload();
            HeadDialogReader.Reload();
            HeadIconMgr talkMgr = new HeadIconMgr();
            var rows = MapListReader.MapList.Table;
            List<string> scriptNameList = new List<string>(); //这个是为了保证导出的节点唯一（由于一个cfg文件可能对应多个mapID,所以可能会出现同个cfg文件多次导出的问题）

            StringBuilder talk = new StringBuilder();
            talk.AppendLine("MapID\tScriptName\tgraphID\tnodeID\ttalkID\tNextID\tName\tTalkStr");
            talk.AppendLine("MapID\t脚本名\t关卡图ID\t节点ID\ttalkID\t下一序号\t名字\t说话者内容颜色:<color=#00ffffff>TestTest   (DialogType类型为3时 *代表空格, {n}为换行)");
            StringBuilder output = new StringBuilder();
            output.AppendLine("tScriptName\tgraphID\tnodeID\tISmp4\tCutScene");
            output.AppendLine("脚本名\t关卡图ID\t节点ID\t是否mp4\tCutScene");
            StringBuilder bubble = new StringBuilder();
            bubble.AppendLine("MapID\tScriptName\tgraphID\tnodeID\tMonsterID\tMonsterName\tText");
            bubble.AppendLine("MapID\t脚本名\t关卡图ID\t节点ID\t怪物ID\t怪物Name\tText");
            StringBuilder showBossName = new StringBuilder();
            showBossName.AppendLine("MapID\tScriptName\tgraphID\tnodeID\tBossName\tEnglishName\tTrait\tAnswer\tSign");
            showBossName.AppendLine("MapID\t脚本名\t关卡图ID\t节点ID\tBoss中文名\tBoss英文名\tTrait\tAnswer\tSign");
            StringBuilder notice = new StringBuilder();
            notice.AppendLine("MapID\tScriptName\tgraphID\tnodeID\tText");
            notice.AppendLine("MapID\t脚本名\t关卡图ID\t节点ID\tText");
            StringBuilder mission = new StringBuilder();
            mission.AppendLine("MapID\tScriptName\tgraphID\tnodeID\tname\tdescription");
            mission.AppendLine("MapID\t脚本名\t关卡图ID\t节点ID\t名称\t描述");
            StringBuilder callUI = new StringBuilder();
            callUI.AppendLine("MapID\tScriptName\tgraphID\tnodeID\tUIName\tUIString");
            callUI.AppendLine("MapID\t脚本名\t关卡图ID\t节点ID\tUI名称\tUI字符");
            StringBuilder stringbuild = new StringBuilder();
            stringbuild.AppendLine("MapID\tScriptName\tgraphID\tnodeID\tstr");
            stringbuild.AppendLine("MapID\t脚本名\t关卡图ID\t节点ID\t字符串");
            StringBuilder showbossname = new StringBuilder();
            showbossname.AppendLine("MapID\tScriptName\tgraphID\tnodeID\tBossName\tEnglishName\tTrait\tAnswer\tSign");
            showbossname.AppendLine("MapID\t脚本名\t关卡图ID\t节点ID\t中文名\t英文名\t特点\t应对手段\tSign");
            foreach (var item in rows)
            {
                if (string.IsNullOrEmpty(item.LevelConfigFile))
                    continue;
                if(!LevelEditorTools.IsCfgLoad(item.LevelConfigFile, scriptNameList))
                {
                    string path = $"{Application.dataPath}{LevelFilePath}/{item.LevelConfigFile}.cfg";
                    var grphData = DataIO.DeserializeData<LevelEditorData>(path);
                    if (grphData != null)
                    {
                        Debug.Log($"{item.MapID}:begin");
                        foreach (var graph in grphData.GraphDataList)
                        {
                            foreach (var node in graph.ScriptData)
                            {
                                scriptNameList.Add(item.LevelConfigFile);
                                switch (node.Cmd)
                                {
                        //            //case LevelScriptCmd.Level_Cmd_Cutscene:
                        //            //    bool bMp4 = node.valueParam.Count > 1 ? (node.valueParam[1] > 0 ? true : false) : false;
                        //            //    output.AppendLine($"{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{bMp4}\t{node.stringParam[0]}");
                        //            //    break;
                                    case LevelScriptCmd.Level_Cmd_Bubble:
                                        if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                        {
                                            var row = XEntityStatisticsReader.GetDataBySid((uint)node.valueParam[0]);
                                            if (row != null)
                                                bubble.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}\t{row.Name}\t{ToSBC(node.stringParam[0])}");
                                            else
                                                bubble.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}\t\t{ToSBC(node.stringParam[0])}");
                                            //Debug.Log(node.) 
                                        }
                                        break;
                        //            //case LevelScriptCmd.Level_Cmd_ShowBossName:
                        //            //    showBossName.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.stringParam[0]}\t{node.stringParam[1]}\t{node.stringParam[2]}\t{node.stringParam[3]}\t{node.stringParam[4]}");
                        //            //    break;
                                   case LevelScriptCmd.Level_Cmd_Notice:
                                        if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                        {
                                            notice.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{ToSBC(node.stringParam[0])}");
                                        }
                                        break;
                                    case LevelScriptCmd.Level_Cmd_Talk:
                                        if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                        {
                                            //var id = (uint)node.valueParam[0];
                                            //talkMgr.AddResult(id, item.MapID);
                                            uint talkID = (uint)node.valueParam[0];
                                            var dialog = HeadDialogReader.GetData(talkID);
                                            if(dialog != null)
                                            {
                                                string talkStr = string.Join("", HeadDialogReader.GetTalkContent(talkID));
                                                talk.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}\t{dialog.NextID}\t{dialog.Name}\t{talkStr}");
                                                for(int i=0;i<1000;i++)
                                                {
                                                    uint nextID = dialog.NextID;
                                                    if (nextID != 0)
                                                    {
                                                        dialog = HeadDialogReader.GetData(nextID);
                                                        if(dialog!=null)
                                                        {
                                                            talkStr = string.Join("", HeadDialogReader.GetTalkContent(nextID));
                                                            talk.AppendLine($"\t\t\t\t{nextID}\t{dialog.NextID}\t{dialog.Name}\t{talkStr}");
                                                        }
                                                        else if(dialog==null)
                                                        {
                                                            talk.AppendLine($"\t\t\t\t{nextID}");
                                                            break;
                                                        }
                                                    }
                                                    else break;
                                                }
                                            }
                                            else talk.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}");
                                        }
                                        break;
                                    case LevelScriptCmd.Level_Cmd_Custom:
                                        if (node.stringParam[0] == "Mission")
                                            if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                            {
                                                mission.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{ToSBC(node.stringParam[1])}\t{ToSBC(node.stringParam[2])}");
                                            }
                                        break;
                                    case LevelScriptCmd.Level_Cmd_CallUI:
                                        if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                        {
                                            callUI.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{ToSBC(node.stringParam[0])}\t{ToSBC(node.stringParam[1])}");
                                        }
                                        break;
                                    case LevelScriptCmd.Level_Cmd_ShowBossName:
                                        if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                        {
                                            showbossname.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{ToSBC(node.stringParam[0])}\t{ToSBC(node.stringParam[1])}\t{ToSBC(node.stringParam[2])}\t{ToSBC(node.stringParam[3])}");
                                        }
                                        break;
                                }
                            }
                            foreach(var node in graph.StringBuildData)
                            {
                                stringbuild.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{ToSBC(node.str)}");
                            }
                        }
                    }
                    Debug.Log($"{item.MapID}:end");
                }  
            }
            //string savePath = $"{Application.dataPath}/Editor/TDTools/LevelWaveDataExport/cutscene里mp4情况.txt";
            StringBuilder debug=new StringBuilder();
            debug.Append(SaveFile(bubble, "bubble配置情况.txt"));
            debug.Append(SaveFile(notice, "Notice配置情况.txt"));
            debug.Append(SaveFile(mission, "Mission配置情况.txt"));
            debug.Append(SaveFile(talk, "Talk配置情况.txt"));
            debug.Append(SaveFile(callUI, "callUI配置情况.txt"));
            debug.Append(SaveFile(showbossname, "ShowBossName配置情况.txt"));
            debug.Append(SaveFile(stringbuild, "stringbuild配置情况.txt"));
            if(debug.Length>1)
                ShowNotification(new GUIContent($"{debug}已打开，无法保存。\n请先关闭表格"), 5);
            else
                ShowNotification(new GUIContent("导出成功！"), 5);
            //string showBossNameSavePath = $"{Application.dataPath}/Editor/TDTools/LevelWaveDataExport/showBossName配置情况.txt";
        }
        
        public void DoImporter(string tableName)
        {
            string tablePath= $"{Application.dataPath}/Editor/TDTools/LevelWaveDataExport/{tableName}配置情况.txt";

            var table = TDTableReadHelper.GetTableFullData(tablePath, true, "gb2312");

            int MapIDIndex = 0;
            int ScriptNameIndex = 1;
            int GraphIDIndex = 2;
            int NodeIDIndex = 3;

            int modifyNum = 0;
            MapListReader.Reload();
            var rows = MapListReader.MapList.Table;
            
            foreach (var item in rows)
            {
                if (string.IsNullOrEmpty(item.LevelConfigFile))
                    continue;
                string path = $"{Application.dataPath}{LevelFilePath}/{item.LevelConfigFile}.cfg";
                var grphData = DataIO.DeserializeData<LevelEditorData>(path); 
                if(grphData!=null)
                {
                    foreach (var graph in grphData.GraphDataList)
                    {
                        bool isChanged = false;
                        if (tableName == "stringbuild")
                        {
                            foreach (var node in graph.StringBuildData)
                            {
                                //if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                //{
                                    for (int i = 0; i < table.dataList.Count; i++)
                                    {
                                        //bubble.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}\t{row.Name}\t{node.stringParam[0]}")
                                        if (uint.Parse(table.dataList[i].valueList[MapIDIndex]) == item.MapID && table.dataList[i].valueList[ScriptNameIndex] == item.LevelConfigFile && uint.Parse(table.dataList[i].valueList[GraphIDIndex]) == graph.graphID && uint.Parse(table.dataList[i].valueList[NodeIDIndex]) == node.NodeID)
                                        {
                                            string Param = ToSBC(node.str);
                                            if (Param != table.dataList[i].valueList[NodeIDIndex + 1])
                                            {
                                                Debug.Log(node.str);
                                                Debug.Log(table.dataList[i].valueList[NodeIDIndex + 1]);
                                                node.str = ToSBC(table.dataList[i].valueList[NodeIDIndex + 1]);
                                                isChanged = true;
                                                modifyNum++;
                                            }
                                            table.dataList.Remove(table.dataList[i]); //找到一个删一个，加快修改效率
                                            break;
                                        }
                                    //}
                                }
                            }
                        }
                        else
                        {
                            foreach (var node in graph.ScriptData)
                            {
                                switch (node.Cmd)
                                {
                                    case LevelScriptCmd.Level_Cmd_Bubble:
                                        if (tableName == "bubble")
                                        {
                                            if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                            {
                                                for (int i = 0; i < table.dataList.Count; i++)
                                                {
                                                    //bubble.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}\t{row.Name}\t{node.stringParam[0]}")
                                                    if (uint.Parse(table.dataList[i].valueList[MapIDIndex]) == item.MapID && table.dataList[i].valueList[ScriptNameIndex] == item.LevelConfigFile && uint.Parse(table.dataList[i].valueList[GraphIDIndex]) == graph.graphID && uint.Parse(table.dataList[i].valueList[NodeIDIndex]) == node.NodeID)
                                                    {
                                                        string Param = ToSBC(node.stringParam[0]);
                                                        if (node.valueParam[0] != float.Parse(table.dataList[i].valueList[NodeIDIndex + 1]) || Param != table.dataList[i].valueList[NodeIDIndex + 3])
                                                        {
                                                            Debug.Log(node.valueParam[0] + node.stringParam[0]);
                                                            Debug.Log(float.Parse(table.dataList[i].valueList[NodeIDIndex + 1]) + table.dataList[i].valueList[NodeIDIndex + 3]);
                                                            node.valueParam[0] = float.Parse(table.dataList[i].valueList[NodeIDIndex + 1]);
                                                            node.stringParam[0] = ToSBC(table.dataList[i].valueList[NodeIDIndex + 3]);
                                                            isChanged = true;
                                                            modifyNum++;
                                                        }
                                                        table.dataList.Remove(table.dataList[i]); //找到一个删一个，加快修改效率
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case LevelScriptCmd.Level_Cmd_Notice:
                                        if (tableName == "Notice")
                                        {
                                            if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                            {
                                                for (int i = 0; i < table.dataList.Count; i++)
                                                {
                                                    //bubble.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}\t{row.Name}\t{node.stringParam[0]}")
                                                    if (uint.Parse(table.dataList[i].valueList[MapIDIndex]) == item.MapID && table.dataList[i].valueList[ScriptNameIndex] == item.LevelConfigFile && uint.Parse(table.dataList[i].valueList[GraphIDIndex]) == graph.graphID && uint.Parse(table.dataList[i].valueList[NodeIDIndex]) == node.NodeID)
                                                    {
                                                        string Param = ToSBC(node.stringParam[0]);
                                                        if (Param != table.dataList[i].valueList[NodeIDIndex + 1])
                                                        {
                                                            Debug.Log(node.stringParam[0]);
                                                            Debug.Log(table.dataList[i].valueList[NodeIDIndex + 1]);
                                                            node.stringParam[0] = ToSBC(table.dataList[i].valueList[NodeIDIndex + 1]);
                                                            isChanged = true;
                                                            modifyNum++;
                                                        }
                                                        table.dataList.Remove(table.dataList[i]); //找到一个删一个，加快修改效率
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    //case LevelScriptCmd.Level_Cmd_Talk:
                                    //    if (IsNodeDisabled(graph, node, node.NodeID))
                                    //    {
                                    //        for (int i = 0; i < talkTable.dataList.Count; i++)
                                    //        {
                                    //            //bubble.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}\t{row.Name}\t{node.stringParam[0]}")
                                    //            if (uint.Parse(talkTable.dataList[i].valueList[MapIDIndex]) == item.MapID && talkTable.dataList[i].valueList[ScriptNameIndex] == item.LevelConfigFile && uint.Parse(talkTable.dataList[i].valueList[GraphIDIndex]) == graph.graphID && uint.Parse(talkTable.dataList[i].valueList[NodeIDIndex]) == node.NodeID)
                                    //            {
                                    //                if (node.valueParam[0] != float.Parse(talkTable.dataList[i].valueList[NodeIDIndex + 1]))
                                    //                {
                                    //                    Debug.Log(node.valueParam[0]);
                                    //                    Debug.Log(talkTable.dataList[i].valueList[NodeIDIndex + 1]);
                                    //                    node.valueParam[0] = float.Parse(talkTable.dataList[i].valueList[NodeIDIndex + 1]);
                                    //                    isChanged = true;
                                    //                }
                                    //                talkTable.dataList.Remove(talkTable.dataList[i]); //找到一个删一个，加快修改效率
                                    //                break;
                                    //            }
                                    //        }
                                    //    }
                                    //    break;
                                    case LevelScriptCmd.Level_Cmd_Custom:
                                        if (tableName == "Mission" && node.stringParam[0] == "Mission")
                                        {
                                            if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                            {
                                                for (int i = 0; i < table.dataList.Count; i++)
                                                {
                                                    //bubble.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}\t{row.Name}\t{node.stringParam[0]}")
                                                    if (uint.Parse(table.dataList[i].valueList[MapIDIndex]) == item.MapID && table.dataList[i].valueList[ScriptNameIndex] == item.LevelConfigFile && uint.Parse(table.dataList[i].valueList[GraphIDIndex]) == graph.graphID && uint.Parse(table.dataList[i].valueList[NodeIDIndex]) == node.NodeID)
                                                    {
                                                        string Param = ToSBC(node.stringParam[1]);
                                                        string Param1 = ToSBC(node.stringParam[2]);
                                                        if (Param != table.dataList[i].valueList[NodeIDIndex + 1] || Param1 != table.dataList[i].valueList[NodeIDIndex + 2])
                                                        {
                                                            Debug.Log(node.stringParam[1] + node.stringParam[2]);
                                                            Debug.Log(table.dataList[i].valueList[NodeIDIndex + 1] + table.dataList[i].valueList[NodeIDIndex + 2]);
                                                            node.stringParam[1] = ToSBC(table.dataList[i].valueList[NodeIDIndex + 1]);
                                                            node.stringParam[2] = ToSBC(table.dataList[i].valueList[NodeIDIndex + 2]);
                                                            isChanged = true;
                                                            modifyNum++;
                                                        }
                                                        table.dataList.Remove(table.dataList[i]); //找到一个删一个，加快修改效率
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case LevelScriptCmd.Level_Cmd_CallUI:
                                        if (tableName == "callUI")
                                        {
                                            if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                            {
                                                for (int i = 0; i < table.dataList.Count; i++)
                                                {
                                                    //bubble.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}\t{row.Name}\t{node.stringParam[0]}")
                                                    if (uint.Parse(table.dataList[i].valueList[MapIDIndex]) == item.MapID && table.dataList[i].valueList[ScriptNameIndex] == item.LevelConfigFile && uint.Parse(table.dataList[i].valueList[GraphIDIndex]) == graph.graphID && uint.Parse(table.dataList[i].valueList[NodeIDIndex]) == node.NodeID)
                                                    {
                                                        string Param = ToSBC(node.stringParam[1]);
                                                        if (Param != table.dataList[i].valueList[NodeIDIndex + 2])
                                                        {
                                                            Debug.Log(node.stringParam[1]);
                                                            Debug.Log(table.dataList[i].valueList[NodeIDIndex + 2]);
                                                            node.stringParam[1] = ToSBC(table.dataList[i].valueList[NodeIDIndex + 2]);
                                                            isChanged = true;
                                                            modifyNum++;
                                                        }
                                                        table.dataList.Remove(table.dataList[i]); //找到一个删一个，加快修改效率
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case LevelScriptCmd.Level_Cmd_ShowBossName:
                                        if (tableName == "ShowBossName")
                                        {
                                            if (LevelEditorTools.IsNodeEnable(graph, node, node.NodeID, item.LevelConfigFile))
                                            {
                                                for (int i = 0; i < table.dataList.Count; i++)
                                                {
                                                    //bubble.AppendLine($"{item.MapID}\t{item.LevelConfigFile}\t{graph.graphID}\t{node.NodeID}\t{node.valueParam[0]}\t{row.Name}\t{node.stringParam[0]}")
                                                    if (uint.Parse(table.dataList[i].valueList[MapIDIndex]) == item.MapID && table.dataList[i].valueList[ScriptNameIndex] == item.LevelConfigFile && uint.Parse(table.dataList[i].valueList[GraphIDIndex]) == graph.graphID && uint.Parse(table.dataList[i].valueList[NodeIDIndex]) == node.NodeID)
                                                    {
                                                        string Param = ToSBC(node.stringParam[0]);
                                                        string Param1 = ToSBC(node.stringParam[1]);
                                                        string Param2 = ToSBC(node.stringParam[2]);
                                                        string Param3 = ToSBC(node.stringParam[3]);
                                                        if ( Param != table.dataList[i].valueList[NodeIDIndex + 1]|| Param1 != table.dataList[i].valueList[NodeIDIndex + 2]|| Param2 != table.dataList[i].valueList[NodeIDIndex + 3] || Param3 != table.dataList[i].valueList[NodeIDIndex + 4])
                                                        {
                                                            Debug.Log(node.stringParam[0] + node.stringParam[1] + node.stringParam[2] + node.stringParam[3]);
                                                            Debug.Log(table.dataList[i].valueList[NodeIDIndex + 1]+table.dataList[i].valueList[NodeIDIndex + 2]+ table.dataList[i].valueList[NodeIDIndex + 3]+ table.dataList[i].valueList[NodeIDIndex + 4]);
                                                            node.stringParam[0] = ToSBC(table.dataList[i].valueList[NodeIDIndex + 1]);
                                                            node.stringParam[1] = ToSBC(table.dataList[i].valueList[NodeIDIndex + 2]);
                                                            node.stringParam[2] = ToSBC(table.dataList[i].valueList[NodeIDIndex + 3]);
                                                            node.stringParam[3] = ToSBC(table.dataList[i].valueList[NodeIDIndex + 4]);
                                                            isChanged = true;
                                                            modifyNum++;
                                                        }
                                                        table.dataList.Remove(table.dataList[i]); //找到一个删一个，加快修改效率
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        
                        

                        if (isChanged)
                            DataIO.SerializeData(path, grphData);
                    }
                }
            }

            if(modifyNum==0)
                ShowNotification(new GUIContent("没有修改"), 5);
            else
                ShowNotification(new GUIContent($"修改项为{modifyNum}个\n请至fork的Assets\\BundleRes\\Table\\Level处检查修改"), 6);
        }
        public string SaveFile(StringBuilder file, string name)
        {
            string SavePath = $"{Application.dataPath}/Editor/TDTools/LevelWaveDataExport/{name}";
            try
            {
                File.WriteAllText(SavePath, file.ToString(), Encoding.GetEncoding("gb2312"));
            }
            catch
            {
                return name;
            }
            return null;
        }


        //半角转全角，只转, ! " 
        public static string ToSBC(string input)
        {
            char[] array = input.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == 33|| array[i] == 44 || array[i] == 34 )
                {
                    array[i] = (char)(array[i] + 65248);
                    continue;
                }
            }
            return new string(array);
        }
    }

    public static class LevelEditorTools
    {
        public static bool IsNodeEnable<T>(LevelGraphData graph, T node, int NodeID, string cfgName="") where T : BluePrintNodeBaseData
        {
            if (!node.enabled)    //排除右键disabled的节点
                return false;

            int goID = -1;
            int flyID = -1;
            for (int i = 0; i < graph.ControllerData.Count; i++)
            {
                if (graph.ControllerData[i].TypeID == 1 || graph.ControllerData[i].TypeID == 15) //go type=1,graphgo type=15
                    goID = graph.ControllerData[i].NodeID;
                else if (graph.ControllerData[i].TypeID == 17)
                    flyID = graph.ControllerData[i].NodeID;
            }
            int leftNodeID = FindLeftNode(graph, NodeID, goID,flyID);

            if (leftNodeID < 0)
                return false;  //排除左边没有连线的节点
            //if (leftNodeID == goID || leftNodeID == flyID)
            //    return true;
            //
            ////排除左边有连线，但是没有连到go或fly上的节点
            ////一张图go和fly都有
            //if (goID > 0 && flyID > 0)
            //{
            //    do
            //    {
            //        leftNodeID = FindLeftNode(graph, leftNodeID, goID, flyID);
            //        if (leftNodeID < 0)
            //            return false;
            //    } while (leftNodeID != goID && leftNodeID != flyID);
            //    return true;  //说明连到了go或fly上
            //}
            //else if (goID > 0 && flyID < 0)
            //{
            //    do
            //    {
            //        leftNodeID = FindLeftNode(graph, leftNodeID, goID, flyID);
            //        if (leftNodeID < 0)
            //            return false;
            //    } while (leftNodeID != goID);
            //    return true;  //说明连到了go上
            //}
            //else if (goID < 0 && flyID > 0)
            //{
            //    do
            //    {
            //        leftNodeID = FindLeftNode(graph, leftNodeID, goID, flyID);
            //        if (leftNodeID < 0)
            //            return false;
            //    } while (leftNodeID != flyID);
            //    return true;  //说明连到了fly上
            //}
            //
            //Debug.Log($"{cfgName}+{graph.graphID} 没有go或fly");
            return true;
        }

        public static int FindLeftNode(LevelGraphData graph, int nodeID ,int goID ,int flyID)
        {
            int leftNodeID = -1;

            foreach (var i in graph.ConnectionData)
            {
                if (i.EndNode == nodeID)
                {
                    if (i.StartNode == goID || i.StartNode == flyID)
                    {
                        leftNodeID = i.StartNode;
                        return leftNodeID;
                    }
                    else if (i.EndPin < 0)
                    {
                        leftNodeID = i.StartNode;
                        return leftNodeID;
                    }
                }
            }
            return leftNodeID;
        }

        //这个是为了保证导出的节点唯一（由于一个cfg文件可能对应多个mapID,所以可能会出现同个cfg文件多次导出的问题）
        public static bool IsCfgLoad(string cfgName, List<string> scriptNameList)
        {
            bool isLoad = false;
            for (int i = 0; i < scriptNameList.Count; i++)
            {
                if (scriptNameList[i] == cfgName)
                {
                    isLoad = true;
                    break;
                }
            }
            return isLoad;
        }
    }

    public class HeadIconMgr
    {
        public Dictionary<uint, HeadDialogData> IconPool;
        public Dictionary<string, HeadIconData> ResultDic;

        public HeadIconMgr()
        {
            IconPool = new Dictionary<uint, HeadDialogData>();
            ResultDic = new Dictionary<string, HeadIconData>();
        }

        public HeadDialogData GetDialogData(uint startID)
        {
            if (!IconPool.ContainsKey(startID))
            {
                IconPool.Add(startID, new HeadDialogData(startID));
            }
            return IconPool[startID];
        }

        public void AddResult(uint startID, uint map)
        {
            var dialogData = GetDialogData(startID);
            foreach (var item in dialogData.IconList)
            {
                if (!ResultDic.ContainsKey(item.Key))
                {
                    var list = new HeadIconData(item.Value.Icon, item.Value.Name);
                    ResultDic.Add(item.Key, list);
                }
                ResultDic[item.Key].AddMapId(map);
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach (var item in ResultDic)
            {
                output.AppendLine(item.Value.ToString());
            }
            return output.ToString();
        }
    }

    public class HeadIconData
    {
        public string Icon;
        public string Name;
        public List<uint> MapList;

        public HeadIconData(string icon, string name)
        {
            Icon = icon;
            Name = name;
            MapList = new List<uint>();
        }

        public void AddMapId(uint map)
        {
            if (!MapList.Contains(map))
                MapList.Add(map);
        }

        public override string ToString()
        {
            string map;
            if (MapList.Count > 1)
                map = string.Join(";", MapList);
            else
                map = MapList[0].ToString();
            return $"{Icon}\t{Name}\t{map}";
        }
    }

    public class HeadDialogData
    {
        public uint StartId;
        public Dictionary<string, HeadIconData> IconList;

        public HeadDialogData(uint startID, bool onlySp = false)
        {
            var item = HeadDialogReader.GetData(startID);
            StartId = startID;
            IconList = new Dictionary<string, HeadIconData>();
            while (item != null)
            {
                string header = string.Empty;
                if (item.Header.Dim >= 2)
                {
                    header = item.Header[1];
                }
                if (item.SpecialHeadPath != string.Empty)
                {
                    header = item.SpecialHeadPath;
                }
                if (header == string.Empty || IconList.ContainsKey(header))
                    break;
                if (!onlySp || item.DialogType == 2)
                {
                    var iconData = new HeadIconData(header, item.Name);
                    IconList.Add(header, iconData);
                }
                item = HeadDialogReader.GetData(item.NextID);
            }
        }
    }
}




