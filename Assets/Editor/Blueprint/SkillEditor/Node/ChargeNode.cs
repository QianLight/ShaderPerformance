using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using EditorEcs;

namespace EditorNode
{

    public class ChargeNode:TimeTriggerNode<XChargeData>
    {
        public override float NextTime { get { return TriggerTime + HosterData.Duration; } }

        private TextAsset forwardCurve;
        private TextAsset sideCurve;
        private TextAsset upCurve;

        private float scriptDuration = 0;

        private BaseSkillNode getPositionNode = null;

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

            scriptDuration = HosterData.Duration;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.Interpolation = EditorGUITool.Toggle("Interpolation", HosterData.Interpolation);
            if (HosterData.Interpolation) HosterData.InterpolationVal = EditorGUITool.Slider("InterpolationVal", HosterData.InterpolationVal, 0.1f, 0.9f);

            HosterData.UsingCurve = EditorGUITool.Toggle("UsingCurve: ", HosterData.UsingCurve);

            /// Not Using Curve
            CheckAndDo(!HosterData.UsingCurve && !HosterData.Immediate,
                () => HosterData.Duration = TimeFrameField("Duration: ", HosterData.Duration),
                () => HosterData.Duration = 0);
            CheckAndDo(!HosterData.UsingCurve && !HosterData.Immediate,
                () => ParamTemplate("Velocity", HosterData, () => { HosterData.Velocity = EditorGUITool.FloatField("Velocity", HosterData.Velocity); }),
                () => { HosterData.Velocity = 0; HosterData.VelocityParamIndex = -1; });
            CheckAndDo(!HosterData.UsingCurve && !HosterData.Immediate,
                () => HosterData.Manually = EditorGUITool.Toggle("Manually: ", HosterData.Manually),
                () => HosterData.Manually = false);

            CheckAndDo(!HosterData.UsingCurve,
                () => HosterData.TargetPosType = EditorGUITool.Popup("TargetPosType", HosterData.TargetPosType, EditorGUITool.Translate<XChargeTargetPosType>()),
                () => HosterData.TargetPosType = 0);
            CheckAndDo(!HosterData.UsingCurve && HosterData.TargetPosType != 0,
                () => HosterData.Immediate = EditorGUITool.Toggle("Immediate", HosterData.Immediate),
                () => HosterData.Immediate = false);
            CheckAndDo(!HosterData.UsingCurve && HosterData.TargetPosType != 0,
                () => EditorGUITool.Vector3Field("Offset", ref HosterData.OffsetX, ref HosterData.OffsetY, ref HosterData.OffsetZ),
                () => { HosterData.OffsetX = 0; HosterData.OffsetY = 0; HosterData.OffsetZ = 0; });
            CheckAndDo(!HosterData.UsingCurve && HosterData.TargetPosType != 0,
                () => HosterData.Range = EditorGUITool.FloatField("RandomRange", HosterData.Range),
                () => HosterData.Range = 0);

            /// UsingCurve
            CheckAndDo(HosterData.UsingCurve && GUILayout.Button("Auto"),
                () =>
                {
                    forwardCurve = AssetDatabase.LoadAssetAtPath(CurvePath + "Curve/" + XEntityPresentationReader.GetData((uint)GetRoot.GetConfigData<XConfigData>().PresentID).CurveLocation + GetRoot.GetConfigData<XConfigData>().Name + "_forward.txt", typeof(TextAsset)) as TextAsset;
                    sideCurve = AssetDatabase.LoadAssetAtPath(CurvePath + "Curve/" + XEntityPresentationReader.GetData((uint)GetRoot.GetConfigData<XConfigData>().PresentID).CurveLocation + GetRoot.GetConfigData<XConfigData>().Name + "_side.txt", typeof(TextAsset)) as TextAsset;
                    upCurve = AssetDatabase.LoadAssetAtPath(CurvePath + "Curve/" + XEntityPresentationReader.GetData((uint)GetRoot.GetConfigData<XConfigData>().PresentID).CurveLocation + GetRoot.GetConfigData<XConfigData>().Name + "_up.txt", typeof(TextAsset)) as TextAsset;
                }, null);

