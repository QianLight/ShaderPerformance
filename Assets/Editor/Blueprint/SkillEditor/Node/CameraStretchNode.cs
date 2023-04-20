using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class CameraStretchNode : TimeTriggerNode<XCameraStretchData>
    {
        enum CameraStretchType {
            cameraStretch = 0,
            cameraDamping = 1,
            cameraRot = 2,
        }
        int lenTargetAngles;
        bool expandTargetAngles;
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header9";
            lenTargetAngles = HosterData.targetAngles != null ? HosterData.targetAngles.Length : 0;
            expandTargetAngles = false;
        }
        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.Type = (int)(CameraStretchType)EditorGUILayout.EnumPopup("Type", (CameraStretchType)HosterData.Type);
            if (HosterData.Type == 0)
            {
                if (HosterData.LifeTime != 0)//�ɰ汾�Ķ�д
                {
                    HosterData.FOVFadeinTime = HosterData.LifeTime;
                    HosterData.FOVFadeOutTime = -1f;
                    HosterData.LifeTime = 0;
                    HosterData.EndBlendTime = 0.5f;//������0.5s��������
                }

                HosterData.UsingFov = EditorGUITool.Toggle("UsingFov", HosterData.UsingFov);
                if (HosterData.UsingFov)
                {
                    HosterData.Fov = EditorGUITool.FloatField("Fov", HosterData.Fov);
                    HosterData.FOVFadeinTime = TimeFrameField("FOVFadeinTime", HosterData.FOVFadeinTime);
                    HosterData.UseFovFadeInCurve = EditorGUITool.Toggle("UseFovFadeInCurve", HosterData.UseFovFadeInCurve);
                    if (HosterData.UseFovFadeInCurve)
                    {
                        if (HosterData.FovFadeInCurve == null)
                        {
                            HosterData.FovFadeInCurve = new AnimationCurve();
                            if (HosterData.FovFadeInCurve.length < 2)
                            {
                                HosterData.FovFadeInCurve.AddKey(new Keyframe(0, 0));
                                HosterData.FovFadeInCurve.AddKey(new Keyframe(1, 1));
                            }
                        }
                        HosterData.FovFadeInCurve = EditorGUILayout.CurveField("FovFadeInCurve", HosterData.FovFadeInCurve);
                    }
                    HosterData.FOVLastTime = TimeFrameField("FOVLastTime", HosterData.FOVLastTime);
                    HosterData.FOVFadeOutTime = TimeFrameField("FOVFadeOutTime", HosterData.FOVFadeOutTime);
                    HosterData.UseFovFadeOutCurve = EditorGUITool.Toggle("UseFovFadeOutCurve", HosterData.UseFovFadeOutCurve);

                    if (HosterData.UseFovFadeOutCurve)
                    {
                        if (HosterData.FovFadeOutCurve == null)
                        {
                            HosterData.FovFadeOutCurve = new AnimationCurve();
                            if (HosterData.FovFadeOutCurve.length < 2)
                            {
                                HosterData.FovFadeOutCurve.AddKey(new Keyframe(0, 0));
                                HosterData.FovFadeOutCurve.AddKey(new Keyframe(1, 1));
                            }
                        }
                        HosterData.FovFadeOutCurve = EditorGUILayout.CurveField("FovFadeOutCurve", HosterData.FovFadeOutCurve);
                    }
                    HosterData.EndBlendTime = TimeFrameField("EndBlendTime_InSKill", HosterData.EndBlendTime);
                    HosterData.isMotion = EditorGUITool.Toggle("IsMotion", HosterData.isMotion);
                }
            }
            else if (HosterData.Type == 1)
            {
                if (HosterData.DampingCurve == null) HosterData.DampingCurve = new AnimationCurve();
                if (HosterData.DampingCurve.length < 2)
                {
                    HosterData.DampingCurve.AddKey(new Keyframe(0, 0));
                    HosterData.DampingCurve.AddKey(new Keyframe(1, 1));
                }
                HosterData.DampingCurve = EditorGUILayout.CurveField("DampingCurve", HosterData.DampingCurve);

                EditorGUILayout.LabelField("StartDamping", (HosterData.DampingCurve.keys[0].value).ToString());
                EditorGUILayout.LabelField("EndDamping", (HosterData.DampingCurve.keys[HosterData.DampingCurve.length - 1].value).ToString());
                HosterData.DampingLastTime = EditorGUITool.FloatField("LastTime", HosterData.DampingLastTime);
            }
            else if (HosterData.Type == 2) 
            {
                lenTargetAngles = EditorGUITool.FloatArrayField("TargetAngles", lenTargetAngles, ref HosterData.targetAngles, ref expandTargetAngles);
                HosterData.rotTime = EditorGUITool.FloatField("RotTime", HosterData.rotTime);
                HosterData.rotAcceleration = EditorGUITool.Slider("RotAcceleration��(0,1]", HosterData.rotAcceleration, 0.01f, 1f);
            }
            HosterData.CanSkipWhenCastAoYi = EditorGUITool.Toggle("CanSkipWhenCastAoYi", HosterData.CanSkipWhenCastAoYi);
        }
    }
}