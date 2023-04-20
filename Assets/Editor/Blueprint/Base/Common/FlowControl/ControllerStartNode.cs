using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class ControllerStartNode : ControllerBaseNode
    {
        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_Start;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.Tag = "";
            nodeEditorData.BackgroundText = "Go";

            BluePrintPin StartPin = new BluePrintPin(this, 1, "", PinType.Main, PinStream.Out);
            AddPin(StartPin);
        }
    }

    class ControllerFlyNode : ControllerBaseNode
    {
        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_Fly;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.Tag = "";
            nodeEditorData.BackgroundText = "Fly";

            BluePrintPin StartPin = new BluePrintPin(this, 1, "", PinType.Main, PinStream.Out);
            AddPin(StartPin);
        }
    }


}
