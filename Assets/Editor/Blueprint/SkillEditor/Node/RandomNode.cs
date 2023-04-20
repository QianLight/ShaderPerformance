using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class RandomNode : TimeTriggerNode<XRandomData>
    {
        public override bool BranchNode => true;
        private const int StartIndex = 1;

        public override bool OneConnectPinOut { get { return true; } }

        public override bool IgnoreMultiIn => false;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);
            HeaderImage = "BluePrint/Header8";

            BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
            AddPin(pinIn);

            if (HosterData != null)
            {
                for (int i = 0; i < HosterData.Params.Count; ++i)
                {
                    BluePrintPin pinOut = new BluePrintPin(this, HosterData.Params[i], HosterData.Params[i].ToString(), PinType.Main, PinStream.Out, pinList[pinList.Count - 1].connectDeltaX + 10);
                    AddPin(pinOut);
                }
            }
        }

        public override T CopyData<T>(T data)
        {
            XRandomData copy = base.CopyData(data) as XRandomData;
            List<int> rhs = new List<int>();
            for (int i = 0; i < copy.Params.Count; ++i)
                rhs.Add(copy.Params[i]);
            copy.Params = rhs;

            return copy as T;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUI.BeginDisabledGroup(HosterData.Params.Count <= 1);
            HosterData.IsCumulative = EditorGUITool.Toggle("IsCumulative", HosterData.IsCumulative);
            if (HosterData.Params.Count <= 1) HosterData.IsCumulative = false;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginHorizontal();
            EditorGUITool.LabelField("Random");
            if (HosterData.Params.Count < 6)
            {
                if (GUILayout.Button("+"))
                {
                    HosterData.Params.Add(pinList.Count + 1);
                    BluePrintPin pinOut = new BluePrintPin(this, pinList.Count, pinList.Count.ToString(), PinType.Main, PinStream.Out, pinList[pinList.Count - 1].connectDeltaX + 10);
                    AddPin(pinOut);
                }
            }
            EditorGUILayout.EndHorizontal();

            int sum = 0;
            for (int i = 0; i < HosterData.Params.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                HosterData.Params[i] = EditorGUITool.IntField("Weight", HosterData.Params[i]);
                pinList[i + StartIndex].Desc = HosterData.Params[i].ToString();
                HosterData.Params[i] = HosterData.Params[i] < 0 ? 0 : HosterData.Params[i];
                if (sum + HosterData.Params[i] > 100) HosterData.Params[i] = 100 - sum;
                sum += HosterData.Params[i];

                if (GUILayout.Button("-"))
                {
                    RemovePin(pinList[i + StartIndex]);
                    HosterData.Params.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public override void BuildDataByPin()
        {
            GetHosterData<XBaseData>().TransferData.Clear();

            for (int i = 1; i < pinList.Count; ++i)
            {
                if (pinList[i].connections.Count != 0)
                {
                    AddPinData<XBaseData>(pinList[i].connections[0].connectEnd.GetNode<BaseSkillNode>().GetHosterData<XBaseData>());
                }
                else
                {
                    AddPinData<XBaseData>(new XBaseData() { Index = -1 });
                }
            }
        }

        public override void BuildPinByData(Dictionary<int, BaseSkillNode> IndexToNodeDic)
        {
            for (int i = 2; i < pinList.Count; ++i)
            {
                pinList[i].OnDeleted();
            }
            if (pinList.Count > 2)
                pinList.RemoveRange(2, pinList.Count - 2);

            for (int i = 0; i < GetHosterData<XBaseData>().TransferData.Count; ++i)
            {
                if (i != 0)
                {
                    BluePrintPin pinOut = new BluePrintPin(this, HosterData.Params[i], HosterData.Params[i].ToString(), PinType.Main, PinStream.Out, pinList[pinList.Count - 1].connectDeltaX + 10);
                    AddPin(pinOut);
                }
                if (GetHosterData<XBaseData>().TransferData[i].Index != -1)
                {
                    BluePrintPin startPin = pinList[i + StartIndex];
                    BluePrintPin endPin = IndexToNodeDic[GetHosterData<XBaseData>().TransferData[i].Index].pinList[0];
                    ConnectPin(startPin, endPin);
                }
            }
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            int sum = 0;
            for (int i = 1; i < HosterData.Params.Count; ++i)
            {
                sum += HosterData.Params[i];
            }
            if (sum > 100) LogError("RandomNode出口值总和不能超过100！！！");

            if (HosterData.IsCumulative)
            {
                for (int i = 1; i < HosterData.Params.Count; ++i)
                {
                    if (HosterData.Params[i] > HosterData.Params[i - 1])
                    {
                        LogError("RandomNode 使用IsCumulative，权值必须从大到小排列！！！");
                        break;
                    }
                }
            }

            return true;
        }
    }
}