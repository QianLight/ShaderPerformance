using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BluePrint;
using EcsData;
using UnityEditor;
using UnityEngine;

namespace EditorNode
{
    public abstract class BaseSkillNode : BluePrintNode
    {
        public float TriggerTime = -1;
        public virtual float NextTime { get { return TriggerTime; } }

        public static string PrefabPath = "Assets/BundleRes/Prefabs/";
        public static string ResourecePath = "Assets/BundleRes/";
        public static string CurvePath = "Assets/Editor/EditorResources/Server/";

        public virtual float OffsetX { get { return 0; } }
        public virtual BaseSkillGraph GetRoot { get { return (BaseSkillGraph)Root; } }

        protected override string HeaderImage
        {
            get { return isUsing ? _header_image : "BluePrint/Header0"; }
            set { _header_image = value; }
        }

        protected virtual bool isUsing
        {
            get
            {
                if (!GetHosterData<XBaseData>().TimeBased && pinList.Count > 0 && pinList[0].reverseConnections.Count == 0)
                {
                    return false;
                }
                return true;
            }
        }

        public List<Vector3> DependenceLine = new List<Vector3>();

        public override void Draw()
        {
            base.Draw();

            DrawDependenceLine();
        }

        private void DrawDependenceLine()
        {
            BuildDataFinish();

            Color preC = Handles.color;
            for (int i = 1; i < DependenceLine.Count; i += 2)
            {
                Vector3 start = DependenceLine[i - 1];
                Vector3 end = DependenceLine[i];

                List<Vector3> points = new List<Vector3>();

                float deltaX = 20 * Scale;
                float deltaY = 70 * Scale;
                points.Add(start);
                points.Add(new Vector3(start.x + deltaX, start.y));
                if (start.x + deltaX * 2 < end.x)
                {
                    points.Add(new Vector3(start.x + deltaX, end.y));
                }
                else
                {
                    points.Add(new Vector3(start.x + deltaX, start.y > end.y ? Math.Min(start.y, end.y + deltaY) : Math.Max(start.y, end.y - deltaY)));
                    points.Add(new Vector3(end.x - deltaX, start.y > end.y ? Math.Min(start.y, end.y + deltaY) : Math.Max(start.y, end.y - deltaY)));
                    points.Add(new Vector3(end.x - deltaX, end.y));
                }
                points.Add(end);
                {
                    for (int j = 1; j < points.Count; ++j)
                    {
                        if (points[j - 1] == points[j]) continue;
                        Vector3 pos1 = points[j - 1] * Scale;
                        Vector3 pos2 = points[j] * Scale;
                        Handles.color = Color.gray;
                        Handles.DrawDottedLine(pos1, pos2, Scale);
                        if (Vector3.Distance(pos2, pos1) > 30)
                        {
                            Vector3 arrow1 = (pos2 + pos1) * 0.5f + (Vector3)CFUtilPoolLib.XCommon.singleton.HorizontalRotateVetor2(pos2 - pos1, 90) * 5 * Scale;
                            Vector3 arrow2 = (pos2 + pos1) * 0.5f + (Vector3)CFUtilPoolLib.XCommon.singleton.HorizontalRotateVetor2(pos2 - pos1, -90) * 5 * Scale;
                            Vector3 arrow3 = (pos2 + pos1) * 0.5f + (pos2 - pos1).normalized * 15 * Scale;
                            Handles.color = Color.gray;
                            Handles.DrawLine(arrow3, arrow2);
                            Handles.DrawLine(arrow3, arrow1);
                            Handles.DrawLine(arrow2, arrow1);
                        }
                    }
                }
            }
            DependenceLine.Clear();
            Handles.color = preC;
        }

        public virtual void ScanPolicy(CFEngine.OrderResList result, CFEngine.ResItem item) { }

        public virtual void InitData<T>(T data, NodeConfigData configData) where T : XBaseData { }
        public virtual T GetHosterData<T>() where T : XBaseData { return null; }
        public virtual T CopyData<T>(T data) where T : XBaseData { return data.Clone() as T; }

