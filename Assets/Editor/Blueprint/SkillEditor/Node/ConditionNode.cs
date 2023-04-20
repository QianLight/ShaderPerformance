using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class ConditionNode : TimeTriggerNode<XConditionData>
    {
        public override bool BranchNode => true;
        public override bool OneConnectPinOut { get { return true; } }

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);
            HeaderImage = "BluePrint/Header8";

            BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
            BluePrintPin pinOut1 = new BluePrintPin(this, -2, "False", PinType.Main, PinStream.Out);
            BluePrintPin pinOut2 = new BluePrintPin(this, -3, "True", PinType.Main, PinStream.Out, 20);
            AddPin(pinIn);
            AddPin(pinOut1);
            AddPin(pinOut2);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            DrawConditionParam(HosterData, "not node");
        }

        public override void BuildDataByPin()
        {
            GetHosterData<XBaseData>().TransferData.Clear();
            if (pinList[1].connections.Count != 0)
            {
                AddPinData<XBaseData>(pinList[1].connections[0].connectEnd.GetNode<BaseSkillNode>().GetHosterData<XBaseData>());
            }
            else
            {
                AddPinData<XBaseData>(new XBaseData() { Index = -1 });
            }

            if (pinList[2].connections.Count != 0)
            {
                AddPinData<XBaseData>(pinList[2].connections[0].connectEnd.GetNode<BaseSkillNode>().GetHosterData<XBaseData>());
            }
            else
            {
                AddPinData<XBaseData>(new XBaseData() { Index = -1 });
            }
        }

        public override void BuildPinByData(Dictionary<int, BaseSkillNode> IndexToNodeDic)
        {
            if (GetHosterData<XBaseData>().TransferData.Count > 0 && GetHosterData<XBaseData>().TransferData[0].Index != -1)
            {
                BluePrintPin startPin = pinList[1];
                BluePrintPin endPin = IndexToNodeDic[GetHosterData<XBaseData>().TransferData[0].Index].pinList[0];
                ConnectPin(startPin, endPin);
            }
            if (GetHosterData<XBaseData>().TransferData.Count > 1 && GetHosterData<XBaseData>().TransferData[1].Index != -1)
            {
                BluePrintPin startPin = pinList[2];
                BluePrintPin endPin = IndexToNodeDic[GetHosterData<XBaseData>().TransferData[1].Index].pinList[0];
                ConnectPin(startPin, endPin);
            }
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            int count = GetRoot.GetConfigData<EcsData.XSkillData>() != null ? GetRoot.GetConfigData<EcsData.XSkillData>().ConditionData.Count : GetRoot.GetConfigData<EcsData.XHitData>().ConditionData.Count;
            count += GetRoot.GetConfigData<EcsData.XSkillData>() != null ? GetRoot.GetConfigData<EcsData.XSkillData>().MultiConditionData.Count : GetRoot.GetConfigData<EcsData.XHitData>().MultiConditionData.Count;

            if (count > EditorEcs.Xuthus_VirtualServer.COND_MAX)
            {
                LogError("Condition  个数最大上限(" + EditorEcs.Xuthus_VirtualServer.COND_MAX + ")！！！" + GetRoot.DataPath);
                return false;
            }

            return true;
        }

        public override bool IgnoreMultiIn => false;
    }
}