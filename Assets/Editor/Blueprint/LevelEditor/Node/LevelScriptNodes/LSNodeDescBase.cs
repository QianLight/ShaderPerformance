using System;
using System.Collections.Generic;
using BluePrint;
using UnityEditor;
using UnityEngine;
using CFUtilPoolLib;
using System.Reflection;
using TableEditor;
using System.Text;
using System.Linq;
using CFClient.React;
using DataEditor;

namespace LevelEditor
{
    class LSNodeDescBase
    {
        static public LSNodeDescBase CreateNodeDesc(LevelScriptCmd cmd, LevelScriptNode n)
        {
            switch (cmd)
            {
                case LevelScriptCmd.Level_Cmd_Addbuff:
                    return new LSNodeDescAddbuff(n);
                case LevelScriptCmd.Level_Cmd_Removebuff:
                    return new LSNodeDescRemovebuff(n);
                case LevelScriptCmd.Level_Cmd_CheckBuff:
                    return new LSNodeDescCheckBuff(n);
                case LevelScriptCmd.Level_Cmd_KillAlly:
                    return new LSNodeDescKillAlly(n);
                case LevelScriptCmd.Level_Cmd_KillWave:
                    return new LSNodeDescKillWave(n);
                case LevelScriptCmd.Level_Cmd_Opendoor:
                    return new LSNodeDescOpendoor(n);
                case LevelScriptCmd.Level_Cmd_Cutscene:
                    return new LSNodeDescCutscene(n);
                //case LevelScriptCmd.Level_Cmd_Summon:
                //    return new LSNodeDescSummon(n);
                case LevelScriptCmd.Level_Cmd_KillAllSpawn:
                    return new LSNodeDescKillAllSpawn(n);
                case LevelScriptCmd.Level_Cmd_SendAICmd:
                    return new LSNodeDescSendAICmd(n);
                case LevelScriptCmd.Level_Cmd_SetExtString:
                    return new LSNodeDescSetExtString(n);
                case LevelScriptCmd.Level_Cmd_ClearExtString:
                    return new LSNodeDescClearExtString(n);
                case LevelScriptCmd.Level_Cmd_TransferLocation:
                    return new LSNodeDescTransferLocation(n);
                case LevelScriptCmd.Level_Cmd_Talk:
                    return new LSNodeDescTalk(n);
                case LevelScriptCmd.Level_Cmd_Notice:
                    return new LSNodeDescNotice(n);
                case LevelScriptCmd.Level_Cmd_StopNotice:
                    return new LSNodeDescStopNotice(n);
                case LevelScriptCmd.Level_Cmd_NpcPopSpeek:
                    return new LSNodeDescNpcPopSpeek(n);
                case LevelScriptCmd.Level_Cmd_Direction:
                    return new LSNodeDescDirection(n);
                case LevelScriptCmd.Level_Cmd_Bubble:
                    return new LSNodeDescBubble(n);
                case LevelScriptCmd.Level_Cmd_HideBillboard:
                    return new LSNodeDescHideBillboard(n);
                case LevelScriptCmd.Level_Cmd_ShowLevel:
                    return new LSNodeDescShowLevel(n);
                case LevelScriptCmd.Level_Cmd_ShowTarget:
                    return new LsNodeDescShowTarget(n);
                case LevelScriptCmd.Level_Cmd_ShowBossName:
                    return new LSNodeDescShowBossName(n);
                case LevelScriptCmd.Level_Cmd_StageGuide:
                    return new LSNodeDescStageGuide(n);
                case LevelScriptCmd.Level_Cmd_LevelName:
                    return new LSNodeDescLevelName(n);
                case LevelScriptCmd.Level_Cmd_ShowWay:
                    return new LSNodeDescShowWay(n);
                case LevelScriptCmd.Level_Cmd_Unlock:
                    return new LSNodeDescUnlock(n);
                case LevelScriptCmd.Level_Cmd_CameraControl:
                    return new LSNodeDescSoloCamera(n);
                case LevelScriptCmd.Level_Cmd_FreeCamera:
                    return new LSNodeDescFreeCamera(n);
                case LevelScriptCmd.Level_Cmd_React:
                    return new LSNodeReact(n);
                case LevelScriptCmd.Level_Cmd_RenderEnv:
                    return new LSNodeRenderEnv(n);
                case LevelScriptCmd.Level_Cmd_CacheMap:
                    return new LSNodeCacheMap(n);
                case LevelScriptCmd.Level_Cmd_Custom:
                    return new LSNodeCustomNode(n);
                case LevelScriptCmd.Level_Cmd_CallUI:
                    return new LSNodeDescCallUI(n);
                case LevelScriptCmd.Level_Cmd_AreaCamera:
                    return new LSNodeDescAreaCamera(n);
                case LevelScriptCmd.Level_Cmd_SetFx:
                    return new LSNodeDescSetFx(n);
                case LevelScriptCmd.Level_Cmd_PostTreatment:
                    return new LSNodeDescPostTreatment(n);
                case LevelScriptCmd.LeveL_Cmd_Action:
                    return new LSNodeDescAction(n);
                case LevelScriptCmd.Level_Cmd_Message:
                    return new LSNodeDescMessage(n);
                case LevelScriptCmd.Level_Cmd_SetAI:
                    return new LSNodeDescSetAI(n);
                case LevelScriptCmd.Level_Cmd_AutoChange:
                    return new LSNodeDescAutoChange(n);
                case LevelScriptCmd.Level_Cmd_ResetNode:
                    return new LSNodeDescReset(n);
                case LevelScriptCmd.Level_Cmd_Sneak:
                    return new LSNodeDescSneak(n);
                case LevelScriptCmd.Level_Cmd_CheckEntityBuff:
                    return new LSNodeDescCheckEntityBuff(n);
                case LevelScriptCmd.Level_Cmd_MonitorCheckBuff:
                    return new LSNodeDescMonitorCheckBuff(n);
                case LevelScriptCmd.Level_Cmd_SetEntityFx:
                    return new LSNodeDescSetEntityFx(n);
                case LevelScriptCmd.Level_Cmd_Weather:
                    return new LSNodeDescWeather(n);
                case LevelScriptCmd.Level_Cmd_ShareDamage:
                    return new LSNodeDescShareDamage(n);
            }

            return null;
        }

        protected LevelScriptNode node;
        public LSNodeDescBase(LevelScriptNode n) { node = n; }

        public virtual void DrawInspector() { }
        public virtual void InitPin() { }

        public virtual void UnInit() { }

        public virtual void BeforeSave()
        {
            TableData config;
            if (node.Cmd == LevelScriptCmd.Level_Cmd_Custom)
            {
               config = LevelHelper.nodeConfig.dataList.Find(d => d.valueList[1] == node.HostData.stringParam[0]);
            }
            else
            {
               config = LevelHelper.nodeConfig.dataList.Find(d => d.valueList[0] == node.HostData.Cmd.ToString());
            }
            if (config == null)
               return;
            if (config.valueList[2] == "1")//服务端节点
            {
               node.HostData.condition1 = 2;
               node.HostData.condition2 = 2;
               return;
            }
            if (!string.IsNullOrEmpty(config.valueList[3]))
               node.HostData.condition1 = GetCondition(config, 3);
            if (!string.IsNullOrEmpty(config.valueList[4]))
               node.HostData.condition2 = GetCondition(config, 4);
        }

        public virtual void ConvenientSave()
        {
            BeforeSave();
        }

        private uint GetCondition(TableData config,int index)
        {
            bool redo = true;
            switch (config.valueList[index])
            {
                case "0":
                    redo = false;
                    break;
                case "1":
                    redo = true;
                    break;
                default:
                    var conditions = config.valueList[index].Split('|');
                    foreach(var con in conditions)
                    {
                        var condition = con.Split('=');
                        switch (condition[0][0])
                        {
                            case 'f':
                                redo = node.HostData.valueParam[int.Parse(condition[0].Substring(1))] == float.Parse(condition[1]);
                                break;
                            case 's':
                                redo = !(node.HostData.stringParam[int.Parse(condition[0].Substring(1))] == condition[1]);
                                break;
                            case 'v':
                                break;
                        }
                        if (!redo)
                            break;
                    }
                    break;
            }
            return (uint)(redo ? 1 : 2);
        }

        public virtual void AfterLoad() { }

        public virtual void ErrorCheck() { }

        public virtual void SetCustomType(string customType) { }

        public virtual void DrawTipBox(Rect rect) { }

        public virtual void OnSelected() { }

        protected void EnsureDataLength(int lf, int ls, int lv)
        {
            if (node == null)
                return;
            int c = node.HostData.valueParam.Count;
            for (int i = c; i < lf; ++i)
                node.HostData.valueParam.Add(0);
            for (int i = c - 1; i >= lf; --i)
                node.HostData.valueParam.RemoveAt(i);

            c = node.HostData.stringParam.Count;
            for (int i = c; i < ls; ++i)
                node.HostData.stringParam.Add("");
            for (int i = c - 1; i >= ls; --i)
                node.HostData.stringParam.RemoveAt(i);

            c = node.HostData.vecParam.Count;
            for (int i = c; i < lv; ++i)
                node.HostData.vecParam.Add(LevelHelper.InValidVector);
            for (int i = c - 1; i >= lv; --i)
                node.HostData.vecParam.RemoveAt(i);

        }
    }

    class LSNodeDescAreaCamera : LSNodeDescBase
    {
        public LSNodeDescAreaCamera(LevelScriptNode n) : base(n) { EnsureDataLength(3, 0, 0); }

        public override void DrawInspector()
        {
            base.DrawInspector();

            node.HostData.valueParam[0] = EditorGUILayout.Toggle("开关", node.HostData.valueParam[0] == 1 ? true : false) ? 1 : 0;
            node.HostData.valueParam[1] = EditorGUILayout.FloatField("相机ID", node.HostData.valueParam[1], new GUILayoutOption[] { GUILayout.Width(275) });
        }

