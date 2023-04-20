using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    public class LevelStartTimingNode : LevelBaseNode<LevelStartTimeData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, true);

            nodeEditorData.Tag = "StartTiming";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinTimeOut = new BluePrintPin(this, 1, "TimeOut", PinType.Main, PinStream.Out);
            AddPin(pinTimeOut);
        }

        public override List<LevelStartTimeData> GetDataList(LevelGraphData data)
        {
            return data.StartTimeData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.CountDown = EditorGUILayout.Toggle("倒计时", HostData.CountDown, new GUILayoutOption[] { GUILayout.Width(270f) });

            if(HostData.CountDown)
            {
                HostData.CountDownLimit = EditorGUILayout.FloatField("时限", HostData.CountDownLimit, new GUILayoutOption[] { GUILayout.Width(270f) });
            }

            HostData.Sync = EditorGUILayout.Toggle("同步到客户端", HostData.Sync, new GUILayoutOption[] { GUILayout.Width(270f) });
            HostData.Tag = EditorGUILayout.TextField("Tag", HostData.Tag, new GUILayoutOption[] { GUILayout.Width(270f) });
        }

        public override void CheckError()
        {
            base.CheckError();

            if(!HostData.CountDown) return;

            BlueprintNodeErrorInfo error = nodeErrorInfo;
            //error.nodeID = nodeEditorData.NodeID;

            if(HostData.CountDownLimit == 0)
            {
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "倒计时时限不能为0", null));
            }

            BluePrintPin outPin = GetPin(-2);
            if(outPin != null && outPin.connections.Count == 1)
            {
                BluePrintNode n = outPin.connections[0].connectEnd.GetNode<BluePrintNode>();
                if(n is BluePrintSubGraphNode) return;
            }
            
            error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "倒计时节点必须连SubGraph", null));
        }
    }


    public class LevelEndTimingNode : LevelBaseNode<LevelEndTimeData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, true);

            nodeEditorData.Tag = "EndTiming";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinTime = new BluePrintValuedPin(this, 1, "Time", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinTime);
        }

        public override List<LevelEndTimeData> GetDataList(LevelGraphData data)
        {
            return data.EndTimeData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.Tag = EditorGUILayout.TextField("Tag", HostData.Tag, new GUILayoutOption[] { GUILayout.Width(270f) });
        }

        public override void CheckError()
        {
            base.CheckError();

            // BluePrintPin outPin = GetPin(-1);
            // if(outPin != null && outPin.reverseConnections.Count == 1)
            // {
            //     BluePrintNode n = outPin.reverseConnections[0].reverseConnectEnd.GetNode<BluePrintNode>();
            //     if(n is BluePrintSubGraphNode) return;
            // }

            // BlueprintNodeErrorInfo error = nodeErrorInfo;
            // error.nodeID = nodeEditorData.NodeID;
            // error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "End Timing node 必须连SubGraph", null));
        }
    }
}