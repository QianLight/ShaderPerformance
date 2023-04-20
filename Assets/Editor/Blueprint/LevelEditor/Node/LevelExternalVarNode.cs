using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{

    public class LevelGetExternalVarNode : LevelBaseNode<LevelVarData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";
            nodeEditorData.Tag = "GetExternalVar";
            CanbeFold = true;

            BluePrintPin pinValue = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinValue);
        }

        public override List<LevelVarData> GetDataList(LevelGraphData data)
        {
            return data.GetGlobalData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.VarName = EditorGUILayout.TextField("ExternalVar", HostData.VarName, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }

    public class LevelSetExternalVarNode : LevelBaseNode<LevelVarData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";
            nodeEditorData.Tag = "SetExternalVar";
            CanbeFold = true;

            BluePrintPin pinValue = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Float);
            AddPin(pinValue);

            BluePrintPin pinValueOut = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinValueOut);
        }

        public override List<LevelVarData> GetDataList(LevelGraphData data)
        {
            return data.SetGlobalData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.VarName = EditorGUILayout.TextField("ExternalVar", HostData.VarName, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }
}