        public virtual bool OneConnectPinOut { get { return false; } }

        public virtual bool RightDownEvent { get { return true; } }

        protected override bool OnMouseRightDown(Event e)
        {
            if (RightDownEvent)
            {
                var genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("Copy"), false, OnCopyClicked);
                genericMenu.AddItem(new GUIContent("Rename"), false, OnRenameClicked);
                //genericMenu.AddItem(new GUIContent("Enable"), Enabled, OnEnableClicked);
                genericMenu.AddItem(new GUIContent("Delete"), false, OnDeleteClicked);
                genericMenu.ShowAsContext();
            }

            return true;
        }

        static bool inDrag = false;
        static int DragRoot = 0;
        static HashSet<int> DragSet = new HashSet<int>();

        protected override bool OnMouseDrag(Event e)
        {
            base.OnMouseDrag(e);

            if (!inDrag)
            {
                DragSet.Clear();
                inDrag = true;
                DragRoot = GetHosterData<EcsData.XBaseData>().Index;
            }

            if (GetRoot.selectNodeList.Count == 0)
            {
                if (e.alt)
                {
                    foreach (var pin in pinList)
                    {
                        foreach (var connect in pin.connections)
                        {
                            var node = connect.connectEnd.GetNode<BaseSkillNode>();
                            if (!node.GetHosterData<EcsData.XBaseData>().TimeBased)
                            {
                                if (!DragSet.Contains(node.GetHosterData<EcsData.XBaseData>().Index))
                                {
                                    DragSet.Add(node.GetHosterData<EcsData.XBaseData>().Index);
                                    node.OnMouseDrag(e);
                                }
                            }
                        }
                    }
                }
            }

            if (GetHosterData<EcsData.XBaseData>().Index == DragRoot) inDrag = false;

            return true;
        }

        protected void OnCopyClicked()
        {
            BaseSkillGraph.CacheNode = this;
            BaseSkillGraph.CacheNodeList.Clear();
            foreach (BaseSkillNode node in Root.selectNodeList)
            {
                BaseSkillGraph.CacheNodeList.Add(node);
            }
        }

        public override void OnDeleteClicked()
        {
            Root.DeleteNode(this);
        }

        protected void OnRenameClicked()
        {
            IsEditing = true;
        }

