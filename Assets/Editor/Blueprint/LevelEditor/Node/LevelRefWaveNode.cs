using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using CFUtilPoolLib;
using XEditor;

namespace LevelEditor
{
    class LevelRefWaveNode:LevelBaseNode<LevelRefWaveData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.Tag = "RefWave";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinHp = new BluePrintValuedPin(this, 1, "Hp", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinHp);

            BluePrintPin pinCount = new BluePrintValuedPin(this, 2, "Alive", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinCount);

            BluePrintPin pinID = new BluePrintValuedPin(this, 3, "ID", PinType.Data, PinStream.Out, VariantType.Var_UINT64);
            AddPin(pinID);

            BluePrintPin pinPosOut = new BluePrintValuedPin(this, 4, "Pos", PinType.Data, PinStream.Out, VariantType.Var_Vector3);
            AddPin(pinPosOut);
            
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.waveNodeID = EditorGUILayout.IntField("waveNodeID",HostData.waveNodeID, new GUILayoutOption[] { GUILayout.Width(275f) });

            HostData.graphID = EditorGUILayout.IntField("waveGraphID", HostData.graphID, new GUILayoutOption[] { GUILayout.Width(275f) });
        }

        public override List<LevelRefWaveData> GetDataList(LevelGraphData data)
        {
            return data.RefWaveData;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            LevelGraph levelGraph = (LevelGraph)LevelEditor.Instance.GetGraph(HostData.graphID);
            if (levelGraph == null)
                return;
            if (!LevelEditor.Instance.OpenGrahps.Contains(levelGraph))
            {
                LevelEditor.Instance.OpenGrahps.Add(levelGraph);
            }
            var waveNode = levelGraph.GetNode(HostData.waveNodeID);
            waveNode?.OnSelected();
        }

        public override void DrawTipBox(Rect boxRect)
        {
            base.DrawTipBox(boxRect);
            var graph = LevelEditor.Instance.GetGraph(HostData.graphID);
            if (graph == null)
                return;
            var node = graph.GetNode(HostData.waveNodeID) as LevelWaveNode;
            if (node == null)
                return;
            var monster = LevelEditorTableData.MonsterInfo.GetByID((uint)node.HostData.SpawnID);
            if (monster == null)
                return;
            Rect tipRect = new Rect(boxRect.x + 10, boxRect.y + boxRect.height - 30, boxRect.width - 80, 10);
            DrawTool.DrawExpandableBox(tipRect, BlueprintStyles.AreaCommentStyle, string.IsNullOrEmpty(monster.Name)?"???":monster.Name, 20);
        }
    }
}
