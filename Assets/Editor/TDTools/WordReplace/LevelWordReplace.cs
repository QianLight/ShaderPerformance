using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LevelEditor;
using BluePrint;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TDTools
{
    [Flags]
    public enum WordReplaceNode
    {
        Cutscene = 1,
        Bubble = 2,
        ShowBossName = 4,
        Notice = 8,
        Mission = 16,
        CallUI = 32,
        StringBuild = 64
    }
    public class LevelWordReplaceWindow : EditorWindow
    {
        private static readonly string LevelWordReplaceWinUxmlPath = "Assets/Editor/TDTools/WordReplace/LevelWordReplace.uxml";
        private LevelEditorData fullData;
        private EditorConfigData editorConfigData;
        private FileSelectorData SelectFsd;
        private List<LevelWordReplaceNodeData> NodeList = new List<LevelWordReplaceNodeData>();
        private ListView listView;
        private LevelWordReplaceWinData lwrwd;

        [MenuItem("Tools/TDTools/关卡相关工具/关卡文案配置工具")]
        public static void ShowWindow()
        {
            var win = GetWindowWithRect<LevelWordReplaceWindow>(new Rect(0, 0, 600, 720));
            win.Show();
        }

        private void OnEnable()
        {
            LevelHelper.ReadLevelCustomConfig();
            lwrwd = CreateInstance<LevelWordReplaceWinData>();
            var so = new SerializedObject(lwrwd);
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LevelWordReplaceWinUxmlPath);
            vta.CloneTree(rootVisualElement);
            rootVisualElement.Bind(so);
            InitTemplate();
            NodeList.Clear();
            var enumField = rootVisualElement.Q<EnumFlagsField>("TypeSelect");
            enumField.Init(WordReplaceNode.Cutscene);
            rootVisualElement.Q<Button>("SaveBtn").RegisterCallback<MouseUpEvent>(obj => { SaveFile(); });
            listView = rootVisualElement.Q<ListView>("ItemList");
            listView.makeItem = () => { return new IMGUIContainer(); };
            listView.bindItem = (e, i) => CreateListViewItem(e, NodeList[i]);
            listView.itemHeight = 150;
        }

        private void InitTemplate()
        {
            var select = rootVisualElement.Q<TemplateContainer>("FileSelector");
            SelectFsd = CreateInstance<FileSelectorData>();
            SelectFsd.FileTitle = "关卡脚本";
            select.BindAndRegister(SelectFsd, obj => { OpenLevel(); });
        }

        private void OpenLevel()
        {
            NodeList.Clear();
            SelectFsd.FilePath = EditorUtility.OpenFilePanel("Select level file", Application.dataPath + "/BundleRes/Table/Level/", "cfg");
            fullData = DataIO.DeserializeData<LevelEditorData>(SelectFsd.FilePath);
            editorConfigData = DataIO.DeserializeData<EditorConfigData>(SelectFsd.FilePath.Replace(".cfg", ".ecfg"));
            foreach (var graph in fullData.GraphDataList)
            {
                var editorData = editorConfigData.GetGraphConfigByID(graph.graphID);
                foreach (var node in graph.ScriptData)
                {
                    if(node.Cmd == LevelScriptCmd.Level_Cmd_Cutscene || 
                       node.Cmd == LevelScriptCmd.Level_Cmd_Bubble || 
                       node.Cmd == LevelScriptCmd.Level_Cmd_Notice || 
                       node.Cmd == LevelScriptCmd.Level_Cmd_ShowBossName || 
                       (node.Cmd == LevelScriptCmd.Level_Cmd_Custom && node.stringParam[0] == "Mission") ||
                       node.Cmd == LevelScriptCmd.Level_Cmd_CallUI)
                    {
                        var nodeEditorData = editorData.GetConfigDataByID(node.NodeID);
                        NodeList.Add(new LevelWordReplaceNodeData(graph.graphID, graph.name, node, null, nodeEditorData));
                    }
                }
                foreach (var node in graph.StringBuildData)
                {
                    var nodeEditorData = editorData.GetConfigDataByID(node.NodeID);
                    NodeList.Add(new LevelWordReplaceNodeData(graph.graphID, graph.name, null, node, nodeEditorData));
                }
            }
            listView.itemsSource = NodeList;
        }
        private void CreateListViewItem(VisualElement item, LevelWordReplaceNodeData data)
        {
            var container = item as IMGUIContainer;
            container.onGUIHandler = () =>
            {
                int selectedTypes = Convert.ToInt32(rootVisualElement.Q<EnumFlagsField>("TypeSelect").value);
                data.ShowData(selectedTypes);
            };
        }

        private void SaveFile()
        {
            DataIO.SerializeData(SelectFsd.FilePath, fullData);
            Debug.Log($"{SelectFsd.FilePath}");
        }
    }

    public class LevelWordReplaceWinData : ScriptableObject
    {
        public WordReplaceNode FilterType;
    }

    public class LevelWordReplaceNodeData
    {
        public int GraphID;
        public string GraphName;
        public LevelScriptData Data;
        public LevelStringBuildData StringData;
        public NodeConfigData EditorData;

        public LevelWordReplaceNodeData(int id, string name, LevelScriptData iScriptData, LevelStringBuildData iStringData, NodeConfigData editorData)
        {
            Data = iScriptData;
            StringData = iStringData;
            GraphID = id;
            GraphName = name;
            EditorData = editorData;
        }

        public void ShowData(int selectedTypes)
        {
            if (StringData != null && (selectedTypes & 1 << 6) != 0)
            {
                GUILayout.BeginArea(new Rect(10, 10, 600, 120));
                EditorGUITool.LabelField($"GraphID: { GraphID } GraphName: { GraphName }");
                //EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUITool.LabelField("Note:", LevelGraph.TitleLayoutW);
                EditorGUITool.LabelField(EditorData.CustomNote, LevelGraph.TitleLayoutW);
                EditorGUILayout.EndHorizontal();
                //*********
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("String", LevelGraph.TitleLayout);
                StringData.str = EditorGUILayout.TextField(StringData.str, LevelGraph.ContentLayout);
                EditorGUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
            else if (Data != null)
            {
                if (Data.Cmd == LevelScriptCmd.Level_Cmd_Bubble && (selectedTypes & 1 << 1) != 0)
                {
                    GUILayout.BeginArea(new Rect(10, 10, 600, 120));
                    //EditorGUILayout.BeginHorizontal();
                    if (Data.Cmd != LevelScriptCmd.Level_Cmd_Custom)
                        EditorGUITool.LabelField(Data.Cmd.ToString().Substring(10));
                    else
                        EditorGUITool.LabelField(Data.stringParam[0]);
                    EditorGUITool.LabelField($"GraphID: { GraphID } GraphName: { GraphName }");
                    //EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUITool.LabelField("Note:", LevelGraph.TitleLayoutW);
                    EditorGUITool.LabelField(EditorData.CustomNote, LevelGraph.TitleLayoutW);
                    EditorGUILayout.EndHorizontal();
                    //*********
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("怪物ID", LevelGraph.TitleLayout);
                    Data.valueParam[0] = EditorGUILayout.FloatField(Data.valueParam[0], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("怪物ID(长)", LevelGraph.TitleLayout);
                    Data.stringParam[2] = EditorGUILayout.TextField(Data.stringParam[2], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Text", LevelGraph.TitleLayout);
                    Data.stringParam[0] = EditorGUILayout.TextField(Data.stringParam[0], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
                else if (Data.Cmd == LevelScriptCmd.Level_Cmd_Cutscene && (selectedTypes & 1 << 0) != 0)
                {
                    GUILayout.BeginArea(new Rect(10, 10, 600, 120));
                    //EditorGUILayout.BeginHorizontal();
                    if (Data.Cmd != LevelScriptCmd.Level_Cmd_Custom)
                        EditorGUITool.LabelField(Data.Cmd.ToString().Substring(10));
                    else
                        EditorGUITool.LabelField(Data.stringParam[0]);
                    EditorGUITool.LabelField($"GraphID: { GraphID } GraphName: { GraphName }");
                    //EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUITool.LabelField("Note:", LevelGraph.TitleLayoutW);
                    EditorGUITool.LabelField(EditorData.CustomNote, LevelGraph.TitleLayoutW);
                    EditorGUILayout.EndHorizontal();
                    //*********
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("是否是Mp4", LevelGraph.TitleLayout);
                    Data.valueParam[1] = EditorGUILayout.FloatField(Data.valueParam[1], LevelGraph.ContentLayout);

                    EditorGUILayout.LabelField("Cutscene", LevelGraph.TitleLayout);
                    Data.stringParam[0] = EditorGUILayout.TextField(Data.stringParam[0], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("进入过渡时间", LevelGraph.TitleLayout);
                    Data.valueParam[3] = EditorGUILayout.FloatField(Data.valueParam[3], LevelGraph.ContentLayout);

                    EditorGUILayout.LabelField("进入纯黑时间", LevelGraph.TitleLayout);
                    Data.valueParam[4] = EditorGUILayout.FloatField(Data.valueParam[4], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("结束过渡时间", LevelGraph.TitleLayout);
                    Data.valueParam[5] = EditorGUILayout.FloatField(Data.valueParam[5], LevelGraph.ContentLayout);

                    EditorGUILayout.LabelField("结束纯黑时间", LevelGraph.TitleLayout);
                    Data.valueParam[6] = EditorGUILayout.FloatField(Data.valueParam[6], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
                else if (Data.Cmd == LevelScriptCmd.Level_Cmd_ShowBossName && (selectedTypes & 1 << 2) != 0)
                {
                    GUILayout.BeginArea(new Rect(10, 10, 600, 120));
                    //EditorGUILayout.BeginHorizontal();
                    if (Data.Cmd != LevelScriptCmd.Level_Cmd_Custom)
                        EditorGUITool.LabelField(Data.Cmd.ToString().Substring(10));
                    else
                        EditorGUITool.LabelField(Data.stringParam[0]);
                    EditorGUITool.LabelField($"GraphID: { GraphID } GraphName: { GraphName }");
                    //EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUITool.LabelField("Note:", LevelGraph.TitleLayoutW);
                    EditorGUITool.LabelField(EditorData.CustomNote, LevelGraph.TitleLayoutW);
                    EditorGUILayout.EndHorizontal();
                    //*********
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("BossName", LevelGraph.TitleLayout);
                    Data.stringParam[0] = EditorGUILayout.TextField(Data.stringParam[0], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("EnglishName", LevelGraph.TitleLayout);
                    Data.stringParam[1] = EditorGUILayout.TextField(Data.stringParam[1], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Trait", LevelGraph.TitleLayout);
                    Data.stringParam[2] = EditorGUILayout.TextField(Data.stringParam[2], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Answer", LevelGraph.TitleLayout);
                    Data.stringParam[3] = EditorGUILayout.TextField(Data.stringParam[3], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Sign", LevelGraph.TitleLayout);
                    Data.stringParam[4] = EditorGUILayout.TextField(Data.stringParam[4], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
                else if (Data.Cmd == LevelScriptCmd.Level_Cmd_Notice && (selectedTypes & 1 << 3) != 0)
                {
                    GUILayout.BeginArea(new Rect(10, 10, 600, 120));
                    //EditorGUILayout.BeginHorizontal();
                    if (Data.Cmd != LevelScriptCmd.Level_Cmd_Custom)
                        EditorGUITool.LabelField(Data.Cmd.ToString().Substring(10));
                    else
                        EditorGUITool.LabelField(Data.stringParam[0]);
                    EditorGUITool.LabelField($"GraphID: { GraphID } GraphName: { GraphName }");
                    //EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUITool.LabelField("Note:", LevelGraph.TitleLayoutW);
                    EditorGUITool.LabelField(EditorData.CustomNote, LevelGraph.TitleLayoutW);
                    EditorGUILayout.EndHorizontal();
                    //*********
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Text", LevelGraph.TitleLayout);
                    Data.stringParam[0] = EditorGUILayout.TextField(Data.stringParam[0], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
                else if (Data.Cmd == LevelScriptCmd.Level_Cmd_Custom && Data.stringParam[0] == "Mission" && (selectedTypes & 1 << 4) != 0)
                {
                    GUILayout.BeginArea(new Rect(10, 10, 600, 120));
                    //EditorGUILayout.BeginHorizontal();
                    if (Data.Cmd != LevelScriptCmd.Level_Cmd_Custom)
                        EditorGUITool.LabelField(Data.Cmd.ToString().Substring(10));
                    else
                        EditorGUITool.LabelField(Data.stringParam[0]);
                    EditorGUITool.LabelField($"GraphID: { GraphID } GraphName: { GraphName }");
                    //EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUITool.LabelField("Note:", LevelGraph.TitleLayoutW);
                    EditorGUITool.LabelField(EditorData.CustomNote, LevelGraph.TitleLayoutW);
                    EditorGUILayout.EndHorizontal();
                    //*********
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("任务标题", LevelGraph.TitleLayout);
                    Data.stringParam[1] = EditorGUILayout.TextField(Data.stringParam[1], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("任务内容", LevelGraph.TitleLayout);
                    Data.stringParam[2] = EditorGUILayout.TextField(Data.stringParam[2], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
                else if (Data.Cmd == LevelScriptCmd.Level_Cmd_CallUI && (selectedTypes & 1 << 5) != 0)
                {
                    GUILayout.BeginArea(new Rect(10, 10, 600, 120));
                    //EditorGUILayout.BeginHorizontal();
                    if (Data.Cmd != LevelScriptCmd.Level_Cmd_Custom)
                        EditorGUITool.LabelField(Data.Cmd.ToString().Substring(10));
                    else
                        EditorGUITool.LabelField(Data.stringParam[0]);
                    EditorGUITool.LabelField($"GraphID: { GraphID } GraphName: { GraphName }");
                    //EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUITool.LabelField("Note:", LevelGraph.TitleLayoutW);
                    EditorGUITool.LabelField(EditorData.CustomNote, LevelGraph.TitleLayoutW);
                    EditorGUILayout.EndHorizontal();
                    //*********
                    for (int i = 0; i < Data.valueParam.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Value" + i, LevelGraph.TitleLayout);
                            Data.valueParam[i] = EditorGUILayout.FloatField(Data.valueParam[i], LevelGraph.ContentLayout);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("Value" + i, LevelGraph.TitleLayout);
                            Data.valueParam[i] = EditorGUILayout.FloatField(Data.valueParam[i], LevelGraph.ContentLayout);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    if (Data.valueParam.Count % 2 == 1)
                        EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("UI Name", LevelGraph.TitleLayout);
                    Data.stringParam[0] = EditorGUILayout.TextField(Data.stringParam[0], LevelGraph.ContentLayout);
                    EditorGUILayout.LabelField("UI String Param", LevelGraph.TitleLayout);
                    Data.stringParam[1] = EditorGUILayout.TextField(Data.stringParam[1], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
            }
            
        }
    }
}
