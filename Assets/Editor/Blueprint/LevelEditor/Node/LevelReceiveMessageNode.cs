using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelReceiveMessageNode:LevelBaseNode<LevelReceiveMessageData>
    {

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);
            nodeEditorData.Tag = "ReceiveMessage";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinOut1 = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinOut1);
            BluePrintPin pinOut2 = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinOut2);
            BluePrintPin pinOut3 = new BluePrintValuedPin(this, 3, "", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinOut3);
            BluePrintPin pinOut4 = new BluePrintValuedPin(this, 4, "", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinOut4);
        }

        public override void DrawTipBox(Rect boxRect)
        {
            base.DrawTipBox(boxRect);
            Rect tipRect = new Rect(boxRect.x + 50, boxRect.y + boxRect.height - 80, boxRect.width - 80, 10);
            DrawTool.DrawExpandableBox(tipRect, BlueprintStyles.AreaCommentStyle, HostData.message, 20);
        }

        public override List<LevelReceiveMessageData> GetDataList(LevelGraphData data)
        {
            return data.receiveMessageData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("消息名", GUILayout.Width(80f));
            HostData.message = EditorGUILayout.TextField(HostData.message, GUILayout.Width(150f));
            EditorGUILayout.EndHorizontal();
        }
    }
}
