using LevelEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using TDTools;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public struct NodeDataClassName
    {
        public string NodeName;
        public string DataName;
        public int InternalParam;

        public NodeDataClassName(string n1, string d1, int sub = 0)
        {
            NodeName = n1;
            DataName = d1;
            InternalParam = sub;
        }
    }

    public class BluePrintGraph
    {
        protected int MAX_NODE_ID = 1;
        public GraphConfigData graphConfigData;

        public int GraphID = -1;
        public string GraphName = "";

        public BlueprintEditor editorWindow;
        public float Scale = 1f;
        public List<BluePrintWidget> widgetList = new List<BluePrintWidget>();
        public List<BlueprintAreaComment> commentList = new List<BlueprintAreaComment>();
        public BluePrintNode selectNode;
        public List<BluePrintWidget> selectNodeList = new List<BluePrintWidget>();

        public BluePrintPin ConnectStartPin;
        private Vector3 cachedMousePosition = Vector3.zero;

        public Vector2 scrollPosition;
        protected float maxWidth;
        protected float maxHeight;

        public bool InMultiselect = false;
        protected Vector2 multiselectStartPosition;
        protected Vector2 multiselectEndPosition;
        
        public BlueprintAreaComment SettingComment;

        protected static Dictionary<string, NodeDataClassName> RegistedNodeType = new Dictionary<string, NodeDataClassName>();
        protected static Dictionary<string, int> RegistedSubGraph = new Dictionary<string, int>();

        public BlueprintGraphVariantManager VarManager = new BlueprintGraphVariantManager();
        public INodeBehaviourDefine NodeBehaviour = NodeBehaviourFactory.defaultBehaviour;

        public Vector2 ScrollPosition { get { return scrollPosition; } }

        private bool ignoreDrag = false;

        public virtual void Init(BlueprintEditor editor)
        {
            editorWindow = editor;
            Reset();

            RegisterRightClickMenu("Common/Start", new NodeDataClassName("BluePrint.ControllerStartNode", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("Common/Fly", new NodeDataClassName("BluePrint.ControllerFlyNode", "BluePrint.BluePrintControllerData"));
            
            RegisterRightClickMenu("SubGraph/New", new NodeDataClassName("BluePrint.BluePrintSubGraphNode", "BluePrint.BluePrintSubGraphData"));
            RegisterRightClickMenu("SubGraph/Start", new NodeDataClassName("BluePrint.ControllerSubGraphStartNode", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("SubGraph/End", new NodeDataClassName("BluePrint.ControllerSubGraphEndNode", "BluePrint.BluePrintControllerData"));

            RegisterRightClickMenu("FlowControl/Branch", new NodeDataClassName("BluePrint.ControllerBranchNode", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("FlowControl/Op&&", new NodeDataClassName("BluePrint.ControllerOpAnd", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("FlowControl/Op||", new NodeDataClassName("BluePrint.ControllerOpOr", "BluePrint.BluePrintControllerData"));

            RegisterRightClickMenu("FlowControl/Op==", new NodeDataClassName("BluePrint.ControllerOpEqual", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("FlowControl/Op>", new NodeDataClassName("BluePrint.ControllerOpGreat", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("FlowControl/Op<", new NodeDataClassName("BluePrint.ControllerOpLess", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("FlowControl/Op>=", new NodeDataClassName("BluePrint.ControllerOpGreatEqual", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("FlowControl/Op<=", new NodeDataClassName("BluePrint.ControllerOpLessEqual", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("FlowControl/While", new NodeDataClassName("BluePrint.ControllerWhileNode", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("FlowControl/LoopEnd", new NodeDataClassName("BluePrint.ControllerLoopEndNode", "BluePrint.BluePrintControllerData"));

            RegisterRightClickMenu("FlowControl/And", new NodeDataClassName("BluePrint.FlowAndNode", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("FlowControl/Or", new NodeDataClassName("BluePrint.FlowOrNode", "BluePrint.BluePrintControllerData"));

            RegisterRightClickMenu("Common/Compute", new NodeDataClassName("BluePrint.ControllerOpArithmeticNode", "BluePrint.BluePrintControllerData"));
            RegisterRightClickMenu("Common/Timer", new NodeDataClassName("BluePrint.CommonTimerNode", "BluePrint.BluePrintTimerData"));
            RegisterRightClickMenu("Common/PlayerPosition", new NodeDataClassName("BluePrint.ExtNodeGetPlayerPosition", "BluePrint.ExtGetPlayerPositionData"));
            RegisterRightClickMenu("Common/PlayerHP", new NodeDataClassName("BluePrint.ExtNodeGetPlayerHP", "BluePrint.BluePrintNodeBaseData"));
            RegisterRightClickMenu("Common/InRange", new NodeDataClassName("BluePrint.ExtNodeInRange", "BluePrint.ExtInRangeData"));
            RegisterRightClickMenu("Common/GetPartnerAttr", new NodeDataClassName("BluePrint.ExtNodeGetPartnerAttr", "BluePrint.ExtGetPartnerAttrData"));
            RegisterRightClickMenu("Common/SetPartnerAttr", new NodeDataClassName("BluePrint.ExtNodeSetPartnerAttr", "BluePrint.ExtSetPartnerAttrData"));
            RegisterRightClickMenu("Common/GetPlayerID", new NodeDataClassName("BluePrint.ExtNodeGetPlayerID", "BluePrint.ExtGetPlayerIDData"));
            RegisterRightClickMenu("Common/GetLevelProgress", new NodeDataClassName("BluePrint.ExtNodeGetLevelProgress", "BluePrint.ExtLevelProgressData"));
            RegisterRightClickMenu("Common/GetScore", new NodeDataClassName("BluePrint.ExtNodeGetScore", "BluePrint.ExtGetScoreData"));
            RegisterRightClickMenu("Common/Random", new NodeDataClassName("BluePrint.ExtNodeGetRandomNum", "BluePrint.ExtGetRandomNumData"));
            RegisterRightClickMenu("Common/EventComplete", new NodeDataClassName("BluePrint.ExtNodeEventComplete", "BluePrint.ExtEventCompleteData"));

            RegisterRightClickMenu("Hidden", new NodeDataClassName("BluePrint.CommonVariableGetNode", "BluePrint.BluePrintVariantData"));
            RegisterRightClickMenu("Hidden2", new NodeDataClassName("BluePrint.CommonVariableSetNode", "BluePrint.BluePrintVariantData"));
        }

        public void Reset()
        {
            Scale = 1f;
            selectNode = null;
            maxHeight = 0;
            maxWidth = 0;
            MAX_NODE_ID = 1;
            graphConfigData = new GraphConfigData();
            simulatorEngine = null;
            selectNodeList.Clear();

            VarManager.UnInit();

            foreach (BluePrintNode node in widgetList)
            {
                node.UnInit();
            }
            widgetList.Clear();
        }

        public void ResetCachedPinData()
        {
            ConnectStartPin = null;
            cachedMousePosition = Vector3.zero;
        }

        public virtual void UnInit()
        {
            Reset();
        }

        public virtual void Update()
        {
            if (simulatorEngine != null) simulatorEngine.Update();
        }

        //public virtual void Clone(ref BluePrintGraph dest)
        //{

        //}

        public virtual void Draw(int startY)
        {
            bool mouseInInspectorWindow = true;
            CheckScrollRect();
            scrollPosition = GUI.BeginScrollView(new Rect(0, startY, editorWindow.position.width, editorWindow.position.height - startY), scrollPosition, new Rect(0, 0, maxWidth, maxHeight), false, false);

            BlueprintStyles.Scale = Scale;
            DrawExtra();

            if (ignoreDrag && Event.current.type == EventType.MouseDrag)
            {
                ignoreDrag = false;
            }
            else
            {
                if (!BlueprintEditor.CheckOver(Event.current.mousePosition - scrollPosition, editorWindow.InspectorWindow))
                {
                    mouseInInspectorWindow = false;
                    //事件不传递
                    bool flag = false;
                    for (int i = 0; i < widgetList.Count; ++i)
                    {
                        flag |= widgetList[i].OnMouseEvent(Event.current, flag);
                    }

                    if(!flag)
                    {
                        for (int i = 0; i < commentList.Count; ++i)
                        {
                            flag |= commentList[i].OnMouseEvent(Event.current, flag);
                        }
                    }
                    if (!flag) MouseEvent();
                    else InMultiselect = false;
                }
                else ignoreDrag = true;

                ignoreDrag |= Event.current.type == EventType.MouseDown && Event.current.button == 1;
            }

            for (int i = 0; i < commentList.Count; ++i)
            {
                commentList[i].Draw();
            }

            for (int i = 0; i < widgetList.Count; ++i)
            {
                if (!widgetList[i].IsSelected) widgetList[i].Draw();
            }
            for (int i = 0; i < widgetList.Count; ++i)
            {
                if (widgetList[i].IsSelected) widgetList[i].Draw();
            }

            DrawMouseLine();

            if (InMultiselect) DrawMultiSelectBox();

            for (int i = 0; i < commentList.Count; ++i)
            {
                commentList[i].PostDraw();
            }
            GUI.EndScrollView(!mouseInInspectorWindow);
        }

        public virtual void DrawExtra() {}
 
        public virtual void DrawDataInspector() {  }

        public virtual void DrawMultiDataInspector() { }

        public void SetCommentEditorWindow(BlueprintAreaComment window)
        {
            if (window != SettingComment)
            {
                if (SettingComment != null) SettingComment.CloseSettingWindow(null);
                SettingComment = window;
            }
        }

        public void SetGlobalExpand(bool bExpand)
        {
            foreach (BluePrintNode node in widgetList)
            {
                if(node.CanbeFold)
                    node.nodeEditorData.Expand = bExpand;
            }
        }

        public bool IsMainGraph { get { return GraphID == 1; } }

        #region save&load
        protected void SaveCommonNode(BluePrintData bpData)
        {
            for (int i = 0; i < VarManager.UserVariant.Count; ++i)
            {
                bpData.VariantDefine.Add(VarManager.UserVariant[i]);
            }

            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedNodeType)
            {
                CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "BaseBuildDataByNodeT", new object[] { bpData});
            }

            foreach (BluePrintNode tmp in widgetList)
            {
                foreach (BluePrintPin pin in tmp.pinList)
                {
                    if (pin is BluePrintValuedPin)
                    {
                        BPVariant v = default(BPVariant);

                        if((pin as BluePrintValuedPin).GetDefaultValue(ref v))
                        {
                            BluePrintPinData pinData = new BluePrintPinData();
                            pinData.nodeID = tmp.nodeEditorData.NodeID;
                            pinData.pinID = pin.pinID;
                            pinData.defaultValue = v.val._float;
                            bpData.pinData.Add(pinData);
                        }
                    }
                       
                }
            }
        }

        public void BaseBuildDataByNodeT<N, T>(BluePrintData bpData)
            where T : BluePrintNodeBaseData, new()
            where N : BluePrintBaseDataNode<T>
        {
            foreach (BluePrintNode tmp in widgetList)
            {
                if (tmp is N)
                {
                    N node = tmp as N;
                    node.BeforeSave();
                    node.GetCommonDataList(bpData).Add(node.HostData);
                }
            }
        }

        protected void SaveCommonTpl(List<BluePrintWidget> nodelist, BluePrintData bpData, GraphConfigData configData)
        {
            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedNodeType)
            {
                CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "BaseBuildTplDataByNodeT", new object[] { nodelist, bpData, configData });
            }
        }

        public void BaseBuildTplDataByNodeT<N, T>(List<BluePrintWidget> nodelist, BluePrintData bpData, GraphConfigData configData)
            where T : BluePrintNodeBaseData, new()
            where N : BluePrintBaseDataNode<T>
        {
            foreach (BluePrintNode tmp in nodelist)
            {
                if (tmp is N)
                {
                    N node = tmp as N;
                    node.BeforeSaveTpl(configData);
                    node.GetCommonDataList(bpData).Add(node.HostData);
                }
            }
        }
 
        protected void LoadCommonNode(BluePrintData bpData)
        {
            for(int i= 0; i < bpData.VariantDefine.Count; ++i)
            {
                VarManager.AddVariant(bpData.VariantDefine[i].VarType, bpData.VariantDefine[i].VariableName,bpData.VariantDefine[i].InitValue);
            }

            // todo: optimize
            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedNodeType)
            {
                if(pair.Value.DataName != "BluePrint.BluePrintControllerData")
                    CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "BaseBuildNodeByDataT", new object[] { bpData });
            }

            LoadControllerData(bpData, graphConfigData, null, Vector2.zero);

            for (int i = 0; i < bpData.pinData.Count; ++i)
            {
                BluePrintNode node = GetNode(bpData.pinData[i].nodeID);

                if (node != null)
                {
                    BluePrintPin pin = node.GetPin(bpData.pinData[i].pinID);

                    if(pin is BluePrintValuedPin)
                    {
                        (pin as BluePrintValuedPin).SetDefaultValue(bpData.pinData[i].defaultValue);
                    }
                    
                }
            }
        }

        public void BaseBuildNodeByDataT<N, T>(BluePrintData bpData)
            where T : BluePrintNodeBaseData, new()
            where N : BluePrintBaseDataNode<T>, new()
        {
            N template = new N();
            List<T> data = template.GetCommonDataList(bpData);

            for (int i = 0; i < data.Count; ++i)
            {
                N node = new N();
                NodeConfigData eData = graphConfigData.GetConfigDataByID(data[i].NodeID);

                if (eData != null)
                {
                    node.InitData(data[i], eData);
                    node.Init(this, eData.Position);
                    node.PostInit();
                    AddNode(node);
                    node.HostData.NodeID = node.nodeEditorData.NodeID;
                    node.AfterLoad();

                }
            }
        }

        protected void LoadCommonTpl(BluePrintData bpData, GraphConfigData configData, Dictionary<int, int> IdMap, Vector2 offset)
        {
            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedNodeType)
            {
                if (pair.Value.DataName != "BluePrint.BluePrintControllerData")
                    CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "BaseBuildNodeByTplDataT", new object[] { bpData, configData, IdMap, offset });
            }

            LoadControllerData(bpData, configData, IdMap, offset);
        }

        public void BaseBuildNodeByTplDataT<N, T>(BluePrintData data, GraphConfigData tplConfigData, Dictionary<int, int> IdMap, Vector2 offset)
            where T : BluePrintNodeBaseData, new()
            where N : BluePrintBaseDataNode<T>, new()
        {
            N template = new N();
            List<T> dataList = template.GetCommonDataList(data);

            for (int i = 0; i < dataList.Count; ++i)
            {
                N node = new N();
                NodeConfigData eData = tplConfigData.GetConfigDataByID(dataList[i].NodeID);
                if (eData != null)
                {
                    NodeConfigData ncd = eData.Copy();
                    ncd.Position += offset;
                    node.InitData(dataList[i], ncd);
                    node.Init(this, ncd.Position);
                    int tplID = node.nodeEditorData.NodeID;
                    node.OnCopy(dataList[i]);
                    node.nodeEditorData.NodeID = 0;
                    AddNode(node);
                    node.HostData.NodeID = node.nodeEditorData.NodeID;
                    node.AfterLoad();
                    IdMap.Add(tplID, node.nodeEditorData.NodeID);
                    AddToMultiSelect(node);
                }
            }
        }

        private void LoadControllerData(BluePrintData bpData, GraphConfigData configData, Dictionary<int, int> IdMap, Vector2 offset)
        {
            for (int i = 0; i < bpData.ControllerData.Count; ++i)
            {
                ControllerBaseNode node = ControllerBaseNode.CreateControllerNode(bpData.ControllerData[i].TypeID);
                NodeConfigData eData = configData.GetConfigDataByID(bpData.ControllerData[i].NodeID);

                if (node != null && eData != null)
                {
                    NodeConfigData ncd = eData.Copy();
                    node.InitData(bpData.ControllerData[i], ncd);
                    ncd.Position += offset;
                    node.Init(this, ncd.Position);
                    int tplID = node.nodeEditorData.NodeID;
                    if (IdMap != null) node.nodeEditorData.NodeID = 0;
                    node.PostInit();
                    AddNode(node);
                    node.HostData.NodeID = node.nodeEditorData.NodeID;
                    if (IdMap != null)  IdMap.Add(tplID, node.nodeEditorData.NodeID);
                    node.AfterLoad();
                    if(IdMap != null) AddToMultiSelect(node);
                }
            }
        }

        protected void SaveCommentInfo()
        {
            for(int i = 0; i < commentList.Count; ++i)
            {
                AreaCommentData data = new AreaCommentData();
                data.Text = commentList[i].commentText;
                data.Offset = commentList[i].offset;
                for(int j = 0; j < commentList[i].commentWidgets.Count; ++j)
                {
                    BluePrintNode node = commentList[i].commentWidgets[j] as BluePrintNode;
                    data.NodeList.Add(node.nodeEditorData.NodeID);
                }

                graphConfigData.AreaCommentData.Add(data);
            }
        }

        protected void LoadCommentInfo()
        {
            for(int i = 0; i < graphConfigData.AreaCommentData.Count; ++i)
            {
                BlueprintAreaComment comment = new BlueprintAreaComment();
                comment.Init(this);

                comment.commentText = graphConfigData.AreaCommentData[i].Text;
                comment.offset = graphConfigData.AreaCommentData[i].Offset;
                for(int j = 0; j < graphConfigData.AreaCommentData[i].NodeList.Count; ++j)
                {
                    int nodeID = graphConfigData.AreaCommentData[i].NodeList[j];
                    BluePrintNode node = GetNode(nodeID);
                    if(node != null)
                    {
                        comment.commentWidgets.Add(node);
                    }
                }

                //comment.Resize();
                commentList.Add(comment);
            }
        }

        public void FocusNode(BluePrintNode node)
        {
            scrollPosition = new Vector2(Mathf.Max(0, node.Bounds.center.x-editorWindow.position.size.x/2),
                Mathf.Max(0, node.Bounds.center.y-editorWindow.position.size.y/2));
        }

        #endregion

        #region events

        protected virtual void OnKeyBoardEvent(Event e)
        {

        }

        protected virtual void OnMouseLeftClicked(Event e)
        {
            UnSelectCurrentNode();

            ClearMultiselect();

            UnityEngine.GUI.FocusControl("");
        }

        public virtual void UnSelectCurrentNode()
        {
            if (selectNode != null)
            {
                selectNode.OnUnselected();
                selectNode.IsSelected = false;
                selectNode = null;
            }
        }

        protected virtual void OnMouseRightClicked(Event e) { }

        public virtual void AddNode(BluePrintNode node)
        {
            if (node.nodeEditorData.NodeID == 0)
                node.nodeEditorData.NodeID = MAX_NODE_ID++;
            else if (node.nodeEditorData.NodeID >= MAX_NODE_ID)
                MAX_NODE_ID = node.nodeEditorData.NodeID + 1;

            widgetList.Add(node);
            node.OnAdded();
        }

        public BluePrintNode GetNode(int nodeID)
        {
            for (int i = 0; i < widgetList.Count; ++i)
            {
                BluePrintNode node = widgetList[i] as BluePrintNode;

                if (node != null)
                {
                    if (node.nodeEditorData.NodeID == nodeID)
                    {
                        return node;
                    }
                }
            }

            return null;
        }

        protected virtual void CheckScrollRect()
        {
            maxWidth = 10;
            maxHeight = 10;
            for (int i = 0; i < widgetList.Count; ++i)
            {
                maxWidth = Math.Max(maxWidth, widgetList[i].xMax + editorWindow.position.width / 2);
                maxHeight = Math.Max(maxHeight, widgetList[i].yMax + editorWindow.position.height / 2);
            }
        }

        #region delete

        
        public bool DeleteNodeConsiderSubGraph(BluePrintNode node)
        {
            Dictionary<int, int> SubGraphRefDic = new Dictionary<int, int>();

            MarkDeleteNode(node, true);
            BluePrintGraph mainGraph = editorWindow.GetGraph(1);
            mainGraph.GetSubGraphReference(ref SubGraphRefDic);
            MarkDeleteNode(node, false);

            for(int i = 0; i < editorWindow.Graphs.Count; ++i)
            {
                if (editorWindow.Graphs[i].GraphID != 1 && !SubGraphRefDic.ContainsKey(editorWindow.Graphs[i].GraphID))
                    SubGraphRefDic.Add(editorWindow.Graphs[i].GraphID, 0);
            }

            string message = "";

            foreach(KeyValuePair<int, int> pair in SubGraphRefDic)
            {
                if(pair.Value == 0)
                {
                    BluePrintGraph g = editorWindow.GetGraph(pair.Key);
                    message += ("子图 =====> " + g.GraphName + " <====== 将被永久删除" + "\r\n");
                }
            }

            bool delete = false;

            if (string.IsNullOrEmpty(message)) delete = true;
            else if (EditorUtility.DisplayDialog("WARNING", message, "OK", "Cancel")) delete = true;
           
            if(delete)
            {
                DeleteNode(node);
                editorWindow.RemoveUselessGraph(SubGraphRefDic);
            }

            return false;
        }

        private void MarkDeleteNode(BluePrintNode node, bool bDelete)
        {
            node.MaskAsDelete = bDelete;

            if (node.IsSelected)
            {
                foreach (BluePrintNode n in selectNodeList)
                    n.MaskAsDelete = bDelete;
            }
        }

        public void GetSubGraphReference(ref Dictionary<int, int> dic)
        {
            for(int i =0; i < widgetList.Count; ++i)
            {
                BluePrintSubGraphNode node = widgetList[i] as BluePrintSubGraphNode;
                if (node != null)
                {
                    int count = DicAdd(ref dic, node.HostData.GraphID, node.MaskAsDelete ? 0 : 1);

                    if(!node.MaskAsDelete && count < 100)
                    {
                        BluePrintGraph g = editorWindow.GetGraph(node.HostData.GraphID);
                        g.GetSubGraphReference(ref dic);
                    }
                    
                }
            }
        }

        private int DicAdd(ref Dictionary<int, int> dic, int id, int count)
        {
            if (!dic.ContainsKey(id))
                dic.Add(id, 0);

            dic[id] += count;

            return dic[id];
        }

        public virtual void DeleteNode(BluePrintNode node)
        {
            if (node.IsSelected)
            {
                selectNode = null;
                foreach (BluePrintNode n in selectNodeList)
                {
                    _DelectNode(n);
                }
                selectNodeList.Clear();
            }
            _DelectNode(node);
        }

        private void _DelectNode(BluePrintNode node)
        {
            node.OnDeleted();
            widgetList.Remove(node);
        }
        #endregion

        private void DrawMouseLine()
        {
            if (ConnectStartPin != null && cachedMousePosition != Vector3.zero)
            {
                Vector3 start = new Vector3(ConnectStartPin.xMax, ConnectStartPin.yMin + (ConnectStartPin.yMax - ConnectStartPin.yMin) * 0.5f);
                Vector3 end = cachedMousePosition;

                //Vector3 startTangent = start + new Vector3(100, (end - start).y * 0.01f);
                //Vector3 endTangent = end - new Vector3(100, (end - start).y * 0.01f);
                DrawTool.DrawConnectionLine(start, end, BluePrintHelper.PinMainNoActiveColor, null, Scale);
                //Handles.DrawBezier(start, end, startTangent, endTangent, BluePrintHelper.PinMainNoActiveColor, null, 3);
            }
        }

        protected virtual void OnCopy() { }

        protected virtual void OnPaste() { }

        protected void MouseEvent()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        if (e.button == 0)
                        {
                            OnMouseLeftClicked(e);
                            if (!e.alt) DoMultiselect(e.mousePosition);
                        }
                        else if (e.button == 1)
                        {
                            OnMouseRightClicked(e);
                        }
                    }
                    break;
                case EventType.MouseDrag:
                    {
                        if (ConnectStartPin != null)
                        {
                            cachedMousePosition = e.mousePosition;

                            Vector3 start = new Vector3(ConnectStartPin.xMax, ConnectStartPin.yMin + (ConnectStartPin.yMax - ConnectStartPin.yMin) * 0.5f);
                            Vector3 end = e.mousePosition;
                        }
                        else if (selectNode == null && e.alt)
                            scrollPosition -= e.delta;
                    }
                    break;
                case EventType.MouseUp:
                    {
                        ConnectStartPin = null;
                        cachedMousePosition = Vector3.zero;
                    }
                    break;
                case EventType.KeyDown:
                    {
                        if(e.control && e.keyCode == KeyCode.C)
                        {
                            OnCopy();
                        }
                        else if(e.control && e.keyCode == KeyCode.V)
                        {
                            OnPaste();
                        }

                        OnKeyBoardEvent(e);
                    }
                    break;
            }

            if (e.alt || e.type == EventType.MouseUp)
            {
                if (!InMultiselect) ClearMultiselect();
                InMultiselect = false;
            }
            if (InMultiselect) DoMultiselect(e.mousePosition);
        }

        #endregion

        #region multi select

        public void ClearMultiselect()
        {
            foreach (BluePrintWidget widget in selectNodeList)
            {
                widget.IsSelected = false;
            }
            selectNodeList.Clear();
        }

        protected void DoMultiselect(Vector2 pos)
        {
            if (!InMultiselect) multiselectStartPosition = pos;
            multiselectEndPosition = pos;
            InMultiselect = true;
        }

        protected void DrawMultiSelectBox()
        {
            Vector2 start = new Vector2(Math.Min(multiselectStartPosition.x, multiselectEndPosition.x), Math.Min(multiselectStartPosition.y, multiselectEndPosition.y));
            Vector2 end = new Vector2(Math.Max(multiselectStartPosition.x, multiselectEndPosition.x), Math.Max(multiselectStartPosition.y, multiselectEndPosition.y));
            if (Vector2.Distance(start, end) < 40) return;
            Rect rect = new Rect(start, end - start);
            DrawTool.DrawStretchBox(rect, BlueprintStyles.BoxHighlighter4, 20);

            ClearMultiselect();
            foreach (BluePrintWidget widget in widgetList)
            {
                if (Math.Abs((widget.xMin + widget.xMax - start.x - end.x)) < ((widget.xMax + end.x - widget.xMin - start.x))
                && Math.Abs((widget.yMin + widget.yMax - start.y - end.y)) < ((widget.yMax + end.y - widget.yMin - start.y)))
                {
                    widget.IsSelected = true;
                    selectNodeList.Add(widget);
                }
            }
        }

        public void AddToMultiSelect(BluePrintNode node)
        {
            node.IsSelected = true;
            selectNodeList.Add(node);
        }

        public void AddComment()
        {
            if(selectNodeList.Count > 0)
            {
                BlueprintAreaComment comment = new BlueprintAreaComment();
                comment.Init(this);

                for(int i = 0; i < selectNodeList.Count; ++i)
                {
                    comment.AddCommentNode(selectNodeList[i]);
                }

                comment.Resize();
                commentList.Add(comment);
            }
        }

        public void RemoveComment(BlueprintAreaComment comment)
        {
            if (comment == SettingComment) SettingComment = null;
            commentList.Remove(comment);
        }

        public void ReSizeAllComment()
        {
            for(int i = 0; i < commentList.Count; ++i)
            {
                commentList[i].Resize();
            }
        }

        #endregion

        #region rightclick
        struct VariantParam
        {
            public Event evt;
            public int varIndex;
        }

        public static void RegisterRightClickMenu(string name, NodeDataClassName className)
        {
            if (!RegistedNodeType.ContainsKey(name))
            {
                RegistedNodeType.Add(name, className);
            }
        }

        public static void RegisterSubGraph(string name, int graphID)
        {
            if (!RegistedSubGraph.ContainsKey(name))
                RegistedSubGraph.Add(name, graphID);
        }

        public void SubgraphFunctionClicked(object o)
        {
            if (LevelEditor.LevelEditor.state == LEVEL_EDITOR_STATE.editor_mode)
            {
                string file = EditorUtility.OpenFilePanel("Select function file", Application.dataPath + "/BundleRes/Table/Level/Function", "Func");

                if (file.Length != 0)
                {
                    LevelGraphData graphData = DataIO.DeserializeData<LevelGraphData>(file);
                    GraphConfigData editorConfigData = DataIO.DeserializeData<GraphConfigData>(file.Replace(".Func", ".eFunc"));             

                    LevelGraph lvGraph = LevelEditor.LevelEditor.Instance.NewGraph<LevelGraph>(-1, graphData.name, false);
                    lvGraph.graphData = graphData;
                    lvGraph.graphConfigData = editorConfigData;

                    lvGraph.LoadDataToGraph();

                    int GraphID = LevelEditor.LevelEditor.Instance.GenGraphID(lvGraph);

                    AddControllerNode("BluePrint.BluePrintSubGraphNode", o, GraphID);               
                }
            }

        }

        //private string _cachedVarName = "";
        protected void AddCommonNodeToRightClickMenu(GenericMenu menu, Event e)
        {
            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedNodeType)
            {
                if(!pair.Key.StartsWith("Hidden"))
                    menu.AddItem(new GUIContent(pair.Key), false, (object o) => { AddControllerNode(pair.Value.NodeName, o, pair.Value.InternalParam); }, e);
            }

            foreach (BluePrintGraph g in editorWindow.Graphs)
            {
                if (!g.IsMainGraph)
                {
                    menu.AddItem(
                                        new GUIContent("SubGraph/" + g.GraphName),
                                        false,
                                        (object o) => { AddControllerNode("BluePrint.BluePrintSubGraphNode", o, g.GraphID); },
                                        e);
                }
            }

            menu.AddItem(
                                        new GUIContent("SubGraph/Function"),
                                        false,
                                        (object o) => { SubgraphFunctionClicked(o); },
                                        e);

            for (int i = 0; i < VarManager.UserVariant.Count; ++i)
            {
                string menuGetEntrance = "Variable/" + "Get " + VarManager.UserVariant[i].VariableName;
                menu.AddItem(new GUIContent(menuGetEntrance), false, (object o) => { AddVariableNode("BluePrint.CommonVariableGetNode", o); }, new VariantParam() { evt = e, varIndex = i });

                string menuSetEntrance = "Variable/" + "Set " + VarManager.UserVariant[i].VariableName;
                menu.AddItem(new GUIContent(menuSetEntrance), false, (object o) => { AddVariableNode("BluePrint.CommonVariableSetNode", o); }, new VariantParam() { evt = e, varIndex = i });
            }

            if(!IsMainGraph)
            {
                for (int i = 0; i < BlueprintGraphVariantManager.GlobalVariants.Count; ++i)
                {
                    string menuGetEntrance = "Variable/" + "Get " + BlueprintGraphVariantManager.GlobalVariants[i].VariableName;
                    menu.AddItem(new GUIContent(menuGetEntrance), false, (object o) => { AddGlobalVariableNode("BluePrint.CommonVariableGetNode", o); }, new VariantParam() { evt = e, varIndex = i });

                    string menuSetEntrance = "Variable/" + "Set " + BlueprintGraphVariantManager.GlobalVariants[i].VariableName;
                    menu.AddItem(new GUIContent(menuSetEntrance), false, (object o) => { AddGlobalVariableNode("BluePrint.CommonVariableSetNode", o); }, new VariantParam() { evt = e, varIndex = i });
                }
            }
        }

        private void AddControllerNode(string className, object o, int internalParam)
        {
            Event e = (Event)o;
            Type type = Type.GetType(className);
            BluePrintNode node = (BluePrintNode)Activator.CreateInstance(type, true);
            node.Init(this, e.mousePosition + scrollPosition);
            node.SetInternalParam(internalParam, "");
            node.PostInit();
            AddNode(node);
            graphConfigData.NodeConfigList.Add(node.nodeEditorData);
        }
        
        private void AddVariableNode(string className, object o)
        {
            Event e = ((VariantParam)o).evt;
            int varIndex = ((VariantParam)o).varIndex;

            Type type = Type.GetType(className);
            BluePrintNode node = (BluePrintNode)Activator.CreateInstance(type, true);
            node.Init(this, e.mousePosition + scrollPosition);

            if(node is CommonVariableGetNode)
            {
                (node as CommonVariableGetNode).HostData.VariableName = VarManager.UserVariant[varIndex].VariableName;
            }

            if(node is CommonVariableSetNode)
            {
                (node as CommonVariableSetNode).HostData.VariableName = VarManager.UserVariant[varIndex].VariableName;
            }
            AddNode(node);
            graphConfigData.NodeConfigList.Add(node.nodeEditorData);
        }

        private void AddGlobalVariableNode(string className, object o)
        {
            Event e = ((VariantParam)o).evt;
            int varIndex = ((VariantParam)o).varIndex;

            Type type = Type.GetType(className);
            BluePrintNode node = (BluePrintNode)Activator.CreateInstance(type, true);
            node.Init(this, e.mousePosition + scrollPosition);

            if (node is CommonVariableGetNode)
            {
                (node as CommonVariableGetNode).HostData.VariableName = BlueprintGraphVariantManager.GlobalVariants[varIndex].VariableName;
            }

            if (node is CommonVariableSetNode)
            {
                (node as CommonVariableSetNode).HostData.VariableName = BlueprintGraphVariantManager.GlobalVariants[varIndex].VariableName;
            }
            AddNode(node);
            graphConfigData.NodeConfigList.Add(node.nodeEditorData);
        }

        #endregion

        #region simulaton
        public IBlueprintRuntimeEngine simulatorEngine;

        public virtual void OnEnterSimulation()
        {
            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedNodeType)
            {
                CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "OnNodeEnterSimulationT");
            }

            NodeBehaviour = NodeBehaviourFactory.simulationBehaviour;
        }

        public virtual void OnEndSimulation()
        {
            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedNodeType)
            {
                CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "OnNodeEndSimulationT");
            }

            NodeBehaviour = NodeBehaviourFactory.defaultBehaviour;
        }

        public void OnNodeEnterSimulationT<N, T>()
            where T : BluePrintNodeBaseData, new()
            where N : BluePrintBaseDataNode<T>
        {
            foreach (BluePrintNode tmp in widgetList)
            {
                if (tmp is N)
                {
                    N node = tmp as N;
                    node.OnEnterSimulation();
                }
            }
        }

        public void OnNodeEndSimulationT<N, T>()
            where T : BluePrintNodeBaseData, new()
            where N : BluePrintBaseDataNode<T>
        {
            foreach (BluePrintNode tmp in widgetList)
            {
                if (tmp is N)
                {
                    N node = tmp as N;
                    node.OnEndSimulation();
                }
            }
        }

        protected void CallTemplateFunc(string nodename, string dataname ,string methodName, object[] param = null)
        {
            Type n1 = Type.GetType(nodename);
            Type t1 = Type.GetType(dataname);
            MethodInfo mi = this.GetType().GetMethod(methodName).MakeGenericMethod(new Type[] { n1, t1 });
            mi.Invoke(this, param);
        }

        public void SetRuntimeEngine(IBlueprintRuntimeEngine engine)
        {
            simulatorEngine = engine;
        }
        #endregion

        public void ShowNotification(GUIContent notification, float time = 5.0f)
        {
            editorWindow.ShowNotification(notification, time);
        }
    }
}
