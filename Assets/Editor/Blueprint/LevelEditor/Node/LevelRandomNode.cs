using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    public class LevelRandomNode : LevelBaseNode<LevelRandomNodeData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, true);

            nodeEditorData.Tag = "GlobalRandom";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;
        }

        public override List<LevelRandomNodeData> GetDataList(LevelGraphData data)
        {
            return data.RandomData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.randomID = EditorGUILayout.IntField("RandomID", HostData.randomID, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }
}
