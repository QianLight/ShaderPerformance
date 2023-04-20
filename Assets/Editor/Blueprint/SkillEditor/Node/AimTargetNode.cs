using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class AimTargetNode : TimeTriggerNode<XAimTargetData>
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

            HosterData.MaxAimAngle = EditorGUITool.FloatField(  "MaxAimAngle", HosterData.MaxAimAngle);
            HosterData.AimSpeed = EditorGUITool.FloatField(  "AimSpeed", HosterData.AimSpeed);
        }
    }
}