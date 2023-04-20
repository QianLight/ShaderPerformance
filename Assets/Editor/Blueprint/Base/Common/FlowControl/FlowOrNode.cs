using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class FlowOrNode : ControllerBaseNode
    {
        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.FlowNode_Or;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.Tag = "FlowOr";
            nodeEditorData.BackgroundText = "";

            BluePrintPin pinIn1 = new BluePrintPin(this, 1, "", PinType.Main, PinStream.In);
            AddPin(pinIn1);

            BluePrintPin pinIn2 = new BluePrintPin(this, 2, "", PinType.Main, PinStream.In);
            AddPin(pinIn2);

            BluePrintPin pinOut = new BluePrintPin(this, 3, "", PinType.Main, PinStream.Out);
            AddPin(pinOut);
        }
    }
}
