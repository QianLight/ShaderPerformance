using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelSwitchPartnerNode : LevelBaseNode<LevelSwitchPartnerData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeEditorData.Tag = "SwitchPartner";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin OutPin = new BluePrintPin(this, 1, "", PinType.Main, PinStream.Out, 20);
            AddPin(OutPin);

            BluePrintPin InPin = new BluePrintPin(this, 2, "", PinType.Main, PinStream.In, 20);
            AddPin(InPin);

            BluePrintPin inPin1 = new BluePrintValuedPin(this, 3, "RoleID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            AddPin(inPin1);
        }

        public override List<LevelSwitchPartnerData> GetDataList(LevelGraphData data)
        {
            return data.SwitchPartnerData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.PartnerID = EditorGUILayout.IntField("PartnerID", HostData.PartnerID, new GUILayoutOption[] { GUILayout.Width(270f) });

            HostData.ForcePartnerID = EditorGUILayout.IntField("ForcePartnerID", HostData.ForcePartnerID, new GUILayoutOption[] { GUILayout.Width(270f) });

            HostData.ForceBack=(int)(State)EditorGUILayout.EnumPopup("ForceBack",(State)HostData.ForceBack, new GUILayoutOption[] { GUILayout.Width(270f) });

            HostData.AddPartnerID = EditorGUILayout.IntField("AddPatnerID", HostData.AddPartnerID, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }
}
