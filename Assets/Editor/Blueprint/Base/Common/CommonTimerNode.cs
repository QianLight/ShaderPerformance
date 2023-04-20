using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class CommonTimerNode : BluePrintBaseDataNode<BluePrintTimerData>
    {
        enum TimerType
        {
            None=0,
            Chapter=1,
            Common=2,
            Adv=3,
            Fish=4,
            CountNameDown=5
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            HeaderImage = "BluePrint/Header8";
            nodeEditorData.Tag = "Timer";
            nodeEditorData.BackgroundText = "";

            BluePrintPin pinIn = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn);
        }

        public override List<BluePrintTimerData> GetCommonDataList(BluePrintData data)
        {
            return data.TimerData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HostData.Interval = EditorGUILayout.FloatField("Time: ", HostData.Interval, new GUILayoutOption[] { GUILayout.Width(270f) });

            HostData.Sync = EditorGUILayout.Toggle("SyncToClient: ", HostData.Sync, new GUILayoutOption[] { GUILayout.Width(270f) });

            HostData.timerType = (int)(TimerType)EditorGUILayout.EnumPopup("TimerType:",
                   (TimerType)HostData.timerType, new GUILayoutOption[] { GUILayout.Width(270f) });
        }

        public override void Draw()
        {
            base.Draw();

            var textRect = new Rect(Bounds.x, Bounds.y + titleHeight, Bounds.width, Bounds.height - titleHeight).Scale(Scale);
            DrawTool.DrawLabel(textRect, HostData.Interval.ToString(), BlueprintStyles.PinTextStyle, TextAnchor.UpperCenter);
        }

    }
}
