using EcsData;
using UnityEditor;
using UnityEngine;
using BluePrint;
using System.Collections.Generic;

namespace EditorNode
{
    public class LoopNode : TimeTriggerNode<XLoopData>
    {
        public override bool OneConnectPinOut { get { return true; } }

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);
            HeaderImage = "BluePrint/Header8";

            BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
            BluePrintPin pinOut1 = new BluePrintPin(this, -2, "Body", PinType.Main, PinStream.Out);
            BluePrintPin pinOut2 = new BluePrintPin(this, -3, "Out", PinType.Main, PinStream.Out, 20);
            AddPin(pinIn);
            AddPin(pinOut1);
            AddPin(pinOut2);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HosterData.Count = EditorGUITool.IntField("Count (Max:32) : ", HosterData.Count);
            if (HosterData.Count < 0) HosterData.Count = 0;
            if (HosterData.Count > 32) HosterData.Count = 32;
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
            if (pinList[1].connections.Count == 0) return false;

            foreach (BlueprintConnection connection in pinList[1].connections)
            {
                if (!LoopChildCheck(connection.connectEnd.GetNode<BaseSkillNode>())) return false;
            }

            return true;
        }

        private bool LoopChildCheck(BaseSkillNode node)
        {
            if (node is ContinueNode || node is BreakNode)
            {
                if (node.GetHosterData<XBaseData>().TransferData.Count == 0)
                    node.GetHosterData<XBaseData>().TransferData.Add(new XTransferData() { Index = GetHosterData<XBaseData>().Index });
                return true;
            }
            if (node is EndNode || node is ScriptTransNode)
                return true;
            if (node is LoopNode)
            {
                LogError("LoopNode_" + HosterData.Index + " 不能相互嵌套！！！");
                return false;
            }
            if (node.pinList.Count <= 1)
            {
                LogError("LoopNode_" + HosterData.Index + " 的 Body内需要以Continue,Break,End,ScriptTrans,为分支终点！！！");
                return false;
            }

            for (int i = 1; i < node.pinList.Count; ++i)
            {
                if (node.pinList[i].connections.Count == 0)
                {
                    LogError("Loop_" + HosterData.Index + " 内需要以Continue,Break,End,为分支终点！！！");
                    return false;
                }
                if (node.pinList[i].connections.Count > 1)
                {
                    LogError("Loop_" + HosterData.Index + " 内单个引脚只能拉出一根线！！！");
                    return false;
                }
                if (!LoopChildCheck(node.pinList[i].connections[0].connectEnd.GetNode<BaseSkillNode>()))
                    return false;
            }

            return true;
        }

        public override bool IgnoreMultiIn => false;
    }
}