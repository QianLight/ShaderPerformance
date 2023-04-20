using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelMonsterCountNode:LevelBaseNode<LevelMonsterCountData>
    {
        enum CountType
        {
            Death,
            Alive
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeEditorData.Tag = "MonsterCount";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintValuedPin pinOut = new BluePrintValuedPin(this, 1, "Count", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinOut);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.countType = (uint)(CountType)EditorGUILayout.EnumPopup("计数类型", (CountType)HostData.countType, GUILayout.Width(250f));

            if(GUILayout.Button("Add",GUILayout.Width(75f)))
            {
                HostData.waveNodeIDList.Add(0);
                HostData.waveGraphIDList.Add(0);
            }

            int temp = -1;
            for (var i = 0; i < HostData.waveNodeIDList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("GraphID", GUILayout.Width(50f));
                HostData.waveGraphIDList[i] = (uint)EditorGUILayout.FloatField(HostData.waveGraphIDList[i], GUILayout.Width(75f));
                EditorGUILayout.LabelField("NodeID", GUILayout.Width(50f));
                HostData.waveNodeIDList[i] = (uint)EditorGUILayout.FloatField(HostData.waveNodeIDList[i], GUILayout.Width(75f));
                if(GUILayout.Button("-"))
                {
                    temp = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if(temp>=0)
            {
                HostData.waveGraphIDList.RemoveAt(temp);
                HostData.waveNodeIDList.RemoveAt(temp);
            }
        }

        public override List<LevelMonsterCountData> GetDataList(LevelGraphData data)
        {
            return data.monsterCountData;
        }

        public override void CheckError()
        {
            base.CheckError();
            for(var i=0;i<HostData.waveGraphIDList.Count;i++)
            {
                var graph = LevelEditor.Instance.GetGraph((int)HostData.waveGraphIDList[i]);
                if(graph==null)
                {
                    nodeErrorInfo.ErrorDataList.Add(
                        new BlueprintErrorData(BlueprintErrorCode.NodeDataError, string.Format("计数节点关联的子图不存在,GraphID:{0}",HostData.waveGraphIDList[i]), null));
                    return;
                }
                var node = graph.GetNode((int)HostData.waveNodeIDList[i])as LevelWaveNode;
                if (node == null)
                {
                    nodeErrorInfo.ErrorDataList.Add(
                        new BlueprintErrorData(BlueprintErrorCode.NodeDataError, 
                        string.Format("计数节点关联的wave节点不存在或该节点不是个wave节点,NodeID:{0}",HostData.waveNodeIDList[i]), null));
                    return;
                }
            }
        }
    }
}
