using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public enum ControllerNodeType
    {
        ControllerNode_None = 0,
        ControllerNode_Start,
        ControllerNode_Branch,
        ControllerNode_And,
        ControllerNode_Or,
        ControllerNode_Equal,
        ControllerNode_Great,
        ControllerNode_Less,
        ControllerNode_GreatEqual,
        ControllerNode_LessEqual,
        ControllerNode_For,
        ControllerNode_While,
        ControllerNode_Arithmetic,
        FlowNode_And,
        FlowNode_Or,
        ControllerNode_SubStart,
        ControllerNode_SubEnd,
        ControllerNode_Fly,
        ControllerNode_LoopEnd
    }

    class ControllerBaseNode : BluePrintBaseDataNode<BluePrintControllerData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeWidth = 150f;
            HostData.TypeID = GetContorllerType();
        }

        public override List<BluePrintControllerData> GetCommonDataList(BluePrintData data)
        {
            return data.ControllerData;
        }

        public virtual int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_None;
        }

       public override void Draw()
        {
            adjustedBounds = Bounds.Scale(Scale);
            adjustedBounds.width = Math.Max(nodeWidth * Scale, DrawTool.CalculateTextSize(nodeEditorData.TitleName, BlueprintStyles.HeaderStyle).x + 40 * Scale);
            Bounds.width = adjustedBounds.width / Scale;

            var boxRect = adjustedBounds;
            boxRect.height = (GetPinRectHeight() + 20) * Scale ;
            var titleRect = new Rect(boxRect.x + 50 * Scale, boxRect.y, 50 * Scale , boxRect.height / 2 + 6 * Scale);
            var pinRect = Bounds;
            pinRect.y += 10 * Scale;

            if(!string.IsNullOrEmpty(nodeEditorData.Tag))
                DrawTag(adjustedBounds);

            DrawBackground(boxRect);

            if(!string.IsNullOrEmpty(nodeEditorData.BackgroundText))
                DrawTitle(titleRect.Scale(1/Scale), nodeEditorData.BackgroundText);

            DrawPin(pinRect);
            DrawTipBox(boxRect);

            nodeEditorData.Position = Bounds.position;
        }

        #region simulation
        private CommonRTControllerNode controllerData;

        public override void OnEnterSimulation()
        {
            base.OnEnterSimulation();
            controllerData = RuntimeData as CommonRTControllerNode;
        }

        public override void OnEndSimulation()
        {
            controllerData = null;
            base.OnEndSimulation();
        }



        #endregion

        public static ControllerBaseNode CreateControllerNode(int ControllerTypeID)
        {
            ControllerNodeType t = (ControllerNodeType)ControllerTypeID;

            switch (t)
            {
                case ControllerNodeType.ControllerNode_Start:
                    return new ControllerStartNode();
                case ControllerNodeType.ControllerNode_Branch:
                    return new ControllerBranchNode();
                 case ControllerNodeType.ControllerNode_And:
                    return new ControllerOpAnd();
                case ControllerNodeType.ControllerNode_Or:
                    return new ControllerOpOr();
                case ControllerNodeType.ControllerNode_Equal:
                    return new ControllerOpEqual();
                case ControllerNodeType.ControllerNode_Great:
                    return new ControllerOpGreat();
                case ControllerNodeType.ControllerNode_Less:
                    return new ControllerOpLess();
                case ControllerNodeType.ControllerNode_GreatEqual:
                    return new ControllerOpGreatEqual();
                case ControllerNodeType.ControllerNode_LessEqual:
                    return new ControllerOpLessEqual();
                case ControllerNodeType.ControllerNode_Arithmetic:
                    return new ControllerOpArithmeticNode();
                case ControllerNodeType.ControllerNode_For:
                case ControllerNodeType.ControllerNode_While:
                    return new ControllerWhileNode();
                case ControllerNodeType.FlowNode_And:
                    return new FlowAndNode();
                case ControllerNodeType.FlowNode_Or:
                    return new FlowOrNode();
                case ControllerNodeType.ControllerNode_SubStart:
                    return new ControllerSubGraphStartNode();
                case ControllerNodeType.ControllerNode_SubEnd:
                    return new ControllerSubGraphEndNode();
                case ControllerNodeType.ControllerNode_Fly:
                    return new ControllerFlyNode();
                case ControllerNodeType.ControllerNode_LoopEnd:
                    return new ControllerLoopEndNode();
            }

            return null;
        }


    }
}
