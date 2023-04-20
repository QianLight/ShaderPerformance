using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using LevelEditor;
using System.Net.Sockets;
using TDTools;

namespace BluePrint
{
    public class BluePrintSubGraphNode : BluePrintBaseDataNode<BluePrintSubGraphData>
    {
        public BluePrintGraph bindingGraph;

        protected override bool OnMouseRightDown(Event e)
        {
            var genericMenu = new GenericMenu();
            //genericMenu.AddItem(new GUIContent("Rename"), false, OnRenameClicked);
            genericMenu.AddItem(new GUIContent("Enable"), Enabled, OnEnableClicked);
            genericMenu.AddItem(new GUIContent("Delete"), false, OnDeleteClicked);
            genericMenu.AddItem(new GUIContent("Save"), false, OnSaveClicked);
            genericMenu.ShowAsContext();

            return true;
        }

        protected void OnSaveClicked()
        {
            string file = EditorUtility.SaveFilePanel("Select Dir", Application.dataPath + "/BundleRes/Table/Level/Function", bindingGraph.GraphName, "Func");
            if (file.EndsWith("Func"))
            {
                LevelGraph lvGraph = bindingGraph as LevelGraph;
                lvGraph.SaveGraphToData();

                string efile = file.Replace(".Func", ".eFunc");
                DataIO.SerializeData<GraphConfigData>(efile, lvGraph.graphConfigData);
                DataIO.SerializeData<LevelGraphData>(file, lvGraph.graphData);
            }
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            HeaderImage = "BluePrint/Header8";
            nodeEditorData.Tag = "SubGraph";
            nodeEditorData.BackgroundText = "";

            CanbeFold = true;

            BluePrintPin pinIn1 = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn1);
            BluePrintPin pinIn2 = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn2);
            BluePrintPin pinIn3 = new BluePrintValuedPin(this, 3, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn3);
            BluePrintPin pinIn4 = new BluePrintValuedPin(this, 4, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn4);

            BluePrintPin pinOut1 = new BluePrintValuedPin(this, 5, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinOut1);
            BluePrintPin pinOut2 = new BluePrintValuedPin(this, 6, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinOut2);
            BluePrintPin pinOut3 = new BluePrintValuedPin(this, 7, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinOut3);
            BluePrintPin pinOut4 = new BluePrintValuedPin(this, 8, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinOut4);
        }

        public override List<BluePrintSubGraphData> GetCommonDataList(BluePrintData data)
        {
            return data.SubGraphData;
        }

        public override void OnCopy(BluePrintSubGraphData sData)
        {
            base.OnCopy(sData);
            BluePrintGraph sourceGraph = Root.editorWindow.GetGraph(sData.GraphID);

            if (sourceGraph != null)
            {
                bindingGraph = Root.editorWindow.CloneGraph(sourceGraph);
                if (bindingGraph != null)
                {
                    HostData.GraphName = bindingGraph.GraphName;
                    HostData.GraphID = bindingGraph.GraphID;
                }
            }
        }

        public override void PostInit()
        {
            if (HostData.GraphID == 0) HostData.GraphID = Root.editorWindow.NewSubGraph();
            bindingGraph = Root.editorWindow.GetGraph(HostData.GraphID);
            if (bindingGraph != null) HostData.GraphName = bindingGraph.GraphName;
        }

        public override void SetInternalParam(int interParam, string customType)
        {
            HostData.GraphID = interParam;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("GraphID:", new GUILayoutOption[] { GUILayout.Width(150f) });
            EditorGUILayout.LabelField(HostData.GraphID.ToString(), new GUILayoutOption[] { GUILayout.Width(120f) });
            GUILayout.EndHorizontal();

            
            if (bindingGraph != null)
            {
                //string tempName = bindingGraph.GraphName;
                bindingGraph.GraphName = EditorGUILayout.TextField("Name: ", bindingGraph.GraphName, new GUILayoutOption[] { GUILayout.Width(270f) });
            }
                
            HostData.GraphName = bindingGraph.GraphName;
        }

        public override void Draw()
        {
            base.Draw();

            var textRect = new Rect(Bounds.x, Bounds.y + titleHeight, Bounds.width, Bounds.height - titleHeight).Scale(Scale);
            DrawTool.DrawLabel(textRect, HostData.GraphName == null ? "" : HostData.GraphName.ToString(), BlueprintStyles.PinTextStyle, TextAnchor.UpperCenter);
        }

        public override void OnAdded()
        {
            base.OnAdded();
            if (LevelEditor.LevelEditor.Instance != null && LevelEditor.LevelEditor.Instance.CurrentGraph != null && LevelEditor.LevelEditor.Instance.IsFocused)
                LevelOperationStack.Instance.PushOperation(LevelOperationType.AddNode, this, LevelEditor.LevelEditor.Instance.CurrentGraph.GraphID);
        }

        //在删除时先于基类函数存储操作 不然pin的连接信息会丢失
        public override void OnDeleted()
        {
            if (LevelEditor.LevelEditor.Instance!=null&&LevelEditor.LevelEditor.Instance.CurrentGraph != null && LevelEditor.LevelEditor.Instance.IsFocused)
                LevelOperationStack.Instance.PushOperation(LevelOperationType.DeleteNode, this, LevelEditor.LevelEditor.Instance.CurrentGraph.GraphID);
            base.OnDeleted();
        }

        public override void OnConnectionBreak(BluePrintPin start, BluePrintPin end)
        {
            base.OnConnectionBreak(start, end);
            if (LevelEditor.LevelEditor.Instance != null && LevelEditor.LevelEditor.Instance.CurrentGraph != null&&LevelEditor.LevelEditor.Instance.IsFocused)
                LevelOperationStack.Instance.PushOperation(LevelOperationType.PinBreak,
                new List<BluePrintPin>() { start, end }
                , LevelEditor.LevelEditor.Instance.CurrentGraph.GraphID);
        }

        public override void OnConnectionSucc(BluePrintPin start, BluePrintPin end, BlueprintConnection connection)
        {
            base.OnConnectionSucc(start, end, connection);
            if (LevelEditor.LevelEditor.Instance != null && LevelEditor.LevelEditor.Instance.CurrentGraph != null && LevelEditor.LevelEditor.Instance.IsFocused)
                LevelOperationStack.Instance.PushOperation(LevelOperationType.PinConnect,
               new List<BluePrintPin>() { start, end }
               , LevelEditor.LevelEditor.Instance.CurrentGraph.GraphID);
        }
    }
}
