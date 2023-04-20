using EcsData;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class LookAtNode : TimeTriggerNode<XLookAtData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);

            HeaderImage = "BluePrint/Header3";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HosterData.LifeTime = TimeFrameField("Time: ", HosterData.LifeTime, true);
        }
    }
}
