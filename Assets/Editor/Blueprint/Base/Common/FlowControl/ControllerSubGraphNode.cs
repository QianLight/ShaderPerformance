using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class ControllerSubGraphStartNode : ControllerBaseNode
    {
        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_SubStart;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.Tag = "";
            nodeEditorData.BackgroundText = "GraphGo";
            CanbeFold = true;
            BluePrintPin StartPin = new BluePrintPin(this, 1, "", PinType.Main, PinStream.Out);
            AddPin(StartPin);

            BluePrintPin pinOut1 = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinOut1);
            BluePrintPin pinOut2 = new BluePrintValuedPin(this, 3, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinOut2);
            BluePrintPin pinOut3 = new BluePrintValuedPin(this, 4, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinOut3);
            BluePrintPin pinOut4 = new BluePrintValuedPin(this, 5, "", PinType.Data, PinStream.Out, VariantType.Var_Custom);
            AddPin(pinOut4);
        }
    }

    class ControllerSubGraphEndNode : ControllerBaseNode
    {
        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_SubEnd;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.Tag = "";
            nodeEditorData.BackgroundText = "GraphEnd";
            CanbeFold = true;
            BluePrintPin StartPin = new BluePrintPin(this, 1, "", PinType.Main, PinStream.In);
            AddPin(StartPin);

            BluePrintPin pinIn1 = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn1);
            BluePrintPin pinIn2 = new BluePrintValuedPin(this, 3, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn2);
            BluePrintPin pinIn3 = new BluePrintValuedPin(this, 4, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn3);
            BluePrintPin pinIn4 = new BluePrintValuedPin(this, 5, "", PinType.Data, PinStream.In, VariantType.Var_Custom);
            AddPin(pinIn4);
        }
    }
}