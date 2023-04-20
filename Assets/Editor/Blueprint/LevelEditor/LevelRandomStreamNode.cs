using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelRandomStreamNode:LevelBaseNode<LevelRandomStreamData>
    {

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);
            nodeEditorData.Tag = "RandomStream";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinIn = new BluePrintPin(this, -1, "", PinType.Main, PinStream.In, 20);
            AddPin(pinIn);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.streamCount = pinList.Count - 1;
            EditorGUILayout.LabelField(string.Format("StreamCount:   {0}", HostData.streamCount), GUILayout.Width(100f));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("StreamList",GUILayout.Width(80f));
            if(GUILayout.Button("+",GUILayout.Width(60f)))
            {
                BluePrintPin pinOut = new BluePrintPin(this, HostData.streamCount, "", PinType.Main, PinStream.Out, 20);
                AddPin(pinOut);
                HostData.weightList.Add(0);
            }
            EditorGUILayout.EndHorizontal();

            int tempIndex = -1;
            int count = 0;
            for(var i=0;i<pinList.Count;i++)
            {
                if (pinList[i].pinStream == PinStream.In)
                    continue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("Pin{0} Weight:", count), GUILayout.Width(100f));
                HostData.weightList[count] = EditorGUILayout.FloatField(HostData.weightList[count], GUILayout.Width(80f));
                if(GUILayout.Button("-",GUILayout.Width(60f)))
                {
                    tempIndex = count;
                }
                EditorGUILayout.EndHorizontal();
                count++;
            }

            if(tempIndex>=0)
            {
                for(var i=0;i<HostData.weightList.Count;i++)
                {
                    var temp = GetPin(i);
                    if (i == tempIndex)
                        RemovePin(temp);
                    else if (i > tempIndex)
                        temp.pinID -= 1;
                }
                HostData.weightList.RemoveAt(tempIndex);
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            for(var i=0;i<HostData.weightList.Count;i++)
            {
                BluePrintPin pinOut = new BluePrintPin(this, i, "", PinType.Main, PinStream.Out, 20);
                AddPin(pinOut);
            }
        }

        public override List<LevelRandomStreamData> GetDataList(LevelGraphData data)
        {
            return data.randomStreamData;
        }
    }
}
