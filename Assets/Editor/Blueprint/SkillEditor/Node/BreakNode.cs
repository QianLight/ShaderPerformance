using EcsData;
using UnityEngine;
using BluePrint;
using System.Collections.Generic;

namespace EditorNode
{
    public class BreakNode : EventTriggerNode<XBreakData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);

            HeaderImage = "BluePrint/Header3";

            if (AutoDefaultMainPin)
            {
                BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
                AddPin(pinIn);
            }
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;
            HashSet<int> loopSet = new HashSet<int>();

            if (!LoopRootCheck(this, ref loopSet))
            {
                LogError("BreakNode_" + HosterData.Index + " 只能出现在LoopBody内!!!");
                return false;
            }

            if (loopSet.Count > 1)
            {
                LogError("BreakNode_" + HosterData.Index + " 不能出现在多个Loop内!!!");
                return false;
            }

            return true;
        }

        public override void BuildPinByData(Dictionary<int, BaseSkillNode> IndexToNodeDic)
        {

        }

        public override bool IgnoreMultiIn => false;
    }
}