            /// ForawrdCurve
            CheckAndDo(HosterData.UsingCurve,
                () => forwardCurve = EditorGUITool.ObjectField("Forward", forwardCurve, typeof(TextAsset), false) as TextAsset,
                () => forwardCurve = null);
            CheckAndDo(HosterData.UsingCurve && forwardCurve != null,
                () => HosterData.CurveForward = AssetDatabase.GetAssetPath(forwardCurve).Replace(CurvePath, ""),
                () => HosterData.CurveForward = "");
            CheckAndDo(HosterData.UsingCurve && forwardCurve != null,
                () => HosterData.Duration = GetRoot.GetCurveDuration(forwardCurve));
            CheckAndDo(HosterData.UsingCurve && forwardCurve != null,
                () => ParamTemplate("ForwardRatio", HosterData, () => { HosterData.ForwardRatio = EditorGUITool.FloatField("ForwardRatio", HosterData.ForwardRatio); }),
                () => { HosterData.ForwardRatio = 1; HosterData.ForwardRatioParamIndex = -1; });

            /// SideCurve
            CheckAndDo(HosterData.UsingCurve,
                () => sideCurve = EditorGUITool.ObjectField("Side", sideCurve, typeof(TextAsset), false) as TextAsset,
                () => sideCurve = null);
            CheckAndDo(HosterData.UsingCurve && sideCurve != null,
                () => HosterData.CurveSide = AssetDatabase.GetAssetPath(sideCurve).Replace(CurvePath, ""),
                () => HosterData.CurveSide = "");
            CheckAndDo(HosterData.UsingCurve && sideCurve != null,
                () => HosterData.Duration = GetRoot.GetCurveDuration(sideCurve));
            CheckAndDo(HosterData.UsingCurve && sideCurve != null,
                () => ParamTemplate("SideRatio", HosterData, () => { HosterData.SideRatio = EditorGUITool.FloatField("SideRatio", HosterData.SideRatio); }),
                () => { HosterData.SideRatio = 1; HosterData.ForwardRatioParamIndex = -1; });

            /// UpCurve
            CheckAndDo(HosterData.UsingCurve,
                () => HosterData.UsingUp = EditorGUITool.Toggle("UsingUp", HosterData.UsingUp),
                () => HosterData.UsingUp = false);
            CheckAndDo(HosterData.UsingCurve && HosterData.UsingUp,
                () => upCurve = EditorGUITool.ObjectField("Up", upCurve, typeof(TextAsset), false) as TextAsset,
                () => upCurve = null);
            CheckAndDo(HosterData.UsingCurve && HosterData.UsingUp && upCurve != null,
                () => HosterData.CurveUp = AssetDatabase.GetAssetPath(upCurve).Replace(CurvePath, ""),
                () => HosterData.CurveUp = "");
            CheckAndDo(HosterData.UsingCurve && HosterData.UsingUp && upCurve != null,
                () => HosterData.Duration = GetRoot.GetCurveDuration(upCurve));
            CheckAndDo(HosterData.UsingCurve && HosterData.UsingUp && upCurve != null,
                () => ParamTemplate("UpRatio", HosterData, () => { HosterData.UpRatio = EditorGUITool.FloatField("UpRatio", HosterData.UpRatio); }),
                () => { HosterData.UpRatio = 1; HosterData.UpRatioParamIndex = -1; });

