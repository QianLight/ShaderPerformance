using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelTriggerNode:LevelBaseNode<LevelTriggerData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeEditorData.Tag = "Trigger";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinIn = new BluePrintPin(this, -1, "", PinType.Main, PinStream.In, 20);
            BluePrintPin pinOut = new BluePrintPin(this, -2, "true", PinType.Main, PinStream.Out, 20);
            AddPin(pinIn);
            AddPin(pinOut);

            var InPin = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Bool);
            AddPin(InPin);

            var outPin = new BluePrintPin(this, 2, "false", PinType.Main, PinStream.Out, 20);
            AddPin(outPin);

            var outDataPin = new BluePrintValuedPin(this, 3, "LeftTime", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(outDataPin);

            var inPin1 = new BluePrintValuedPin(this, 4, "param", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(inPin1);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.interval = EditorGUILayout.FloatField("Interval", HostData.interval, new GUILayoutOption[] { GUILayout.Width(270f) });
            HostData.totalTime = EditorGUILayout.FloatField("TotalTime", HostData.totalTime, new GUILayoutOption[] { GUILayout.Width(270f) });
            HostData.excuteImmediatly = EditorGUILayout.Toggle("ExcuteImmidialy", HostData.excuteImmediatly, GUILayout.Width(200f));
        }

        public override List<LevelTriggerData> GetDataList(LevelGraphData data)
        {
            return data.TriggerData;
        }
    }
}
