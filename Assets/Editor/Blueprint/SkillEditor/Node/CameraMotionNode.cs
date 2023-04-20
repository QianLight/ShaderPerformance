using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class CameraMotionNode : TimeTriggerNode<XCameraMotionData>
    {
        private AnimationClip motion;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header9";

            if (GetRoot.NeedInitRes)
            {
                motion = AssetDatabase.LoadAssetAtPath(ResourecePath + HosterData.MotionPath + ".anim", typeof(AnimationClip)) as AnimationClip;
            }
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            motion = EditorGUITool.ObjectField("Motion Clip", motion, typeof(AnimationClip), false) as AnimationClip;
            if (motion != null)
            {
                HosterData.MotionPath = AssetDatabase.GetAssetPath(motion).Replace(ResourecePath, "");
                HosterData.MotionPath = HosterData.MotionPath.Remove(HosterData.MotionPath.LastIndexOf('.'));
                TimeFrameField("����ʱ��(s): ", HosterData.At + motion.length);
            }
            else HosterData.MotionPath = "";

            HosterData.AnchorBased = true;// Toggle("AnchorBased: ", HosterData.AnchorBased);
            HosterData.FollowHoster = EditorGUITool.Toggle("FollowHoster", HosterData.FollowHoster);

            HosterData.EnterBlendTime = TimeFrameField("EnterBlendTime:", HosterData.EnterBlendTime);

            HosterData.BlendTime = TimeFrameField("EndBlendTime:", HosterData.BlendTime);

            HosterData.UseCameraPosAtEnd = EditorGUITool.Toggle("UseCameraPosAtEnd:", HosterData.UseCameraPosAtEnd);

            HosterData.ForceBlendTime = TimeFrameField("ForceEndBlendTime:", HosterData.ForceBlendTime);

            HosterData.FarClipPlane = EditorGUITool.FloatField("FarClipPlane", HosterData.FarClipPlane);

            HosterData.UseAssistCameraAnim = EditorGUITool.Toggle("UseAssistCameraAnim:", HosterData.UseAssistCameraAnim);

            HosterData.PlayerTrigger = EditorGUITool.Toggle("PlayerTrigger", HosterData.PlayerTrigger);

            HosterData.CanSkipWhenCastAoYi = EditorGUITool.Toggle("CanSkipWhenCastAoYi", HosterData.CanSkipWhenCastAoYi);
        }

        public override void ScanPolicy(CFEngine.OrderResList result, CFEngine.ResItem item)
        {
            if (!string.IsNullOrEmpty(HosterData.MotionPath))
                result.Add(item, System.IO.Path.GetFileName(HosterData.MotionPath) + ".anim", ResourecePath + HosterData.MotionPath + ".anim");
        }
    }
}