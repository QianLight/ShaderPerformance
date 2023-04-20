using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public abstract class EventTriggerNode<N> : BaseSkillNode where N : XBaseData, new()
    {
        public N HosterData = new N();

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);

            if (AutoDefaultMainPin)
            {
                BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
                BluePrintPin pinOut = new BluePrintPin(this, -2, "Out", PinType.Main, PinStream.Out);
                AddPin(pinIn);
                AddPin(pinOut);
            }

            string tag = typeof(N).ToString();
            tag = tag.Remove(0, tag.LastIndexOf('.') + 2).Replace("Data", "");
            nodeEditorData.Tag = tag;
            if (HosterData.Index != -1) nodeEditorData.Tag += "_" + HosterData.Index.ToString();
            if (string.IsNullOrEmpty(nodeEditorData.TitleName)) nodeEditorData.TitleName = tag;
        }

        public override void InitData<T>(T data, NodeConfigData configData)
        {
            HosterData = data as N;
            HosterData.TimeBased = false;
            nodeEditorData = configData;
        }

        public override T GetHosterData<T>()
        {
            return HosterData as T;
        }

        public override void CalcTriggerTime()
        {
            base.CalcTriggerTime();

            DFSTriggerTime(this);
        }

        public bool LoopRootCheck(BaseSkillNode node, ref HashSet<int> loopSet)
        {
            if (node is LoopNode)
            {
                loopSet.Add(node.GetHosterData<XBaseData>().Index);
                return true;
            }

            bool flag = false;
            foreach (BlueprintReverseConnection connection in node.pinList[0].reverseConnections)
            {
                bool f = LoopRootCheck(connection.reverseConnectEnd.GetNode<BaseSkillNode>(), ref loopSet);
                if (connection.reverseConnectEnd.GetNode<BaseSkillNode>() is LoopNode)
                {
                    foreach (BlueprintConnection tmp in connection.reverseConnectEnd.GetNode<BaseSkillNode>().pinList[2].connections)
                    {
                        if (tmp.connectEnd.GetNode<BaseSkillNode>() == node)
                        {
                            f = false;
                            break;
                        }
                    }
                }
                flag |= f;
            }

            return flag;
        }
    }
}