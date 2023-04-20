using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using System.IO;

namespace EditorNode
{
    public class MessageNode : TimeTriggerNode<XMessageData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header2";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.Type = EditorGUITool.Popup("Type: ", HosterData.Type, EditorGUITool.Translate<EditorEcs.XMessageType>());

            HosterData.Message = EditorGUITool.TextField("Message", HosterData.Message);
        }
    }
}