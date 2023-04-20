using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class ControllerOpAnd : ControllerBaseNode
    {
        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_And;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.TitleName = "";
            nodeEditorData.BackgroundText = "&&";

            BluePrintPin pinIn = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Bool);
            AddPin(pinIn);

            BluePrintPin pinIn2 = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.In, VariantType.Var_Bool);
            AddPin(pinIn2);

            BluePrintPin pinOut = new BluePrintValuedPin(this, 3, "", PinType.Data, PinStream.Out, VariantType.Var_Bool);
            AddPin(pinOut);
        }

    }
}
