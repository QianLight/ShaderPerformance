using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelStringBuildNode:LevelBaseNode<LevelStringBuildData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeEditorData.Tag = "StringBuild";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintValuedPin OutPin = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.Out, VariantType.Var_String);
            AddPin(OutPin);

            BluePrintValuedPin InPin1 = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(InPin1);

            BluePrintValuedPin InPin2 = new BluePrintValuedPin(this, 3, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(InPin2);

            BluePrintValuedPin InPin3 = new BluePrintValuedPin(this, 4, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(InPin3);

            BluePrintValuedPin InPin4 = new BluePrintValuedPin(this, 5, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(InPin4);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUILayout.LabelField("String");
            HostData.str = EditorGUILayout.TextField(HostData.str, new GUILayoutOption[] { GUILayout.Width(270f),GUILayout.Height(50f) });
        }

        public override List<LevelStringBuildData> GetDataList(LevelGraphData data)
        {
            return data.StringBuildData;
        }
    }
}
