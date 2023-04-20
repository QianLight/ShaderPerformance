using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{

    public class KnockedBackNode:TimeTriggerNode<XKnockedBackData>
    {
        private TextAsset forwardCurve;
        private TextAsset sideCurve;
        private TextAsset upCurve;

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

            if (GetRoot.NeedInitRes)
            {
                forwardCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveForward, typeof(TextAsset)) as TextAsset;
                sideCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveSide, typeof(TextAsset)) as TextAsset;
                upCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveUp, typeof(TextAsset)) as TextAsset;
            }
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.Interpolation = EditorGUITool.Toggle("Interpolation", HosterData.Interpolation);
            if (HosterData.Interpolation) HosterData.InterpolationVal = EditorGUITool.Slider("InterpolationVal", HosterData.InterpolationVal, 0.1f, 0.9f);

            HosterData.MoveToHitter = EditorGUITool.Toggle("MoveToHitter", HosterData.MoveToHitter);
            if (HosterData.MoveToHitter)
            {
                HosterData.UsingCurve = false;
                HosterData.Duration = TimeFrameField("Duration", HosterData.Duration);
                HosterData.RandomRange = EditorGUITool.FloatField("RandomRange", HosterData.RandomRange);
                HosterData.Velocity = EditorGUITool.FloatField("Velocity", HosterData.Velocity);
            }
            else
            {
                HosterData.UsingCurve = true;
                HosterData.Duration = 0;

                forwardCurve = EditorGUITool.ObjectField("Forward", forwardCurve, typeof(TextAsset), false) as TextAsset;
                if (forwardCurve != null)
                {
                    HosterData.CurveForward = AssetDatabase.GetAssetPath(forwardCurve).Replace(CurvePath, "");
                    HosterData.Duration = GetRoot.GetCurveDuration(forwardCurve);
                    ParamTemplate("ForwardRatio", HosterData,
                    () => { HosterData.ForwardRatio = EditorGUITool.FloatField("ForwardRatio", HosterData.ForwardRatio); });
                }
                else HosterData.CurveForward = "";

                sideCurve = EditorGUITool.ObjectField("Side", sideCurve, typeof(TextAsset), false) as TextAsset;
                if (sideCurve != null)
                {
                    HosterData.CurveSide = AssetDatabase.GetAssetPath(sideCurve).Replace(CurvePath, "");
                    HosterData.Duration = GetRoot.GetCurveDuration(sideCurve);
                    ParamTemplate("SideRatio", HosterData,
                    () => { HosterData.SideRatio = EditorGUITool.FloatField("SideRatio", HosterData.SideRatio); });
                }
                else HosterData.CurveSide = "";

                HosterData.UsingUp = EditorGUITool.Toggle("UsingUp", HosterData.UsingUp);
                if (HosterData.UsingUp)
                {
                    upCurve = EditorGUITool.ObjectField("Up", upCurve, typeof(TextAsset), false) as TextAsset;
                    if (upCurve != null)
                    {
                        HosterData.CurveUp = AssetDatabase.GetAssetPath(upCurve).Replace(CurvePath, "");
                        HosterData.Duration = GetRoot.GetCurveDuration(upCurve);
                        ParamTemplate("UpRatio", HosterData,
                        () => { HosterData.UpRatio = EditorGUITool.FloatField("UpRatio", HosterData.SideRatio); });
                    }
                    else HosterData.CurveUp = "";
                }

                HosterData.Duration = TimeFrameField("Duration: ", HosterData.Duration / HosterData.Ratio) * HosterData.Ratio;
                ParamTemplate("Ratio", HosterData,
                () => { HosterData.Ratio = EditorGUITool.FloatField("Ratio", HosterData.Ratio); });
                HosterData.IgnoreAttackCode = EditorGUITool.Toggle("IgnoreAttackCode", HosterData.IgnoreAttackCode);

            }
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            if (HosterData.UsingCurve)
            {
                forwardCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveForward, typeof(TextAsset)) as TextAsset;
                sideCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveSide, typeof(TextAsset)) as TextAsset;
                upCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveUp, typeof(TextAsset)) as TextAsset;

                if (forwardCurve == null)
                {
                    hasError = true;
                    LogError("Node_" + HosterData.Index + "，forwardCurve 缺失！！！");
                    return false;
                }

                float duration = GetRoot.GetCurveDuration(forwardCurve);
                HosterData.Duration = duration;
                if (sideCurve != null && duration != GetRoot.GetCurveDuration(sideCurve))
                {
                    hasError = true;
                    LogError("Node_" + HosterData.Index + "，sideCurve 时长不匹配！！！");
                    return false;
                }
                if (HosterData.UsingUp && upCurve != null && duration != GetRoot.GetCurveDuration(upCurve))
                {
                    hasError = true;
                    LogError("Node_" + HosterData.Index + "，upCurve 时长不匹配！！！");
                    return false;
                }
            }

            return true;
        }

        public override void CopyDataFromTemplate(int templateID, int presentID)
        {
            CFUtilPoolLib.XEntityPresentation.RowData templateData = XEntityPresentationReader.GetData((uint)templateID);
            CFUtilPoolLib.XEntityPresentation.RowData presentData = XEntityPresentationReader.GetData((uint)presentID);

            string templateStr = templateData.CurveLocation.Remove(templateData.CurveLocation.Length - 1, 1);
            //string presentStr = presentData.CurveLocation.Remove(presentData.CurveLocation.Length - 1, 1);
            string presentStr = string.IsNullOrEmpty(GetRoot.GetConfigData<XConfigData>().AnimTemplate) ?
                presentData.CurveLocation.Remove(presentData.CurveLocation.Length - 1, 1) : GetRoot.GetConfigData<XConfigData>().AnimTemplate;
            string presentCurveLocation = presentData.CurveLocation.Remove(presentData.CurveLocation.Length - 1, 1);

            if (!string.IsNullOrEmpty(HosterData.CurveForward))
            {
                //HosterData.CurveForward = HosterData.CurveForward.Replace(templateStr, presentStr);
                var splitStr = HosterData.CurveForward.Split('/');
                HosterData.CurveForward = splitStr[0] + "/" + splitStr[1].Replace(templateStr, presentCurveLocation) 
                                                      + "/" + splitStr[2].Replace(templateStr, presentStr);
                forwardCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveForward, typeof(TextAsset)) as TextAsset;
                if (forwardCurve != null)
                {
                    float duration = GetRoot.GetCurveDuration(forwardCurve);
                    HosterData.Duration = duration;
                }
            }
            if (!string.IsNullOrEmpty(HosterData.CurveSide)) 
            {
                //HosterData.CurveSide = HosterData.CurveSide.Replace(templateStr, presentStr);
                var splitStr = HosterData.CurveSide.Split('/');
                HosterData.CurveSide = splitStr[0] + "/" + splitStr[1].Replace(templateStr, presentCurveLocation) 
                                                      + "/" + splitStr[2].Replace(templateStr, presentStr);
            }
            if (!string.IsNullOrEmpty(HosterData.CurveUp)) 
            {
                //HosterData.CurveUp = HosterData.CurveUp.Replace(templateStr, presentStr);
                var splitStr = HosterData.CurveUp.Split('/');
                HosterData.CurveUp = splitStr[0] + "/" + splitStr[1].Replace(templateStr, presentCurveLocation) 
                                                      + "/" + splitStr[2].Replace(templateStr, presentStr);
            }
            
        }
    }
}