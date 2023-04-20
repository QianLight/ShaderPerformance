using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;


namespace LevelEditor
{
    class LevelTemproryPartnerNode:LevelBaseNode<LevelTemproryPartnerData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.Tag = "TemproryPartner";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;
        }

        public override List<LevelTemproryPartnerData> GetDataList(LevelGraphData data)
        {
            return data.TemproryPartnerData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.partnerID = EditorGUILayout.IntField("PartnerID", HostData.partnerID, new GUILayoutOption[] { GUILayout.Width(270f) });
            HostData.add = EditorGUILayout.Toggle("添加", HostData.add, new GUILayoutOption[] {GUILayout.Width(270f) });
        }
    }
}