            /// Ratio
            CheckAndDo(HosterData.UsingCurve && HosterData.RatioList.Count != 0,
                () => { TimeFrameField("Duration", GetCurveLength()); HosterData.RatioParamIndex = -1; });
            CheckAndDo(HosterData.UsingCurve && HosterData.RatioList.Count == 0,
                () =>
                {
                    HosterData.Duration = TimeFrameField("Duration: ", HosterData.Duration / HosterData.Ratio) * HosterData.Ratio;
                    DrawLine();
                    TimeFrameField("ScriptDuration", scriptDuration);
                    DrawLine();
                });
            CheckAndDo(HosterData.UsingCurve && HosterData.RatioList.Count == 0,
                () => ParamTemplate("Ratio", HosterData, () => { HosterData.Ratio = EditorGUITool.FloatField("Ratio", HosterData.Ratio); }),
                () => { HosterData.Ratio = 1; HosterData.RatioParamIndex = -1; });
            CheckAndDo(HosterData.UsingCurve && HosterData.RatioParamIndex == -1,
                () => DrawRatio());
            DrawLine();


            HosterData.IgnoreCollision = EditorGUITool.Toggle("Disable Target Collision", HosterData.IgnoreCollision);
            CheckAndDo(!HosterData.IgnoreCollision,
                () => HosterData.CollisionAngle = EditorGUITool.Slider("CollisionAngle (0~90): ", HosterData.CollisionAngle, 0, 90),
                () => HosterData.CollisionAngle = 60);

            HosterData.DynamicForward = EditorGUITool.Toggle("DynamicDistance", HosterData.DynamicForward);
            DrawLine();
            CheckAndDo(HosterData.DynamicForward,
                () => GetNodeByIndex<GetPositionNode>(ref getPositionNode, ref HosterData.GetPositionIndex, true, "GetPositionIndex"),
                () => HosterData.GetPositionIndex = -1);

        }

        public override T CopyData<T>(T data)
        {
            XChargeData copy = base.CopyData(data) as XChargeData;

            List<XChargeRatioData> tmp = new List<XChargeRatioData>();
            for (int i = 0; i < copy.RatioList.Count; ++i)
                tmp.Add(copy.RatioList[i].Clone() as XChargeRatioData);
            copy.RatioList = tmp;

            return copy as T;
        }

        private void DrawRatio()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUITool.LabelField("Ratio");
            if (HosterData.RatioList.Count < 8)
            {
                if (GUILayout.Button("+"))
                {
                    XChargeRatioData data = new XChargeRatioData();
                    data.RatioTime = HosterData.RatioList.Count == 0 ? 0 : HosterData.RatioList[HosterData.RatioList.Count - 1].RatioTime;
                    data.RatioValue = 1;
                    HosterData.RatioList.Add(data);

                    data = new XChargeRatioData();
                    data.RatioTime = GetCurveLength();
                    data.RatioValue = 1;
                    HosterData.RatioList.Add(data);
                }
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < HosterData.RatioList.Count; i += 2)
            {
                DrawLine();
                EditorGUILayout.BeginHorizontal();
                EditorGUITool.LabelField("Ratio_" + i.ToString());
                if (GUILayout.Button("Delete"))
                {
                    HosterData.RatioList.RemoveAt(i + 1);
                    HosterData.RatioList.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                float minValue = GetMinTime(i);
                float maxValue = GetMaxTime(i);
                float preValue = HosterData.RatioList[i].RatioTime;
                HosterData.RatioList[i].RatioTime = Mathf.Max(minValue,
                    Mathf.Min(maxValue, TimeFrameField("At", HosterData.RatioList[i].RatioTime)));
                //if (preValue != HosterData.RatioList[i].RatioTime)
                //{
                //    float delta = (HosterData.RatioList[i].RatioTime - preValue) - (HosterData.RatioList[i].RatioTime - preValue) * HosterData.RatioList[i].RatioValue;
                //    MoveRatio(delta, i + 1);
                //}

                minValue = GetMinTime(i + 1);
                maxValue = GetMaxTime(i + 1);
                preValue = HosterData.RatioList[i + 1].RatioTime;
                HosterData.RatioList[i + 1].RatioTime = Mathf.Max(minValue, Mathf.Min(maxValue, TimeFrameField("End", HosterData.RatioList[i + 1].RatioTime)));
                //if (preValue != HosterData.RatioList[i + 1].RatioTime)
                //{
                //    float delta = (HosterData.RatioList[i + 1].RatioTime - preValue) - (HosterData.RatioList[i + 1].RatioTime - preValue) * HosterData.RatioList[i].RatioValue;
                //    MoveRatio(delta, i + 2);
                //}

                preValue = HosterData.RatioList[i].RatioValue;
                HosterData.RatioList[i].RatioValue = Mathf.Min(3f, Mathf.Max(0.05f, EditorGUITool.FloatField(  "Value", HosterData.RatioList[i].RatioValue)));
                //if (preValue != HosterData.RatioList[i].RatioValue)
                //{
                //    float delta = (HosterData.RatioList[i + 1].RatioTime - HosterData.RatioList[i].RatioTime) * preValue / HosterData.RatioList[i].RatioValue - (HosterData.RatioList[i + 1].RatioTime - HosterData.RatioList[i].RatioTime);
                //    MoveRatio(delta, i + 1);
                //}
            }
        }

