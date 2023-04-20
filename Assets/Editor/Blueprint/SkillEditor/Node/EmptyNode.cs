using EcsData;
using UnityEngine;
using BluePrint;
using UnityEditor;
using System.Collections.Generic;

namespace EditorNode
{
    public class EmptyNode : TimeTriggerNode<XEmptyData>
    {
        public override bool OneConnectPinOut { get { return HosterData.Group; } }

        public override float OffsetX
        {
            get { return HosterData.Group ? -50 : 0; }
        }

        public override void DrawPin(Rect pinRect)
        {
            int inCount = 0;
            int outCount = 0;

            //Vector2 size = new Vector2(16, 16) * Scale;
            float iconSize = 16;

            for (int i = 0; i < pinList.Count; i++)
            {
                if (!nodeEditorData.Expand && !pinList[i].HasConnectionOrDefaultValue()) continue;

                if (pinList[i].pinStream == PinStream.In)
                {
                    pinList[i].Bounds = new Rect(pinRect.x + 8, pinRect.y + inCount * 22, iconSize, iconSize);
                    inCount++;
                }
                else if (pinList[i].pinStream == PinStream.Out)
                {
                    if (HosterData.Group)
                    {
                        pinList[i].Bounds = new Rect(Bounds.x + 40 + GetRoot.GetDelta() * (i > 1 ? GetRoot.TimeToFrame(HosterData.TransTime[i - 2]) : 0), Bounds.y + titleHeight + 10, 10, iconSize);
                        pinList[i].Desc = "";
                        pinList[i].connectDeltaX = 0;
                        pinList[i].connectDeltaY = 0;
                        pinList[i].pinHeight = 0;
                    }
                    else
                    {
                        pinList[i].Bounds = new Rect(pinRect.x + pinRect.width - 24, pinRect.y + outCount * 22, iconSize, iconSize);
                        pinList[i].Desc = "Out";
                        pinList[i].connectDeltaX = 15;
                        pinList[i].connectDeltaY = 70;
                        pinList[i].pinHeight = 22;
                    }
                    outCount++;
                    if (i > 1) DrawPinEx(pinList[i], HosterData.TransTime[i - 2]);
                }

                pinList[i].Draw();
            }

            if (HosterData.Group)
            {
                DrawTimeLine((int)GetRoot.TimeToFrame(HosterData.Length));
            }
            else
            {
                nodeWidth = 0;
            }
        }

        private void DrawPinEx(BluePrintPin pin, float time)
        {
            if (pin.connections.Count == 0)
            {
                var node = CreateTransNode(pin.Bounds.position + new Vector2(50, 50));

                BluePrintPin startPin = pin;
                BluePrintPin endPin = node.pinList[0];
                ConnectPin(startPin, endPin);
            }

            TimerNode n = pin.connections[0].connectEnd.GetNode<TimerNode>();
            n.Bounds.position = new Vector2(pin.Bounds.position.x + 40, n.Bounds.position.y);
            n.HosterData.Time = time;
            n.nodeEditorData.TitleName = string.Empty;
            n.nodeEditorData.Tag = n.nodeEditorData.Tag.Replace("Timer", "Trans");
            n.NoRightDownEvent = true;
            pin.NoRightDownEvent = true;
            n.ShowInspector = false;
            n.DrawHead = false;
            n.nodeWidth = 90;
            for (int i = 0; i < n.pinList.Count; ++i)
            {
                n.pinList[i].Desc = i == 0 ? ((int)(GetRoot.TimeToFrame(time) * 100) / 100f).ToString() : string.Empty;
            }
        }

        public void DrawTimeLine(int frame)
        {
            nodeWidth = 100 + GetRoot.GetDelta() * frame;
            Vector2 pos = new Vector2(Bounds.x + 50, Bounds.y + titleHeight * 0.7f) * Scale;
            float delta = GetRoot.GetDelta() * Scale;
            Vector2 start;
            Vector2 end;
            Color oldHandleColor = Handles.color;
            Handles.color = Color.white;
            for (int i = 0; i <= frame; ++i)
            {

                start = new Vector2(pos.x + i * delta, pos.y + 0);
                if (i % 10 == 0)
                {
                    end = new Vector2(pos.x + i * delta, pos.y + 20 * Scale);
                }
                else if (i % 5 == 0)
                {
                    end = new Vector2(pos.x + i * delta, pos.y + 10 * Scale);
                }
                else
                {
                    end = new Vector2(pos.x + i * delta, pos.y + 5 * Scale);
                }

                Handles.DrawLine(start, end);
                if (i % 10 == 0)
                {
                    GUI.Label(new Rect(start.x + 1, start.y, 100 * Scale, 25 * Scale), new GUIContent(i.ToString()), BlueprintStyles.PinTextStyle);
                }
            }

            Handles.color = oldHandleColor;
        }

