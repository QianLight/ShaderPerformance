using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class GetDirectionNode : TimeTriggerNode<XGetDirectionData>
    {

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header5";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.Target = EditorGUITool.Toggle("Target", HosterData.Target);
        }
    }
}