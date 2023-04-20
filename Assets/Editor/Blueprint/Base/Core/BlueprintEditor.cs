using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public class BlueprintEditor : EditorWindow
    {
        protected static int MAX_GRAPH_ID = 1;

        public static BlueprintEditor Editor;

        public GUILayoutOption[] InspectorLine;

        public List<BluePrintGraph> Graphs = new List<BluePrintGraph>();
        public List<BluePrintGraph> OpenGrahps = new List<BluePrintGraph>();
        public BluePrintGraph CurrentGraph = null;

        public Rect InspectorWindow;

        public int ToolBarHeight;
        public EditorConfigData editorConfigData = new EditorConfigData();

        public bool OpenSnap = false;
        public int SnapSize = 10;

        protected float InspectorWidth = 300f;
        protected bool HideInspector = false;
        public static int LineMode = 1;
        private string[] lineModeStr = { "直线", "曲线" };
        private int[] lineValue = { 1, 2 };
        public virtual void OnEnable()
        {
            Editor = GetWindow<BlueprintEditor>();
        }

        public virtual void OnDisable()
        {
            Reset();
        }

        public virtual void Reset()
        {
            MAX_GRAPH_ID = 1;

            if (CurrentGraph != null)
            {
                CurrentGraph.UnInit();
                CurrentGraph = null;
            }

            for (int i = 0; i < Graphs.Count; ++i)
            {
                Graphs[i].UnInit();
            }
            Graphs.Clear();
            OpenGrahps.Clear();

            editorConfigData = null;
        }

        public bool IsFocused { get; set; }

        public void OnFocus()
        {
            IsFocused = true;
        }
        public void OnLostFocus()
        {
            IsFocused = false;
        }

        #region graph
        public virtual int NewSubGraph() { return 0; }
        public T NewGraph<T>(int graphID = 0, string graphName = "main", bool bAutoSet = true) where T : BluePrintGraph, new()
        {
            T graph = new T();
            graph.Init(this);

            graph.GraphID = graphID;
            graph.GraphName = graphName;       
            AddGraph(graph, bAutoSet);

            graph.graphConfigData.graphID = graph.GraphID;
            graph.VarManager.HostGraphID = graph.GraphID;
            return graph;
        }

        public virtual BluePrintGraph CloneGraph(BluePrintGraph sourceGraph)
        {
            return null;
        }

        public int AddGraph(BluePrintGraph graph, bool AutoSet = true)
        {
            if  (graph.GraphID >= 0)
            {
                if (graph.GraphID == 0)
                    graph.GraphID = MAX_GRAPH_ID++;
                else if (graph.GraphID >= MAX_GRAPH_ID)
                    MAX_GRAPH_ID = graph.GraphID + 1;
            }
            Graphs.Add(graph);
            if (AutoSet) SetEditGraph(graph);

            return graph.GraphID;
        }

        public int GenGraphID(BluePrintGraph graph)
        {
            graph.GraphID = MAX_GRAPH_ID++;
            graph.graphConfigData.graphID = graph.GraphID;
            graph.VarManager.HostGraphID = graph.GraphID;
            return graph.GraphID;
        }

        public int GetCurrentGraphID()
        {
            return CurrentGraph != null ? CurrentGraph.GraphID : 0;
        }

        public static BluePrintGraph GetMainGraph()
        {
            return Editor.GetGraph(1);
        }

        public BluePrintGraph GetGraph(int graphID)
        {
            for (int i = 0; i < Graphs.Count; ++i)
            {
                if (Graphs[i].GraphID == graphID) return Graphs[i];
            }
            return null;
        }

        public void RemoveUselessGraph(Dictionary<int, int> SubGraphReferenceMap)
        {
            foreach (KeyValuePair<int, int> pair in SubGraphReferenceMap)
            {
                if (pair.Value == 0)
                {
                    Graphs.RemoveAll(g => g.GraphID == pair.Key);
                    OpenGrahps.RemoveAll(g => g.GraphID == pair.Key);
                }
            }
        }

        public virtual void SetEditGraph(BluePrintGraph graph)
        {
            if (CurrentGraph == graph) return;

            CurrentGraph = graph;            
        }

        public void OpenGraph(BluePrintGraph graph)
        {
            if(!OpenGrahps.Contains(graph))
            {
                OpenGrahps.Add(graph);
            }

            SetEditGraph(graph);
        }

        public void OpenGraph(int graphID)
        {
            BluePrintGraph g = GetGraph(graphID);
            if (g != null) OpenGraph(g);
        }

        public void CloseOpenGraph(BluePrintGraph graph)
        {
            if(OpenGrahps.Contains(graph))
            {
                OpenGrahps.Remove(graph);
            }

            SetEditGraph(OpenGrahps[0]);
        }

        #endregion
        public void OnGUI()
        {
            ToolBarHeight = DrawToolBar();
            //Draw();

            if (CurrentGraph != null) CurrentGraph.Draw(ToolBarHeight);

            if (!Application.isPlaying || EditorWindow.focusedWindow == null ||
                (!EditorWindow.focusedWindow.ToString().Contains("UnityEditor.SceneView") &&
                !EditorWindow.focusedWindow.ToString().Contains("UnityEditor.GameView")))
            {
                BeginWindows();
                DrawInspectorWindow();
                EndWindows();
            }
        }

        public void DrawInspectorWindow()
        {
            Rect rect = GUILayout.Window(1, new Rect(position.width - InspectorWidth - 40, ToolBarHeight, 0, 0), ShowInspector, "Inspector",GUILayout.MaxWidth(InspectorWidth));
            if (rect.size != Vector2.zero) InspectorWindow = rect;
        }

        #region Search
        private string curKeyWord = string.Empty;
        private int curSearchIndex = 0;
        private bool DoSearch()
        {
            for (int i = curSearchIndex + 1; i < CurrentGraph.widgetList.Count; ++i)
            {
                BluePrintNode node = CurrentGraph.widgetList[i] as BluePrintNode;
                if (node.nodeEditorData.Tag.ToLower().Contains(curKeyWord.ToLower()))
                {
                    curSearchIndex = i;
                    return true;
                }
            }

            for (int i = 0; i <= curSearchIndex; ++i)
            {
                BluePrintNode node = CurrentGraph.widgetList[i] as BluePrintNode;
                if (node.nodeEditorData.Tag.ToLower().Contains(curKeyWord.ToLower()))
                {
                    curSearchIndex = i;
                    return true;
                }
            }

            return false;
        }
        #endregion

        public virtual void ShowInspector(int unusedWindowID)
        {
            if (CurrentGraph != null)
            {
                if (this is SkillEditor || this is BehitEditor)
                {
                    EditorGUILayout.BeginHorizontal();
                    InspectorWidth = Mathf.Min(700f, Mathf.Max(330f, EditorGUILayout.FloatField("InspectorWidth", InspectorWidth)));
                    if (GUILayout.Button("Set"))
                    {
                        CacheEditorGUIData();
                    }
                    EditorGUILayout.EndHorizontal();

                    Rect popupRect = EditorGUILayout.GetControlRect(true);
                    Rect rectLabel = new Rect(popupRect.x, popupRect.y, EditorGUIUtility.labelWidth, popupRect.height);
                    popupRect.width -= rectLabel.width;
                    popupRect.x += rectLabel.width;
                    GUI.Label(rectLabel, "选择曲线模式");
                    LineMode = EditorGUI.IntPopup(popupRect, LineMode, lineModeStr, lineValue);
                    EditorGUILayout.BeginHorizontal();
                    HideInspector = EditorGUILayout.Toggle(HideInspector,GUILayout.Width(15));
                    curKeyWord = EditorGUITool.TextField("SearchNode(" + CurrentGraph.widgetList.Count + ")", curKeyWord);

                    if (GUILayout.Button("Search"))
                    {
                        if (DoSearch())
                        {
                            if (CurrentGraph.selectNode != null)
                                CurrentGraph.selectNode.IsSelected = false;
                            CurrentGraph.ClearMultiselect();
                            CurrentGraph.widgetList[curSearchIndex].IsSelected = true;
                            CurrentGraph.selectNode = CurrentGraph.widgetList[curSearchIndex] as BluePrintNode;
                            CurrentGraph.FocusNode(CurrentGraph.selectNode);
                        }
                        else
                        {
                            ShowNotification(new GUIContent("未检测到相关节点"));
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (!HideInspector)
                    CurrentGraph.DrawDataInspector();
                GUILayout.Box("", new GUILayoutOption[] { GUILayout.Width(InspectorWidth), GUILayout.Height(2) });
                EditorGUILayout.Space();
                if (CurrentGraph.selectNode != null)
                {
                    CurrentGraph.selectNode.DrawDataInspector();
                }
                else if(CurrentGraph.selectNodeList.Count!=0)
                {
                    CurrentGraph.DrawMultiDataInspector();
                }
            }
        }

        public virtual void ShowComment(int unusedWindowID)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("hello");
            EditorGUILayout.EndHorizontal();
        }


        public void OnInspectorUpdate()
        {
            if (IsFocused) Repaint();
        }

        public virtual void Update()
        {
            if (IsFocused) Repaint();
        }

        public static bool CheckOver(Vector2 pos, Rect rect)
        {
            if (rect.xMin > pos.x) return false;
            if (rect.xMax < pos.x) return false;
            if (rect.yMin > pos.y) return false;
            if (rect.yMax < pos.y) return false;

            return true;
        }

        public virtual int DrawToolBar()
        {
            GUILayout.BeginHorizontal(BlueprintStyles.Toolbar());
            {
                if (GUILayout.Button("New", BlueprintStyles.ToolbarButton()))
                {
                    ToolBarNewClicked();
                }
                if (GUILayout.Button("Open", BlueprintStyles.ToolbarButton()))
                {
                    ToolBarOpenClicked();
                }
                ToolBarLeftExtra();
                if (GUILayout.Button("SaveAs", BlueprintStyles.ToolbarButton()))
                {
                    ToolBarSaveClicked();
                }
                if (CurrentGraph != null)
                {
                    float oldScale = CurrentGraph.Scale;
                    CurrentGraph.Scale = EditorGUILayout.Slider(CurrentGraph.Scale, 0.5f, 3f);
                    CurrentGraph.scrollPosition.x = (CurrentGraph.scrollPosition.x + position.width / 2) / oldScale * CurrentGraph.Scale - position.width / 2;
                    CurrentGraph.scrollPosition.y = (CurrentGraph.scrollPosition.y + position.height / 2) / oldScale * CurrentGraph.Scale - position.height / 2;
                }

                GUILayout.FlexibleSpace();
                ToolBarExtra();
            }
            GUILayout.EndHorizontal();
            return BlueprintStyles.ToolbarButton().fontSize * 2;
        }

        public virtual void ToolBarNewClicked() { }
        public virtual void ToolBarOpenClicked() { }
        public virtual void ToolBarSaveClicked() { }
        public virtual void ToolBarExtra() { }
        public virtual void ToolBarLeftExtra() { }

        public void CacheEditorGUIData()
        {
            EditorGUIData data = new EditorGUIData();
            data.InspectorWidth = InspectorWidth;
            data.HideInspector = HideInspector;
            data.LineMode = LineMode;
            data.CacheData();
        }

        public void LoadEditorGUIData()
        {
            EditorGUIData data = EditorGUIData.LoadData();
            if (data == null) return;

            InspectorWidth = data.InspectorWidth;
            HideInspector = data.HideInspector;
            LineMode = data.LineMode;
        }
    }
}

[Serializable]
public class EditorGUIData
{
    [SerializeField, DefaultValueAttribute(300.0f)]
    public float InspectorWidth = 300.0f;
    [SerializeField, DefaultValueAttribute(false)]
    public bool HideInspector = false;
    [SerializeField, DefaultValueAttribute(1)]
    public int LineMode = 1;//连线模式 1 直线 2 曲线
    public void CacheData()
    {
        string path = Application.dataPath + "/Editor Default Resources/SkillBackup/EditorGUIData.cache";

        DataIO.SerializeData<EditorGUIData>(path, this);
    }

    public static EditorGUIData LoadData()
    {
        string path = Application.dataPath + "/Editor Default Resources/SkillBackup/EditorGUIData.cache";

        return File.Exists(path) ? DataIO.DeserializeData<EditorGUIData>(path) : null;
    }
}