        private TimerNode CreateTransNode(Vector3 pos)
        {
            var data = (GetRoot is SkillGraph) ? (GetRoot.GetConfigData<EcsData.XSkillData>().TimerData) : (GetRoot.GetConfigData<EcsData.XHitData>().TimerData);
            var node = GetRoot.AddNodeInGraphByScript<XTimerData, EditorNode.TimerNode>(pos, ref data, true);
            return node;
        }

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);

            HeaderImage = "BluePrint/Header1";
        }

        public override void OnDeleteClicked()
        {
            for (int i = pinList.Count - 1; i > 1; --i)
            {
                pinList[i].connections[0].connectEnd.GetNode<BaseSkillNode>().OnDeleteClicked();
                RemovePin(pinList[i]);
            }

            base.OnDeleteClicked();
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            if (HosterData.TransTime.Count == 0)
            {
                HosterData.Group = EditorGUITool.Toggle("Group", HosterData.Group);
                if (HosterData.Group && pinList[1].connections.Count > 1)
                {
                    LogError("使用Group，Out连出节点个数必须小于2！！！");
                    HosterData.Group = false;
                }
            }
            if(HosterData.Group)
            {
                HosterData.Length = GetRoot.TimeFrameField("Length", HosterData.Length);
                DrawTrans();
            }
        }

        private void DrawTrans()
        {
            DrawLine();
            EditorGUILayout.BeginHorizontal();
            EditorGUITool.LabelField("Trans");
            if (HosterData.TransTime.Count < 8)
            {
                if (GUILayout.Button("+"))
                {
                    HosterData.TransTime.Add(HosterData.TransTime.Count == 0 ? 0 : HosterData.TransTime[HosterData.TransTime.Count - 1]);

                    BluePrintPin pinOut = new BluePrintPin(this, pinList.Count, "", PinType.Main, PinStream.Out, 0, 0, 0);
                    AddPin(pinOut);
                }
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < HosterData.TransTime.Count; ++i)
            {
                DrawLine();
                EditorGUILayout.BeginHorizontal();
                EditorGUITool.LabelField("Trans_" + i.ToString());
                if (GUILayout.Button("Delete"))
                {
                    HosterData.TransTime.RemoveAt(i);
                    pinList[i + 2].connections[0].connectEnd.GetNode<BaseSkillNode>().OnDeleteClicked();
                    RemovePin(pinList[i + 2]);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                float minValue = 0;
                float maxValue = HosterData.Length;
                HosterData.TransTime[i] = Mathf.Max(minValue, Mathf.Min(maxValue, TimeFrameField("At", HosterData.TransTime[i])));
            }
        }

        public override T CopyData<T>(T data)
        {
            XEmptyData copy = base.CopyData(data) as XEmptyData;

            List<float> transTime = new List<float>();
            for (int i = 0; i < copy.TransTime.Count; ++i)
                transTime.Add(copy.TransTime[i]);
            copy.TransTime = transTime;

            return copy as T;
        }

        public override void BuildDataByPin()
        {
            GetHosterData<XBaseData>().TransferData.Clear();

            for (int i = 1; i < pinList.Count; ++i)
            {
                if (pinList[i].connections.Count != 0)
                {
                    for (int j = 0; j < pinList[i].connections.Count; ++j)
                        AddPinData<XBaseData>(pinList[i].connections[j].connectEnd.GetNode<BaseSkillNode>().GetHosterData<XBaseData>());
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
                if (i > 0 && HosterData.TransTime.Count > 0)
                {
                    BluePrintPin pinOut = new BluePrintPin(this, pinList.Count, "", PinType.Main, PinStream.Out, 0, 0, 0);
                    AddPin(pinOut);
                }
                if (GetHosterData<XBaseData>().TransferData[i].Index != -1)
                {
                    BluePrintPin startPin = pinList[pinList.Count - 1];
                    BluePrintPin endPin = IndexToNodeDic[GetHosterData<XBaseData>().TransferData[i].Index].pinList[0];
                    ConnectPin(startPin, endPin);
                }
            }
        }
    }
}