        public override void InitPin()
        {
            BluePrintPin pinIn = new BluePrintValuedPin(node, 1, "怪物ID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinIn);
        }

    }


    class LSNodeDescCallUI : LSNodeDescBase
    {
        public static string[] desc_f = new string[] { "UI Param" };
        public static string[] desc_s = new string[] { "UI Name", "UI String Param" };

        public LSNodeDescCallUI(LevelScriptNode n) : base(n) { EnsureDataLength(n==null?0:node.HostData.valueParam.Count, 2, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Call UI:");

            int fTemp = -1;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(desc_f[0], LevelGraph.TitleLayout);
            if(GUILayout.Button("Add",GUILayout.Width(50f)))
            {
                node.HostData.valueParam.Add(0);
            }
            EditorGUILayout.EndHorizontal();
            for (var i=0;i<node.HostData.valueParam.Count;i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("ValueParam{0}:", i), GUILayout.Width(100f));
                node.HostData.valueParam[i] = EditorGUILayout.FloatField(node.HostData.valueParam[i], GUILayout.Width(80f));
                if(GUILayout.Button("-", GUILayout.Width(50f)))
                {
                    fTemp = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (fTemp >= 0)
                node.HostData.valueParam.RemoveAt(fTemp);            

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(desc_s[0], LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(desc_s[1]);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            node.HostData.stringParam[1] = EditorGUILayout.TextField(node.HostData.stringParam[1], new GUILayoutOption[] { GUILayout.Width(250f), GUILayout.Height(100f) });
            EditorGUILayout.EndHorizontal();
        }

        public override void InitPin()
        {
            base.InitPin();
            BluePrintPin pinIn = new BluePrintValuedPin(node, 1, "Param", PinType.Data, PinStream.In, VariantType.Var_String);
            node.AddPin(pinIn);

            BluePrintPin pinIn1 = new BluePrintValuedPin(node, 2, "ValueParam", PinType.Data, PinStream.In, VariantType.Var_Float);
            node.AddPin(pinIn1);

            BluePrintPin pinPlayerID = new BluePrintValuedPin(node, 3, "PlayerID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinPlayerID);
        }
    }

    class LSNodeDescAddbuff : LSNodeDescBase
    {
        private EntityType eType = EntityType.Normal;

        public LSNodeDescAddbuff(LevelScriptNode n) : base(n) { EnsureDataLength(5, 1, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Add Buff:");
            EditorGUILayout.BeginHorizontal();
            eType = (EntityType)EditorGUILayout.EnumPopup("类型", eType, new GUILayoutOption[] { GUILayout.Width(270f) });
            node.HostData.valueParam[3] = (float)eType;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField( "怪物ID", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("怪物ID(长)", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BuffID", LevelGraph.TitleLayout);
            node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Buff等级", LevelGraph.TitleLayout);
            node.HostData.valueParam[2] = EditorGUILayout.FloatField(node.HostData.valueParam[2], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Caster", LevelGraph.TitleLayout);
            node.HostData.valueParam[4] = EditorGUILayout.FloatField(node.HostData.valueParam[4], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void InitPin()
        {
            base.InitPin();
            BluePrintPin pinPlayerID = new BluePrintValuedPin(node, 1, "", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinPlayerID);
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            eType = node.HostData.valueParam[3] == 0 ? EntityType.Normal : EntityType.Partner;
        }

        public override void DrawTipBox(Rect rect)
        {
            Rect tipRect = new Rect(rect.x + 40, rect.y + rect.height - 30, rect.width - 80, 10);
            DrawTool.DrawExpandableBox(tipRect, BlueprintStyles.AreaCommentStyle,string.Format("buffID:{0}",node.HostData.valueParam[1]) ,20);
        }

        public override void ErrorCheck()
        {
            base.ErrorCheck();
            if (node.HostData.valueParam[2] <= 0)
                node.nodeErrorInfo.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "buff等级不能为0", null));
        }
    }

    class LSNodeDescCheckBuff:LSNodeDescBase
    {
        private bool self = true;
        private bool last = true;
        private bool check = true;

        public LSNodeDescCheckBuff(LevelScriptNode n) : base(n) { EnsureDataLength(2, 3, 0); }

        public override void DrawInspector()
        {
            self = EditorGUILayout.Toggle("self", self, new GUILayoutOption[] { GUILayout.Width(270f) });
            node.HostData.stringParam[0] = self ? "on" : "off";

            node.HostData.valueParam[0] = EditorGUILayout.IntField("enemyID",(int)node.HostData.valueParam[0], new GUILayoutOption[] { GUILayout.Width(270f) });

            node.HostData.valueParam[1] = EditorGUILayout.IntField("buffID", (int)node.HostData.valueParam[1], new GUILayoutOption[] { GUILayout.Width(270f) });

            last = EditorGUILayout.Toggle("Last", last, new GUILayoutOption[] { GUILayout.Width(270f) });
            node.HostData.stringParam[1] = last ? "on" : "off";

            check = EditorGUILayout.Toggle("检查异常状态", check, new GUILayoutOption[] { GUILayout.Width(270f) });
            node.HostData.stringParam[2] = check ? "on" : "off";
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            self = node.HostData.stringParam[0] == "on" ? true : false;

            last = node.HostData.stringParam[1] == "on" ? true : false;

            check = node.HostData.stringParam[2] == "on" ? true : false;
        }

        public override void InitPin()
        {
            BluePrintPin pinOut = new BluePrintPin(node, 1, "False", PinType.Main, PinStream.Out);
            node.AddPin(pinOut);

            BluePrintValuedPin outPin = new BluePrintValuedPin(node, 2, "RoleID", PinType.Data, PinStream.Out,VariantType.Var_UINT64);
            node.AddPin(outPin);
        }
    }

    class LSNodeDescRemovebuff : LSNodeDescBase
    {
        private EntityType eType = EntityType.Normal;

        public LSNodeDescRemovebuff(LevelScriptNode n) : base(n) { EnsureDataLength(3, 1, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Remove Buff:");

            EditorGUILayout.BeginHorizontal();
            eType = (EntityType)EditorGUILayout.EnumPopup("类型", eType, new GUILayoutOption[] { GUILayout.Width(270f) });
            node.HostData.valueParam[2] = (float)eType;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("怪物ID", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("怪物ID(长)", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BuffID", LevelGraph.TitleLayout);
            node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void InitPin()
        {
            base.InitPin();
            BluePrintPin pinPlayerID = new BluePrintValuedPin(node, 1, "", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinPlayerID);
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            eType = node.HostData.valueParam[2] == 0 ? EntityType.Normal : EntityType.Partner;
        }

        public override void DrawTipBox(Rect rect)
        {
            Rect tipRect = new Rect(rect.x + 40, rect.y + rect.height - 30, rect.width - 80, 10);
            DrawTool.DrawExpandableBox(tipRect, BlueprintStyles.AreaCommentStyle, string.Format("buffID:{0}", node.HostData.valueParam[1]), 20);
        }

        public override void ErrorCheck()
        {
            base.ErrorCheck();
            if (node.HostData.valueParam[1] <= 0)
                node.nodeErrorInfo.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "buff等级不能为0", null));
        }
    }

    class LSNodeDescKillAlly : LSNodeDescBase
    {
        public LSNodeDescKillAlly(LevelScriptNode n) : base(n) { EnsureDataLength(1, 0, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Kill Ally:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Wave ID", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeDescKillWave : LSNodeDescBase
    {
        public LSNodeDescKillWave(LevelScriptNode n) : base(n) { EnsureDataLength(1, 0, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Kill Wave:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Wave ID", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeDescOpendoor : LSNodeDescBase
    {
        enum DoorState
        {
            On,
            Off,
        }

        public GameObject door;
        DoorState doorState;
        GameObject transferSrc;
        float TransferSrcRadius;
        GameObject transferDest;
        float TransferDestRadius;
        float sceneID = -1;
        bool bOne;

        public LSNodeDescOpendoor(LevelScriptNode n) : base(n) { }
        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Open Door:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Door", LevelGraph.TitleLayout);
            door = (GameObject)EditorGUILayout.ObjectField(door, typeof(GameObject), true, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("State", LevelGraph.TitleLayout);
            doorState = (DoorState)EditorGUILayout.EnumPopup(doorState, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("TransferSrc", LevelGraph.TitleLayout);
            transferSrc = (GameObject)EditorGUILayout.ObjectField(transferSrc, typeof(GameObject), true, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("TransferSrc Radius", LevelGraph.TitleLayout);
            TransferSrcRadius = EditorGUILayout.FloatField(TransferSrcRadius, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("TransferDest", LevelGraph.TitleLayout);
            transferDest = (GameObject)EditorGUILayout.ObjectField(transferDest, typeof(GameObject), true, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("TransferDest Radius", LevelGraph.TitleLayout);
            TransferDestRadius = EditorGUILayout.FloatField(TransferDestRadius, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scene ID", LevelGraph.TitleLayout);
            sceneID = EditorGUILayout.FloatField(sceneID, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("One?", LevelGraph.TitleLayout);
            bOne = EditorGUILayout.Toggle(bOne, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void DrawTipBox(Rect rect)
        {
            var color = BlueprintStyles.AreaCommentStyle.normal.textColor;
            BlueprintStyles.AreaCommentStyle.normal.textColor = doorState == DoorState.On ? Color.red : Color.green;
            Rect tipRect = new Rect(rect.x + 40, rect.y + rect.height - 30, rect.width - 80, 10);
            DrawTool.DrawExpandableBox(tipRect, BlueprintStyles.AreaCommentStyle, door == null ? string.Empty : door.name, 20);
            BlueprintStyles.AreaCommentStyle.normal.textColor = color;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if(door!=null)
            {
                Vector3 viewCenter = door.transform.position;
                Quaternion rotation = Quaternion.Euler(45, 0, 0);
                SceneView.lastActiveSceneView.LookAtDirect(viewCenter, rotation, 20);
                EditorGUIUtility.PingObject(door);
                Selection.activeGameObject = door;
            }
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            node.HostData.stringParam.Clear();
            node.HostData.vecParam.Clear();
            node.HostData.valueParam.Clear();

            if (door != null)
                node.HostData.stringParam.Add(door.name);
            else
                node.HostData.stringParam.Add("");

            node.HostData.stringParam.Add(doorState == DoorState.On ? "On" : "Off");

            if (transferSrc != null)
            {
                node.nodeEditorData.CustomData = transferSrc.name;

                Vector3 pos = transferSrc.transform.position;
                //float r = transferSrc.transform.rotation.eulerAngles.y;
                node.HostData.vecParam.Add(new Vector4(pos.x, pos.y, pos.z, TransferSrcRadius));
            }
            else
                node.HostData.vecParam.Add(LevelHelper.InValidVector);

            if (transferDest != null)
            {
                node.nodeEditorData.CustomData += ("#" + transferDest.name);

                Vector3 pos = transferDest.transform.position;
                //float r = transferDest.transform.rotation.eulerAngles.y;
                node.HostData.vecParam.Add(new Vector4(pos.x, pos.y, pos.z, TransferDestRadius));
            }
            else
                node.HostData.vecParam.Add(LevelHelper.InValidVector);

            node.HostData.valueParam.Add(sceneID);
            node.HostData.stringParam.Add((bOne ? "one" : "noone"));
        }

        public override void AfterLoad()
        {
            base.AfterLoad();

            string strDoorName = node.HostData.stringParam[0];
            string strDoorState = node.HostData.stringParam[1];

            string strTransferSrc = "";
            string strTransferDest = "";

            if (!string.IsNullOrEmpty(node.nodeEditorData.CustomData))
            {
                string[] s = node.nodeEditorData.CustomData.Split(new char[] { '#' });

                if (s.Length > 0) strTransferSrc = s[0];
                if (s.Length > 1) strTransferDest = s[1];
            }

            bOne = (node.HostData.stringParam[2] == "one" ? true : false);

            LevelEditor editor = node.Root.editorWindow as LevelEditor;
            if (editor.DynamicSceneRoot != null)
                door = BluePrintHelper.FindGameObject(editor.DynamicSceneRoot.transform, strDoorName);
            doorState = (strDoorState == "On" ? DoorState.On : DoorState.Off);
            if (editor.DynamicSceneRoot != null)
                if (!string.IsNullOrEmpty(strTransferSrc))
                    transferSrc = BluePrintHelper.FindGameObject(editor.DynamicSceneRoot.transform, strTransferSrc);
            TransferSrcRadius = node.HostData.vecParam.Count > 0 ? node.HostData.vecParam[0].w : 0;
            TransferDestRadius = node.HostData.vecParam.Count > 1 ? node.HostData.vecParam[1].w : 0;

            if (editor.DynamicSceneRoot != null)
                if (!string.IsNullOrEmpty(strTransferDest))
                    transferDest = BluePrintHelper.FindGameObject(editor.DynamicSceneRoot.transform, strTransferDest);

            sceneID = node.HostData.valueParam[0];
        }

        public override void ErrorCheck()
        {
            BlueprintNodeErrorInfo error = node.nodeErrorInfo;
            //error.nodeID = node.nodeEditorData.NodeID;

            if (door == null)
            {
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "Door不能为空", null));
            }

            if (door != null && door.name.Contains("#"))
            {
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "Door名字非法", null));
            }
        }
    }

    class LSNodeDescCutscene : LSNodeDescBase
    {
        bool bMp4 = false;
        bool bNeedAI = false;
        string cutscene;
        GameObject transferPos;
        float radius = -1;
        bool black = false;
        float fadeIn;
        Color fadeInColor;
        float blackTime;

        float fadeOut;
        Color fadeOutColor;
        float blackTimeOut;

        private LSNodeDescSoloCamera.camera_type cType;
        private string cameraSource;
        private bool setCamera;
        private bool waitForAnim;

        public LSNodeDescCutscene(LevelScriptNode n) : base(n) { }

        public override void InitPin()
        {
            BluePrintPin pinMonsterID = new BluePrintValuedPin(node, 1, "LookAtID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinMonsterID);

            BluePrintPin pinMonsterID2 = new BluePrintValuedPin(node, 2, "FollowID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinMonsterID2);
        }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("CutScene:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mp4", LevelGraph.TitleLayout);
            bMp4 = EditorGUILayout.Toggle(bMp4, LevelGraph.TitleLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (bMp4)
            {
                EditorGUILayout.LabelField("Mp4", LevelGraph.TitleLayout);
            }
            else
            {
                EditorGUILayout.LabelField("CutScene", LevelGraph.TitleLayout);
            }

            cutscene = EditorGUILayout.TextField(cutscene, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("NeedAI", LevelGraph.TitleLayout);
            bNeedAI = EditorGUILayout.Toggle(bNeedAI, LevelGraph.TitleLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Transfer To", LevelGraph.TitleLayout);
            EditorGUILayout.LabelField("使用TransferLocation节点传送", LevelGraph.ContentLayout);

            //transferPos = (GameObject)EditorGUILayout.ObjectField(transferPos, typeof(GameObject), true, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            if (transferPos != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("TransferInfo", LevelGraph.TitleLayout);

                EditorGUILayout.LabelField(transferPos.name, GUILayout.Width(100f));
                if (GUILayout.Button("Clear", GUILayout.Width(50f)))
                {
                    transferPos = null;
                }
                EditorGUILayout.EndHorizontal();
            }


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Radius", LevelGraph.TitleLayout);
            radius = EditorGUILayout.FloatField(radius, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("黑屏", LevelGraph.TitleLayout);
            //black = EditorGUILayout.Toggle(black, LevelGraph.ContentLayout);
            //EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("进入过渡时间", LevelGraph.TitleLayout);
            fadeIn = EditorGUILayout.FloatField(fadeIn, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("进入颜色", LevelGraph.TitleLayout);
            fadeInColor = EditorGUILayout.ColorField(fadeInColor, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("进入纯黑时间", LevelGraph.TitleLayout);
            blackTime = EditorGUILayout.FloatField(blackTime, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("结束过渡时间", LevelGraph.TitleLayout);
            fadeOut = EditorGUILayout.FloatField(fadeOut, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("结束颜色", LevelGraph.TitleLayout);
            fadeOutColor = EditorGUILayout.ColorField(fadeOutColor, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("结束纯黑时间", LevelGraph.TitleLayout);
            blackTimeOut = EditorGUILayout.FloatField(blackTimeOut, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("开启相机", LevelGraph.TitleLayout);
            setCamera = EditorGUILayout.Toggle(setCamera, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            
            if (setCamera)
            {
                EditorGUILayout.BeginHorizontal();
                cType = (LSNodeDescSoloCamera.camera_type)EditorGUILayout.EnumPopup(
                    "相机类型", (LSNodeDescSoloCamera.camera_type)cType, new GUILayoutOption[] { GUILayout.Width(270) });
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("相机资源", LevelGraph.TitleLayout);
                cameraSource = EditorGUILayout.TextField(cameraSource, LevelGraph.ContentLayout);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("等待角色动作", LevelGraph.TitleLayout);
            waitForAnim = EditorGUILayout.Toggle(waitForAnim, LevelGraph.TitleLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            node.HostData.stringParam.Clear();
            node.HostData.vecParam.Clear();
            node.HostData.valueParam.Clear();

            node.HostData.stringParam.Add(cutscene);

            if (transferPos != null)
            {
                node.nodeEditorData.CustomData = transferPos.name;

                Vector3 pos = transferPos.transform.position;
                float r = transferPos.transform.rotation.eulerAngles.y;
                node.HostData.vecParam.Add(new Vector4(pos.x, pos.y, pos.z, r));
            }
            else
            {
                node.HostData.vecParam.Add(LevelHelper.InValidVector);
                node.nodeEditorData.CustomData = string.Empty;
            }

            node.HostData.valueParam.Add(radius);
            node.HostData.valueParam.Add(bMp4 ? 1.0f : 0.0f);
            node.HostData.valueParam.Add(black ? 1.0f : 0.0f);
            node.HostData.valueParam.Add(fadeIn);
            node.HostData.valueParam.Add(blackTime);
            node.HostData.valueParam.Add(fadeOut);
            node.HostData.valueParam.Add(blackTimeOut);
            node.HostData.valueParam.Add(bNeedAI ? 1.0f : 0.0f);

            node.HostData.vecParam.Add(fadeInColor);
            node.HostData.vecParam.Add(fadeOutColor);

            node.HostData.stringParam.Add(setCamera ? "on" : "off");
            node.HostData.stringParam.Add(setCamera?cameraSource:string.Empty);
            node.HostData.valueParam.Add(setCamera?(float)cType:-1);
            node.HostData.valueParam.Add(waitForAnim ? 1 : 0);
        }

        public override void AfterLoad()
        {
            base.AfterLoad();

            cutscene = node.HostData.stringParam[0];

            if (!string.IsNullOrEmpty(node.nodeEditorData.CustomData))
            {
                LevelEditor editor = node.Root.editorWindow as LevelEditor;

                if (editor.DynamicSceneRoot != null)
                    transferPos = BluePrintHelper.FindGameObject(editor.DynamicSceneRoot.transform, node.nodeEditorData.CustomData);
            }

            radius = node.HostData.valueParam[0];
            bMp4 = node.HostData.valueParam.Count > 1 ? (node.HostData.valueParam[1] > 0 ? true : false) : false;
            black = node.HostData.valueParam.Count > 2 ? (node.HostData.valueParam[2] > 0 ? true : false) : false;
            fadeIn = node.HostData.valueParam.Count > 3 ? node.HostData.valueParam[3] : 0;
            blackTime = node.HostData.valueParam.Count > 4 ? node.HostData.valueParam[4] : 0;
            fadeOut = node.HostData.valueParam.Count > 5 ? node.HostData.valueParam[5] : 0;
            blackTimeOut = node.HostData.valueParam.Count > 6 ? node.HostData.valueParam[6] : 0;
            bNeedAI = node.HostData.valueParam.Count > 7 ? (node.HostData.valueParam[7] > 0 ? true : false) : false;

            fadeInColor = node.HostData.vecParam.Count > 1 ? (Color)node.HostData.vecParam[1] : Color.black;
            fadeOutColor = node.HostData.vecParam.Count > 2 ? (Color)node.HostData.vecParam[2] : Color.black;

            setCamera = node.HostData.stringParam.Count > 1 ? (node.HostData.stringParam[1] == "on" ? true : false) : false;
            cameraSource = node.HostData.stringParam.Count > 2 ? node.HostData.stringParam[2] : string.Empty;
            cType = (LSNodeDescSoloCamera.camera_type)(node.HostData.valueParam.Count > 8 ? node.HostData.valueParam[8] : 0);
            waitForAnim = node.HostData.valueParam.Count > 9 ? (node.HostData.valueParam[9] == 1 ? true : false) : false;
        }
    }

    class LSNodeDescKillAllSpawn : LSNodeDescBase
    {
        public LSNodeDescKillAllSpawn(LevelScriptNode n) : base(n) { }
    }


    class LSNodeDescSendAICmd : LSNodeDescBase
    {
        public LSNodeDescSendAICmd(LevelScriptNode n) : base(n) { EnsureDataLength(3, 2, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Send AI Command:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Monster ID", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("怪物ID(长)", LevelGraph.TitleLayout);
            node.HostData.stringParam[1] = EditorGUILayout.TextField(node.HostData.stringParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Wave ID", LevelGraph.TitleLayout);
            node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AI Command", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();


            bool bStopAI = node.HostData.valueParam[2] > 0 ? true : false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("停止AI", LevelGraph.TitleLayout);
            bStopAI = EditorGUILayout.Toggle(bStopAI, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
            node.HostData.valueParam[2] = bStopAI ? 1.0f : 0;
        }
    }

    class LSNodeDescSetExtString : LSNodeDescBase
    {
        public LSNodeDescSetExtString(LevelScriptNode n) : base(n) { EnsureDataLength(0, 1, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Set ExString:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ExString", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeDescClearExtString : LSNodeDescBase
    {
        public LSNodeDescClearExtString(LevelScriptNode n) : base(n) { EnsureDataLength(0, 1, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Clear ExString:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ExString", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeDescTransferLocation : LSNodeDescBase
    {
        GameObject transferPos;
        float monsterID = -1;

        public LSNodeDescTransferLocation(LevelScriptNode n) : base(n) { }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Transfer:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Transfer To", LevelGraph.TitleLayout);
            transferPos = (GameObject)EditorGUILayout.ObjectField(transferPos, typeof(GameObject), true, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MonsterID", LevelGraph.TitleLayout);
            monsterID = EditorGUILayout.FloatField(monsterID, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void InitPin()
        {
            base.InitPin();
            BluePrintPin pinPlayerID = new BluePrintValuedPin(node, 1, "PlayerID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinPlayerID);
        }

        public override void ConvenientSave()
        {
            base.BeforeSave();
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            node.HostData.stringParam.Clear();
            node.HostData.vecParam.Clear();
            node.HostData.valueParam.Clear();

            if (transferPos != null)
            {
                node.nodeEditorData.CustomData = transferPos.name;

                Vector3 pos = transferPos.transform.position;
                float r = transferPos.transform.rotation.eulerAngles.y;
                node.HostData.vecParam.Add(new Vector4(pos.x, pos.y, pos.z, r));
            }
            else
                node.HostData.vecParam.Add(LevelHelper.InValidVector);

            node.HostData.valueParam.Add(monsterID);
        }

        public override void AfterLoad()
        {
            base.AfterLoad();

            if (!string.IsNullOrEmpty(node.nodeEditorData.CustomData))
            {
                LevelEditor editor = node.Root.editorWindow as LevelEditor;
                if (editor.DynamicSceneRoot != null)
                    transferPos = BluePrintHelper.FindGameObject(editor.DynamicSceneRoot.transform, node.nodeEditorData.CustomData);
            }

            monsterID = node.HostData.valueParam[0];
        }
    }

    class LSNodeDescTalk : LSNodeDescBase
    {
        public LSNodeDescTalk(LevelScriptNode n) : base(n) { EnsureDataLength(1, 1, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Talk:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Talk", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UI", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextArea(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            var talkStr = HeadDialogReader.GetTalkContent((uint)node.HostData.valueParam[0]);
            if(talkStr!=null)
            {
                node.nodeEditorData.CustomNote = string.Concat(talkStr);
            }
        }
    }

    class LSNodeDescNotice : LSNodeDescBase
    {
        public LSNodeDescNotice(LevelScriptNode n) : base(n) { EnsureDataLength(1, 2, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Notice:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Duration", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Audio", LevelGraph.TitleLayout);
            node.HostData.stringParam[1] = EditorGUILayout.TextField(node.HostData.stringParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void InitPin()
        {
            base.InitPin();
            BluePrintPin pinPlayerID = new BluePrintValuedPin(node, 1, "PlayerID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinPlayerID);
        }

        public override void ErrorCheck()
        {
            if (node.HostData.valueParam[0] == 0)
            {
                BlueprintNodeErrorInfo error = node.nodeErrorInfo;
                //error.nodeID = node.nodeEditorData.NodeID;
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "Duration = 0", null));
            }
        }
    }

    class LSNodeDescStopNotice : LSNodeDescBase
    {
        public LSNodeDescStopNotice(LevelScriptNode n) : base(n) { }

        public override void InitPin()
        {
            base.InitPin();
            BluePrintPin pinPlayerID = new BluePrintValuedPin(node, 1, "PlayerID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinPlayerID);
        }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Stop Notice");
        }
    }

    class LSNodeDescNpcPopSpeek : LSNodeDescBase
    {
        public LSNodeDescNpcPopSpeek(LevelScriptNode n) : base(n) { }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("暂未实现");
        }
    }

    class LSNodeDescDirection : LSNodeDescBase
    {
        public LSNodeDescDirection(LevelScriptNode n) : base(n) { }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("暂未实现");
        }
    }

    class LSNodeDescBubble : LSNodeDescBase
    {
        public LSNodeDescBubble(LevelScriptNode n) : base(n) { EnsureDataLength(3, 3, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("Bubble:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("怪物ID", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("怪物ID(长)", LevelGraph.TitleLayout);
            node.HostData.stringParam[2] = EditorGUILayout.TextField(node.HostData.stringParam[2], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Duration", LevelGraph.TitleLayout);
            node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Audio", LevelGraph.TitleLayout);
            node.HostData.stringParam[1] = EditorGUILayout.TextField(node.HostData.stringParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("最大个数", LevelGraph.TitleLayout);
            node.HostData.valueParam[2] = EditorGUILayout.FloatField(node.HostData.valueParam[2], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void InitPin()
        {
            base.InitPin();
            BluePrintPin pinPlayerID = new BluePrintValuedPin(node, 1, "PlayerID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinPlayerID);
        }
    }

    class LSNodeDescHideBillboard : LSNodeDescBase
    {
        public LSNodeDescHideBillboard(LevelScriptNode n) : base(n) { EnsureDataLength(2, 1, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("HideBillboard:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("怪物ID", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("怪物ID(长)", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time", LevelGraph.TitleLayout);
            node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeDescShowLevel : LSNodeDescBase
    {
        public LSNodeDescShowLevel(LevelScriptNode n) : base(n) { EnsureDataLength(1, 2, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("ShowLevel:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MapName", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MapName(English)", LevelGraph.TitleLayout);
            node.HostData.stringParam[1] = EditorGUILayout.TextField(node.HostData.stringParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();


        }
    }

    class LSNodeDescShowBossName : LSNodeDescBase
    {
        public LSNodeDescShowBossName(LevelScriptNode n) : base(n) { EnsureDataLength(1, 5, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("ShowBossName:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BossName", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("EnglishName", LevelGraph.TitleLayout);
            node.HostData.stringParam[1] = EditorGUILayout.TextField(node.HostData.stringParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Trait", LevelGraph.TitleLayout);
            node.HostData.stringParam[2] = EditorGUILayout.TextField(node.HostData.stringParam[2], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Answer", LevelGraph.TitleLayout);
            node.HostData.stringParam[3] = EditorGUILayout.TextField(node.HostData.stringParam[3], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sign", LevelGraph.TitleLayout);
            node.HostData.stringParam[4] = EditorGUILayout.TextField(node.HostData.stringParam[4], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();


        }
    }

    class LSNodeDescStageGuide : LSNodeDescBase
    {
        public LSNodeDescStageGuide(LevelScriptNode n) : base(n) { EnsureDataLength(1, 0, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("ShowGuide:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("StageGuideID", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeDescLevelName : LSNodeDescBase
    {
        public LSNodeDescLevelName(LevelScriptNode n) : base(n) { EnsureDataLength(0, 2, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("BossName:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("章节名", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("关卡名", LevelGraph.TitleLayout);
            node.HostData.stringParam[1] = EditorGUILayout.TextField(node.HostData.stringParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeDescShowWay : LSNodeDescBase
    {
        GameObject WayPoint;
        string LevelProgress;

        float taskID;

        bool always;

        public LSNodeDescShowWay(LevelScriptNode n) : base(n) { EnsureDataLength(3, 1, 1); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("流程步骤:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("箭头指向", LevelGraph.TitleLayout);
            WayPoint = (GameObject)EditorGUILayout.ObjectField(WayPoint, typeof(GameObject), true, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("半径", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("描述", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("任务ID", LevelGraph.TitleLayout);
            node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("箭头时间", LevelGraph.TitleLayout);
            node.HostData.valueParam[2] = EditorGUILayout.FloatField(node.HostData.valueParam[2], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            if (WayPoint != null)
            {
                node.nodeEditorData.CustomData = WayPoint.name;

                Vector3 pos = WayPoint.transform.position;
                //float r = WayPoint.transform.rotation.eulerAngles.y;
                node.HostData.vecParam[0] = new Vector4(pos.x, pos.y, pos.z, 0);
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();

            if (!string.IsNullOrEmpty(node.nodeEditorData.CustomData))
            {
                LevelEditor editor = node.Root.editorWindow as LevelEditor;
                if (editor.DynamicSceneRoot != null)
                    WayPoint = BluePrintHelper.FindGameObject(editor.DynamicSceneRoot.transform, node.nodeEditorData.CustomData);
            }
        }

        public override void ErrorCheck()
        {
            if (WayPoint == null)
            {
                BlueprintNodeErrorInfo error = node.nodeErrorInfo;
                //error.nodeID = node.nodeEditorData.NodeID;
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "WayPoint = NULL", null));
            }
        }
    }

    class LsNodeDescShowTarget : LSNodeDescBase
    {
        bool bShow;
        GameObject WayPoint;

        public LsNodeDescShowTarget(LevelScriptNode n) : base(n) { EnsureDataLength(1, 3, 1); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("流程步骤:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target", LevelGraph.TitleLayout);
            WayPoint = (GameObject)EditorGUILayout.ObjectField(WayPoint, typeof(GameObject), true, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            bool bOpen = node.HostData.stringParam[0] == "On" ? true : false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("开启", LevelGraph.TitleLayout);
            bOpen = EditorGUILayout.Toggle(bOpen, LevelGraph.TitleLayout);
            EditorGUILayout.EndHorizontal();
            node.HostData.stringParam[0] = bOpen ? "On" : "Off";

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Atlas", LevelGraph.TitleLayout);
            node.HostData.stringParam[1] = EditorGUILayout.TextField(node.HostData.stringParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Icon", LevelGraph.TitleLayout);
            node.HostData.stringParam[2] = EditorGUILayout.TextField(node.HostData.stringParam[2], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Radius", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void ConvenientSave()
        {
            base.BeforeSave();
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            if (WayPoint != null)
            {
                node.nodeEditorData.CustomData = WayPoint.name;

                Vector3 pos = WayPoint.transform.position;
                //float r = WayPoint.transform.rotation.eulerAngles.y;
                node.HostData.vecParam[0] = new Vector4(pos.x, pos.y, pos.z, 0);
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();

            if (!string.IsNullOrEmpty(node.nodeEditorData.CustomData))
            {
                LevelEditor editor = node.Root.editorWindow as LevelEditor;
                if (editor.DynamicSceneRoot != null)
                    WayPoint = BluePrintHelper.FindGameObject(editor.DynamicSceneRoot.transform, node.nodeEditorData.CustomData);
            }
        }

        public override void ErrorCheck()
        {
            if (WayPoint == null)
            {
                BlueprintNodeErrorInfo error = node.nodeErrorInfo;
                //error.nodeID = node.nodeEditorData.NodeID;
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "WayPoint = NULL", null));
            }
        }
    }


    class LSNodeDescUnlock : LSNodeDescBase
    {
        public enum unlock_type
        {
            skill_slot,
            character_slot,
        }
        public LSNodeDescUnlock(LevelScriptNode n) : base(n) { EnsureDataLength(2, 0, 0); }

        public override void InitPin()
        {
            //node.HostData.valueParam.Add(0);
            //node.HostData.valueParam.Add(0);
        }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("解锁技能:");

            unlock_type t = (unlock_type)node.HostData.valueParam[0];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("解锁槽类型", LevelGraph.TitleLayout);
            t = (unlock_type)EditorGUILayout.EnumPopup(t, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
            node.HostData.valueParam[0] = (float)t;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("解锁槽位置", LevelGraph.TitleLayout);
            node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void ErrorCheck()
        {
            if (node.HostData.valueParam[1] == 0)
            {
                BlueprintNodeErrorInfo error = node.nodeErrorInfo;
                //error.nodeID = node.nodeEditorData.NodeID;
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "Param Invalid", null));
            }
        }
    }

    class LSNodeDescSoloCamera : LSNodeDescBase
    {
        public enum camera_type
        {
            solo_camera,
            area_camera,
            free_camera,
            track_camera,
            blend_camera,
            pvp_camera,
            level_camera,
            interrupt_camera,
        }

        public LSNodeDescSoloCamera(LevelScriptNode n) : base(n) { EnsureDataLength(10, 6, 0); }

        public override void InitPin()
        {
            BluePrintPin pinMonsterID = new BluePrintValuedPin(node, 1, "LookAtID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinMonsterID);

            BluePrintPin pinMonsterID2 = new BluePrintValuedPin(node, 2, "FollowID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinMonsterID2);

            BluePrintPin pinPlayerID = new BluePrintValuedPin(node, 3, "PlayerID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinPlayerID);
        }

        public override void DrawInspector()
        {
            camera_type t = (camera_type)node.HostData.valueParam[2];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("相机类型", LevelGraph.TitleLayout);
            t = (camera_type)EditorGUILayout.EnumPopup(t, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
            node.HostData.valueParam[2] = (float)t;

            bool bOpen = node.HostData.stringParam[1] == "On" ? true : false;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("开启相机", LevelGraph.TitleLayout);
            bOpen = EditorGUILayout.Toggle(bOpen, LevelGraph.TitleLayout);
            EditorGUILayout.EndHorizontal();

            node.HostData.stringParam[1] = bOpen ? "On" : "Off";

            if (bOpen)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("相机资源", LevelGraph.TitleLayout);
                node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("怪物ID", LevelGraph.TitleLayout);
                node.HostData.stringParam[5] = EditorGUILayout.TextField(node.HostData.stringParam[5], LevelGraph.ContentLayout);
                EditorGUILayout.EndHorizontal();

                if (t == camera_type.solo_camera || t == camera_type.free_camera
                    || t == camera_type.blend_camera || t == camera_type.track_camera
                    || t == camera_type.pvp_camera || t == camera_type.level_camera)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("拟合时间", LevelGraph.TitleLayout);
                    node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();

                    bool bNotReset = node.HostData.valueParam[3] > 0 ? true : false;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("相机不复位", LevelGraph.TitleLayout);
                    bNotReset = EditorGUILayout.Toggle(bNotReset, LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();
                    node.HostData.valueParam[3] = bNotReset ? 1.0f : 0;

                    if (bNotReset)
                    {
                        bool bResetHeightOnly = node.HostData.valueParam[6] > 0 ? true : false;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("仅高度复位", LevelGraph.TitleLayout);
                        bResetHeightOnly = EditorGUILayout.Toggle(bResetHeightOnly, LevelGraph.ContentLayout);
                        EditorGUILayout.EndHorizontal();
                        node.HostData.valueParam[6] = bResetHeightOnly ? 1.0f : 0;
                    }

                    bool bForbidUnlock = node.HostData.valueParam[4] > 0 ? true : false;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("禁止解锁", LevelGraph.TitleLayout);
                    bForbidUnlock = EditorGUILayout.Toggle(bForbidUnlock, LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();
                    node.HostData.valueParam[4] = bForbidUnlock ? 1.0f : 0;

                    if (t == camera_type.blend_camera)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Cam1Follow", LevelGraph.TitleLayout);
                        node.HostData.stringParam[3] = EditorGUILayout.TextField(node.HostData.stringParam[3], LevelGraph.ContentLayout);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Cam1LookAt", LevelGraph.TitleLayout);
                        node.HostData.stringParam[4] = EditorGUILayout.TextField(node.HostData.stringParam[4], LevelGraph.ContentLayout);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("FollowSelf", LevelGraph.TitleLayout);
                        bool self = node.HostData.valueParam[5] > 0 ? true : false;
                        self = EditorGUILayout.Toggle(self, LevelGraph.ContentLayout);
                        node.HostData.valueParam[5] = self ? 1 : 0;
                        EditorGUILayout.EndHorizontal();
                    }
                    else if (t == camera_type.track_camera)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Cam1Follow", LevelGraph.TitleLayout);
                        node.HostData.stringParam[3] = EditorGUILayout.TextField(node.HostData.stringParam[3], LevelGraph.ContentLayout);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Cam1LookAt", LevelGraph.TitleLayout);
                        node.HostData.stringParam[4] = EditorGUILayout.TextField(node.HostData.stringParam[4], LevelGraph.ContentLayout);
                        EditorGUILayout.EndHorizontal();
                    }

                }
                EditorGUILayout.BeginHorizontal();
                node.HostData.valueParam[7] = EditorGUILayout.IntField("临时镜头存在帧数", (int)node.HostData.valueParam[7], new GUILayoutOption[] { GUILayout.Width(270f) });
                EditorGUILayout.EndHorizontal();

                #region 临时相机删除时是否复位 占用valueParam 9,10 位
                if (node.HostData.valueParam[7] > 0)
                {
                    bool bTmpNotReset = node.HostData.valueParam[8] > 0 ? true : false;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("[临时]相机不复位", LevelGraph.TitleLayout);
                    bTmpNotReset = EditorGUILayout.Toggle(bTmpNotReset, LevelGraph.ContentLayout);
                    EditorGUILayout.EndHorizontal();
                    node.HostData.valueParam[8] = bTmpNotReset ? 1.0f : 0;

                    if (bTmpNotReset)
                    {
                        bool bTmpResetHeightOnly = node.HostData.valueParam[9] > 0 ? true : false;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("[临时]仅高度复位", LevelGraph.TitleLayout);
                        bTmpResetHeightOnly = EditorGUILayout.Toggle(bTmpResetHeightOnly, LevelGraph.ContentLayout);
                        EditorGUILayout.EndHorizontal();
                        node.HostData.valueParam[9] = bTmpResetHeightOnly ? 1.0f : 0;
                    }
                }
                #endregion
                
            }
        }

        public override void ErrorCheck()
        {
            if (node.HostData.stringParam[1] == "On")
            {
                //if (string.IsNullOrEmpty(node.HostData.stringParam[0]))
                //{
                //    BlueprintNodeErrorInfo error = node.nodeErrorInfo;
                //    //error.nodeID = node.nodeEditorData.NodeID;
                //    error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "Camera Asset Empty", null));
                //}
            }
        }

    }


    class LSNodeDescFreeCamera : LSNodeDescBase
    {
        public LSNodeDescFreeCamera(LevelScriptNode n) : base(n) { EnsureDataLength(2, 0, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("自由相机:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("拟合时间", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("角度", LevelGraph.TitleLayout);
            node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeReact : LSNodeDescBase
    {
        public LSNodeReact(LevelScriptNode n) : base(n) { EnsureDataLength(1, 1, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("播放动作:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID", LevelGraph.TitleLayout);
            node.HostData.valueParam[0] = EditorGUILayout.FloatField(node.HostData.valueParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("React脚本", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeRenderEnv : LSNodeDescBase
    {
        public LSNodeRenderEnv(LevelScriptNode n) : base(n) { EnsureDataLength(0, 1, 0); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("环境切换:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("天空盒路径", LevelGraph.TitleLayout);
            node.HostData.stringParam[0] = EditorGUILayout.TextField(node.HostData.stringParam[0], LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeCacheMap : LSNodeDescBase
    {
        GameObject CacheTarget;

        public LSNodeCacheMap(LevelScriptNode n) : base(n) { EnsureDataLength(0, 0, 1); }

        public override void DrawInspector()
        {
            EditorGUILayout.LabelField("缓存地形块:");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("地形块位置", LevelGraph.TitleLayout);
            CacheTarget = (GameObject)EditorGUILayout.ObjectField(CacheTarget, typeof(GameObject), true, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            if (CacheTarget != null)
            {
                node.nodeEditorData.CustomData = CacheTarget.name;

                Vector3 pos = CacheTarget.transform.position;
                node.HostData.vecParam[0] = new Vector4(pos.x, pos.y, pos.z, 0);
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();

            if (!string.IsNullOrEmpty(node.nodeEditorData.CustomData))
            {
                LevelEditor editor = node.Root.editorWindow as LevelEditor;
                CacheTarget = BluePrintHelper.FindGameObject(editor.DynamicSceneRoot.transform, node.nodeEditorData.CustomData);
            }
        }
    }

    class LSNodeCustomNode : LSNodeDescBase
    {
        LevelCustomNodeDefineData customDefine;
        GameObject[] VectorHandler;
        private Assembly assembly;

        public LSNodeCustomNode(LevelScriptNode n) : base(n) { }

        public override void SetCustomType(string customType)
        {
            customDefine = LevelHelper.GetCustomNodeDefineData(customType);
            if (customDefine == null) return;

            assembly = Assembly.Load("Assembly-CSharp-Editor");

            int lf = 0;
            int ls = 0;
            int lv = 0;

            for (int i = 0; i < customDefine.type.Count; ++i)
            {
                if (customDefine.type[i] == "float") lf++;
                if (customDefine.type[i] == "string") ls++;
                if (customDefine.type[i] == "vector") lv++;
                if (customDefine.type[i] == "bool") lf++;
                if (customDefine.type[i].Contains("enumflag")) lf++;
                if (customDefine.type[i].Contains("enumstring")) ls++;
            }

            EnsureDataLength(lf, ls + 1, lv);
            node.HostData.stringParam[0] = customType;
            VectorHandler = new GameObject[lv];
        }

        public override void InitPin()
        {
            if(!customDefine.useDefaultPin)
            {
                foreach(var pin in node.pinList)
                {
                    pin.OnDeleted();
                }
                node.pinList.Clear();
            }
            foreach(LevelCustomPinData pinData in customDefine.pinList)
            {
                BluePrintPin pin;
                if (pinData.pinType == PinType.Main)
                    pin = new BluePrintPin(node, pinData.pinID, pinData.pinName, pinData.pinType, pinData.pinStream);
                else
                {
                    pin = new BluePrintValuedPin(node, pinData.pinID, pinData.pinName, pinData.pinType, pinData.pinStream, pinData.pinDatatType,
                        pinData.pinStream == PinStream.In ? 0 : float.MinValue);
                }
                node.AddPin(pin);
            }
        }

        public override void DrawInspector()
        {
            int i_f = 0;
            int i_s = 1;
            int i_v = 0;

            for (int i = 0; i < customDefine.type.Count; ++i)
            {
                var layoutOption = customDefine.layout[i] == "short" ? LevelGraph.ContentLayout : LevelGraph.TextContentLayout;

                EditorGUILayout.BeginHorizontal();
                EditorGUITool.LabelField(customDefine.desc[i], LevelGraph.TitleLayoutW);

                if (customDefine.type[i] == "float")
                {
                    node.HostData.valueParam[i_f] = EditorGUILayout.FloatField(node.HostData.valueParam[i_f], layoutOption);
                    i_f++;
                }
                if (customDefine.type[i] == "bool")
                {
                    bool on = node.HostData.valueParam[i_f] == 1 ? true : false;
                    node.HostData.valueParam[i_f] = EditorGUILayout.Toggle(on, layoutOption) ? 1 : 0;
                    i_f++;
                }
                if (customDefine.type[i] == "string")
                {
                    node.HostData.stringParam[i_s] = EditorGUILayout.TextField(node.HostData.stringParam[i_s], layoutOption);
                    i_s++;
                }
                if (customDefine.type[i] == "vector")
                {
                    VectorHandler[i_v] = (GameObject)EditorGUILayout.ObjectField(VectorHandler[i_v], typeof(GameObject), true, layoutOption);
                    i_v++;
                }
                if (customDefine.type[i].Contains("enumflag"))
                {
                    string enumName = customDefine.type[i].Split('|')[1];
                    Type t = assembly.GetType(string.Format("LevelEditor.{0}", enumName));
                    MethodInfo mi = this.GetType().GetMethod("ForceConvert").MakeGenericMethod(new Type[] { t });
                    MethodInfo miToInt = this.GetType().GetMethod("ForceConvert").MakeGenericMethod(new Type[] { typeof(int) });
                    object value = mi.Invoke(this, new object[] { (int)node.HostData.valueParam[i_f] });
                    var e = EditorGUILayout.EnumFlagsField((Enum)value, LevelGraph.ContentLayout);
                    node.HostData.valueParam[i_f] = (int)miToInt.Invoke(this, new object[] { e });
                    i_f++;
                }
                if (customDefine.type[i].Contains("enumstring"))
                {
                    switch (customDefine.type[i].Split('|')[1])
                    {
                        case "State":
                            node.HostData.stringParam[i_s] =
                                (State)EditorGUILayout.EnumPopup(node.HostData.stringParam[i_s] == "on" ? State.On : State.Off, LevelGraph.ContentLayout)
                                == State.On ? "on" : "off";
                            break;
                        default:
                            break;
                    }
                    i_s++;
                }
                EditorGUILayout.EndHorizontal();
            }

        }

        public T ForceConvert<T>(T obj)
        {
            return obj;
        }

        public override void BeforeSave()
        {
            node.nodeEditorData.CustomData = "";
            for (int i = 0; i < VectorHandler.Length; ++i)
            {
                if (VectorHandler[i] != null)
                {
                    node.nodeEditorData.CustomData += ("#" + VectorHandler[i].name);

                    Vector3 pos = VectorHandler[i].transform.position;
                    float r = VectorHandler[i].transform.rotation.eulerAngles.y;
                    node.HostData.vecParam[i] = new Vector4(pos.x, pos.y, pos.z, r);
                }
            }

            if (node.nodeEditorData.CustomData.Length > 0)
            {
                node.nodeEditorData.CustomData = node.nodeEditorData.CustomData.Substring(1);
            }
        }

        public override void AfterLoad()
        {
            customDefine = LevelHelper.GetCustomNodeDefineData(node.HostData.stringParam[0]);

            if (!string.IsNullOrEmpty(node.nodeEditorData.CustomData))
            {
                string[] s = node.nodeEditorData.CustomData.Split(new char[] { '#' });

                VectorHandler = new GameObject[s.Length];

                for (int i = 0; i < s.Length; ++i)
                {
                    LevelEditor editor = node.Root.editorWindow as LevelEditor;

                    if (editor.DynamicSceneRoot != null)
                        VectorHandler[i] = BluePrintHelper.FindGameObject(editor.DynamicSceneRoot.transform, s[i]);
                }
            }
        }
    }

    class LSNodeDescSetFx : LSNodeDescBase
    {
        private GameObject obj;
        private State state;
        private bool modifyPos = false;

        public LSNodeDescSetFx(LevelScriptNode n) : base(n) { EnsureDataLength(0, 3, 1); }

        public override void DrawInspector()
        {
            base.DrawInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("物体"), LevelGraph.TitleLayout);
            obj = (GameObject)EditorGUILayout.ObjectField(obj, typeof(GameObject), true, LevelGraph.ContentLayout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            state = (State)EditorGUILayout.EnumPopup("State", state, new GUILayoutOption[] { GUILayout.Width(270) });
            EditorGUILayout.EndHorizontal();

            modifyPos = EditorGUILayout.Toggle("修改位置", modifyPos);
            if (modifyPos)
                node.HostData.vecParam[0] = EditorGUILayout.Vector3Field("更改后的位置", node.HostData.vecParam[0]);
        }

        public override void InitPin()
        {
            base.InitPin();
            BluePrintValuedPin pinIn = new BluePrintValuedPin(this.node, 1, "Pos", PinType.Data, PinStream.In, VariantType.Var_Vector3);
            node.AddPin(pinIn);
        }

        public override void AfterLoad()
        {
            base.AfterLoad();

            var editor = node.Root.editorWindow as LevelEditor;
            if (editor.DynamicSceneRoot != null)
                obj = BluePrintHelper.FindGameObject(editor.DynamicSceneRoot.transform, node.HostData.stringParam[0]);
            state = node.HostData.stringParam[1] == "On" ? State.On : State.Off;
            if (obj == null)
                return;
            Vector4 dummy = new Vector4(obj.transform.position.x, obj.transform.position.y, obj.transform.position.z, 0);
            modifyPos = dummy != node.HostData.vecParam[0];
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            node.HostData.vecParam.RemoveRange(1, node.HostData.vecParam.Count - 1);
            node.HostData.stringParam[0] = obj.name;
            node.HostData.stringParam[1] = state == State.On ? "On" : "Off";
            StringBuilder sb = new StringBuilder();
            for(var i=0;i<obj.transform.childCount;i++)
            {
                var fx = obj.transform.GetChild(i);
                var prefabType = PrefabUtility.GetPrefabAssetType(fx);
                if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Model)
                {
                    string fullPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(fx);
                    sb.Append(fullPath.Substring(17));//Assets/BundleRes/不计入路径中
                    sb.Append('|');
                    node.HostData.vecParam.Add(fx.transform.localPosition);
                }
            }
            if(!modifyPos)
                node.HostData.vecParam[0] = obj.transform.position;
            node.HostData.stringParam[2] = sb.ToString().TrimEnd('|');
        }
    }

    class LSNodeDescPostTreatment : LSNodeDescBase
    {
        private Transform root;
        private PostprogressPreviewMgr mgr;

        public LSNodeDescPostTreatment(LevelScriptNode n) : base(n) { EnsureDataLength(0, 1, 0); }

        public override void DrawInspector()
        {
            node.HostData.stringParam[0] = EditorGUILayout.TextField("数据路径", node.HostData.stringParam[0]);

            if (GUILayout.Button(new GUIContent("预览")))
            {
                if (root == null)
                    root = GameObject.Find("Main Camera").transform;
                if (!root.TryGetComponent<PostprogressPreviewMgr>(out mgr))
                    mgr=root.gameObject.AddComponent<PostprogressPreviewMgr>();
                mgr.Preview(node.HostData.stringParam[0]);
            }
            if(GUILayout.Button(new GUIContent("停止预览")))
            {
                mgr.Stop();
                GameObject.DestroyImmediate(mgr);
                mgr = null;
            }
        }
    }

    class LSNodeDescAction : LSNodeDescBase
    {
        public LSNodeDescAction(LevelScriptNode n) : base(n) { EnsureDataLength(1, 0, 0); }

        public override void DrawInspector()
        {
            base.DrawInspector();
            node.HostData.valueParam[0] = EditorGUILayout.FloatField("action", node.HostData.valueParam[0], new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }

    class LSNodeDescMessage : LSNodeDescBase
    {
        public LSNodeDescMessage(LevelScriptNode n) : base(n) { EnsureDataLength(2, 0, 0); }

        public override void InitPin()
        {
            BluePrintPin pinKey = new BluePrintValuedPin(node, 1, "Key", PinType.Data, PinStream.In, VariantType.Var_Float);
            node.AddPin(pinKey);

            BluePrintPin pinHp = new BluePrintValuedPin(node, 2, "Hp", PinType.Data, PinStream.In, VariantType.Var_Float);
            node.AddPin(pinHp);
        }
    }

    class LSNodeDescSetAI:LSNodeDescBase
    {
        enum VType
        {
            Float=0,
            Bool,
            Uint64=3,
            String,
            Int
        }

        public LSNodeDescSetAI(LevelScriptNode n) : base(n) { EnsureDataLength(3, 2, 0); }

        public override void DrawInspector()
        {
            base.DrawInspector();

            node.HostData.stringParam[0] = EditorGUILayout.TextField("name", node.HostData.stringParam[0], new GUILayoutOption[] { GUILayout.Width(270f) });
            node.HostData.valueParam[0] = (int)(VType)EditorGUILayout.EnumPopup("数据类型",(VType)node.HostData.valueParam[0], 
                new GUILayoutOption[] { GUILayout.Width(270f) });
            node.HostData.valueParam[1]= EditorGUILayout.FloatField("templateID", node.HostData.valueParam[1], new GUILayoutOption[] { GUILayout.Width(270f) });
            if ((VType)node.HostData.valueParam[0]==VType.String)
            {
                node.HostData.stringParam[1] = EditorGUILayout.TextField("value", node.HostData.stringParam[1], new GUILayoutOption[] { GUILayout.Width(270f) });
            }
            else
            {
                node.HostData.valueParam[2] = EditorGUILayout.FloatField("value", node.HostData.valueParam[2], new GUILayoutOption[] { GUILayout.Width(270f) });
            }            
        }
    }

    class LSNodeDescAutoChange:LSNodeDescBase
    {
        private string[] changeType = new string[] { "Add", "Reduce" };

        private List<string> vName = new List<string> { };
        private List<BluePrintVariant> vDefine=new List<BluePrintVariant>();

        public LSNodeDescAutoChange(LevelScriptNode n) : base(n) { EnsureDataLength(1, n==null?0:n.HostData.stringParam.Count, 0); }

        public override void DrawInspector()
        {
            base.DrawInspector();
            if (LevelEditor.Instance.CurrentGraph == null)
                return;
            vName.Clear();
            var gID = LevelEditor.Instance.CurrentGraph.GraphID;
            vDefine.Clear();
            foreach (var v in LevelEditor.Instance.CurrentGraph.VarManager.UserVariant)
            {
                vName.Add(string.Format("子图{0}:{1}", gID, v.VariableName));
                vDefine.Add(v);
            }
            if(gID!=1)
            {
                var globalV = BlueprintGraphVariantManager.GlobalVariants;
                foreach(var v in globalV)
                {
                    vName.Add(string.Format("子图{0}:{1}",1, v.VariableName));
                    vDefine.Add(v);
                }
            }            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Variant List:",GUILayout.Width(80f));
            if (GUILayout.Button("+", GUILayout.Width(20f)))
            {
                if(vName.Count<=node.HostData.stringParam.Count)
                {
                    Debug.LogError("AutoChange节点添加变量数量不能超过子图和主图已定义变量数量");
                    return;
                }
                node.HostData.stringParam.Add(vDefine[0].VariableName);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ChangeType:", GUILayout.Width(80f));
            node.HostData.valueParam[0] = EditorGUILayout.Popup((int)node.HostData.valueParam[0], changeType,GUILayout.Width(80f));
            EditorGUILayout.EndHorizontal();
            if (vName.Count <= 0)
                return;
            int temp = -1;
            for(var i=0;i<node.HostData.stringParam.Count;i++)
            {
                var index = vDefine.FindIndex(v => v.VariableName.Equals(node.HostData.stringParam[i]));
                if (index < 0)
                {
                    temp = i;
                    Debug.LogError(string.Format("关卡变量：{0}不存在 已自动移除", node.HostData.stringParam[i]));
                    break;
                }
                EditorGUILayout.BeginHorizontal();                
                node.HostData.stringParam[i] = vDefine[EditorGUILayout.Popup(index, vName.ToArray(),GUILayout.Width(150f))].VariableName;
                if (GUILayout.Button("-", GUILayout.Width(50f)))
                    temp = i;
                EditorGUILayout.EndHorizontal();
            }
            if (temp >= 0 && temp <= node.HostData.stringParam.Count)
                node.HostData.stringParam.RemoveAt(temp);
        }

        public override void ErrorCheck()
        {
            base.ErrorCheck();
            var vDefine = node.Root.VarManager.UserVariant;
            for (var i = 0; i < node.HostData.stringParam.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var index = vDefine.FindIndex(v => v.VariableName.Equals(node.HostData.stringParam[i]));
                if(index<0)
                    index = BlueprintGraphVariantManager.GlobalVariants.FindIndex(v => v.VariableName.Equals(node.HostData.stringParam[i]));
                if (index < 0)
                {
                    node.nodeErrorInfo.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, 
                        string.Format("引用的关卡变量不存在：{0}",node.HostData.stringParam[i]), null));
                }
            }
        }
    }

    class LSNodeDescReset:LSNodeDescBase
    {
        public LSNodeDescReset(LevelScriptNode n) : base(n) { EnsureDataLength(n==null?0:n.HostData.valueParam.Count, 0, 0); }

        public override void DrawInspector()
        {
            base.DrawInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("重置节点列表", GUILayout.Width(80f));
            if(GUILayout.Button("Add",GUILayout.Width(50f)))
            {
                node.HostData.valueParam.Add(0);
            }
            EditorGUILayout.EndHorizontal();

            int temp = -1;
            for(var i=0;i<node.HostData.valueParam.Count;i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("NodeID:", GUILayout.Width(75f));
                node.HostData.valueParam[i] = EditorGUILayout.FloatField(node.HostData.valueParam[i], GUILayout.Width(85f));
                if(GUILayout.Button("-",GUILayout.Width(50f)))
                {
                    temp = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (temp >= 0)
                node.HostData.valueParam.RemoveAt(temp);
        }

        public override void ErrorCheck()
        {
            base.ErrorCheck();
            var graph = LevelEditor.Instance.CurrentGraph;
            for(var i=0;i<node.HostData.valueParam.Count;i++)
            {
                if (graph.GetNode((int)node.HostData.valueParam[i]) == null)
                    node.nodeErrorInfo.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, 
                        string.Format("引用的节点:{0}不存在",node.HostData.valueParam[i]), null));
            }
        }
    }

    class LSNodeDescSneak:LSNodeDescBase
    {
        public LSNodeDescSneak(LevelScriptNode n) : base(n) {EnsureDataLength( 1, 0, 0); }

        public override void DrawInspector()
        {
            base.DrawInspector();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("On",GUILayout.Width(50f));
            node.HostData.valueParam[0] = EditorGUILayout.Toggle(node.HostData.valueParam[0] == 1 ? true : false, GUILayout.Width(50f))?1:0;
            EditorGUILayout.EndHorizontal();
        }
    }

    class LSNodeDescCheckEntityBuff:LSNodeDescBase
    {
        public LSNodeDescCheckEntityBuff(LevelScriptNode n) : base(n) { EnsureDataLength(6, 0, 0); }


        public override void DrawInspector()
        {
            base.DrawInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CheckType", GUILayout.Width(80f));
            node.HostData.valueParam[0] = (uint)(CheckBuffType)EditorGUILayout.EnumPopup((CheckBuffType)node.HostData.valueParam[0], GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();
            CheckBuffType checkType = (CheckBuffType)node.HostData.valueParam[0];

            if ( checkType== CheckBuffType.Monster)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("EnemyID", GUILayout.Width(80f));
                node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], GUILayout.Width(100f));
                EditorGUILayout.EndHorizontal();
            }

            if(checkType==CheckBuffType.Monster||checkType==CheckBuffType.MultiPlayer)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Exist", GUILayout.Width(80f));
                node.HostData.valueParam[2] = EditorGUILayout.Toggle(node.HostData.valueParam[2] == 1 ? true : false, GUILayout.Width(50f)) ? 1 : 0;
                EditorGUILayout.EndHorizontal();
            }

            if(node.HostData.valueParam[2]==1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Single", GUILayout.Width(80f));
                node.HostData.valueParam[3] = EditorGUILayout.Toggle(node.HostData.valueParam[3] == 1 ? true : false, GUILayout.Width(50f)) ? 1 : 0;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BuffID", GUILayout.Width(80f));
            node.HostData.valueParam[4] = EditorGUILayout.FloatField(node.HostData.valueParam[4], GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("检查异常状态", GUILayout.Width(80f));
            node.HostData.valueParam[5] = EditorGUILayout.Toggle(node.HostData.valueParam[5] == 1 ? true : false, GUILayout.Width(50f)) ? 1 : 0;
            EditorGUILayout.EndHorizontal();
        }

        public override void InitPin()
        {
            base.InitPin();

            BluePrintPin pinOut = new BluePrintPin(node, 1, "False", PinType.Main, PinStream.Out);
            node.AddPin(pinOut);

            BluePrintPin dataPinOut = new BluePrintPin(node, 2, "RoleID", PinType.Data, PinStream.Out);
            node.AddPin(dataPinOut);
        }
    }

    class LSNodeDescMonitorCheckBuff:LSNodeDescBase
    {
        public LSNodeDescMonitorCheckBuff(LevelScriptNode n) : base(n) { EnsureDataLength(6, 0, 0); }

        public override void DrawInspector()
        {
            base.DrawInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CheckType", GUILayout.Width(80f));
            node.HostData.valueParam[0] = (uint)(CheckBuffType)EditorGUILayout.EnumPopup((CheckBuffType)node.HostData.valueParam[0], GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();
            CheckBuffType checkType = (CheckBuffType)node.HostData.valueParam[0];

            if (checkType == CheckBuffType.Monster)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("EnemyID", GUILayout.Width(80f));
                node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], GUILayout.Width(100f));
                EditorGUILayout.EndHorizontal();
            }

            if (checkType == CheckBuffType.Monster || checkType == CheckBuffType.MultiPlayer)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Exist", GUILayout.Width(80f));
                node.HostData.valueParam[2] = EditorGUILayout.Toggle(node.HostData.valueParam[2] == 1 ? true : false, GUILayout.Width(50f)) ? 1 : 0;
                EditorGUILayout.EndHorizontal();
            }

            if (node.HostData.valueParam[2] == 1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Single", GUILayout.Width(80f));
                node.HostData.valueParam[3] = EditorGUILayout.Toggle(node.HostData.valueParam[3] == 1 ? true : false, GUILayout.Width(50f)) ? 1 : 0;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BuffID", GUILayout.Width(80f));
            node.HostData.valueParam[4] = EditorGUILayout.FloatField(node.HostData.valueParam[4], GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("检查异常状态", GUILayout.Width(80f));
            node.HostData.valueParam[5] = EditorGUILayout.Toggle(node.HostData.valueParam[5] == 1 ? true : false, GUILayout.Width(50f)) ? 1 : 0;
            EditorGUILayout.EndHorizontal();
        }

        public override void InitPin()
        {
            base.InitPin();

            BluePrintPin dataPinOut = new BluePrintPin(node, 1, "RoleID", PinType.Data, PinStream.Out);
            node.AddPin(dataPinOut);
        }
    }

    class LSNodeDescSetEntityFx:LSNodeDescBase
    {
        public LSNodeDescSetEntityFx(LevelScriptNode n) : base(n) { EnsureDataLength(3, 0, 0); }

        public override void DrawInspector()
        {
            base.DrawInspector();

            //预留一个数据以后拓展给自己或者其他玩家挂特效
            node.HostData.valueParam[0] = 0;
            EditorGUILayout.LabelField("EntityType:怪物");

            switch((ApplyEntityType)node.HostData.valueParam[0])
            {
                case ApplyEntityType.Monster:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("MonsterID:", GUILayout.Width(70f));
                    node.HostData.valueParam[1] = EditorGUILayout.FloatField(node.HostData.valueParam[1], GUILayout.Width(100f));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("FXID:", GUILayout.Width(70f));
                    node.HostData.valueParam[2] = EditorGUILayout.FloatField(node.HostData.valueParam[2], GUILayout.Width(100f));
                    EditorGUILayout.EndHorizontal();
                    break;
            }
        }
    }

    class LSNodeDescWeather:LSNodeDescBase
    {
        private RainEnvShow rainController;

        public LSNodeDescWeather(LevelScriptNode n) : base(n) { EnsureDataLength(1, 1, 0); }

        public override void DrawInspector()
        {
            base.DrawInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("天气开关", GUILayout.Width(75f));
            node.HostData.valueParam[0] = EditorGUILayout.Toggle(node.HostData.valueParam[0] == 1 ? true : false, GUILayout.Width(80f)) ? 1 : 0;
            EditorGUILayout.EndHorizontal();

            node.HostData.stringParam[0] = EditorGUILayout.TextField("数据路径", node.HostData.stringParam[0]);

            if (GUILayout.Button(new GUIContent("预览")))
            {
                if(rainController==null)
                {
                    var prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Test/WeatherTest/RainControl.prefab", typeof(GameObject));
                    var obj = GameObject.Instantiate(prefab);
                    rainController = obj.GetComponent<RainEnvShow>();
                    rainController.m_light = GameObject.Find("EditorScene/Light/MainScene/mainLight").GetComponent<Light>();
                }
                DataBase data = new WeatherData();
                DataEditor.DataEditor.ReadData(ref data, "Assets/BundleRes/CommonData/Weather/"+node.HostData.stringParam[0]+".txt");
                var weatherData = data as WeatherData;
                if (weatherData != null)
                {
                    RainEnvManager.RainParams param = default;
                    param.useTime = weatherData.useTime;
                    param.useLighting = weatherData.useLighting;
                    param.DarkValueInRain = weatherData.darkValueInRain;
                    param.RainRoughness = weatherData.rainRoughness;
                    param.rainPrefabName = weatherData.rainPrefab;
                    param.RippleIntensity = weatherData.rippleIntensity;
                    param.RippleSpeed = weatherData.rippleSpeed;
                    param.RippleTilling = weatherData.rippleTilling;
                    param.isRain = true;
                    param.NormalTSScale = weatherData.normalTSScale;
                    rainController.SetRainParams(param);
                }
            }
            if(GUILayout.Button("停止预览"))
            {
                GameObject.DestroyImmediate(rainController.gameObject);
                rainController = null;
            }
        }

        public override void UnInit()
        {
            base.UnInit();
            if(rainController!=null)
            {
                GameObject.DestroyImmediate(rainController.gameObject);
                rainController = null;
            }
        }
    }

    class LSNodeDescShareDamage:LSNodeDescBase
    {
        public LSNodeDescShareDamage(LevelScriptNode n) : base(n) { EnsureDataLength(5, 2, 0); }

        enum ShareType
        {
            Circle=0,
            Rect=1
        }

        enum OriginPointType
        {
            Player=0,
            Monster=1
        }


        public override void DrawInspector()
        {
            base.DrawInspector();

            node.HostData.valueParam[0] = (float)(ShareType)EditorGUILayout.EnumPopup("ShareType",(ShareType)node.HostData.valueParam[0]);
            node.HostData.valueParam[2] = EditorGUILayout.FloatField("TargetNum", node.HostData.valueParam[2]);
            node.HostData.valueParam[3] = EditorGUILayout.FloatField("Time", node.HostData.valueParam[3]);
            node.HostData.valueParam[4] = (float)(OriginPointType)EditorGUILayout.EnumPopup("OriginPoint", (OriginPointType)node.HostData.valueParam[4]);
            node.HostData.stringParam[1] = EditorGUILayout.TextField("MonsterID", node.HostData.stringParam[1]);

            switch((ShareType)node.HostData.valueParam[0])
            {
                case ShareType.Circle:
                    node.HostData.valueParam[1] = EditorGUILayout.FloatField("Range", node.HostData.valueParam[1]);
                    break;
                case ShareType.Rect:
                    node.HostData.stringParam[0] = EditorGUILayout.TextField("RectRange", node.HostData.stringParam[0]);
                    break;
            }
        }

        public override void InitPin()
        {
            base.InitPin();

            BluePrintPin pinIn = new BluePrintValuedPin(node, 1, "PlayerID", PinType.Data, PinStream.In,VariantType.Var_UINT64);
            node.AddPin(pinIn);

            BluePrintPin pinIn2 = new BluePrintValuedPin(node, 2, "Time", PinType.Data, PinStream.In, VariantType.Var_Float);
            node.AddPin(pinIn2);
        }
    }

}
