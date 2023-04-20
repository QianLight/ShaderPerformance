using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class FreezeNode : TimeTriggerNode<XFreezeData>
    {

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header5";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.LifeTime = TimeFrameField("LifeTime: ", HosterData.LifeTime);

            HosterData.Fixed = EditorGUITool.Toggle("Fixed", HosterData.Fixed);

            HosterData.UseTimeScale = EditorGUITool.Toggle("UseTimeScale", HosterData.UseTimeScale);
            if (HosterData.UseTimeScale)
                HosterData.Scale = EditorGUITool.Slider("Scale", HosterData.Scale, 0.01f, 0.05f);
        }
    }
}