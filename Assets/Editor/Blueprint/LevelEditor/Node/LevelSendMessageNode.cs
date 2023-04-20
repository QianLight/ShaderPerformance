using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelSendMessageNode:LevelBaseNode<LevelSendMessageData>
    {

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);
            nodeEditorData.Tag = "SendMessage";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinIn1 = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn1);
            BluePrintPin pinIn2 = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn2);
            BluePrintPin pinIn3 = new BluePrintValuedPin(this, 3, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn3);
            BluePrintPin pinIn4 = new BluePrintValuedPin(this, 4, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn4);
        }

        public override void DrawTipBox(Rect boxRect)
        {
            base.DrawTipBox(boxRect);
            Rect tipRect = new Rect(boxRect.x + 50, boxRect.y + boxRect.height - 30, boxRect.width - 80, 10);
            DrawTool.DrawExpandableBox(tipRect, BlueprintStyles.AreaCommentStyle, HostData.message, 20);
        }

        public override List<LevelSendMessageData> GetDataList(LevelGraphData data)
        {
            return data.sendMessageData;
        }


        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("消息名", GUILayout.Width(160f));
            HostData.message = EditorGUILayout.TextField(HostData.message, GUILayout.Width(120f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("是否发送给所有团本内关卡", GUILayout.Width(160f));
            HostData.sendToAll = EditorGUILayout.Toggle(HostData.sendToAll, GUILayout.Width(80f));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("是否缓存消息", GUILayout.Width(160f));
            HostData.cacheMessage = EditorGUILayout.Toggle(HostData.cacheMessage, GUILayout.Width(80f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("目标阶段", GUILayout.Width(160f));
            HostData.targetPhase = (uint)EditorGUILayout.FloatField(HostData.targetPhase, GUILayout.Width(80f));
            EditorGUILayout.EndHorizontal();

            if(!HostData.sendToAll)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("MapIDList", GUILayout.Width(160f));
                if (GUILayout.Button("+", GUILayout.Width(60f)))
                {
                    HostData.mapIDList.Add(0);
                }
                EditorGUILayout.EndHorizontal();

                int tempIndex = -1;
                for (var i = 0; i < HostData.mapIDList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("Map{0}:", i), GUILayout.Width(60f));
                    HostData.mapIDList[i] = (uint)EditorGUILayout.FloatField(HostData.mapIDList[i], GUILayout.Width(80f));
                    if (GUILayout.Button("-",GUILayout.Width(60f)))
                        tempIndex = i;
                    EditorGUILayout.EndHorizontal();
                }
                if (tempIndex >= 0)
                {
                    HostData.mapIDList.RemoveAt(tempIndex);
                }
            }
        }

        public override void BeforeSave()
        {
            base.BeforeSave();
            if (HostData.sendToAll)
                HostData.mapIDList.Clear();
        }

        public override void CheckError()
        {
            base.CheckError();
            if(!HostData.sendToAll&&HostData.mapIDList.Count<=0)
            {
                nodeErrorInfo.ErrorDataList.Add(
                              new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "MapIDList为空", null));
            }
        }
    }
}
