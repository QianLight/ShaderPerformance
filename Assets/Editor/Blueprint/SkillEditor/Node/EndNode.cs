using EcsData;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class EndNode : EventTriggerNode<XEndData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);

            HeaderImage = "BluePrint/Header3";

            if (AutoDefaultMainPin)
            {
                BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
                AddPin(pinIn);
            }
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
        }
    }
}
