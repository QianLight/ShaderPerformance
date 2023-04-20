using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using CFUtilPoolLib;

namespace EditorNode
{
    public class CameraLayerMaskNode : TimeTriggerNode<XCameraLayerMaskData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header9";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.LifeTime = TimeFrameField("LifeTime", HosterData.LifeTime);
            HosterData.Mask = EditorGUITool.MaskField("Mask", HosterData.Mask, GameObjectLayerHelper.hideLayerStr);
            HosterData.ChangeSelf2Player2 = EditorGUITool.Toggle("ChangeSelf2Player2", HosterData.ChangeSelf2Player2);

            HosterData.CanSkipWhenCastAoYi = EditorGUITool.Toggle("CanSkipWhenCastAoYi", HosterData.CanSkipWhenCastAoYi);
        }
    }
}