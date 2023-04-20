using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class ControllerBranchNode : ControllerBaseNode
    {
        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_Branch;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.Tag = "Branch";
            nodeEditorData.BackgroundText = "";

            BluePrintPin pinIn = new BluePrintPin(this, 1, "", PinType.Main, PinStream.In);
            AddPin(pinIn);

            BluePrintPin pinCondition = new BluePrintValuedPin(this, 2, "Cond", PinType.Data, PinStream.In, VariantType.Var_Bool);
            AddPin(pinCondition);

            BluePrintPin pinTrue = new BluePrintPin(this, 3, "True", PinType.Main, PinStream.Out);
            AddPin(pinTrue);

            BluePrintPin pinFalse = new BluePrintPin(this, 4, "False", PinType.Main, PinStream.Out);
            AddPin(pinFalse);
        }
    }
}
