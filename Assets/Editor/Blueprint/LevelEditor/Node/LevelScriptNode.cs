using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    public enum LevelScriptCmd
    {
        Level_Cmd_Invalid = 0,

        Level_Cmd_Talk,
        Level_Cmd_Notice,
        Level_Cmd_StopNotice,
        Level_Cmd_NpcPopSpeek,
        Level_Cmd_Direction,
        Level_Cmd_Bubble,
        Level_Cmd_HideBillboard,
        Level_Cmd_ShowLevel,
        Level_Cmd_ShowBossName,
        Level_Cmd_StageGuide,
        Level_Cmd_LevelName,
        Level_Cmd_ShowWay,
        Level_Cmd_ShowTarget,
        Level_Cmd_Unlock,
        Level_Cmd_CameraControl,
        Level_Cmd_FreeCamera,
        Level_Cmd_React,
        Level_Cmd_RenderEnv,
        Level_Cmd_CacheMap,
        Level_Cmd_Custom,
        Level_Cmd_CallUI,
        //Level_Cmd_HideBattleUI,
        //Level_Cmd_Mission,

        Level_Cmd_Addbuff,
        Level_Cmd_Removebuff,
        Level_Cmd_CheckBuff,
        //Level_Cmd_KillSpawn,
        Level_Cmd_KillAlly,
        Level_Cmd_KillWave,
        Level_Cmd_Opendoor,
        Level_Cmd_Cutscene,
        Level_Cmd_Summon,
        Level_Cmd_KillAllSpawn,
        Level_Cmd_SendAICmd,
        Level_Cmd_SetExtString,
        Level_Cmd_ClearExtString,
        Level_Cmd_TransferLocation,
        Level_Cmd_AreaCamera,
        Level_Cmd_SetFx,
        Level_Cmd_PostTreatment,
        LeveL_Cmd_Action,
        Level_Cmd_Message,
        Level_Cmd_SetAI,
        Level_Cmd_SetData,
        Level_Cmd_AutoChange,
        Level_Cmd_ResetNode,
        Level_Cmd_Sneak,
        Level_Cmd_CheckEntityBuff,
        Level_Cmd_MonitorCheckBuff,
        Level_Cmd_SetEntityFx,
        Level_Cmd_Weather,
        Level_Cmd_ShareDamage,
        Level_Cmd_Max
    }


    class LevelScriptNode : LevelBaseNode<LevelScriptData>
    {
        public LevelScriptCmd Cmd;
        //public string CustomType;
        public LSNodeDescBase NodeDesc;
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);
            nodeWidth = 150f;
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;
        }

        public override void SetInternalParam(int interParam, string customType)
        {
            Cmd = (LevelScriptCmd)interParam;
            HostData.Cmd = Cmd;
           
            NodeDesc = LSNodeDescBase.CreateNodeDesc(Cmd, this);

            if (NodeDesc == null)
            {
                Debug.Log("NodeDesc = null : Cmd = " + Cmd.ToString());
            }

            NodeDesc.SetCustomType(customType);
            nodeEditorData.Tag = string.IsNullOrEmpty(customType) ? Cmd.ToString().Substring(10) : customType; 
            NodeDesc.InitPin();

        }

        public override List<LevelScriptData> GetDataList(LevelGraphData data)
        {
            return data.ScriptData;
        }

        public override void BeforeSave()
        {
            base.BeforeSave();
            NodeDesc.BeforeSave();
        }

        public override void ConvenientSave()
        {
            HostData.NodeID = nodeEditorData.NodeID;
            Root.graphConfigData.NodeConfigList.Add(nodeEditorData);
            NodeDesc.ConvenientSave();
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            Cmd = HostData.Cmd;
            NodeDesc = LSNodeDescBase.CreateNodeDesc(Cmd, this);
            if (NodeDesc == null)
            {
                Debug.Log("NodeDesc = null : Cmd = " + Cmd.ToString());
            }

            if(Cmd == LevelScriptCmd.Level_Cmd_Custom)
                NodeDesc.SetCustomType(HostData.stringParam[0]);

            nodeEditorData.Tag = Cmd != LevelScriptCmd.Level_Cmd_Custom ? Cmd.ToString().Substring(10) : HostData.stringParam[0];
            nodeEditorData.BackgroundText = "";

            NodeDesc.InitPin();
            NodeDesc.AfterLoad();
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            NodeDesc.DrawInspector();
        }

        public override void DrawTipBox(Rect boxRect)
        {
            base.DrawTipBox(boxRect);
            NodeDesc.DrawTipBox(boxRect);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            NodeDesc.OnSelected();
        }

        //public void GetWallInfo(List<LevelWallData> info)
        //{
        //    if (Cmd == LevelScriptCmd.Level_Cmd_Opendoor)
        //    {
        //        GameObject go = (NodeDesc as LSNodeDescOpendoor).door;

        //        if (go == null)
        //        {
        //            Debug.Log("Node " + nodeEditorData.NodeID + ": Door = null");
        //            return;
        //        }
        //        if (IsSpecialWall(go)) return;

        //        if(go.GetComponent<XDummyWall>() == null) return;

        //        LevelWallData data = LevelHelper.GetWallColliderData(go, WallType.normal);
        //        if (data != null)
        //            info.Add(data);
        //    }
        //}

        private bool IsSpecialWall(GameObject wall)
        {
            if (wall == null) return false;
            Transform t = wall.transform;
             while (t != null)
            {
                if (t.name == "bosswallroot" || t.name == "playerwallroot")
                {
                    return true;
                }
                t = t.parent;
            }
 
            return false;
        }

        public override void CheckError()
        {
            base.CheckError();
            NodeDesc.ErrorCheck();
        }

        public override void UnInit()
        {
            base.UnInit();
            NodeDesc.UnInit();
        }
    }
}
