using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelFailNode : LevelBaseNode<LevelFailData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeEditorData.Tag = "Fail";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin InPin = new BluePrintPin(this, 1, "", PinType.Main, PinStream.In, 20);
            AddPin(InPin);
        }

        public override List<LevelFailData> GetDataList(LevelGraphData data)
        {
            return data.FailData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.PlayerAbandon = EditorGUILayout.Toggle("PlayerDie",HostData.PlayerAbandon, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }
}
