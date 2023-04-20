using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BluePrint
{
    class ControllerLoopEndNode : ControllerBaseNode
    {
        public override int GetContorllerType()
        {
            return (int)ControllerNodeType.ControllerNode_LoopEnd;
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            nodeEditorData.Tag = "LoopEnd";
            nodeEditorData.BackgroundText = "";

            BluePrintPin pinIn = new BluePrintPin(this, 1, "", PinType.Main, PinStream.In);
            AddPin(pinIn);

            BluePrintPin pinOut = new BluePrintPin(this, 2, "", PinType.Main, PinStream.Out);
            AddPin(pinOut);
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HostData.nodeID = EditorGUILayout.IntField("NodeID",HostData.nodeID);
        }
    }
}
