using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    public class LevelRecordNode : LevelBaseNode<LevelRecordData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, true);

            nodeEditorData.Tag = "Record";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;
        }

        public override List<LevelRecordData> GetDataList(LevelGraphData data)
        {
            return data.RecordData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.recordID = EditorGUILayout.IntField("RecordID", HostData.recordID, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }

    public class LevelRunSkillNode : LevelBaseNode<LevelRunSkillScriptData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, true);

            nodeEditorData.Tag = "SkillScript";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;
        }

        public override List<LevelRunSkillScriptData> GetDataList(LevelGraphData data)
        {
            return data.RunSkillScriptData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUILayout.LabelField("SkillScript", LevelGraph.TitleLayout);
            HostData.script = EditorGUILayout.TextField( HostData.script, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }

    public class LevelRatioNode : LevelBaseNode<LevelRatioData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, true);

            nodeEditorData.Tag = "ServerRation";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;
        }

        public override List<LevelRatioData> GetDataList(LevelGraphData data)
        {
            return data.RatioData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            //EditorGUILayout.LabelField("Rate", LevelGraph.TitleLayout);
            //HostData.rate = EditorGUILayout.FloatField(HostData.rate, new GUILayoutOption[] { GUILayout.Width(270f) });

            HostData.rate = EditorGUILayout.FloatField("Rate", HostData.rate, new GUILayoutOption[] { GUILayout.Width(270f) });
        }
    }
}