using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelSetDataNode:LevelBaseNode<LevelSetParamData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, true);

            nodeEditorData.Tag = "SetData";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin InPin = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(InPin);
            BluePrintPin OutPin = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(OutPin);
        }

        public override List<LevelSetParamData> GetDataList(LevelGraphData data)
        {
            return data.ParamData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.nodeID = (uint)EditorGUILayout.FloatField("nodeID:", HostData.nodeID, new GUILayoutOption[] { GUILayout.Width(270f) });
            HostData.graphID = (uint)EditorGUILayout.FloatField("graphID:", HostData.graphID, new GUILayoutOption[] { GUILayout.Width(270f) });
            HostData.data = EditorGUILayout.TextField("param", HostData.data, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }
}
