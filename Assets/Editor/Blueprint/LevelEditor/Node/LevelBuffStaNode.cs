using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using CFUtilPoolLib;
using XEditor;

namespace LevelEditor
{
    class LevelBuffStaNode:LevelBaseNode<LevelBuffStaData>
    {
        enum StaType
        {
            Amount,
            Character,
            Monster
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeEditorData.Tag = "BuffSta";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinOut = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinOut);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.staType = (uint)(StaType)EditorGUILayout.EnumPopup("统计类型", (StaType)HostData.staType,
               new GUILayoutOption[] { GUILayout.Width(270f) });
            HostData.buffID = (uint)EditorGUILayout.FloatField("BuffID", HostData.buffID);
        }

        public override void DrawTipBox(Rect boxRect)
        {
            base.DrawTipBox(boxRect);
            Rect tipRect = new Rect(boxRect.x + 40, boxRect.y + boxRect.height - 30, boxRect.width - 80, 10);
            DrawTool.DrawExpandableBox(tipRect, BlueprintStyles.AreaCommentStyle, string.Format("buffID:{0}", HostData.buffID), 20);
        }

        public override List<LevelBuffStaData> GetDataList(LevelGraphData data)
        {
            return data.BuffStaData;
        }
    }
}
