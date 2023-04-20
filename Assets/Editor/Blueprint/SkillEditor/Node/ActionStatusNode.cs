using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class ActionStatusNode : TimeTriggerNode<XActionStatusData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header8";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.CanMove = EditorGUITool.Toggle("CanMove: ", HosterData.CanMove);
            HosterData.CanRotate = EditorGUITool.Toggle("CanRotate: ", HosterData.CanRotate);
            CheckAndDo(HosterData.CanRotate,
                () => HosterData.TightStop = EditorGUITool.Toggle("TightStop: ", HosterData.TightStop),
                () => HosterData.TightStop = false);
            HosterData.Scale = EditorGUITool.Slider("Scale", HosterData.Scale, 0, 3);
        }
    }
}