        public override void DrawDataInspector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUITool.LabelField("Tips", (GUILayoutOption)null, this.GetType().ToString().Replace("EditorNode.", ""));
            var tex = EditorGUIUtility.FindTexture("_Help");
            if (TDTools.SkillNodeWikiHelper.WikiDic.TryGetValue($"{GetType().ToString().Replace("EditorNode.", "")}_F", out string url))
            {
                if (GUILayout.Button(new GUIContent(tex, "跳转wiki页面")))
                {
                    Application.OpenURL(url);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Calc"))
            {
                for (int i = 0; i < GetRoot.widgetList.Count; ++i) (GetRoot.widgetList[i] as BaseSkillNode).TriggerTime = -1;
                for (int i = 0; i < GetRoot.widgetList.Count; ++i)
                {
                    (GetRoot.widgetList[i] as BaseSkillNode).CalcTriggerTime();
                }
            }
            GetRoot.TimeFrameField("TriggerTime", TriggerTime);
            EditorGUILayout.EndHorizontal();
            DrawLine();
        }

        public void AddPinData<T>(T other) where T : XBaseData
        {
            XTransferData transfer = new XTransferData();
            transfer.Index = other.Index;
            GetHosterData<T>().TransferData.Add(transfer);
        }

        public virtual void BuildDataByPin()
        {
            GetHosterData<XBaseData>().TransferData.Clear();
            if (pinList.Count <= 1) return;
            foreach (BlueprintConnection transfer in pinList[1].connections)
            {
                AddPinData<XBaseData>(transfer.connectEnd.GetNode<BaseSkillNode>().GetHosterData<XBaseData>());
            }
        }

        public virtual void BuildPinByData(Dictionary<int, BaseSkillNode> IndexToNodeDic)
        {
            foreach (XTransferData transfer in GetHosterData<XBaseData>().TransferData)
            {
                if (transfer.Index == -1) return;
                BluePrintPin startPin = pinList[1];
                BluePrintPin endPin = IndexToNodeDic[transfer.Index].pinList[0];
                ConnectPin(startPin, endPin);
            }
        }

        public virtual void BuildDataFinish() { }

        public virtual bool BranchNode => false;
        public virtual bool IgnoreMultiIn => true;
        public virtual int CheckConnectivity(BluePrintPin pinIn, ref HashSet<BaseSkillNode> IndexSet, BluePrintPin pinOut = null)
        {
            int rootCount = 0;

            if (pinIn == null) return 0;

            IndexSet.Add(pinIn.GetNode<BaseSkillNode>());
            if (pinOut != null)
            {
                BaseSkillNode nextNode = pinOut.GetNode<BaseSkillNode>();
                if (IndexSet.Contains(nextNode))
                {
                    if (nextNode == this)
                        rootCount += 999;
                    else if (nextNode.BranchNode)
                    {

                    }
                    else rootCount += 1;
                }
                else rootCount += CheckConnectivity(nextNode.GetPin(-1), ref IndexSet);
            }
            else if (pinIn.reverseConnections.Count == 0 && !IgnoreMultiIn)
                ++rootCount;

            for (int i = 0; i < pinIn.reverseConnections.Count; ++i)
            {
                BluePrintPin nextPin = pinIn.reverseConnections[i].reverseConnectEnd;
                BaseSkillNode nextNode = nextPin.GetNode<BaseSkillNode>();
                if (IndexSet.Contains(nextNode))
                {
                    if (nextNode == this)
                        rootCount += 999;
                    else if (nextNode.BranchNode)
                        continue;
                    else if (!IgnoreMultiIn)
                        rootCount += 1;
                }
                else rootCount += CheckConnectivity(nextNode.GetPin(-1), ref IndexSet);
            }

            return rootCount;
        }

        public override bool CanConnect(BluePrintPin start, BluePrintPin end)
        {
            if (end.GetNode<BaseSkillNode>().GetHosterData<XBaseData>().TimeBased) return false;
            HashSet<BaseSkillNode> IndexSet = new HashSet<BaseSkillNode>();
            if (end.GetNode<BaseSkillNode>() == this && CheckConnectivity(end, ref IndexSet, start) > 1) return false;
            if (start.GetNode<BaseSkillNode>().OneConnectPinOut && start.connections.Count > 0) return false;
            for (int i = 0; i < start.connections.Count; ++i)
                if (start.connections[i].connectEnd == end)
                    return false;

            return true;
        }

        public virtual void SetDebug(bool flag = true)
        {
            hasError = flag;
            for (int i = 1; i < pinList.Count; ++i)
            {
                foreach (BlueprintConnection transfer in pinList[i].connections)
                {
                    transfer.connectEnd.GetNode<BaseSkillNode>().SetDebug(false);
                }
            }
        }

        public virtual bool CompileCheck()
        {
            if (GetHosterData<XBaseData>().Index >= EditorEcs.Xuthus_VirtualServer.XNodeMax || GetHosterData<XBaseData>().Index >= Xuthus.XNodeMax)
            {
                LogError("节点数超过最大上限(" + EditorEcs.Xuthus_VirtualServer.XNodeMax + ")！！！");
                return false;
            }

            if (GetHosterData<XBaseData>().TimeBased && pinList[0].reverseConnections.Count > 0)
            {
                LogError("Node_" + GetHosterData<XBaseData>().Index + "，时间触发节点，不能连线触发！！！");
                return false;
            }

            HashSet<BaseSkillNode> IndexSet = new HashSet<BaseSkillNode>();
            if (CheckConnectivity(GetPin(-1), ref IndexSet) > 1)
            {
                hasError = true;
                LogError("Node_" + GetHosterData<XBaseData>().Index + "，有多个触发入口！！！");
                return false;
            }

            if (GetRoot.GetConfigData<XConfigData>().Name != null && GetRoot.GetConfigData<XConfigData>().Name.EndsWith("_Hit_Header"))
            {
                if (!(GetHosterData<XBaseData>() is XSwitchData) &&
                !(GetHosterData<XBaseData>() is XConditionData) &&
                !(GetHosterData<XBaseData>() is XMultiConditionData) &&
                !(GetHosterData<XBaseData>() is XSpecialActionData) &&
                !(GetHosterData<XBaseData>() is XBuffData) &&
                !(GetHosterData<XBaseData>() is XMessageData))
                {
                    LogError("Hit_Header 脚本内只能存在SwitchNode,ConditionNode,XMultiConditionNode,SpecialActionNode,XBuffNode,XMessageNode！！！");
                    return false;
                }
                else if (GetRoot.GetConfigData<XConfigData>().Length != 0)
                {
                    LogError("Hit_Header 脚本时长必须为0！！！");
                    return false;
                }
            }


            return true;
        }

        public virtual bool useParam(int index)
        {
            Type t = GetHosterData<XBaseData>().GetType();
            var members = t.GetMembers();
            for (int i = 0; i < members.Length; ++i)
            {
                if (members[i].Name.EndsWith("ParamIndex"))
                {
                    object o = GetHosterData<XBaseData>().GetPublicField(members[i].Name);
                    if ((int)o == index)
                        return true;
                }
            }

            return false;
        }

        protected void LogError(string error)
        {
            Debug.LogError(error + "\n" + GetRoot.GetConfigData<XConfigData>().Name);
            GetRoot.ShowNotification(new GUIContent(error));
        }

        public virtual void PreBuild() { }

        protected void GetNodeByIndex<T>(ref BaseSkillNode node, ref int index, bool showgui = false, string label = "", [CallerFilePath] string path = "") where T : BaseSkillNode
        {
            if (!(node is T)) node = GetRoot.GetNodeByIndex(index);
            else node = GetRoot.widgetList.Contains(node) ? node : null;

            if (!(node is T)) index = -1;
            else index = node.GetHosterData<XBaseData>().Index;

            if (showgui)
            {
                EditorGUILayout.BeginHorizontal();

                string showlabel = label == "" ? (typeof(T).ToString().Replace("EditorNode.", "") + "Index") : label;
                index = EditorGUILayout.IntField(new GUIContent(showlabel, EditorGUITool.Translate(EditorGUITool.GetTypeString(path) + "_" + showlabel)), index);

                if (index != -1 && node != null)
                {
                    if (GUILayout.Button("JumpTo"))
                    {
                        if (GetRoot.selectNode != null)
                            GetRoot.selectNode.IsSelected = false;
                        GetRoot.PreSelectNode = GetRoot.selectNode;
                        GetRoot.selectNode = node;
                        GetRoot.selectNode.IsSelected = true;
                        GetRoot.FocusNode(node);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (node != null)
            {
                DependenceLine.Add(this.Bounds.position + new Vector2(this.Bounds.width, 20));
                DependenceLine.Add(node.Bounds.position + new Vector2(0, 20));
            }

            if (node != null && index != node.GetHosterData<XBaseData>().Index)
                node = null;
        }

        public virtual void CopyDataFromTemplate(int templateID, int presentID)
        {

        }

        protected void ObjectFieldByPath<T>(string label, ref T obj, ref string path, [CallerFilePath] string cpath = "") where T : UnityEngine.Object
        {
            obj = EditorGUITool.ObjectField(label, obj, typeof(T), false, cpath) as T;
            if (obj != null)
            {
                path = AssetDatabase.GetAssetPath(obj).Replace(ResourecePath, "");
                path = path.Remove(path.LastIndexOf('.'));
            }
            else path = "";
        }

        protected void DrawLine()
        {
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.Width(300), GUILayout.Height(2) });
        }

        protected void DrawHitParam<T>(T data, [CallerFilePath] string path = "") where T : XBaseResult
        {
            data.ParamHitPose = Mathf.Min(7, Mathf.Max(0, EditorGUITool.IntField("ParamHitPose", data.ParamHitPose, path)));
            data.ParamFreeze = Mathf.Min(15f, Mathf.Max(0, TimeFrameField("ParamFreeze", data.ParamFreeze, false, false, 60, path)));
            data.ParamKnockBackDirType = EditorGUITool.Toggle("ParamKnockBackDirType", data.ParamKnockBackDirType, path);
            data.ParamAnimCurveRatio = Mathf.Min(15.9f, Mathf.Max(0, EditorGUITool.FloatField("ParamAnimCurveRatio", data.ParamAnimCurveRatio, path)));
            data.ParamForwardCurveScale = Mathf.Min(15.9f, Mathf.Max(-15.9f, EditorGUITool.FloatField("ParamForwardCurveScale", data.ParamForwardCurveScale, path)));
            data.ParamUpCurveScale = Mathf.Min(15.9f, Mathf.Max(0, EditorGUITool.FloatField("ParamUpCurveScale", data.ParamUpCurveScale, path)));

            EditorGUITool.Vector3Field("ParamOffset", ref data.ParamOffsetX, ref data.ParamOffsetY, ref data.ParamOffsetZ, 20, path);

            data.ParamKnockBackDuration = TimeFrameField("ParamKnockDuration", data.ParamKnockBackDuration, false, false, 60, path);
            data.ParamKnockBackRandomRange = EditorGUITool.FloatField("ParamKnockRandomRange", data.ParamKnockBackRandomRange, path);
            data.ParamKnockBackVelocity = EditorGUITool.FloatField("ParamKnockVelocity", data.ParamKnockBackVelocity, path);
            data.ParamVelocityH = PowerAssistField("ParamVelocityH", data.ParamVelocityH);
            data.ParamVelocityV = PowerAssistField("ParamVelocityV", data.ParamVelocityV);
        }

        #region ConditionData

        protected void DrawConditions(List<XConditionData> cond, string label, List<int> error = null, string without = "@", [CallerFilePath] string path = "")
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            if (cond.Count < 5)
            {
                if (GUILayout.Button("+"))
                {
                    cond.Add(new XConditionData());
                    if (error != null) error.Add(0);
                }
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < cond.Count; ++i)
            {
                DrawLine();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Condition_" + i.ToString());
                if (GUILayout.Button("Delete"))
                {
                    cond.RemoveAt(i);
                    if (error != null) error.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                DrawConditionParam(cond[i], without, path);
                if (error != null) error[i] = (int)(ClientEcsData.EConditionFailType)EditorGUILayout.EnumPopup("ErrorType", (ClientEcsData.EConditionFailType)error[i]);
            }
        }

        protected void DrawConditionParam(XConditionData data, string without, [CallerFilePath] string path = "")
        {
            int index = 0;
            string key;
            GetRoot.FunctionHash2Name.TryGetValue(data.FunctionHash, out key);
            index = GetRoot.GetFunctionIndex(key, "c_", without);
            string[] funcName = GetRoot.GetFunctionTranslate("c_", without);
            data.Not = EditorGUITool.Toggle("Not", data.Not, path);
            GetRoot.FunctionName2Hash.TryGetValue(funcName[EditorGUITool.Popup("Function: ", index, EditorGUITool.Translate(funcName), path)], out data.FunctionHash);
            if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_is_slot_down") ||
                data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_is_slot_up"))
            {
                data.Parameter1 = (int)(XHitType)EditorGUITool.Popup("Slot  (Max为当前技能)", data.Parameter1, Enum.GetNames(typeof(XInputSlot)), path);
                data.Parameter2 = EditorGUITool.IntField("Frame", data.Parameter2, path);
            }
            else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_is_attr_less") ||
                data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_is_attr_greater"))
            {
                data.Parameter1 = EditorGUITool.IntField("AttrID", data.Parameter1, path);
                data.Parameter2 = EditorGUITool.IntField("Value", data.Parameter2, path);
            }
            else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_get_buff_layers_count"))
            {
                data.Parameter1 = EditorGUITool.IntField("BuffID", data.Parameter1, path);
                data.Parameter2 = EditorGUITool.IntField("Layer", data.Parameter2, path);
            }
            else
            {
                if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_is_attr_guard")) { }
                else data.Operation = EditorGUITool.Popup("Operation : ", data.Operation, EditorGUITool.Translate<XConditionOperation>(), path);
                if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_get_last_hit"))
                {
                    data.Rhs = EditorGUITool.Popup("HitType", data.Rhs, EditorGUITool.Translate<XHitType>(), path);
                }
                else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_get_last_state"))
                {
                    data.Rhs = EditorGUITool.Popup("Status", data.Rhs, EditorGUITool.Translate<XStateType>(), path);
                }
                else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_hit_direction"))
                {
                    data.Rhs = EditorGUITool.Popup("Direction", data.Rhs, EditorGUITool.Translate<XHitDirection>(), path);
                }
                else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_get_death_from_slot"))
                {
                    data.Rhs = EditorGUITool.Popup("State", data.Rhs, EditorGUITool.Translate<XHitSlot>(), path);
                }
                else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_is_at_state"))
                {
                    data.Rhs = EditorGUITool.Popup("State", data.Rhs, EditorGUITool.Translate<XStateType>(), path);
                }
                else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_forbidenby_buff_state"))
                {
                    data.Rhs = EditorGUITool.IntField("ForbidenByBuffState", data.Rhs);
                    if (data.Rhs != 0 && GUILayout.Button("ClearForbidenByBuff")) data.Rhs = 0;
                }
                else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_get_skill_level"))
                {
                    data.Rhs = EditorGUITool.IntField("Level", data.Rhs, path);
                    data.StringParameter = EditorGUITool.TextField("SkillName", data.StringParameter, path);
                    data.Parameter1 = (int)CFUtilPoolLib.XCommon.singleton.XHash(data.StringParameter);
                }
                else if(data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_get_buff_level"))
                {
                    data.Rhs = EditorGUITool.IntField("Level", data.Rhs, path);
                    data.Parameter1 = EditorGUITool.IntField("BuffID", data.Parameter1, path);
                }
                else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_get_last_skill"))
                {
                    data.StringParameter = EditorGUITool.TextField("SkillName", data.StringParameter, path);
                    data.Rhs = (int)CFUtilPoolLib.XCommon.singleton.XHash(data.StringParameter);
                }
                else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_get_paracurve_up_velocity") ||
                    data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_get_paracurve_height"))
                {
                    data.Rhs = EditorGUITool.IntDataFloatField("Rhs", data.Rhs);
                }
                else if (data.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("c_is_attr_guard")) { }
                else
                {
                    data.Rhs = EditorGUITool.IntField("Rhs : ", data.Rhs, path);
                }
            }
        }
        #endregion

        #region Tool
        public void ParamTemplate(string label, object obj, Action callback)
        {
            EditorGUILayout.BeginHorizontal();

            if (obj != null)
            {
                string name = label + "ParamIndex";
                object paramIndex = obj.GetPublicField(name);
                if (paramIndex != null)
                {
                    paramIndex = EditorGUILayout.Popup((int)paramIndex, GetRoot.GetConfigData<XConfigData>().ParamNames.ToArray(), GUILayout.Width(60f));
                    if (GUILayout.Button("-", GUILayout.Width(20f))) paramIndex = -1;
                    obj.SetPublicField(name, paramIndex);
                }
            }

            callback();
            EditorGUILayout.EndHorizontal();
        }

        public float TimeFrameField(string label, float value, bool useInt = false, bool delay = false, float max = 60f, [CallerFilePath] string path = "")
        {
            ParamTemplate(label, GetHosterData<XBaseData>(),
                () =>
                {
                    EditorGUITool.LabelField(label, GUILayout.MaxWidth(100), path);
                    GUILayout.FlexibleSpace();
                    value = EditorGUILayout.FloatField(value, GUILayout.Width(50));
                    EditorGUITool.LabelField("(s)", GUILayout.Width(20));
                    if (useInt)
                        if (!delay) value = GetRoot.FrameToTime(EditorGUILayout.IntField((int)(GetRoot.TimeToFrame(value) + 0.5f), GUILayout.Width(30)));
                        else value = GetRoot.FrameToTime(EditorGUILayout.DelayedIntField((int)(GetRoot.TimeToFrame(value) + 0.5f), GUILayout.Width(30)));
                    else
                        if (!delay) value = GetRoot.FrameToTime(EditorGUILayout.FloatField(GetRoot.TimeToFrame(value), GUILayout.Width(30)));
                    else value = GetRoot.FrameToTime(EditorGUILayout.DelayedFloatField(GetRoot.TimeToFrame(value), GUILayout.Width(30)));
                    EditorGUITool.LabelField("(f)", GUILayout.Width(20));
                });

            return Mathf.Min(max, value);
        }

        public float PowerAssistField(string label, float value, [CallerFilePath] string path = "")
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUITool.LabelField(label, GUILayout.MaxWidth(100), path);
            GUILayout.FlexibleSpace();
            float symbol = value < 0 ? -1 : 1;
            value = Mathf.Abs(value);
            value = EditorGUILayout.FloatField(symbol * value, GUILayout.Width(50));
            symbol = value < 0 ? -1 : 1;
            value = Mathf.Abs(value);

            EditorGUITool.LabelField("^2=", GUILayout.Width(25));
            value = value * value;
            value = EditorGUILayout.FloatField(symbol * value, GUILayout.Width(60));
            symbol = value < 0 ? -1 : 1;
            value = Mathf.Sqrt(Mathf.Abs(value));

            EditorGUILayout.EndHorizontal();
            return symbol * value;
        }

        public void IntListField(ref List<int> list, int maxCount = 10, string name = "Param", [CallerFilePath] string path = "")
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUITool.LabelField(name, (GUILayoutOption)null, path);
            if (list.Count < maxCount)
            {
                if (GUILayout.Button("+"))
                {
                    list.Add(0);
                }
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < list.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    list[i] = EditorGUITool.IntField(name + "_" + i, list[i], path);
                }

                if (GUILayout.Button("-"))
                {
                    list.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public virtual void CalcTriggerTime()
        {

        }
        protected void DFSTriggerTime(BaseSkillNode baseNode)
        {
            if (baseNode == null) return;
            if (baseNode.TriggerTime == -1) return;

            for (int i = 0; i < baseNode.GetHosterData<XBaseData>().TransferData.Count; ++i)
            {
                DFSTriggerTime(baseNode, baseNode.GetHosterData<XBaseData>().TransferData[i].Index);
            }
        }

        protected void DFSTriggerTime(BaseSkillNode baseNode, int index)
        {
            if (baseNode == null) return;
            if (baseNode.TriggerTime == -1) return;

            BaseSkillNode node = GetRoot.GetNodeByIndex(index);
            if (node != null)
            {
                if (SetTriggerTime(baseNode.NextTime, ref node.TriggerTime))
                    node.CalcTriggerTime();
            }
        }

        protected bool SetTriggerTime(float from, ref float to)
        {
            if (to == -1 || to > from)
            {
                to = from;
                return true;
            }
            return false;
        }

        protected void CheckAndDo(bool check, Action succDo, Action failDo = null)
        {
            if (check)
            {
                if (succDo != null) succDo();
            }
            else
            {
                if (failDo != null) failDo();
            }
        }
        #endregion

    }
}