using EcsData;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class TimerNode : EventTriggerNode<XTimerData>
    {
        public override float NextTime { get { return TriggerTime + HosterData.Time; } }
        public bool NoRightDownEvent = false;
        public override bool RightDownEvent { get { return !NoRightDownEvent; } }
        public bool ShowInspector = true;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);

            HeaderImage = "BluePrint/Header3";
        }

        public override void DrawDataInspector()
        {
            if (ShowInspector)
            {
                base.DrawDataInspector();
                HosterData.Time = TimeFrameField("Time", HosterData.Time, true);
            }
        }
    }
}
