using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class ControllerWhileNode : ControllerBaseNode
    {
        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_While;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.TitleName = "While";
            nodeEditorData.BackgroundText = "";

            BluePrintPin pinIn = new BluePrintPin(this, 1, "", PinType.Main, PinStream.In);
            AddPin(pinIn);

            BluePrintPin pinCondition = new BluePrintValuedPin(this, 2, "Cond", PinType.Data, PinStream.In, VariantType.Var_Bool);
            AddPin(pinCondition);

            BluePrintPin pinLoop = new BluePrintPin(this, 3, "Loop", PinType.Main, PinStream.Out);
            AddPin(pinLoop);

            BluePrintPin pinOut = new BluePrintPin(this, 4, "Over", PinType.Main, PinStream.Out);
            AddPin(pinOut);
        }
    }
}
