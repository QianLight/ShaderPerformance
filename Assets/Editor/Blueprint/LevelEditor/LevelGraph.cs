using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEditor;
using UnityEngine;
using BluePrint;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace LevelEditor
{
    public struct RightClickParam
    {
        public string key;
        public int param;
        public string customType;

        public RightClickParam(string n1, int sub, string custom = "")
        {
            key = n1;
            param = sub;
            customType = custom;
        }
    }

    public class LevelGraph : BluePrintGraph
    {
        public LevelGraphData graphData;

        protected Dictionary<string, RightClickParam> RightClickList = new Dictionary<string, RightClickParam>();

        protected Dictionary<string, NodeDataClassName> RegistedLevelNodeType = new Dictionary<string, NodeDataClassName>();

        public static GUILayoutOption[] TitleLayout = new GUILayoutOption[] { GUILayout.Width(120f) };
        public static GUILayoutOption[] ContentLayout = new GUILayoutOption[] { GUILayout.Width(150f) };
        public static GUILayoutOption[] TextContentLayout = new GUILayoutOption[] { GUILayout.Width(150f), GUILayout.Height(50f) };

        public static GUILayoutOption TitleLayoutW = GUILayout.Width(120f);
        public static GUILayoutOption ContentLayoutW = GUILayout.Width(150f);
        private float wheelScale = 0.2f;

        public override void Init(BlueprintEditor editor)
        {
            base.Init(editor);

            LevelHelper.ReadLevelCustomConfig();
            LevelHelper.ReadLevelNodeConfig();

            RightClickList.Add("WaveNode", new RightClickParam( "WaveNode", 0));
            RightClickList.Add("RefWaveNode", new RightClickParam("RefWaveNode", 0));
            RightClickList.Add("MonitorNode", new RightClickParam("MonitorNode", 0));
            RightClickList.Add("ExStringNode", new RightClickParam("ExStringNode", 0));
            RightClickList.Add("VictoryNode", new RightClickParam("VictoryNode", 0));
            RightClickList.Add("FailNode", new RightClickParam("FailNode", 0));
            RightClickList.Add("Trigger", new RightClickParam("LevelTriggerNode", 0));
            RightClickList.Add("MonitorValueNode", new RightClickParam("LevelMonitorValueNode", 0));
            RightClickList.Add("AppointPos", new RightClickParam("LevelAppointPosNode", 0)); // 只有在main子图中才会显示该节点
            RightClickList.Add("RobotWave", new RightClickParam("LevelRobotWaveNode", 0));
            RightClickList.Add("SendMessage", new RightClickParam("LevelSendMessageNode", 0));
            RightClickList.Add("ReceiveMessage", new RightClickParam("LevelReceiveMessageNode", 0));
            RightClickList.Add("Doodad", new RightClickParam("LevelDoodadNode", 0));
            RightClickList.Add("RandomStream", new RightClickParam("LevelRandomStreamNode", 0));
            RightClickList.Add("Misc/RandomNode", new RightClickParam("LevelRandomNode", 0));
            RightClickList.Add("Misc/Record", new RightClickParam("LevelRecordNode", 0));
            RightClickList.Add("Misc/StartTiming", new RightClickParam("LevelStartTimingNode", 0));
            RightClickList.Add("Misc/EndTiming", new RightClickParam("LevelEndTimingNode", 0));
            RightClickList.Add("Misc/SwitchPartnerNode", new RightClickParam("SwitchPartnerNode", 0));
            RightClickList.Add("Variable/GetExternalVar", new RightClickParam("LevelGetExternalVarNode", 0));
            RightClickList.Add("Variable/SetExternalVar", new RightClickParam("LevelSetExternalVarNode", 0));
            RightClickList.Add("Variable/Varient", new RightClickParam("LevelVarientNode", 0));
            RightClickList.Add("Variable/StringBuild", new RightClickParam("LevelStringBuildNode", 0));
            RightClickList.Add("Variable/MonsterCount", new RightClickParam("LevelMonsterCountNode", 0));
            RightClickList.Add("Misc/RunSkillScript", new RightClickParam("RunSkillNode", 0));
            RightClickList.Add("Misc/ServerRatio", new RightClickParam("RatioNode", 0));
            RightClickList.Add("LevelScript/Server/SetData", new RightClickParam("LevelSetDataNode", 0));
            RightClickList.Add("LevelScript/Server/BuffSta", new RightClickParam("LevelBuffStaNode", 0));
            //RightClickList.Add("Misc/TemproryPartner", new RightClickParam("LevelTemproryPartnerNode", 0));

            RightClickList.Add("LevelScript/Server/AddBuff", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Addbuff));
            RightClickList.Add("LevelScript/Server/RemoveBuff", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Removebuff));
            RightClickList.Add("LevelScript/Server/CheckBuff", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_CheckBuff));
            RightClickList.Add("LevelScript/Server/KillAlly", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_KillAlly));
            RightClickList.Add("LevelScript/Server/KillWave", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_KillWave));
            RightClickList.Add("LevelScript/Server/Opendoor", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Opendoor));
            //RightClickList.Add("LevelScript/OpendoorEx", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_OpendoorEx));
            RightClickList.Add("LevelScript/Server/Cutscene", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Cutscene));
            //RightClickList.Add("LevelScript/Summon", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Summon));
            RightClickList.Add("LevelScript/Server/KillAllSpawn", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_KillAllSpawn));
            RightClickList.Add("LevelScript/Server/SendAICmd", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_SendAICmd));
            RightClickList.Add("LevelScript/Server/SetExtString", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_SetExtString));
            RightClickList.Add("LevelScript/Server/ClearExtString", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_ClearExtString));
            RightClickList.Add("LevelScript/Server/TransferLocation", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_TransferLocation));
            RightClickList.Add("LevelScript/Server/Message", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Message));
            RightClickList.Add("LevelScript/Server/SetAI", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_SetAI));            
            RightClickList.Add("LevelScript/Server/AutoChange", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_AutoChange));
            RightClickList.Add("LevelScript/Server/ResetNode", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_ResetNode));
            RightClickList.Add("LevelScript/Server/Sneak", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Sneak));
            RightClickList.Add("LevelScript/Server/CheckEntityBuff", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_CheckEntityBuff));
            RightClickList.Add("LevelScript/Server/MonitorCheckBuff", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_MonitorCheckBuff));
            RightClickList.Add("LevelScript/Server/ShareDamage", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_ShareDamage));

            RightClickList.Add("LevelScript/Client/Talk", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Talk));
            RightClickList.Add("LevelScript/Client/Notice", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Notice));
            RightClickList.Add("LevelScript/Client/StopNotice", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_StopNotice));
            RightClickList.Add("LevelScript/Client/NpcPopSpeek", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_NpcPopSpeek));
            RightClickList.Add("LevelScript/Client/Direction", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Direction));
            RightClickList.Add("LevelScript/Client/Bubble", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Bubble));
            RightClickList.Add("LevelScript/Client/HideBillboard", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_HideBillboard));
            RightClickList.Add("LevelScript/Client/ShowLevel", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_ShowLevel));
            RightClickList.Add("LevelScript/Client/ShowTarget", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_ShowTarget));
            RightClickList.Add("LevelScript/Client/ShowBossName", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_ShowBossName));
            RightClickList.Add("LevelScript/Client/StageGuide", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_StageGuide));
            RightClickList.Add("LevelScript/Client/LevelName", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_LevelName));
            RightClickList.Add("LevelScript/Client/ShowWay", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_ShowWay));
            RightClickList.Add("LevelScript/Client/Unlock", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Unlock));
            RightClickList.Add("LevelScript/Client/CameraControl", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_CameraControl));
            RightClickList.Add("LevelScript/Client/FreeCamera", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_FreeCamera));
            RightClickList.Add("LevelScript/Client/React", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_React));
            RightClickList.Add("LevelScript/Client/RenderEnv", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_RenderEnv));
            RightClickList.Add("LevelScript/Client/CacheMap", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_CacheMap));
            RightClickList.Add("LevelScript/Client/CallUI", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_CallUI));
            RightClickList.Add("LevelScript/Client/AreaCamera", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_AreaCamera));
            RightClickList.Add("LevelScript/Client/SetFxActive", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_SetFx));
            RightClickList.Add("LevelScript/Client/PostTreatment", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_PostTreatment));
            RightClickList.Add("LevelScript/Client/Action", new RightClickParam("LevelScript", (int)LevelScriptCmd.LeveL_Cmd_Action));
            RightClickList.Add("LevelScript/Client/SetEntityFx", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_SetEntityFx));
            RightClickList.Add("LevelScript/Client/Weather", new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Weather));

            for (int i = 0; i < LevelHelper.dList.CustomDefineNodes.Count; ++i)
            {
                string name = LevelHelper.dList.CustomDefineNodes[i].name;

                RightClickList.Add("LevelScript/Custom/"+name, new RightClickParam("LevelScript", (int)LevelScriptCmd.Level_Cmd_Custom, name));
            }

            RegisterLevelNodeType("WaveNode", new NodeDataClassName("LevelEditor.LevelWaveNode", "LevelEditor.LevelWaveData"));
            RegisterLevelNodeType("RefWaveNode", new NodeDataClassName("LevelEditor.LevelRefWaveNode", "LevelEditor.LevelRefWaveData"));
            RegisterLevelNodeType("MonitorNode", new NodeDataClassName("LevelEditor.LevelMonitorNode", "LevelEditor.LevelMonitorData"));
            RegisterLevelNodeType("ExStringNode", new NodeDataClassName("LevelEditor.LevelExstringNode", "LevelEditor.LevelExstringData"));
            RegisterLevelNodeType("LevelScript", new NodeDataClassName("LevelEditor.LevelScriptNode", "LevelEditor.LevelScriptData"));
            RegisterLevelNodeType("VictoryNode", new NodeDataClassName("LevelEditor.LevelVictoryNode", "LevelEditor.LevelVictoryData"));
            RegisterLevelNodeType("FailNode", new NodeDataClassName("LevelEditor.LevelFailNode", "LevelEditor.LevelFailData"));
            RegisterLevelNodeType("LevelRandomNode", new NodeDataClassName("LevelEditor.LevelRandomNode", "LevelEditor.LevelRandomNodeData"));
            RegisterLevelNodeType("LevelRecordNode", new NodeDataClassName("LevelEditor.LevelRecordNode", "LevelEditor.LevelRecordData"));
            RegisterLevelNodeType("LevelGetExternalVarNode", new NodeDataClassName("LevelEditor.LevelGetExternalVarNode", "LevelEditor.LevelVarData"));
            RegisterLevelNodeType("LevelSetExternalVarNode", new NodeDataClassName("LevelEditor.LevelSetExternalVarNode", "LevelEditor.LevelVarData"));
            RegisterLevelNodeType("LevelStartTimingNode", new NodeDataClassName("LevelEditor.LevelStartTimingNode", "LevelEditor.LevelStartTimeData"));
            RegisterLevelNodeType("LevelEndTimingNode", new NodeDataClassName("LevelEditor.LevelEndTimingNode", "LevelEditor.LevelEndTimeData"));
            RegisterLevelNodeType("SwitchPartnerNode", new NodeDataClassName("LevelEditor.LevelSwitchPartnerNode", "LevelEditor.LevelSwitchPartnerData"));
            RegisterLevelNodeType("RunSkillNode", new NodeDataClassName("LevelEditor.LevelRunSkillNode", "LevelEditor.LevelRunSkillScriptData"));
            RegisterLevelNodeType("RatioNode", new NodeDataClassName("LevelEditor.LevelRatioNode", "LevelEditor.LevelRatioData"));
            //RegisterLevelNodeType("LevelTemproryPartnerNode", new NodeDataClassName("LevelEditor.LevelTemproryPartnerNode", "LevelEditor.LevelTemproryPartnerData"));
            RegisterLevelNodeType("LevelVarientNode", new NodeDataClassName("LevelEditor.LevelVarientNode", "LevelEditor.LevelVarientData"));
            RegisterLevelNodeType("LevelStringBuildNode", new NodeDataClassName("LevelEditor.LevelStringBuildNode", "LevelEditor.LevelStringBuildData"));
            RegisterLevelNodeType("LevelTriggerNode", new NodeDataClassName("LevelEditor.LevelTriggerNode", "LevelEditor.LevelTriggerData"));
            RegisterLevelNodeType("LevelMonitorValueNode", new NodeDataClassName("LevelEditor.LevelMonitorValueNode", "LevelEditor.LevelMonitorValueData"));
            RegisterLevelNodeType("LevelSetDataNode", new NodeDataClassName("LevelEditor.LevelSetDataNode", "LevelEditor.LevelSetParamData"));
            RegisterLevelNodeType("LevelBuffStaNode", new NodeDataClassName("LevelEditor.LevelBuffStaNode", "LevelEditor.LevelBuffStaData"));
            RegisterLevelNodeType("LevelAppointPosNode", new NodeDataClassName("LevelEditor.LevelAppointPosNode", "LevelEditor.LevelAppointPosData"));
            RegisterLevelNodeType("LevelMonsterCountNode", new NodeDataClassName("LevelEditor.LevelMonsterCountNode", "LevelEditor.LevelMonsterCountData"));
            RegisterLevelNodeType("LevelRobotWaveNode", new NodeDataClassName("LevelEditor.LevelRobotWaveNode", "LevelEditor.LevelRobotWaveData"));
            RegisterLevelNodeType("LevelSendMessageNode", new NodeDataClassName("LevelEditor.LevelSendMessageNode", "LevelEditor.LevelSendMessageData"));
            RegisterLevelNodeType("LevelReceiveMessageNode", new NodeDataClassName("LevelEditor.LevelReceiveMessageNode", "LevelEditor.LevelReceiveMessageData"));
            RegisterLevelNodeType("LevelDoodadNode", new NodeDataClassName("LevelEditor.LevelDoodadNode", "LevelEditor.LevelDoodadData"));
            RegisterLevelNodeType("LevelRandomStreamNode", new NodeDataClassName("LevelEditor.LevelRandomStreamNode", "LevelEditor.LevelRandomStreamData"));
        }

        public override void Update()
        {
            base.Update();

            foreach (BluePrintNode tmp in widgetList)
            {
                if (tmp is LevelWaveNode)
                {
                    (tmp as LevelWaveNode).Update();
                }
            }
        }

        public void Clone(ref LevelGraph dest)
        {
            string cachedGraphPath = Application.dataPath + "/Editor Default Resources/Level/__cache_graph.lvtpl";
            selectNode = null;
            selectNodeList.Clear();
            SaveTpl(cachedGraphPath);

            dest.LoadTpl(cachedGraphPath, Vector2.zero);


        }

        public void ReloadMonster()
        {
            foreach (BluePrintNode tmp in widgetList)
            {
                if (tmp is LevelWaveNode)
                {
                    LevelWaveNode node = (tmp as LevelWaveNode);
                    node.ReloadMonster();
                }
            }
        }

        public void ReloadStartOrEndPoint()
        {
            foreach(BluePrintNode tmp in widgetList)
            {
                (tmp as LevelAppointPosNode)?.Reload();
            }
        }

        public void RelocateMonster(float delta)
        {
            foreach (BluePrintNode tmp in widgetList)
            {
                if (tmp is LevelWaveNode)
                {
                    LevelWaveNode node = (tmp as LevelWaveNode);
                    node.RelocateMonster(delta);
                }
            }
        }

        public void RegisterLevelNodeType(string name, NodeDataClassName className)
        {
            if (!RegistedLevelNodeType.ContainsKey(name))
            {
                RegistedLevelNodeType.Add(name, className);
            }
        }

        protected override void OnKeyBoardEvent(Event e)
        {
            base.OnKeyBoardEvent(e);

            if(e.control)
            {                
                var position = editorWindow.position;
                float oldScale = Scale;
                if (e.keyCode == KeyCode.Equals)
                {
                    Scale = Mathf.Min(3f, Scale + wheelScale);
                    scrollPosition.x = (scrollPosition.x + position.width / 2) / oldScale * Scale - position.width / 2;
                    scrollPosition.y = (scrollPosition.y + position.height / 2) / oldScale * Scale - position.height / 2;
                }
                else if (e.keyCode == KeyCode.Minus)
                {
                    Scale = Mathf.Max(0.3f, Scale - wheelScale);
                    scrollPosition.x = (scrollPosition.x + position.width / 2) / oldScale * Scale - position.width / 2;
                    scrollPosition.y = (scrollPosition.y + position.height / 2) / oldScale * Scale - position.height / 2;
                }
                else if(e.keyCode==KeyCode.Z)
                {
                    LevelOperationStack.Instance.PopOperation();
                    return;
                }
            }
        }

        public override void DrawExtra()
        {
            base.DrawExtra();

            DrawSnapGrid();

            if (LevelEditor.state == LEVEL_EDITOR_STATE.simulation_mode)
                DrawTool.DrawLabel(new Rect(0, 0, 120, 30), "Simulation", BlueprintStyles.HeaderStyle, TextAnchor.MiddleCenter);
 
            if (simulatorEngine != null && simulatorEngine.IsPausing())
                DrawTool.DrawLabel(new Rect(120, 0, 120, 30), "Pause", BlueprintStyles.HeaderStyle, TextAnchor.MiddleCenter);
        }

        public float GetDelta()
        {
            return 10;
        }

        private void DrawSnapGrid()
        {
            if (!editorWindow.OpenSnap) return;

            float delta = editorWindow.SnapSize * Scale;

            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Vector2 start;
            Vector2 end;

            for (int i = (int)(scrollPosition.x / delta); i * delta < scrollPosition.x + editorWindow.position.width; ++i)
            {
                start = new Vector2(i * delta, scrollPosition.y + 0);
                end = new Vector2(i * delta, scrollPosition.y + editorWindow.position.height);
                Handles.DrawLine(start, end);
            }

            for(int i = (int)scrollPosition.y; i * delta < scrollPosition.y + editorWindow.position.height; ++i)
            {
                start = new Vector2(scrollPosition.x, i * delta);
                end = new Vector2(scrollPosition.x + editorWindow.position.width, i * delta);
                Handles.DrawLine(start, end);
            }

        }

        public void OnSceneViewEvent(Event e)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (Event.current.button == 0 && e.clickCount == 2)
                    {
                        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                        int layerMask = (1 << 9 | 1);
                        RaycastHit hitInfo;
                        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
                        {
                            LevelWaveNode node = selectNode as LevelWaveNode;
                            if (node != null)
                            {
                                node.AddMonsterAtPos(hitInfo.point + new Vector3(0, 0.5f, 0));
                            }
                            LevelRobotWaveNode robotNode = selectNode as LevelRobotWaveNode;
                            if (robotNode != null)
                            {
                                robotNode.AddRobotAtPos(hitInfo.point + new Vector3(0, 0.5f, 0));
                            }
                            LevelDoodadNode doodadNode = selectNode as LevelDoodadNode;
                            if (doodadNode != null)
                            {
                                doodadNode.AddDoodadAtPos(hitInfo.point + new Vector3(0, 0.5f, 0));
                            }
                        }
                    }
                    break;
                case EventType.Repaint:
                    {
                        foreach (BluePrintNode tmp in widgetList)
                        {
                            tmp.DrawGizmo();
                        }
                    }
                    break;
            }
        }

        protected override void OnMouseRightClicked(Event e)
        {
            var genericMenu = new GenericMenu();

            AddCommonNodeToRightClickMenu(genericMenu, e);

            foreach (KeyValuePair<string, RightClickParam> pair in RightClickList)
            {
                if (pair.Key == "AppointPos" && GraphID != 1)
                    continue;

                string nodeClassName = RegistedLevelNodeType[pair.Value.key].NodeName;
                string dataClassName = RegistedLevelNodeType[pair.Value.key].DataName;
                int subclass = pair.Value.param;
                string customType = pair.Value.customType;

                genericMenu.AddItem(
                    new GUIContent(pair.Key),
                    false,
                    (object o) => { CallTemplateFunc(nodeClassName, dataClassName, "AddLevelGraphNodeT", new object[] { o, subclass, customType }); },
                    e);
            }

            genericMenu.ShowAsContext();
        }

        public void AddLevelGraphNodeT<N, T>(Event e, int subclass, string cutomType)
            where T : BluePrintNodeBaseData, new()
            where N : LevelBaseNode<T>, new()
        {
            N node = new N();
            node.Init(this, e.mousePosition + scrollPosition);
            node.SetInternalParam(subclass, cutomType);
            node.PostInit();
            AddNode(node);
            node.HostData.NodeID = node.nodeEditorData.NodeID;
            graphConfigData.NodeConfigList.Add(node.nodeEditorData);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            GUILayout.BeginHorizontal();
            //GUILayout.Label("GraphID:" + graphConfigData.graphID);
            EditorGUILayout.LabelField("GraphID:", new GUILayoutOption[] { GUILayout.Width(150f) });
            EditorGUILayout.LabelField(graphConfigData.graphID.ToString(), new GUILayoutOption[] { GUILayout.Width(120f) });
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            //GUILayout.Label("Scene Name:" + SceneManager.GetActiveScene().name);
            EditorGUILayout.LabelField("Scene Name:", new GUILayoutOption[] { GUILayout.Width(150f) });
            //EditorGUILayout.LabelField(SceneManager.GetActiveScene().name, new GUILayoutOption[] { GUILayout.Width(120f) });
            string sceneName = (editorWindow as LevelEditor).unityScene;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(sceneName.Length > 23? sceneName.Substring(23) : "", new GUILayoutOption[] { GUILayout.Width(320f) });
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bind"))
            {
                (editorWindow as LevelEditor).BindCurrentEditorScene();
            }
            
            if (GUILayout.Button("Open"))
            {
                if(!string.IsNullOrEmpty(sceneName) && sceneName != SceneManager.GetActiveScene().path)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        (editorWindow as LevelEditor).loadScene = true;
                        EditorSceneManager.OpenScene(sceneName);
                    }
                }
                else
                {
                    (editorWindow as LevelEditor).CheckAndAddDynamicObject();
                }
            }
           
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            (editorWindow as LevelEditor).objGridRoot = 
                (GameObject)EditorGUILayout.ObjectField("物体节点",(editorWindow as LevelEditor).objGridRoot, typeof(GameObject),true, GUILayout.Width(250f));
            if(GUILayout.Button("LoadGrid"))
            {
                string path = EditorUtility.OpenFilePanel("select grid file", Application.dataPath + "/Scenes/Scenelib", "mapheight");
                if(!string.IsNullOrEmpty(path))
                {
                    (editorWindow as LevelEditor).LoadGrid(path);
                }
            }
            GUILayout.EndHorizontal();


            VarManager.DrawDataInspector();
        }

        protected override void OnCopy()
        {
            string cachedCopyPath = Application.dataPath + "/Editor Default Resources/Level/__cache_template.lvtpl";

            if (selectNodeList.Count > 0 || selectNode != null)
            {
                SaveTpl(cachedCopyPath);
            }
        }

        protected override void OnPaste()
        {
            string cachedCopyPath = Application.dataPath + "/Editor Default Resources/Level/__cache_template.lvtpl";
            if (File.Exists(cachedCopyPath))
            {
                LoadTpl(cachedCopyPath, Event.current.mousePosition);
            }
        }       

        #region simulation

        public void PrepareSimulation()
        {
            //AutoSave();
            UnSelectCurrentNode();
        }
 
        public override void OnEnterSimulation()
        {
            base.OnEnterSimulation();

            //ExstringManager.Clear();

            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedLevelNodeType)
            {
                CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "OnNodeEnterSimulationT");
            }
        }

        public override void OnEndSimulation()
        {
            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedLevelNodeType)
            {
                CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "OnNodeEndSimulationT");
            }
        }

        #endregion

        #region SaveLoad

        private void SaveLinkInfo(List<BluePrintWidget> widgets, LevelGraphData lvData)
        {
            for (int i = 0; i < widgets.Count; ++i)
            {
                BluePrintNode node = (BluePrintNode)widgets[i];

                for (int j = 0; j < node.pinList.Count; ++j)
                {
                    foreach (BlueprintConnection conn in node.pinList[j].connections)
                    {
                        BluePrintNode eNode = conn.connectEnd.GetNode<BluePrintNode>();

                        if (widgets.Contains(eNode))
                        {
                            LevelConnectionData connData = new LevelConnectionData();
                            connData.StartNode = node.nodeEditorData.NodeID;
                            connData.StartPin = node.pinList[j].pinID;
                            connData.EndNode = eNode.nodeEditorData.NodeID;
                            connData.EndPin = conn.connectEnd.pinID;
                            lvData.ConnectionData.Add(connData);
                        }
                    }
                }
            }
        }

        private void LoadLinkInfo(LevelGraphData lvData, Dictionary<int, int> IDMap)
        {
            for (int i = 0; i < lvData.ConnectionData.Count; ++i)
            {
                int sNodeID = lvData.ConnectionData[i].StartNode;
                if (IDMap != null && IDMap.ContainsKey(sNodeID)) sNodeID = IDMap[sNodeID];
                int sPinID = lvData.ConnectionData[i].StartPin;
                int eNodeID = lvData.ConnectionData[i].EndNode;
                if (IDMap != null && IDMap.ContainsKey(eNodeID)) eNodeID = IDMap[eNodeID];
                int ePinID = lvData.ConnectionData[i].EndPin;

                BluePrintNode sNode = GetNode(sNodeID);
                BluePrintNode eNode = GetNode(eNodeID);

                if (sNode != null && eNode != null)
                {
                    BluePrintPin sPin = sNode.GetPin(sPinID);
                    BluePrintPin ePin = eNode.GetPin(ePinID);

                    if (sPin != null && ePin != null)
                        sNode.ConnectPin(sPin, ePin);
                }
            }
        }

        public void SaveGraphToData(bool convenient=false)
        {
            graphData = new LevelGraphData();
            graphData.graphID = this.GraphID;
            graphData.name = this.GraphName;

            graphConfigData.ClearData();

            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedLevelNodeType)
            {
                CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "BuildDataByNodeT",new object[] { convenient});
            }

            SaveCommonNode(graphData);
            SaveLinkInfo(widgetList, graphData);

            SaveCommentInfo();
        }

        public void BuildDataByNodeT<N, T>(bool convenient)
            where T : BluePrintNodeBaseData, new()
            where N : LevelBaseNode<T>
        {
            foreach (BluePrintNode tmp in widgetList)
            {
                if (tmp is N)
                {
                    N node = tmp as N;
                    if (convenient)
                        node.ConvenientSave();
                    else
                        node.BeforeSave();
                    node.GetDataList(graphData).Add(node.HostData);
                    node.HostData.enabled = node.Enabled;
                }
                else
                {
                    var node = tmp as BluePrintNode;
                    var hostData = node.GetType().GetField("HostData").GetValue(node);
                    hostData.GetType().GetField("enabled").SetValue(hostData, node.Enabled);
                }
            }
        }

        public void LoadDataToGraph()
        {
            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedLevelNodeType)
            {
                CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "BuildNodeByDataT");
            }

            LoadCommonNode(graphData);
            LoadLinkInfo(graphData, null);

            LoadCommentInfo();
        }

        public void BuildNodeByDataT<N, T>()
            where T : BluePrintNodeBaseData, new()
            where N : LevelBaseNode<T>, new()
        {
            N template = new N();
            List<T> data = template.GetDataList(graphData);

            for (int i = 0; i < data.Count; ++i)
            {
                N node = new N();
                NodeConfigData eData = graphConfigData.GetConfigDataByID(data[i].NodeID);
                if(eData != null)
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

        public void QuickLink()
        {
            if (selectNodeList.Count <= 1)
                return;
            bool check = true;
            foreach(var node in selectNodeList)
            {
                int inputCount = 0, outputCount = 0;
                List<BluePrintPin> pinList = (node as BluePrintNode).pinList;
                foreach(var pin in pinList)
                {
                    if(pin.pinType==PinType.Main)
                    {
                        inputCount += pin.pinStream == PinStream.In ? 1 : 0;
                        outputCount += pin.pinStream == PinStream.Out ? 1 : 0;
                    }                    
                }
                if (inputCount > 1 || outputCount > 1) 
                {
                    check = false;
                    break;
                }
            }
            if(!check)
            {
                ShowNotification(new GUIContent("选中的点必须不大于1个主输入输出才能快速连接"), 3);
                return;
            }
            selectNodeList.Sort(SortNodeByX);
            List<BluePrintPin> linkPinList = new List<BluePrintPin>();
            List<BluePrintPin> breakPinList = new List<BluePrintPin>();
            for (var i = 0; i < selectNodeList.Count-1; i++)
            {
                BluePrintNode curNode = selectNodeList[i] as BluePrintNode;
                BluePrintNode nextNode = selectNodeList[i + 1] as BluePrintNode;
                var outputPin = curNode.pinList.Find(p => p.pinStream == PinStream.Out && p.pinType == PinType.Main);
                var inputPin = nextNode.pinList.Find(p => p.pinStream == PinStream.In && p.pinType == PinType.Main);
                if (inputPin == null || outputPin == null)
                    break;
                foreach(var rc in inputPin.reverseConnections)
                {
                    rc.reverseConnectEnd.RemoveConnection(inputPin);
                    breakPinList.Add(rc.reverseConnectEnd);
                    breakPinList.Add(inputPin);
                }
                inputPin.reverseConnections.Clear();
                foreach(var c in outputPin.connections)
                {
                    c.connectEnd.RemoveReverseConnection(outputPin);
                    breakPinList.Add(outputPin);
                    breakPinList.Add(c.connectEnd);
                }
                outputPin.connections.Clear();
                var con = curNode.ConnectPin(outputPin, inputPin);
                linkPinList.Add(outputPin);
                linkPinList.Add(inputPin);
            }
            LevelOperationStack.Instance.PushOperation(LevelOperationType.PinBreak, breakPinList, LevelEditor.Instance.CurrentGraph.GraphID);
            LevelOperationStack.Instance.PushOperation(LevelOperationType.PinConnect, linkPinList, LevelEditor.Instance.CurrentGraph.GraphID);
        }

        private int SortNodeByX(BluePrintWidget x, BluePrintWidget y)
        {
            return x.Bounds.x.CompareTo(y.Bounds.x);
        }


        public void SaveTpl(string path)
        {
            LevelGraphData tplData = new LevelGraphData();
            GraphConfigData tplConfigData = new GraphConfigData();

            List<BluePrintWidget> saveList = null;
            if (selectNodeList.Count > 0)
                saveList = selectNodeList;
            else if (selectNode != null)
            {
                saveList = new List<BluePrintWidget>();
                saveList.Add(selectNode);
            }
            else
                saveList = widgetList;

            if (saveList != null && saveList.Count > 0)
            {
                foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedLevelNodeType)
                {
                    CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "BuildTplDataByNodeT", new object[] { saveList, tplData, tplConfigData });
                }

                SaveCommonTpl(saveList, tplData, tplConfigData);
                SaveLinkInfo(saveList, tplData);
            }

            DataIO.SerializeData<LevelGraphData>(path, tplData);
            DataIO.SerializeData<GraphConfigData>(path.Replace(".lvtpl", ".elvtpl"), tplConfigData);
        }

        public void BuildTplDataByNodeT<N, T>(List<BluePrintWidget> list, LevelGraphData data, GraphConfigData tplConfigData)
            where T : BluePrintNodeBaseData, new()
            where N : LevelBaseNode<T>
        {
            foreach (BluePrintNode tmp in list)
            {
                if (tmp is N)
                {
                    N node = tmp as N;
                    node.BeforeSaveTpl(tplConfigData);
                    node.GetDataList(data).Add(node.HostData);
                }
            }
        }

        Dictionary<int, int> NodeIDMap = new Dictionary<int, int>();
        private Vector2 firstTplNodePos = Vector2.zero;

        public void LoadTpl(string path, Vector2 offset)
        {
            ClearMultiselect();
            NodeIDMap.Clear();

            LevelGraphData tplData = DataIO.DeserializeData<LevelGraphData>(path);
            GraphConfigData tplConfigData = DataIO.DeserializeData<GraphConfigData>(path.Replace(".lvtpl", ".elvtpl"));

            firstTplNodePos = Vector2.zero;
            foreach (KeyValuePair<string, NodeDataClassName> pair in RegistedLevelNodeType)
            {
                CallTemplateFunc(pair.Value.NodeName, pair.Value.DataName, "BuildNodeByTplDataT", new object[] { tplData , tplConfigData, NodeIDMap, offset});
            }

            LoadCommonTpl(tplData, tplConfigData, NodeIDMap, offset);
            LoadLinkInfo(tplData, NodeIDMap);
        }

        public void BuildNodeByTplDataT<N, T>(LevelGraphData data, GraphConfigData tplConfigData, Dictionary<int, int> IdMap, Vector2 offset)
            where T : BluePrintNodeBaseData, new()
            where N : LevelBaseNode<T>, new()
        {
            N template = new N();
            List<T> dataList = template.GetDataList(data);

            for (int i = 0; i < dataList.Count; ++i)
            {
                N node = new N();
                NodeConfigData eData = tplConfigData.GetConfigDataByID(dataList[i].NodeID);
                if (eData != null)
                {
                    NodeConfigData ncd = eData.Copy();
                    Vector2 tplPos = ncd.Position;
                    bool firstNode = Equals(Vector2.zero, firstTplNodePos);
                    ncd.Position = offset + (firstNode ? Vector2.zero : (ncd.Position - firstTplNodePos));
                    if(firstNode)
                    {
                        firstTplNodePos = tplPos;
                    }               
                    node.InitData(dataList[i], ncd);
                    node.Init(this, ncd.Position);
                    int tplID = node.nodeEditorData.NodeID;
                    node.nodeEditorData.NodeID = 0;
                    node.OnCopy(dataList[i]);
                    AddNode(node);
                    node.HostData.NodeID = node.nodeEditorData.NodeID;
                    IdMap.Add(tplID, node.nodeEditorData.NodeID);
                    node.AfterLoad();
                    AddToMultiSelect(node);
                }
            }
        }
        #endregion
    }

}


