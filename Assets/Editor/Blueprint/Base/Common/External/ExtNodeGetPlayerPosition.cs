using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class ExtNodeGetPlayerPosition : BluePrintBaseDataNode<ExtGetPlayerPositionData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";

            BluePrintPin pinValue = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.Out, VariantType.Var_Vector3);
            AddPin(pinValue);

            BluePrintPin pinPlayerID = new BluePrintValuedPin(this, 2, "PlayerID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            AddPin(pinPlayerID);
        }

        public override List<ExtGetPlayerPositionData> GetCommonDataList(BluePrintData data)
        {
            return data.GetPlayerPositionData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "PlayerPosition";
        }
    }

    class ExtNodeGetPlayerHP : BluePrintBaseDataNode<BluePrintNodeBaseData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";

            BluePrintPin pinValue = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinValue);

            BluePrintPin pinPlayerID = new BluePrintValuedPin(this, 2, "PlayerID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            AddPin(pinPlayerID);
        }

        public override List<BluePrintNodeBaseData> GetCommonDataList(BluePrintData data)
        {
            return data.GetPlayerHpData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "PlayerHP";
        }
    }

    class ExtNodeGetPartnerAttr : BluePrintBaseDataNode<ExtGetPartnerAttrData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";

            BluePrintPin pinValue = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinValue);

            BluePrintPin pinPlayerID = new BluePrintValuedPin(this, 2, "PlayerID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            AddPin(pinPlayerID);
        }

        public override List<ExtGetPartnerAttrData> GetCommonDataList(BluePrintData data)
        {
            return data.GetPartnerAttrData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "GetPartnerAttr";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.PartnerID = EditorGUILayout.IntField("PartnerID", HostData.PartnerID, new GUILayoutOption[] { GUILayout.Width(270f) });

            HostData.AttrID = EditorGUILayout.IntField("AttrID", HostData.AttrID, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }

    class ExtNodeSetPartnerAttr : BluePrintBaseDataNode<ExtSetPartnerAttrData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, true);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";

            BluePrintPin pinValue = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Float);
            AddPin(pinValue);

            BluePrintPin pinPlayerID = new BluePrintValuedPin(this, 2, "PlayerID", PinType.Data, PinStream.In, VariantType.Var_UINT64);
            AddPin(pinPlayerID);
        }

        public override List<ExtSetPartnerAttrData> GetCommonDataList(BluePrintData data)
        {
            return data.SetPartnerAttrData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "SetPartnerAttr";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.PartnerID = EditorGUILayout.IntField("PartnerID", HostData.PartnerID, new GUILayoutOption[] { GUILayout.Width(270f) });

            HostData.AttrID = EditorGUILayout.IntField("AttrID", HostData.AttrID, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }

    class ExtNodeGetPlayerID: BluePrintBaseDataNode<ExtGetPlayerIDData>
    {
        enum PlayerSlot
        {
            All = 0,
            Slot1,
            Slot2,
            Slot3,
            Slot4
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";

            BluePrintPin pinValue = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.Out, VariantType.Var_UINT64);
            AddPin(pinValue);
        }

        public override List<ExtGetPlayerIDData> GetCommonDataList(BluePrintData data)
        {
            return data.GetPlayerIDData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "GetPlayerID";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("PlayerSlot", GUILayout.Width(80f));
            HostData.PlayerSlot = (int)(PlayerSlot)EditorGUILayout.EnumPopup((PlayerSlot)HostData.PlayerSlot, GUILayout.Width(70f));
            EditorGUILayout.EndHorizontal();
        }
    }
    class ExtNodeGetLevelProgress : BluePrintBaseDataNode<ExtLevelProgressData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";

            BluePrintPin pinOut = new BluePrintValuedPin(this, 2, "Progress", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinOut);
        }

        public override List<ExtLevelProgressData> GetCommonDataList(BluePrintData data)
        {
            return data.LevelProgressData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "GetLevelProgress";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.maxProgress = EditorGUILayout.FloatField("MaxProgress", HostData.maxProgress, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }

    class ExtNodeGetScore:BluePrintBaseDataNode<ExtGetScoreData>
    {
        enum PlayerSlot
        {
            Slot1=1,
            Slot2,
            Slot3,
            Slot4
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";

            BluePrintPin pinOut = new BluePrintValuedPin(this, 1, "Score", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinOut);
        }

        public override List<ExtGetScoreData> GetCommonDataList(BluePrintData data)
        {
            return data.PlayerScoreData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "GetScore";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.slot = (uint)(PlayerSlot)EditorGUILayout.EnumPopup("PlayerSlot",(PlayerSlot) HostData.slot, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }

    class ExtNodeGetRandomNum : BluePrintBaseDataNode<ExtGetRandomNumData>
    {

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";

            BluePrintPin pinOut = new BluePrintValuedPin(this, 1, "Num", PinType.Data, PinStream.Out, VariantType.Var_UINT64);
            AddPin(pinOut);
        }

        public override List<ExtGetRandomNumData> GetCommonDataList(BluePrintData data)
        {
            return data.GetRandomNumData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "Random";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.min = EditorGUILayout.FloatField("Min", HostData.min, new GUILayoutOption[] { GUILayout.Width(270f) });
            HostData.max = EditorGUILayout.FloatField("Max", HostData.max, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }

    class ExtNodeEventComplete : BluePrintBaseDataNode<ExtEventCompleteData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;

            nodeEditorData.BackgroundText = "";

            BluePrintPin pinOut = new BluePrintValuedPin(this, 1, "Num", PinType.Data, PinStream.Out, VariantType.Var_Bool);
            AddPin(pinOut);
        }

        public override List<ExtEventCompleteData> GetCommonDataList(BluePrintData data)
        {
            return data.EventCompleteData;
        }

        public override void OnAdded()
        {
            nodeEditorData.Tag = "EventComplete";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.eventID = (uint)EditorGUILayout.FloatField("EventID", HostData.eventID, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }

}
