using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelMonitorValueNode:LevelBaseNode<LevelMonitorValueData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);
            nodeEditorData.Tag = "MonitorValue";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            var InPin = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Float);
            AddPin(InPin);

            BluePrintPin pinOut = new BluePrintPin(this, 2, "OnChange", PinType.Main, PinStream.Out, 20);
            AddPin(pinOut);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.value = EditorGUILayout.FloatField("初始值", HostData.value, new GUILayoutOption[] { GUILayout.Width(270f) });
        }

        public override List<LevelMonitorValueData> GetDataList(LevelGraphData data)
        {
            return data.MonitorValueData;
        }
    }
}