        private void MoveRatio(float deltaTime, int beginIndex)
        {
            for (int i = beginIndex; i < HosterData.RatioList.Count; ++i)
            {
                HosterData.RatioList[i].RatioTime += deltaTime;
            }
        }

        private float GetMinTime(int index)
        {
            return index != 0 ? HosterData.RatioList[index - 1].RatioTime : 0;
        }

        private float GetMaxTime(int index)
        {
            float nextTime = ((index + 1 == HosterData.RatioList.Count) ? GetCurveLength() : HosterData.RatioList[index + 1].RatioTime) - HosterData.RatioList[index].RatioTime;
            float curValue = (index == 0 ? 1 : HosterData.RatioList[index - 1].RatioValue);
            float nextValue = (index == HosterData.RatioList.Count) ? 1 : HosterData.RatioList[index].RatioValue;
            return Mathf.Min(GetMaxLength(), HosterData.RatioList[index].RatioTime + nextTime * nextValue / curValue);
        }

        private float GetMaxLength()
        {
            float left = HosterData.Duration;
            float cur = 0;
            float ratio = 1;
            for (int i = 0; i < HosterData.RatioList.Count; ++i)
            {
                float delta = Mathf.Max(HosterData.RatioList[i].RatioTime - cur, 0);
                if (left < delta * ratio)
                    delta = left / ratio;
                left -= delta * ratio;
                cur += delta;
                if (i != HosterData.RatioList.Count - 1) ratio = HosterData.RatioList[i].RatioValue;
            }
            return cur + left / ratio;
        }

        private float GetCurveLength()
        {
            float left = HosterData.Duration;
            float cur = 0;
            float ratio = 1;
            for (int i = 0; i < HosterData.RatioList.Count; ++i)
            {
                float delta = Mathf.Max(HosterData.RatioList[i].RatioTime - cur, 0);
                if (left < delta * ratio)
                    delta = left / ratio;
                left -= delta * ratio;
                cur += delta;
                ratio = HosterData.RatioList[i].RatioValue;
            }
            return cur + left / ratio;
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
            string presentStr = presentData.CurveLocation.Remove(presentData.CurveLocation.Length - 1, 1);

            if (HosterData.CurveForward != null)
            {
                HosterData.CurveForward = HosterData.CurveForward.Replace(templateStr, presentStr);

                forwardCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveForward, typeof(TextAsset)) as TextAsset;
                if (forwardCurve != null)
                {
                    float duration = GetRoot.GetCurveDuration(forwardCurve);
                    HosterData.Duration = duration;
                }
            }
            if (HosterData.CurveSide != null) HosterData.CurveSide = HosterData.CurveSide.Replace(templateStr, presentStr);
            if(HosterData.CurveUp!=null) HosterData.CurveUp = HosterData.CurveUp.Replace(templateStr, presentStr);
        }

        public override void BuildDataFinish()
        {
            base.BuildDataFinish();

            GetNodeByIndex<GetPositionNode>(ref getPositionNode, ref HosterData.GetPositionIndex);
        }
    }
}