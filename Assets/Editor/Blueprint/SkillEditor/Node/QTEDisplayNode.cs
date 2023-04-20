using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using System.IO;

namespace EditorNode
{
    public class QTEDisplayNode : TimeTriggerNode<XQTEDisplayData>
    {
        private BaseSkillNode qteNode = null;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header9";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            GetNodeByIndex<QTENode>(ref qteNode, ref HosterData.QTEIndex, true);
        }

        public override void BuildDataFinish()
        {
            base.BuildDataFinish();

            GetNodeByIndex<QTENode>(ref qteNode, ref HosterData.QTEIndex);
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            if (HosterData.QTEIndex == -1)
            {
                LogError("QTEDisplayNode_" + HosterData.Index + ",  QTEIndex≈‰÷√¥ÌŒÛ£°£°£°");
                return false;
            }

            return true;
        }
    }
}