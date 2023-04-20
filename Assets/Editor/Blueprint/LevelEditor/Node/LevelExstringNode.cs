using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
    class LevelExstringNode : LevelBaseNode<LevelExstringData>
    {
        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeEditorData.Tag = "ExString";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin OutPin = new BluePrintPin(this, 1, "", PinType.Main, PinStream.Out, 20);
            AddPin(OutPin);

            BluePrintPin InPin = new BluePrintPin(this, 2, "", PinType.Main, PinStream.In, 20);
            AddPin(InPin);

        }

        public override List<LevelExstringData> GetDataList(LevelGraphData data)
        {
            return data.ExstringData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.exString = EditorGUILayout.TextField("Exstring", HostData.exString, new GUILayoutOption[] { GUILayout.Width(270f) });

            HostData.maxTriggerTime = EditorGUILayout.IntField("最大触发次数", HostData.maxTriggerTime, new GUILayoutOption[] { GUILayout.Width(270f) });
        }

        #region Simulation

        LevelRTExstringNode exstringData;

        public override void OnEnterSimulation()
        {
            base.OnEnterSimulation();
            exstringData = RuntimeData as LevelRTExstringNode;

            BlueprinButton bpButton = new BlueprinButton(this);
            bpButton.ImageName = "BluePrint/ExstringActive";
            AddButton(bpButton);
            bpButton.RegisterClickEvent(ButtonClickCb);

            ExstringManager.RegisterListenString(exstringData.GetData().exString, exstringData);
        }

        public override void OnEndSimulation()
        {
            exstringData = null;
            base.OnEndSimulation();
        }

        protected void ButtonClickCb(object o)
        {
            ExstringManager.ActiveString(exstringData.GetData().exString);
        }

        public override void Draw()
        {
            base.Draw();

            var textRect = new Rect(Bounds.x, Bounds.y + titleHeight, Bounds.width, Bounds.height - titleHeight).Scale(Scale);
            DrawTool.DrawLabel(textRect, HostData.exString, BlueprintStyles.PinTextStyle, TextAnchor.UpperCenter);
        }

        public override bool DrawNodeExtra(Rect boxRect, ref Rect RectWithExtra)
        {
            if (LevelEditor.state == LEVEL_EDITOR_STATE.simulation_mode)
            {
                Rect simulationRect = new Rect(boxRect.x, boxRect.y + boxRect.height, boxRect.width, 30);
                DrawTool.DrawStretchBox(simulationRect, BlueprintStyles.NodeBackground, 12f);

                buttonList[0].Bounds = new Rect(boxRect.x + 8 , boxRect.y + boxRect.height + 2, 40, 30);
                buttonList[0].Draw();

                RectWithExtra = new Rect(boxRect.x, boxRect.y, boxRect.width, boxRect.height + 30);
                return true;
            }
            return false;
        }
        #endregion

        public override void CheckError()
        {
            base.CheckError();

            BlueprintNodeErrorInfo error = nodeErrorInfo;
            error.nodeID = nodeEditorData.NodeID;

            if (string.IsNullOrEmpty(HostData.exString))
            {
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "exstring不能为空", null));
            }
        }
    }
}
