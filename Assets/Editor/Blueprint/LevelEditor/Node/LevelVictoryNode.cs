using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelVictoryNode : LevelBaseNode<LevelVictoryData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeEditorData.Tag = "Victory";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin InPin = new BluePrintPin(this, 1, "", PinType.Main, PinStream.In, 20);
            AddPin(InPin);
        }

        public override List<LevelVictoryData> GetDataList(LevelGraphData data)
        {
            return data.VictoryData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            EditorGUILayout.LabelField("Victory!", new GUILayoutOption[] { GUILayout.Width(270f) });
        }


    }
}
