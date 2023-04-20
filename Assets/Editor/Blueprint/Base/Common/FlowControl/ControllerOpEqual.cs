using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class ControllerOpEqual : ControllerBaseNode
    {
        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_Equal;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.TitleName = "";
            nodeEditorData.BackgroundText = "==";

            BluePrintValuedPin pinIn = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Float);
            pinIn.SetDefaultValue(0.0f);
            AddPin(pinIn);

            BluePrintValuedPin pinIn2 = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.In, VariantType.Var_Float);
            pinIn2.SetDefaultValue(0.0f);
            AddPin(pinIn2);

            BluePrintPin pinOut = new BluePrintValuedPin(this, 3, "", PinType.Data, PinStream.Out, VariantType.Var_Bool);
            AddPin(pinOut);
        }

    }
}
