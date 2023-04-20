using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{

    public class RotateNode:TimeTriggerNode<XRotateData>
    {
        private TextAsset rotateCurve;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);
            HeaderImage = "BluePrint/Header7";

            if (AutoDefaultMainPin)
            {
                BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
                BluePrintPin pinOut = new BluePrintPin(this, -2, "End", PinType.Main, PinStream.Out);
                AddPin(pinIn);
                AddPin(pinOut);
            }

            rotateCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveRotate, typeof(TextAsset)) as TextAsset;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.LifeTime = 0;
            rotateCurve = EditorGUITool.ObjectField("Rotate", rotateCurve, typeof(TextAsset), false) as TextAsset;
            if (rotateCurve != null)
            {
                HosterData.CurveRotate = AssetDatabase.GetAssetPath(rotateCurve).Replace(CurvePath, "");
                string[] curve = rotateCurve.text.Replace("\r\n", "\t").Replace("\n", "\t").Split('\t');
                if (curve.Length > 3) HosterData.LifeTime = float.Parse(curve[curve.Length - 3]);
            }
            else HosterData.CurveRotate = "";

            HosterData.LifeTime = TimeFrameField("LifeTime: ", HosterData.LifeTime / HosterData.Ratio) * HosterData.Ratio;
            HosterData.Ratio = EditorGUITool.FloatField(  "Ratio", HosterData.Ratio);

            HosterData.DynamicAt = TimeFrameField("DynamicAt", HosterData.DynamicAt);
            HosterData.DynamicEnd = TimeFrameField("DynamicEnd", HosterData.DynamicEnd);
        }
    }
}