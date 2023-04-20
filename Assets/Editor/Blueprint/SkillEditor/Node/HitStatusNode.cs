using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class HitStatusNode:TimeTriggerNode<XHitStatusData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header8";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HosterData.Status = EditorGUITool.Popup("Status", HosterData.Status, EditorGUITool.Translate<XHitType>());
            if ((XHitType)HosterData.Status == XHitType.None)
            {
                HosterData.Status = (int)XHitType.Back;
            }
        }
    }
}